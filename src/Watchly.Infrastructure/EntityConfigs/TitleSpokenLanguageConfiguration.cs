using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class TitleSpokenLanguageConfiguration : IEntityTypeConfiguration<TitleSpokenLanguage>
{
    public void Configure(EntityTypeBuilder<TitleSpokenLanguage> builder)
    {
        builder.HasKey(tg => tg.Id);

        builder.Property(tg => tg.Id);
        builder.Property(tg => tg.SpokenLanguageId);
        builder.Property(tg => tg.TitleId);

        builder.HasOne(tg => tg.Title)
            .WithMany(t => t.TitleSpokenLanguages)
            .HasForeignKey(tg => tg.TitleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(tg => tg.SpokenLanguage)
            .WithMany(sl => sl.TitleSpokenLanguages)
            .HasForeignKey(tg => tg.SpokenLanguageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
