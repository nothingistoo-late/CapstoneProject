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
        CancellationToken cancellationToken = default)
    {
        var query = _db.OrbitCoinTransactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new OrbitCoinTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                TransactionType = t.TransactionType,
                RelatedEntityType = t.RelatedEntityType,
                RelatedEntityId = t.RelatedEntityId,
                FeeAmount = t.FeeAmount,
                BalanceAfter = t.BalanceAfter,
                Note = t.Note,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
        return (items, total);
    }

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
        wallet.UpdatedAt = DateTime.UtcNow;
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
            CreatedAt = DateTime.UtcNow,
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
        wallet.UpdatedAt = DateTime.UtcNow;
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
            CreatedAt = DateTime.UtcNow,
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
        buyerWallet.UpdatedAt = DateTime.UtcNow;
        sellerWallet.Balance += amount;
        sellerWallet.UpdatedAt = DateTime.UtcNow;

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
            CreatedAt = DateTime.UtcNow
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
            CreatedAt = DateTime.UtcNow
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
        buyerWallet.UpdatedAt = DateTime.UtcNow;
        sellerWallet.Balance += sellerReceives;
        sellerWallet.UpdatedAt = DateTime.UtcNow;

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
            CreatedAt = DateTime.UtcNow
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
            CreatedAt = DateTime.UtcNow
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
            CreatedAt = DateTime.UtcNow,
            Status = EntityStatusEnum.Active
        };
        _db.UserWallets.Add(wallet);
        await _db.SaveChangesAsync(cancellationToken);
        return wallet;
    }
}
