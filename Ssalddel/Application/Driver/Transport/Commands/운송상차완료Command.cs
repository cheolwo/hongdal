using Ssalddel.Contracts.Driver.Transport;
using FluentResults;
using Ssalddel.Application.Abstractions;
using 살뜰.도메인.사용자;

namespace Ssalddel.Application.Driver.Transport;

public sealed record 운송상차완료Command : 살뜰CommandBase, IRequest<Result<기사운송상태변경응답>>
{
    public 운송상차완료Command(string driverId, long id)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        Id = id;
        참여자Id = 기사Id;
        실행역할 = 살뜰역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;

    public long Id { get; init; }

    public string? 상차사진ObjectName { get; init; }

    public string? 상차사진Url { get; init; }

    public string? 인수증증빙방식 { get; init; }

    public string? 인수자명 { get; init; }

    public string? 인수자소속 { get; init; }

    public string? 인수자서명 { get; init; }

    public string? 기사서명 { get; init; }

    public bool 인수증확인완료 { get; init; }

    public bool 인수증서명생략확인 { get; init; }

    public string? 인수증서명생략사유 { get; init; }
}
