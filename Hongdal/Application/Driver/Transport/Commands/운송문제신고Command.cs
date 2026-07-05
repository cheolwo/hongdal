using FluentResults;
using Hongdal.Application.Abstractions;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송문제신고Command : 홍달CommandBase, IRequest<Result<Hongdal.Contracts.Driver.Transport.기사운송요약응답>>
{
    public 운송문제신고Command(string driverId, long id, string 사유, string? 메모)
    {
        기사Id = string.IsNullOrWhiteSpace(driverId) ? string.Empty : driverId;
        Id = id;
        this.사유 = 사유;
        this.메모 = 메모;
        참여자Id = 기사Id;
        실행역할 = 홍달역할유형.기사;
    }

    public string 기사Id { get; init; } = string.Empty;
    public long Id { get; init; }
    public string 사유 { get; init; } = string.Empty;
    public string? 메모 { get; init; }
}
