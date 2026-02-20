using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Dashboard page for daily statistics and breakdown.
/// </summary>
public partial class DashboardDayPage : Page
{
    private readonly DashboardDayViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly INavigationService _navigationService;
    private bool _isSubscribedToChanges;
    private bool _isUpdatingCalendar;

    public DashboardDayPage(DashboardDayViewModel viewModel, IBreadcrumbService breadcrumbService, INavigationService navigationService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _breadcrumbService = breadcrumbService ?? throw new ArgumentNullException(nameof(breadcrumbService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _breadcrumbService.SetItems(
            new BreadcrumbItem(TimeTracker.App.Resources.Resources.Nav_Dashboard, () => _navigationService.Navigate<DashboardIndexPage>()),
            new BreadcrumbItem(TimeTracker.App.Resources.Resources.Dashboard_Day)
        );

        if (!_isSubscribedToChanges)
        {
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            _isSubscribedToChanges = true;
        }

        await _viewModel.LoadDataAsync();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_isSubscribedToChanges)
        {
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _isSubscribedToChanges = false;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardDayViewModel.SelectedDate))
        {
            if (!_isUpdatingCalendar && DashboardCalendar.SelectedDate != _viewModel.SelectedDate)
            {
                _isUpdatingCalendar = true;
                DashboardCalendar.SelectedDate = _viewModel.SelectedDate;
                DashboardCalendar.DisplayDate = _viewModel.SelectedDate;
                _isUpdatingCalendar = false;
            }
        }
    }

    private async void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingCalendar) return;

        if (DashboardCalendar.SelectedDate.HasValue && DashboardCalendar.SelectedDate.Value != _viewModel.SelectedDate)
        {
            _isUpdatingCalendar = true;
            await _viewModel.SelectDateCommand.ExecuteAsync(DashboardCalendar.SelectedDate.Value);
            _isUpdatingCalendar = false;
        }
    }

    private async void Calendar_DisplayDateChanged(object? sender, CalendarDateChangedEventArgs e)
    {
        // Guard: event fires during InitializeComponent before _viewModel is assigned
        if (_viewModel is null) return;

        // Reload calendar indicators when month changes
        if (e.AddedDate.HasValue)
        {
            _isUpdatingCalendar = true;
            _viewModel.SelectedDate = e.AddedDate.Value;
            await _viewModel.LoadDataAsync();
            _isUpdatingCalendar = false;
        }
    }
}
