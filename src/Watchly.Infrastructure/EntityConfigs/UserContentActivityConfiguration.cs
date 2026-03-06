using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

public class UserContentActivityConfiguration : IEntityTypeConfiguration<UserContentActivity>
{
    public void Configure(EntityTypeBuilder<UserContentActivity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id);
        builder.Property(a => a.ContentId).IsRequired();
        builder.Property(a => a.ContentType).IsRequired();
        builder.Property(a => a.ActivityType).IsRequired();
        builder.Property(a => a.UserId);
        builder.Property(a => a.WatchedAt).IsRequired();

        builder.HasOne(a => a.User)
            .WithMany(u => u.UserContentActivities)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
