using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> b)
    {
        b.HasKey(s => s.Id);

        b.Property(s=>s.Id);
        b.Property(s=>s.OrdinalNumber).IsRequired();
        b.Property(s=> s.Name).HasMaxLength(300);
        b.Property(s=>s.TitleId).IsRequired();

        b.HasOne(s => s.Title)
            .WithMany(t => t.Seasons)
            .HasForeignKey(s => s.TitleId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(s => s.Episodes)
            .WithOne(e => e.Season)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
