using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed record 배차경로좌표(decimal Latitude, decimal Longitude);

    public sealed record 배차경로예상결과(decimal? DistanceKm, TimeSpan? Duration, decimal? TollFare);

    public sealed record 배차삽입경로예상결과(
        decimal? 기존경로거리Km,
        decimal? 삽입경로거리Km,
        decimal? 기존경로소요시간분,
        decimal? 삽입경로소요시간분,
        decimal? 기존배송지연분,
        decimal? 삽입추가톨비);

    public sealed record 배차추천판정결과(
        string 추천유형,
        decimal 허용지연분,
        bool 화물민감여부,
        bool 단독배송여부,
        bool 묶음삽입가능,
        bool 도착후추천가능,
        bool 차량적합여부,
        string[] 차량부적합사유,
        string[] 차량경고);

    public sealed record 배차추천평가결과(
        decimal? 추천점수,
        string[] 배지,
        string[] 경고,
        string 추천사유);
}