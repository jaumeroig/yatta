namespace Yatta.App.ViewModels;

using System.IO;
using System.Globalization;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yatta.Core.Interfaces;

/// <summary>
/// ViewModel for the What's New page that displays version history.
/// </summary>
public partial class WhatsNewViewModel : ObservableObject
{
    private const string DefaultChangelogResourceName = "Yatta.App.Resources.changelog.md";
    private readonly ILocalizationService _localizationService;

    public WhatsNewViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

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
            var executingAssembly = Assembly.GetExecutingAssembly();
            var resourceName = GetChangelogResourceName(executingAssembly);

            using var stream = executingAssembly.GetManifestResourceStream(resourceName);
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

    private string GetChangelogResourceName(Assembly assembly)
    {
        var availableResources = assembly.GetManifestResourceNames();
        var candidates = GetChangelogCandidates();

        foreach (var candidate in candidates)
        {
            if (availableResources.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return DefaultChangelogResourceName;
    }

    private IEnumerable<string> GetChangelogCandidates()
    {
        var cultureName = _localizationService.GetCurrentCulture();

        CultureInfo? culture = null;

        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            try
            {
                culture = new CultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
            }
        }

        if (culture != null)
        {
            yield return $"Yatta.App.Resources.changelog.{culture.Name}.md";
            yield return $"Yatta.App.Resources.changelog.{culture.TwoLetterISOLanguageName}.md";
        }

        yield return DefaultChangelogResourceName;
    }
}
