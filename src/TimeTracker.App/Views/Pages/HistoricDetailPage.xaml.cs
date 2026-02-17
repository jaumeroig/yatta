namespace TimeTracker.App.Views.Pages;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Models;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;
using Wpf.Ui.Controls;

/// <summary>
/// Detail page to edit or create a time record.
/// </summary>
public partial class HistoricDetailPage : Page
{
    private readonly HistoricDetailViewModel _viewModel;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private ContentDialog? _deleteDialog;
    private bool _isDeleteDialogVisible;
    private bool _isSubscribedToChanges;

    public HistoricDetailPage(
        HistoricDetailViewModel viewModel,
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

        // Get navigation parameter (can be Guid? or HistoricNavigationParameter)
        Guid? recordId = null;
        var fromNotification = false;

        if (_navigationService.CurrentParameter is HistoricNavigationParameter navParam)
        {
            recordId = navParam.RecordId;
            fromNotification = navParam.FromNotification;
        }
        else if (_navigationService.CurrentParameter is Guid guid)
        {
            recordId = guid;
        }

        await _viewModel.InitializeAsync(recordId, fromNotification);

        // Focus on EndTime field if coming from notification
        if (_viewModel.ShouldFocusEndTime)
        {
            EndTimeTextBox.Focus();
            EndTimeTextBox.SelectAll();
            _viewModel.ShouldFocusEndTime = false;
        }
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_isSubscribedToChanges)
        {
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _isSubscribedToChanges = false;
        }

        DisposeDialog(ref _deleteDialog, OnDeleteDialogClosed);
        _isDeleteDialogVisible = false;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HistoricDetailViewModel.IsDeleteConfirmationOpen))
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
        if (_deleteDialog == null)
        {
            _deleteDialog = CreateDialog("DeleteRecordDialogTemplate");
            _deleteDialog.Closed += OnDeleteDialogClosed;
        }

        if (_isDeleteDialogVisible)
        {
            return;
        }

        try
        {
            _isDeleteDialogVisible = true;
            await _deleteDialog.ShowAsync();
        }
        finally
        {
            _isDeleteDialogVisible = false;
        }
    }

    private ContentDialog CreateDialog(string templateKey)
    {
        var content = CreateDialogContent(templateKey);
        var dialogHost = _dialogService.GetDialogHost();
        
        return new ContentDialog(dialogHost)
        {
            Content = content
        };
    }

    private FrameworkElement CreateDialogContent(string templateKey)
    {
        if (Resources[templateKey] is DataTemplate template && template.LoadContent() is FrameworkElement element)
        {
            element.DataContext = _viewModel;
            return element;
        }

        throw new InvalidOperationException($"Dialog template '{templateKey}' not found.");
    }

    private void DisposeDialog(ref ContentDialog? dialog, TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> closedHandler)
    {
        if (dialog == null)
        {
            return;
        }

        dialog.Closed -= closedHandler;
        dialog.Hide();
        dialog = null;
    }

    private void OnDeleteDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _viewModel.IsDeleteConfirmationOpen = false;
        _isDeleteDialogVisible = false;
    }
}
