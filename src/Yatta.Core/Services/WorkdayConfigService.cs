namespace Yatta.Core.Services;

using Yatta.Core.Extensions;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Implementation of the workday configuration service.
/// </summary>
public class WorkdayConfigService : IWorkdayConfigService
{
    private readonly IWorkdayRepository _workdayRepository;
    private readonly ISettingsRepository _settingsRepository;

    /// <summary>
    /// Constructor for the workday configuration service.
    /// </summary>
    /// <param name="workdayRepository">Workday repository.</param>
    /// <param name="settingsRepository">Settings repository.</param>
    public WorkdayConfigService(
        IWorkdayRepository workdayRepository,
        ISettingsRepository settingsRepository)
    {
        _workdayRepository = workdayRepository;
        _settingsRepository = settingsRepository;
    }

    /// <inheritdoc/>
    public async Task<Workday> GetEffectiveConfigurationAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var workday = await _workdayRepository.GetByDateAsync(date);

        if (workday != null)
        {
            return workday;
        }

        // Return default configuration based on app settings
        var settings = await _settingsRepository.GetAsync();
        return new Workday
        {
            Id = Guid.Empty,
            Date = date,
            DayType = DayType.WorkDay,
            TargetDuration = settings.WorkdayTotalTime
        };
    }

    /// <inheritdoc/>
    public async Task<TimeSpan> GetTargetDurationAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var workday = await GetEffectiveConfigurationAsync(date, cancellationToken);

        // Non-working days have zero target duration
        if (!IsWorkingDayType(workday.DayType))
        {
            return TimeSpan.Zero;
        }

        return workday.TargetDuration;
    }

    /// <inheritdoc/>
    public async Task<DayType> GetDayTypeAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var workday = await GetEffectiveConfigurationAsync(date, cancellationToken);
        return workday.DayType;
    }

    /// <inheritdoc/>
    public async Task<bool> IsWorkingDayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var dayType = await GetDayTypeAsync(date, cancellationToken);
        return IsWorkingDayType(dayType);
    }

    /// <inheritdoc/>
    public async Task<Workday> SetConfigurationAsync(
        DateOnly date,
        DayType dayType,
        TimeSpan? targetDuration = null,
        CancellationToken cancellationToken = default)
    {
        TimeSpan effectiveTargetDuration;

        if (IsWorkingDayType(dayType))
        {
            if (targetDuration.HasValue)
            {
                effectiveTargetDuration = targetDuration.Value;
            }
            else
            {
                // Use default from settings for working days
                var settings = await _settingsRepository.GetAsync();
                effectiveTargetDuration = settings.WorkdayTotalTime;
            }
        }
        else
        {
            // Non-working days always have zero duration
            effectiveTargetDuration = TimeSpan.Zero;
        }

        var workday = await _workdayRepository.GetByDateAsync(date);

        if (workday == null)
        {
            workday = new Workday
            {
                Id = Guid.NewGuid(),
                Date = date,
                DayType = dayType,
                TargetDuration = effectiveTargetDuration
            };
        }
        else
        {
            workday.DayType = dayType;
            workday.TargetDuration = effectiveTargetDuration;
        }

        return await _workdayRepository.SaveAsync(workday);
    }

    /// <inheritdoc/>
    public async Task ResetConfigurationAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        await _workdayRepository.DeleteAsync(date);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<DayType, int>> GetDayTypeCountsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        return await _workdayRepository.GetDayTypeCountsAsync(startDate, endDate);
    }

    /// <inheritdoc/>
    public async Task<TimeSpan> GetRemainingWorkTimeAsync(
        DateOnly date,
        TimeSpan workedDuration,
        CancellationToken cancellationToken = default)
    {
        var targetDuration = await GetTargetDurationAsync(date, cancellationToken);

        // Non-working days or zero target means no remaining time
        if (targetDuration == TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        var remaining = targetDuration - workedDuration;

        // Don't return negative values
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Determines if a day type represents a working day using the <see cref="Attributes.WorkableDayAttribute"/>.
    /// </summary>
    /// <param name="dayType">The day type to check.</param>
    /// <returns>True if it's a working day type, false otherwise.</returns>
    private static bool IsWorkingDayType(DayType dayType)
    {
        return dayType.IsWorkable();
    }
}
