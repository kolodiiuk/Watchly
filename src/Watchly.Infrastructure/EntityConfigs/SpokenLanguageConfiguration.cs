using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class SpokenLanguageConfiguration : IEntityTypeConfiguration<SpokenLanguage>
{
    public void Configure(EntityTypeBuilder<SpokenLanguage> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id);
        builder.Property(sl => sl.Name).HasMaxLength(100).IsRequired();

        builder.HasMany(sl => sl.TitleSpokenLanguages)
            .WithOne(tsl => tsl.SpokenLanguage)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sl => sl.Name).IsUnique();
    }
}
