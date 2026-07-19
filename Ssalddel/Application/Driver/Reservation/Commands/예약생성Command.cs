using FluentResults;
using Ssalddel.Application.Abstractions;
using 살뜰.도메인.사용자;

namespace Ssalddel.Application.Driver.Reservation;

public sealed record 예약생성Command : 살뜰CommandBase, IRequest<Result<Ssalddel.Contracts.Driver.Reservation.기사예약응답>>
{
    public 예약생성Command(string driverId, string 시작모드, DateTime? 시작시각, string 시작위치, string? 복귀지)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        this.시작모드 = 시작모드;
        this.시작시각 = 시작시각;
        this.시작위치 = 시작위치;
        this.복귀지 = 복귀지;
        참여자Id = 기사Id;
        실행역할 = 살뜰역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;
    public string 시작모드 { get; init; } = string.Empty;
    public DateTime? 시작시각 { get; init; }
    public string 시작위치 { get; init; } = string.Empty;
    public string? 복귀지 { get; init; }
}
