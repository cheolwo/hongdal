using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using SsalddelApp.Services.Commerce.Orders;
using SsalddelApp.Services.Warehouse.Fulfillment;

namespace SsalddelApp.ViewModels.Shipper;

public static class OrderFulfillmentFilterValues
{
    public const string All = "all";
}

public sealed record OrderFulfillmentSnapshot(
    IReadOnlyList<WarehouseOutboundNotification> 출고알림,
    IReadOnlyList<InboundRestockNotification> 입고필요알림,
    IReadOnlyList<SellerRestockNotificationPreference> 입고알림정책,
    IReadOnlyList<RestockKakaoTalkOutboxMessage> 카카오발송의도,
    IReadOnlyList<MarketInventorySnapshot> 마켓재고,
    IReadOnlyList<WarehouseOrderPickingTask> 피킹작업,
    IReadOnlyList<WarehousePackingTask> 포장작업)
{
    public static OrderFulfillmentSnapshot Empty { get; } = new([], [], [], [], [], [], []);
}

public sealed record OrderFulfillmentOrderSummary(
    string Key,
    string 국내외구분,
    string 채널종류,
    string 채널주문번호,
    string 대표상품명,
    string 상태,
    int 총수량,
    int 창고수,
    DateTime 생성시각,
    IReadOnlyList<WarehouseOutboundNotification> 출고라인목록);

/// <summary>판매 주문 이행 화면에 필요한 일곱 조회 결과와 사용자의 정확한 주문 선택만 관리합니다.</summary>
public sealed class OrderFulfillmentReadViewModel(
    ICommerceOrderFulfillmentService service) : 업무작업ViewModelBase
{
    private const char KeySeparator = '\u001f';
    private OrderFulfillmentSnapshot _스냅샷 = OrderFulfillmentSnapshot.Empty;
    private bool _초기화됨;
    private string _검색어 = string.Empty;
    private string _국내외필터 = OrderFulfillmentFilterValues.All;
    private string _상태필터 = OrderFulfillmentFilterValues.All;
    private string? _선택주문Key;

    public OrderFulfillmentSnapshot 스냅샷
    {
        get => _스냅샷;
        private set
        {
            if (!SetProperty(ref _스냅샷, value))
            {
                return;
            }

            NotifyProjectionChanged();
        }
    }

    public bool 초기화됨
    {
        get => _초기화됨;
        private set => SetProperty(ref _초기화됨, value);
    }

    public string 검색어
    {
        get => _검색어;
        set
        {
            if (SetProperty(ref _검색어, value ?? string.Empty))
            {
                OnPropertyChanged(nameof(필터된주문목록));
                OnPropertyChanged(nameof(검색결과없음));
            }
        }
    }

    public string 국내외필터
    {
        get => _국내외필터;
        set
        {
            if (SetProperty(ref _국내외필터, string.IsNullOrWhiteSpace(value) ? OrderFulfillmentFilterValues.All : value))
            {
                OnPropertyChanged(nameof(필터된주문목록));
                OnPropertyChanged(nameof(검색결과없음));
            }
        }
    }

    public string 상태필터
    {
        get => _상태필터;
        set
        {
            if (SetProperty(ref _상태필터, string.IsNullOrWhiteSpace(value) ? OrderFulfillmentFilterValues.All : value))
            {
                OnPropertyChanged(nameof(필터된주문목록));
                OnPropertyChanged(nameof(검색결과없음));
            }
        }
    }

    public string? 선택주문Key
    {
        get => _선택주문Key;
        private set
        {
            if (SetProperty(ref _선택주문Key, value))
            {
                OnPropertyChanged(nameof(선택주문));
                OnPropertyChanged(nameof(선택요청없음));
                OnPropertyChanged(nameof(선택주문찾을수없음));
            }
        }
    }

    public IReadOnlyList<OrderFulfillmentOrderSummary> 주문목록
        => 스냅샷.출고알림
            .GroupBy(item => CreateOrderKey(item.ChannelType, item.ChannelOrderNo), StringComparer.Ordinal)
            .Select(group =>
            {
                var lines = group.OrderBy(item => item.Id).ToArray();
                var first = lines[0];
                var status = lines.Any(item => item.Status == WarehouseOutboundNotificationStatusCodes.Blocked)
                    ? WarehouseOutboundNotificationStatusCodes.Blocked
                    : lines.Any(item => item.Status == WarehouseOutboundNotificationStatusCodes.Ready)
                        ? WarehouseOutboundNotificationStatusCodes.Ready
                        : lines.Select(item => item.Status).FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)) ?? "상태 확인 필요";
                var productNames = lines.Select(item => item.ProductName).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct().ToArray();
                var productLabel = productNames.Length switch
                {
                    0 => "상품 확인 필요",
                    1 => productNames[0],
                    _ => $"{productNames[0]} 외 {productNames.Length - 1}종"
                };

                return new OrderFulfillmentOrderSummary(
                    group.Key,
                    first.OrderScope,
                    first.ChannelType,
                    first.ChannelOrderNo,
                    productLabel,
                    status,
                    lines.Sum(item => item.RequestedQuantity),
                    lines.Where(item => item.WarehouseId.HasValue).Select(item => item.WarehouseId).Distinct().Count(),
                    lines.Max(item => item.CreatedAt),
                    lines);
            })
            .OrderByDescending(item => item.생성시각)
            .ToArray();

    public IReadOnlyList<OrderFulfillmentOrderSummary> 필터된주문목록
        => 주문목록.Where(MatchesFilters).ToArray();

    public IReadOnlyList<string> 상태옵션
        => 주문목록.Select(item => item.상태).Distinct(StringComparer.Ordinal).OrderBy(item => item).ToArray();

    public OrderFulfillmentOrderSummary? 선택주문
        => 선택주문Key is null
            ? null
            : 주문목록.FirstOrDefault(item => string.Equals(item.Key, 선택주문Key, StringComparison.Ordinal));

    public bool 선택요청없음 => 선택주문Key is null;
    public bool 선택주문찾을수없음 => 선택주문Key is not null && 선택주문 is null;
    public bool 원장없음 => 초기화됨 && 주문목록.Count == 0;
    public bool 검색결과없음 => 초기화됨 && 주문목록.Count > 0 && 필터된주문목록.Count == 0;
    public int 국내주문수 => 주문목록.Count(item => item.국내외구분 == CommerceOrderScopeCodes.Domestic);
    public int 해외주문수 => 주문목록.Count(item => item.국내외구분 == CommerceOrderScopeCodes.International);
    public int 출고대기라인수 => 스냅샷.출고알림.Count(item => item.Status == WarehouseOutboundNotificationStatusCodes.Ready);
    public int 입고필요수 => 스냅샷.입고필요알림.Count;

    public Task<bool> 조회Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                var notificationsTask = service.GetNotificationsAsync(token);
                var restockTask = service.GetRestockNotificationsAsync(token);
                var preferencesTask = service.GetSellerRestockNotificationPreferencesAsync(token);
                var outboxTask = service.GetRestockKakaoTalkOutboxMessagesAsync(token);
                var inventoryTask = service.GetMarketInventoryAsync(token);
                var pickingTask = service.GetPickingTasksAsync(token);
                var packingTask = service.GetPackingTasksAsync(token);

                await Task.WhenAll(
                    notificationsTask,
                    restockTask,
                    preferencesTask,
                    outboxTask,
                    inventoryTask,
                    pickingTask,
                    packingTask);

                스냅샷 = new OrderFulfillmentSnapshot(
                    await notificationsTask,
                    await restockTask,
                    await preferencesTask,
                    await outboxTask,
                    await inventoryTask,
                    await pickingTask,
                    await packingTask);
                초기화됨 = true;
            },
            "판매 주문 이행 현황을 새로고침했습니다.",
            cancellationToken,
            ex => $"판매 주문 이행 현황을 불러오지 못했습니다. {ex.Message}");

    public void 주문선택(string orderKey)
    {
        if (!string.IsNullOrWhiteSpace(orderKey))
        {
            선택주문Key = orderKey;
        }
    }

    public void 주문선택(string channelType, string channelOrderNo)
        => 주문선택(CreateOrderKey(channelType, channelOrderNo));

    public void 선택해제() => 선택주문Key = null;

    public void 필터초기화()
    {
        검색어 = string.Empty;
        국내외필터 = OrderFulfillmentFilterValues.All;
        상태필터 = OrderFulfillmentFilterValues.All;
    }

    private bool MatchesFilters(OrderFulfillmentOrderSummary order)
    {
        if (국내외필터 != OrderFulfillmentFilterValues.All
            && !string.Equals(order.국내외구분, 국내외필터, StringComparison.Ordinal))
        {
            return false;
        }

        if (상태필터 != OrderFulfillmentFilterValues.All
            && !string.Equals(order.상태, 상태필터, StringComparison.Ordinal))
        {
            return false;
        }

        var search = 검색어.Trim();
        return search.Length == 0
               || order.채널주문번호.Contains(search, StringComparison.OrdinalIgnoreCase)
               || order.채널종류.Contains(search, StringComparison.OrdinalIgnoreCase)
               || order.출고라인목록.Any(line =>
                   line.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase)
                   || line.Sku.Contains(search, StringComparison.OrdinalIgnoreCase)
                   || line.WarehouseName.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    private static string CreateOrderKey(string channelType, string channelOrderNo)
        => $"{channelType}{KeySeparator}{channelOrderNo}";

    private void NotifyProjectionChanged()
    {
        OnPropertyChanged(nameof(주문목록));
        OnPropertyChanged(nameof(필터된주문목록));
        OnPropertyChanged(nameof(상태옵션));
        OnPropertyChanged(nameof(선택주문));
        OnPropertyChanged(nameof(선택주문찾을수없음));
        OnPropertyChanged(nameof(원장없음));
        OnPropertyChanged(nameof(검색결과없음));
        OnPropertyChanged(nameof(국내주문수));
        OnPropertyChanged(nameof(해외주문수));
        OnPropertyChanged(nameof(출고대기라인수));
        OnPropertyChanged(nameof(입고필요수));
    }
}

/// <summary>외부 주문 수집 없이 로컬 샘플 주문을 Simulation 원장에 반영하는 명령만 담당합니다.</summary>
public sealed class OrderFulfillmentSimulationViewModel(
    ICommerceOrderFulfillmentService service,
    ICommerceOrderSampleFeedService sampleFeed) : 업무작업ViewModelBase
{
    private int _처리건수;

    public int 처리건수
    {
        get => _처리건수;
        private set => SetProperty(ref _처리건수, value);
    }

    public Task<bool> 실행Async(CancellationToken cancellationToken = default)
        => 작업실행Async(
            async token =>
            {
                var count = 0;
                foreach (var order in sampleFeed.GetSampleOrders())
                {
                    token.ThrowIfCancellationRequested();
                    await service.ProcessOrderAsync(order, token);
                    count++;
                }

                처리건수 = count;
            },
            "로컬 Simulation 샘플 주문을 반영했습니다.",
            cancellationToken,
            ex => $"Simulation 샘플 주문을 반영하지 못했습니다. {ex.Message}");
}

public sealed record OrderFulfillmentRestockPreferenceUpdate(
    string SellerUserId,
    bool? AdminAllowsKakaoTalk = null,
    bool? SellerWantsKakaoTalk = null,
    bool? UseInternalNotification = null);

/// <summary>판매자 한 명의 입고 알림 수신 정책 변경만 담당합니다.</summary>
public sealed class OrderFulfillmentRestockPolicyViewModel(
    ICommerceOrderFulfillmentService service) : 업무작업ViewModelBase
{
    public Task<bool> 저장Async(
        OrderFulfillmentRestockPreferenceUpdate request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SellerUserId))
        {
            return Task.FromResult(유효성실패("알림 정책을 변경할 판매자를 확인해 주세요."));
        }

        return 작업실행Async(
            token => service.UpdateSellerRestockNotificationPreferenceAsync(
                request.SellerUserId,
                request.AdminAllowsKakaoTalk,
                request.SellerWantsKakaoTalk,
                request.UseInternalNotification,
                token),
            "판매자 입고 알림 정책을 저장했습니다.",
            cancellationToken,
            ex => $"판매자 입고 알림 정책을 저장하지 못했습니다. {ex.Message}");
    }
}

/// <summary>사용자가 명시적으로 선택한 피킹 작업의 스캔·보류·취소 명령만 담당합니다.</summary>
public sealed partial class OrderFulfillmentPickingViewModel(
    ICommerceOrderFulfillmentService service) : 업무작업ViewModelBase
{
    private IReadOnlyList<WarehouseOrderPickingTask> _작업목록 = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(선택작업))]
    [NotifyPropertyChangedFor(nameof(스캔가능))]
    [NotifyPropertyChangedFor(nameof(예외처리가능))]
    public partial long? 선택작업Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(스캔가능))]
    public partial string 스캔바코드 { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(예외처리가능))]
    public partial string 예외사유 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? 최근결과메시지 { get; private set; }

    public IReadOnlyList<WarehouseOrderPickingTask> 작업목록
    {
        get => _작업목록;
        private set
        {
            if (SetProperty(ref _작업목록, value))
            {
                OnPropertyChanged(nameof(선택작업));
                OnPropertyChanged(nameof(스캔가능));
                OnPropertyChanged(nameof(예외처리가능));
            }
        }
    }

    public WarehouseOrderPickingTask? 선택작업
        => 선택작업Id is long taskId
            ? 작업목록.FirstOrDefault(item => item.Id == taskId)
            : null;

    public bool 스캔가능 => 선택작업 is not null && !string.IsNullOrWhiteSpace(스캔바코드) && !처리중;
    public bool 예외처리가능 => 선택작업 is not null && !string.IsNullOrWhiteSpace(예외사유) && !처리중;

    public void 작업목록설정(IReadOnlyList<WarehouseOrderPickingTask> tasks)
        => 작업목록 = tasks;

    public Task<bool> 스캔Async(CancellationToken cancellationToken = default)
    {
        if (선택작업Id is not long taskId || string.IsNullOrWhiteSpace(스캔바코드))
        {
            return Task.FromResult(유효성실패("피킹 작업과 스캔할 바코드를 직접 선택해 주세요."));
        }

        var barcode = 스캔바코드.Trim();
        WarehousePickingScanResult? result = null;
        return 작업실행Async(
            async token =>
            {
                result = await service.ScanPickingBarcodeAsync(taskId, barcode, token);
                최근결과메시지 = result.Message;
                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(result.Message);
                }

                스캔바코드 = string.Empty;
            },
            "피킹 바코드를 Simulation 작업에 반영했습니다.",
            cancellationToken,
            ex => result?.Message ?? $"피킹 바코드를 반영하지 못했습니다. {ex.Message}");
    }

    public Task<bool> 보류Async(CancellationToken cancellationToken = default)
        => 예외처리Async(cancel: false, cancellationToken);

    public Task<bool> 취소Async(CancellationToken cancellationToken = default)
        => 예외처리Async(cancel: true, cancellationToken);

    private Task<bool> 예외처리Async(bool cancel, CancellationToken cancellationToken)
    {
        if (선택작업Id is not long taskId)
        {
            return Task.FromResult(유효성실패("보류 또는 취소할 피킹 작업을 직접 선택해 주세요."));
        }

        if (string.IsNullOrWhiteSpace(예외사유))
        {
            return Task.FromResult(유효성실패("피킹 작업의 보류 또는 취소 사유를 입력해 주세요."));
        }

        var reason = 예외사유.Trim();
        WarehousePickingScanResult? result = null;
        return 작업실행Async(
            async token =>
            {
                result = cancel
                    ? await service.CancelPickingTaskAsync(taskId, reason, token)
                    : await service.HoldPickingTaskAsync(taskId, reason, token);
                최근결과메시지 = result.Message;
                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(result.Message);
                }

                예외사유 = string.Empty;
            },
            cancel ? "피킹 작업을 Simulation에서 취소했습니다." : "피킹 작업을 Simulation에서 보류했습니다.",
            cancellationToken,
            ex => result?.Message ?? $"피킹 작업 상태를 변경하지 못했습니다. {ex.Message}");
    }
}

/// <summary>사용자가 선택한 포장 작업의 시작·완료 명령만 담당합니다.</summary>
public sealed class OrderFulfillmentPackingViewModel(
    ICommerceOrderFulfillmentService service) : 업무작업ViewModelBase
{
    private string? _최근결과메시지;

    public string? 최근결과메시지
    {
        get => _최근결과메시지;
        private set => SetProperty(ref _최근결과메시지, value);
    }

    public Task<bool> 시작Async(long packingTaskId, CancellationToken cancellationToken = default)
        => 실행Async(packingTaskId, complete: false, cancellationToken);

    public Task<bool> 완료Async(long packingTaskId, CancellationToken cancellationToken = default)
        => 실행Async(packingTaskId, complete: true, cancellationToken);

    private Task<bool> 실행Async(long packingTaskId, bool complete, CancellationToken cancellationToken)
    {
        if (packingTaskId <= 0)
        {
            return Task.FromResult(유효성실패("포장 작업을 확인해 주세요."));
        }

        WarehousePackingActionResult? result = null;
        return 작업실행Async(
            async token =>
            {
                result = complete
                    ? await service.CompletePackingTaskAsync(packingTaskId, token)
                    : await service.StartPackingTaskAsync(packingTaskId, token);
                최근결과메시지 = result.Message;
                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(result.Message);
                }
            },
            complete ? "포장 작업을 Simulation에서 완료했습니다." : "포장 작업을 Simulation에서 시작했습니다.",
            cancellationToken,
            ex => result?.Message ?? $"포장 작업 상태를 변경하지 못했습니다. {ex.Message}");
    }
}

/// <summary>조회와 네 가지 독립 Command의 실행 후 재조회 순서만 조립합니다.</summary>
public sealed class OrderFulfillmentPageViewModel : 화주PageViewModelBase
{
    public OrderFulfillmentPageViewModel(
        OrderFulfillmentReadViewModel read,
        OrderFulfillmentSimulationViewModel simulation,
        OrderFulfillmentRestockPolicyViewModel restockPolicy,
        OrderFulfillmentPickingViewModel picking,
        OrderFulfillmentPackingViewModel packing)
    {
        조회 = 하위ViewModel등록(read);
        Simulation = 하위ViewModel등록(simulation);
        입고알림정책 = 하위ViewModel등록(restockPolicy);
        피킹 = 하위ViewModel등록(picking);
        포장 = 하위ViewModel등록(packing);
    }

    public OrderFulfillmentReadViewModel 조회 { get; }
    public OrderFulfillmentSimulationViewModel Simulation { get; }
    public OrderFulfillmentRestockPolicyViewModel 입고알림정책 { get; }
    public OrderFulfillmentPickingViewModel 피킹 { get; }
    public OrderFulfillmentPackingViewModel 포장 { get; }

    protected override bool 하위ViewModel처리중
        => 조회.처리중 || Simulation.처리중 || 입고알림정책.처리중 || 피킹.처리중 || 포장.처리중;

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        if (!await 조회.조회Async(cancellationToken))
        {
            throw new InvalidOperationException(
                조회.오류메시지 ?? "판매주문 이행 원장을 조회하지 못했습니다.");
        }

        피킹.작업목록설정(조회.스냅샷.피킹작업);
    }

    public Task<bool> Simulation실행Async(CancellationToken cancellationToken = default)
        => 실행후새로고침Async(token => Simulation.실행Async(token), cancellationToken);

    public Task<bool> 입고알림정책저장Async(
        OrderFulfillmentRestockPreferenceUpdate request,
        CancellationToken cancellationToken = default)
        => 실행후새로고침Async(token => 입고알림정책.저장Async(request, token), cancellationToken);

    public Task<bool> 피킹스캔Async(CancellationToken cancellationToken = default)
        => 실행후새로고침Async(token => 피킹.스캔Async(token), cancellationToken);

    public Task<bool> 피킹보류Async(CancellationToken cancellationToken = default)
        => 실행후새로고침Async(token => 피킹.보류Async(token), cancellationToken);

    public Task<bool> 피킹취소Async(CancellationToken cancellationToken = default)
        => 실행후새로고침Async(token => 피킹.취소Async(token), cancellationToken);

    public Task<bool> 포장시작Async(long taskId, CancellationToken cancellationToken = default)
        => 실행후새로고침Async(token => 포장.시작Async(taskId, token), cancellationToken);

    public Task<bool> 포장완료Async(long taskId, CancellationToken cancellationToken = default)
        => 실행후새로고침Async(token => 포장.완료Async(taskId, token), cancellationToken);

    private async Task<bool> 실행후새로고침Async(
        Func<CancellationToken, Task<bool>> action,
        CancellationToken cancellationToken)
    {
        if (처리중 || !await action(cancellationToken))
        {
            return false;
        }

        return await 새로고침Async(cancellationToken);
    }
}
