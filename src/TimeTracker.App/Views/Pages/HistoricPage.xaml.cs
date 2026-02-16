namespace TimeTracker.App.Views.Pages;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;
using Wpf.Ui.Controls;

/// <summary>
/// Page for time records management.
/// </summary>
public partial class HistoricPage : Page
{
    private readonly HistoricViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly IDialogService _dialogService;
    private ContentDialog? _editRecordDialog;
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
            TimeTracker.App.Resources.Resources.Nav_Records
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

        var template = (DataTemplate)Resources["EditRecordDialogTemplate"];
        var content = template.LoadContent();
        ((FrameworkElement)content).DataContext = _viewModel;

        _editRecordDialog = new ContentDialog(dialogHost)
        {
            Content = content
        };

        await _editRecordDialog.ShowAsync();
        DisposeDialogs();
    }

    private void DisposeDialogs()
    {
        _editRecordDialog = null;
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
