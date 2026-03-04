using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Infrastructure.Context.Configurations;

public static class LevelDetailEntityConfiguration
{
    public static void Configure(EntityTypeBuilder<LevelDetail> e)
    {
        e.ToTable("LevelDetails");
        e.Property(x => x.JsonContent).HasColumnType("nvarchar(max)");
        e.HasIndex(x => x.LevelCatalogId).IsUnique();
    }
}
