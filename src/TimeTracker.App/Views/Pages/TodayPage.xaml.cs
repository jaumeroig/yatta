namespace TimeTracker.App.Views.Pages;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;
using TimeTracker.App.Views.Dialogs;
using Wpf.Ui.Controls;

/// <summary>
/// Interaction logic for TodayPage.xaml
/// </summary>
public partial class TodayPage : Page
{
    private readonly TodayViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly IDialogService _dialogService;
    private ContentDialog? _configureDayDialog;
    private ContentDialog? _changeActivityDialog;
    private bool _isSubscribedToChanges;

    public TodayPage(TodayViewModel viewModel, IBreadcrumbService breadcrumbService, IDialogService dialogService)
    {
        _viewModel = viewModel;
        _breadcrumbService = breadcrumbService;
        _dialogService = dialogService;
        
        DataContext = _viewModel;
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _breadcrumbService.SetItems(TimeTracker.App.Resources.Resources.Page_Today_Title);
        
        if (!_isSubscribedToChanges)
        {
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            _isSubscribedToChanges = true;
        }
        
        await _viewModel.LoadDataAsync();
    }

    private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isSubscribedToChanges)
        {
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _isSubscribedToChanges = false;
        }

        DisposeDialogs();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TodayViewModel.IsConfigureDayDialogOpen))
        {
            if (_viewModel.IsConfigureDayDialogOpen)
            {
                _ = ShowConfigureDayDialogAsync();
            }
            else
            {
                _configureDayDialog?.Hide();
            }
        }
        else if (e.PropertyName == nameof(TodayViewModel.IsChangeActivityDialogOpen))
        {
            if (_viewModel.IsChangeActivityDialogOpen)
            {
                _ = ShowChangeActivityDialogAsync();
            }
            else
            {
                _changeActivityDialog?.Hide();
            }
        }
    }

    private async Task ShowConfigureDayDialogAsync()
    {
        if (_configureDayDialog != null)
        {
            return;
        }

        var dialogHost = _dialogService.GetDialogHost();
        if (dialogHost == null)
        {
            return;
        }

        var content = new ConfigureDayDialogControl
        {
            DataContext = _viewModel
        };

        _configureDayDialog = new ContentDialog(dialogHost)
        {
            Content = content
        };

        await _configureDayDialog.ShowAsync();
        DisposeDialogs();
    }

    private async Task ShowChangeActivityDialogAsync()
    {
        if (_changeActivityDialog != null)
        {
            // Dialog is already open, just make sure the window is visible
            return;
        }

        var dialogHost = _dialogService.GetDialogHost();
        if (dialogHost == null)
        {
            return;
        }

        var content = new ChangeActivityDialogControl
        {
            DataContext = _viewModel
        };

        _changeActivityDialog = new ContentDialog(dialogHost)
        {
            Content = content
        };

        await _changeActivityDialog.ShowAsync();
        DisposeDialogs();
    }

    /// <summary>
    /// Brings the change activity dialog to the front if it's already open,
    /// or opens it if it's not.
    /// </summary>
    public void BringChangeActivityDialogToFront()
    {
        if (_changeActivityDialog != null && _viewModel.IsChangeActivityDialogOpen)
        {
            // Dialog is already open, focus is already on it
            // The window activation in MainWindow will handle bringing it to front
            return;
        }

        // Open the dialog if it's not already open
        if (!_viewModel.IsChangeActivityDialogOpen)
        {
            _viewModel.ChangeActivityCommand.Execute(null);
        }
    }

    private void DisposeDialogs()
    {
        _configureDayDialog = null;
        _changeActivityDialog = null;
    }
}
