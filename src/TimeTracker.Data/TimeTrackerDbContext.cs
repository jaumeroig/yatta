namespace TimeTracker.Data;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Models;
using TimeTracker.Data.Configurations;

/// <summary>
/// Database context for the Time Tracker application.
/// </summary>
public class TimeTrackerDbContext : DbContext
{
    /// <summary>
    /// DbSet for activities.
    /// </summary>
    public DbSet<Activity> Activities { get; set; } = null!;

    /// <summary>
    /// DbSet for time records.
    /// </summary>
    public DbSet<TimeRecord> TimeRecords { get; set; } = null!;

    /// <summary>
    /// DbSet for workday slots.
    /// </summary>
    public DbSet<WorkdaySlot> WorkdaySlots { get; set; } = null!;

    /// <summary>
    /// DbSet for application configuration.
    /// </summary>
    public DbSet<AppSettings> AppSettings { get; set; } = null!;

    public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new ActivityConfiguration());
        modelBuilder.ApplyConfiguration(new TimeRecordConfiguration());
        modelBuilder.ApplyConfiguration(new WorkdaySlotConfiguration());
        modelBuilder.ApplyConfiguration(new AppSettingsConfiguration());
    }
}
