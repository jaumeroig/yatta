namespace Yatta.Data.Repositories;

using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Implementation of the configuration repository.
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly YattaDbContext dbContext;

    public SettingsRepository(YattaDbContext context)
    {
        dbContext = context;
    }

    public async Task<AppSettings> GetAsync()
    {
        // Obtenir la configuració amb Id = 1 o crear-ne una de nova
        var settings = await dbContext.AppSettings.FindAsync(1);
        
        if (settings == null)
        {
            settings = new AppSettings
            {
                Id = 1,
                Theme = Theme.System,
                Notifications = false,
                WorkdayTotalTime = TimeSpan.FromHours(8)
            };
            
            dbContext.AppSettings.Add(settings);
            await dbContext.SaveChangesAsync();
        }

        return settings;
    }

    public async Task UpdateAsync(AppSettings settings)
    {
        // Assegurar que sempre utilitzem Id = 1
        settings.Id = 1;
        
        var existingSettings = await dbContext.AppSettings.FindAsync(1);
        
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
            existingSettings.GlobalHotkey = settings.GlobalHotkey;

            dbContext.AppSettings.Update(existingSettings);
        }
        else
        {
            dbContext.AppSettings.Add(settings);
        }
        
        await dbContext.SaveChangesAsync();
    }
}
