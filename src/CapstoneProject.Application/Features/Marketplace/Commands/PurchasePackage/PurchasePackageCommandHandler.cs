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

namespace CapstoneProject.Application.Features.Marketplace.Commands.PurchasePackage;

public class PurchasePackageCommandHandler : IRequestHandler<PurchasePackageCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public PurchasePackageCommandHandler(
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

    public async Task<Result<Guid>> Handle(PurchasePackageCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để mua gói.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var pkg = await _unitOfWork.Repository<Package>().GetQueryable()
            .FirstOrDefaultAsync(p => p.Id == command.PackageId && !p.IsDeleted && p.Status == EntityStatusEnum.Active, cancellationToken);
        if (pkg == null)
            return Result<Guid>.Failure("Gói không được tìm thấy hoặc không hoạt động.", ErrorCodeEnum.NotFound);
        if (pkg.Price <= 0)
            return Result<Guid>.Failure("Gói này không có giá; liên hệ hỗ trợ.", ErrorCodeEnum.InvalidOperation);

        // Deduct OrbitCoin (platform only accepts OrbitCoin; user must have topped up first)
        var (success, error) = await _orbitCoinService.DebitAsync(
            userId,
            pkg.Price,
            CoinTransactionTypeEnum.SpendPackagePurchase,
            "Package",
            pkg.Id,
            feeAmount: 0,
            $"Purchase package: {pkg.Name}",
            userId,
            cancellationToken);
        if (!success)
        {
            var failedRecord = new PaymentRecord
            {
                UserId = userId,
                PackageId = pkg.Id,
                Amount = pkg.Price,
                PaymentStatus = PaymentStatusEnum.Failed,
                PaidAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                PaymentId = null
            };
            failedRecord.InitializeEntity(userId);
            await _unitOfWork.Repository<PaymentRecord>().AddAsync(failedRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await TryNotifyPaymentAsync(
                NotificationTypeEnum.PaymentFailed,
                userId,
                pkg,
                pkg.Price,
                "Thanh toán mua gói thất bại",
                error ?? "Giao dịch mua gói không thành công.",
                cancellationToken);

            return Result<Guid>.Failure(error ?? "OrbitCoin không đủ. Vui lòng nạp tiền trước.", ErrorCodeEnum.InvalidOperation);
        }

        var orbitCoinPayment = await _unitOfWork.Repository<Payment>()
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.Code == "OrbitCoin", cancellationToken);

        var record = new PaymentRecord
        {
            UserId = userId,
            PackageId = pkg.Id,
            Amount = pkg.Price,
            PaymentStatus = PaymentStatusEnum.Completed,
            PaidAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            PaymentId = orbitCoinPayment?.Id
        };
        record.InitializeEntity(userId);
        await _unitOfWork.Repository<PaymentRecord>().AddAsync(record);

        var remaining = pkg.Limit ?? 1;
        var expiresAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow.AddDays(pkg.DurationDays);
        var userPkg = new UserPackage
        {
            UserId = userId,
            PackageId = pkg.Id,
            Remaining = remaining,
            ExpiresAt = expiresAt
        };
        userPkg.InitializeEntity(userId);
        await _unitOfWork.Repository<UserPackage>().AddAsync(userPkg);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await TryNotifyPaymentAsync(
            NotificationTypeEnum.PaymentSucceeded,
            userId,
            pkg,
            pkg.Price,
            "Mua gói thành công",
            $"Bạn đã mua thành công gói \"{pkg.Name}\" với giá {pkg.Price:0.##} OrbitCoin.",
            cancellationToken);

        return Result<Guid>.Success(record.Id, "Gói mua bằng OrbitCoin.");
    }

    private async Task TryNotifyPaymentAsync(
        NotificationTypeEnum type,
        Guid recipientUserId,
        Package pkg,
        decimal amount,
        string title,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            var payloadJson = JsonSerializer.Serialize(new
            {
                packageId = pkg.Id,
                packageName = pkg.Name,
                amount
            });

            await _notificationPersistenceService.CreateNotificationAsync(
                type,
                title,
                body,
                new List<Guid> { recipientUserId },
                null,
                payloadJson,
                "/learner/wallet",
                cancellationToken);
        }
        catch
        {
            // Notification failure must not break package purchase flow.
        }
    }
}



