using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Page to manage the workday with calendar and time records (read-only view).
/// </summary>
public partial class JornadaPage : Page
{
    private readonly JornadaViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private bool _isSubscribedToChanges;
    private bool _isUpdatingCalendar;

    public JornadaPage(JornadaViewModel viewModel, IBreadcrumbService breadcrumbService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _breadcrumbService = breadcrumbService ?? throw new ArgumentNullException(nameof(breadcrumbService));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Configurar el breadcrumb amb el títol de la pàgina
        _breadcrumbService.SetItems(
            TimeTracker.App.Resources.Resources.Nav_Workday
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
        if (e.PropertyName == nameof(JornadaViewModel.SelectedDate))
        {
            // Sincronitzar el calendari quan canvia la data des del ViewModel
            if (!_isUpdatingCalendar && WorkdayCalendar.SelectedDate != _viewModel.SelectedDate)
            {
                _isUpdatingCalendar = true;
                WorkdayCalendar.SelectedDate = _viewModel.SelectedDate;
                WorkdayCalendar.DisplayDate = _viewModel.SelectedDate;
                _isUpdatingCalendar = false;
            }
        }
    }

    /// <summary>
    /// Gestiona el canvi de data seleccionada al calendari.
    /// </summary>
    private void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingCalendar)
        {
            return;
        }

        if (WorkdayCalendar.SelectedDate.HasValue)
        {
            _isUpdatingCalendar = true;
            _viewModel.SelectedDate = WorkdayCalendar.SelectedDate.Value;
            _isUpdatingCalendar = false;
        }
    }

    /// <summary>
    /// Gestiona el canvi de mes visualitzat al calendari.
    /// </summary>
    private void Calendar_DisplayDateChanged(object? sender, CalendarDateChangedEventArgs e)
    {
        // Ignorar events durant la inicialització (abans de Page_Loaded)
        if (!_isSubscribedToChanges || _isUpdatingCalendar || !e.AddedDate.HasValue)
        {
            return;
        }

        // Actualitzar el resum mensual quan canvia el mes visualitzat
        var newDate = e.AddedDate.Value;
        if (newDate.Year != _viewModel.SelectedDate.Year || 
            newDate.Month != _viewModel.SelectedDate.Month)
        {
            // Si el mes canvia, actualitzar el ViewModel per refrescar el resum mensual
            _viewModel.SelectedDate = newDate;
        }
    }
}
