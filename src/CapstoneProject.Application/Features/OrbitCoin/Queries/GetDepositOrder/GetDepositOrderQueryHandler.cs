using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetDepositOrder;

public class GetDepositOrderQueryHandler : IRequestHandler<GetDepositOrderQuery, Result<DepositOrderDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetDepositOrderQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DepositOrderDetailDto>> Handle(GetDepositOrderQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<DepositOrderDetailDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var record = await _unitOfWork.Repository<PaymentRecord>()
            .GetQueryable()
            .AsNoTracking()
            .Include(r => r.Payment)
            .FirstOrDefaultAsync(
                r => r.Id == request.OrderId
                     && r.UserId == userId.Value
                     && r.PackageId == null
                     && r.GameId == null,
                cancellationToken);

        if (record == null)
            return Result<DepositOrderDetailDto>.Failure("Không tìm thấy đơn đặt hàng hoặc quyền truy cập bị từ chối.", ErrorCodeEnum.NotFound);

        var code = record.Payment?.Code?.Trim();
        var name = record.Payment?.Name?.Trim();
        if (string.IsNullOrEmpty(code)) code = "PayOS";
        if (string.IsNullOrEmpty(name)) name = "PayOS";

        var dto = new DepositOrderDetailDto
        {
            OrderId = record.Id,
            PaymentStatus = record.PaymentStatus.ToString(),
            CreatedAt = record.CreatedAt,
            PaidAt = record.PaidAt,
            AmountOrbitCoin = record.Amount,
            AmountVnd = record.AmountVnd,
            ExternalOrderCode = record.ExternalId,
            PaymentMethodCode = code,
            PaymentMethodName = name,
        };

        return Result<DepositOrderDetailDto>.Success(dto, "Đã lấy lại lệnh gửi tiền.");
    }
}
