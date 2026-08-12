using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace 살뜰.Services.Dispatch.Recommendation
{
    public sealed partial class 배차추천평가Service
    {
        private static decimal ScoreRecommendation(운송삽입평가결과? scheduleEvaluation, decimal? estimatedExtraProfit, decimal? additionalDelayMinutes, decimal? routeAnchorDistanceKm, string recommendationType, bool cargoSensitive, decimal? returnDetourDistanceKm, bool returnBasisUsed)
        {
            return 화물배차추천점수Policy.판정(new 화물배차추천점수요청
            {
                전체일정완수가능여부 = scheduleEvaluation?.전체완수가능여부,
                일정삽입가능여부 = scheduleEvaluation?.삽입가능여부,
                경로변경이점여부 = scheduleEvaluation?.경로변경이점여부 ?? false,
                예상추가순이익 = estimatedExtraProfit,
                추가지연분 = additionalDelayMinutes,
                경로기준거리Km = routeAnchorDistanceKm,
                추천유형 = recommendationType,
                화물민감여부 = cargoSensitive,
                복귀우회증가거리Km = returnDetourDistanceKm,
                복귀지기준사용여부 = returnBasisUsed,
            }).총점;
        }
    }
}
