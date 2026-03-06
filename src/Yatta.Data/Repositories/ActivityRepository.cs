namespace Yatta.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Implementation of the activities repository.
/// </summary>
public class ActivityRepository : IActivityRepository
{
    private readonly YattaDbContext dbContext;

    public ActivityRepository(YattaDbContext context)
    {
        dbContext = context;
    }

    public async Task<IEnumerable<Activity>> GetAllAsync()
    {
        return await dbContext.Activities.ToListAsync();
    }

    public async Task<Activity?> GetByIdAsync(Guid id)
    {
        return await dbContext.Activities.FindAsync(id);
    }

    public async Task<IEnumerable<Activity>> GetActiveAsync()
    {
        return await dbContext.Activities
            .Where(a => a.Active)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Activity?> GetByNameAsync(string name)
    {
        return await dbContext.Activities
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());
    }

    public async Task<Activity> AddAsync(Activity activity)
    {
        dbContext.Activities.Add(activity);
        await dbContext.SaveChangesAsync();
        return activity;
    }

    public async Task UpdateAsync(Activity activity)
    {
        var existingActivity = await dbContext.Activities.FindAsync(activity.Id);
        if (existingActivity != null)
        {
            existingActivity.Name = activity.Name;
            existingActivity.Color = activity.Color;
            existingActivity.Active = activity.Active;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var activity = await GetByIdAsync(id);
        if (activity != null)
        {
            dbContext.Activities.Remove(activity);
            await dbContext.SaveChangesAsync();
        }
    }
}
