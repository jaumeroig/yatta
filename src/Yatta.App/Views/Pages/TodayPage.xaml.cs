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
/// Interaction logic for TodayPage.xaml
/// </summary>
public partial class TodayPage : Page
{
    private readonly TodayViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly IDialogService _dialogService;
    private ContentDialog? _configureDayDialog;
    private ContentDialog? _changeActivityDialog;
    private ContentDialog? _editRecordDialog;
    private ContentDialog? _deleteConfirmationDialog;
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
        _breadcrumbService.SetItems(Yatta.App.Resources.Resources.Page_Today_Title);
        
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
        else if (e.PropertyName == nameof(TodayViewModel.IsEditRecordDialogOpen))
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
        else if (e.PropertyName == nameof(TodayViewModel.IsDeleteConfirmationOpen))
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
            Content = content,
            Title = _viewModel.ChangeActivityModel.DialogTitle,
            PrimaryButtonText = _viewModel.ChangeActivityModel.PrimaryButtonText,
            CloseButtonText = AppResources.Button_Cancel,
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = _viewModel.ChangeActivityModel.HasChanges,
        };

        _viewModel.ChangeActivityModel.PropertyChanged += OnChangeActivityModelPropertyChanged;
        _changeActivityDialog.ButtonClicked += OnChangeActivityDialogButtonClicked;

        await _changeActivityDialog.ShowAsync();
        _viewModel.IsChangeActivityDialogOpen = false;
        DisposeChangeActivityDialog();
    }

    private void OnChangeActivityModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_changeActivityDialog == null) return;

        if (e.PropertyName == nameof(ChangeActivityModel.HasChanges))
        {
            _changeActivityDialog.IsPrimaryButtonEnabled = _viewModel.ChangeActivityModel.HasChanges;
        }
        else if (e.PropertyName == nameof(ChangeActivityModel.PrimaryButtonText))
        {
            _changeActivityDialog.PrimaryButtonText = _viewModel.ChangeActivityModel.PrimaryButtonText;
        }
        else if (e.PropertyName == nameof(ChangeActivityModel.DialogTitle))
        {
            _changeActivityDialog.Title = _viewModel.ChangeActivityModel.DialogTitle;
        }
    }

    private async void OnChangeActivityDialogButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (args.Button == ContentDialogButton.Primary)
        {
            args.Handled = true;
            await _viewModel.SaveChangeActivityCommand.ExecuteAsync(null);
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
            PrimaryButtonAppearance = ControlAppearance.Danger,
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
        DisposeConfigureDayDialog();
        DisposeChangeActivityDialog();
        DisposeEditDialog();
        DisposeDeleteDialog();
    }

    private void DisposeConfigureDayDialog()
    {
        if (_configureDayDialog != null)
        {
            _configureDayDialog.ButtonClicked -= OnConfigureDayDialogButtonClicked;
        }

        _configureDayDialog = null;
    }

    private void DisposeChangeActivityDialog()
    {
        if (_changeActivityDialog != null)
        {
            _changeActivityDialog.ButtonClicked -= OnChangeActivityDialogButtonClicked;
        }

        _viewModel.ChangeActivityModel.PropertyChanged -= OnChangeActivityModelPropertyChanged;
        _changeActivityDialog = null;
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
}
