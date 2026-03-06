namespace Yatta.Data;

using Microsoft.EntityFrameworkCore;
using Yatta.Core.Models;
using Yatta.Data.Configurations;

/// <summary>
/// Database context for the Time Tracker application.
/// </summary>
public class YattaDbContext : DbContext
{
    /// <summary>
    /// DbSet for application configuration.
    /// </summary>
    public DbSet<AppSettings> AppSettings { get; set; } = null!;

    /// <summary>
    /// DbSet for activities.
    /// </summary>
    public DbSet<Activity> Activities { get; set; } = null!;

    /// <summary>
    /// DbSet for day configurations.
    /// </summary>
    public DbSet<Workday> Workdays { get; set; } = null!;

    /// <summary>
    /// DbSet for time records.
    /// </summary>
    public DbSet<TimeRecord> TimeRecords { get; set; } = null!;


    public YattaDbContext(DbContextOptions<YattaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new AppSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityConfiguration());
        modelBuilder.ApplyConfiguration(new WorkdayEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TimeRecordConfiguration());
    }
}
