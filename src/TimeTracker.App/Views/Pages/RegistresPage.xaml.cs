using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.Services;
using TimeTracker.App.ViewModels;
using Wpf.Ui.Controls;

namespace TimeTracker.App.Views.Pages;

public partial class RegistresPage : Page
{
    private readonly RegistresViewModel _viewModel;
    private readonly IDialogService _dialogService;
    private ContentDialog? _recordDialog;
    private bool _isRecordDialogVisible;
    private bool _isSubscribedToChanges;

    public RegistresPage(RegistresViewModel viewModel, IDialogService dialogService)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
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
        if (e.PropertyName == nameof(RegistresViewModel.IsDialogOpen))
        {
            if (_viewModel.IsDialogOpen)
            {
                _ = ShowRecordDialogAsync();
            }
            else
            {
                _recordDialog?.Hide();
            }
        }
    }

    private async Task ShowRecordDialogAsync()
    {
        if (_recordDialog == null)
        {
            _recordDialog = CreateDialog();
            _recordDialog.Closed += OnRecordDialogClosed;
        }

        if (_isRecordDialogVisible)
        {
            return;
        }

        try
        {
            _isRecordDialogVisible = true;
            await _recordDialog.ShowAsync();
        }
        finally
        {
            _isRecordDialogVisible = false;
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
        if (Resources["RecordDialogTemplate"] is DataTemplate template && template.LoadContent() is FrameworkElement element)
        {
            element.DataContext = _viewModel;
            return element;
        }

        throw new InvalidOperationException("Dialog template 'RecordDialogTemplate' not found.");
    }

    private void DisposeDialog()
    {
        if (_recordDialog == null)
        {
            return;
        }

        _recordDialog.Closed -= OnRecordDialogClosed;
        _recordDialog.Hide();
        _recordDialog = null;
        _isRecordDialogVisible = false;
    }

    private void OnRecordDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _viewModel.IsDialogOpen = false;
        _isRecordDialogVisible = false;
    }
}
