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

        var isAuthor = map.CreatedBy.HasValue && map.CreatedBy.Value == userId.Value;
        var purchased = false;
        var inMyMap = false;
        if (!isAuthor)
        {
            var paymentRepo = _unitOfWork.Repository<PaymentRecord>();
            purchased = await paymentRepo.GetQueryable()
                .AnyAsync(p => !p.IsDeleted && p.UserId == userId.Value && p.MapId == request.MapId && p.PaymentStatus == PaymentStatusEnum.Completed, cancellationToken);
            if (!purchased)
            {
                var myMapRepo = _unitOfWork.Repository<MyMap>();
                inMyMap = await myMapRepo.GetQueryable()
                    .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId.Value && mm.MapId == request.MapId, cancellationToken);
            }
        }

        dto.IsOwned = isAuthor || purchased || inMyMap;
        dto.IsAuthor = isAuthor;
        return Result<CheckMapOwnershipDto>.Success(dto, "Đã kiểm tra quyền sở hữu bản đồ.");
    }
}
