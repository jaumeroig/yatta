namespace TimeTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the workday slots repository.
/// </summary>
public class WorkdaySlotRepository : IWorkdaySlotRepository
{
    private readonly TimeTrackerDbContext _context;

    public WorkdaySlotRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkdaySlot>> GetAllAsync()
    {
        return await _context.WorkdaySlots
            .OrderByDescending(ws => ws.Date)
            .ThenBy(ws => ws.StartTime)
            .ToListAsync();
    }

    public async Task<WorkdaySlot?> GetByIdAsync(Guid id)
    {
        return await _context.WorkdaySlots.FindAsync(id);
    }

    public async Task<IEnumerable<WorkdaySlot>> GetByDateAsync(DateOnly date)
    {
        return await _context.WorkdaySlots
            .Where(ws => ws.Date == date)
            .OrderBy(ws => ws.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkdaySlot>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.WorkdaySlots
            .Where(ws => ws.Date >= startDate && ws.Date <= endDate)
            .OrderBy(ws => ws.Date)
            .ThenBy(ws => ws.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<DateOnly>> GetDatesWithSlotsAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.WorkdaySlots
            .Where(ws => ws.Date >= startDate && ws.Date <= endDate)
            .Select(ws => ws.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }

    public async Task<WorkdaySlot> AddAsync(WorkdaySlot workdaySlot)
    {
        _context.WorkdaySlots.Add(workdaySlot);
        await _context.SaveChangesAsync();
        return workdaySlot;
    }

    public async Task UpdateAsync(WorkdaySlot workdaySlot)
    {
        var existingSlot = await _context.WorkdaySlots.FindAsync(workdaySlot.Id);

        if (existingSlot is null)
            return;

        existingSlot.Date = workdaySlot.Date;
        existingSlot.StartTime = workdaySlot.StartTime;
        existingSlot.EndTime = workdaySlot.EndTime;
        existingSlot.Telework = workdaySlot.Telework;
        await _context.SaveChangesAsync();

        //_context.WorkdaySlots.Update(workdaySlot); --- Peta al editar jornada por change tracker EF
        //await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var workdaySlot = await GetByIdAsync(id);
        if (workdaySlot != null)
        {
            _context.WorkdaySlots.Remove(workdaySlot);
            await _context.SaveChangesAsync();
        }
    }
}
