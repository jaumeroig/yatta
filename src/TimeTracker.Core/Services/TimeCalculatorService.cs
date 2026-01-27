namespace TimeTracker.Core.Services;

using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the service for calculating durations, daily totals, and percentages.
/// </summary>
public class TimeCalculatorService : ITimeCalculatorService
{
    /// <inheritdoc/>
    public double CalculateDuration(TimeOnly startTime, TimeOnly endTime)
    {
        // Calculate the difference in hours
        var duration = endTime - startTime;
        return duration.TotalHours;
    }

    /// <inheritdoc/>
    public double CalculateTotalHours(IEnumerable<TimeRecord> records)
    {
        double totalHours = 0;

        foreach (var record in records)
        {
            // Only count records with defined end time
            if (record.EndTime.HasValue)
            {
                totalHours += CalculateDuration(record.StartTime, record.EndTime.Value);
            }
        }

        return totalHours;
    }

    /// <inheritdoc/>
    public double CalculateTotalHours(IEnumerable<WorkdaySlot> slots)
    {
        double totalHours = 0;

        foreach (var slot in slots)
        {
            totalHours += CalculateDuration(slot.StartTime, slot.EndTime);
        }

        return totalHours;
    }

    /// <inheritdoc/>
    public double CalculateTeleworkPercentage(IEnumerable<WorkdaySlot> slots)
    {
        var totalHours = CalculateTotalHours(slots);
        
        if (totalHours == 0)
        {
            return 0;
        }

        var teleworkHours = CalculateTeleworkHours(slots);
        return (teleworkHours / totalHours) * 100;
    }

    /// <inheritdoc/>
    public double CalculateTeleworkHours(IEnumerable<WorkdaySlot> slots)
    {
        double teleworkHours = 0;

        foreach (var slot in slots.Where(s => s.Telework))
        {
            teleworkHours += CalculateDuration(slot.StartTime, slot.EndTime);
        }

        return teleworkHours;
    }

    /// <inheritdoc/>
    public double CalculateOfficeHours(IEnumerable<WorkdaySlot> slots)
    {
        double officeHours = 0;

        foreach (var slot in slots.Where(s => !s.Telework))
        {
            officeHours += CalculateDuration(slot.StartTime, slot.EndTime);
        }

        return officeHours;
    }
}
