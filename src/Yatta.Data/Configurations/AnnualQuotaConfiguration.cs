namespace Yatta.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yatta.Core.Models;

/// <summary>
/// Entity configuration for AnnualQuota.
/// </summary>
public class AnnualQuotaConfiguration : IEntityTypeConfiguration<AnnualQuota>
{
    public void Configure(EntityTypeBuilder<AnnualQuota> builder)
    {
        builder.ToTable("AnnualQuotas");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Year)
            .IsRequired();

        builder.Property(q => q.VacationDays)
            .IsRequired();

        builder.Property(q => q.FreeChoiceDays)
            .IsRequired();

        builder.Property(q => q.IntensiveDays)
            .IsRequired();

        // Unique constraint on Year
        builder.HasIndex(q => q.Year)
            .IsUnique();
    }
}
