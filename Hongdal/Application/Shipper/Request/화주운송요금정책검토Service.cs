using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public interface I화주운송요금정책검토Service
{
    화주운송요금정책검토결과 검토(PricingDTO? pricing, int? 결제예정금액);
}

public sealed class 화주운송요금정책검토Service : I화주운송요금정책검토Service
{
    public 화주운송요금정책검토결과 검토(PricingDTO? pricing, int? 결제예정금액)
    {
        if (pricing is null)
        {
            return new 화주운송요금정책검토결과();
        }

        var eventCodes = new List<string>();
        var warnings = new List<string>();

        var 기준운임 = CalculatePolicyFare(pricing);
        if (기준운임.HasValue && 결제예정금액.HasValue && 결제예정금액.Value < 기준운임.Value)
        {
            eventCodes.Add(화주운송요금정책이벤트코드.기준운임미달);
            warnings.Add($"결제예정금액이 기준운임 {기준운임.Value:N0}원보다 낮습니다.");
        }

        var policy = pricing.알선정책;
        if (policy is { 재알선금지: true, 알선단계: > 1 })
        {
            eventCodes.Add(화주운송요금정책이벤트코드.재알선차단필요);
            warnings.Add("재알선금지 의뢰에서 2차 이상 알선 단계가 감지되었습니다.");
        }

        decimal? spread = null;
        if (결제예정금액.HasValue && pricing.기사지급예정운임.HasValue)
        {
            var platformFee = pricing.플랫폼수수료 ?? 0m;
            spread = 결제예정금액.Value - pricing.기사지급예정운임.Value - platformFee;
            if (spread.Value > 0m)
            {
                eventCodes.Add(화주운송요금정책이벤트코드.재알선의심);
                warnings.Add($"화주 결제액과 기사 지급 예정액 사이에 {spread.Value:N0}원의 설명되지 않은 차액이 있습니다.");
            }
        }
        else if (결제예정금액.HasValue && policy?.재알선금지 == true)
        {
            eventCodes.Add(화주운송요금정책이벤트코드.기사지급운임누락);
            warnings.Add("재알선금지 의뢰는 기사 지급 예정 운임을 함께 기록해야 합니다.");
        }

        return new 화주운송요금정책검토결과
        {
            정책위반 = eventCodes.Contains(화주운송요금정책이벤트코드.재알선차단필요)
                || eventCodes.Contains(화주운송요금정책이벤트코드.기준운임미달),
            재알선의심 = eventCodes.Contains(화주운송요금정책이벤트코드.재알선의심)
                || eventCodes.Contains(화주운송요금정책이벤트코드.재알선차단필요),
            기준운임 = 기준운임,
            화주기사운임차액 = spread,
            이벤트코드목록 = eventCodes,
            경고목록 = warnings
        };
    }

    private static decimal? CalculatePolicyFare(PricingDTO pricing)
    {
        decimal? distanceFare = pricing.거리운임;
        if (!distanceFare.HasValue && pricing.예상거리Km.HasValue && pricing.Km당단가.HasValue)
        {
            distanceFare = pricing.예상거리Km.Value * pricing.Km당단가.Value;
        }

        if (!pricing.기본운임.HasValue && !distanceFare.HasValue && !pricing.최소운임.HasValue)
        {
            return null;
        }

        var subtotal =
            (pricing.기본운임 ?? 0m)
            + (distanceFare ?? 0m)
            + (pricing.대기료 ?? 0m)
            + (pricing.수작업비 ?? 0m)
            + (pricing.할증 ?? 0m);

        if (pricing.최소운임.HasValue && subtotal < pricing.최소운임.Value)
        {
            return pricing.최소운임.Value;
        }

        return subtotal;
    }
}
