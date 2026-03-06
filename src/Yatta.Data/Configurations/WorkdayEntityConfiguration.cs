namespace Yatta.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yatta.Core.Models;

/// <summary>
/// Configuration of the Workday entity for Entity Framework.
/// </summary>
public class WorkdayEntityConfiguration : IEntityTypeConfiguration<Workday>
{
    public void Configure(EntityTypeBuilder<Workday> builder)
    {
        builder.ToTable("Workdays");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Date)
            .IsRequired();

        builder.Property(w => w.DayType)
            .IsRequired();

        builder.Property(w => w.TargetDuration)
            .IsRequired();

        // Create unique index on Date to ensure only one configuration per date
        builder.HasIndex(w => w.Date)
            .IsUnique();
    }
}
