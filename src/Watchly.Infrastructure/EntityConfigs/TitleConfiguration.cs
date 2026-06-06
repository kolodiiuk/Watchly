using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;
using Watchly.Domain.Enums;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class TitleConfiguration : IEntityTypeConfiguration<Title>
{
    public void Configure(EntityTypeBuilder<Title> b)
    {
        b.HasKey(t => t.Id);

        b.Property(t => t.ReleaseDate);
        b.Property(t => t.UpdatedAt).IsRequired();
        b.Property(t => t.Runtime).IsRequired();
        b.Property(t => t.Name).HasMaxLength(300).IsRequired();
        b.Property(t => t.AvgTmdbRating);
        b.Property(t => t.HomePage).HasMaxLength(500);
        b.Property(t => t.Overview).HasMaxLength(5000).IsRequired();
        b.Property(t => t.PosterUrl).HasMaxLength(500);
        b.Property(t => t.Actors).HasMaxLength(3000);
        b.Property(t => t.Director).HasMaxLength(200);
        b.Property(t => t.LocalizationLanguages).HasMaxLength(1500);
        b.Property(t => t.IsAdult).IsRequired();
        b.Property(t => t.IsDeleted).IsRequired().HasDefaultValue(false);
        b.Property(t => t.Tagline).HasMaxLength(500);
        b.Property(t => t.ContentType).IsRequired();

        b.HasDiscriminator(t => t.ContentType)
            .HasValue<Title>(TitleType.Movie)
            .HasValue<TvShow>(TitleType.Series);

        b.HasMany(t => t.Votes)
            .WithOne(v => v.Title)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(t => t.TitleProductionCompanies)
            .WithOne(tpc => tpc.Title)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(t => t.TitleGenres)
            .WithOne(tg => tg.Title)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(t => t.Comments)
            .WithOne(c => c.Title)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(t => t.KeywordTitles)
            .WithOne(kt => kt.Title)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(t => t.Seasons)
            .WithOne(s => s.Title)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(t => t.TitleSpokenLanguages)
            .WithOne(tsl => tsl.Title)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(t => t.WatchListItems)
            .WithOne(wi => wi.Title)
            .OnDelete(DeleteBehavior.Cascade);

//        b.HasIndex(title => title.Runtime);
    }
}
