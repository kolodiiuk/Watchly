using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class KeywordConfiguration : IEntityTypeConfiguration<Keyword>
{
    public void Configure(EntityTypeBuilder<Keyword> b)
    {
        b.HasKey(k => k.Id);

        b.Property(k => k.Id);
        b.Property(k => k.Name).HasMaxLength(300).IsRequired();

        b.HasMany(k => k.KeywordTitles)
            .WithOne(kt => kt.Keyword)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(k => k.Name)
            .HasOperators("varchar_pattern_ops");
        b.HasIndex(k => k.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
