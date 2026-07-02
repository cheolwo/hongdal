using FluentResults;

namespace Hongdal.Application.Driver.Work;

public sealed record 운행시작Command(
    string 기사Id,
    string 시작모드,
    DateTime? 시작시각,
    string 시작위치,
    string? 복귀지,
    string? 오늘의복귀지주소,
    decimal? 오늘의복귀지위도,
    decimal? 오늘의복귀지경도,
    bool 기본복귀지사용,
    string? 복귀지출처) : IRequest<Result<Hongdal.Contracts.Driver.Work.기사운행시작응답>>;
