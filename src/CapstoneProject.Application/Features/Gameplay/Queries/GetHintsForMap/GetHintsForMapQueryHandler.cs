using MediatR;
using CapstoneProject.Application.Common.Enums;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Security;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetHintsForMap;

public class GetHintsForMapQueryHandler : IRequestHandler<GetHintsForMapQuery, Result<List<HintLevelDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEntitlementService _entitlementService;

    public GetHintsForMapQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEntitlementService entitlementService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _entitlementService = entitlementService;
    }

    public async Task<Result<List<HintLevelDto>>> Handle(GetHintsForMapQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<List<HintLevelDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var userId = userIdNullable.Value;
        var quotaValue = await _entitlementService.GetNumericFeatureAsync(
            userId,
            FeatureKeys.MonthlyHintQuota,
            cancellationToken);
        var monthlyQuota = quotaValue.HasValue ? Math.Max(0, (int)Math.Floor(quotaValue.Value)) : 0;
        if (monthlyQuota <= 0)
        {
            return Result<List<HintLevelDto>>.Failure(
                "Gói hiện tại không bao gồm quota gợi ý tháng.",
                ErrorCodeEnum.Forbidden);
        }

        var now = VietnamDateTime.DbNow;
        var monthKey = now.Year * 100 + now.Month;
        var usageRepo = _unitOfWork.Repository<UserMonthlyHintUsage>();
        var usage = await usageRepo.GetQueryable()
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.MonthKey == monthKey && !x.IsDeleted,
                cancellationToken);
        var usedCount = usage?.UsedCount ?? 0;
        if (usedCount >= monthlyQuota)
        {
            return Result<List<HintLevelDto>>.Failure(
                "Bạn đã dùng hết quota gợi ý trong tháng. Vui lòng nâng gói hoặc chờ tháng mới.",
                ErrorCodeEnum.Forbidden);
        }

        var q = _unitOfWork.Repository<Hint>().GetQueryable()
            .AsNoTracking()
            .Where(h => !h.IsDeleted && h.GameDetail.GameId == request.GameId && !h.GameDetail.IsDeleted);

        if (request.GameDetailId.HasValue)
            q = q.Where(h => h.GameDetailId == request.GameDetailId.Value);

        var hints = await q
            .OrderBy(h => h.GameDetail.LevelOrder)
            .ThenBy(h => h.OrderNo)
            .Select(h => new HintLevelDto
            {
                LevelOrder = h.GameDetail.LevelOrder,
                GameDetailId = h.GameDetailId,
                OrderNo = h.OrderNo,
                Content = h.Content
            })
            .ToListAsync(cancellationToken);

        if (usage == null)
        {
            usage = new UserMonthlyHintUsage
            {
                UserId = userId,
                MonthKey = monthKey,
                UsedCount = 1
            };
            usage.InitializeEntity(userId);
            await usageRepo.AddAsync(usage);
        }
        else
        {
            usage.UsedCount += 1;
            usage.UpdatedAt = VietnamDateTime.DbNow;
            usage.UpdatedBy = userId;
            usageRepo.Update(usage);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var remaining = Math.Max(0, monthlyQuota - (usage?.UsedCount ?? usedCount));
        return Result<List<HintLevelDto>>.Success(hints, $"Đã lấy gợi ý cho bản đồ. Còn {remaining}/{monthlyQuota} lượt trong tháng.");
    }
}

