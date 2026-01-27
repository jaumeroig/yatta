namespace TimeTracker.Core.Services;

using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementation of the service for workday-specific logic.
/// </summary>
public class WorkdayService : IWorkdayService
{
    private readonly IWorkdaySlotRepository _workdaySlotRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly IValidationService _validationService;

    /// <summary>
    /// Constructor of the workday service.
    /// </summary>
    /// <param name="workdaySlotRepository">Workday slots repository.</param>
    /// <param name="timeCalculatorService">Time calculation service.</param>
    /// <param name="validationService">Validation service.</param>
    public WorkdayService(
        IWorkdaySlotRepository workdaySlotRepository,
        ITimeCalculatorService timeCalculatorService,
        IValidationService validationService)
    {
        _workdaySlotRepository = workdaySlotRepository;
        _timeCalculatorService = timeCalculatorService;
        _validationService = validationService;
    }

    /// <inheritdoc/>
    public async Task<WorkdaySummary> GetDailySummaryAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var slots = await _workdaySlotRepository.GetByDateAsync(date);
        var slotsList = slots.ToList();

        var totalHours = _timeCalculatorService.CalculateTotalHours(slotsList);
        var teleworkHours = _timeCalculatorService.CalculateTeleworkHours(slotsList);
        var officeHours = _timeCalculatorService.CalculateOfficeHours(slotsList);
        var teleworkPercentage = _timeCalculatorService.CalculateTeleworkPercentage(slotsList);

        return new WorkdaySummary
        {
            Date = date,
            TotalHours = totalHours,
            TeleworkHours = teleworkHours,
            OfficeHours = officeHours,
            TeleworkPercentage = teleworkPercentage,
            SlotCount = slotsList.Count
        };
    }

    /// <inheritdoc/>
    public async Task<bool> CanAddWorkdaySlotAsync(WorkdaySlot slot, CancellationToken cancellationToken = default)
    {
        var (isValid, _) = await ValidateWorkdaySlotAsync(slot, cancellationToken);
        return isValid;
    }

    /// <inheritdoc/>
    public async Task<(bool IsValid, string ErrorMessage)> ValidateWorkdaySlotAsync(WorkdaySlot slot, CancellationToken cancellationToken = default)
    {
        // Validate that the slot is valid (end time after start time)
        if (!_validationService.ValidateWorkdaySlot(slot, out var errorMessage))
        {
            return (false, errorMessage);
        }

        // Get existing slots for the same day
        var existingSlots = await _workdaySlotRepository.GetByDateAsync(slot.Date);

        // Validate that there is no overlap
        if (!_validationService.ValidateNoOverlap(slot, existingSlots, out errorMessage))
        {
            return (false, errorMessage);
        }

        return (true, string.Empty);
    }

    /// <inheritdoc/>
    public async Task<double> GetTotalHoursAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var slots = await _workdaySlotRepository.GetByDateRangeAsync(startDate, endDate);
        return _timeCalculatorService.CalculateTotalHours(slots);
    }

    /// <inheritdoc/>
    public async Task<double> GetTeleworkPercentageAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var slots = await _workdaySlotRepository.GetByDateRangeAsync(startDate, endDate);
        return _timeCalculatorService.CalculateTeleworkPercentage(slots);
    }
}
