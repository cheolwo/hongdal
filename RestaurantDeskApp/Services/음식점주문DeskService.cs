using Ssalddel.Contracts.Food;
using Microsoft.Extensions.Options;
using RestaurantDeskApp.Models.Restaurant;
using RestaurantDeskApp.Options;

namespace RestaurantDeskApp.Services;

public sealed class 음식점주문DeskService : I음식점주문DeskService
{
    private readonly object _gate = new();
    private readonly List<음식점주문DeskItem> _orders;
    private readonly I음식주문ApiClient _foodOrderClient;
    private readonly I주문알림Service _orderAlertService;
    private readonly 음식점전표DraftFactory _slipFactory;
    private readonly I음식점조리시간설정Service _조리시간설정;
    private readonly RestaurantDeskOptions _options;
    private long _nextId = 1000;

    public 음식점주문DeskService(
        I음식주문ApiClient foodOrderClient,
        I주문알림Service orderAlertService,
        음식점전표DraftFactory slipFactory,
        I음식점조리시간설정Service 조리시간설정,
        IOptions<RestaurantDeskOptions> options)
    {
        _foodOrderClient = foodOrderClient;
        _orderAlertService = orderAlertService;
        _slipFactory = slipFactory;
        _조리시간설정 = 조리시간설정;
        _options = options.Value;
        _orders = [];
    }

    public Task<음식점주문DeskItem?> 주문조회Async(
        string 주문번호,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(주문번호))
        {
            return Task.FromResult<음식점주문DeskItem?>(null);
        }

        lock (_gate)
        {
            return Task.FromResult(FindLocked(주문번호.Trim()));
        }
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
        if (payload.음식점Id != _options.RestaurantId)
        {
            throw new InvalidOperationException("현재 음식점과 다른 주문 알림은 수신함에 저장할 수 없습니다.");
        }

        var 조리시간설정 = _조리시간설정.현재조회();
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
            item.상품목록 = Clone상품목록(payload.상품목록);
            item.상품별조리기준 = Build상품별조리기준(item.상품목록);
            item.주문금액 = payload.주문금액;
            item.추천조리예상분 = 음식점조리시간정책.주문추천분(
                item.상품목록,
                조리시간설정.상품별기본조리분,
                조리시간설정.음식점기본조리분);
            item.선택조리예상분 = item.추천조리예상분;
            item.상태 = 음식점주문Desk상태코드.주문대기;
            item.미확인 = true;
            item.최근메시지 = string.IsNullOrWhiteSpace(payload.본문) ? "실시간 신규 주문 수신" : payload.본문;
        }

        await _orderAlertService.신규주문알림재생Async(cancellationToken);
        return item;
    }

    public async Task<음식점주문수락결과> 주문수락후전표준비Async(string 주문번호, CancellationToken cancellationToken = default)
    {
        var item = await 주문조회Async(주문번호, cancellationToken);
        var 조리시간설정 = _조리시간설정.현재조회();
        var 조리예상분 = item?.선택조리예상분
            ?? item?.추천조리예상분
            ?? 조리시간설정.음식점기본조리분;

        return await 주문수락후전표준비Async(주문번호, 조리예상분, cancellationToken);
    }

    public async Task<음식점주문수락결과> 주문수락후전표준비Async(
        string 주문번호,
        int 조리예상분,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(주문번호))
        {
            return new 음식점주문수락결과 { 성공 = false, 메시지 = "주문번호가 없습니다." };
        }

        var 선택조리예상분 = 음식점조리시간정책.Clamp(조리예상분);
        음식점주문DeskItem? item;
        lock (_gate)
        {
            item = FindLocked(주문번호);
            if (item is not null)
            {
                item.선택조리예상분 = 선택조리예상분;
                item.상태 = 음식점주문Desk상태코드.수락처리중;
                item.최근메시지 = $"조리 예상 {선택조리예상분}분으로 서버 주문을 수락하고 있습니다.";
            }
        }

        음식주문응답? detail;
        try
        {
            detail = await _foodOrderClient.음식점수락Async(
                주문번호,
                new 음식점주문수락요청
                {
                    처리UserId = $"restaurant:{_options.RestaurantId}",
                    음식점명 = _options.RestaurantName,
                    음식점주소 = _options.RestaurantAddress,
                    음식점상세주소 = _options.RestaurantDetailAddress,
                    음식점위도 = _options.RestaurantLatitude,
                    음식점경도 = _options.RestaurantLongitude,
                    조리예상분 = 선택조리예상분,
                    즉시픽업가능여부 = false,
                    수락메모 = "Restaurant Desk에서 수락"
                },
                cancellationToken);
        }
        catch
        {
            lock (_gate)
            {
                item = FindLocked(주문번호);
                if (item is not null)
                {
                    item.상태 = 음식점주문Desk상태코드.상세조회실패;
                    item.최근메시지 = "서버 주문 수락에 실패했습니다. 다시 시도할 수 있습니다.";
                }
            }

            throw;
        }

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
            item.선택조리예상분 = 선택조리예상분;
            item.상태 = 음식점주문Desk상태코드.수락됨;
            item.미확인 = false;
            item.수락시각 = DateTimeOffset.Now;
            item.최근메시지 = $"주문 수락 완료 · 조리 예상 {선택조리예상분}분 · 전표 출력 준비";
        }

        return new 음식점주문수락결과
        {
            성공 = true,
            메시지 = $"주문을 수락했고 조리 예상시간을 {선택조리예상분}분으로 설정했습니다.",
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

    private 음식점주문DeskItem CreateDeskItem(음식주문응답 detail)
    {
        var item = new 음식점주문DeskItem
        {
            주문번호 = detail.주문번호,
            접수시각 = ToLocalOffset(detail.CreatedAt),
        };

        ApplyDetail(item, detail);
        return item;
    }

    private void ApplyDetail(음식점주문DeskItem item, 음식주문응답 detail)
    {
        item.음식점Id = detail.음식점Id;
        item.고객명 = detail.수령인정보.수령인명;
        item.메뉴요약 = BuildMenuSummary(detail);
        item.상품목록 = Clone상품목록(detail.상품목록);
        item.상품별조리기준 = Build상품별조리기준(item.상품목록);
        var 조리시간설정 = _조리시간설정.현재조회();
        item.추천조리예상분 = 음식점조리시간정책.주문추천분(
            item.상품목록,
            조리시간설정.상품별기본조리분,
            조리시간설정.음식점기본조리분);
        item.주문금액 = detail.총주문금액;
        item.배차상태 = detail.배차상태;
        item.배차요청시각Utc = detail.배차요청시각Utc;
        item.상세주문 = detail;
    }

    private static string BuildMenuSummary(음식주문응답 detail)
    {
        return string.Join(", ", detail.상품목록.Select(x => $"{x.상품명} {x.수량}"));
    }

    private static IReadOnlyList<음식주문상품Dto> Clone상품목록(
        IEnumerable<음식주문상품Dto> 상품목록)
        => 상품목록.Select(item => new 음식주문상품Dto
        {
            상품명 = item.상품명,
            수량 = item.수량,
            단가 = item.단가
        }).ToArray();

    private IReadOnlyList<음식점주문상품조리기준> Build상품별조리기준(
        IEnumerable<음식주문상품Dto> 상품목록)
    {
        var 조리시간설정 = _조리시간설정.현재조회();
        return 상품목록.Select(item =>
        {
            var 상품기본조리분 = 음식점조리시간정책.상품기본분(
                item.상품명,
                조리시간설정.상품별기본조리분,
                조리시간설정.음식점기본조리분);
            var 별도설정있음 = 조리시간설정.상품별기본조리분.Keys.Any(key =>
                string.Equals(key.Trim(), item.상품명.Trim(), StringComparison.OrdinalIgnoreCase));

            return new 음식점주문상품조리기준(
                item.상품명,
                item.수량,
                상품기본조리분,
                !별도설정있음);
        }).ToArray();
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
