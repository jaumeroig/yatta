using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.App.ViewModels;
using Wpf.Ui.Controls;

namespace TimeTracker.App.Views.Pages;

public partial class ActivitatsPage : Page
{
    private readonly ActivitatsViewModel _viewModel;
    private ContentDialog? _activityDialog;
    private ContentDialog? _deleteDialog;
    private bool _isActivityDialogVisible;
    private bool _isDeleteDialogVisible;
    private bool _isSubscribedToChanges;

    public ActivitatsPage(ActivitatsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = viewModel;
    }

    private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
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

        DisposeDialog(ref _activityDialog, OnActivityDialogClosed);
        _isActivityDialogVisible = false;
        DisposeDialog(ref _deleteDialog, OnDeleteDialogClosed);
        _isDeleteDialogVisible = false;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActivitatsViewModel.IsDialogOpen))
        {
            if (_viewModel.IsDialogOpen)
            {
                _ = ShowActivityDialogAsync();
            }
            else
            {
                _activityDialog?.Hide();
            }
        }
        else if (e.PropertyName == nameof(ActivitatsViewModel.IsDeleteConfirmationOpen))
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

    private async Task ShowActivityDialogAsync()
    {
        if (_activityDialog == null)
        {
            _activityDialog = CreateDialog("ActivityDialogTemplate");
            _activityDialog.Closed += OnActivityDialogClosed;
        }

        if (_isActivityDialogVisible)
        {
            return;
        }

        try
        {
            _isActivityDialogVisible = true;
            await _activityDialog.ShowAsync();
        }
        finally
        {
            _isActivityDialogVisible = false;
        }
    }

    private async Task ShowDeleteDialogAsync()
    {
        if (_deleteDialog == null)
        {
            _deleteDialog = CreateDialog("DeleteActivityDialogTemplate");
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
        return new ContentDialog(RootDialogPresenter)
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

    private void OnActivityDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _viewModel.IsDialogOpen = false;
        _isActivityDialogVisible = false;
    }

    private void OnDeleteDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _viewModel.IsDeleteConfirmationOpen = false;
        _isDeleteDialogVisible = false;
    }
}
