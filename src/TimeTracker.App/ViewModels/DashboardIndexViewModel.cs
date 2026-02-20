namespace TimeTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.App.Services;

/// <summary>
/// ViewModel for the Dashboard index page that shows the four period options.
/// </summary>
public partial class DashboardIndexViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public DashboardIndexViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>
    /// Navigates to the Day dashboard page.
    /// </summary>
    [RelayCommand]
    private void NavigateToDay()
    {
        _navigationService.Navigate<Views.Pages.DashboardDayPage>();
    }

    /// <summary>
    /// Navigates to the Week dashboard page.
    /// </summary>
    [RelayCommand]
    private void NavigateToWeek()
    {
        _navigationService.Navigate<Views.Pages.DashboardWeekPage>();
    }

    /// <summary>
    /// Navigates to the Month dashboard page.
    /// </summary>
    [RelayCommand]
    private void NavigateToMonth()
    {
        _navigationService.Navigate<Views.Pages.DashboardMonthPage>();
    }

    /// <summary>
    /// Navigates to the Year dashboard page.
    /// </summary>
    [RelayCommand]
    private void NavigateToYear()
    {
        _navigationService.Navigate<Views.Pages.DashboardYearPage>();
    }
}
