namespace Yatta.App.Services;

using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;
using Yatta.Core.Interfaces;

/// <summary>
/// Checks and applies application updates via GitHub Releases using Velopack.
/// </summary>
public class UpdateService : IUpdateService
{
    private const string GitHubRepoUrl = "https://github.com/jaumeroig/yatta";

    private readonly UpdateManager _manager;

    public UpdateService()
    {
        _manager = new UpdateManager(new GithubSource(GitHubRepoUrl, null, false));
    }

    /// <inheritdoc />
    public bool IsInstalled => _manager.IsInstalled;

    /// <inheritdoc />
    public async Task<bool> IsUpdateAvailableAsync()
    {
        if (!_manager.IsInstalled)
            return false;

        try
        {
            var updateInfo = await _manager.CheckForUpdatesAsync();
            return updateInfo != null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateService] Update check failed: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task ApplyUpdateAndRestartAsync()
    {
        if (!_manager.IsInstalled)
            return;

        try
        {
            var updateInfo = await _manager.CheckForUpdatesAsync();
            if (updateInfo == null)
                return;

            await _manager.DownloadUpdatesAsync(updateInfo);
            _manager.ApplyUpdatesAndRestart(updateInfo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateService] Failed to apply update: {ex.Message}");
        }
    }
}
