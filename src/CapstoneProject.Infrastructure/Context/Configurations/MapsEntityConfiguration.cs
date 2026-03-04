using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Infrastructure.Context.Configurations;

public static class MapsEntityConfiguration
{
    public static void Configure(EntityTypeBuilder<Maps> e)
    {
        e.ToTable("LevelMaps");
        e.Property(x => x.ExternalId).HasMaxLength(128);
        e.Property(x => x.Name).HasMaxLength(256);
        e.Property(x => x.JsonContent).HasColumnType("nvarchar(max)");
        e.HasIndex(x => x.ExternalId);
        e.HasIndex(x => x.Name);
    }
}
