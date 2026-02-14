namespace TimeTracker.Core.Services;

using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the service for validating time records.
/// Note: Error messages should be translated through the localization service.
/// </summary>
public class ValidationService : IValidationService
{
    /// <inheritdoc/>
    public bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        return endTime > startTime;
    }

    /// <inheritdoc/>
    public bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime, out string errorMessage)
    {
        if (endTime > startTime)
        {
            errorMessage = string.Empty;
            return true;
        }

        // Resource key: Validation_EndTimeAfterStartTime
        errorMessage = "Validation_EndTimeAfterStartTime";
        return false;
    }

    /// <inheritdoc/>
    public bool ValidateNoOverlap(TimeRecord record, IEnumerable<TimeRecord> existingRecords)
    {
        return ValidateNoOverlap(record, existingRecords, out _);
    }

    /// <inheritdoc/>
    public bool ValidateNoOverlap(TimeRecord record, IEnumerable<TimeRecord> existingRecords, out string errorMessage)
    {
        errorMessage = string.Empty;

        // If the record has no end time, overlap cannot be validated
        if (!record.EndTime.HasValue)
        {
            return true;
        }

        foreach (var existing in existingRecords)
        {
            // Do not validate against the same record
            if (existing.Id == record.Id)
            {
                continue;
            }

            // If the existing record has no end time, overlap cannot be validated
            if (!existing.EndTime.HasValue)
            {
                continue;
            }

            // Check if there is overlap
            // Overlap if:
            // - The start of the new record is between the start and end of an existing one
            // - The end of the new record is between the start and end of an existing one
            // - The new record completely surrounds an existing one
            if ((record.StartTime >= existing.StartTime && record.StartTime < existing.EndTime.Value) ||
                (record.EndTime.Value > existing.StartTime && record.EndTime.Value <= existing.EndTime.Value) ||
                (record.StartTime <= existing.StartTime && record.EndTime.Value >= existing.EndTime.Value))
            {
                // Resource key: Validation_OverlappingRecord (with format of 2 arguments: startTime, endTime)
                errorMessage = $"Validation_OverlappingRecord|{existing.StartTime:HH\\:mm}|{existing.EndTime.Value:HH\\:mm}";
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public bool ValidateTimeRecord(TimeRecord record)
    {
        return ValidateTimeRecord(record, out _);
    }

    /// <inheritdoc/>
    public bool ValidateTimeRecord(TimeRecord record, out string errorMessage)
    {
        errorMessage = string.Empty;

        // If there is no end time, the record is valid (may be in progress)
        if (!record.EndTime.HasValue)
        {
            return true;
        }

        // Validate that end time is after start time
        return ValidateTimeRange(record.StartTime, record.EndTime.Value, out errorMessage);
    }
}
