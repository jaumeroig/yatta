using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;
using TimeTracker.App.Views.Dialogs;
using Wpf.Ui.Controls;
using AppResources = TimeTracker.App.Resources.Resources;

namespace TimeTracker.App.Views.Pages;

/// <summary>
/// Detail page to edit or create an activity.
/// </summary>
public partial class ActivityDetailPage : Page
{
    private readonly ActivityDetailViewModel _viewModel;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private ContentDialog? _deleteDialog;
    private bool _isDeleteDialogVisible;
    private bool _isSubscribedToChanges;

    public ActivityDetailPage(
        ActivityDetailViewModel viewModel,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_isSubscribedToChanges)
        {
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            _isSubscribedToChanges = true;
        }

        // Obtenir el paràmetre de navegació (l'ID de l'activitat)
        var activityId = _navigationService.CurrentParameter as Guid?;
        await _viewModel.InitializeAsync(activityId);
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_isSubscribedToChanges)
        {
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _isSubscribedToChanges = false;
        }

        DisposeDeleteDialog();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActivityDetailViewModel.IsDeleteConfirmationOpen))
        {
            if (_viewModel.IsDeleteConfirmationOpen)
            {
                _ = ShowDeleteDialogAsync();
            }
            else
            {
                _deleteDialog?.Hide();
            }
        }
    }

    private async Task ShowDeleteDialogAsync()
    {
        if (_isDeleteDialogVisible)
        {
            return;
        }

        var dialogHost = _dialogService.GetDialogHost();
        if (dialogHost == null)
        {
            return;
        }

        if (_deleteDialog == null)
        {
            var content = new DeleteActivityDialogControl
            {
                DataContext = _viewModel
            };

            _deleteDialog = new ContentDialog(dialogHost)
            {
                Content = content,
                CloseButtonText = AppResources.Button_CancelDelete,
                PrimaryButtonText = AppResources.Button_Delete,
                PrimaryButtonAppearance = ControlAppearance.Danger,
            };

            _deleteDialog.ButtonClicked += OnDeleteDialogButtonClicked;
        }

        try
        {
            _isDeleteDialogVisible = true;
            await _deleteDialog.ShowAsync();
        }
        finally
        {
            _isDeleteDialogVisible = false;
            _viewModel.IsDeleteConfirmationOpen = false;
        }
    }

    private async void OnDeleteDialogButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (args.Button == ContentDialogButton.Primary)
        {
            args.Handled = true;
            await _viewModel.ConfirmDeleteCommand.ExecuteAsync(null);
        }
    }

    private void DisposeDeleteDialog()
    {
        if (_deleteDialog != null)
        {
            _deleteDialog.ButtonClicked -= OnDeleteDialogButtonClicked;
            _deleteDialog.Hide();
        }

        _deleteDialog = null;
        _isDeleteDialogVisible = false;
    }
}
