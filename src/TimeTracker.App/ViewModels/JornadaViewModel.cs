using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

namespace TimeTracker.App.ViewModels;

/// <summary>
/// ViewModel per a la gestió de la jornada laboral.
/// </summary>
public partial class JornadaViewModel : ObservableObject
{
    private readonly IWorkdaySlotRepository _workdaySlotRepository;
    private readonly IWorkdayService _workdayService;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private List<WorkdaySlot> _allSlots = [];

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private string _selectedDateDisplay = string.Empty;

    [ObservableProperty]
    private ObservableCollection<WorkdaySlotDisplay> _slots = [];

    [ObservableProperty]
    private string _totalWorkedTime = "0h 0m";

    [ObservableProperty]
    private string _teleworkPercentage = "0%";

    [ObservableProperty]
    private string _officeTime = "0h 0m";

    [ObservableProperty]
    private string _teleworkTime = "0h 0m";

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private WorkdaySlotEditModel _editingSlot = new();

    [ObservableProperty]
    private string _monthYear = string.Empty;

    [ObservableProperty]
    private string _monthTotalTime = "0h 0m";

    [ObservableProperty]
    private string _monthTeleworkPercentage = "0%";

    // Propietats pel gràfic de barres
    [ObservableProperty]
    private double _officeHoursValue;

    [ObservableProperty]
    private double _teleworkHoursValue;

    [ObservableProperty]
    private double _officeBarWidth;

    [ObservableProperty]
    private double _teleworkBarWidth;

    // Dates amb registres (pel calendari)
    [ObservableProperty]
    private ObservableCollection<DateTime> _datesWithRecords = [];

    public JornadaViewModel(
        IWorkdaySlotRepository workdaySlotRepository,
        IWorkdayService workdayService,
        ITimeCalculatorService timeCalculatorService)
    {
        _workdaySlotRepository = workdaySlotRepository;
        _workdayService = workdayService;
        _timeCalculatorService = timeCalculatorService;
        
        UpdateDateDisplay();
        UpdateMonthYearDisplay();
    }

    /// <summary>
    /// Carrega les dades inicials.
    /// </summary>
    public async Task LoadDataAsync()
    {
        await LoadSlotsForDateAsync(SelectedDate);
        await UpdateMonthlySummaryAsync();
        await LoadDatesWithRecordsAsync();
    }

    /// <summary>
    /// Carrega les dates del mes que tenen registres.
    /// </summary>
    private async Task LoadDatesWithRecordsAsync()
    {
        var firstDay = new DateOnly(SelectedDate.Year, SelectedDate.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var dates = await _workdaySlotRepository.GetDatesWithSlotsAsync(firstDay, lastDay);
        DatesWithRecords = new ObservableCollection<DateTime>(dates.Select(d => d.ToDateTime(TimeOnly.MinValue)));
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        UpdateDateDisplay();
        _ = LoadSlotsForDateAsync(value);
    }

    private async Task LoadSlotsForDateAsync(DateTime date)
    {
        var dateOnly = DateOnly.FromDateTime(date);
        _allSlots = (await _workdaySlotRepository.GetByDateAsync(dateOnly)).ToList();
        UpdateSlotsDisplay();
        UpdateDailySummary();
    }

    private void UpdateSlotsDisplay()
    {
        var slotDisplays = _allSlots
            .OrderBy(s => s.StartTime)
            .Select(slot => new WorkdaySlotDisplay
            {
                Id = slot.Id,
                StartTime = slot.StartTime.ToString("HH:mm"),
                EndTime = slot.EndTime.ToString("HH:mm"),
                Duration = FormatDuration(_timeCalculatorService.CalculateDuration(slot.StartTime, slot.EndTime)),
                LocationText = slot.Telework ? "Casa (teletreball)" : "Oficina",
                LocationIcon = slot.Telework ? "Home24" : "Building24",
                Telework = slot.Telework
            });

        Slots = new ObservableCollection<WorkdaySlotDisplay>(slotDisplays);
    }

    private void UpdateDailySummary()
    {
        var totalHours = _timeCalculatorService.CalculateTotalHours(_allSlots);
        var teleworkHours = _timeCalculatorService.CalculateTeleworkHours(_allSlots);
        var officeHours = _timeCalculatorService.CalculateOfficeHours(_allSlots);
        var percentage = _timeCalculatorService.CalculateTeleworkPercentage(_allSlots);

        TotalWorkedTime = FormatDuration(totalHours);
        TeleworkTime = FormatDuration(teleworkHours);
        OfficeTime = FormatDuration(officeHours);
        TeleworkPercentage = $"{percentage:F0}%";

        // Actualitzar valors pel gràfic de barres
        OfficeHoursValue = officeHours;
        TeleworkHoursValue = teleworkHours;
        UpdateBarWidths();
    }

    private void UpdateBarWidths()
    {
        const double maxBarWidth = 200.0;
        var maxHours = Math.Max(OfficeHoursValue, TeleworkHoursValue);
        
        if (maxHours > 0)
        {
            OfficeBarWidth = (OfficeHoursValue / maxHours) * maxBarWidth;
            TeleworkBarWidth = (TeleworkHoursValue / maxHours) * maxBarWidth;
        }
        else
        {
            OfficeBarWidth = 0;
            TeleworkBarWidth = 0;
        }
    }

    private async Task UpdateMonthlySummaryAsync()
    {
        var firstDay = new DateOnly(SelectedDate.Year, SelectedDate.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var totalHours = await _workdayService.GetTotalHoursAsync(firstDay, lastDay);
        var percentage = await _workdayService.GetTeleworkPercentageAsync(firstDay, lastDay);

        MonthTotalTime = FormatDuration(totalHours);
        MonthTeleworkPercentage = $"{percentage:F0}%";
    }

    private void UpdateDateDisplay()
    {
        var culture = new CultureInfo("ca-ES");
        var dayName = culture.TextInfo.ToTitleCase(SelectedDate.ToString("dddd", culture));
        var day = SelectedDate.Day;
        var month = culture.TextInfo.ToTitleCase(SelectedDate.ToString("MMMM", culture));
        var year = SelectedDate.Year;
        SelectedDateDisplay = $"{dayName}, {day} de {month} de {year}";
    }

    private void UpdateMonthYearDisplay()
    {
        var culture = new CultureInfo("ca-ES");
        var month = culture.TextInfo.ToTitleCase(SelectedDate.ToString("MMMM", culture));
        MonthYear = $"{month} {SelectedDate.Year}";
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        return $"{h}h {m}m";
    }

    [RelayCommand]
    private void PreviousDay()
    {
        SelectedDate = SelectedDate.AddDays(-1);
    }

    [RelayCommand]
    private void NextDay()
    {
        SelectedDate = SelectedDate.AddDays(1);
    }

    [RelayCommand]
    private void Today()
    {
        SelectedDate = DateTime.Today;
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        SelectedDate = SelectedDate.AddMonths(-1);
        UpdateMonthYearDisplay();
        _ = UpdateMonthlySummaryAsync();
    }

    [RelayCommand]
    private void NextMonth()
    {
        SelectedDate = SelectedDate.AddMonths(1);
        UpdateMonthYearDisplay();
        _ = UpdateMonthlySummaryAsync();
    }

    [RelayCommand]
    private void OpenNewSlotDialog()
    {
        IsEditing = false;
        EditingSlot = new WorkdaySlotEditModel
        {
            Date = SelectedDate,
            StartTimeText = "09:00",
            EndTimeText = "14:00",
            Telework = false,
            ValidationError = string.Empty
        };
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditSlotDialog(WorkdaySlotDisplay slot)
    {
        var originalSlot = _allSlots.FirstOrDefault(s => s.Id == slot.Id);
        if (originalSlot == null) return;

        IsEditing = true;
        var editModel = new WorkdaySlotEditModel
        {
            Id = originalSlot.Id,
            Date = originalSlot.Date.ToDateTime(TimeOnly.MinValue),
            Telework = originalSlot.Telework,
            ValidationError = string.Empty
        };
        editModel.SetStartTime(originalSlot.StartTime);
        editModel.SetEndTime(originalSlot.EndTime);
        EditingSlot = editModel;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        IsDialogOpen = false;
    }

    [RelayCommand]
    private async Task SaveSlotAsync()
    {
        var startTime = EditingSlot.GetStartTime();
        if (!startTime.HasValue)
        {
            EditingSlot.ValidationError = "L'hora d'inici no és vàlida.";
            return;
        }

        var endTime = EditingSlot.GetEndTime();
        if (!endTime.HasValue)
        {
            EditingSlot.ValidationError = "L'hora de fi no és vàlida.";
            return;
        }

        // Validar que l'hora de fi sigui posterior a l'hora d'inici
        if (endTime.Value <= startTime.Value)
        {
            EditingSlot.ValidationError = "L'hora de fi ha de ser posterior a l'hora d'inici.";
            return;
        }

        var slot = new WorkdaySlot
        {
            Id = IsEditing ? EditingSlot.Id : Guid.NewGuid(),
            Date = DateOnly.FromDateTime(EditingSlot.Date),
            StartTime = startTime.Value,
            EndTime = endTime.Value,
            Telework = EditingSlot.Telework
        };

        // Validar solapament
        var (isValid, errorMessage) = await _workdayService.ValidateWorkdaySlotAsync(slot);
        
        if (!isValid)
        {
            EditingSlot.ValidationError = errorMessage;
            return;
        }

        if (IsEditing)
        {
            await _workdaySlotRepository.UpdateAsync(slot);
            var index = _allSlots.FindIndex(s => s.Id == slot.Id);
            if (index >= 0) _allSlots[index] = slot;
        }
        else
        {
            await _workdaySlotRepository.AddAsync(slot);
            _allSlots.Add(slot);
        }

        IsDialogOpen = false;
        UpdateSlotsDisplay();
        UpdateDailySummary();
        await UpdateMonthlySummaryAsync();
    }

    [RelayCommand]
    private async Task DeleteSlotAsync(WorkdaySlotDisplay slot)
    {
        await _workdaySlotRepository.DeleteAsync(slot.Id);
        var index = _allSlots.FindIndex(s => s.Id == slot.Id);
        if (index >= 0) _allSlots.RemoveAt(index);
        UpdateSlotsDisplay();
        UpdateDailySummary();
        await UpdateMonthlySummaryAsync();
    }
}

/// <summary>
/// Model de presentació per a una franja de jornada.
/// </summary>
public class WorkdaySlotDisplay
{
    public Guid Id { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string LocationText { get; set; } = string.Empty;
    public string LocationIcon { get; set; } = string.Empty;
    public bool Telework { get; set; }
}

/// <summary>
/// Model d'edició per a una franja de jornada.
/// </summary>
public partial class WorkdaySlotEditModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private string _startTimeText = "09:00";

    [ObservableProperty]
    private string _endTimeText = "14:00";

    [ObservableProperty]
    private bool _telework;

    [ObservableProperty]
    private string _validationError = string.Empty;

    public TimeOnly? GetStartTime()
    {
        if (TimeOnly.TryParse(StartTimeText, out var time))
            return time;
        return null;
    }

    public TimeOnly? GetEndTime()
    {
        if (TimeOnly.TryParse(EndTimeText, out var time))
            return time;
        return null;
    }

    public void SetStartTime(TimeOnly time)
    {
        StartTimeText = time.ToString("HH:mm");
    }

    public void SetEndTime(TimeOnly time)
    {
        EndTimeText = time.ToString("HH:mm");
    }
}
