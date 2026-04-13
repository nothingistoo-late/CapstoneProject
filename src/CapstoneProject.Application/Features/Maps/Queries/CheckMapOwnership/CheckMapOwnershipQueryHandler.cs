using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Queries.CheckMapOwnership;

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

        var mapRepo = _unitOfWork.Repository<Map>();
        var map = await mapRepo.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MapId && m.Status == EntityStatusEnum.Active, cancellationToken);

        var dto = new CheckMapOwnershipDto { MapExists = map != null };
        if (map == null)
            return Result<CheckMapOwnershipDto>.Success(dto, "Đã kiểm tra quyền sở hữu bản đồ.");

        var rootMapId = map.RootMapId ?? map.Id;
        var lineMapIds = await mapRepo.GetQueryable()
            .Where(m => m.Status == EntityStatusEnum.Active && (m.RootMapId ?? m.Id) == rootMapId)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var isAuthor = map.CreatedBy.HasValue && map.CreatedBy.Value == userId.Value;
        var purchased = false;
        DateTime? purchasedAt = null;
        var inMyMap = false;
        if (!isAuthor)
        {
            var paymentRepo = _unitOfWork.Repository<PaymentRecord>();
            purchasedAt = await paymentRepo.GetQueryable()
                .Where(p => !p.IsDeleted
                            && p.UserId == userId.Value
                            && p.MapId.HasValue
                            && lineMapIds.Contains(p.MapId.Value)
                            && p.PaymentStatus == PaymentStatusEnum.Completed)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => (DateTime?)p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            purchased = purchasedAt.HasValue;
            if (!purchased)
            {
                var myMapRepo = _unitOfWork.Repository<MyMap>();
                inMyMap = await myMapRepo.GetQueryable()
                    .AnyAsync(mm => !mm.IsDeleted
                                    && mm.UserId == userId.Value
                                    && lineMapIds.Contains(mm.MapId),
                        cancellationToken);
            }
        }

        dto.IsOwned = isAuthor || purchased || inMyMap;
        dto.IsAuthor = isAuthor;
        dto.IsPurchased = purchased;
        dto.PurchasedAt = purchasedAt;
        return Result<CheckMapOwnershipDto>.Success(dto, "Đã kiểm tra quyền sở hữu bản đồ.");
    }
}
