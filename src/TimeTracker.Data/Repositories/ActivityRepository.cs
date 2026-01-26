namespace TimeTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementació del repositori d'activitats.
/// </summary>
public class ActivityRepository : IActivityRepository
{
    private readonly TimeTrackerDbContext _context;

    public ActivityRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Activity>> GetAllAsync()
    {
        return await _context.Activities.ToListAsync();
    }

    public async Task<Activity?> GetByIdAsync(Guid id)
    {
        return await _context.Activities.FindAsync(id);
    }

    public async Task<IEnumerable<Activity>> GetActiveAsync()
    {
        return await _context.Activities
            .Where(a => a.Active)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Activity?> GetByNameAsync(string name)
    {
        return await _context.Activities
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());
    }

    public async Task<Activity> AddAsync(Activity activity)
    {
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task UpdateAsync(Activity activity)
    {
        var existingActivity = await _context.Activities.FindAsync(activity.Id);
        if (existingActivity != null)
        {
            existingActivity.Name = activity.Name;
            existingActivity.Color = activity.Color;
            existingActivity.Active = activity.Active;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var activity = await GetByIdAsync(id);
        if (activity != null)
        {
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
        }
    }
}
