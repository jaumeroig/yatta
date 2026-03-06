namespace Yatta.App.Views.Pages;

using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using Yatta.App.Services;
using Yatta.App.ViewModels;
using Yatta.Core.Models;


/// <summary>
/// Settings and configuration page.
/// </summary>
public partial class SettingsPage : Page
{
    private readonly SettingsViewModel _viewModel;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly IDialogService _dialogService;

    public SettingsPage(SettingsViewModel viewModel, IBreadcrumbService breadcrumbService, IDialogService dialogService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _breadcrumbService = breadcrumbService;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Event that is called when the page is loaded.
    /// </summary>
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Configure the breadcrumb with the page title
        _breadcrumbService.SetItems(
            Yatta.App.Resources.Resources.Page_Settings_Title
        );

        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Handles the purge now button click event.
    /// </summary>
    private async void PurgeNow_Click(object sender, RoutedEventArgs e)
    {
        var dialogHost = _dialogService.GetDialogHost();
        if (dialogHost == null)
        {
            return;
        }

        // Check if retention policy is Forever
        if (_viewModel.SelectedRetentionPolicy?.Value == RetentionPolicy.Forever)
        {
            var infoDialog = new ContentDialog(dialogHost)
            {
                Title = Yatta.App.Resources.Resources.Dialog_Purge_Title,
                Content = Yatta.App.Resources.Resources.Dialog_Purge_RetentionForever,
                CloseButtonText = Yatta.App.Resources.Resources.Button_Cancel
            };
            await infoDialog.ShowAsync();
            return;
        }

        // Get purge preview
        var (cutoffDate, timeRecordCount, workdayCount) = await _viewModel.GetPurgePreviewAsync();

        if (!cutoffDate.HasValue || (timeRecordCount == 0 && workdayCount == 0))
        {
            var nothingDialog = new ContentDialog(dialogHost)
            {
                Title = Yatta.App.Resources.Resources.Dialog_Purge_Title,
                Content = Yatta.App.Resources.Resources.Dialog_Purge_NothingToPurge,
                CloseButtonText = Yatta.App.Resources.Resources.Button_Cancel
            };
            await nothingDialog.ShowAsync();
            return;
        }

        // Build confirmation message
        var message = string.Format(Yatta.App.Resources.Resources.Dialog_Purge_Message, cutoffDate.Value.ToString("d"))
            + "\n\n" + string.Format(Yatta.App.Resources.Resources.Dialog_Purge_RecordCount, timeRecordCount, workdayCount)
            + "\n\n" + Yatta.App.Resources.Resources.Dialog_Purge_Warning;

        var confirmDialog = new ContentDialog(dialogHost)
        {
            Title = Yatta.App.Resources.Resources.Dialog_Purge_Title,
            Content = message,
            PrimaryButtonText = Yatta.App.Resources.Resources.Button_Purge,
            CloseButtonText = Yatta.App.Resources.Resources.Button_Cancel
        };

        var result = await confirmDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var (timeRecordsDeleted, workdaysDeleted) = await _viewModel.ExecutePurgeAsync();
            var successMessage = string.Format(Yatta.App.Resources.Resources.Dialog_Purge_Success, timeRecordsDeleted, workdaysDeleted);

            var successDialog = new ContentDialog(dialogHost)
            {
                Title = Yatta.App.Resources.Resources.Dialog_Purge_Title,
                Content = successMessage,
                CloseButtonText = Yatta.App.Resources.Resources.Button_Cancel
            };
            await successDialog.ShowAsync();
        }
    }
}
