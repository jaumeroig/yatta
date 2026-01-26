using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using TimeTracker.App.Services;
using TimeTracker.App.Views.Pages;

namespace TimeTracker.App.ViewModels;

/// <summary>
/// ViewModel per a la gestió de registres de temps.
/// </summary>
public partial class RegistresViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INavigationService _navigationService;
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

    public RegistresViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Carrega les dades inicials.
    /// </summary>
    public async Task LoadDataAsync()
    {
        _allActivities = (await _activityRepository.GetActiveAsync()).ToList();

        // Afegir opció "Totes les activitats" al principi
        var allActivitiesText = Resources.Resources.Filter_AllActivities;
        var allActivitiesOption = new Activity { Id = Guid.Empty, Name = allActivitiesText };
        var activitiesWithAll = new List<Activity> { allActivitiesOption };
        activitiesWithAll.AddRange(_allActivities);
        Activities = new ObservableCollection<Activity>(activitiesWithAll);

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

        // Filtre per activitat (excepte "Totes les activitats")
        if (SelectedActivityFilter != null && SelectedActivityFilter.Id != Guid.Empty)
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
            ActivityName = activity?.Name ?? Resources.Resources.Activity_Unknown,
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
        return date.ToLongDateString();
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = Resources.Resources.Format_Duration;
        return string.Format(format, h, m);
    }

    private void CalculateTodayWorkedTime()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayRecords = _allRecords.Where(r => r.Date == today);
        var totalHours = _timeCalculatorService.CalculateTotalHours(todayRecords);
        TodayWorkedTime = FormatDuration(totalHours);
    }

    /// <summary>
    /// Navega a la pàgina de detall per crear un nou registre.
    /// </summary>
    [RelayCommand]
    private void NavigateToNewRecord()
    {
        _navigationService.Navigate<RecordDetailPage>(null);
    }

    /// <summary>
    /// Navega a la pàgina de detall per editar un registre existent.
    /// </summary>
    [RelayCommand]
    private void NavigateToRecord(TimeRecordDisplay record)
    {
        _navigationService.Navigate<RecordDetailPage>(record.Id);
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

    /// <summary>
    /// Retorna el color com a SolidColorBrush per facilitar el binding.
    /// </summary>
    public System.Windows.Media.SolidColorBrush ActivityColorBrush
    {
        get
        {
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ActivityColor);
                return new System.Windows.Media.SolidColorBrush(color);
            }
            catch
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }
    }
}
