using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

public enum DriverTransportProofMessageTone
{
    Info,
    Success,
    Warning,
    Error
}

public sealed record DriverTransportProofOperations(
    Func<CancellationToken, Task<기사운송요약응답>> LoadCurrentTransport,
    Func<long, CancellationToken, Task<기사운송상세응답>> LoadTransportDetail,
    Func<long, CancellationToken, Task<기사운송상태변경응답>> ArrivePickup,
    Func<long, CancellationToken, Task<기사운송상태변경응답>> ArriveDropoff,
    Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> UploadPhoto,
    Func<long, 기사운송사진업로드결과, 기사상차인수증입력, CancellationToken, Task<기사운송상태변경응답>> CompletePickup,
    Func<long, 기사운송사진업로드결과, CancellationToken, Task<기사운송상태변경응답>> CompleteDropoff,
    Func<long, 기사운송문제신고요청, CancellationToken, Task<기사운송상태변경응답>> ReportIssue);

public delegate Task DriverTransportProofCommandRunner(
    string successMessage,
    Func<CancellationToken, Task> action);

public sealed class DriverTransportProofPageViewModel : 조립ViewModelBase
{
    private readonly DriverTransportProofOperations _operations;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private long _transportId = 1;
    private 기사운송요약응답? _currentTransport;
    private 기사운송상태변경응답? _lastState;
    private bool _isBusy;
    private string? _statusMessage;
    private DriverTransportProofMessageTone _statusTone = DriverTransportProofMessageTone.Info;

    public DriverTransportProofPageViewModel(기사운송증빙Service transportService)
        : this(new DriverTransportProofOperations(
            transportService.현재운송조회Async,
            transportService.상세조회Async,
            transportService.상차지도착Async,
            transportService.하차지도착Async,
            transportService.사진업로드Async,
            transportService.상차완료Async,
            transportService.하차완료Async,
            transportService.예외신고Async))
    {
    }

    public DriverTransportProofPageViewModel(DriverTransportProofOperations operations)
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
        Dropoff = 하위ViewModel등록(
            new DriverDropoffProofViewModel(
                () => TransportId,
                operations.UploadPhoto,
                CompleteDropoffCoreAsync,
                RunAsync,
                SetStatus),
            수명소유: true);
        Issue = 하위ViewModel등록(
            new DriverTransportIssueViewModel(
                () => TransportId,
                operations.UploadPhoto,
                ReportIssueCoreAsync,
                RunAsync),
            수명소유: true);
    }

    public DriverPickupProofViewModel Pickup { get; }
    public DriverDropoffProofViewModel Dropoff { get; }
    public DriverTransportIssueViewModel Issue { get; }

    public long TransportId
    {
        get => _transportId;
        set
        {
            var normalized = Math.Max(1, value);
            if (!SetProperty(ref _transportId, normalized))
            {
                return;
            }

            ResetTransportContext();
        }
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

    public void ConfigureTransportId(long? transportId)
    {
        if (transportId is > 0)
        {
            TransportId = transportId.Value;
        }
    }

    public Task LoadCurrentTransportAsync()
        => RunAsync("현재 운송을 조회했습니다.", async cancellationToken =>
        {
            var transport = await _operations.LoadCurrentTransport(cancellationToken);
            TransportId = transport.Id;
            CurrentTransport = transport;
        });

    public Task LoadTransportDetailAsync()
        => RunAsync("운송 상세를 조회했습니다.", async cancellationToken =>
        {
            CurrentTransport = await _operations.LoadTransportDetail(TransportId, cancellationToken);
        });

    public Task ArrivePickupAsync()
        => RunAsync("상차지 도착 상태를 서버에 반영했습니다.", async cancellationToken =>
        {
            LastState = await _operations.ArrivePickup(TransportId, cancellationToken);
            await RefreshDetailQuietlyAsync(cancellationToken);
        });

    public Task ArriveDropoffAsync()
        => RunAsync("하차지 도착 상태를 서버에 반영했습니다.", async cancellationToken =>
        {
            LastState = await _operations.ArriveDropoff(TransportId, cancellationToken);
            await RefreshDetailQuietlyAsync(cancellationToken);
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
        await RefreshDetailQuietlyAsync(cancellationToken);
    }

    private async Task CompleteDropoffCoreAsync(
        기사운송사진업로드결과 upload,
        CancellationToken cancellationToken)
    {
        LastState = await _operations.CompleteDropoff(
            TransportId,
            upload,
            cancellationToken);
        await RefreshDetailQuietlyAsync(cancellationToken);
    }

    private async Task ReportIssueCoreAsync(
        기사운송문제신고요청 request,
        CancellationToken cancellationToken)
    {
        LastState = await _operations.ReportIssue(TransportId, request, cancellationToken);
        await RefreshDetailQuietlyAsync(cancellationToken);
    }

    private async Task RefreshDetailQuietlyAsync(CancellationToken cancellationToken)
    {
        try
        {
            CurrentTransport = await _operations.LoadTransportDetail(TransportId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // 상태 전환 성공 메시지를 후속 상세 조회 실패로 덮지 않는다.
        }
    }

    private async Task RunAsync(
        string successMessage,
        Func<CancellationToken, Task> action)
    {
        if (IsBusy)
        {
            SetStatus("진행 중인 서버 요청이 끝난 뒤 다시 시도해 주세요.", DriverTransportProofMessageTone.Info);
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
        Dropoff.Reset();
        Issue.Reset();
    }

    private void SetStatus(string message, DriverTransportProofMessageTone tone)
    {
        StatusTone = tone;
        StatusMessage = message;
    }
}
