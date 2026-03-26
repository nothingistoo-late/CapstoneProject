using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.HandlePayOSWebhook;

public class HandlePayOSWebhookCommandHandler : IRequestHandler<HandlePayOSWebhookCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayOSService _payOSService;
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ILogger<HandlePayOSWebhookCommandHandler> _logger;

    public HandlePayOSWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IPayOSService payOSService,
        IOrbitCoinService orbitCoinService,
        ILogger<HandlePayOSWebhookCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _payOSService = payOSService;
        _orbitCoinService = orbitCoinService;
        _logger = logger;
    }

    public async Task<bool> Handle(HandlePayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        var verified = await _payOSService.VerifyWebhookAsync(request.WebhookJson, cancellationToken);
        if (verified == null)
        {
            _logger.LogWarning("PayOS webhook: verification failed or invalid payload.");
            return false;
        }

        _logger.LogInformation("PayOS webhook: verified orderCode={OrderCode}, amount={Amount}", verified.OrderCode, verified.Amount);

        var record = await _unitOfWork.Repository<PaymentRecord>()
            .GetQueryable()
            .FirstOrDefaultAsync(r => r.ExternalId == verified.OrderCode.ToString(), cancellationToken);
        if (record == null)
        {
            _logger.LogWarning("PayOS webhook: no PaymentRecord found for orderCode={OrderCode}. Check if webhook URL is correct and order was created by this app.", verified.OrderCode);
            return true; // idempotent: unknown order, still return 200
        }
        if (record.PaymentStatus == PaymentStatusEnum.Completed)
        {
            _logger.LogInformation("PayOS webhook: orderCode={OrderCode} already completed, skipping.", verified.OrderCode);
            return true; // already processed
        }

        var (success, error) = await _orbitCoinService.CreditAsync(
            record.UserId,
            record.Amount,
            CoinTransactionTypeEnum.EarnDeposit,
            "Payment",
            record.Id,
            feeAmount: 0,
            $"Náº¡p OrbitCoin qua PayOS (order {verified.OrderCode})",
            null,
            cancellationToken);
        if (!success)
        {
            _logger.LogError("PayOS webhook: CreditAsync failed for orderCode={OrderCode}, userId={UserId}: {Error}", verified.OrderCode, record.UserId, error);
            return false;
        }

        record.PaymentStatus = PaymentStatusEnum.Completed;
        record.PaidAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        _unitOfWork.Repository<PaymentRecord>().Update(record);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("PayOS webhook: credited userId={UserId} amount={Amount} OrbitCoin for orderCode={OrderCode}.", record.UserId, record.Amount, verified.OrderCode);
        return true;
    }
}



