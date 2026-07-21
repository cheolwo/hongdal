using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

public sealed record DriverTransportPickupOperations(
    Func<long, CancellationToken, Task<기사운송상세응답>> LoadTransport,
    Func<long, CancellationToken, Task<기사운송상태변경응답>> ArrivePickup,
    Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> UploadPhoto,
    Func<long, 기사운송사진업로드결과, 기사상차인수증입력, CancellationToken, Task<기사운송상태변경응답>> CompletePickup,
    Func<long, 기사운송문제신고요청, CancellationToken, Task<기사운송상태변경응답>> ReportIssue);

public sealed class DriverTransportPickupPageViewModel : 조립ViewModelBase
{
    public static readonly IReadOnlyList<DriverTransportIssueReason> PickupIssueReasons =
    [
        new("상차물건없음", "상차지에 물건이 없음", "상차"),
        new("수량불일치", "수량이 다름", "상차"),
        new("상차담당자부재", "상차 담당자 부재", "상차"),
        new("화물훼손", "화물 훼손", "상차"),
        new("사진재촬영필요", "사진 재촬영 필요", "상차")
    ];

    private readonly DriverTransportPickupOperations _operations;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private long _transportId;
    private 기사운송요약응답? _currentTransport;
    private 기사운송상태변경응답? _lastState;
    private bool _isBusy;
    private string? _statusMessage;
    private DriverTransportProofMessageTone _statusTone = DriverTransportProofMessageTone.Info;
    private bool _initialized;

    public DriverTransportPickupPageViewModel(기사운송증빙Service transportService)
        : this(new DriverTransportPickupOperations(
            transportService.상세조회Async,
            transportService.상차지도착Async,
            transportService.사진업로드Async,
            transportService.상차완료Async,
            transportService.예외신고Async))
    {
    }

    public DriverTransportPickupPageViewModel(DriverTransportPickupOperations operations)
    {
        _operations = operations;
        Pickup = 하위ViewModel등록(
            new DriverPickupProofViewModel(
                () => TransportId,
                operations.UploadPhoto,
                CompletePickupCoreAsync,
                RunAsync,
                SetStatus),
            수명소유: true);
        Issue = 하위ViewModel등록(
            new DriverTransportIssueViewModel(
                () => TransportId,
                operations.UploadPhoto,
                ReportIssueCoreAsync,
                RunAsync,
                PickupIssueReasons),
            수명소유: true);
    }

    public DriverPickupProofViewModel Pickup { get; }
    public DriverTransportIssueViewModel Issue { get; }

    public long TransportId
    {
        get => _transportId;
        private set => SetProperty(ref _transportId, value);
    }

    public 기사운송요약응답? CurrentTransport
    {
        get => _currentTransport;
        private set => SetProperty(ref _currentTransport, value);
    }

    public 기사운송상태변경응답? LastState
    {
        get => _lastState;
        private set => SetProperty(ref _lastState, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public DriverTransportProofMessageTone StatusTone
    {
        get => _statusTone;
        private set => SetProperty(ref _statusTone, value);
    }

    public async Task InitializeAsync(
        long transportId,
        CancellationToken cancellationToken = default)
    {
        if (_initialized && TransportId == transportId)
        {
            return;
        }

        TransportId = transportId;
        ResetTransportContext();
        _initialized = true;

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _lifetimeCancellation.Token);
        try
        {
            await LoadTransportCoreAsync(linkedCancellation.Token);
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            SetStatus(
                $"운송 정보를 아직 불러오지 못했습니다. {ex.Message}",
                DriverTransportProofMessageTone.Info);
        }
    }

    public Task LoadTransportAsync()
        => RunAsync("운송 정보를 조회했습니다.", LoadTransportCoreAsync);

    public Task ArrivePickupAsync()
        => RunAsync("상차지 도착 상태를 서버에 반영했습니다.", async cancellationToken =>
        {
            LastState = await _operations.ArrivePickup(TransportId, cancellationToken);
            await RefreshTransportQuietlyAsync(cancellationToken);
        });

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lifetimeCancellation.Cancel();
            _lifetimeCancellation.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task LoadTransportCoreAsync(CancellationToken cancellationToken)
    {
        CurrentTransport = await _operations.LoadTransport(TransportId, cancellationToken);
    }

    private async Task CompletePickupCoreAsync(
        기사운송사진업로드결과 upload,
        기사상차인수증입력 receipt,
        CancellationToken cancellationToken)
    {
        LastState = await _operations.CompletePickup(
            TransportId,
            upload,
            receipt,
            cancellationToken);
        await RefreshTransportQuietlyAsync(cancellationToken);
    }

    private async Task ReportIssueCoreAsync(
        기사운송문제신고요청 request,
        CancellationToken cancellationToken)
    {
        LastState = await _operations.ReportIssue(TransportId, request, cancellationToken);
        await RefreshTransportQuietlyAsync(cancellationToken);
    }

    private async Task RefreshTransportQuietlyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await LoadTransportCoreAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // 상태 Command 성공 메시지는 후속 상세 조회 실패로 덮지 않는다.
        }
    }

    private async Task RunAsync(
        string successMessage,
        Func<CancellationToken, Task> action)
    {
        if (IsBusy)
        {
            SetStatus(
                "진행 중인 서버 요청이 끝난 뒤 다시 시도해 주세요.",
                DriverTransportProofMessageTone.Info);
            return;
        }

        IsBusy = true;
        SetStatus("서버 요청을 처리하는 중입니다.", DriverTransportProofMessageTone.Info);

        try
        {
            await action(_lifetimeCancellation.Token);
            SetStatus(successMessage, DriverTransportProofMessageTone.Success);
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, DriverTransportProofMessageTone.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetTransportContext()
    {
        CurrentTransport = null;
        LastState = null;
        StatusMessage = null;
        StatusTone = DriverTransportProofMessageTone.Info;
        Pickup.Reset();
        Issue.Reset();
    }

    private void SetStatus(string message, DriverTransportProofMessageTone tone)
    {
        StatusTone = tone;
        StatusMessage = message;
    }
}
