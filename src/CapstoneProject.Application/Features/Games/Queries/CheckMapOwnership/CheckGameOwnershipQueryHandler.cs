using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Queries.CheckMapOwnership;

public class CheckMapOwnershipQueryHandler : IRequestHandler<CheckMapOwnershipQuery, Result<CheckMapOwnershipDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CheckMapOwnershipQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CheckMapOwnershipDto>> Handle(CheckMapOwnershipQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<CheckMapOwnershipDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để kiểm tra quyền sở hữu bản đồ.", ErrorCodeEnum.Unauthorized);

        var mapRepo = _unitOfWork.Repository<Game>();
        var game = await mapRepo.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.GameId && m.Status == EntityStatusEnum.Active, cancellationToken);

        var dto = new CheckMapOwnershipDto { MapExists = game != null };
        if (game == null)
            return Result<CheckMapOwnershipDto>.Success(dto, "Đã kiểm tra quyền sở hữu bản đồ.");

        var rootGameId = game.RootGameId ?? game.Id;
        var lineGameIds = await mapRepo.GetQueryable()
            .Where(m => m.Status == EntityStatusEnum.Active && (m.RootGameId ?? m.Id) == rootGameId)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var isAuthor = game.CreatedBy.HasValue && game.CreatedBy.Value == userId.Value;
        var purchased = false;
        DateTime? purchasedAt = null;
        var inMyGame = false;
        if (!isAuthor)
        {
            var paymentRepo = _unitOfWork.Repository<PaymentRecord>();
            purchasedAt = await paymentRepo.GetQueryable()
                .Where(p => !p.IsDeleted
                            && p.UserId == userId.Value
                            && p.GameId.HasValue
                            && lineGameIds.Contains(p.GameId.Value)
                            && (p.PaymentStatus == PaymentStatusEnum.Pending || p.PaymentStatus == PaymentStatusEnum.Completed))
                .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
                .Select(p => (DateTime?)(p.PaidAt ?? p.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);
            purchased = purchasedAt.HasValue;
            if (!purchased)
            {
                var myMapRepo = _unitOfWork.Repository<MyGame>();
                inMyGame = await myMapRepo.GetQueryable()
                    .AnyAsync(mm => !mm.IsDeleted
                                    && mm.UserId == userId.Value
                                    && lineGameIds.Contains(mm.GameId),
                        cancellationToken);
            }
        }

        dto.IsOwned = isAuthor || purchased || inMyGame;
        dto.IsAuthor = isAuthor;
        dto.IsPurchased = purchased;
        dto.PurchasedAt = purchasedAt;
        return Result<CheckMapOwnershipDto>.Success(dto, "Đã kiểm tra quyền sở hữu bản đồ.");
    }
}
