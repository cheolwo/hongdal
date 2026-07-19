using 살뜰.도메인.화주;

namespace 살뜰.Services.Dispatch.Recommendation
{
    public sealed partial class 배차추천평가Service
    {
        private static string[] BuildWarnings(화주운송의뢰? request, 배차추천판정결과 판정결과, 운송삽입평가결과? scheduleEvaluation, decimal? additionalDelayMinutes, decimal? pickupWindowSlackMinutes)
        {
            var warnings = new List<string>();

            if (request is null)
            {
                warnings.Add("의뢰 정보를 찾지 못했습니다.");
                return warnings.ToArray();
            }

            if (scheduleEvaluation is not null && !scheduleEvaluation.전체완수가능여부)
            {
                warnings.AddRange(scheduleEvaluation.위반사유);
            }

            if (string.Equals(판정결과.추천유형, "bundle_insert", StringComparison.OrdinalIgnoreCase) && additionalDelayMinutes.HasValue && additionalDelayMinutes.Value > 0m)
            {
                warnings.Add($"기존 배송 예상 지연 +{Math.Round(additionalDelayMinutes.Value, 0):0}분");
            }

            if (pickupWindowSlackMinutes.HasValue && pickupWindowSlackMinutes.Value < 0m)
            {
                warnings.Add($"픽업 시간창이 약 {Math.Abs(Math.Round(pickupWindowSlackMinutes.Value, 0)):0}분 부족할 수 있습니다.");
            }

            if (판정결과.화물민감여부)
            {
                warnings.Add("파손/온도/긴급 화물은 단독 운송을 우선 확인하세요.");
            }

            if (판정결과.단독배송여부)
            {
                warnings.Add("화주가 단독 배송 성격으로 등록한 의뢰입니다.");
            }

            return warnings.ToArray();
        }
    }
}
