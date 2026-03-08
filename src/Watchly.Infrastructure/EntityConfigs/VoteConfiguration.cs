using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id);
        builder.Property(v => v.Value).IsRequired();
        builder.Property(v => v.UpdatedAt).IsRequired();
        builder.Property(v => v.TitleId);
        builder.Property(v => v.EpisodeId);
        builder.Property(v => v.UserId).IsRequired();

        builder.HasOne(v => v.Title)
            .WithMany(t => t.Votes)
            .HasForeignKey(v => v.TitleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(v => v.Episode)
            .WithMany(e => e.Votes)
            .HasForeignKey(v => v.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
