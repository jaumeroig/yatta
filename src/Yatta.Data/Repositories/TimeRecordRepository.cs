namespace Yatta.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Implementation of the time records repository.
/// </summary>
public class TimeRecordRepository : ITimeRecordRepository
{
    private readonly YattaDbContext dbContext;

    public TimeRecordRepository(YattaDbContext context)
    {
        dbContext = context;
    }

    public async Task<IEnumerable<TimeRecord>> GetAllAsync()
    {
        return await dbContext.TimeRecords
            .AsNoTracking()
            .OrderByDescending(tr => tr.Date)
            .ThenByDescending(tr => tr.StartTime)
            .ToListAsync();
    }

    public async Task<TimeRecord?> GetByIdAsync(Guid id)
    {
        return await dbContext.TimeRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(tr => tr.Id == id);
    }

    public async Task<IEnumerable<TimeRecord>> GetByDateAsync(DateOnly date)
    {
        return await dbContext.TimeRecords
            .AsNoTracking()
            .Where(tr => tr.Date == date)
            .OrderBy(tr => tr.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeRecord>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await dbContext.TimeRecords
            .AsNoTracking()
            .Where(tr => tr.Date >= startDate && tr.Date <= endDate)
            .OrderBy(tr => tr.Date)
            .ThenBy(tr => tr.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeRecord>> GetByActivityIdAsync(Guid activityId)
    {
        return await dbContext.TimeRecords
            .AsNoTracking()
            .Where(tr => tr.ActivityId == activityId)
            .OrderByDescending(tr => tr.Date)
            .ThenByDescending(tr => tr.StartTime)
            .ToListAsync();
    }

    public async Task<TimeRecord?> GetActiveAsync()
    {
        return await dbContext.TimeRecords
            .AsNoTracking()
            .Where(tr => tr.EndTime == null)
            .OrderByDescending(tr => tr.Date)
            .ThenByDescending(tr => tr.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<TimeRecord> AddAsync(TimeRecord timeRecord)
    {
        dbContext.TimeRecords.Add(timeRecord);

        await dbContext.SaveChangesAsync();

        return timeRecord;
    }

    public async Task UpdateAsync(TimeRecord timeRecord)
    {
        // Load the existing tracked entity and update its properties to avoid
        // identity conflicts when a different instance with the same key
        // is provided (EF Core throws if two instances with same key are tracked).

        var existing = await dbContext.TimeRecords.FindAsync(timeRecord.Id) ??
            throw new InvalidOperationException($"TimeRecord with Id '{timeRecord.Id}' not found.");

        existing.ActivityId = timeRecord.ActivityId;
        existing.Date = timeRecord.Date;
        existing.StartTime = timeRecord.StartTime;
        existing.EndTime = timeRecord.EndTime;
        existing.Notes = timeRecord.Notes;
        existing.Telework = timeRecord.Telework;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var timeRecord = await GetByIdAsync(id);

        if (timeRecord is null)
            return;

        dbContext.TimeRecords.Remove(timeRecord);
        await dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CountBeforeDateAsync(DateOnly date)
    {
        return await dbContext.TimeRecords
            .Where(tr => tr.Date < date)
            .CountAsync();
    }

    /// <inheritdoc/>
    public async Task<int> DeleteBeforeDateAsync(DateOnly date)
    {
        return await dbContext.TimeRecords
            .Where(tr => tr.Date < date)
            .ExecuteDeleteAsync();
    }
}
