using Ssalddel.Contracts.Driver.Reservation;

namespace Ssalddel.Application.Driver.Reservation;

public sealed record 예약목록조회Query(string 기사Id) : IRequest<IReadOnlyList<기사예약목록응답>>;
