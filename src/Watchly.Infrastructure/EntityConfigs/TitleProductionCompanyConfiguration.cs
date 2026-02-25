using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class TitleProductionCompanyConfiguration : IEntityTypeConfiguration<TitleProductionCompany>
{
    public void Configure(EntityTypeBuilder<TitleProductionCompany> builder)
    {
        builder.HasKey(tpc => tpc.Id);

        builder.Property(tpc => tpc.Id);
        builder.Property(tpc => tpc.TitleId);
        builder.Property(tpc => tpc.ProductionCompanyId);

        builder.HasOne(tpc => tpc.Title)
            .WithMany(t => t.TitleProductionCompanies)
            .HasForeignKey(tpc => tpc.TitleId);

        builder.HasOne(tpc => tpc.ProductionCompany)
            .WithMany(pc => pc.TitleProductionCompanies)
            .HasForeignKey(tpc => tpc.ProductionCompanyId);
    }
}
