using FluentResults;
using Hongdal.Application.Abstractions;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed record 배차거절Command : 홍달CommandBase, IRequest<Result<배차거절결과>>
{
    public 배차거절Command(string driverId, string requestId, string? 사유 = null)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        RequestId = requestId;
        this.사유 = 사유;
        참여자Id = 기사Id;
        실행역할 = 홍달역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;

    public string RequestId { get; init; } = string.Empty;

    public string? 사유 { get; init; }
}

public sealed record 배차거절결과(string RequestId, string Message);
