using MediatR;

namespace Hongdal.Application.ProductJourney.Queries;

public sealed record 스캔코드기반상품여정조회Query(string 코드값) : IRequest<상품여정조회응답?>;

public sealed class 상품여정조회응답
{
    public string 코드값 { get; init; } = string.Empty;
    public long 상품Id { get; init; }
    public string 상품명 { get; init; } = string.Empty;
    public long? 주문Id { get; init; }
    public IReadOnlyList<상품여정단계응답> 단계목록 { get; init; } = Array.Empty<상품여정단계응답>();
}

public sealed class 상품여정단계응답
{
    public string 단계코드 { get; init; } = string.Empty;
    public string 단계명 { get; init; } = string.Empty;
    public string 상태 { get; init; } = string.Empty;
    public DateTimeOffset? 시각 { get; init; }
    public IReadOnlyList<처리주체응답> 처리주체목록 { get; init; } = Array.Empty<처리주체응답>();
}

public sealed class 처리주체응답
{
    public string 참여자Id { get; init; } = string.Empty;
    public string 역할 { get; init; } = string.Empty;
    public string 표시명 { get; init; } = string.Empty;
    public bool 감사가능 { get; init; }
    public bool 인연연결가능 { get; init; }
}
