namespace TimeTracker.App.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.App.Services;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;


/// <summary>
/// ViewModel for the time record detail page.
/// </summary>
public partial class HistoricDetailViewModel : ObservableObject
{
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IBreadcrumbService _breadcrumbService;
    private Guid _recordId;
    private bool _isNewRecord;

    [ObservableProperty]
    private ObservableCollection<Activity> _activities = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private Guid _activityId;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _startTimeText = "09:00";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _endTimeText = "10:00";

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _isDeleteConfirmationOpen;

    [ObservableProperty]
    private string _pageTitle = "Nou registre";

    [ObservableProperty]
    private string _activityError = string.Empty;

    [ObservableProperty]
    private bool _hasActivityError;

    [ObservableProperty]
    private string _timeError = string.Empty;

    [ObservableProperty]
    private bool _shouldFocusEndTime;

    /// <summary>
    /// Indicates if the record already exists (not new).
    /// </summary>
    public bool IsExistingRecord => !_isNewRecord;

    /// <summary>
    /// Indicates if the record can be saved (basic validation).
    /// The end time is optional (can be empty).
    /// </summary>
    public bool CanSave => ActivityId != Guid.Empty &&
                           TimeOnly.TryParse(StartTimeText, out _) &&
                           (string.IsNullOrWhiteSpace(EndTimeText) || TimeOnly.TryParse(EndTimeText, out _));

    public HistoricDetailViewModel(
        ITimeRecordRepository timeRecordRepository,
        IActivityRepository activityRepository,
        INavigationService navigationService,
        IDialogService dialogService,
        IBreadcrumbService breadcrumbService)
    {
        _timeRecordRepository = timeRecordRepository;
        _activityRepository = activityRepository;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _breadcrumbService = breadcrumbService;
    }

    /// <summary>
    /// Initializes the ViewModel with the record data.
    /// </summary>
    public async Task InitializeAsync(Guid? recordId, bool fromNotification = false)
    {
        // Load active activities
        var activeActivities = await _activityRepository.GetActiveAsync();
        Activities = new ObservableCollection<Activity>(activeActivities);

        if (recordId.HasValue && recordId.Value != Guid.Empty)
        {
            _recordId = recordId.Value;
            _isNewRecord = false;
            OnPropertyChanged(nameof(IsExistingRecord));
            await LoadRecordAsync();

            // If coming from notification, set current time as end time
            if (fromNotification)
            {
                EndTimeText = DateTime.Now.ToString("HH:mm");
                ShouldFocusEndTime = true;
            }
        }
        else
        {
            _recordId = Guid.NewGuid();
            _isNewRecord = true;
            OnPropertyChanged(nameof(IsExistingRecord));
            PageTitle = Resources.Resources.RecordDetail_NewTitle;
            ActivityId = Guid.Empty;
            Date = DateTime.Today;
            StartTimeText = await GetDefaultStartTimeAsync(DateOnly.FromDateTime(Date));
            EndTimeText = "";
            Notes = string.Empty;
        }

        UpdateBreadcrumb();
        ClearErrors();
    }

    private async Task LoadRecordAsync()
    {
        var record = await _timeRecordRepository.GetByIdAsync(_recordId);
        if (record == null)
        {
            _navigationService.GoBack();
            return;
        }

        // Ensure the record's activity is available for selection
        // (the activity might be inactive)
        var activity = await _activityRepository.GetByIdAsync(record.ActivityId);
        if (activity != null && !Activities.Any(a => a.Id == activity.Id))
        {
            Activities.Insert(0, activity);
        }

        PageTitle = Resources.Resources.RecordDetail_EditTitle;
        ActivityId = record.ActivityId;
        Date = record.Date.ToDateTime(TimeOnly.MinValue);
        StartTimeText = record.StartTime.ToString("HH:mm");
        EndTimeText = record.EndTime?.ToString("HH:mm") ?? "";
        Notes = record.Notes ?? string.Empty;
    }

    /// <summary>
    /// Gets the default start time for a new record on the given date.
    /// If there are existing records, returns the end time of the last one.
    /// Otherwise, returns the current time.
    /// </summary>
    private async Task<string> GetDefaultStartTimeAsync(DateOnly date)
    {
        var records = await _timeRecordRepository.GetByDateAsync(date);
        var lastRecord = records
            .Where(r => r.EndTime.HasValue)
            .OrderByDescending(r => r.EndTime)
            .FirstOrDefault();

        if (lastRecord?.EndTime != null)
        {
            return lastRecord.EndTime.Value.ToString("HH:mm");
        }

        return DateTime.Now.ToString("HH:mm");
    }

    /// <summary>
    /// Updates the breadcrumb items.
    /// </summary>
    private void UpdateBreadcrumb()
    {
        var recordsLabel = Resources.Resources.Nav_Records;

        _breadcrumbService.SetItems(
            new BreadcrumbItem(recordsLabel, () => _navigationService.GoBack()),
            new BreadcrumbItem(PageTitle)
        );
    }

    /// <summary>
    /// Clears all validation errors.
    /// </summary>
    private void ClearErrors()
    {
        ActivityError = string.Empty;
        HasActivityError = false;
        TimeError = string.Empty;
    }

    /// <summary>
    /// Validates the record data.
    /// </summary>
    /// <returns>True if the data is valid, false otherwise.</returns>
    private bool Validate()
    {
        ClearErrors();
        var isValid = true;


        // Validate selected activity
        if (ActivityId == Guid.Empty)
        {
            ActivityError = Resources.Resources.Validation_ActivityRequired;
            HasActivityError = true;
            isValid = false;
        }


        // Validate start time
        if (!TimeOnly.TryParse(StartTimeText, out var startTime))
        {
            TimeError = Resources.Resources.Validation_InvalidStartTime;
            return false;
        }


        // Validate end time (optional if empty)
        TimeOnly? endTime = null;
        if (!string.IsNullOrWhiteSpace(EndTimeText))
        {
            if (!TimeOnly.TryParse(EndTimeText, out var parsedEndTime))
            {
                TimeError = Resources.Resources.Validation_InvalidEndTime;
                return false;
            }
            endTime = parsedEndTime;

            // Validate that end time is after start time
            if (endTime.Value <= startTime)
            {
                TimeError = Resources.Resources.Validation_EndTimeAfterStartTime;
                return false;
            }
        }

        return isValid;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // Validate before saving
        if (!Validate())
        {
            return;
        }

        var startTime = TimeOnly.Parse(StartTimeText);
        TimeOnly? endTime = string.IsNullOrWhiteSpace(EndTimeText)
            ? null
            : TimeOnly.Parse(EndTimeText);

        var record = new TimeRecord
        {
            Id = _recordId,
            ActivityId = ActivityId,
            Date = DateOnly.FromDateTime(Date),
            StartTime = startTime,
            EndTime = endTime,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes
        };

        try
        {
            if (_isNewRecord)
            {
                await _timeRecordRepository.AddAsync(record);
            }
            else
            {
                await _timeRecordRepository.UpdateAsync(record);
            }

            _navigationService.GoBack();
        }
        catch (Exception)
        {
            // In case of unexpected error, show generic message
            TimeError = Resources.Resources.Validation_RecordSaveError;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.GoBack();
    }

    [RelayCommand]
    private void RequestDelete()
    {
        if (_isNewRecord) return;
        IsDeleteConfirmationOpen = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmationOpen = false;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        await _timeRecordRepository.DeleteAsync(_recordId);
        IsDeleteConfirmationOpen = false;
        _navigationService.GoBack();
    }
}
