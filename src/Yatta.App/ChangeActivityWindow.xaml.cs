namespace Yatta.App;

using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Yatta.App.Models;
using Yatta.App.ViewModels;
using AppResources = Yatta.App.Resources.Resources;

/// <summary>
/// Standalone window for starting or changing an activity.
/// Shown independently of the MainWindow when triggered from the tray icon or global hotkey
/// while the MainWindow is hidden or minimized.
/// </summary>
public partial class ChangeActivityWindow : FluentWindow
{
    private readonly TodayViewModel _viewModel;
    private bool _saved;

    public ChangeActivityWindow(IServiceProvider serviceProvider)
    {
        _viewModel = serviceProvider.GetRequiredService<TodayViewModel>();
        DataContext = _viewModel;
        InitializeComponent();
    }

    /// <summary>
    /// Loads the activity data and initializes the change activity model when the window is loaded.
    /// </summary>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadDataAsync();
        await _viewModel.ChangeActivityCommand.ExecuteAsync(null);

        // Update UI elements with model data
        DialogTitleText.Text = _viewModel.ChangeActivityModel.DialogTitle;
        PrimaryButton.Content = _viewModel.ChangeActivityModel.PrimaryButtonText;
        PrimaryButton.IsEnabled = _viewModel.ChangeActivityModel.HasChanges;
        Title = _viewModel.ChangeActivityModel.DialogTitle;

        // Subscribe to model changes to keep buttons in sync
        _viewModel.ChangeActivityModel.PropertyChanged += OnChangeActivityModelPropertyChanged;
    }

    /// <summary>
    /// Keeps the primary button state and text in sync with the model.
    /// </summary>
    private void OnChangeActivityModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChangeActivityModel.HasChanges))
        {
            PrimaryButton.IsEnabled = _viewModel.ChangeActivityModel.HasChanges;
        }
        else if (e.PropertyName == nameof(ChangeActivityModel.PrimaryButtonText))
        {
            PrimaryButton.Content = _viewModel.ChangeActivityModel.PrimaryButtonText;
        }
        else if (e.PropertyName == nameof(ChangeActivityModel.DialogTitle))
        {
            DialogTitleText.Text = _viewModel.ChangeActivityModel.DialogTitle;
            Title = _viewModel.ChangeActivityModel.DialogTitle;
        }
    }

    /// <summary>
    /// Handles the primary button click (Start/Change activity).
    /// </summary>
    private async void OnPrimaryClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveChangeActivityCommand.ExecuteAsync(null);

        // If the dialog was closed (save successful), close the window
        if (!_viewModel.IsChangeActivityDialogOpen)
        {
            _saved = true;
            Close();
        }
    }

    /// <summary>
    /// Handles the cancel button click.
    /// </summary>
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Gets whether the activity was saved successfully.
    /// </summary>
    public bool WasSaved => _saved;
}
