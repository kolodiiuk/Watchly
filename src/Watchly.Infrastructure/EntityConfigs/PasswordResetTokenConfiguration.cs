using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id);
        builder.Property(t => t.Expires).IsRequired();
        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.TokenHash).HasMaxLength(200).IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(t => t.UserId);
    }
}
