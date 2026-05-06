using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class KeywordTitleConfiguration : IEntityTypeConfiguration<KeywordTitle>
{
    public void Configure(EntityTypeBuilder<KeywordTitle> builder)
    {
        builder.HasKey(kt => kt.Id);

        builder.Property(kt => kt.Id);
        builder.Property(kt => kt.KeywordId);
        builder.Property(kt => kt.TitleId);
        builder.Property(kt => kt.IsTvShow).IsRequired();

        builder.HasOne(tg => tg.Title)
            .WithMany(t => t.KeywordTitles)
            .HasForeignKey(tg => tg.TitleId);

        builder.HasOne(tg => tg.Keyword)
            .WithMany(k => k.KeywordTitles)
            .HasForeignKey(kt => kt.KeywordId);

        builder.HasIndex(kt => new { kt.KeywordId, kt.TitleId });
        builder.HasIndex(kt => new { kt.TitleId, kt.KeywordId });
    }
}
