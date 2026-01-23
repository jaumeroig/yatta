namespace TimeTracker.Core.Services;

using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// Implementació del servei per a la lògica específica de jornada laboral.
/// </summary>
public class WorkdayService : IWorkdayService
{
    private readonly IWorkdaySlotRepository _workdaySlotRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly IValidationService _validationService;

    /// <summary>
    /// Constructor del servei de jornada laboral.
    /// </summary>
    /// <param name="workdaySlotRepository">Repositori de franges de jornada.</param>
    /// <param name="timeCalculatorService">Servei de càlcul de temps.</param>
    /// <param name="validationService">Servei de validació.</param>
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
        // Validar que la franja sigui vàlida (hora de fi posterior a hora d'inici)
        if (!_validationService.ValidateWorkdaySlot(slot, out var errorMessage))
        {
            return (false, errorMessage);
        }

        // Obtenir les franges existents del mateix dia
        var existingSlots = await _workdaySlotRepository.GetByDateAsync(slot.Date);

        // Validar que no hi hagi solapament
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
