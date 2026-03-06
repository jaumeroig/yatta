namespace Yatta.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory to create the DbContext at design time (for migrations).
/// </summary>
public class YattaDbContextFactory : IDesignTimeDbContextFactory<YattaDbContext>
{
    public YattaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<YattaDbContext>();
        optionsBuilder.UseSqlite(DatabaseConfiguration.GetConnectionString());

        return new YattaDbContext(optionsBuilder.Options);
    }
}
