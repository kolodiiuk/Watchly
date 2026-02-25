using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> b)
    {
        b.HasKey(g => g.Id);

        b.Property(g => g.Id);
        b.Property(g => g.Name).HasMaxLength(200).IsRequired();

        b.HasMany(g => g.TitleGenres)
            .WithOne(tg => tg.Genre)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
