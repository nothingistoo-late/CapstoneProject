using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.CreateDepositOrder;

public class CreateDepositOrderCommandHandler : IRequestHandler<CreateDepositOrderCommand, Result<CreateDepositOrderResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrbitCoinDepositSettings _depositSettings;
    private readonly IPayOSService _payOSService;
    private readonly ICurrentUserService _currentUserService;

    public CreateDepositOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IOrbitCoinDepositSettings depositSettings,
        IPayOSService payOSService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _depositSettings = depositSettings;
        _payOSService = payOSService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CreateDepositOrderResult>> Handle(CreateDepositOrderCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<CreateDepositOrderResult>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        if (request.AmountOrbitCoin <= 0)
            return Result<CreateDepositOrderResult>.Failure("Amount must be positive.", ErrorCodeEnum.ValidationFailed);

        var payOSPayment = await _unitOfWork.Repository<Payment>()
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.Code == "PayOS", cancellationToken);
        if (payOSPayment == null)
            return Result<CreateDepositOrderResult>.Failure("PayOS payment method is not configured. Contact support.", ErrorCodeEnum.InvalidOperation);

        var amountVnd = (long)(request.AmountOrbitCoin * _depositSettings.VndPerOrbitCoin);
        if (amountVnd <= 0)
            return Result<CreateDepositOrderResult>.Failure("Amount too small for conversion.", ErrorCodeEnum.ValidationFailed);

        // Unique order code for PayOS (int)
        var orderCode = Math.Abs((int)(CapstoneProject.Domain.Common.VietnamDateTime.DbNow.Ticks / 10 % int.MaxValue));
        if (orderCode <= 0) orderCode = 1;

        var record = new PaymentRecord
        {
            UserId = userId,
            PackageId = null,
            MapId = null,
            PaymentId = payOSPayment.Id,
            Amount = request.AmountOrbitCoin,
            AmountVnd = amountVnd,
            PaymentStatus = PaymentStatusEnum.Pending,
            PaidAt = null,
            ExternalId = orderCode.ToString()
        };
        record.InitializeEntity(userId);
        await _unitOfWork.Repository<PaymentRecord>().AddAsync(record);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var returnUrl = _depositSettings.ReturnUrlBase.TrimEnd('/') + "?orderId=" + record.Id;
        var cancelUrl = _depositSettings.CancelUrlBase.TrimEnd('/') + "?orderId=" + record.Id;
        var description = $"Nạp {request.AmountOrbitCoin} OrbitCoin";

        var (checkoutUrl, error) = await _payOSService.CreatePaymentLinkAsync(
            orderCode,
            amountVnd,
            description,
            returnUrl,
            cancelUrl,
            cancellationToken);

        if (string.IsNullOrEmpty(checkoutUrl))
            return Result<CreateDepositOrderResult>.Failure(error ?? "Could not create payment link.", ErrorCodeEnum.InvalidOperation);

        return Result<CreateDepositOrderResult>.Success(
            new CreateDepositOrderResult { OrderId = record.Id, AmountVnd = amountVnd, CheckoutUrl = checkoutUrl },
            "Redirect user to CheckoutUrl to complete payment.");
    }
}



