using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services;

public class XpEngineService : IXpEngineService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IXpPolicy> _policies;

    public XpEngineService(IUnitOfWork unitOfWork, IEnumerable<IXpPolicy> policies)
    {
        _unitOfWork = unitOfWork;
        _policies = policies;
    }

    public async Task<Result<XpGrantResult>> GrantXpAsync(XpGrantInput input, CancellationToken cancellationToken = default)
    {
        if (input.UserId == Guid.Empty)
            return Result<XpGrantResult>.Failure("Id người dùng là bắt buộc.", ErrorCodeEnum.ValidationFailed);
        if (string.IsNullOrWhiteSpace(input.IdempotencyKey))
            return Result<XpGrantResult>.Failure("IdempotencyKey là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var txRepo = _unitOfWork.Repository<XpTransaction>();
        var existedTx = await txRepo.GetQueryable()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == input.IdempotencyKey && !x.IsDeleted, cancellationToken);

        var userRepo = _unitOfWork.Repository<AppUser>();
        var user = await userRepo.GetQueryable().FirstOrDefaultAsync(x => x.Id == input.UserId, cancellationToken);
        if (user == null)
            return Result<XpGrantResult>.Failure($"Không tìm thấy người dùng có Id: {input.UserId}.", ErrorCodeEnum.NotFound);

        if (existedTx != null)
        {
            return Result<XpGrantResult>.Success(new XpGrantResult
            {
                IsDuplicate = true,
                GrantedXp = 0,
                NewTotalXp = user.CurrentXp,
                PreviousLevel = user.CurrentLevel,
                NewLevel = user.CurrentLevel,
                TransactionId = existedTx.Id
            }, "Phần thưởng XP đã được xử lý.");
        }

        var sourceConfig = await _unitOfWork.Repository<XpSourceConfig>().GetQueryable()
            .FirstOrDefaultAsync(x => x.SourceType == input.SourceType && !x.IsDeleted, cancellationToken);
        if (sourceConfig is { IsEnabled: false })
            return Result<XpGrantResult>.Success(new XpGrantResult
            {
                IsDuplicate = false,
                GrantedXp = 0,
                NewTotalXp = user.CurrentXp,
                PreviousLevel = user.CurrentLevel,
                NewLevel = user.CurrentLevel
            }, "Nguồn XP bị vô hiệu hóa.");

        var policyConfigs = await _unitOfWork.Repository<XpPolicyConfig>().GetQueryable()
            .Where(x => !x.IsDeleted && x.IsEnabled && (x.ActiveFrom == null || x.ActiveFrom <= VietnamDateTime.DbNow) &&
                        (x.ActiveTo == null || x.ActiveTo >= VietnamDateTime.DbNow))
            .OrderBy(x => x.Priority)
            .ToListAsync(cancellationToken);

        var policyContext = new XpPolicyContext
        {
            UserId = input.UserId,
            RequestedXp = input.RequestedXp,
            CurrentXpBeforeGrant = user.CurrentXp,
            SourceType = input.SourceType,
            SourceId = input.SourceId,
            CurrentTime = VietnamDateTime.DbNow,
            SourceConfig = sourceConfig,
            PolicyConfigs = policyConfigs.ToDictionary(x => x.PolicyKey, x => x)
        };

        var xpValue = input.RequestedXp;
        foreach (var policy in _policies.OrderBy(p => policyContext.PolicyConfigs.TryGetValue(p.PolicyKey, out var cfg) ? cfg.Priority : int.MaxValue))
        {
            if (policyContext.PolicyConfigs.ContainsKey(policy.PolicyKey))
                xpValue = await policy.ApplyAsync(policyContext, xpValue, cancellationToken);
        }

        if (xpValue <= 0)
        {
            return Result<XpGrantResult>.Success(new XpGrantResult
            {
                IsDuplicate = false,
                GrantedXp = 0,
                NewTotalXp = user.CurrentXp,
                PreviousLevel = user.CurrentLevel,
                NewLevel = user.CurrentLevel
            }, "Không có XP được cấp sau khi đánh giá chính sách.");
        }

        var previousLevel = user.CurrentLevel;
        user.CurrentXp += xpValue;

        var levels = await _unitOfWork.Repository<LevelThreshold>().GetQueryable()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.RequiredTotalXp)
            .ToListAsync(cancellationToken);

        var newLevel = levels
            .Where(x => x.RequiredTotalXp <= user.CurrentXp)
            .Select(x => x.Level)
            .DefaultIfEmpty(1)
            .Max();
        user.CurrentLevel = newLevel;

        var tx = new XpTransaction
        {
            UserId = input.UserId,
            GameId = input.SourceType == Domain.Enums.XpSourceTypeEnum.MapSolve ? input.SourceId : null,
            SourceId = input.SourceId,
            SourceType = input.SourceType,
            IdempotencyKey = input.IdempotencyKey.Trim(),
            Metadata = input.Metadata,
            Delta = xpValue,
            Reason = input.Reason
        };
        tx.InitializeEntity(input.UserId);
        await txRepo.AddAsync(tx);

        userRepo.Update(user);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Handle race-condition on unique idempotency key as duplicate-safe.
            return Result<XpGrantResult>.Success(new XpGrantResult
            {
                IsDuplicate = true,
                GrantedXp = 0,
                NewTotalXp = user.CurrentXp,
                PreviousLevel = previousLevel,
                NewLevel = user.CurrentLevel
            }, "Phần thưởng XP đã được xử lý.");
        }

        return Result<XpGrantResult>.Success(new XpGrantResult
        {
            IsDuplicate = false,
            GrantedXp = xpValue,
            NewTotalXp = user.CurrentXp,
            PreviousLevel = previousLevel,
            NewLevel = user.CurrentLevel,
            TransactionId = tx.Id
        }, "XP được cấp thành công.");
    }
}

