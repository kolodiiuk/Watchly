using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).IsRequired();
        builder.Property(c => c.TitleId);
        builder.Property(c => c.EpisodeId);
        builder.Property(c => c.UserId);
        builder.Property(c => c.UpdatedAt).IsRequired();
        builder.Property(c => c.Text).HasMaxLength(5000).IsRequired();
        builder.Property(c => c.IsDeleted).IsRequired();

        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(c => c.Title)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TitleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Episode)
            .WithMany(e => e.Comments)
            .HasForeignKey(c => c.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

