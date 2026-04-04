using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Infrastructure.Context.Configurations;

public static class OrbitCoinEntityConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<UserWallet>(ConfigureUserWallet);
        builder.Entity<OrbitCoinTransaction>(ConfigureOrbitCoinTransaction);
        builder.Entity<ExchangeRate>(ConfigureExchangeRate);
    }

    static void ConfigureUserWallet(EntityTypeBuilder<UserWallet> e)
    {
        e.HasIndex(x => x.UserId).IsUnique();
        e.Property(x => x.Balance).HasPrecision(18, 4);
        e.Property(x => x.RowVersion).IsRowVersion();
    }

    static void ConfigureOrbitCoinTransaction(EntityTypeBuilder<OrbitCoinTransaction> e)
    {
        e.Property(x => x.TransactionType).HasConversion<int>();
        e.Property(x => x.Amount).HasPrecision(18, 4);
        e.Property(x => x.FeeAmount).HasPrecision(18, 4);
        e.Property(x => x.BalanceAfter).HasPrecision(18, 4);
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.CreatedAt);
        e.HasIndex(x => new { x.RelatedEntityType, x.RelatedEntityId });
    }

    static void ConfigureExchangeRate(EntityTypeBuilder<ExchangeRate> e)
    {
        e.Property(x => x.Rate).HasPrecision(18, 4);
        e.HasIndex(x => new { x.FromCurrency, x.ToCurrency, x.IsActive });
        e.HasIndex(x => x.CreatedAt).HasFilter("\"IsDeleted\" = false");
    }
}
