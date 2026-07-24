using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class CommunityPostIngredientPriceHintsViewModel(
    ICommunityPostIngredientPriceHintClient client) : ObservableObject
{
    private const int DebounceMilliseconds = 450;

    private IReadOnlyList<CommunityPostIngredientPriceHint> _hints = [];
    private bool _isLoading;
    private bool _hasAnalyzedText;
    private string _notice = string.Empty;
    private string? _statusMessage;
    private string _queuedBody = string.Empty;
    private string _lastCompletedBody = string.Empty;
    private long _requestGeneration;

    public IReadOnlyList<CommunityPostIngredientPriceHint> Hints
    {
        get => _hints;
        private set => SetProperty(ref _hints, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool HasAnalyzedText
    {
        get => _hasAnalyzedText;
        private set => SetProperty(ref _hasAnalyzedText, value);
    }

    public string Notice
    {
        get => _notice;
        private set => SetProperty(ref _notice, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public void QueueRefresh(string? body)
    {
        var snapshot = body ?? string.Empty;
        if (string.Equals(snapshot, _queuedBody, StringComparison.Ordinal))
        {
            return;
        }

        _queuedBody = snapshot;
        var generation = Interlocked.Increment(ref _requestGeneration);
        if (string.IsNullOrWhiteSpace(snapshot))
        {
            Reset();
            return;
        }

        _ = RefreshAfterDelayAsync(snapshot, generation);
    }

    public Task RefreshNowAsync(
        string? body,
        CancellationToken cancellationToken = default)
    {
        var snapshot = body ?? string.Empty;
        _queuedBody = snapshot;
        var generation = Interlocked.Increment(ref _requestGeneration);
        if (string.IsNullOrWhiteSpace(snapshot))
        {
            Reset();
            return Task.CompletedTask;
        }

        return RefreshCoreAsync(snapshot, generation, true, cancellationToken);
    }

    private async Task RefreshAfterDelayAsync(string body, long generation)
    {
        await Task.Delay(DebounceMilliseconds);
        if (generation != Volatile.Read(ref _requestGeneration))
        {
            return;
        }

        await RefreshCoreAsync(body, generation, false, CancellationToken.None);
    }

    private async Task RefreshCoreAsync(
        string body,
        long generation,
        bool force,
        CancellationToken cancellationToken)
    {
        if (generation != Volatile.Read(ref _requestGeneration)
            || (!force
                && string.Equals(body, _lastCompletedBody, StringComparison.Ordinal)))
        {
            return;
        }

        IsLoading = true;
        StatusMessage = null;
        try
        {
            var response = await client.GetHintsAsync(body, cancellationToken);
            if (generation != Volatile.Read(ref _requestGeneration))
            {
                return;
            }

            Hints = response.Hints;
            Notice = response.Notice;
            HasAnalyzedText = true;
            _lastCompletedBody = body;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            if (generation != Volatile.Read(ref _requestGeneration))
            {
                return;
            }

            Hints = [];
            Notice = string.Empty;
            HasAnalyzedText = true;
            StatusMessage = "저장된 공공가격을 불러오지 못했습니다. 본문 작성은 계속할 수 있습니다.";
        }
        finally
        {
            if (generation == Volatile.Read(ref _requestGeneration))
            {
                IsLoading = false;
            }
        }
    }

    private void Reset()
    {
        Hints = [];
        IsLoading = false;
        HasAnalyzedText = false;
        Notice = string.Empty;
        StatusMessage = null;
        _lastCompletedBody = string.Empty;
    }
}
