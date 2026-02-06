namespace TimeTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the configuration repository.
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
        // Obtenir la configuració amb Id = 1 o crear-ne una de nova
        var settings = await _context.AppSettings.FindAsync(1);
        
        if (settings == null)
        {
            settings = new AppSettings
            {
                Id = 1,
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
        // Assegurar que sempre utilitzem Id = 1
        settings.Id = 1;
        
        var existingSettings = await _context.AppSettings.FindAsync(1);
        
        if (existingSettings != null)
        {
            existingSettings.Theme = settings.Theme;
            existingSettings.Notifications = settings.Notifications;
            existingSettings.WorkdayTotalTime = settings.WorkdayTotalTime;
            existingSettings.Language = settings.Language;
            existingSettings.MinimizeToTray = settings.MinimizeToTray;
            existingSettings.NotificationIntervalMinutes = settings.NotificationIntervalMinutes;
            existingSettings.StartWithWindows = settings.StartWithWindows;
            existingSettings.RetentionPolicy = settings.RetentionPolicy;
            existingSettings.CustomRetentionDays = settings.CustomRetentionDays;

            _context.AppSettings.Update(existingSettings);
        }
        else
        {
            _context.AppSettings.Add(settings);
        }
        
        await _context.SaveChangesAsync();
    }
}
