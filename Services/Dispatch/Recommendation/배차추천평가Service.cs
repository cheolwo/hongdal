using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I배차추천평가Service
    {
        배차추천평가결과 평가(
            화주운송의뢰? request,
            배차추천판정결과 판정결과,
            decimal? 예상추가순이익,
            decimal? 추가지연분,
            decimal? 경로기준거리Km,
            decimal? 추가예상시간분,
            decimal? 픽업시간창여유분);
    }

    public sealed class 배차추천평가Service : I배차추천평가Service
    {
        private const decimal 지연적음기준분 = 10m;
        private const decimal 수익좋음기준원 = 5000m;

        public 배차추천평가결과 평가(
            화주운송의뢰? request,
            배차추천판정결과 판정결과,
            decimal? 예상추가순이익,
            decimal? 추가지연분,
            decimal? 경로기준거리Km,
            decimal? 추가예상시간분,
            decimal? 픽업시간창여유분)
        {
            var 추천점수 = ScoreRecommendation(예상추가순이익, 추가지연분, 경로기준거리Km, 판정결과.추천유형, 판정결과.화물민감여부);
            var 배지 = BuildBadges(판정결과.추천유형, 예상추가순이익, 추가지연분, 경로기준거리Km, 판정결과.화물민감여부);
            var 경고 = BuildWarnings(request, 판정결과, 추가지연분, 픽업시간창여유분);
            var 추천사유 = BuildRecommendationReason(추가예상시간분, 예상추가순이익, 추가지연분, 경로기준거리Km, 추천점수);

            return new 배차추천평가결과(추천점수, 배지, 경고, 추천사유);
        }

        private static decimal ScoreRecommendation(decimal? estimatedExtraProfit, decimal? additionalDelayMinutes, decimal? routeAnchorDistanceKm, string recommendationType, bool cargoSensitive)
        {
            var score = 0m;

            if (estimatedExtraProfit.HasValue)
            {
                score += Math.Clamp(estimatedExtraProfit.Value / 1000m, -20m, 40m);
            }

            if (additionalDelayMinutes.HasValue)
            {
                score += additionalDelayMinutes.Value <= 5m ? 18m
                    : additionalDelayMinutes.Value <= 10m ? 10m
                    : additionalDelayMinutes.Value <= 20m ? 2m
                    : -10m;
            }

            if (routeAnchorDistanceKm.HasValue)
            {
                score += routeAnchorDistanceKm.Value <= 2m ? 15m
                    : routeAnchorDistanceKm.Value <= 5m ? 8m
                    : routeAnchorDistanceKm.Value <= 8m ? 2m
                    : -8m;
            }

            if (string.Equals(recommendationType, "bundle_insert", StringComparison.OrdinalIgnoreCase))
            {
                score += 12m;
            }
            else if (string.Equals(recommendationType, "next_after_dropoff", StringComparison.OrdinalIgnoreCase))
            {
                score += 8m;
            }

            if (cargoSensitive)
            {
                score -= 6m;
            }

            return score;
        }

        private static string[] BuildBadges(string recommendationType, decimal? estimatedExtraProfit, decimal? additionalDelayMinutes, decimal? routeAnchorDistanceKm, bool cargoSensitive)
        {
            var badges = new List<string>();

            if (string.Equals(recommendationType, "bundle_insert", StringComparison.OrdinalIgnoreCase))
            {
                badges.Add("묶음 가능");
            }
            else if (string.Equals(recommendationType, "next_after_dropoff", StringComparison.OrdinalIgnoreCase))
            {
                badges.Add("완료 후 이어가기");
            }
            else
            {
                badges.Add("단건 추천");
            }

            if (estimatedExtraProfit.HasValue && estimatedExtraProfit.Value >= 수익좋음기준원)
            {
                badges.Add("수익 좋음");
            }

            if (additionalDelayMinutes.HasValue && additionalDelayMinutes.Value <= 지연적음기준분)
            {
                badges.Add("지연 적음");
            }

            if (routeAnchorDistanceKm.HasValue && routeAnchorDistanceKm.Value <= 5m)
            {
                badges.Add("경로 근처");
            }

            if (cargoSensitive)
            {
                badges.Add("주의 필요");
            }

            return badges.ToArray();
        }

        private static string[] BuildWarnings(화주운송의뢰? request, 배차추천판정결과 판정결과, decimal? additionalDelayMinutes, decimal? pickupWindowSlackMinutes)
        {
            var warnings = new List<string>();

            if (request is null)
            {
                warnings.Add("의뢰 정보를 찾지 못했습니다.");
                return warnings.ToArray();
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

        private static string BuildRecommendationReason(decimal? additionalDurationMinutes, decimal? estimatedExtraProfit, decimal? additionalDelayMinutes, decimal? routeAnchorDistanceKm, decimal? score)
        {
            var reasons = new List<string>();

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
    }
}