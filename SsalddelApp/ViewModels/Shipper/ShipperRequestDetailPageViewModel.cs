using Microsoft.Extensions.Options;
using Ssalddel.Client.Infrastructure;
using Ssalddel.Client.Infrastructure.Transport;
using Ssalddel.Contracts.Common.Payments;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using SsalddelApp.Models.Shipper;
using SsalddelApp.Services;

namespace SsalddelApp.ViewModels.Shipper;

/// <summary>
/// 모바일 상세 route의 서버/sample adapter 조회, 원장 관찰과 FakePG 개발 흐름을 담당합니다.
/// 결제 실행은 명시적인 payment route에서만 이 ViewModel을 통해 호출합니다.
/// </summary>
public sealed class ShipperRequestDetailPageViewModel : IDisposable
{
    private readonly IShipperOperationsService _operations;
    private readonly ITransportRequestLedgerObserver _ledgerObserver;
    private readonly FakeShipperPaymentService _fakePayments;
    private CancellationTokenSource? _pollingCts;
    private ShipperRequestItem? _source;
    private FakeShipperPaymentReceipt? _receipt;

    public ShipperRequestDetailPageViewModel(
        IShipperOperationsService operations,
        ITransportRequestLedgerObserver ledgerObserver,
        FakeShipperPaymentService fakePayments,
        IOptions<ClientDataModeOptions> dataModeOptions)
    {
        _operations = operations;
        _ledgerObserver = ledgerObserver;
        _fakePayments = fakePayments;
        State.SourceBoundaryMessage = dataModeOptions.Value.CanUseSampleFallback
            ? "서버 원장을 먼저 조회하며 개발 설정에서는 실패 시 sample adapter를 명시적으로 사용할 수 있습니다."
            : "Web과 모바일이 같은 서버 운송 의뢰 endpoint와 ID를 다시 조회합니다.";
    }

    public event Action? StateChanged;

    public ShipperRequestDetailPageState State { get; } = new();
    public bool PaymentWindowOpen { get; private set; }
    public bool PaymentCompleting { get; private set; }
    public string PaymentMethod { get; private set; } = PaymentMethodCode.TossCard;
    public string SettlementMode { get; private set; } = SettlementModeCode.Prepaid;
    public string? PaymentMemo { get; private set; }

    public int PaymentAmount
    {
        get
        {
            if (_source?.결제예정금액 is > 0)
            {
                return _source.결제예정금액.Value;
            }

            return _source?.기준운임 is > 0
                ? decimal.ToInt32(decimal.Round(_source.기준운임.Value, 0, MidpointRounding.AwayFromZero))
                : 0;
        }
    }

    public string PaymentOrderName
        => _source is null ? "살뜰 운송 의뢰" : $"살뜰 운송 의뢰 {_source.의뢰Id}";

    public ShipperRequestPaymentReceiptPresentation? Receipt
        => _receipt is null
            ? null
            : new ShipperRequestPaymentReceiptPresentation(
                _receipt.ReceiptId,
                _receipt.Amount,
                _receipt.PaymentMethodText,
                _receipt.SettlementModeText,
                _receipt.PaymentStatus,
                _receipt.ApprovedAt,
                _receipt.PayerMemo);

    public async Task InitializeAsync(
        string? requestId,
        bool created = false,
        bool showMessage = true,
        CancellationToken cancellationToken = default)
    {
        EnsureObserverStarted();
        await LoadAsync(requestId, created, showMessage, cancellationToken);
    }

    public async Task LoadAsync(
        string? requestId,
        bool created = false,
        bool showMessage = true,
        CancellationToken cancellationToken = default)
    {
        if (State.IsBusy)
        {
            return;
        }

        var normalized = string.IsNullOrWhiteSpace(requestId) ? null : requestId.Trim();
        State.LookupRequestId = normalized ?? string.Empty;
        State.Created = created;
        State.IsWorkflowEnabled = true;
        State.RequiresLogin = false;

        if (normalized is null)
        {
            _source = null;
            State.Request = null;
            SetStatus("조회할 운송 의뢰 ID가 없습니다.", ShipperRequestDetailMessageTone.Warning);
            return;
        }

        State.IsBusy = true;
        if (showMessage)
        {
            SetStatus("운송 의뢰 원장을 조회하는 중입니다.", ShipperRequestDetailMessageTone.Info);
        }

        try
        {
            _source = await _operations.GetRequestAsync(normalized, cancellationToken);
            ApplyReceipt();
            State.Request = _source is null ? null : Map(_source);
            if (showMessage)
            {
                SetStatus(
                    _source is null
                        ? "운송 의뢰 정보를 찾지 못했습니다."
                        : $"{_source.의뢰Id} 원장을 adapter에서 다시 조회했습니다.",
                    _source is null ? ShipperRequestDetailMessageTone.Warning : ShipperRequestDetailMessageTone.Success);
            }
        }
        catch (Exception ex)
        {
            _source = null;
            State.Request = null;
            SetStatus($"운송 의뢰 원장 조회 실패: {ex.Message}", ShipperRequestDetailMessageTone.Error);
        }
        finally
        {
            State.IsBusy = false;
            NotifyStateChanged();
        }
    }

    public void OpenPaymentWindow()
    {
        if (_source?.CanPay != true || PaymentAmount <= 0)
        {
            return;
        }

        PaymentMethod = ResolvePaymentMethodCode(_source.결제수단);
        SettlementMode = SettlementModeCode.Prepaid;
        PaymentMemo = null;
        PaymentWindowOpen = true;
        NotifyStateChanged();
    }

    public void ClosePaymentWindow()
    {
        if (PaymentCompleting)
        {
            return;
        }

        PaymentWindowOpen = false;
        NotifyStateChanged();
    }

    public void SetPaymentMethod(string value) => PaymentMethod = value;

    public void SetSettlementMode(string value) => SettlementMode = value;

    public void SetPaymentMemo(string value) => PaymentMemo = value;

    public async Task CompletePaymentAsync(PaymentRequestPlan plan, CancellationToken cancellationToken = default)
    {
        if (_source is null)
        {
            return;
        }

        PaymentCompleting = true;
        NotifyStateChanged();
        try
        {
            _receipt = await _fakePayments.ConfirmAsync(_source, plan, PaymentMemo, cancellationToken);
            PaymentWindowOpen = false;
            SetStatus(
                $"{_source.의뢰Id} FakePG 개발 승인이 기록되었습니다. 같은 원장을 다시 조회합니다.",
                ShipperRequestDetailMessageTone.Success);
            await LoadAsync(_source.의뢰Id, showMessage: false, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            SetStatus($"FakePG 개발 승인 실패: {ex.Message}", ShipperRequestDetailMessageTone.Error);
        }
        finally
        {
            PaymentCompleting = false;
            NotifyStateChanged();
        }
    }

    private void ApplyReceipt()
    {
        _receipt = _source is null ? null : _fakePayments.GetReceipt(_source.의뢰Id);
        if (_source is not null && _receipt is not null)
        {
            _fakePayments.TryApplyReceipt(_source);
        }
    }

    private static ShipperRequestDetailSnapshot Map(ShipperRequestItem source)
        => new()
        {
            RequestId = source.의뢰Id,
            CargoType = source.화물종류,
            RequestStatus = source.의뢰상태,
            PaymentStatus = source.결제상태,
            SettlementStatus = source.정산상태,
            DispatchStatus = source.배차상태,
            TransportMethod = source.운송방식,
            VehicleType = source.차량종류,
            PaymentMethod = source.결제수단,
            ExpectedPaymentAmount = source.결제예정금액,
            FinalFare = source.기준운임,
            DriverFare = source.기사지급예정운임,
            CreatedAt = source.생성일시,
            PickupAddress = source.픽업지 ?? string.Empty,
            DropoffAddress = source.하차지 ?? string.Empty,
            SettlementTiming = source.정산시점,
            EvidenceMethod = source.증빙방식,
            CollectionOwner = source.수납주체,
            TaxInvoiceRequired = source.세금계산서필요,
            CashReceiptRequired = source.현금영수증필요,
            SettlementMemo = source.정산메모,
            ReceiptNumber = source.인수증번호,
            ReceiptRegisteredAt = source.인수증등록일시,
            OnSiteCollectionConfirmedAt = source.현장수금확인일시,
            OnSitePaymentMemo = source.현장지급메모,
            CargoDimensions = BuildDimensions(source.화물길이Mm, source.화물폭Mm, source.화물높이Mm),
            PalletCount = source.팔레트개수,
            CanPay = source.CanPay
        };

    private static string BuildDimensions(int? length, int? width, int? height)
        => length is > 0 && width is > 0 && height is > 0
            ? $"{length:N0} × {width:N0} × {height:N0} mm"
            : string.Empty;

    private static string ResolvePaymentMethodCode(string? paymentMethod)
    {
        if (paymentMethod?.Contains("계좌", StringComparison.OrdinalIgnoreCase) == true)
        {
            return PaymentMethodCode.TossTransfer;
        }

        if (paymentMethod?.Contains("가상", StringComparison.OrdinalIgnoreCase) == true)
        {
            return PaymentMethodCode.TossVirtualAccount;
        }

        return PaymentMethodCode.TossCard;
    }

    private void EnsureObserverStarted()
    {
        if (_pollingCts is not null)
        {
            return;
        }

        _ledgerObserver.Changed += OnLedgerChanged;
        _ledgerObserver.RefreshRequested += OnLedgerRefreshRequested;
        _pollingCts = new CancellationTokenSource();
        _ = RunPollingAsync(_pollingCts.Token);
    }

    private async Task RunPollingAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                if (!string.IsNullOrWhiteSpace(State.LookupRequestId) && !State.IsBusy)
                {
                    await LoadAsync(State.LookupRequestId, State.Created, showMessage: false, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnLedgerChanged(TransportRequestLedgerChange change)
    {
        if (IsCurrentRequest(change.RequestId))
        {
            _ = LoadAsync(State.LookupRequestId, State.Created, showMessage: false);
        }
    }

    private void OnLedgerRefreshRequested(TransportRequestLedgerRefreshRequest request)
    {
        if (IsCurrentRequest(request.RequestId))
        {
            _ = LoadAsync(State.LookupRequestId, State.Created, showMessage: false);
        }
    }

    private bool IsCurrentRequest(string requestId)
        => !string.IsNullOrWhiteSpace(State.LookupRequestId)
           && string.Equals(State.LookupRequestId, requestId?.Trim(), StringComparison.OrdinalIgnoreCase);

    private void SetStatus(string message, ShipperRequestDetailMessageTone tone)
    {
        State.StatusMessage = message;
        State.StatusTone = tone;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public void Dispose()
    {
        _ledgerObserver.Changed -= OnLedgerChanged;
        _ledgerObserver.RefreshRequested -= OnLedgerRefreshRequested;
        _pollingCts?.Cancel();
        _pollingCts?.Dispose();
        _pollingCts = null;
    }
}
