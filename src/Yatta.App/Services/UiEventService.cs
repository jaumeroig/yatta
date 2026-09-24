namespace Yatta.App.Services;

/// <summary>
/// Broadcasts data and quick-action changes across the independent Blazor views.
/// </summary>
public sealed class UiEventService
{
    private TaskCompletionSource<CloseDecision>? _pendingClose;
    private TaskCompletionSource<bool>? _pendingUpdate;
    /// <summary>Raised when time entries or activities change.</summary>
    public event EventHandler? DataChanged;

    /// <summary>Raised when a native action requests the activity picker.</summary>
    public event EventHandler? ChangeActivityRequested;

    /// <summary>Raised when the desktop host needs a close decision.</summary>
    public event EventHandler? CloseDecisionRequested;

    /// <summary>Raised when the quick activity window completed its action.</summary>
    public event EventHandler? QuickActionCompleted;

    /// <summary>Raised when an update decision is needed.</summary>
    public event EventHandler? UpdateDecisionRequested;

    /// <summary>Raised when an informational notice is shown.</summary>
    public event EventHandler? NoticeChanged;

    /// <summary>The resource key for the notice currently shown in the main shell.</summary>
    public string? NoticeKey { get; private set; }

    /// <summary>Formatting arguments for the current notice.</summary>
    public object[] NoticeArguments { get; private set; } = [];

    /// <summary>Whether an update decision is waiting for the Blazor shell.</summary>
    public bool HasPendingUpdate => _pendingUpdate is not null;

    /// <summary>Notifies every open view that data has changed.</summary>
    public void PublishDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>Opens the activity picker in a visible view.</summary>
    public void RequestChangeActivity() => ChangeActivityRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>Asks the visible Blazor shell what to do with the active timer.</summary>
    public async Task<CloseDecision> AskCloseDecisionAsync()
    {
        if (CloseDecisionRequested is null)
        {
            return CloseDecision.Cancel;
        }
        TaskCompletionSource<CloseDecision> pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingClose = pending;
        CloseDecisionRequested?.Invoke(this, EventArgs.Empty);
        try
        {
            return await pending.Task.WaitAsync(TimeSpan.FromMinutes(5));
        }
        catch (TimeoutException)
        {
            if (ReferenceEquals(_pendingClose, pending))
            {
                _pendingClose = null;
            }
            return CloseDecision.Cancel;
        }
    }

    /// <summary>Completes the pending close question.</summary>
    public void AnswerCloseDecision(CloseDecision answer)
    {
        _pendingClose?.TrySetResult(answer);
        _pendingClose = null;
    }

    /// <summary>Asks the Blazor shell whether an available update should be installed.</summary>
    public async Task<bool> AskUpdateDecisionAsync()
    {
        TaskCompletionSource<bool> pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingUpdate = pending;
        UpdateDecisionRequested?.Invoke(this, EventArgs.Empty);
        try
        {
            return await pending.Task.WaitAsync(TimeSpan.FromMinutes(5));
        }
        catch (TimeoutException)
        {
            if (ReferenceEquals(_pendingUpdate, pending))
            {
                _pendingUpdate = null;
            }
            return false;
        }
    }

    /// <summary>Completes the pending update question.</summary>
    public void AnswerUpdateDecision(bool install)
    {
        _pendingUpdate?.TrySetResult(install);
        _pendingUpdate = null;
    }

    /// <summary>Shows a notice to the user in the main Blazor shell.</summary>
    public void ShowNotice(string resourceKey, params object[] arguments)
    {
        NoticeKey = resourceKey;
        NoticeArguments = arguments;
        NoticeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Dismisses the current notice.</summary>
    public void DismissNotice()
    {
        NoticeKey = null;
        NoticeArguments = [];
        NoticeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Closes the quick action window after a successful action.</summary>
    public void CompleteQuickAction() => QuickActionCompleted?.Invoke(this, EventArgs.Empty);
}

/// <summary>Choices shown when closing Yatta with a running timer.</summary>
public enum CloseDecision { Cancel, StopAndClose, KeepRunningAndClose }
