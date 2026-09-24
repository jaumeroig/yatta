namespace Yatta.App.Services;

using Microsoft.Extensions.DependencyInjection;
using Yatta.Core.Helpers;
using Yatta.Core.Interfaces;
using Yatta.Core.Models;

/// <summary>
/// Coordinates time entry changes from the main view, tray, and quick action window.
/// Each operation owns a short EF scope.
/// </summary>
public sealed class TimeEntryService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly UiEventService _events;
    private readonly INotificationService _notifications;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>Creates the coordinator.</summary>
    public TimeEntryService(IServiceScopeFactory scopes, UiEventService events, INotificationService notifications)
    {
        _scopes = scopes;
        _events = events;
        _notifications = notifications;
    }

    /// <summary>Starts or changes the active activity.</summary>
    public async Task StartOrSwitchAsync(Guid activityId, bool telework, string? notes = null)
    {
        if (activityId == Guid.Empty)
        {
            throw new ArgumentException("Validation_ActivityRequired", nameof(activityId));
        }

        await _gate.WaitAsync();
        try
        {
            using IServiceScope scope = _scopes.CreateScope();
            IActivityRepository activities = scope.ServiceProvider.GetRequiredService<IActivityRepository>();
            ITimeRecordRepository records = scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>();
            Activity? activity = await activities.GetByIdAsync(activityId);
            if (activity is null || !activity.Active)
            {
                throw new ArgumentException("Validation_ActivityRequired", nameof(activityId));
            }

            DateTime now = DateTime.Now;
            TimeOnly nextStart = TimeOnly.FromDateTime(now);
            TimeRecord? active = await records.GetActiveAsync();
            if (active is not null)
            {
                active.EndTime = GetEndTime(active, now);
                if (active.Date == DateOnly.FromDateTime(now) && active.EndTime > nextStart)
                {
                    nextStart = active.EndTime.Value;
                }
                await records.UpdateAsync(active);
            }

            await records.AddAsync(new TimeRecord
            {
                Id = Guid.NewGuid(),
                ActivityId = activityId,
                Date = DateOnly.FromDateTime(now),
                StartTime = nextStart,
                Telework = telework,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
            });
            _notifications.ResetTimer();
            _events.PublishDataChanged();
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Stops the active entry if present.</summary>
    public async Task StopAsync()
    {
        await _gate.WaitAsync();
        try
        {
            using IServiceScope scope = _scopes.CreateScope();
            ITimeRecordRepository records = scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>();
            TimeRecord? active = await records.GetActiveAsync();
            if (active is null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            active.EndTime = GetEndTime(active, now);
            await records.UpdateAsync(active);
            _notifications.ResetTimer();
            _events.PublishDataChanged();
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Creates or updates a manually entered time record.</summary>
    public async Task SaveAsync(TimeRecord record)
    {
        if (record.ActivityId == Guid.Empty)
        {
            throw new ArgumentException("Validation_ActivityRequired", nameof(record));
        }
        if (record.EndTime is not null && record.EndTime <= record.StartTime)
        {
            throw new ArgumentException("Validation_EndTimeAfterStartTime", nameof(record));
        }
        if (!string.IsNullOrWhiteSpace(record.Link) && !TimeRecordLinkHelper.IsValid(record.Link))
        {
            throw new ArgumentException("Validation_InvalidRecordLink", nameof(record));
        }

        await _gate.WaitAsync();
        try
        {
            using IServiceScope scope = _scopes.CreateScope();
            ITimeRecordRepository records = scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>();
            IValidationService validation = scope.ServiceProvider.GetRequiredService<IValidationService>();
            IEnumerable<TimeRecord> sameDay = await records.GetByDateAsync(record.Date);
            if (!validation.ValidateNoOverlap(record, sameDay))
            {
                throw new ArgumentException("Validation_OverlappingRecord", nameof(record));
            }
            if (record.EndTime is not null && sameDay.Any(existing =>
                existing.Id != record.Id &&
                existing.EndTime is null &&
                record.EndTime > existing.StartTime))
            {
                throw new ArgumentException("Validation_OverlappingRecord", nameof(record));
            }
            if (record.EndTime is null)
            {
                TimeRecord? active = await records.GetActiveAsync();
                if (active is not null && active.Id != record.Id)
                {
                    throw new ArgumentException("Validation_OverlappingRecord", nameof(record));
                }
            }

            record.Link = string.IsNullOrWhiteSpace(record.Link) ? null : record.Link.Trim();
            record.Notes = string.IsNullOrWhiteSpace(record.Notes) ? null : record.Notes.Trim();
            if (record.Id == Guid.Empty)
            {
                record.Id = Guid.NewGuid();
                await records.AddAsync(record);
            }
            else
            {
                await records.UpdateAsync(record);
            }
            _notifications.ResetTimer();
            _events.PublishDataChanged();
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Deletes an existing time record.</summary>
    public async Task DeleteAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            using IServiceScope scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ITimeRecordRepository>().DeleteAsync(id);
            _notifications.ResetTimer();
            _events.PublishDataChanged();
        }
        finally
        {
            _gate.Release();
        }
    }

    private static TimeOnly GetEndTime(TimeRecord active, DateTime now)
    {
        TimeOnly end = active.Date < DateOnly.FromDateTime(now)
            ? TimeOnly.MaxValue
            : TimeOnly.FromDateTime(now);
        if (end > active.StartTime)
        {
            return end;
        }
        if (active.StartTime == TimeOnly.MaxValue)
        {
            throw new ArgumentException("Validation_EndTimeAfterStartTime", nameof(active));
        }
        return active.StartTime.Add(TimeSpan.FromTicks(1));
    }
}
