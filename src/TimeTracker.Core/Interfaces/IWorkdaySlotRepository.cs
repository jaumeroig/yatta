namespace TimeTracker.Core.Interfaces;

using TimeTracker.Core.Models;

/// <summary>
/// Repository to manage workday slots.
/// </summary>
public interface IWorkdaySlotRepository
{
    /// <summary>
    /// Gets all workday slots.
    /// </summary>
    Task<IEnumerable<WorkdaySlot>> GetAllAsync();

    /// <summary>
    /// Gets a workday slot by identifier.
    /// </summary>
    Task<WorkdaySlot?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets workday slots for a specific date.
    /// </summary>
    Task<IEnumerable<WorkdaySlot>> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Gets workday slots for a date range.
    /// </summary>
    Task<IEnumerable<WorkdaySlot>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Gets dates that have workday slots within a range.
    /// </summary>
    Task<IEnumerable<DateOnly>> GetDatesWithSlotsAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Adds a new workday slot.
    /// </summary>
    Task<WorkdaySlot> AddAsync(WorkdaySlot workdaySlot);

    /// <summary>
    /// Updates an existing workday slot.
    /// </summary>
    Task UpdateAsync(WorkdaySlot workdaySlot);

    /// <summary>
    /// Deletes a workday slot.
    /// </summary>
    Task DeleteAsync(Guid id);
}
