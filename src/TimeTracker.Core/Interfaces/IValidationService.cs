namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Service for validation of records and work slots.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates that the end time is after the start time.
    /// </summary>
    /// <param name="startTime">Start time.</param>
    /// <param name="endTime">End time.</param>
    /// <returns>True if valid, false otherwise.</returns>
    bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime);

    /// <summary>
    /// Validates that the end time is after the start time with error message.
    /// </summary>
    /// <param name="startTime">Start time.</param>
    /// <param name="endTime">End time.</param>
    /// <param name="errorMessage">Error message if not valid.</param>
    /// <returns>True if valid, false otherwise.</returns>
    bool ValidateTimeRange(TimeOnly startTime, TimeOnly endTime, out string errorMessage);

    /// <summary>
    /// Validates that a time record does not overlap with other records on the same day.
    /// </summary>
    /// <param name="record">Record to validate.</param>
    /// <param name="existingRecords">Existing records on the same day.</param>
    /// <returns>True if there is no overlap, false otherwise.</returns>
    bool ValidateNoOverlap(TimeRecord record, IEnumerable<TimeRecord> existingRecords);

    /// <summary>
    /// Validates that a time record does not overlap with other records on the same day with error message.
    /// </summary>
    /// <param name="record">Record to validate.</param>
    /// <param name="existingRecords">Existing records on the same day.</param>
    /// <param name="errorMessage">Error message if there is overlap.</param>
    /// <returns>True if there is no overlap, false otherwise.</returns>
    bool ValidateNoOverlap(TimeRecord record, IEnumerable<TimeRecord> existingRecords, out string errorMessage);

    /// <summary>
    /// Validates that a work slot does not overlap with other slots on the same day.
    /// </summary>
    /// <param name="slot">Slot to validate.</param>
    /// <param name="existingSlots">Existing slots on the same day.</param>
    /// <returns>True if there is no overlap, false otherwise.</returns>
    bool ValidateNoOverlap(WorkdaySlot slot, IEnumerable<WorkdaySlot> existingSlots);

    /// <summary>
    /// Validates that a work slot does not overlap with other slots on the same day with error message.
    /// </summary>
    /// <param name="slot">Slot to validate.</param>
    /// <param name="existingSlots">Existing slots on the same day.</param>
    /// <param name="errorMessage">Error message if there is overlap.</param>
    /// <returns>True if there is no overlap, false otherwise.</returns>
    bool ValidateNoOverlap(WorkdaySlot slot, IEnumerable<WorkdaySlot> existingSlots, out string errorMessage);

    /// <summary>
    /// Validates if a time record is valid (end time after start time).
    /// </summary>
    /// <param name="record">Record to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    bool ValidateTimeRecord(TimeRecord record);

    /// <summary>
    /// Validates if a time record is valid with error message.
    /// </summary>
    /// <param name="record">Record to validate.</param>
    /// <param name="errorMessage">Error message if not valid.</param>
    /// <returns>True if valid, false otherwise.</returns>
    bool ValidateTimeRecord(TimeRecord record, out string errorMessage);

    /// <summary>
    /// Validates if a work slot is valid (end time after start time).
    /// </summary>
    /// <param name="slot">Slot to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    bool ValidateWorkdaySlot(WorkdaySlot slot);

    /// <summary>
    /// Validates if a work slot is valid with error message.
    /// </summary>
    /// <param name="slot">Slot to validate.</param>
    /// <param name="errorMessage">Error message if not valid.</param>
    /// <returns>True if valid, false otherwise.</returns>
    bool ValidateWorkdaySlot(WorkdaySlot slot, out string errorMessage);
}
