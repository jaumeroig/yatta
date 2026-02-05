namespace TimeTracker.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Core.Models;

/// <summary>
/// Configuration of the TimeRecord entity for Entity Framework.
/// </summary>
public class TimeRecordConfiguration : IEntityTypeConfiguration<TimeRecord>
{
    public void Configure(EntityTypeBuilder<TimeRecord> builder)
    {
        builder.ToTable("TimeRecords");

        builder.HasKey(tr => tr.Id);

        builder.Property(tr => tr.Date)
            .IsRequired();

        builder.Property(tr => tr.StartTime)
            .IsRequired();

        builder.Property(tr => tr.EndTime);

        builder.Property(tr => tr.ActivityId)
            .IsRequired();

        builder.Property(tr => tr.Notes)
            .HasMaxLength(500);

        builder.Property(tr => tr.Telework)
            .HasDefaultValue(false);

        builder.HasIndex(tr => new { tr.Date, tr.StartTime });
    }
}
