using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id);
        builder.Property(rt => rt.Token).IsRequired();
        builder.Property(rt => rt.Expires)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(rt => rt.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(100).IsRequired();
        builder.Property(rt => rt.Revoked);
        builder.Property(rt => rt.RevokedByIp);
        builder.Property(rt => rt.ReplacedByToken);
        builder.Property(rt => rt.UserId).IsRequired();

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);

        builder.HasIndex(rt => rt.Token)
            .IsUnique();
    }
}
