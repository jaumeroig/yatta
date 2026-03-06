namespace Yatta.App.Models;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Yatta.Core.Models;
using AppResources = Yatta.App.Resources.Resources;

/// <summary>
/// Model for the edit record dialog.
/// </summary>
public partial class TimeRecordEditModel : ObservableObject
{
    [ObservableProperty]
    private string _dialogTitle = string.Empty;

    [ObservableProperty]
    private Guid _recordId;

    [ObservableProperty]
    private bool _isNewRecord;

    [ObservableProperty]
    private ObservableCollection<Activity> _availableActivities = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private Guid _selectedActivityId;

    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _startTimeText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _endTimeText = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _telework;

    [ObservableProperty]
    private string _validationError = string.Empty;

    /// <summary>
    /// Inverse of Telework for radio button binding (Office selected).
    /// </summary>
    public bool IsOffice
    {
        get => !Telework;
        set
        {
            if (value != !Telework)
            {
                Telework = !value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if the record can be saved (basic validation).
    /// </summary>
    public bool CanSave => SelectedActivityId != Guid.Empty &&
                           TimeOnly.TryParse(StartTimeText, out _) &&
                           (string.IsNullOrWhiteSpace(EndTimeText) || TimeOnly.TryParse(EndTimeText, out _));

    /// <summary>
    /// Validates the record data.
    /// </summary>
    public bool Validate()
    {
        ValidationError = string.Empty;

        if (SelectedActivityId == Guid.Empty)
        {
            ValidationError = AppResources.Validation_ActivityRequired;
            return false;
        }

        if (!TimeOnly.TryParse(StartTimeText, out var startTime))
        {
            ValidationError = AppResources.Validation_InvalidStartTime;
            return false;
        }

        if (!string.IsNullOrWhiteSpace(EndTimeText))
        {
            if (!TimeOnly.TryParse(EndTimeText, out var parsedEndTime))
            {
                ValidationError = AppResources.Validation_InvalidEndTime;
                return false;
            }

            if (parsedEndTime <= startTime)
            {
                ValidationError = AppResources.Validation_EndTimeAfterStartTime;
                return false;
            }
        }

        return true;
    }
}