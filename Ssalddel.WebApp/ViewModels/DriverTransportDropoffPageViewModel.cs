using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

public sealed record DriverTransportDropoffOperations(
    Func<long, CancellationToken, Task<기사운송상세응답>> LoadTransport,
    Func<long, CancellationToken, Task<기사운송상태변경응답>> ArriveDropoff,
    Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> UploadPhoto,
    Func<long, 기사운송사진업로드결과, CancellationToken, Task<기사운송상태변경응답>> CompleteDropoff,
    Func<long, 기사운송문제신고요청, CancellationToken, Task<기사운송상태변경응답>> ReportIssue);

public sealed class DriverTransportDropoffPageViewModel : 조립ViewModelBase
{
    public static readonly IReadOnlyList<DriverTransportIssueReason> DropoffIssueReasons =
    [
        new("하차지부재", "하차지에 수령자가 없음", "하차"),
        new("하차주소불일치", "하차 주소가 다름", "하차"),
        new("수령거부", "수령자가 인수를 거부함", "하차"),
        new("화물훼손", "화물 훼손", "하차"),
        new("사진재촬영필요", "사진 재촬영 필요", "하차")
    ];

    private readonly DriverTransportDropoffOperations _operations;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private long _transportId;
    private 기사운송요약응답? _currentTransport;
    private 기사운송상태변경응답? _lastState;
    private bool _isBusy;
    private string? _statusMessage;
    private DriverTransportProofMessageTone _statusTone = DriverTransportProofMessageTone.Info;
    private string? _receiverName;
    private string _paymentEvidenceMethod = "인수증";
    private bool _dropoffPlaceConfirmed;
    private bool _receiverConfirmed;
    private bool _paymentEvidenceConfirmed;
    private bool _initialized;

    public DriverTransportDropoffPageViewModel(기사운송증빙Service transportService)
        : this(new DriverTransportDropoffOperations(
            transportService.상세조회Async,
            transportService.하차지도착Async,
            transportService.사진업로드Async,
            transportService.하차완료Async,
            transportService.예외신고Async))
    {
    }

    public DriverTransportDropoffPageViewModel(DriverTransportDropoffOperations operations)
    {
        _operations = operations;
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
                RunAsync,
                DropoffIssueReasons),
            수명소유: true);
    }

    public DriverDropoffProofViewModel Dropoff { get; }
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
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanCompleteDropoff));
            }
        }
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

    public string? ReceiverName
    {
        get => _receiverName;
        set => SetProperty(ref _receiverName, value);
    }

    public string PaymentEvidenceMethod
    {
        get => _paymentEvidenceMethod;
        set => SetProperty(ref _paymentEvidenceMethod, value);
    }

    public bool DropoffPlaceConfirmed
    {
        get => _dropoffPlaceConfirmed;
        set
        {
            if (SetProperty(ref _dropoffPlaceConfirmed, value))
            {
                OnPropertyChanged(nameof(CanCompleteDropoff));
            }
        }
    }

    public bool ReceiverConfirmed
    {
        get => _receiverConfirmed;
        set
        {
            if (SetProperty(ref _receiverConfirmed, value))
            {
                OnPropertyChanged(nameof(CanCompleteDropoff));
            }
        }
    }

    public bool PaymentEvidenceConfirmed
    {
        get => _paymentEvidenceConfirmed;
        set
        {
            if (SetProperty(ref _paymentEvidenceConfirmed, value))
            {
                OnPropertyChanged(nameof(CanCompleteDropoff));
            }
        }
    }

    public bool CanCompleteDropoff
        => !IsBusy
           && Dropoff.Upload is not null
           && DropoffPlaceConfirmed
           && ReceiverConfirmed
           && PaymentEvidenceConfirmed;

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

    public Task ArriveDropoffAsync()
        => RunAsync("하차지 도착 상태를 서버에 반영했습니다.", async cancellationToken =>
        {
            LastState = await _operations.ArriveDropoff(TransportId, cancellationToken);
            await RefreshTransportQuietlyAsync(cancellationToken);
        });

    public Task CompleteDropoffAsync()
    {
        if (!CanCompleteDropoff)
        {
            SetStatus(ResolveCompletionGuide(), DriverTransportProofMessageTone.Warning);
            return Task.CompletedTask;
        }

        var upload = Dropoff.Upload!;
        return RunAsync("하차 완료 상태를 서버에 반영했습니다.", cancellationToken =>
            CompleteDropoffCoreAsync(upload, cancellationToken));
    }

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

    private async Task CompleteDropoffCoreAsync(
        기사운송사진업로드결과 upload,
        CancellationToken cancellationToken)
    {
        LastState = await _operations.CompleteDropoff(TransportId, upload, cancellationToken);
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
        ReceiverName = null;
        PaymentEvidenceMethod = "인수증";
        DropoffPlaceConfirmed = false;
        ReceiverConfirmed = false;
        PaymentEvidenceConfirmed = false;
        Dropoff.Reset();
        Issue.Reset();
        OnPropertyChanged(nameof(CanCompleteDropoff));
    }

    private string ResolveCompletionGuide()
    {
        if (Dropoff.Upload is null)
        {
            return "하차 완료 사진을 먼저 업로드해 주세요.";
        }

        return "하차지, 수령자 인계와 현장 결제·증빙 확인을 모두 완료해 주세요.";
    }

    private void SetStatus(string message, DriverTransportProofMessageTone tone)
    {
        StatusTone = tone;
        StatusMessage = message;
    }
}
