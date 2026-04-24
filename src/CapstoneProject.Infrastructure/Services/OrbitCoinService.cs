using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Infrastructure.Context;

namespace CapstoneProject.Infrastructure.Services;

public class OrbitCoinService : IOrbitCoinService
{
    private readonly CapstoneProjectDbContext _db;

    public OrbitCoinService(CapstoneProjectDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _db.UserWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted, cancellationToken);
        return wallet?.Balance ?? 0;
    }

    public async Task<(IReadOnlyList<OrbitCoinTransactionDto> Items, int TotalCount)> GetTransactionHistoryAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        OrbitCoinTransactionFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.OrbitCoinTransactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        var statusSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(filter?.Status))
        {
            statusSet.Add(filter.Status.Trim());
        }
        if (filter?.Statuses is { Count: > 0 })
        {
            foreach (var item in filter.Statuses)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    statusSet.Add(item.Trim());
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(filter?.Direction))
        {
            var direction = filter.Direction.Trim().ToLowerInvariant();
            if (direction is "in" or "credit")
            {
                query = query.Where(t => t.Amount >= 0);
            }
            else if (direction is "out" or "debit")
            {
                query = query.Where(t => t.Amount < 0);
            }
        }

        if (filter?.Categories is { Count: > 0 })
        {
            query = query.Where(t => filter.Categories.Contains(t.TransactionType));
        }

        if (!string.IsNullOrWhiteSpace(filter?.RelatedEntityType))
        {
            var relatedType = filter.RelatedEntityType.Trim();
            query = query.Where(t => t.RelatedEntityType != null && t.RelatedEntityType == relatedType);
        }

        if (filter?.RelatedEntityId.HasValue == true)
        {
            query = query.Where(t => t.RelatedEntityId == filter.RelatedEntityId);
        }

        if (filter?.From.HasValue == true)
        {
            query = query.Where(t => t.CreatedAt >= filter.From.Value);
        }

        if (filter?.To.HasValue == true)
        {
            query = query.Where(t => t.CreatedAt <= filter.To.Value);
        }

        if (filter?.MinAmount.HasValue == true)
        {
            query = query.Where(t => Math.Abs(t.Amount) >= filter.MinAmount.Value);
        }

        if (filter?.MaxAmount.HasValue == true)
        {
            query = query.Where(t => Math.Abs(t.Amount) <= filter.MaxAmount.Value);
        }

        if (statusSet.Count > 0)
        {
            var allowedStatuses = Enum.GetValues<PaymentStatusEnum>()
                .Where(x => statusSet.Contains(x.ToString()))
                .ToList();
            if (allowedStatuses.Count > 0)
            {
                query = query.Where(t =>
                    t.RelatedEntityType != null
                    && (t.RelatedEntityType.ToLower() == "payment" || t.RelatedEntityType.ToLower() == "paymentrecord")
                    && t.RelatedEntityId.HasValue
                        && _db.PaymentRecords.AsNoTracking().Any(p =>
                            p.Id == t.RelatedEntityId.Value
                            && allowedStatuses.Contains(p.PaymentStatus)));
            }
        }

        if (!string.IsNullOrWhiteSpace(filter?.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(t =>
                (t.Note != null && t.Note.ToLower().Contains(term))
                || (t.RelatedEntityType != null && t.RelatedEntityType.ToLower().Contains(term))
                || (t.RelatedEntityId.HasValue && t.RelatedEntityId.Value.ToString().ToLower().Contains(term)));
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);
        var txPage = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        var relatedPaymentIds = txPage
            .Where(t =>
                t.RelatedEntityId.HasValue
                && (string.Equals(t.RelatedEntityType, "Payment", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(t.RelatedEntityType, "PaymentRecord", StringComparison.OrdinalIgnoreCase)))
            .Select(t => t.RelatedEntityId!.Value)
            .Distinct()
            .ToList();

        var relatedGameIds = txPage
            .Where(t =>
                t.RelatedEntityId.HasValue
                && (string.Equals(t.RelatedEntityType, "Game", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(t.RelatedEntityType, "GameEscrow", StringComparison.OrdinalIgnoreCase)))
            .Select(t => t.RelatedEntityId!.Value)
            .Distinct()
            .ToList();

        var paymentMap = relatedPaymentIds.Count == 0
            ? new Dictionary<Guid, PaymentRecord>()
            : await _db.PaymentRecords
                .AsNoTracking()
                .Where(p => relatedPaymentIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

        var gameIds = paymentMap.Values
            .Where(p => p.GameId.HasValue)
            .Select(p => p.GameId!.Value)
            .Concat(relatedGameIds)
            .Distinct()
            .ToList();

        var gameCreatorMap = gameIds.Count == 0
            ? new Dictionary<Guid, Guid>()
            : await _db.Games
                .AsNoTracking()
                .Where(g => gameIds.Contains(g.Id))
                .Select(g => new { g.Id, g.CreatedBy })
                .ToDictionaryAsync(x => x.Id, x => x.CreatedBy ?? Guid.Empty, cancellationToken);

        var userIdsForLookup = new HashSet<Guid>();
        foreach (var payment in paymentMap.Values)
        {
            userIdsForLookup.Add(payment.UserId);
            if (payment.GameId.HasValue && gameCreatorMap.TryGetValue(payment.GameId.Value, out var creatorId) && creatorId != Guid.Empty)
            {
                userIdsForLookup.Add(creatorId);
            }
        }
        foreach (var creatorId in gameCreatorMap.Values)
        {
            if (creatorId != Guid.Empty)
            {
                userIdsForLookup.Add(creatorId);
            }
        }

        var userNameMap = userIdsForLookup.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users
                .AsNoTracking()
                .Where(u => userIdsForLookup.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.UserName })
                .ToDictionaryAsync(
                    x => x.Id,
                    x => $"{x.FirstName} {x.LastName}".Trim().Length > 0 ? $"{x.FirstName} {x.LastName}".Trim() : (x.UserName ?? x.Id.ToString()),
                    cancellationToken);

        var items = txPage
            .Select(t => new OrbitCoinTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                AmountVnd = t.RelatedEntityId.HasValue && paymentMap.TryGetValue(t.RelatedEntityId.Value, out var payment)
                    ? payment.AmountVnd
                    : null,
                TransactionType = t.TransactionType,
                Direction = t.Amount >= 0 ? "In" : "Out",
                Category = ResolveCategory(t.TransactionType),
                RelatedEntityType = t.RelatedEntityType,
                RelatedEntityId = t.RelatedEntityId,
                PaymentRecordId = t.RelatedEntityId.HasValue && paymentMap.ContainsKey(t.RelatedEntityId.Value)
                    ? t.RelatedEntityId
                    : null,
                GameId = t.RelatedEntityId.HasValue && paymentMap.TryGetValue(t.RelatedEntityId.Value, out var paymentByGame)
                    ? paymentByGame.GameId
                    : null,
                PackageId = t.RelatedEntityId.HasValue && paymentMap.TryGetValue(t.RelatedEntityId.Value, out var paymentByPackage)
                    ? paymentByPackage.PackageId
                    : null,
                CounterpartyName = ResolveCounterpartyName(userId, t, paymentMap, gameCreatorMap, userNameMap),
                GrossAmount = Math.Abs(t.Amount),
                NetAmount = t.Amount >= 0 ? Math.Abs(t.Amount) : -Math.Abs(t.Amount),
                FeeAmount = t.FeeAmount,
                Status = t.RelatedEntityId.HasValue && paymentMap.TryGetValue(t.RelatedEntityId.Value, out var paymentStatus)
                    ? paymentStatus.PaymentStatus.ToString()
                    : "Completed",
                BalanceAfter = t.BalanceAfter,
                Note = t.Note,
                CreatedAt = t.CreatedAt
            })
            .ToList();
        return (items, total);
    }

    private static string ResolveCounterpartyName(
        Guid currentUserId,
        OrbitCoinTransaction transaction,
        Dictionary<Guid, PaymentRecord> paymentMap,
        Dictionary<Guid, Guid> gameCreatorMap,
        Dictionary<Guid, string> userNameMap)
    {
        if (transaction.RelatedEntityId.HasValue && paymentMap.TryGetValue(transaction.RelatedEntityId.Value, out var payment))
        {
            if (payment.GameId.HasValue)
            {
                if (transaction.Amount < 0 && gameCreatorMap.TryGetValue(payment.GameId.Value, out var creatorId))
                {
                    if (creatorId != Guid.Empty && userNameMap.TryGetValue(creatorId, out var creatorName))
                    {
                        return creatorName;
                    }
                }
                if (transaction.Amount >= 0 && payment.UserId != currentUserId && userNameMap.TryGetValue(payment.UserId, out var buyerName))
                {
                    return buyerName;
                }
            }
            if (payment.PackageId.HasValue)
            {
                return "System";
            }
            if (payment.UserId != currentUserId && userNameMap.TryGetValue(payment.UserId, out var paymentUserName))
            {
                return paymentUserName;
            }
        }
        if (transaction.RelatedEntityId.HasValue
            && (string.Equals(transaction.RelatedEntityType, "Game", StringComparison.OrdinalIgnoreCase)
                || string.Equals(transaction.RelatedEntityType, "GameEscrow", StringComparison.OrdinalIgnoreCase)))
        {
            if (gameCreatorMap.TryGetValue(transaction.RelatedEntityId.Value, out var creatorId)
                && creatorId != Guid.Empty
                && userNameMap.TryGetValue(creatorId, out var creatorName))
            {
                return creatorName;
            }
        }
        return "System";
    }

    private static string ResolveCategory(CoinTransactionTypeEnum type) => type switch
    {
        CoinTransactionTypeEnum.EarnDeposit => "Topup",
        CoinTransactionTypeEnum.SpendPackagePurchase => "BuyPackage",
        CoinTransactionTypeEnum.SpendMapPurchase => "BuyGame",
        CoinTransactionTypeEnum.EarnMapSold => "GameRevenue",
        CoinTransactionTypeEnum.Refund => "Refund",
        CoinTransactionTypeEnum.SystemFee => "SystemFee",
        CoinTransactionTypeEnum.AdminAdjustment => "AdminAdjustment",
        _ => "Other"
    };

    public async Task<(bool Success, string? Error)> CreditAsync(
        Guid userId,
        decimal amount,
        CoinTransactionTypeEnum transactionType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        decimal feeAmount,
        string? note,
        Guid? createdBy,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return (false, "Credit amount must be positive.");
        var wallet = await GetOrCreateWalletAsync(userId, cancellationToken);
        wallet.Balance += amount;
        wallet.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        wallet.UpdatedBy = createdBy;
        var tx = new OrbitCoinTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            TransactionType = transactionType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            FeeAmount = feeAmount,
            BalanceAfter = wallet.Balance,
            Note = note,
            CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            CreatedBy = createdBy
        };
        _db.OrbitCoinTransactions.Add(tx);
        await _db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DebitAsync(
        Guid userId,
        decimal amount,
        CoinTransactionTypeEnum transactionType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        decimal feeAmount,
        string? note,
        Guid? createdBy,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return (false, "Debit amount must be positive.");
        var wallet = await GetOrCreateWalletAsync(userId, cancellationToken);
        var totalDebit = amount + feeAmount;
        if (wallet.Balance < totalDebit)
            return (false, "Insufficient OrbitCoin balance.");
        wallet.Balance -= totalDebit;
        wallet.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        wallet.UpdatedBy = createdBy;
        var tx = new OrbitCoinTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = -totalDebit,
            TransactionType = transactionType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            FeeAmount = feeAmount,
            BalanceAfter = wallet.Balance,
            Note = note,
            CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            CreatedBy = createdBy
        };
        _db.OrbitCoinTransactions.Add(tx);
        await _db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> TransferAsync(
        Guid buyerUserId,
        Guid sellerUserId,
        decimal amount,
        decimal feeAmount,
        CoinTransactionTypeEnum buyerTransactionType,
        CoinTransactionTypeEnum sellerTransactionType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return (false, "Transfer amount must be positive.");
        var buyerWallet = await GetOrCreateWalletAsync(buyerUserId, cancellationToken);
        var totalCharge = amount + feeAmount;
        if (buyerWallet.Balance < totalCharge)
            return (false, "Insufficient OrbitCoin balance.");
        var sellerWallet = await GetOrCreateWalletAsync(sellerUserId, cancellationToken);

        buyerWallet.Balance -= totalCharge;
        buyerWallet.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        sellerWallet.Balance += amount;
        sellerWallet.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;

        var buyerTx = new OrbitCoinTransaction
        {
            Id = Guid.NewGuid(),
            UserId = buyerUserId,
            Amount = -totalCharge,
            TransactionType = buyerTransactionType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            FeeAmount = feeAmount,
            BalanceAfter = buyerWallet.Balance,
            Note = note,
            CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
        };
        var sellerTx = new OrbitCoinTransaction
        {
            Id = Guid.NewGuid(),
            UserId = sellerUserId,
            Amount = amount,
            TransactionType = sellerTransactionType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            FeeAmount = 0,
            BalanceAfter = sellerWallet.Balance,
            Note = note,
            CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
        };
        _db.OrbitCoinTransactions.Add(buyerTx);
        _db.OrbitCoinTransactions.Add(sellerTx);
        await _db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    /// <summary>
    /// Buyer pays amount (full price). Seller receives amount - feeAmount (seller bears the fee).
    /// </summary>
    public async Task<(bool Success, string? Error)> TransferWithSellerFeeAsync(
        Guid buyerUserId,
        Guid sellerUserId,
        decimal amount,
        decimal feeAmount,
        CoinTransactionTypeEnum buyerTransactionType,
        CoinTransactionTypeEnum sellerTransactionType,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return (false, "Transfer amount must be positive.");
        var sellerReceives = amount - feeAmount;
        if (sellerReceives < 0)
            return (false, "Fee cannot exceed amount.");

        var buyerWallet = await GetOrCreateWalletAsync(buyerUserId, cancellationToken);
        if (buyerWallet.Balance < amount)
            return (false, "Insufficient OrbitCoin balance.");
        var sellerWallet = await GetOrCreateWalletAsync(sellerUserId, cancellationToken);

        buyerWallet.Balance -= amount;
        buyerWallet.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        sellerWallet.Balance += sellerReceives;
        sellerWallet.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;

        var buyerTx = new OrbitCoinTransaction
        {
            Id = Guid.NewGuid(),
            UserId = buyerUserId,
            Amount = -amount,
            TransactionType = buyerTransactionType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            FeeAmount = 0,
            BalanceAfter = buyerWallet.Balance,
            Note = note,
            CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
        };
        var sellerTx = new OrbitCoinTransaction
        {
            Id = Guid.NewGuid(),
            UserId = sellerUserId,
            Amount = sellerReceives,
            TransactionType = sellerTransactionType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            FeeAmount = feeAmount,
            BalanceAfter = sellerWallet.Balance,
            Note = note,
            CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
        };
        _db.OrbitCoinTransactions.Add(buyerTx);
        _db.OrbitCoinTransactions.Add(sellerTx);
        await _db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private async Task<UserWallet> GetOrCreateWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wallet = await _db.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted, cancellationToken);
        if (wallet != null)
            return wallet;
        wallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 0,
            CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
            Status = EntityStatusEnum.Active
        };
        _db.UserWallets.Add(wallet);
        await _db.SaveChangesAsync(cancellationToken);
        return wallet;
    }
}



