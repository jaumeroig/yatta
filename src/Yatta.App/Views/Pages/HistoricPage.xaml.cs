namespace Yatta.App.Views.Pages;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Yatta.App.Models;
using Yatta.App.Services;
using Yatta.App.ViewModels;
using Yatta.App.Views.Dialogs;
using Wpf.Ui.Controls;
using AppResources = Yatta.App.Resources.Resources;

/// <summary>
/// Page for time records management.
/// </summary>
public partial class HistoricPage : Page
{
    private readonly HistoricViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly IDialogService _dialogService;
    private ContentDialog? _editRecordDialog;
    private ContentDialog? _deleteConfirmationDialog;
    private ContentDialog? _configureDayDialog;
    private bool _isSubscribedToChanges;

    public HistoricPage(HistoricViewModel viewModel, IBreadcrumbService breadcrumbService, IDialogService dialogService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _breadcrumbService = breadcrumbService ?? throw new ArgumentNullException(nameof(breadcrumbService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Configure the breadcrumb with the page title
        _breadcrumbService.SetItems(
            Yatta.App.Resources.Resources.Nav_Historic
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

        DisposeDialogs();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HistoricViewModel.IsEditRecordDialogOpen))
        {
            if (_viewModel.IsEditRecordDialogOpen)
            {
                _ = ShowEditRecordDialogAsync();
            }
            else
            {
                _editRecordDialog?.Hide();
            }
        }
        else if (e.PropertyName == nameof(HistoricViewModel.IsDeleteConfirmationOpen))
        {
            if (_viewModel.IsDeleteConfirmationOpen)
            {
                _ = ShowDeleteConfirmationDialogAsync();
            }
            else
            {
                _deleteConfirmationDialog?.Hide();
            }
        }
        else if (e.PropertyName == nameof(HistoricViewModel.IsConfigureDayDialogOpen))
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
    }

    private async Task ShowEditRecordDialogAsync()
    {
        if (_editRecordDialog != null)
        {
            return;
        }

        var dialogHost = _dialogService.GetDialogHost();
        if (dialogHost == null)
        {
            return;
        }

        var content = new EditRecordDialogControl
        {
            DataContext = _viewModel
        };

        _editRecordDialog = new ContentDialog(dialogHost)
        {
            Content = content,
            CloseButtonText = AppResources.Button_Cancel,
            PrimaryButtonText = AppResources.Button_Save,
            IsPrimaryButtonEnabled = _viewModel.EditRecordModel.CanSave,
        };

        _viewModel.EditRecordModel.PropertyChanged += OnEditRecordModelPropertyChanged;
        _editRecordDialog.ButtonClicked += OnEditRecordDialogButtonClicked;

        await _editRecordDialog.ShowAsync();
        _viewModel.IsEditRecordDialogOpen = false;
        DisposeEditDialog();
    }

    private void OnEditRecordModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeRecordEditModel.CanSave) && _editRecordDialog != null)
        {
            _editRecordDialog.IsPrimaryButtonEnabled = _viewModel.EditRecordModel.CanSave;
        }
    }

    private async void OnEditRecordDialogButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (args.Button == ContentDialogButton.Primary)
        {
            args.Handled = true;
            await _viewModel.SaveEditRecordCommand.ExecuteAsync(null);
        }
    }

    private async Task ShowDeleteConfirmationDialogAsync()
    {
        if (_deleteConfirmationDialog != null)
        {
            return;
        }

        var dialogHost = _dialogService.GetDialogHost();
        if (dialogHost == null)
        {
            return;
        }

        var content = new DeleteRecordConfirmationDialogControl();

        _deleteConfirmationDialog = new ContentDialog(dialogHost)
        {
            Content = content,
            CloseButtonText = AppResources.Button_CancelDelete,
            PrimaryButtonText = AppResources.Button_Delete,
            PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger,
        };

        _deleteConfirmationDialog.ButtonClicked += OnDeleteConfirmationDialogButtonClicked;

        await _deleteConfirmationDialog.ShowAsync();
        _viewModel.CancelDeleteRecordCommand.Execute(null);
        DisposeDeleteDialog();
    }

    private async void OnDeleteConfirmationDialogButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (args.Button == ContentDialogButton.Primary)
        {
            args.Handled = true;
            await _viewModel.ConfirmDeleteRecordCommand.ExecuteAsync(null);
        }
    }

    private void DisposeDialogs()
    {
        DisposeEditDialog();
        DisposeDeleteDialog();
        DisposeConfigureDayDialog();
    }

    private void DisposeEditDialog()
    {
        if (_editRecordDialog != null)
        {
            _editRecordDialog.ButtonClicked -= OnEditRecordDialogButtonClicked;
        }

        _viewModel.EditRecordModel.PropertyChanged -= OnEditRecordModelPropertyChanged;
        _editRecordDialog = null;
    }

    private void DisposeDeleteDialog()
    {
        if (_deleteConfirmationDialog != null)
        {
            _deleteConfirmationDialog.ButtonClicked -= OnDeleteConfirmationDialogButtonClicked;
        }

        _deleteConfirmationDialog = null;
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
            Content = content,
            Title = _viewModel.ConfigureDayModel.DialogTitle,
            PrimaryButtonText = AppResources.Button_SaveChanges,
            CloseButtonText = AppResources.Button_Cancel,
            DefaultButton = ContentDialogButton.Primary,
        };

        _configureDayDialog.ButtonClicked += OnConfigureDayDialogButtonClicked;

        await _configureDayDialog.ShowAsync();
        _viewModel.IsConfigureDayDialogOpen = false;
        DisposeConfigureDayDialog();
    }

    private async void OnConfigureDayDialogButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (args.Button == ContentDialogButton.Primary)
        {
            args.Handled = true;
            await _viewModel.SaveConfigureDayCommand.ExecuteAsync(null);
        }
    }

    private void DisposeConfigureDayDialog()
    {
        if (_configureDayDialog != null)
        {
            _configureDayDialog.ButtonClicked -= OnConfigureDayDialogButtonClicked;
        }

        _configureDayDialog = null;
    }

    private void RecordMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
    }
}
