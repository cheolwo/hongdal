using FluentResults;
using Ssalddel.Application.Abstractions;
using 살뜰.도메인.사용자;

namespace Ssalddel.Application.Driver.DispatchAction;

public sealed record 배차수락취소Command : 살뜰CommandBase, IRequest<Result<배차수락취소결과>>
{
    public 배차수락취소Command(string driverId, string requestId, string? 사유 = null)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        RequestId = requestId;
        this.사유 = 사유;
        참여자Id = 기사Id;
        실행역할 = 살뜰역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;

    public string RequestId { get; init; } = string.Empty;

    public string? 사유 { get; init; }
}

public sealed record 배차수락취소결과(string RequestId, string Message);
