using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using TimeTracker.App.Services;

namespace TimeTracker.App.ViewModels;

/// <summary>
/// ViewModel per a la pàgina de detall d'un registre de temps.
/// </summary>
public partial class RecordDetailViewModel : ObservableObject
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

    /// <summary>
    /// Indica si el registre ja existeix (no és nou).
    /// </summary>
    public bool IsExistingRecord => !_isNewRecord;

    /// <summary>
    /// Indica si es pot desar el registre (validació bàsica).
    /// L'hora de fi és opcional (pot estar buida).
    /// </summary>
    public bool CanSave => ActivityId != Guid.Empty && 
                           TimeOnly.TryParse(StartTimeText, out _) && 
                           (string.IsNullOrWhiteSpace(EndTimeText) || TimeOnly.TryParse(EndTimeText, out _));

    public RecordDetailViewModel(
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
    /// Inicialitza el ViewModel amb les dades del registre.
    /// </summary>
    public async Task InitializeAsync(Guid? recordId)
    {
        // Carregar activitats actives
        var activeActivities = await _activityRepository.GetActiveAsync();
        Activities = new ObservableCollection<Activity>(activeActivities);

        if (recordId.HasValue && recordId.Value != Guid.Empty)
        {
            _recordId = recordId.Value;
            _isNewRecord = false;
            OnPropertyChanged(nameof(IsExistingRecord));
            await LoadRecordAsync();
        }
        else
        {
            _recordId = Guid.NewGuid();
            _isNewRecord = true;
            OnPropertyChanged(nameof(IsExistingRecord));
            PageTitle = Resources.Resources.RecordDetail_NewTitle;
            ActivityId = Guid.Empty;
            Date = DateTime.Today;
            StartTimeText = "09:00";
            EndTimeText = "10:00";
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

        // Assegurem que l'activitat del registre estigui disponible per seleccionar
        // (pot ser que l'activitat estigui inactiva)
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
    /// Actualitza els elements del breadcrumb.
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
    /// Neteja tots els errors de validació.
    /// </summary>
    private void ClearErrors()
    {
        ActivityError = string.Empty;
        HasActivityError = false;
        TimeError = string.Empty;
    }

    /// <summary>
    /// Valida les dades del registre.
    /// </summary>
    /// <returns>True si les dades són vàlides, false en cas contrari.</returns>
    private bool Validate()
    {
        ClearErrors();
        var isValid = true;
        
        // Validar activitat seleccionada
        if (ActivityId == Guid.Empty)
        {
            ActivityError = Resources.Resources.Validation_ActivityRequired;
            HasActivityError = true;
            isValid = false;
        }
        
        // Validar hora d'inici
        if (!TimeOnly.TryParse(StartTimeText, out var startTime))
        {
            TimeError = Resources.Resources.Validation_InvalidStartTime;
            return false;
        }
        
        // Validar hora de fi (opcional si està buida)
        TimeOnly? endTime = null;
        if (!string.IsNullOrWhiteSpace(EndTimeText))
        {
            if (!TimeOnly.TryParse(EndTimeText, out var parsedEndTime))
            {
                TimeError = Resources.Resources.Validation_InvalidEndTime;
                return false;
            }
            endTime = parsedEndTime;
            
            // Validar que l'hora de fi sigui posterior a l'hora d'inici
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
        // Validar abans de desar
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
            // En cas d'error inesperat, mostrar missatge genèric
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
