using MediatR;

namespace Ssalddel.Application.Connections.Queries;

public sealed record 내인연연결요청함조회Query(int 페이지 = 1, int 페이지크기 = 50) : IRequest<IReadOnlyList<인연연결요청항목응답>>;

public sealed record 내인연연결수신함조회Query(int 페이지 = 1, int 페이지크기 = 50) : IRequest<IReadOnlyList<인연연결요청항목응답>>;

public sealed class 인연연결요청항목응답
{
    public long 인연연결요청Id { get; init; }
    public string 요청자참여자Id { get; init; } = string.Empty;
    public string 대상자참여자Id { get; init; } = string.Empty;
    public string 요청자역할 { get; init; } = string.Empty;
    public string 대상자역할 { get; init; } = string.Empty;
    public string 상태 { get; init; } = string.Empty;
    public string 요청목적 { get; init; } = string.Empty;
    public string 요청메시지 { get; init; } = string.Empty;
    public DateTimeOffset 요청일시 { get; init; }
    public DateTimeOffset? 응답일시 { get; init; }
    public string? 거절사유 { get; init; }
}
