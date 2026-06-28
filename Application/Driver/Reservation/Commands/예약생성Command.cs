using FluentResults;

namespace Hongdal.Application.Driver.Reservation;

public sealed record 예약생성Command(string 기사Id, string 시작모드, DateTime? 시작시각, string 시작위치, string? 복귀지) : IRequest<Result<Hongdal.Contracts.Driver.Reservation.기사예약응답>>;
