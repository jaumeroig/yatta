namespace TimeTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the workday repository.
/// </summary>
public class WorkdayRepository : IWorkdayRepository
{
    private readonly TimeTrackerDbContext _context;

    public WorkdayRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Workday?> GetByDateAsync(DateOnly date)
    {
        return await _context.Workdays
            .FirstOrDefaultAsync(w => w.Date == date);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Workday>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.Workdays
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .OrderBy(w => w.Date)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Workday>> GetByDayTypeAsync(DateOnly startDate, DateOnly endDate, DayType dayType)
    {
        return await _context.Workdays
            .Where(w => w.Date >= startDate && w.Date <= endDate && w.DayType == dayType)
            .OrderBy(w => w.Date)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Dictionary<DayType, int>> GetDayTypeCountsAsync(DateOnly startDate, DateOnly endDate)
    {
        var counts = await _context.Workdays
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .GroupBy(w => w.DayType)
            .Select(g => new { DayType = g.Key, Count = g.Count() })
            .ToListAsync();

        return counts.ToDictionary(x => x.DayType, x => x.Count);
    }

    /// <inheritdoc/>
    public async Task<Workday> SaveAsync(Workday workday)
    {
        var existing = await _context.Workdays
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
            _context.Workdays.Add(workday);
        }

        await _context.SaveChangesAsync();

        return existing ?? workday;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(DateOnly date)
    {
        var workday = await _context.Workdays
            .FirstOrDefaultAsync(w => w.Date == date);

        if (workday != null)
        {
            _context.Workdays.Remove(workday);
            await _context.SaveChangesAsync();
        }
    }
}
