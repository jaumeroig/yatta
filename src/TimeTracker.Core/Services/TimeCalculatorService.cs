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
    public double CalculateTeleworkPercentage(IEnumerable<TimeRecord> records)
    {
        var totalHours = CalculateTotalHours(records);

        if (totalHours == 0)
        {
            return 0;
        }

        var teleworkHours = CalculateTeleworkHours(records);
        return (teleworkHours / totalHours) * 100;
    }

    /// <inheritdoc/>
    public double CalculateTeleworkHours(IEnumerable<TimeRecord> records)
    {
        double teleworkHours = 0;

        foreach (var record in records.Where(r => r.Telework && r.EndTime.HasValue))
        {
            teleworkHours += CalculateDuration(record.StartTime, record.EndTime!.Value);
        }

        return teleworkHours;
    }

    /// <inheritdoc/>
    public double CalculateOfficeHours(IEnumerable<TimeRecord> records)
    {
        double officeHours = 0;

        foreach (var record in records.Where(r => !r.Telework && r.EndTime.HasValue))
        {
            officeHours += CalculateDuration(record.StartTime, record.EndTime!.Value);
        }

        return officeHours;
    }
}
