using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.PurchaseMapWithOrbitCoin;

public class PurchaseMapWithOrbitCoinCommandHandler : IRequestHandler<PurchaseMapWithOrbitCoinCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ICurrentUserService _currentUserService;

    private const decimal PlatformFeePercent = 5m; // 5% platform fee

    public PurchaseMapWithOrbitCoinCommandHandler(
        IUnitOfWork unitOfWork,
        IOrbitCoinService orbitCoinService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _orbitCoinService = orbitCoinService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(PurchaseMapWithOrbitCoinCommand request, CancellationToken cancellationToken)
    {
        var (isValid, buyerId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !buyerId.HasValue)
            return Result.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);
        var buyerUserId = buyerId.Value;

        var map = await _unitOfWork.Repository<Map>()
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result.Failure("Map not found.", ErrorCodeEnum.NotFound);
        if (map.Price == null || map.Price <= 0)
            return Result.Failure("This map is free and cannot be purchased with OrbitCoin.", ErrorCodeEnum.InvalidOperation);
        var sellerUserId = map.CreatedBy ?? Guid.Empty;
        if (sellerUserId == Guid.Empty)
            return Result.Failure("Map has no creator; cannot complete purchase.", ErrorCodeEnum.InvalidOperation);
        if (sellerUserId == buyerUserId)
            return Result.Failure("You cannot purchase your own map.", ErrorCodeEnum.InvalidOperation);

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
            return Result.Failure(error ?? "Transfer failed.", ErrorCodeEnum.InvalidOperation);

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
                PaidAt = CapstoneProject.Domain.Common.VietnamDateTime.Now,
                PaymentId = orbitCoinPayment.Id
            };
            record.InitializeEntity(buyerUserId);
            await _unitOfWork.Repository<PaymentRecord>().AddAsync(record);

            var myMap = new MyMap { MapId = map.Id, UserId = buyerUserId, IsAuthor = false };
            myMap.InitializeEntity(buyerUserId);
            await _unitOfWork.Repository<MyMap>().AddAsync(myMap);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success("Map purchased with OrbitCoin. Platform fee is deducted from seller.");
    }
}

