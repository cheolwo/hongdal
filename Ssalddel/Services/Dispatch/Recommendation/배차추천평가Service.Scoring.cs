namespace 살뜰.Services.Dispatch.Recommendation
{
    public sealed partial class 배차추천평가Service
    {
        private static decimal ScoreRecommendation(운송삽입평가결과? scheduleEvaluation, decimal? estimatedExtraProfit, decimal? additionalDelayMinutes, decimal? routeAnchorDistanceKm, string recommendationType, bool cargoSensitive, decimal? returnDetourDistanceKm, bool returnBasisUsed)
        {
            var score = 0m;

            if (scheduleEvaluation is not null)
            {
                if (!scheduleEvaluation.전체완수가능여부)
                {
                    score -= 50m;
                }
                else if (scheduleEvaluation.삽입가능여부)
                {
                    score += 10m;
                }

                if (scheduleEvaluation.경로변경이점여부)
                {
                    score += 15m;
                }
            }

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

            if (returnBasisUsed && returnDetourDistanceKm.HasValue)
            {
                score += returnDetourDistanceKm.Value <= 0m ? 20m
                    : returnDetourDistanceKm.Value <= 5m ? 10m
                    : returnDetourDistanceKm.Value <= 15m ? 0m
                    : returnDetourDistanceKm.Value <= 30m ? -10m
                    : -25m;
            }

            return score;
        }
    }
}
