namespace TimeTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the time records repository.
/// </summary>
public class TimeRecordRepository : ITimeRecordRepository
{
    private readonly TimeTrackerDbContext _context;

    public TimeRecordRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TimeRecord>> GetAllAsync()
    {
        return await _context.TimeRecords
            .OrderByDescending(tr => tr.Date)
            .ThenByDescending(tr => tr.StartTime)
            .ToListAsync();
    }

    public async Task<TimeRecord?> GetByIdAsync(Guid id)
    {
        return await _context.TimeRecords.FindAsync(id);
    }

    public async Task<IEnumerable<TimeRecord>> GetByDateAsync(DateOnly date)
    {
        return await _context.TimeRecords
            .Where(tr => tr.Date == date)
            .OrderBy(tr => tr.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeRecord>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.TimeRecords
            .Where(tr => tr.Date >= startDate && tr.Date <= endDate)
            .OrderBy(tr => tr.Date)
            .ThenBy(tr => tr.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeRecord>> GetByActivityIdAsync(Guid activityId)
    {
        return await _context.TimeRecords
            .Where(tr => tr.ActivityId == activityId)
            .OrderByDescending(tr => tr.Date)
            .ThenByDescending(tr => tr.StartTime)
            .ToListAsync();
    }

    public async Task<TimeRecord?> GetActiveAsync()
    {
        return await _context.TimeRecords
            .Where(tr => tr.EndTime == null)
            .OrderByDescending(tr => tr.Date)
            .ThenByDescending(tr => tr.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<TimeRecord> AddAsync(TimeRecord timeRecord)
    {
        _context.TimeRecords.Add(timeRecord);

        await _context.SaveChangesAsync();

        return timeRecord;
    }

    public async Task UpdateAsync(TimeRecord timeRecord)
    {
        // Load the existing tracked entity and update its properties to avoid
        // identity conflicts when a different instance with the same key
        // is provided (EF Core throws if two instances with same key are tracked).

        var existing = await _context.TimeRecords.FindAsync(timeRecord.Id) ?? 
            throw new InvalidOperationException($"TimeRecord with Id '{timeRecord.Id}' not found.");

        existing.ActivityId = timeRecord.ActivityId;
        existing.Date = timeRecord.Date;
        existing.StartTime = timeRecord.StartTime;
        existing.EndTime = timeRecord.EndTime;
        existing.Notes = timeRecord.Notes;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var timeRecord = await GetByIdAsync(id);

        if (timeRecord is null)
            return;

        _context.TimeRecords.Remove(timeRecord);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CountBeforeDateAsync(DateOnly date)
    {
        return await _context.TimeRecords
            .Where(tr => tr.Date < date)
            .CountAsync();
    }

    /// <inheritdoc/>
    public async Task<int> DeleteBeforeDateAsync(DateOnly date)
    {
        return await _context.TimeRecords
            .Where(tr => tr.Date < date)
            .ExecuteDeleteAsync();
    }
}
