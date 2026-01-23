using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;

namespace TimeTracker.App.ViewModels;

/// <summary>
/// ViewModel per a la gestió d'activitats.
/// </summary>
public partial class ActivitatsViewModel : ObservableObject
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private List<Activity> _allActivities = [];
    private List<TimeRecord> _allRecords = [];

    [ObservableProperty]
    private ObservableCollection<ActivityDisplay> _activities = [];

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private ActivityEditModel _editingActivity = new();

    [ObservableProperty]
    private bool _isDeleteConfirmationOpen;

    [ObservableProperty]
    private ActivityDisplay? _activityToDelete;

    public ActivitatsViewModel(
        IActivityRepository activityRepository,
        ITimeRecordRepository timeRecordRepository,
        ITimeCalculatorService timeCalculatorService)
    {
        _activityRepository = activityRepository;
        _timeRecordRepository = timeRecordRepository;
        _timeCalculatorService = timeCalculatorService;
    }

    /// <summary>
    /// Carrega les dades inicials.
    /// </summary>
    public async Task LoadDataAsync()
    {
        _allActivities = (await _activityRepository.GetAllAsync()).ToList();
        _allRecords = (await _timeRecordRepository.GetAllAsync()).ToList();
        UpdateActivitiesDisplay();
    }

    private void UpdateActivitiesDisplay()
    {
        var activityDisplays = _allActivities.Select(activity =>
        {
            var records = _allRecords.Where(r => r.ActivityId == activity.Id).ToList();
            var totalHours = _timeCalculatorService.CalculateTotalHours(records);
            
            return new ActivityDisplay
            {
                Id = activity.Id,
                Name = activity.Name,
                Color = activity.Color,
                Active = activity.Active,
                RecordCount = records.Count,
                TotalTime = FormatDuration(totalHours),
                StatusText = activity.Active ? "Activa" : "Inactiva"
            };
        }).OrderBy(a => a.Name);

        Activities = new ObservableCollection<ActivityDisplay>(activityDisplays);
    }

    private static string FormatDuration(double hours)
    {
        var totalMinutes = (int)(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        return $"{h}h {m}m";
    }

    [RelayCommand]
    private void OpenNewActivityDialog()
    {
        IsEditing = false;
        EditingActivity = new ActivityEditModel
        {
            Name = string.Empty,
            Color = "#0078D4", // Default blue color
            Active = true
        };
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditActivityDialog(ActivityDisplay activity)
    {
        var originalActivity = _allActivities.FirstOrDefault(a => a.Id == activity.Id);
        if (originalActivity == null) return;

        IsEditing = true;
        EditingActivity = new ActivityEditModel
        {
            Id = originalActivity.Id,
            Name = originalActivity.Name,
            Color = originalActivity.Color,
            Active = originalActivity.Active
        };
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        IsDialogOpen = false;
    }

    [RelayCommand]
    private async Task SaveActivityAsync()
    {
        if (string.IsNullOrWhiteSpace(EditingActivity.Name)) return;

        var activity = new Activity
        {
            Id = IsEditing ? EditingActivity.Id : Guid.NewGuid(),
            Name = EditingActivity.Name.Trim(),
            Color = EditingActivity.Color,
            Active = EditingActivity.Active
        };

        if (IsEditing)
        {
            await _activityRepository.UpdateAsync(activity);
            var index = _allActivities.FindIndex(a => a.Id == activity.Id);
            if (index >= 0) _allActivities[index] = activity;
        }
        else
        {
            await _activityRepository.AddAsync(activity);
            _allActivities.Add(activity);
        }

        IsDialogOpen = false;
        UpdateActivitiesDisplay();
    }

    [RelayCommand]
    private async Task RequestDeleteActivity(ActivityDisplay activity)
    {
        ActivityToDelete = activity;
        
        // Check if activity has records
        if (activity.RecordCount > 0)
        {
            IsDeleteConfirmationOpen = true;
        }
        else
        {
            await ConfirmDeleteActivity();
        }
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmationOpen = false;
        ActivityToDelete = null;
    }

    [RelayCommand]
    private async Task ConfirmDeleteActivity()
    {
        if (ActivityToDelete == null) return;

        await _activityRepository.DeleteAsync(ActivityToDelete.Id);
        var index = _allActivities.FindIndex(a => a.Id == ActivityToDelete.Id);
        if (index >= 0) _allActivities.RemoveAt(index);
        
        IsDeleteConfirmationOpen = false;
        ActivityToDelete = null;
        UpdateActivitiesDisplay();
    }

    [RelayCommand]
    private void SelectColor(string color)
    {
        EditingActivity.Color = color;
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

/// <summary>
/// Model d'edició per a una activitat.
/// </summary>
public partial class ActivityEditModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorBrush))]
    private string _color = "#0078D4";

    [ObservableProperty]
    private bool _active = true;

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
