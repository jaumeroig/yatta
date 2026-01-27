namespace TimeTracker.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory to create the DbContext at design time (for migrations).
/// </summary>
public class TimeTrackerDbContextFactory : IDesignTimeDbContextFactory<TimeTrackerDbContext>
{
    public TimeTrackerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TimeTrackerDbContext>();
        optionsBuilder.UseSqlite(DatabaseConfiguration.GetConnectionString());

        return new TimeTrackerDbContext(optionsBuilder.Options);
    }
}
