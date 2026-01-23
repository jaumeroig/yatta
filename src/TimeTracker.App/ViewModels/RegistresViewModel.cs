using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

namespace TimeTracker.App.ViewModels;

/// <summary>
/// ViewModel per a la gestió de registres de temps.
/// </summary>
public partial class RegistresViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private List<TimeRecord> _allRecords = [];
    private List<Activity> _allActivities = [];

    [ObservableProperty]
    private ObservableCollection<DayGroup> _groupedRecords = [];

    [ObservableProperty]
    private ObservableCollection<Activity> _activities = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Activity? _selectedActivityFilter;

    [ObservableProperty]
    private DateTime? _selectedDate;

    [ObservableProperty]
    private string _todayWorkedTime = "0h 0m";

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private TimeRecordEditModel _editingRecord = new();

    public RegistresViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        ITimeCalculatorService timeCalculatorService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _timeCalculatorService = timeCalculatorService;
    }

    /// <summary>
    /// Carrega les dades inicials.
    /// </summary>
    public async Task LoadDataAsync()
    {
        _allActivities = (await _activityRepository.GetActiveAsync()).ToList();
        Activities = new ObservableCollection<Activity>(_allActivities);

        _allRecords = (await _timeRecordRepository.GetAllAsync()).ToList();
        ApplyFilters();
        CalculateTodayWorkedTime();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedActivityFilterChanged(Activity? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedDateChanged(DateTime? value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allRecords.AsEnumerable();

        // Filtre per text (cerca a notes i nom d'activitat)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(r =>
                (r.Notes?.Contains(searchLower, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                _allActivities.FirstOrDefault(a => a.Id == r.ActivityId)?.Name.Contains(searchLower, StringComparison.InvariantCultureIgnoreCase) == true);
        }

        // Filtre per activitat
        if (SelectedActivityFilter != null)
        {
            filtered = filtered.Where(r => r.ActivityId == SelectedActivityFilter.Id);
        }

        // Filtre per data
        if (SelectedDate.HasValue)
        {
            var date = DateOnly.FromDateTime(SelectedDate.Value);
            filtered = filtered.Where(r => r.Date == date);
        }

        // Agrupar per dia
        var groups = filtered
            .GroupBy(r => r.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new DayGroup
            {
                Date = g.Key,
                DateDisplay = FormatDate(g.Key),
                TotalWorked = FormatDuration(_timeCalculatorService.CalculateTotalHours(g)),
                Records = new ObservableCollection<TimeRecordDisplay>(
                    g.OrderBy(r => r.StartTime).Select(r => CreateRecordDisplay(r)))
            });

        GroupedRecords = new ObservableCollection<DayGroup>(groups);
    }

    private TimeRecordDisplay CreateRecordDisplay(TimeRecord record)
    {
        var activity = _allActivities.FirstOrDefault(a => a.Id == record.ActivityId);
        var duration = record.EndTime.HasValue
            ? _timeCalculatorService.CalculateDuration(record.StartTime, record.EndTime.Value)
            : 0;

        return new TimeRecordDisplay
        {
            Id = record.Id,
            ActivityName = activity?.Name ?? "Desconeguda",
            ActivityColor = activity?.Color ?? "#808080",
            Notes = record.Notes ?? string.Empty,
            StartTime = record.StartTime.ToString("HH:mm"),
            EndTime = record.EndTime?.ToString("HH:mm") ?? "--:--",
            Duration = FormatDuration(duration),
            Date = record.Date
        };
    }

    private static string FormatDate(DateOnly date)
    {
        var culture = new CultureInfo("ca-ES");
        var dayName = culture.TextInfo.ToTitleCase(date.ToString("dddd", culture));
        var day = date.Day;
        var month = culture.TextInfo.ToTitleCase(date.ToString("MMMM", culture));
        return $"{dayName}, {day} de {month}";
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        return $"{h}h {m}m";
    }

    private void CalculateTodayWorkedTime()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayRecords = _allRecords.Where(r => r.Date == today);
        var totalHours = _timeCalculatorService.CalculateTotalHours(todayRecords);
        TodayWorkedTime = FormatDuration(totalHours);
    }

    [RelayCommand]
    private void OpenNewRecordDialog()
    {
        IsEditing = false;
        EditingRecord = new TimeRecordEditModel
        {
            Date = DateTime.Today,
            StartTimeText = "09:00",
            EndTimeText = "10:00"
        };
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditRecordDialog(TimeRecordDisplay record)
    {
        var originalRecord = _allRecords.FirstOrDefault(r => r.Id == record.Id);
        if (originalRecord == null) return;

        IsEditing = true;
        var editModel = new TimeRecordEditModel
        {
            Id = originalRecord.Id,
            ActivityId = originalRecord.ActivityId,
            Date = originalRecord.Date.ToDateTime(TimeOnly.MinValue),
            Notes = originalRecord.Notes ?? string.Empty
        };
        editModel.SetStartTime(originalRecord.StartTime);
        editModel.SetEndTime(originalRecord.EndTime);
        EditingRecord = editModel;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        IsDialogOpen = false;
    }

    [RelayCommand]
    private async Task SaveRecordAsync()
    {
        if (EditingRecord.ActivityId == Guid.Empty) return;

        var startTime = EditingRecord.GetStartTime();
        if (!startTime.HasValue) return;

        var record = new TimeRecord
        {
            Id = IsEditing ? EditingRecord.Id : Guid.NewGuid(),
            ActivityId = EditingRecord.ActivityId,
            Date = DateOnly.FromDateTime(EditingRecord.Date),
            StartTime = startTime.Value,
            EndTime = EditingRecord.GetEndTime(),
            Notes = string.IsNullOrWhiteSpace(EditingRecord.Notes) ? null : EditingRecord.Notes
        };

        if (IsEditing)
        {
            await _timeRecordRepository.UpdateAsync(record);
            var index = _allRecords.FindIndex(r => r.Id == record.Id);
            if (index >= 0) _allRecords[index] = record;
        }
        else
        {
            await _timeRecordRepository.AddAsync(record);
            _allRecords.Add(record);
        }

        IsDialogOpen = false;
        ApplyFilters();
        CalculateTodayWorkedTime();
    }

    [RelayCommand]
    private async Task DeleteRecordAsync(TimeRecordDisplay record)
    {
        await _timeRecordRepository.DeleteAsync(record.Id);
        var index = _allRecords.FindIndex(r => r.Id == record.Id);
        if (index >= 0) _allRecords.RemoveAt(index);
        ApplyFilters();
        CalculateTodayWorkedTime();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedActivityFilter = null;
        SelectedDate = null;
    }
}

/// <summary>
/// Grup de registres per dia.
/// </summary>
public class DayGroup
{
    public DateOnly Date { get; set; }
    public string DateDisplay { get; set; } = string.Empty;
    public string TotalWorked { get; set; } = string.Empty;
    public ObservableCollection<TimeRecordDisplay> Records { get; set; } = [];
}

/// <summary>
/// Model de presentació per a un registre de temps.
/// </summary>
public class TimeRecordDisplay
{
    public Guid Id { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityColor { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}

/// <summary>
/// Model d'edició per a un registre de temps.
/// </summary>
public partial class TimeRecordEditModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private Guid _activityId;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private string _startTimeText = "09:00";

    [ObservableProperty]
    private string _endTimeText = "10:00";

    [ObservableProperty]
    private string _notes = string.Empty;

    public TimeOnly? GetStartTime()
    {
        if (TimeOnly.TryParse(StartTimeText, out var time))
            return time;
        return null;
    }

    public TimeOnly? GetEndTime()
    {
        if (string.IsNullOrWhiteSpace(EndTimeText))
            return null;
        if (TimeOnly.TryParse(EndTimeText, out var time))
            return time;
        return null;
    }

    public void SetStartTime(TimeOnly time)
    {
        StartTimeText = time.ToString("HH:mm");
    }

    public void SetEndTime(TimeOnly? time)
    {
        EndTimeText = time?.ToString("HH:mm") ?? "";
    }
}
