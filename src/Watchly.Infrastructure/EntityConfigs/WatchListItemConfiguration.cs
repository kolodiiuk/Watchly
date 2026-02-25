using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class WatchListItemConfiguration : IEntityTypeConfiguration<WatchListItem>
{
    public void Configure(EntityTypeBuilder<WatchListItem> builder)
    {
        builder.HasKey(wi => wi.Id);

        builder.Property(wi => wi.Id);

        builder.HasOne(wi => wi.Title)
            .WithMany(t => t.WatchListItems)
            .HasForeignKey(wi => wi.TitleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(wi => wi.WatchList)
            .WithMany(wl => wl.WatchListItems)
            .HasForeignKey(wi => wi.WatchListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
