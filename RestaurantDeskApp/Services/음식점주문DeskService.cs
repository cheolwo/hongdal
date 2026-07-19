using Ssalddel.Contracts.Food;
using RestaurantDeskApp.Models.Restaurant;

namespace RestaurantDeskApp.Services;

public sealed class 음식점주문DeskService : I음식점주문DeskService
{
    private readonly object _gate = new();
    private readonly List<음식점주문DeskItem> _orders;
    private readonly I음식주문ApiClient _foodOrderClient;
    private readonly I주문알림Service _orderAlertService;
    private readonly 음식점전표DraftFactory _slipFactory;
    private long _nextId = 1000;

    public 음식점주문DeskService(
        RestaurantDeskSampleService sampleService,
        I음식주문ApiClient foodOrderClient,
        I주문알림Service orderAlertService,
        음식점전표DraftFactory slipFactory)
    {
        _foodOrderClient = foodOrderClient;
        _orderAlertService = orderAlertService;
        _slipFactory = slipFactory;
        _orders = sampleService.Get신규주문목록()
            .Select(x => new 음식점주문DeskItem
            {
                Id = x.Id,
                주문번호 = x.주문번호,
                음식점Id = x.음식점Id,
                고객명 = x.고객명,
                메뉴요약 = x.메뉴요약,
                주문금액 = x.주문금액,
                접수시각 = new DateTimeOffset(x.접수시각),
                상태 = 음식점주문Desk상태코드.주문대기,
                미확인 = x.미확인,
                최근메시지 = "실시간 주문 알림 대기"
            })
            .ToList();
    }

    public Task<IReadOnlyList<음식점주문DeskItem>> 주문목록조회Async(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<음식점주문DeskItem>>(
                _orders
                    .OrderByDescending(x => x.미확인)
                    .ThenByDescending(x => x.접수시각)
                    .ToArray());
        }
    }

    public async Task<음식점주문DeskItem> 주문알림수신Async(음식점주문수신Payload payload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.주문번호);

        음식점주문DeskItem? item;
        lock (_gate)
        {
            item = FindLocked(payload.주문번호);
            if (item is null)
            {
                item = new 음식점주문DeskItem
                {
                    Id = ++_nextId,
                    주문번호 = payload.주문번호,
                    음식점Id = payload.음식점Id,
                    접수시각 = payload.수신시각,
                };
                _orders.Add(item);
            }

            item.고객명 = payload.고객명;
            item.메뉴요약 = payload.메뉴요약;
            item.주문금액 = payload.주문금액;
            item.상태 = 음식점주문Desk상태코드.주문대기;
            item.미확인 = true;
            item.최근메시지 = string.IsNullOrWhiteSpace(payload.본문) ? "실시간 신규 주문 수신" : payload.본문;
        }

        await _orderAlertService.신규주문알림재생Async(cancellationToken);
        return item;
    }

    public async Task<음식점주문수락결과> 주문수락후전표준비Async(string 주문번호, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(주문번호))
        {
            return new 음식점주문수락결과 { 성공 = false, 메시지 = "주문번호가 없습니다." };
        }

        음식점주문DeskItem? item;
        lock (_gate)
        {
            item = FindLocked(주문번호);
            if (item is not null)
            {
                item.상태 = 음식점주문Desk상태코드.수락처리중;
                item.최근메시지 = "서버에서 주문 상세를 조회하고 있습니다.";
            }
        }

        var detail = await _foodOrderClient.주문상세조회Async(주문번호, cancellationToken);
        if (detail is null)
        {
            lock (_gate)
            {
                item = FindLocked(주문번호);
                if (item is not null)
                {
                    item.상태 = 음식점주문Desk상태코드.상세조회실패;
                    item.최근메시지 = "서버에서 주문 상세를 찾지 못했습니다.";
                }
            }

            return new 음식점주문수락결과
            {
                성공 = false,
                메시지 = "주문 상세를 찾지 못해 전표를 만들 수 없습니다.",
                주문 = item
            };
        }

        var draft = _slipFactory.Create주문전표Draft(detail);
        lock (_gate)
        {
            item = FindLocked(주문번호);
            if (item is null)
            {
                item = CreateDeskItem(detail);
                item.Id = ++_nextId;
                _orders.Add(item);
            }

            ApplyDetail(item, detail);
            item.상태 = 음식점주문Desk상태코드.수락됨;
            item.미확인 = false;
            item.수락시각 = DateTimeOffset.Now;
            item.최근메시지 = "주문 수락 완료, 전표 출력 준비";
        }

        return new 음식점주문수락결과
        {
            성공 = true,
            메시지 = "주문을 수락했고 전표를 출력합니다.",
            주문 = item,
            상세주문 = detail,
            전표Draft = draft
        };
    }

    public Task 전표출력완료Async(string 주문번호, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var item = FindLocked(주문번호);
            if (item is not null)
            {
                item.상태 = 음식점주문Desk상태코드.전표출력됨;
                item.전표출력시각 = DateTimeOffset.Now;
                item.최근메시지 = "전표 출력 요청 완료";
            }
        }

        return Task.CompletedTask;
    }

    private 음식점주문DeskItem? FindLocked(string 주문번호)
    {
        return _orders.FirstOrDefault(x => string.Equals(x.주문번호, 주문번호, StringComparison.OrdinalIgnoreCase));
    }

    private static 음식점주문DeskItem CreateDeskItem(음식주문응답 detail)
    {
        var item = new 음식점주문DeskItem
        {
            주문번호 = detail.주문번호,
            접수시각 = ToLocalOffset(detail.CreatedAt),
        };

        ApplyDetail(item, detail);
        return item;
    }

    private static void ApplyDetail(음식점주문DeskItem item, 음식주문응답 detail)
    {
        item.음식점Id = detail.음식점Id;
        item.고객명 = detail.수령인정보.수령인명;
        item.메뉴요약 = BuildMenuSummary(detail);
        item.주문금액 = detail.총주문금액;
        item.상세주문 = detail;
    }

    private static string BuildMenuSummary(음식주문응답 detail)
    {
        return string.Join(", ", detail.상품목록.Select(x => $"{x.상품명} {x.수량}"));
    }

    private static DateTimeOffset ToLocalOffset(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            return new DateTimeOffset(value).ToLocalTime();
        }

        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Local));
    }
}
