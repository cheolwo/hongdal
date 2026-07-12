using Hongdal.Contracts.Common.Participants;
using Hongdal.Contracts.Food;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.음식;

namespace Hongdal.Services.Food;

public sealed class EfHongdalFoodOrderStore : IHongdalFoodOrderStore, I커뮤니티원장반영가능음식주문Store
{
    private readonly HongdalContext _db;

    public EfHongdalFoodOrderStore(HongdalContext db)
    {
        _db = db;
    }

    public 음식주문목록응답 GetOrders()
        => new()
        {
            Items = _db.음식주문
                .AsNoTracking()
                .Include(x => x.상품목록)
                .Include(x => x.상태이력)
                .OrderByDescending(x => x.CreatedAt)
                .Take(200)
                .AsEnumerable()
                .Select(ToDto)
                .ToArray()
        };

    public 음식주문응답? GetOrder(string orderNo)
    {
        var cleanOrderNo = Clean(orderNo);
        if (cleanOrderNo is null)
        {
            return null;
        }

        var order = _db.음식주문
            .AsNoTracking()
            .Include(x => x.상품목록)
            .Include(x => x.상태이력)
            .FirstOrDefault(x => x.주문번호 == cleanOrderNo);

        return order is null ? null : ToDto(order);
    }

    public 음식주문응답 AddOrder(음식주문등록요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = DateTime.UtcNow;
        var orderNo = GenerateOrderNo(now);
        var order = new 음식주문
        {
            주문번호 = orderNo,
            음식점Id = request.음식점Id,
            주문자UserId = Clean(request.주문자UserId) ?? string.Empty,
            수령인명 = Clean(request.수령인정보.수령인명) ?? string.Empty,
            수령인연락처 = Clean(request.수령인정보.연락처) ?? string.Empty,
            수령지주소 = Clean(request.수령인정보.주소) ?? string.Empty,
            수령지상세주소 = Clean(request.수령인정보.상세주소) ?? string.Empty,
            수령요청사항 = Clean(request.수령인정보.요청사항) ?? string.Empty,
            주문자본인수령여부 = request.수령인정보.주문자본인수령여부,
            총주문금액 = request.상품목록.Sum(x => x.단가 * x.수량),
            상태 = 음식주문상태코드.주문대기,
            배차상태 = 음식주문배차상태코드.미요청,
            결제수단 = Clean(request.결제수단),
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var item in request.상품목록)
        {
            order.상품목록.Add(new 음식주문상품
            {
                상품명 = Clean(item.상품명) ?? string.Empty,
                수량 = item.수량,
                단가 = item.단가,
                CreatedAt = now
            });
        }

        order.상태이력.Add(new 음식주문상태이력
        {
            이전상태 = string.Empty,
            다음상태 = 음식주문상태코드.주문대기,
            사유 = "주문 등록",
            전이시각Utc = now
        });

        _db.음식주문.Add(order);
        _db.SaveChanges();

        return ToDto(order);
    }

    public 음식주문응답? 음식점수락(string orderNo, 음식점주문수락요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var cleanOrderNo = Clean(orderNo);
        if (cleanOrderNo is null)
        {
            return null;
        }

        var order = LoadOrderForUpdate(cleanOrderNo);
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
        order.음식점명 = Clean(request.음식점명) ?? order.음식점명;
        order.음식점주소 = Clean(request.음식점주소) ?? order.음식점주소;
        order.음식점상세주소 = Clean(request.음식점상세주소) ?? order.음식점상세주소;
        order.음식점위도 = request.음식점위도 ?? order.음식점위도;
        order.음식점경도 = request.음식점경도 ?? order.음식점경도;
        order.음식점수락시각Utc = now;
        order.조리예상완료시각Utc = now.AddMinutes(cookingMinutes);
        order.수락메모 = Clean(request.수락메모);
        order.UpdatedAt = now;
        order.상태이력.Add(new 음식주문상태이력
        {
            이전상태 = currentStatus,
            다음상태 = nextStatus,
            사유 = "음식점 주문 수락",
            전이시각Utc = now
        });

        _db.SaveChanges();
        return ToDto(order);
    }

    public 음식주문응답? 배차대기반영(string orderNo, long dispatchWaitId, DateTime dispatchRequestedAtUtc)
    {
        var cleanOrderNo = Clean(orderNo);
        if (cleanOrderNo is null)
        {
            return null;
        }

        var order = LoadOrderForUpdate(cleanOrderNo);
        if (order is null)
        {
            return null;
        }

        order.배차상태 = 음식주문배차상태코드.배차대기;
        order.배차대기Id = dispatchWaitId;
        order.배차요청시각Utc = dispatchRequestedAtUtc;
        order.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();
        return ToDto(order);
    }

    public 음식주문응답? 커뮤니티원장반영(
        string orderNo,
        string ledgerId,
        string ledgerTemplateKey,
        string ledgerState,
        DateTime syncedAtUtc)
    {
        var cleanOrderNo = Clean(orderNo);
        if (cleanOrderNo is null)
        {
            return null;
        }

        var order = LoadOrderForUpdate(cleanOrderNo);
        if (order is null)
        {
            return null;
        }

        order.커뮤니티원장Id = Clean(ledgerId);
        order.커뮤니티원장템플릿Key = Clean(ledgerTemplateKey);
        order.커뮤니티원장상태 = Clean(ledgerState);
        order.커뮤니티원장동기화시각Utc = syncedAtUtc;
        order.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();
        return ToDto(order);
    }

    private 음식주문? LoadOrderForUpdate(string orderNo)
        => _db.음식주문
            .Include(x => x.상품목록)
            .Include(x => x.상태이력)
            .FirstOrDefault(x => x.주문번호 == orderNo);

    private string GenerateOrderNo(DateTime now)
    {
        var prefix = $"FOOD-{now:yyyyMMddHHmmssfff}";
        if (!_db.음식주문.Any(x => x.주문번호 == prefix))
        {
            return prefix;
        }

        for (var index = 1; index <= 99; index++)
        {
            var candidate = $"{prefix}-{index:00}";
            if (!_db.음식주문.Any(x => x.주문번호 == candidate))
            {
                return candidate;
            }
        }

        return $"FOOD-{Guid.NewGuid():N}";
    }

    private static 음식주문응답 ToDto(음식주문 order)
        => new()
        {
            주문번호 = order.주문번호,
            음식점Id = order.음식점Id,
            음식점명 = order.음식점명,
            음식점주소 = order.음식점주소,
            음식점상세주소 = order.음식점상세주소,
            음식점위도 = order.음식점위도,
            음식점경도 = order.음식점경도,
            주문자UserId = order.주문자UserId,
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = order.수령인명,
                연락처 = order.수령인연락처,
                주소 = order.수령지주소,
                상세주소 = order.수령지상세주소,
                요청사항 = order.수령요청사항,
                주문자본인수령여부 = order.주문자본인수령여부
            },
            상품목록 = order.상품목록
                .OrderBy(x => x.Id)
                .Select(x => new 음식주문상품Dto
                {
                    상품명 = x.상품명,
                    수량 = x.수량,
                    단가 = x.단가
                })
                .ToArray(),
            총주문금액 = order.총주문금액,
            상태 = order.상태,
            배차상태 = order.배차상태,
            배차대기Id = order.배차대기Id,
            결제수단 = order.결제수단,
            음식점수락시각Utc = order.음식점수락시각Utc,
            조리예상완료시각Utc = order.조리예상완료시각Utc,
            배차요청시각Utc = order.배차요청시각Utc,
            수락메모 = order.수락메모,
            커뮤니티원장Id = order.커뮤니티원장Id,
            커뮤니티원장템플릿Key = order.커뮤니티원장템플릿Key,
            커뮤니티원장상태 = order.커뮤니티원장상태,
            커뮤니티원장동기화시각Utc = order.커뮤니티원장동기화시각Utc,
            CreatedAt = order.CreatedAt,
            상태이력 = order.상태이력
                .OrderBy(x => x.전이시각Utc)
                .Select(x => new 음식주문상태전이기록Dto
                {
                    이전상태 = x.이전상태,
                    다음상태 = x.다음상태,
                    사유 = x.사유,
                    전이시각Utc = x.전이시각Utc
                })
                .ToArray()
        };

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
