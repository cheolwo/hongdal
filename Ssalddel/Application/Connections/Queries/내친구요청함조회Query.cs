using MediatR;
using System.Text.Json.Serialization;

namespace Ssalddel.Application.Connections.Queries;

public sealed record 내친구요청함조회Query(int 페이지 = 1, int 페이지크기 = 50) : IRequest<IReadOnlyList<친구요청항목응답>>;

public sealed record 내친구수신함조회Query(int 페이지 = 1, int 페이지크기 = 50) : IRequest<IReadOnlyList<친구요청항목응답>>;

public sealed class 친구요청항목응답
{
    [JsonPropertyName("friendRequestId")]
    public long 친구요청Id { get; init; }

    // 기존 공개 응답 consumer를 위한 읽기 전용 호환 별칭이다.
    [JsonPropertyName("인연연결요청Id")]
    [Obsolete("friendRequestId를 사용하세요.")]
    public long LegacyFriendRequestId => 친구요청Id;
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
