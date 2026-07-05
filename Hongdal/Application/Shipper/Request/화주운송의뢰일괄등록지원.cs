using System.Globalization;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

internal static class 화주운송의뢰일괄등록지원
{
    internal static Dictionary<int, List<string>> 행오류사전만들기(IReadOnlyList<string> errors)
    {
        var map = new Dictionary<int, List<string>>();
        foreach (var error in errors)
        {
            var rowNumber = ParseRowNumber(error);
            if (!rowNumber.HasValue)
            {
                continue;
            }

            if (!map.TryGetValue(rowNumber.Value, out var list))
            {
                list = [];
                map[rowNumber.Value] = list;
            }

            list.Add(error);
        }

        return map;
    }

    internal static 차량추천요청 To차량추천요청(화주운송의뢰일괄등록행입력 row)
    {
        return new 차량추천요청
        {
            화물종류 = row.화물종류,
            화물수량 = row.화물수량,
            화물길이Mm = row.화물길이Mm,
            화물폭Mm = row.화물폭Mm,
            화물높이Mm = row.화물높이Mm,
            화물중량Kg = row.화물중량Kg,
            화물부피Cbm = row.화물부피Cbm,
            팔레트개수 = row.팔레트개수,
            화물온도조건 = row.화물온도조건,
            화물파손주의여부 = row.화물파손주의여부
        };
    }

    internal static 화주운송의뢰추천결과 To통합추천결과(화주운송의뢰추천결과 baseRecommendation, 차량추천응답 vehicleRecommendation, string? selectedVehicleType = null)
    {
        var vehicleType = string.IsNullOrWhiteSpace(selectedVehicleType)
            ? (!string.IsNullOrWhiteSpace(vehicleRecommendation.추천차량종류) ? vehicleRecommendation.추천차량종류 : baseRecommendation.차량종류)
            : selectedVehicleType.Trim();

        var reasonList = vehicleRecommendation.추천사유.Any()
            ? vehicleRecommendation.추천사유.ToArray()
            : string.IsNullOrWhiteSpace(baseRecommendation.추천사유)
                ? Array.Empty<string>()
                : [baseRecommendation.추천사유];

        var warnings = baseRecommendation.경고목록
            .Concat(vehicleRecommendation.경고목록)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new 화주운송의뢰추천결과
        {
            운송방식 = baseRecommendation.운송방식,
            차량종류 = vehicleType,
            결제수단 = baseRecommendation.결제수단,
            정산시점 = baseRecommendation.정산시점,
            증빙방식 = baseRecommendation.증빙방식,
            수납주체 = baseRecommendation.수납주체,
            추천사유 = string.Join("; ", reasonList),
            추정화물부피Cbm = vehicleRecommendation.추정화물부피Cbm,
            추천사유목록 = reasonList,
            경고목록 = warnings,
            후보차량목록 = vehicleRecommendation.후보목록
        };
    }

    internal static 의뢰생성Command To의뢰생성Command(화주운송의뢰일괄등록행입력 row, 화주운송의뢰추천결과 recommendation)
    {
        var pickupStart = DateTime.UtcNow;
        var pickupEnd = pickupStart.AddHours(1);

        return new 의뢰생성Command(
            row.화주Id,
            recommendation.운송방식,
            recommendation.차량종류,
            recommendation.결제수단,
            null,
            new 화주운송정산조건DTO
            {
                정산시점 = ParseSettlementTime(recommendation.정산시점),
                결제수단 = ParsePaymentMethod(recommendation.결제수단),
                증빙방식 = ParseEvidenceMethod(recommendation.증빙방식),
                수납주체 = ParseCollector(recommendation.수납주체),
                정산메모 = recommendation.추천사유,
                세금계산서필요 = string.Equals(recommendation.증빙방식, "세금계산서", StringComparison.OrdinalIgnoreCase),
                현금영수증필요 = string.Equals(recommendation.증빙방식, "현금영수증", StringComparison.OrdinalIgnoreCase)
            },
            row.화물종류,
            row.화물설명,
            row.화물수량,
            row.화물길이Mm,
            row.화물폭Mm,
            row.화물높이Mm,
            row.화물중량Kg,
            row.화물부피Cbm,
            row.팔레트개수,
            row.화물파손주의여부,
            row.화물온도조건,
            row.픽업도로명주소 ?? string.Empty,
            row.픽업상세주소,
            null,
            null,
            "확인필요",
            "확인필요",
            pickupStart,
            pickupEnd,
            row.하차도로명주소 ?? string.Empty,
            row.하차상세주소,
            null,
            null,
            "확인필요",
            "확인필요",
            null,
            null,
            row.서비스레벨,
            row.요청사항,
            null,
            null,
            null,
            row.클라이언트행Id,
            null);
    }

    internal static IReadOnlyList<string> ValidateRow(화주운송의뢰일괄등록행입력 row)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(row.화물종류))
        {
            errors.Add($"{row.행번호}행: 화물종류는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(row.픽업도로명주소))
        {
            errors.Add($"{row.행번호}행: 픽업도로명주소는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(row.하차도로명주소))
        {
            errors.Add($"{row.행번호}행: 하차도로명주소는 필수입니다.");
        }

        return errors;
    }

    private static int? ParseRowNumber(string error)
    {
        var index = error.IndexOf('행');
        if (index <= 0)
        {
            return null;
        }

        var raw = error[..index].Trim();
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rowNumber) ? rowNumber : null;
    }

    private static 정산시점 ParseSettlementTime(string? value)
    {
        return Enum.TryParse<정산시점>(value, true, out var parsed) ? parsed : 정산시점.선결제;
    }

    private static 결제수단 ParsePaymentMethod(string? value)
    {
        return Enum.TryParse<결제수단>(value, true, out var parsed) ? parsed : 결제수단.카드;
    }

    private static 증빙방식 ParseEvidenceMethod(string? value)
    {
        return Enum.TryParse<증빙방식>(value, true, out var parsed) ? parsed : 증빙방식.없음;
    }

    private static 수납주체 ParseCollector(string? value)
    {
        return Enum.TryParse<수납주체>(value, true, out var parsed) ? parsed : 수납주체.플랫폼;
    }
}
