namespace TimeTracker.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory per crear el DbContext en temps de disseny (per migracions).
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
