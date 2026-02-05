using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;
using Wpf.Ui.Controls;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Page to manage the workday with calendar and time slots.
/// </summary>
public partial class JornadaPage : Page
{
    private readonly JornadaViewModel _viewModel;
    private readonly IDialogService _dialogService;
    private readonly IBreadcrumbService _breadcrumbService;
    private ContentDialog? _slotDialog;
    private ContentDialog? _configDialog;
    private bool _isSlotDialogVisible;
    private bool _isConfigDialogVisible;
    private bool _isSubscribedToChanges;
    private bool _isUpdatingCalendar;

    public JornadaPage(JornadaViewModel viewModel, IDialogService dialogService, IBreadcrumbService breadcrumbService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
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

        DisposeDialog();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(JornadaViewModel.IsDialogOpen))
        {
            if (_viewModel.IsDialogOpen)
            {
                _ = ShowSlotDialogAsync();
            }
            else
            {
                _slotDialog?.Hide();
            }
        }
        else if (e.PropertyName == nameof(JornadaViewModel.IsConfigDialogOpen))
        {
            if (_viewModel.IsConfigDialogOpen)
            {
                _ = ShowConfigDialogAsync();
            }
            else
            {
                _configDialog?.Hide();
            }
        }
        else if (e.PropertyName == nameof(JornadaViewModel.SelectedDate))
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

    private async Task ShowSlotDialogAsync()
    {
        if (_slotDialog == null)
        {
            _slotDialog = CreateDialog();
            _slotDialog.Closed += OnSlotDialogClosed;
        }

        if (_isSlotDialogVisible)
        {
            return;
        }

        try
        {
            _isSlotDialogVisible = true;
            await _slotDialog.ShowAsync();
        }
        finally
        {
            _isSlotDialogVisible = false;
        }
    }

    private ContentDialog CreateDialog()
    {
        var content = CreateDialogContent();
        var dialogHost = _dialogService.GetDialogHost();
        
        return new ContentDialog(dialogHost)
        {
            Content = content
        };
    }

    private FrameworkElement CreateDialogContent()
    {
        if (Resources["SlotDialogTemplate"] is DataTemplate template && template.LoadContent() is FrameworkElement element)
        {
            element.DataContext = _viewModel;
            return element;
        }

        throw new InvalidOperationException("Dialog template 'SlotDialogTemplate' not found.");
    }

    private void DisposeDialog()
    {
        if (_slotDialog != null)
        {
            _slotDialog.Closed -= OnSlotDialogClosed;
            _slotDialog.Hide();
            _slotDialog = null;
            _isSlotDialogVisible = false;
        }

        if (_configDialog != null)
        {
            _configDialog.Closed -= OnConfigDialogClosed;
            _configDialog.Hide();
            _configDialog = null;
            _isConfigDialogVisible = false;
        }
    }

    private void OnSlotDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _viewModel.IsDialogOpen = false;
        _isSlotDialogVisible = false;
    }

    private async Task ShowConfigDialogAsync()
    {
        if (_configDialog == null)
        {
            _configDialog = BuildConfigDialog();
            _configDialog.Closed += OnConfigDialogClosed;
        }

        if (_isConfigDialogVisible)
        {
            return;
        }

        try
        {
            _isConfigDialogVisible = true;
            await _configDialog.ShowAsync();
        }
        finally
        {
            _isConfigDialogVisible = false;
        }
    }

    private ContentDialog BuildConfigDialog()
    {
        var dialogContent = LoadConfigDialogContent();
        var host = _dialogService.GetDialogHost();
        
        return new ContentDialog(host)
        {
            Content = dialogContent
        };
    }

    private FrameworkElement LoadConfigDialogContent()
    {
        if (Resources["ConfigDayDialogTemplate"] is DataTemplate templateResource && 
            templateResource.LoadContent() is FrameworkElement contentElement)
        {
            contentElement.DataContext = _viewModel;
            return contentElement;
        }

        throw new InvalidOperationException("Dialog template 'ConfigDayDialogTemplate' not found.");
    }

    private void OnConfigDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _viewModel.IsConfigDialogOpen = false;
        _isConfigDialogVisible = false;
    }
}
