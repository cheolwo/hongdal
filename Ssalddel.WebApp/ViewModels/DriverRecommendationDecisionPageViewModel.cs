using Microsoft.AspNetCore.Components;
using Ssalddel.Contracts.Driver.Recommendation;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Pages.DriverRecommendationComponents;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

public enum DriverRecommendationDecisionMessageTone
{
    Info,
    Success,
    Warning,
    Error
}

public sealed record DriverRecommendationDecisionOperations(
    Func<string, 기사추천수신항목?> GetSelected,
    Func<CancellationToken, Task<IReadOnlyList<기사추천수신항목>>> LoadAll,
    Action<기사추천수신항목, string> Select,
    Func<DateTimeOffset?> GetSelectedDeadline,
    Func<int?> GetSelectedResponseSeconds,
    Func<string, CancellationToken, Task<기사추천처리결과>> Accept,
    Func<string, string, CancellationToken, Task<기사추천처리결과>> Reject,
    Action<string> ClearSelected,
    Action<string> Navigate,
    Func<DateTimeOffset> UtcNow,
    Func<TimeSpan, CancellationToken, Task> Delay);

public sealed class DriverRecommendationDecisionPageViewModel : 조립ViewModelBase
{
    private const int DefaultResponseSeconds = 60;

    private readonly DriverRecommendationDecisionOperations _operations;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private CancellationTokenSource? _countdownCancellation;
    private string? _loadedRequestId;
    private 기사추천수신항목? _recommendation;
    private bool _isBusy;
    private bool _isExpired;
    private int _remainingSeconds;
    private int _responseSeconds = DefaultResponseSeconds;
    private DateTimeOffset? _deadlineUtc;
    private string _rejectReason = "기사 직접 거절";
    private string? _statusMessage;
    private DriverRecommendationDecisionMessageTone _statusTone =
        DriverRecommendationDecisionMessageTone.Info;

    public DriverRecommendationDecisionPageViewModel(
        I기사추천수신Service recommendationService,
        NavigationManager navigation)
        : this(new DriverRecommendationDecisionOperations(
            recommendationService.선택추천조회,
            cancellationToken => recommendationService.추천조회Async(
                기사추천조회범위.전체,
                cancellationToken),
            (item, source) => recommendationService.선택추천설정(item, source),
            () => recommendationService.선택추천마감시각,
            () => recommendationService.선택추천응답초,
            recommendationService.수락Async,
            (requestId, reason, cancellationToken) =>
                recommendationService.거절Async(requestId, reason, cancellationToken),
            requestId => recommendationService.선택추천해제(requestId),
            href => navigation.NavigateTo(href),
            () => DateTimeOffset.UtcNow,
            Task.Delay))
    {
    }

    public DriverRecommendationDecisionPageViewModel(
        DriverRecommendationDecisionOperations operations)
    {
        _operations = operations;
    }

    public 기사추천수신항목? Recommendation
    {
        get => _recommendation;
        private set => SetProperty(ref _recommendation, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool IsExpired
    {
        get => _isExpired;
        private set
        {
            if (SetProperty(ref _isExpired, value))
            {
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }
    }

    public int RemainingSeconds
    {
        get => _remainingSeconds;
        private set
        {
            if (SetProperty(ref _remainingSeconds, value))
            {
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }
    }

    public int ResponseSeconds
    {
        get => _responseSeconds;
        private set
        {
            if (SetProperty(ref _responseSeconds, value))
            {
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }
    }

    public string RejectReason
    {
        get => _rejectReason;
        set => SetProperty(ref _rejectReason, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public DriverRecommendationDecisionMessageTone StatusTone
    {
        get => _statusTone;
        private set => SetProperty(ref _statusTone, value);
    }

    public double ProgressPercent
        => ResponseSeconds <= 0
            ? 100
            : Math.Clamp(
                (double)(ResponseSeconds - RemainingSeconds) / ResponseSeconds * 100,
                0,
                100);

    public async Task InitializeAsync(string? requestId)
    {
        var normalized = Normalize(requestId);
        if (string.Equals(normalized, _loadedRequestId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _loadedRequestId = normalized;
        StopCountdown();
        Recommendation = null;

        if (normalized is null)
        {
            SetStatus(
                "추천 의뢰 ID가 필요합니다.",
                DriverRecommendationDecisionMessageTone.Warning);
            return;
        }

        await LoadAsync(normalized);
    }

    public async Task AcceptAsync()
    {
        if (Recommendation is null || IsExpired)
        {
            SetStatus(
                "응답 기한이 지난 추천은 수락할 수 없습니다. 추천 목록을 다시 조회해 주세요.",
                DriverRecommendationDecisionMessageTone.Warning);
            return;
        }

        var requestId = Recommendation.의뢰Id;
        await RunDecisionAsync(
            "추천을 수락했습니다. 서버가 생성한 현재 운송을 다시 조회합니다.",
            cancellationToken => _operations.Accept(requestId, cancellationToken),
            () =>
            {
                _operations.ClearSelected(requestId);
                _operations.Navigate(DriverRoutes.CurrentTransportFor(requestId));
            });
    }

    public async Task RejectAsync()
    {
        if (Recommendation is null)
        {
            return;
        }

        var requestId = Recommendation.의뢰Id;
        var reason = IsExpired
            ? "추천 응답 시간 만료 확인"
            : string.IsNullOrWhiteSpace(RejectReason)
                ? "기사 직접 거절"
                : RejectReason.Trim();

        await RunDecisionAsync(
            "추천 거절을 서버에 전송했습니다.",
            cancellationToken => _operations.Reject(requestId, reason, cancellationToken),
            () =>
            {
                _operations.ClearSelected(requestId);
                _operations.Navigate(DriverRoutes.Recommendations);
            });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            StopCountdown();
            _lifetimeCancellation.Cancel();
            _lifetimeCancellation.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task LoadAsync(string requestId)
    {
        IsBusy = true;
        SetStatus(
            "현재 기사에게 제시된 추천을 확인하는 중입니다.",
            DriverRecommendationDecisionMessageTone.Info);

        try
        {
            Recommendation = _operations.GetSelected(requestId);
            if (Recommendation is null)
            {
                var items = await _operations.LoadAll(_lifetimeCancellation.Token);
                Recommendation = items.FirstOrDefault(
                    item => string.Equals(
                        item.의뢰Id,
                        requestId,
                        StringComparison.OrdinalIgnoreCase));
                if (Recommendation is not null)
                {
                    _operations.Select(Recommendation, "배차 판단 재조회");
                }
            }

            if (Recommendation is null)
            {
                SetStatus(
                    "현재 기사에게 유효하게 제시된 추천에서 이 의뢰를 찾지 못했습니다.",
                    DriverRecommendationDecisionMessageTone.Warning);
                return;
            }

            StartCountdown(Recommendation);
            SetStatus(
                "수락 또는 거절 전송 전 조건을 마지막으로 확인해 주세요.",
                DriverRecommendationDecisionMessageTone.Info);
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, DriverRecommendationDecisionMessageTone.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RunDecisionAsync(
        string successMessage,
        Func<CancellationToken, Task<기사추천처리결과>> command,
        Action completed)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        SetStatus(
            "선택을 서버에 전송하는 중입니다.",
            DriverRecommendationDecisionMessageTone.Info);

        try
        {
            await command(_lifetimeCancellation.Token);
            StopCountdown();
            SetStatus(successMessage, DriverRecommendationDecisionMessageTone.Success);
            completed();
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, DriverRecommendationDecisionMessageTone.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StartCountdown(기사추천수신항목 item)
    {
        StopCountdown();
        _countdownCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            _lifetimeCancellation.Token);

        var now = _operations.UtcNow();
        _deadlineUtc = DriverRecommendationPresentation.ResolveDeadline(
            item,
            _operations.GetSelectedDeadline(),
            now,
            DefaultResponseSeconds);
        ResponseSeconds = Math.Max(
            1,
            _operations.GetSelectedResponseSeconds()
            ?? (int)Math.Ceiling((_deadlineUtc.Value - now).TotalSeconds));
        UpdateRemaining();

        if (!IsExpired)
        {
            _ = RunCountdownAsync(_countdownCancellation.Token);
        }
    }

    private async Task RunCountdownAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            UpdateRemaining();
            if (IsExpired)
            {
                SetStatus(
                    "응답 기한이 지났습니다. 브라우저는 자동 거절을 전송하지 않았으며 서버 상태를 다시 확인해야 합니다.",
                    DriverRecommendationDecisionMessageTone.Warning);
                return;
            }

            try
            {
                await _operations.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private void UpdateRemaining()
    {
        RemainingSeconds = _deadlineUtc.HasValue
            ? Math.Max(
                0,
                (int)Math.Ceiling(
                    (_deadlineUtc.Value - _operations.UtcNow()).TotalSeconds))
            : 0;
        IsExpired = RemainingSeconds <= 0;
    }

    private void StopCountdown()
    {
        _countdownCancellation?.Cancel();
        _countdownCancellation?.Dispose();
        _countdownCancellation = null;
    }

    private void SetStatus(
        string message,
        DriverRecommendationDecisionMessageTone tone)
    {
        StatusTone = tone;
        StatusMessage = message;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
