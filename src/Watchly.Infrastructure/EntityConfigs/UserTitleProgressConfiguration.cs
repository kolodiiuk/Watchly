using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

public class UserTitleProgressConfiguration : IEntityTypeConfiguration<UserTitleProgress>
{
    public void Configure(EntityTypeBuilder<UserTitleProgress> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id);
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.TitleId).IsRequired();
        builder.Property(p => p.UserId).IsRequired();

        builder.HasOne(p => p.User)
            .WithMany(u => u.UserTitleProgresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(p => p.Title)
            .WithMany()
            .HasForeignKey(p => p.TitleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
