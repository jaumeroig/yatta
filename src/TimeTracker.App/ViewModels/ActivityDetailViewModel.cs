using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Models;
using TimeTracker.App.Services;

namespace TimeTracker.App.ViewModels;

/// <summary>
/// ViewModel per a la pàgina de detall d'una activitat.
/// </summary>
public partial class ActivityDetailViewModel : ObservableObject
{
    private readonly IActivityRepository _activityRepository;
    private readonly ITimeRecordRepository _timeRecordRepository;
    private readonly ITimeCalculatorService _timeCalculatorService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private Guid _activityId;
    private bool _isNewActivity;
    private string _originalName = string.Empty;

    /// <summary>
    /// Longitud màxima permesa per al nom de l'activitat.
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
    /// Indica si es pot desar l'activitat (validació bàsica).
    /// </summary>
    public bool CanSave => !string.IsNullOrWhiteSpace(Name) && Name.Length <= MaxNameLength;

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

    public ActivityDetailViewModel(
        IActivityRepository activityRepository,
        ITimeRecordRepository timeRecordRepository,
        ITimeCalculatorService timeCalculatorService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _activityRepository = activityRepository;
        _timeRecordRepository = timeRecordRepository;
        _timeCalculatorService = timeCalculatorService;
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Inicialitza el ViewModel amb les dades de l'activitat.
    /// </summary>
    public async Task InitializeAsync(Guid? activityId)
    {
        if (activityId.HasValue && activityId.Value != Guid.Empty)
        {
            _activityId = activityId.Value;
            _isNewActivity = false;
            await LoadActivityAsync();
        }
        else
        {
            _activityId = Guid.NewGuid();
            _isNewActivity = true;
            PageTitle = Resources.Resources.ActivityDetail_NewTitle;
            Name = string.Empty;
            _originalName = string.Empty;
            Color = "#0078D4";
            Active = true;
            RecordCount = 0;
            TotalTime = FormatDuration(0);
        }
        
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

        // Carregar estadístiques
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
    /// Neteja tots els errors de validació.
    /// </summary>
    private void ClearErrors()
    {
        NameError = string.Empty;
        HasNameError = false;
    }

    /// <summary>
    /// Valida el nom de l'activitat.
    /// </summary>
    /// <returns>True si el nom és vàlid, false en cas contrari.</returns>
    private async Task<bool> ValidateNameAsync()
    {
        ClearErrors();
        
        var trimmedName = Name?.Trim() ?? string.Empty;
        
        // Validar que no estigui buit
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            NameError = Resources.Resources.Validation_ActivityNameRequired;
            HasNameError = true;
            return false;
        }
        
        // Validar longitud màxima
        if (trimmedName.Length > MaxNameLength)
        {
            NameError = string.Format(Resources.Resources.Validation_ActivityNameTooLong, MaxNameLength);
            HasNameError = true;
            return false;
        }
        
        // Validar que no existeixi una altra activitat amb el mateix nom
        // (excepte si és la mateixa activitat que estem editant)
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

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // Validar abans de desar
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
            // En cas d'error inesperat (ex: constraint de BD), mostrar missatge genèric
            NameError = Resources.Resources.Validation_ActivitySaveError;
            HasNameError = true;
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
        // Eliminar tots els registres relacionats primer
        var relatedRecords = await _timeRecordRepository.GetByActivityIdAsync(_activityId);
        foreach (var record in relatedRecords)
        {
            await _timeRecordRepository.DeleteAsync(record.Id);
        }

        // Després eliminar l'activitat
        await _activityRepository.DeleteAsync(_activityId);
        
        IsDeleteConfirmationOpen = false;
        _navigationService.GoBack();
    }
}
