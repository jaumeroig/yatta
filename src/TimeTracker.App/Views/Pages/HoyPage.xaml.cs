namespace TimeTracker.App.Views.Pages;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;
using Wpf.Ui.Controls;

/// <summary>
/// Interaction logic for HoyPage.xaml
/// </summary>
public partial class HoyPage : Page
{
    private readonly HoyViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly IDialogService _dialogService;
    private ContentDialog? _configureDayDialog;
    private ContentDialog? _changeActivityDialog;
    private bool _isSubscribedToChanges;

    public HoyPage(HoyViewModel viewModel, IBreadcrumbService breadcrumbService, IDialogService dialogService)
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
        if (e.PropertyName == nameof(HoyViewModel.IsConfigureDayDialogOpen))
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
        else if (e.PropertyName == nameof(HoyViewModel.IsChangeActivityDialogOpen))
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

        var template = (DataTemplate)Resources["ConfigureDayDialogTemplate"];
        var content = template.LoadContent();
        ((FrameworkElement)content).DataContext = _viewModel;

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
            return;
        }

        var dialogHost = _dialogService.GetDialogHost();
        if (dialogHost == null)
        {
            return;
        }

        var template = (DataTemplate)Resources["ChangeActivityDialogTemplate"];
        var content = template.LoadContent();
        ((FrameworkElement)content).DataContext = _viewModel;

        _changeActivityDialog = new ContentDialog(dialogHost)
        {
            Content = content
        };

        await _changeActivityDialog.ShowAsync();
        DisposeDialogs();
    }

    private void DisposeDialogs()
    {
        _configureDayDialog = null;
        _changeActivityDialog = null;
    }
}
