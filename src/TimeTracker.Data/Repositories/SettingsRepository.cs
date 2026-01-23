namespace TimeTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementació del repositori de configuració.
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly TimeTrackerDbContext _context;

    public SettingsRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<AppSettings> GetAsync()
    {
        // Obtenir la primera configuració o crear-ne una de nova amb valors per defecte
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new AppSettings
            {
                Theme = Theme.System,
                Notifications = false,
                WorkdayTotalTime = TimeSpan.FromHours(8)
            };
            
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task UpdateAsync(AppSettings settings)
    {
        var existingSettings = await _context.AppSettings.FirstOrDefaultAsync();
        
        if (existingSettings != null)
        {
            existingSettings.Theme = settings.Theme;
            existingSettings.Notifications = settings.Notifications;
            existingSettings.WorkdayTotalTime = settings.WorkdayTotalTime;
            
            _context.AppSettings.Update(existingSettings);
        }
        else
        {
            _context.AppSettings.Add(settings);
        }
        
        await _context.SaveChangesAsync();
    }
}
