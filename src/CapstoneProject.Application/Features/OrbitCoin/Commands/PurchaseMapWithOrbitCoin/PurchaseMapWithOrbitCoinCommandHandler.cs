using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using System.Text.Json;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.PurchaseMapWithOrbitCoin;

public class PurchaseMapWithOrbitCoinCommandHandler : IRequestHandler<PurchaseMapWithOrbitCoinCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    private const decimal PlatformFeePercent = 5m; // 5% platform fee

    public PurchaseMapWithOrbitCoinCommandHandler(
        IUnitOfWork unitOfWork,
        IOrbitCoinService orbitCoinService,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _orbitCoinService = orbitCoinService;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result> Handle(PurchaseMapWithOrbitCoinCommand request, CancellationToken cancellationToken)
    {
        var (isValid, buyerId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !buyerId.HasValue)
            return Result.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
        var buyerUserId = buyerId.Value;

        var map = await _unitOfWork.Repository<Map>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure("Bản đồ không được tìm thấy.", ErrorCodeEnum.NotFound);
        if (map.Price == null || map.Price <= 0)
            return Result.Failure("Bản đồ này miễn phí và không thể mua bằng OrbitCoin.", ErrorCodeEnum.InvalidOperation);
        var sellerUserId = map.CreatedBy ?? Guid.Empty;
        if (sellerUserId == Guid.Empty)
            return Result.Failure("Bản đồ không có người tạo; không thể hoàn tất việc mua hàng.", ErrorCodeEnum.InvalidOperation);
        if (sellerUserId == buyerUserId)
            return Result.Failure("Bạn không thể mua bản đồ của riêng bạn.", ErrorCodeEnum.InvalidOperation);

        var amount = map.Price.Value;
        var feeAmount = Math.Round(amount * (PlatformFeePercent / 100m), 4);

        // NgÆ°á»i mua tráº£ Ä‘Ãºng giÃ¡ map; ngÆ°á»i bÃ¡n nháº­n = giÃ¡ - phÃ­ (ngÆ°á»i bÃ¡n chá»‹u phÃ­)
        var (success, error) = await _orbitCoinService.TransferWithSellerFeeAsync(
            buyerUserId,
            sellerUserId,
            amount,
            feeAmount,
            CoinTransactionTypeEnum.SpendMapPurchase,
            CoinTransactionTypeEnum.EarnMapSold,
            "Map",
            map.Id,
            $"Purchase map: {map.Title}",
            cancellationToken);

        if (!success)
        {
            var failedRecord = new PaymentRecord
            {
                UserId = buyerUserId,
                MapId = map.Id,
                Amount = amount,
                PaymentStatus = PaymentStatusEnum.Failed,
                PaidAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                PaymentId = null
            };
            failedRecord.InitializeEntity(buyerUserId);
            await _unitOfWork.Repository<PaymentRecord>().AddAsync(failedRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await TryNotifyPaymentAsync(
                NotificationTypeEnum.PaymentFailed,
                buyerUserId,
                null,
                map,
                amount,
                "Thanh toán mua map thất bại",
                error ?? "Giao dịch mua map không thành công.",
                cancellationToken);

            return Result.Failure(error ?? "Chuyển không thành công.", ErrorCodeEnum.InvalidOperation);
        }

        // Reuse PaymentRecords: record this map purchase (paid with OrbitCoin) for unified purchase history
        var orbitCoinPayment = await _unitOfWork.Repository<Payment>()
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.Code == "OrbitCoin", cancellationToken);
        if (orbitCoinPayment != null)
        {
            var record = new PaymentRecord
            {
                UserId = buyerUserId,
                MapId = map.Id,
                Amount = amount,
                PaymentStatus = PaymentStatusEnum.Completed,
                PaidAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                PaymentId = orbitCoinPayment.Id
            };
            record.InitializeEntity(buyerUserId);
            await _unitOfWork.Repository<PaymentRecord>().AddAsync(record);

            var myMap = new MyMap { MapId = map.Id, UserId = buyerUserId, IsAuthor = false };
            myMap.InitializeEntity(buyerUserId);
            await _unitOfWork.Repository<MyMap>().AddAsync(myMap);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await TryNotifyPaymentAsync(
                NotificationTypeEnum.PaymentSucceeded,
                buyerUserId,
                null,
                map,
                amount,
                "Mua map thành công",
                $"Bạn đã mua thành công map \"{map.Title}\" với giá {amount:0.##} OrbitCoin.",
                cancellationToken);

            // Notify map creator about the purchase
            if (sellerUserId != Guid.Empty && sellerUserId != buyerUserId)
            {
                try
                {
                    var sellerPayloadJson = JsonSerializer.Serialize(new
                    {
                        mapId = map.Id,
                        mapTitle = map.Title,
                        buyerId = buyerUserId,
                        buyerAmount = amount,
                        sellerEarns = amount - feeAmount,
                        platformFee = feeAmount
                    });

                    await _notificationPersistenceService.CreateNotificationAsync(
                        NotificationTypeEnum.MapPurchased,
                        "Có người mua map của bạn",
                        $"Map \"{map.Title}\" vừa được mua với giá {amount:0.##} OrbitCoin. Bạn nhận được {amount - feeAmount:0.##} OrbitCoin (sau phí).",
                        new List<Guid> { sellerUserId },
                        buyerUserId,
                        sellerPayloadJson,
                        $"/learner/maps/{map.Id}",
                        cancellationToken);
                }
                catch
                {
                    // Notification failure must not break purchase flow.
                }
            }
        }

        return Result.Success("Bản đồ được mua bằng OrbitCoin. Phí nền tảng được khấu trừ từ người bán.");
    }

    private async Task TryNotifyPaymentAsync(
        NotificationTypeEnum type,
        Guid recipientUserId,
        Guid? actorUserId,
        Map map,
        decimal amount,
        string title,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            var payloadJson = JsonSerializer.Serialize(new
            {
                mapId = map.Id,
                mapTitle = map.Title,
                amount
            });

            await _notificationPersistenceService.CreateNotificationAsync(
                type,
                title,
                body,
                new List<Guid> { recipientUserId },
                actorUserId,
                payloadJson,
                $"/learner/maps/{map.Id}",
                cancellationToken);
        }
        catch
        {
            // Notification failure must not break purchase flow.
        }
    }
}



