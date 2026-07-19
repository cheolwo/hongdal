using Ssalddel.Contracts.Driver.Reservation;
using Ssalddel.Application.Abstractions;
using 살뜰.도메인.사용자;

namespace Ssalddel.Application.Driver.Reservation;

public sealed record 예약취소Command : 살뜰CommandBase, IRequest<기사예약취소응답>
{
    public 예약취소Command(string driverId, long id)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        Id = id;
        참여자Id = 기사Id;
        실행역할 = 살뜰역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;
    public long Id { get; init; }
}
