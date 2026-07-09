using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface I배차추천평가Service
    {
        배차추천평가결과 평가(
            화주운송의뢰? request,
            배차추천판정결과 판정결과,
            운송삽입평가결과? 일정삽입평가결과,
            decimal? 예상추가순이익,
            decimal? 추가지연분,
            decimal? 경로기준거리Km,
            decimal? 추가예상시간분,
            decimal? 픽업시간창여유분,
            decimal? 복귀우회증가거리Km,
            bool 복귀지기준사용됨,
            string? 복귀지출처);
    }

    public sealed partial class 배차추천평가Service : I배차추천평가Service
    {
        private const decimal 지연적음기준분 = 10m;
        private const decimal 수익좋음기준원 = 5000m;

        public 배차추천평가결과 평가(
            화주운송의뢰? request,
            배차추천판정결과 판정결과,
            운송삽입평가결과? 일정삽입평가결과,
            decimal? 예상추가순이익,
            decimal? 추가지연분,
            decimal? 경로기준거리Km,
            decimal? 추가예상시간분,
            decimal? 픽업시간창여유분,
            decimal? 복귀우회증가거리Km,
            bool 복귀지기준사용됨,
            string? 복귀지출처)
        {
            var 추천점수 = ScoreRecommendation(일정삽입평가결과, 예상추가순이익, 추가지연분, 경로기준거리Km, 판정결과.추천유형, 판정결과.화물민감여부, 복귀우회증가거리Km, 복귀지기준사용됨);
            var 배지 = BuildBadges(판정결과.추천유형, 예상추가순이익, 추가지연분, 경로기준거리Km, 판정결과.화물민감여부, 복귀우회증가거리Km, 복귀지기준사용됨, 복귀지출처, 일정삽입평가결과);
            var 경고 = BuildWarnings(request, 판정결과, 일정삽입평가결과, 추가지연분, 픽업시간창여유분);
            var 추천사유 = BuildRecommendationReason(일정삽입평가결과, 추가예상시간분, 예상추가순이익, 추가지연분, 경로기준거리Km, 추천점수);
            var 복귀추천사유 = BuildReturnReason(복귀우회증가거리Km, 복귀지기준사용됨, 복귀지출처);

            return new 배차추천평가결과(추천점수, 배지, 경고, 추천사유, 복귀추천사유);
        }

    }
}
