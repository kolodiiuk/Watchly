using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class TitleGenreConfiguration : IEntityTypeConfiguration<TitleGenre>
{
    public void Configure(EntityTypeBuilder<TitleGenre> builder)
    {
        builder.HasKey(tg => tg.Id);

        builder.Property(tg => tg.Id);
        builder.Property(tg => tg.TitleId);
        builder.Property(tg => tg.IsTvShow).IsRequired();
        builder.Property(tg => tg.GenreId);

        builder.HasOne(tg => tg.Title)
            .WithMany(t => t.TitleGenres)
            .HasForeignKey(tg => tg.TitleId);

        builder.HasOne(tg => tg.Genre)
            .WithMany(g => g.TitleGenres)
            .HasForeignKey(tg => tg.GenreId);
    }
}
