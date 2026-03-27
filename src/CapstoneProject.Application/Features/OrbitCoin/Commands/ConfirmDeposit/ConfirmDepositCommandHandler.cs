using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.ConfirmDeposit;

public class ConfirmDepositCommandHandler : IRequestHandler<ConfirmDepositCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayOSService _payOSService;
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ICurrentUserService _currentUserService;

    public ConfirmDepositCommandHandler(
        IUnitOfWork unitOfWork,
        IPayOSService payOSService,
        IOrbitCoinService orbitCoinService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _payOSService = payOSService;
        _orbitCoinService = orbitCoinService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ConfirmDepositCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var record = await _unitOfWork.Repository<PaymentRecord>()
            .GetQueryable()
            .FirstOrDefaultAsync(r => r.Id == request.OrderId && r.UserId == userId.Value, cancellationToken);
        if (record == null)
            return Result.Failure("Order not found or access denied.", ErrorCodeEnum.NotFound);
        if (record.PaymentStatus == PaymentStatusEnum.Completed)
            return Result.Success("Deposit already completed. OrbitCoin was credited.");

        if (string.IsNullOrEmpty(record.ExternalId) || !long.TryParse(record.ExternalId, out var orderCode))
            return Result.Failure("Invalid order data.", ErrorCodeEnum.InvalidOperation);

        var isPaid = await _payOSService.GetPaymentStatusByOrderCodeAsync(orderCode, cancellationToken);
        if (isPaid == null)
            return Result.Failure("Could not verify payment status. Please try again or contact support.", ErrorCodeEnum.InvalidOperation);
        if (isPaid != true)
            return Result.Failure("Payment not completed yet. Please wait or check PayOS.", ErrorCodeEnum.InvalidOperation);

        var (success, error) = await _orbitCoinService.CreditAsync(
            record.UserId,
            record.Amount,
            CoinTransactionTypeEnum.EarnDeposit,
            "Payment",
            record.Id,
            feeAmount: 0,
            $"Nạp OrbitCoin qua PayOS (order {orderCode})",
            null,
            cancellationToken);
        if (!success)
            return Result.Failure(error ?? "Credit failed.", ErrorCodeEnum.InvalidOperation);

        record.PaymentStatus = PaymentStatusEnum.Completed;
        record.PaidAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        _unitOfWork.Repository<PaymentRecord>().Update(record);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Deposit confirmed. OrbitCoin has been credited.");
    }
}



