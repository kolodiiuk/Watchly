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
        builder.Property(v => v.Value);
        builder.Property(v => v.UpdatedAt);
        builder.Property(v => v.TitleId);

        builder.HasOne(v => v.Title)
            .WithMany(t => t.Votes)
            .HasForeignKey(v => v.TitleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
