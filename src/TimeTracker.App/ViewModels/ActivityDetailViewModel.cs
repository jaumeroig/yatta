namespace TimeTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.App.Services;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using Wpf.Ui.Controls;

/// <summary>
/// ViewModel for the activity detail page.
/// </summary>
public partial class ActivityDetailViewModel : ObservableObject
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IBreadcrumbService _breadcrumbService;
    private Guid _activityId;
    private bool _isNewActivity;
    private string _originalName = string.Empty;

    /// <summary>
    /// Maximum allowed length for the activity name.
    /// </summary>
    public const int MaxNameLength = 100;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorBrush))]
    private string _color = "#0078D4";

    [ObservableProperty]
    private bool _active = true;

    [ObservableProperty]
    private int _recordCount;

    [ObservableProperty]
    private string _totalTime = "0h 0m";

    [ObservableProperty]
    private bool _isDeleteConfirmationOpen;

    [ObservableProperty]
    private string _pageTitle = "Nova activitat";

    [ObservableProperty]
    private string _nameError = string.Empty;

    [ObservableProperty]
    private bool _hasNameError;

    /// <summary>
    /// Indicates if the activity already exists (not new).
    /// </summary>
    public bool IsExistingActivity => !_isNewActivity;

    /// <summary>
    /// Text for the archive/unarchive button based on Active state.
    /// </summary>
    public string ArchiveButtonText => Active
        ? Resources.Resources.Button_Archive
        : Resources.Resources.Button_Unarchive;

    public IconElement ArchiveButtonIcon => Active 
        ?new SymbolIcon(SymbolRegular.Archive24)
        : new SymbolIcon(SymbolRegular.ArchiveArrowBack24);

    public ControlAppearance ArchiveButtonAppearance => Active 
        ? ControlAppearance.Danger 
        : ControlAppearance.Secondary;

    /// <summary>
    /// Indicates if the activity can be saved (basic validation).
    /// </summary>
    public bool CanSave => !string.IsNullOrWhiteSpace(Name) && Name.Length <= MaxNameLength;

    /// <summary>
    /// Returns the color as a SolidColorBrush to facilitate binding.
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

    public ActivityDetailViewModel(
        IActivityRepository activityRepository,
        ITimeRecordRepository timeRecordRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService,
        IDialogService dialogService,
        IBreadcrumbService breadcrumbService)
    {
        _activityRepository = activityRepository;
        _timeRecordRepository = timeRecordRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _breadcrumbService = breadcrumbService;
    }

    /// <summary>
    /// Initializes the ViewModel with the activity data.
    /// </summary>
    public async Task InitializeAsync(Guid? activityId)
    {
        if (activityId.HasValue && activityId.Value != Guid.Empty)
        {
            _activityId = activityId.Value;
            _isNewActivity = false;
            OnPropertyChanged(nameof(IsExistingActivity));
            await LoadActivityAsync();
        }
        else
        {
            _activityId = Guid.NewGuid();
            _isNewActivity = true;
            OnPropertyChanged(nameof(IsExistingActivity));
            PageTitle = Resources.Resources.ActivityDetail_NewTitle;
            Name = string.Empty;
            _originalName = string.Empty;
            Color = "#0078D4";
            Active = true;
            RecordCount = 0;
            TotalTime = FormatDuration(0);
        }

        UpdateBreadcrumb();
        ClearErrors();
    }

    private async Task LoadActivityAsync()
    {
        var activity = await _activityRepository.GetByIdAsync(_activityId);
        if (activity == null)
        {
            _navigationService.GoBack();
            return;
        }

        PageTitle = activity.Name;
        Name = activity.Name;
        _originalName = activity.Name;
        Color = activity.Color;
        Active = activity.Active;
        OnPropertyChanged(nameof(ArchiveButtonText));

        // Load statistics
        var records = (await _timeRecordRepository.GetByActivityIdAsync(_activityId)).ToList();
        RecordCount = records.Count;
        var totalHours = _timeCalculatorService.CalculateTotalHours(records);
        TotalTime = FormatDuration(totalHours);
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
    /// Updates the breadcrumb items.
    /// </summary>
    private void UpdateBreadcrumb()
    {
        var activitiesLabel = Resources.Resources.Nav_Activities;

        _breadcrumbService.SetItems(
            new BreadcrumbItem(activitiesLabel, () => _navigationService.GoBack()),
            new BreadcrumbItem(PageTitle)
        );
    }

    /// <summary>
    /// Clears all validation errors.
    /// </summary>
    private void ClearErrors()
    {
        NameError = string.Empty;
        HasNameError = false;
    }

    /// <summary>
    /// Validates the activity name.
    /// </summary>
    /// <returns>True if the name is valid, false otherwise.</returns>
    private async Task<bool> ValidateNameAsync()
    {
        ClearErrors();

        var trimmedName = Name?.Trim() ?? string.Empty;

        // Validate that it's not empty
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            NameError = Resources.Resources.Validation_ActivityNameRequired;
            HasNameError = true;
            return false;
        }

        // Validate maximum length
        if (trimmedName.Length > MaxNameLength)
        {
            NameError = string.Format(Resources.Resources.Validation_ActivityNameTooLong, MaxNameLength);
            HasNameError = true;
            return false;
        }

        // Validate that another activity with the same name doesn't exist
        // (except if it's the same activity we're editing)
        var nameChanged = !string.Equals(trimmedName, _originalName, StringComparison.OrdinalIgnoreCase);
        if (nameChanged)
        {
            var existingActivity = await _activityRepository.GetByNameAsync(trimmedName);
            if (existingActivity != null && existingActivity.Id != _activityId)
            {
                NameError = Resources.Resources.Validation_ActivityNameExists;
                HasNameError = true;
                return false;
            }
        }

        return true;
    }

    [RelayCommand]
    private void SelectColor(string color)
    {
        Color = color;
    }

    partial void OnActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(ArchiveButtonText));
        OnPropertyChanged(nameof(ArchiveButtonIcon)); 
        OnPropertyChanged(nameof(ArchiveButtonAppearance));
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        // Validate before saving
        if (!await ValidateNameAsync())
        {
            return;
        }

        var activity = new Activity
        {
            Id = _activityId,
            Name = Name.Trim(),
            Color = Color,
            Active = Active
        };

        try
        {
            if (_isNewActivity)
            {
                await _activityRepository.AddAsync(activity);
            }
            else
            {
                await _activityRepository.UpdateAsync(activity);
            }

            _navigationService.GoBack();
        }
        catch (Exception)
        {
            // In case of unexpected error (e.g., DB constraint), show generic message
            NameError = Resources.Resources.Validation_ActivitySaveError;
            HasNameError = true;
        }
    }

    [RelayCommand]
    private async Task ToggleArchive()
    {
        // Toggle Active state
        Active = !Active;

        // Update repository if existing activity
        if (!_isNewActivity)
        {
            var activity = await _activityRepository.GetByIdAsync(_activityId);
            if (activity != null)
            {
                activity.Active = Active;
                await _activityRepository.UpdateAsync(activity);
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.GoBack();
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    [RelayCommand]
    private void RequestDelete()
    {
        if (_isNewActivity) return;

        if (RecordCount > 0)
        {
            IsDeleteConfirmationOpen = true;
        }
        else
        {
            _ = ConfirmDeleteAsync();
        }
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmationOpen = false;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        // Delete all related records first
        var relatedRecords = await _timeRecordRepository.GetByActivityIdAsync(_activityId);
        foreach (var record in relatedRecords)
        {
            await _timeRecordRepository.DeleteAsync(record.Id);
        }

        // Then delete the activity
        await _activityRepository.DeleteAsync(_activityId);

        IsDeleteConfirmationOpen = false;
        _navigationService.GoBack();
    }
}
