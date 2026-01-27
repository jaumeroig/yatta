namespace TimeTracker.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Core.Models;

/// <summary>
/// Configuration of the WorkdaySlot entity for Entity Framework.
/// </summary>
public class WorkdaySlotConfiguration : IEntityTypeConfiguration<WorkdaySlot>
{
    public void Configure(EntityTypeBuilder<WorkdaySlot> builder)
    {
        builder.ToTable("WorkdaySlots");

        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.Date)
            .IsRequired();

        builder.Property(ws => ws.StartTime)
            .IsRequired();

        builder.Property(ws => ws.EndTime)
            .IsRequired();

        builder.Property(ws => ws.Telework)
            .IsRequired();

        builder.HasIndex(ws => ws.Date);
    }
}
