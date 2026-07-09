namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed partial class 배차추천평가Service
    {
        private static string[] BuildBadges(string recommendationType, decimal? estimatedExtraProfit, decimal? additionalDelayMinutes, decimal? routeAnchorDistanceKm, bool cargoSensitive, decimal? returnDetourDistanceKm, bool returnBasisUsed, string? returnSource, 운송삽입평가결과? scheduleEvaluation)
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

            if (scheduleEvaluation?.경로변경이점여부 == true)
            {
                badges.Add("경로 변경 이점");
            }

            if (routeAnchorDistanceKm.HasValue && routeAnchorDistanceKm.Value <= 5m)
            {
                badges.Add("경로 근처");
            }

            if (cargoSensitive)
            {
                badges.Add("주의 필요");
            }

            if (returnBasisUsed)
            {
                badges.Add("복귀 기준");

                if (returnDetourDistanceKm.HasValue)
                {
                    if (returnDetourDistanceKm.Value <= 0m)
                    {
                        badges.Add("복귀 동선 양호");
                    }
                    else if (returnDetourDistanceKm.Value <= 5m)
                    {
                        badges.Add($"우회 {returnDetourDistanceKm.Value:0.0}km");
                    }
                }

                if (!string.IsNullOrWhiteSpace(returnSource))
                {
                    badges.Add($"{returnSource} 기준");
                }
            }

            return badges.ToArray();
        }
    }
}
