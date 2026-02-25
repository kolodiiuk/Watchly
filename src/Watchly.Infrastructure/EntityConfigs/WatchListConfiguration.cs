using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class WatchListConfiguration : IEntityTypeConfiguration<WatchList>
{
    public void Configure(EntityTypeBuilder<WatchList> builder)
    {
        builder.HasKey(wl => wl.Id);

        builder.Property(wl => wl.Id);
        builder.Property(wl => wl.UserId).IsRequired();
        builder.Property(wl => wl.Name).HasMaxLength(300).IsRequired();

        builder.HasOne(wl => wl.User)
            .WithMany(u => u.WatchLists)
            .HasForeignKey(wl => wl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(wl => wl.WatchListItems)
            .WithOne(wi => wi.WatchList)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
