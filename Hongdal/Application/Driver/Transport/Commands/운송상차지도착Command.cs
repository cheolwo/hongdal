using Hongdal.Contracts.Driver.Transport;
using FluentResults;
using Hongdal.Application.Abstractions;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송상차지도착Command : 홍달CommandBase, IRequest<Result<기사운송상태변경응답>>
{
    public 운송상차지도착Command(string driverId, long id)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        Id = id;
        참여자Id = 기사Id;
        실행역할 = 홍달역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;

    public long Id { get; init; }
}
