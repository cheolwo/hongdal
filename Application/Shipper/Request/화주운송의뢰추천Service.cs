using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public interface I화주운송의뢰추천Service
{
    화주운송의뢰추천결과 추천(화주운송의뢰일괄등록행입력 입력);
}

public sealed class 화주운송의뢰추천Service : I화주운송의뢰추천Service
{
    public 화주운송의뢰추천결과 추천(화주운송의뢰일괄등록행입력 입력)
    {
        var warnings = new List<string>();

        var vehicleType = RecommendVehicleType(입력, warnings);
        var transportMode = RecommendTransportMode(입력);
        var paymentMethod = RecommendPaymentMethod(입력);
        var settlementTime = RecommendSettlementTime(입력, warnings);
        var evidenceMethod = RecommendEvidenceMethod(입력, settlementTime);
        var collector = RecommendCollector(입력, settlementTime);

        var reasons = new List<string>();
        if (!string.IsNullOrWhiteSpace(입력.화물온도조건))
        {
            reasons.Add($"온도조건({입력.화물온도조건}) 반영");
        }
        if (입력.화물파손주의여부)
        {
            reasons.Add("파손주의 반영");
        }
        if (입력.화물중량Kg.HasValue)
        {
            reasons.Add($"중량 {입력.화물중량Kg.Value:N1}kg 기준");
        }
        if (입력.화물부피Cbm.HasValue)
        {
            reasons.Add($"부피 {입력.화물부피Cbm.Value:N2}cbm 기준");
        }

        return new 화주운송의뢰추천결과
        {
            운송방식 = transportMode,
            차량종류 = vehicleType,
            결제수단 = paymentMethod,
            정산시점 = settlementTime,
            증빙방식 = evidenceMethod,
            수납주체 = collector,
            추천사유 = reasons.Count > 0 ? string.Join("; ", reasons) : "기본 화물 조건을 기준으로 추천했습니다.",
            추천사유목록 = reasons.Count > 0 ? reasons.ToArray() : ["기본 화물 조건을 기준으로 추천했습니다."],
            경고목록 = warnings.ToArray()
        };
    }

    private static string RecommendVehicleType(화주운송의뢰일괄등록행입력 입력, List<string> warnings)
    {
        var temperature = 입력.화물온도조건?.Trim();
        if (string.Equals(temperature, "냉동", StringComparison.OrdinalIgnoreCase))
        {
            return "냉동탑차";
        }

        if (string.Equals(temperature, "냉장", StringComparison.OrdinalIgnoreCase))
        {
            return "냉동탑차";
        }

        var weight = 입력.화물중량Kg ?? 0m;
        if (weight >= 1000m)
        {
            return "1.4톤";
        }

        if (weight >= 400m)
        {
            return "1톤";
        }

        if (입력.화물파손주의여부)
        {
            warnings.Add("파손주의 화물이므로 차량 상태와 포장 확인이 필요합니다.");
        }

        return "1톤";
    }

    private static string RecommendTransportMode(화주운송의뢰일괄등록행입력 입력)
    {
        if (string.Equals(입력.화물온도조건?.Trim(), "냉동", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(입력.화물온도조건?.Trim(), "냉장", StringComparison.OrdinalIgnoreCase))
        {
            return "냉동운송";
        }

        if (입력.화물파손주의여부)
        {
            return "안전운송";
        }

        return string.IsNullOrWhiteSpace(입력.운송방식) ? "일반운송" : 입력.운송방식.Trim();
    }

    private static string RecommendPaymentMethod(화주운송의뢰일괄등록행입력 입력)
    {
        return string.IsNullOrWhiteSpace(입력.결제수단) ? "카드" : 입력.결제수단.Trim();
    }

    private static string RecommendSettlementTime(화주운송의뢰일괄등록행입력 입력, List<string> warnings)
    {
        if (!string.IsNullOrWhiteSpace(입력.정산시점))
        {
            return 입력.정산시점.Trim();
        }

        if (string.Equals(입력.증빙방식?.Trim(), "인수증", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("인수증은 기본적으로 후불 정산 후보로 처리합니다.");
            return "운송완료후정산";
        }

        return "선결제";
    }

    private static string RecommendEvidenceMethod(화주운송의뢰일괄등록행입력 입력, string settlementTime)
    {
        if (!string.IsNullOrWhiteSpace(입력.증빙방식))
        {
            return 입력.증빙방식.Trim();
        }

        return settlementTime switch
        {
            "현장지급" => "현금영수증",
            "운송완료후정산" => "인수증",
            "월말정산" => "세금계산서",
            _ => "없음"
        };
    }

    private static string RecommendCollector(화주운송의뢰일괄등록행입력 입력, string settlementTime)
    {
        if (!string.IsNullOrWhiteSpace(입력.수납주체))
        {
            return 입력.수납주체.Trim();
        }

        return settlementTime switch
        {
            "현장지급" => "기사",
            "운송완료후정산" or "월말정산" => "화주직접",
            _ => "플랫폼"
        };
    }
}
