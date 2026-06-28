using Hongdal.Contracts.Driver.Reservation;

namespace Hongdal.Application.Driver.Reservation;

public sealed record 예약취소Command(string 기사Id, long Id) : IRequest<기사예약취소응답>;
