using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.EfaConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EfaConfigs;
internal sealed class EfaConfigurationConfiguration : IEntityTypeConfiguration<EfaConfiguration>
{
    public void Configure(EntityTypeBuilder<EfaConfiguration> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Year)
            .IsRequired();

        builder.Property(e => e.EfaRate)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedBy)
            .IsRequired();

        // Create unique index on Year
        builder.HasIndex(e => e.Year)
            .IsUnique()
            .HasDatabaseName("IX_EfaConfigurations_Year");

        builder.ToTable("efa_configurations");
    }
}
