using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Infrastructure.Context.Configurations;

public static class LevelCatalogEntityConfiguration
{
    public static void Configure(EntityTypeBuilder<LevelCatalog> e)
    {
        e.ToTable("LevelCatalogs");
        e.Property(x => x.Name).HasMaxLength(256);
        e.Property(x => x.Type).HasMaxLength(64);
        e.Property(x => x.Difficulty).HasMaxLength(32);
        e.HasIndex(x => x.Name);
        e.HasIndex(x => x.Type);
        e.HasOne(x => x.Detail)
            .WithOne(x => x.LevelCatalog)
            .HasForeignKey<LevelDetail>(x => x.LevelCatalogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
