using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.WatchLists)
            .WithOne(wl => wl.User)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
