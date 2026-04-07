using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Yatta.App.Services;
using Yatta.App.ViewModels;
using Yatta.App.Views.Dialogs;
using Wpf.Ui.Controls;
using AppResources = Yatta.App.Resources.Resources;

namespace Yatta.App.Views.Pages;

/// <summary>
/// Dashboard page for yearly statistics and breakdown.
/// </summary>
public partial class DashboardYearPage : Page
{
    private readonly DashboardYearViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private ContentDialog? _configureQuotaDialog;
    private bool _isSubscribedToChanges;

    public DashboardYearPage(
        DashboardYearViewModel viewModel,
        IBreadcrumbService breadcrumbService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _breadcrumbService = breadcrumbService ?? throw new ArgumentNullException(nameof(breadcrumbService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _breadcrumbService.SetItems(
            new BreadcrumbItem(Yatta.App.Resources.Resources.Nav_Dashboard, () => _navigationService.Navigate<DashboardIndexPage>()),
            new BreadcrumbItem(Yatta.App.Resources.Resources.Dashboard_Year)
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

        DisposeConfigureQuotaDialog();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardYearViewModel.IsConfigureYearQuotaDialogOpen))
        {
            if (_viewModel.IsConfigureYearQuotaDialogOpen)
            {
                _ = ShowConfigureQuotaDialogAsync();
            }
            else
            {
                _configureQuotaDialog?.Hide();
            }
        }
    }

    private async Task ShowConfigureQuotaDialogAsync()
    {
        if (_configureQuotaDialog != null)
        {
            return;
        }

        var dialogHost = _dialogService.GetDialogHost();
        if (dialogHost == null)
        {
            return;
        }

        var content = new ConfigureYearQuotaDialogControl
        {
            DataContext = _viewModel
        };

        _configureQuotaDialog = new ContentDialog(dialogHost)
        {
            Content = content,
            Title = _viewModel.ConfigureYearQuotaModel.DialogTitle,
            PrimaryButtonText = AppResources.Button_SaveChanges,
            CloseButtonText = AppResources.Button_Cancel,
            DefaultButton = ContentDialogButton.Primary,
        };

        _configureQuotaDialog.ButtonClicked += OnConfigureQuotaDialogButtonClicked;

        await _configureQuotaDialog.ShowAsync();
        _viewModel.IsConfigureYearQuotaDialogOpen = false;
        DisposeConfigureQuotaDialog();
    }

    private async void OnConfigureQuotaDialogButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (args.Button == ContentDialogButton.Primary)
        {
            args.Handled = true;
            await _viewModel.SaveConfigureQuotaCommand.ExecuteAsync(null);
        }
    }

    private void DisposeConfigureQuotaDialog()
    {
        if (_configureQuotaDialog != null)
        {
            _configureQuotaDialog.ButtonClicked -= OnConfigureQuotaDialogButtonClicked;
        }

        _configureQuotaDialog = null;
    }
}
