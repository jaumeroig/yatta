namespace TimeTracker.Data;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Models;
using TimeTracker.Data.Configurations;

/// <summary>
/// Context de base de dades per l'aplicació Time Tracker.
/// </summary>
public class TimeTrackerDbContext : DbContext
{
    /// <summary>
    /// DbSet per les activitats.
    /// </summary>
    public DbSet<Activity> Activities { get; set; } = null!;

    /// <summary>
    /// DbSet per els registres de temps.
    /// </summary>
    public DbSet<TimeRecord> TimeRecords { get; set; } = null!;

    /// <summary>
    /// DbSet per les franges de jornada.
    /// </summary>
    public DbSet<WorkdaySlot> WorkdaySlots { get; set; } = null!;

    /// <summary>
    /// DbSet per la configuració de l'aplicació.
    /// </summary>
    public DbSet<AppSettings> AppSettings { get; set; } = null!;

    public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar les configuracions de les entitats
        modelBuilder.ApplyConfiguration(new ActivityConfiguration());
        modelBuilder.ApplyConfiguration(new TimeRecordConfiguration());
        modelBuilder.ApplyConfiguration(new WorkdaySlotConfiguration());
        modelBuilder.ApplyConfiguration(new AppSettingsConfiguration());
    }
}
