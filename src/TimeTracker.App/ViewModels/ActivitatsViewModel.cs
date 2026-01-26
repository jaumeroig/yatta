using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using TimeTracker.App.Services;
using TimeTracker.App.Views.Pages;

namespace TimeTracker.App.ViewModels;

/// <summary>
/// ViewModel per a la gestió d'activitats.
/// </summary>
public partial class ActivitatsViewModel : ObservableObject
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INavigationService _navigationService;
    private List<Activity> _allActivities = [];
    private List<TimeRecord> _allRecords = [];

    [ObservableProperty]
    private ObservableCollection<ActivityDisplay> _activities = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showInactive = false;

    /// <summary>
    /// S'executa quan canvia el text de cerca.
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// S'executa quan canvia el switch de mostrar inactives.
    /// </summary>
    partial void OnShowInactiveChanged(bool value)
    {
        ApplyFilters();
    }

    public ActivitatsViewModel(
        IActivityRepository activityRepository,
        ITimeRecordRepository timeRecordRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService)
    {
        _activityRepository = activityRepository;
        _timeRecordRepository = timeRecordRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Carrega les dades inicials.
    /// </summary>
    public async Task LoadDataAsync()
    {
        _allActivities = (await _activityRepository.GetAllAsync()).ToList();
        _allRecords = (await _timeRecordRepository.GetAllAsync()).ToList();
        ApplyFilters();
    }

    /// <summary>
    /// Aplica els filtres de cerca i estat actiu/inactiu a la llista d'activitats.
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = _allActivities.AsEnumerable();

        // Filtrar per estat actiu/inactiu
        if (!ShowInactive)
        {
            filtered = filtered.Where(a => a.Active);
        }

        // Filtrar per text de cerca
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(a => a.Name.ToLower().Contains(searchLower));
        }

        var activityDisplays = filtered.Select(activity =>
        {
            var records = _allRecords.Where(r => r.ActivityId == activity.Id).ToList();
            var totalHours = _timeCalculatorService.CalculateTotalHours(records);
            var totalTime = FormatDuration(totalHours);
            
            // Crear subtítol amb format: "X registres · Xh Xm"
            var recordsText = records.Count == 1 
                ? Resources.Resources.Activity_SingleRecord 
                : string.Format(Resources.Resources.Activity_MultipleRecords, records.Count);
            var subtitle = records.Count > 0 
                ? $"{recordsText} · {totalTime}" 
                : Resources.Resources.Activity_NoRecords;
            
            return new ActivityDisplay
            {
                Id = activity.Id,
                Name = activity.Name,
                Color = activity.Color,
                Active = activity.Active,
                RecordCount = records.Count,
                TotalTime = totalTime,
                Subtitle = subtitle,
                StatusText = activity.Active 
                    ? Resources.Resources.Status_Active
                    : Resources.Resources.Status_Inactive
            };
        }).OrderBy(a => a.Name);

        Activities = new ObservableCollection<ActivityDisplay>(activityDisplays);
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        var format = Resources.Resources.Format_Duration;
        return string.Format(format, h, m);
    }

    /// <summary>
    /// Navega a la pàgina de detall per crear una nova activitat.
    /// </summary>
    [RelayCommand]
    private void NavigateToNewActivity()
    {
        _navigationService.Navigate<ActivityDetailPage>(null);
    }

    /// <summary>
    /// Navega a la pàgina de detall per editar una activitat existent.
    /// </summary>
    [RelayCommand]
    private void NavigateToActivity(ActivityDisplay activity)
    {
        _navigationService.Navigate<ActivityDetailPage>(activity.Id);
    }
}

/// <summary>
/// Model de presentació per a una activitat.
/// </summary>
public class ActivityDisplay
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool Active { get; set; }
    public int RecordCount { get; set; }
    public string TotalTime { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    
    /// <summary>
    /// Subtítol amb el resum de registres i temps total.
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Retorna el color com a SolidColorBrush per facilitar el binding.
    /// </summary>
    public System.Windows.Media.SolidColorBrush ColorBrush
    {
        get
        {
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Color);
                return new System.Windows.Media.SolidColorBrush(color);
            }
            catch
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }
    }
}

