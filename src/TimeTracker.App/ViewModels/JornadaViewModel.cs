namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.App.Controls;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

/// <summary>
/// ViewModel for workday management.
/// </summary>
public partial class JornadaViewModel : ObservableObject
{
    private readonly IWorkdaySlotRepository _workdaySlotRepository;
    private readonly IWorkdayService _workdayService;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkdayConfigService _workdayConfigService;
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

    // Properties for bar chart
    [ObservableProperty]
    private double _officeHoursValue;

    [ObservableProperty]
    private double _teleworkHoursValue;

    [ObservableProperty]
    private double _officeBarWidth;

    [ObservableProperty]
    private double _teleworkBarWidth;

    // Timeline segments for WorkdayTimelineBar
    [ObservableProperty]
    private ObservableCollection<TimeSegment> _timelineSegments = [];

    // Dates with records (for calendar)
    [ObservableProperty]
    private ObservableCollection<DateTime> _datesWithRecords = [];

    // Dates categorized by location for calendar indicators
    [ObservableProperty]
    private ObservableCollection<DateTime> _teleworkDates = [];

    [ObservableProperty]
    private ObservableCollection<DateTime> _officeDates = [];

    [ObservableProperty]
    private ObservableCollection<DateTime> _bothDates = [];

    // Properties for day configuration dialog
    [ObservableProperty]
    private bool _isConfigDialogOpen;

    [ObservableProperty]
    private WorkdayConfigEditModel _editingConfig = new();

    public JornadaViewModel(
        IWorkdaySlotRepository workdaySlotRepository,
        IWorkdayService workdayService,
        ITimeCalculatorService timeCalculatorService,
        ILocalizationService localizationService,
        IWorkdayConfigService workdayConfigService)
    {
        _workdaySlotRepository = workdaySlotRepository;
        _workdayService = workdayService;
        _timeCalculatorService = timeCalculatorService;
        _localizationService = localizationService;
        _workdayConfigService = workdayConfigService;

        UpdateDateDisplay();
        UpdateMonthYearDisplay();
    }

    /// <summary>
    /// Loads initial data.
    /// </summary>
    public async Task LoadDataAsync()
    {
        await LoadSlotsForDateAsync(SelectedDate);
        await UpdateMonthlySummaryAsync();
        await LoadDatesWithRecordsAsync();
    }

    /// <summary>
    /// Loads the dates in the month that have records.
    /// </summary>
    private async Task LoadDatesWithRecordsAsync()
    {
        var firstDay = new DateOnly(SelectedDate.Year, SelectedDate.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var dates = await _workdaySlotRepository.GetDatesWithSlotsAsync(firstDay, lastDay);
        var monthSlots = await _workdaySlotRepository.GetByDateRangeAsync(firstDay, lastDay);

        var telework = new HashSet<DateTime>();
        var office = new HashSet<DateTime>();
        var both = new HashSet<DateTime>();

        foreach (var slot in monthSlots)
        {
            var dt = slot.Date.ToDateTime(TimeOnly.MinValue);
            if (slot.Telework)
            {
                if (office.Contains(dt)) both.Add(dt); else telework.Add(dt);
            }
            else
            {
                if (telework.Contains(dt)) both.Add(dt); else office.Add(dt);
            }
        }

        // Remove dates from single sets if they are in both
        foreach (var dt in both)
        {
            telework.Remove(dt);
            office.Remove(dt);
        }

        DatesWithRecords = new ObservableCollection<DateTime>(dates.Select(d => d.ToDateTime(TimeOnly.MinValue)));
        TeleworkDates = new ObservableCollection<DateTime>(telework.OrderBy(d => d));
        OfficeDates = new ObservableCollection<DateTime>(office.OrderBy(d => d));
        BothDates = new ObservableCollection<DateTime>(both.OrderBy(d => d));
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
                LocationText = slot.Telework
                    ? Resources.Resources.Location_Telework
                    : Resources.Resources.Location_Office,
                LocationIcon = slot.Telework ? "Home24" : "Building24",
                Telework = slot.Telework
            });

        Slots = new ObservableCollection<WorkdaySlotDisplay>(slotDisplays);
        UpdateTimelineSegments();
    }

    private void UpdateTimelineSegments()
    {
        var segments = _allSlots
            .OrderBy(s => s.StartTime)
            .Select(slot =>
            {
                var date = DateOnly.FromDateTime(SelectedDate);
                return new TimeSegment
                {
                    Label = slot.Telework
                        ? Resources.Resources.Location_Telework
                        : Resources.Resources.Location_Office,
                    Start = date.ToDateTime(slot.StartTime),
                    End = date.ToDateTime(slot.EndTime),
                    Color = slot.Telework
                        ? Color.FromRgb(0x21, 0x96, 0xF3)  // #2196F3 blue
                        : Color.FromRgb(0x4C, 0xAF, 0x50)  // #4CAF50 green
                };
            });

        TimelineSegments = new ObservableCollection<TimeSegment>(segments);
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

        // Update values for bar chart
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
            OfficeBarWidth = OfficeHoursValue / maxHours * maxBarWidth;
            TeleworkBarWidth = TeleworkHoursValue / maxHours * maxBarWidth;
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
        SelectedDateDisplay = SelectedDate.ToString("D");
    }

    private void UpdateMonthYearDisplay()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        var month = culture.TextInfo.ToTitleCase(SelectedDate.ToString("MMMM", culture));
        MonthYear = $"{month} {SelectedDate.Year}";
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = Resources.Resources.Format_Duration;
        return string.Format(format, h, m);
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
    private async Task SaveSlot()
    {
        var startTime = EditingSlot.GetStartTime();
        if (!startTime.HasValue)
        {
            EditingSlot.ValidationError = Resources.Resources.Validation_InvalidStartTime;
            return;
        }

        var endTime = EditingSlot.GetEndTime();
        if (!endTime.HasValue)
        {
            EditingSlot.ValidationError = Resources.Resources.Validation_InvalidEndTime;
            return;
        }

        // Validate that end time is after start time
        if (endTime.Value <= startTime.Value)
        {
            EditingSlot.ValidationError = Resources.Resources.Validation_EndTimeAfterStartTime;
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

        // Validate overlap
        var (isValid, errorMessage) = await _workdayService.ValidateWorkdaySlotAsync(slot);
        if (!isValid)
        {
            // If errorMessage is a resource key with arguments (pipe-delimited), localize it
            if (!string.IsNullOrWhiteSpace(errorMessage) && errorMessage.Contains("|"))
            {
                var parts = errorMessage.Split('|');
                var key = parts[0];
                var args = parts.Skip(1).ToArray();
                EditingSlot.ValidationError = _localizationService.GetString(key, args);
            }
            else if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                EditingSlot.ValidationError = _localizationService.GetString(errorMessage);
            }
            else
            {
                EditingSlot.ValidationError = string.Empty;
            }
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
        await LoadDatesWithRecordsAsync();
    }

    [RelayCommand]
    private async Task DeleteSlot(WorkdaySlotDisplay slot)
    {
        await _workdaySlotRepository.DeleteAsync(slot.Id);
        var index = _allSlots.FindIndex(s => s.Id == slot.Id);
        if (index >= 0) _allSlots.RemoveAt(index);
        UpdateSlotsDisplay();
        UpdateDailySummary();
        await UpdateMonthlySummaryAsync();
        await LoadDatesWithRecordsAsync();
    }

    [RelayCommand]
    private async Task OpenConfigureDay()
    {
        var date = DateOnly.FromDateTime(SelectedDate);
        var config = await _workdayConfigService.GetEffectiveConfigurationAsync(date);
        var hasSpecificConfig = config.Id != Guid.Empty;

        EditingConfig = new WorkdayConfigEditModel
        {
            Date = SelectedDate,
            DayType = config.DayType,
            HasSpecificConfiguration = hasSpecificConfig,
            ValidationError = string.Empty
        };

        // Set the target duration text
        if (config.TargetDuration > TimeSpan.Zero)
        {
            EditingConfig.TargetDurationText = FormatDurationForEdit(config.TargetDuration);
        }
        else
        {
            EditingConfig.TargetDurationText = "00:00";
        }

        // Store original values for change detection
        EditingConfig.OriginalDayType = config.DayType;
        EditingConfig.OriginalTargetDuration = config.TargetDuration;

        IsConfigDialogOpen = true;
    }

    [RelayCommand]
    private void CloseConfigDialog()
    {
        IsConfigDialogOpen = false;
    }

    [RelayCommand]
    private async Task SaveDayConfiguration()
    {
        var date = DateOnly.FromDateTime(EditingConfig.Date);
        var dayType = EditingConfig.DayType;

        TimeSpan? targetDuration = null;

        // Only parse duration for working days
        if (dayType == DayType.WorkDay || dayType == DayType.IntensiveDay)
        {
            var parsedDuration = ParseDuration(EditingConfig.TargetDurationText);
            if (!parsedDuration.HasValue)
            {
                EditingConfig.ValidationError = Resources.Resources.Validation_InvalidDuration;
                return;
            }
            targetDuration = parsedDuration.Value;
        }

        try
        {
            await _workdayConfigService.SetConfigurationAsync(date, dayType, targetDuration);
            IsConfigDialogOpen = false;
            
            // Refresh the view if needed
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            EditingConfig.ValidationError = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ResetDayConfiguration()
    {
        var date = DateOnly.FromDateTime(EditingConfig.Date);
        await _workdayConfigService.ResetConfigurationAsync(date);
        
        IsConfigDialogOpen = false;
        
        // Refresh the view
        await LoadDataAsync();
    }

    private static string FormatDurationForEdit(TimeSpan duration)
    {
        return WorkdayConfigEditModel.FormatDuration(duration);
    }

    private static TimeSpan? ParseDuration(string text)
    {
        return WorkdayConfigEditModel.ParseDuration(text);
    }
}

/// <summary>
/// Display model for a workday slot.
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
/// Edit model for a workday slot.
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

/// <summary>
/// Edit model for workday configuration.
/// </summary>
public partial class WorkdayConfigEditModel : ObservableObject
{
    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private DayType _dayType = DayType.WorkDay;

    [ObservableProperty]
    private string _targetDurationText = "08:00";

    [ObservableProperty]
    private bool _hasSpecificConfiguration;

    [ObservableProperty]
    private string _validationError = string.Empty;

    // For change detection
    public DayType OriginalDayType { get; set; }
    public TimeSpan OriginalTargetDuration { get; set; }

    public bool HasChanges
    {
        get
        {
            if (DayType != OriginalDayType)
            {
                return true;
            }

            // Only check duration changes for working days
            if (DayType == DayType.WorkDay || DayType == DayType.IntensiveDay)
            {
                var currentDuration = ParseDuration(TargetDurationText);
                if (currentDuration.HasValue && currentDuration.Value != OriginalTargetDuration)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool IsWorkingDay => DayType == DayType.WorkDay || DayType == DayType.IntensiveDay;

    // Radio button properties
    public bool IsNormalWorkDay
    {
        get => DayType == DayType.WorkDay;
        set { if (value) DayType = DayType.WorkDay; }
    }

    public bool IsIntensiveWorkDay
    {
        get => DayType == DayType.IntensiveDay;
        set { if (value) DayType = DayType.IntensiveDay; }
    }

    public bool IsHolidayDay
    {
        get => DayType == DayType.Holiday;
        set { if (value) DayType = DayType.Holiday; }
    }

    public bool IsFreeChoiceDay
    {
        get => DayType == DayType.FreeChoice;
        set { if (value) DayType = DayType.FreeChoice; }
    }

    public bool IsVacationDay
    {
        get => DayType == DayType.Vacation;
        set { if (value) DayType = DayType.Vacation; }
    }

    partial void OnDayTypeChanged(DayType value)
    {
        OnPropertyChanged(nameof(IsWorkingDay));
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(IsNormalWorkDay));
        OnPropertyChanged(nameof(IsIntensiveWorkDay));
        OnPropertyChanged(nameof(IsHolidayDay));
        OnPropertyChanged(nameof(IsFreeChoiceDay));
        OnPropertyChanged(nameof(IsVacationDay));
    }

    partial void OnTargetDurationTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasChanges));
    }

    /// <summary>
    /// Formats a TimeSpan as HH:mm for display in edit controls.
    /// </summary>
    public static string FormatDuration(TimeSpan duration)
    {
        var totalMinutes = (int)duration.TotalMinutes;
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return $"{hours:D2}:{minutes:D2}";
    }

    /// <summary>
    /// Parses a duration string in HH:mm format to TimeSpan.
    /// Returns null if the format is invalid.
    /// </summary>
    public static TimeSpan? ParseDuration(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var parts = text.Split(':');
        if (parts.Length != 2)
        {
            return null;
        }

        if (!int.TryParse(parts[0], out var hours) || !int.TryParse(parts[1], out var minutes))
        {
            return null;
        }

        if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
        {
            return null;
        }

        return new TimeSpan(hours, minutes, 0);
    }
}
