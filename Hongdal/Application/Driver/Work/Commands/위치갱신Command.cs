using FluentResults;
using Hongdal.Application.Abstractions;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Driver.Work;

public sealed record 위치갱신Command : 홍달CommandBase, IRequest<Result<기사위치갱신응답>>
{
    public 위치갱신Command(
        string driverId,
        string? appKey,
        decimal? 위도,
        decimal? 경도,
        decimal? 정확도_m,
        decimal? 상차접근허용반경Km,
        string? 운행상태,
        DateTime? 기록시각)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        AppKey = string.IsNullOrWhiteSpace(appKey) ? null : appKey.Trim();
        this.위도 = 위도;
        this.경도 = 경도;
        this.정확도_m = 정확도_m;
        this.상차접근허용반경Km = 상차접근허용반경Km;
        this.운행상태 = 운행상태;
        this.기록시각 = 기록시각;
        참여자Id = 기사Id;
        실행역할 = 홍달역할유형.기사;
    }

    public string 기사Id { get; init; }
    public string? AppKey { get; init; }
    public decimal? 위도 { get; init; }
    public decimal? 경도 { get; init; }
    public decimal? 정확도_m { get; init; }
    public decimal? 상차접근허용반경Km { get; init; }
    public string? 운행상태 { get; init; }
    public DateTime? 기록시각 { get; init; }
}
