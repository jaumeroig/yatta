namespace TimeTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the workday repository.
/// </summary>
public class WorkdayRepository : IWorkdayRepository
{
    private readonly TimeTrackerDbContext dbContext;

    public WorkdayRepository(TimeTrackerDbContext context)
    {
        dbContext = context;
    }

    /// <inheritdoc/>
    public async Task<Workday?> GetByDateAsync(DateOnly date)
    {
        return await dbContext.Workdays
            .FirstOrDefaultAsync(w => w.Date == date);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Workday>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await dbContext.Workdays
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .OrderBy(w => w.Date)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Workday>> GetByDayTypeAsync(DateOnly startDate, DateOnly endDate, DayType dayType)
    {
        return await dbContext.Workdays
            .Where(w => w.Date >= startDate && w.Date <= endDate && w.DayType == dayType)
            .OrderBy(w => w.Date)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Dictionary<DayType, int>> GetDayTypeCountsAsync(DateOnly startDate, DateOnly endDate)
    {
        var counts = await dbContext.Workdays
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .GroupBy(w => w.DayType)
            .Select(g => new { DayType = g.Key, Count = g.Count() })
            .ToListAsync();

        return counts.ToDictionary(x => x.DayType, x => x.Count);
    }

    /// <inheritdoc/>
    public async Task<Workday> SaveAsync(Workday workday)
    {
        var existing = await dbContext.Workdays
            .FirstOrDefaultAsync(w => w.Date == workday.Date);

        if (existing != null)
        {
            existing.DayType = workday.DayType;
            existing.TargetDuration = workday.TargetDuration;
        }
        else
        {
            if (workday.Id == Guid.Empty)
            {
                workday.Id = Guid.NewGuid();
            }
            dbContext.Workdays.Add(workday);
        }

        await dbContext.SaveChangesAsync();

        return existing ?? workday;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(DateOnly date)
    {
        var workday = await dbContext.Workdays
            .FirstOrDefaultAsync(w => w.Date == date);

        if (workday != null)
        {
            dbContext.Workdays.Remove(workday);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<int> CountBeforeDateAsync(DateOnly date)
    {
        return await dbContext.Workdays
            .Where(w => w.Date < date)
            .CountAsync();
    }

    /// <inheritdoc/>
    public async Task<int> DeleteBeforeDateAsync(DateOnly date)
    {
        return await dbContext.Workdays
            .Where(w => w.Date < date)
            .ExecuteDeleteAsync();
    }
}
