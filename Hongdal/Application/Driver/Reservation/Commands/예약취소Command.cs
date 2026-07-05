using Hongdal.Contracts.Driver.Reservation;
using Hongdal.Application.Abstractions;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Driver.Reservation;

public sealed record 예약취소Command : 홍달CommandBase, IRequest<기사예약취소응답>
{
    public 예약취소Command(string driverId, long id)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        Id = id;
        참여자Id = 기사Id;
        실행역할 = 홍달역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;
    public long Id { get; init; }
}
