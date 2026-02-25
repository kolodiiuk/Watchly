using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watchly.Domain.Entities;

namespace Watchly.Infrastructure.EntityConfigs;

internal sealed class ProductionCompanyConfiguration : IEntityTypeConfiguration<ProductionCompany>
{
    public void Configure(EntityTypeBuilder<ProductionCompany> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Id);
        builder.Property(pc => pc.Name).HasMaxLength(400).IsRequired();

        builder.HasMany(pc => pc.TitleProductionCompanies)
            .WithOne(t => t.ProductionCompany)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
