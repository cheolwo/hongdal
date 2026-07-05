using 홍달.도메인.화물;
using 홍달.도메인.기사;
using 홍달.도메인.차량;
using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I차량화물적합성Service
    {
        차량화물적합성결과 판정(차량제원? 차량, 화주운송의뢰 request, 화물요구조건? 요구조건);
    }

    public sealed class 차량화물적합성Service : I차량화물적합성Service
    {
        public 차량화물적합성결과 판정(차량제원? 차량, 화주운송의뢰 request, 화물요구조건? 요구조건)
        {
            var reasons = new List<string>();
            var warnings = new List<string>();

            if (차량 is null)
            {
                warnings.Add("기사 차량 제원이 등록되지 않았습니다.");
                return new 차량화물적합성결과(true, warnings.ToArray(), reasons.ToArray());
            }

            var cargo = 요구조건 ?? BuildFallbackRequirement(request);

            if (cargo.화물길이Mm.HasValue && cargo.화물길이Mm.Value > 차량.적재함길이Mm)
            {
                reasons.Add($"길이 초과({cargo.화물길이Mm.Value}mm > {차량.적재함길이Mm}mm)");
            }

            if (cargo.화물폭Mm.HasValue && cargo.화물폭Mm.Value > 차량.적재함폭Mm)
            {
                reasons.Add($"폭 초과({cargo.화물폭Mm.Value}mm > {차량.적재함폭Mm}mm)");
            }

            if (cargo.화물높이Mm.HasValue && 차량.적재함높이Mm.HasValue && cargo.화물높이Mm.Value > 차량.적재함높이Mm.Value)
            {
                reasons.Add($"높이 초과({cargo.화물높이Mm.Value}mm > {차량.적재함높이Mm.Value}mm)");
            }

            if (cargo.화물무게Kg.HasValue)
            {
                var allowedWeight = 차량.운영권장중량Kg ?? 차량.최대적재중량Kg;
                if (cargo.화물무게Kg.Value > allowedWeight)
                {
                    reasons.Add($"중량 초과({cargo.화물무게Kg.Value}kg > {allowedWeight}kg)");
                }
            }

            if (cargo.비맞으면안됨 && !차량.비눈보호가능)
            {
                reasons.Add("비/눈 보호 불가");
            }

            if (cargo.냉장필요 && !차량.냉장가능)
            {
                reasons.Add("냉장 불가");
            }

            if (cargo.냉동필요 && !차량.냉동가능)
            {
                reasons.Add("냉동 불가");
            }

            if (cargo.리프트필요 && !차량.리프트가능)
            {
                reasons.Add("리프트 불가");
            }

            if (cargo.측면상하차필요 && !차량.측면상하차가능)
            {
                reasons.Add("측면 상하차 불가");
            }

            if (cargo.장재물 && !차량.장재물유리)
            {
                reasons.Add("장재물 부적합");
            }

            if (!cargo.혼적허용 || cargo.독차필수)
            {
                warnings.Add("단독 배차 선호 조건입니다.");
            }

            return new 차량화물적합성결과(reasons.Count == 0, warnings.ToArray(), reasons.ToArray());
        }

        private static 화물요구조건 BuildFallbackRequirement(화주운송의뢰 request)
        {
            var text = string.Join(' ', new[] { request.운송방식, request.서비스레벨, request.요청사항, request.화물종류, request.화물설명 }
                .Where(x => !string.IsNullOrWhiteSpace(x))!);

            return new 화물요구조건
            {
                의뢰Id = request.의뢰Id,
                화물무게Kg = request.화물중량Kg.HasValue ? (int?)Math.Ceiling(request.화물중량Kg.Value) : null,
                비맞으면안됨 = text.Contains("비", StringComparison.OrdinalIgnoreCase) || text.Contains("방수", StringComparison.OrdinalIgnoreCase),
                냉장필요 = string.Equals(request.화물온도조건, "냉장", StringComparison.OrdinalIgnoreCase),
                냉동필요 = string.Equals(request.화물온도조건, "냉동", StringComparison.OrdinalIgnoreCase),
                리프트필요 = text.Contains("리프트", StringComparison.OrdinalIgnoreCase),
                측면상하차필요 = text.Contains("측면", StringComparison.OrdinalIgnoreCase),
                장재물 = text.Contains("장재물", StringComparison.OrdinalIgnoreCase),
                혼적허용 = !text.Contains("단독", StringComparison.OrdinalIgnoreCase),
                독차필수 = text.Contains("단독", StringComparison.OrdinalIgnoreCase),
                주의사항 = request.화물설명
            };
        }
    }

    public sealed record 차량화물적합성결과(bool 적합여부, string[] 경고, string[] 부적합사유);
}