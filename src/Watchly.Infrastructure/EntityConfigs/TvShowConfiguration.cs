using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class TvShowConfiguration : IEntityTypeConfiguration<TvShow>
{
    public void Configure(EntityTypeBuilder<TvShow> builder)
    {
        builder.Property(t => t.NumberOfSeasons).IsRequired();
        builder.Property(t => t.NumberOfEpisodes).IsRequired();
        builder.Property(t => t.OriginalLanguage).HasMaxLength(50);
        builder.Property(t => t.VoteAverage);
        builder.Property(t => t.FirstAirDate);
        builder.Property(t => t.LastAirDate);
        builder.Property(t => t.InProduction).IsRequired();
        builder.Property(t => t.OriginalName).HasMaxLength(300).IsRequired();
        builder.Property(t => t.Type).HasMaxLength(100);
        builder.Property(t => t.Status).HasMaxLength(100);
        builder.Property(t => t.CreatedBy).HasMaxLength(3000);
        builder.Property(t => t.EpisodeRunTime);
    }
}
