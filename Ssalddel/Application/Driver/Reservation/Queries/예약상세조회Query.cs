using Ssalddel.Contracts.Driver.Reservation;

namespace Ssalddel.Application.Driver.Reservation;

public sealed record 예약상세조회Query(string 기사Id, long Id) : IRequest<기사예약응답>;
