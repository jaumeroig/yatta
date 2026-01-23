namespace TimeTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementació del repositori de registres de temps.
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

    public async Task<TimeRecord> AddAsync(TimeRecord timeRecord)
    {
        _context.TimeRecords.Add(timeRecord);
        await _context.SaveChangesAsync();
        return timeRecord;
    }

    public async Task UpdateAsync(TimeRecord timeRecord)
    {
        _context.TimeRecords.Update(timeRecord);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var timeRecord = await GetByIdAsync(id);
        if (timeRecord != null)
        {
            _context.TimeRecords.Remove(timeRecord);
            await _context.SaveChangesAsync();
        }
    }
}
