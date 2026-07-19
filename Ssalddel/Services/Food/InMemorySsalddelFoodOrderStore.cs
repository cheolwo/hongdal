using Ssalddel.Contracts.Common.Participants;
using Ssalddel.Contracts.Food;

namespace Ssalddel.Services.Food;

public sealed class InMemorySsalddelFoodOrderStore : ISsalddelFoodOrderStore, I커뮤니티원장반영가능음식주문Store
{
    private readonly object _gate = new();
    private readonly List<음식주문응답> _orders;

    public InMemorySsalddelFoodOrderStore()
    {
        _orders = FoodOrderSampleData.CreateOrders().Select(FoodOrderSampleData.Clone).ToList();
    }

    public 음식주문목록응답 GetOrders()
    {
        lock (_gate)
        {
            return new 음식주문목록응답
            {
                Items = _orders.OrderByDescending(x => x.CreatedAt).Select(FoodOrderSampleData.Clone).ToArray()
            };
        }
    }

    public 음식주문응답? GetOrder(string orderNo)
    {
        if (string.IsNullOrWhiteSpace(orderNo))
        {
            return null;
        }

        lock (_gate)
        {
            return _orders
                .Where(x => string.Equals(x.주문번호, orderNo, StringComparison.OrdinalIgnoreCase))
                .Select(FoodOrderSampleData.Clone)
                .FirstOrDefault();
        }
    }

    public 음식주문응답 AddOrder(음식주문등록요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = new 음식주문응답
        {
            주문번호 = $"FOOD-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            음식점Id = request.음식점Id,
            주문자UserId = request.주문자UserId.Trim(),
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = request.수령인정보.수령인명,
                연락처 = request.수령인정보.연락처,
                주소 = request.수령인정보.주소,
                상세주소 = request.수령인정보.상세주소,
                요청사항 = request.수령인정보.요청사항,
                주문자본인수령여부 = request.수령인정보.주문자본인수령여부
            },
            상품목록 = request.상품목록.Select(x => new 음식주문상품Dto
            {
                상품명 = x.상품명,
                수량 = x.수량,
                단가 = x.단가
            }).ToArray(),
            총주문금액 = request.상품목록.Sum(x => x.단가 * x.수량),
            상태 = 음식주문상태코드.주문대기,
            배차상태 = 음식주문배차상태코드.미요청,
            결제수단 = request.결제수단,
            CreatedAt = DateTime.UtcNow,
            상태이력 =
            [
                new 음식주문상태전이기록Dto
                {
                    이전상태 = string.Empty,
                    다음상태 = 음식주문상태코드.주문대기,
                    사유 = "주문 등록",
                    전이시각Utc = DateTime.UtcNow
                }
            ]
        };

        lock (_gate)
        {
            _orders.Add(order);
        }

        return FoodOrderSampleData.Clone(order);
    }

    public 음식주문응답? 음식점수락(string orderNo, 음식점주문수락요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_gate)
        {
            var order = _orders.FirstOrDefault(x => string.Equals(x.주문번호, orderNo, StringComparison.OrdinalIgnoreCase));
            if (order is null)
            {
                return null;
            }

            var currentStatus = 음식주문상태코드.Normalize(order.상태);
            if (!음식주문상태코드.CanRestaurantAccept(currentStatus))
            {
                throw new InvalidOperationException($"음식점 수락이 가능한 주문 상태가 아닙니다. 현재상태={order.상태}");
            }

            var now = DateTime.UtcNow;
            var nextStatus = request.즉시픽업가능여부
                ? 음식주문상태코드.픽업대기
                : 음식주문상태코드.조리중;
            var cookingMinutes = request.즉시픽업가능여부
                ? 0
                : Math.Clamp(request.조리예상분 ?? 15, 1, 180);

            order.상태 = nextStatus;
            order.음식점명 = NormalizeOptional(request.음식점명) ?? order.음식점명;
            order.음식점주소 = NormalizeOptional(request.음식점주소) ?? order.음식점주소;
            order.음식점상세주소 = NormalizeOptional(request.음식점상세주소) ?? order.음식점상세주소;
            order.음식점위도 = request.음식점위도 ?? order.음식점위도;
            order.음식점경도 = request.음식점경도 ?? order.음식점경도;
            order.음식점수락시각Utc = now;
            order.조리예상완료시각Utc = now.AddMinutes(cookingMinutes);
            order.수락메모 = NormalizeOptional(request.수락메모);
            order.상태이력 = AppendHistory(order, currentStatus, nextStatus, "음식점 주문 수락", now);

            return FoodOrderSampleData.Clone(order);
        }
    }

    public 음식주문응답? 배차대기반영(string orderNo, long dispatchWaitId, DateTime dispatchRequestedAtUtc)
    {
        lock (_gate)
        {
            var order = _orders.FirstOrDefault(x => string.Equals(x.주문번호, orderNo, StringComparison.OrdinalIgnoreCase));
            if (order is null)
            {
                return null;
            }

            order.배차상태 = 음식주문배차상태코드.배차대기;
            order.배차대기Id = dispatchWaitId;
            order.배차요청시각Utc = dispatchRequestedAtUtc;

            return FoodOrderSampleData.Clone(order);
        }
    }

    public 음식주문응답? 커뮤니티원장반영(
        string orderNo,
        string ledgerId,
        string ledgerTemplateKey,
        string ledgerState,
        DateTime syncedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(orderNo))
        {
            return null;
        }

        lock (_gate)
        {
            var order = _orders.FirstOrDefault(x => string.Equals(x.주문번호, orderNo, StringComparison.OrdinalIgnoreCase));
            if (order is null)
            {
                return null;
            }

            order.커뮤니티원장Id = ledgerId;
            order.커뮤니티원장템플릿Key = ledgerTemplateKey;
            order.커뮤니티원장상태 = ledgerState;
            order.커뮤니티원장동기화시각Utc = syncedAtUtc;

            return FoodOrderSampleData.Clone(order);
        }
    }

    private static IReadOnlyList<음식주문상태전이기록Dto> AppendHistory(
        음식주문응답 order,
        string previousStatus,
        string nextStatus,
        string reason,
        DateTime changedAtUtc)
    {
        return order.상태이력
            .Concat(
            [
                new 음식주문상태전이기록Dto
                {
                    이전상태 = previousStatus,
                    다음상태 = nextStatus,
                    사유 = reason,
                    전이시각Utc = changedAtUtc
                }
            ])
            .ToArray();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
