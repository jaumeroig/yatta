namespace Yatta.App.ViewModels;

using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

/// <summary>
/// ViewModel for the What's New page that displays version history.
/// </summary>
public partial class WhatsNewViewModel : ObservableObject
{
    /// <summary>
    /// The markdown content loaded from the changelog file.
    /// </summary>
    [ObservableProperty]
    private string _markdownContent = string.Empty;

    /// <summary>
    /// Loads the changelog content from the embedded resource.
    /// </summary>
    [RelayCommand]
    private void LoadData()
    {
        LoadChangelogContent();
    }

    /// <summary>
    /// Loads the changelog markdown content from the embedded resource.
    /// </summary>
    private void LoadChangelogContent()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TimeTracker.App.Resources.changelog.md";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                MarkdownContent = reader.ReadToEnd();
            }
            else
            {
                MarkdownContent = "# No content available";
            }
        }
        catch (FileNotFoundException)
        {
            MarkdownContent = "# Changelog file not found";
        }
        catch (IOException)
        {
            MarkdownContent = "# Error reading changelog";
        }
    }
}
