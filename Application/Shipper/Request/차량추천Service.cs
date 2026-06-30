using Hongdal.Contracts.Shipper.Request;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Shipper.Request;

public interface I차량추천Service
{
    Task<차량추천응답> 추천Async(차량추천요청 request, CancellationToken cancellationToken);
}

public sealed class 차량추천Service : I차량추천Service
{
    private readonly HongdalContext _db;

    public 차량추천Service(HongdalContext db)
    {
        _db = db;
    }

    public async Task<차량추천응답> 추천Async(차량추천요청 request, CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var reasons = new List<string>();
        var normalizedTemperature = request.화물온도조건?.Trim();
        var estimatedCbm = ResolveCargoCbm(request, warnings);

        var vehicles = await _db.차량제원
            .AsNoTracking()
            .Where(x => x.추천사용여부)
            .Where(x => x.운영권장중량Kg.HasValue || x.최대적재중량Kg > 0)
            .ToListAsync(cancellationToken);

        if (vehicles.Count == 0)
        {
            warnings.Add("추천에 사용할 차량 제원 기준이 비어 있습니다.");
            return new 차량추천응답
            {
                추천차량종류 = string.Empty,
                추정화물부피Cbm = estimatedCbm,
                추천사유 = reasons,
                경고목록 = warnings,
                후보목록 = Array.Empty<차량추천후보응답>()
            };
        }

        var matches = vehicles
            .Select(vehicle => EvaluateVehicle(vehicle, request, normalizedTemperature, estimatedCbm))
            .Where(result => result.적합)
            .OrderBy(result => result.차량.추천우선순위)
            .ThenBy(result => result.정렬점수)
            .ThenBy(result => result.차량.운영권장중량Kg ?? result.차량.최대적재중량Kg)
            .ThenBy(result => result.차량.팔레트적재개수 ?? int.MaxValue)
            .Take(5)
            .ToList();

        if (estimatedCbm.HasValue)
        {
            reasons.Add($"화물 부피 {estimatedCbm.Value:0.###}cbm 기준으로 비교했습니다.");
            reasons.Add("관리자가 설정한 차량별 권장 최대 CBM 기준을 우선 반영했습니다.");
        }

        if (request.화물중량Kg.HasValue)
        {
            reasons.Add($"화물 중량 {request.화물중량Kg.Value:0.###}kg 기준으로 비교했습니다.");
        }

        if (request.팔레트개수.HasValue)
        {
            reasons.Add($"팔레트 {request.팔레트개수.Value}개 적재 가능 여부를 확인했습니다.");
        }

        if (!string.IsNullOrWhiteSpace(normalizedTemperature))
        {
            reasons.Add($"온도조건 {normalizedTemperature} 기준으로 후보를 제한했습니다.");
        }

        if (request.화물길이Mm.HasValue || request.화물폭Mm.HasValue || request.화물높이Mm.HasValue)
        {
            reasons.Add("화물 치수를 차량 적재함 제원과 비교했습니다.");
        }

        if (request.화물파손주의여부)
        {
            warnings.Add("파손주의 화물이므로 추천 차량 선택 후 포장 상태를 추가로 확인해 주세요.");
        }

        if (matches.Count == 0)
        {
            warnings.Add("입력한 조건에 정확히 맞는 차량 제원을 찾지 못했습니다.");
            return new 차량추천응답
            {
                추천차량종류 = string.Empty,
                추정화물부피Cbm = estimatedCbm,
                추천사유 = reasons,
                경고목록 = warnings,
                후보목록 = Array.Empty<차량추천후보응답>()
            };
        }

        var candidates = matches
            .Select((match, index) => new 차량추천후보응답
            {
                차량코드 = match.차량.차량코드,
                차량종류 = match.차량.차량명,
                우선순위 = index + 1,
                적재가능중량Kg = match.차량.운영권장중량Kg ?? match.차량.최대적재중량Kg,
                적재가능부피Cbm = match.차량적재부피Cbm,
                적재가능팔레트개수 = match.차량.팔레트적재개수,
                설명 = BuildDescription(match)
            })
            .ToArray();

        return new 차량추천응답
        {
            추천차량종류 = candidates[0].차량종류,
            추정화물부피Cbm = estimatedCbm,
            추천사유 = reasons,
            경고목록 = warnings,
            후보목록 = candidates
        };
    }

    private static decimal? ResolveCargoCbm(차량추천요청 request, List<string> warnings)
    {
        if (request.화물부피Cbm.HasValue && request.화물부피Cbm.Value > 0)
        {
            return decimal.Round(request.화물부피Cbm.Value, 3, MidpointRounding.AwayFromZero);
        }

        if (!request.화물길이Mm.HasValue || !request.화물폭Mm.HasValue || !request.화물높이Mm.HasValue)
        {
            return null;
        }

        var quantity = request.화물수량.GetValueOrDefault(1);
        if (quantity <= 0)
        {
            quantity = 1;
        }

        var cbm = (request.화물길이Mm.Value / 1000m)
                  * (request.화물폭Mm.Value / 1000m)
                  * (request.화물높이Mm.Value / 1000m)
                  * quantity;

        warnings.Add("화물 부피(CBM)가 없어 치수와 수량 기준으로 추정했습니다.");
        return decimal.Round(cbm, 3, MidpointRounding.AwayFromZero);
    }

    private static 차량평가결과 EvaluateVehicle(홍달.도메인.차량.차량제원 vehicle, 차량추천요청 request, string? normalizedTemperature, decimal? cargoCbm)
    {
        var failReasons = new List<string>();

        if (string.Equals(normalizedTemperature, "냉동", StringComparison.OrdinalIgnoreCase) && !vehicle.냉동가능)
        {
            failReasons.Add("냉동 불가");
        }

        if (string.Equals(normalizedTemperature, "냉장", StringComparison.OrdinalIgnoreCase) && !vehicle.냉장가능 && !vehicle.냉동가능)
        {
            failReasons.Add("냉장 불가");
        }

        var allowedWeight = vehicle.운영권장중량Kg ?? vehicle.최대적재중량Kg;
        if (request.화물중량Kg.HasValue && request.화물중량Kg.Value > allowedWeight)
        {
            failReasons.Add($"중량 초과({request.화물중량Kg.Value:0.###}kg > {allowedWeight}kg)");
        }

        if (request.팔레트개수.HasValue && vehicle.팔레트적재개수.HasValue && request.팔레트개수.Value > vehicle.팔레트적재개수.Value)
        {
            failReasons.Add($"팔레트 초과({request.팔레트개수.Value}개 > {vehicle.팔레트적재개수.Value}개)");
        }

        if (request.화물길이Mm.HasValue && request.화물길이Mm.Value > vehicle.적재함길이Mm)
        {
            failReasons.Add($"길이 초과({request.화물길이Mm.Value}mm > {vehicle.적재함길이Mm}mm)");
        }

        if (request.화물폭Mm.HasValue && request.화물폭Mm.Value > vehicle.적재함폭Mm)
        {
            failReasons.Add($"폭 초과({request.화물폭Mm.Value}mm > {vehicle.적재함폭Mm}mm)");
        }

        if (request.화물높이Mm.HasValue && vehicle.적재함높이Mm.HasValue && request.화물높이Mm.Value > vehicle.적재함높이Mm.Value)
        {
            failReasons.Add($"높이 초과({request.화물높이Mm.Value}mm > {vehicle.적재함높이Mm.Value}mm)");
        }

        var vehicleCbm = CalculateVehicleCbm(vehicle);
        var allowedCbm = ResolveAllowedCbm(vehicle, vehicleCbm);
        if (cargoCbm.HasValue && allowedCbm.HasValue && cargoCbm.Value > allowedCbm.Value)
        {
            failReasons.Add($"부피 초과({cargoCbm.Value:0.###}cbm > {allowedCbm.Value:0.###}cbm)");
        }

        var score = CalculateScore(vehicle, allowedWeight, allowedCbm, request, cargoCbm);
        return new 차량평가결과(vehicle, allowedCbm, score, failReasons.Count == 0, failReasons);
    }

    private static decimal? CalculateVehicleCbm(홍달.도메인.차량.차량제원 vehicle)
    {
        if (vehicle.적재함높이Mm.GetValueOrDefault() <= 0)
        {
            return null;
        }

        var cbm = (vehicle.적재함길이Mm / 1000m)
                  * (vehicle.적재함폭Mm / 1000m)
                  * (vehicle.적재함높이Mm!.Value / 1000m);
        return decimal.Round(cbm, 3, MidpointRounding.AwayFromZero);
    }

    private static decimal? ResolveAllowedCbm(홍달.도메인.차량.차량제원 vehicle, decimal? physicalCbm)
    {
        if (vehicle.권장최대CBM.HasValue && vehicle.권장최대CBM.Value > 0)
        {
            if (!physicalCbm.HasValue)
            {
                return decimal.Round(vehicle.권장최대CBM.Value, 3, MidpointRounding.AwayFromZero);
            }

            return decimal.Round(decimal.Min(vehicle.권장최대CBM.Value, physicalCbm.Value), 3, MidpointRounding.AwayFromZero);
        }

        return physicalCbm;
    }

    private static decimal CalculateScore(홍달.도메인.차량.차량제원 vehicle, decimal allowedWeight, decimal? vehicleCbm, 차량추천요청 request, decimal? cargoCbm)
    {
        decimal score = vehicle.추천우선순위 * 10000m + allowedWeight;

        if (request.화물중량Kg.HasValue)
        {
            score = vehicle.추천우선순위 * 10000m + (allowedWeight - request.화물중량Kg.Value);
        }

        if (cargoCbm.HasValue && vehicleCbm.HasValue)
        {
            score += (vehicleCbm.Value - cargoCbm.Value) * 100m;
        }

        if (request.팔레트개수.HasValue && vehicle.팔레트적재개수.HasValue)
        {
            score += (vehicle.팔레트적재개수.Value - request.팔레트개수.Value) * 50m;
        }

        return score;
    }

    private static string BuildDescription(차량평가결과 match)
    {
        var parts = new List<string>
        {
            $"권장중량 {(match.차량.운영권장중량Kg ?? match.차량.최대적재중량Kg)}kg"
        };

        if (match.차량적재부피Cbm.HasValue)
        {
            parts.Add($"적재부피 {match.차량적재부피Cbm.Value:0.###}cbm");
        }

        if (match.차량.팔레트적재개수.HasValue)
        {
            parts.Add($"팔레트 {match.차량.팔레트적재개수.Value}개");
        }

        if (match.차량.냉동가능)
        {
            parts.Add("냉동 가능");
        }
        else if (match.차량.냉장가능)
        {
            parts.Add("냉장 가능");
        }

        return string.Join(" / ", parts);
    }

    private sealed record 차량평가결과(
        홍달.도메인.차량.차량제원 차량,
        decimal? 차량적재부피Cbm,
        decimal 정렬점수,
        bool 적합,
        IReadOnlyList<string> 부적합사유);
}
