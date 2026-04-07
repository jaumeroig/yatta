namespace Yatta.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using AppResources = Yatta.App.Resources.Resources;

/// <summary>
/// Model for the Configure Year Quota dialog.
/// </summary>
public partial class ConfigureYearQuotaModel : ObservableObject
{
    [ObservableProperty]
    private int _year;

    [ObservableProperty]
    private int _vacationDays;

    [ObservableProperty]
    private int _freeChoiceDays;

    [ObservableProperty]
    private int _intensiveDays;

    [ObservableProperty]
    private string _validationError = string.Empty;
}
