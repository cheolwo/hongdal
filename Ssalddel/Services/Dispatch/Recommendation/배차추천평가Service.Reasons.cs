namespace 살뜰.Services.Dispatch.Recommendation
{
    public sealed partial class 배차추천평가Service
    {
        private static string BuildRecommendationReason(운송삽입평가결과? scheduleEvaluation, decimal? additionalDurationMinutes, decimal? estimatedExtraProfit, decimal? additionalDelayMinutes, decimal? routeAnchorDistanceKm, decimal? score)
        {
            var reasons = new List<string>();

            if (scheduleEvaluation is not null)
            {
                reasons.Add(scheduleEvaluation.전체완수가능여부 ? "전체 일정 완수 가능" : "전체 일정 완수 어려움");

                if (scheduleEvaluation.최적삽입인덱스.HasValue)
                {
                    reasons.Add($"삽입위치 {scheduleEvaluation.최적삽입인덱스.Value}");
                }

                if (scheduleEvaluation.경로변경이점여부 && scheduleEvaluation.경로변경절감분.HasValue)
                {
                    reasons.Add($"경로변경 {scheduleEvaluation.경로변경절감분.Value:0.0}분 절감");
                }
            }

            if (additionalDurationMinutes.HasValue)
            {
                reasons.Add($"추가 {additionalDurationMinutes.Value:0.0}분");
            }

            if (estimatedExtraProfit.HasValue)
            {
                reasons.Add($"예상순이익 {estimatedExtraProfit.Value:0}원");
            }

            if (additionalDelayMinutes.HasValue)
            {
                reasons.Add($"지연 {additionalDelayMinutes.Value:0.0}분");
            }

            if (routeAnchorDistanceKm.HasValue)
            {
                reasons.Add($"경로 {routeAnchorDistanceKm.Value:0.0}km");
            }

            if (score.HasValue)
            {
                reasons.Add($"추천점수 {score.Value:0}");
            }

            return reasons.Count > 0 ? string.Join(" · ", reasons) : "경로 정보가 부족합니다.";
        }

        private static string? BuildReturnReason(decimal? returnDetourDistanceKm, bool returnBasisUsed, string? returnSource)
        {
            if (!returnBasisUsed)
            {
                return null;
            }

            if (!returnDetourDistanceKm.HasValue)
            {
                return string.IsNullOrWhiteSpace(returnSource)
                    ? "복귀지 기준 추천을 적용했습니다."
                    : $"{returnSource} 기준 복귀지 추천을 적용했습니다.";
            }

            var body = returnDetourDistanceKm.Value <= 0m
                ? "이 의뢰를 수행하면 복귀 동선에 더 가까워집니다."
                : $"바로 복귀하는 경우보다 약 {returnDetourDistanceKm.Value:0.0}km 우회합니다.";

            return string.IsNullOrWhiteSpace(returnSource)
                ? body
                : $"{returnSource} 기준 {body}";
        }
    }
}
