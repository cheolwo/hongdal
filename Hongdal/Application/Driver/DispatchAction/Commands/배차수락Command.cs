using FluentResults;
using Hongdal.Application.Abstractions;
using Hongdal.Application.CommandProcessing;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed record 배차수락Command : 홍달CommandBase, IRequest<Result<배차수락결과>>, IWorkRelationshipSnapshotCommand
{
    public 배차수락Command(string driverId, string requestId)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        RequestId = requestId;
        참여자Id = 기사Id;
        실행역할 = 홍달역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;

    public string RequestId { get; init; } = string.Empty;
}

public sealed record 배차수락결과(string RequestId, string Message);
