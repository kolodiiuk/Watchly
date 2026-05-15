using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
{
    public void Configure(EntityTypeBuilder<Episode> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id);
        builder.Property(e => e.SeasonId).IsRequired();
        builder.Property(e => e.TvShowId);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.OrdinalNumber).IsRequired();
        builder.Property(e => e.Runtime).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
        builder.Property(e => e.PosterUrl).HasMaxLength(500);
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.ReleaseDate);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Episode)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Season)
            .WithMany(s => s.Episodes)
            .HasForeignKey(e => e.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.TvShow)
            .WithMany(tvShow => tvShow.Episodes)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
