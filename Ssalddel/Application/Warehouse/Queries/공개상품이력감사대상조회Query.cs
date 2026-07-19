using Ssalddel.Application.Abstractions;

namespace Ssalddel.Application.Warehouse;

public sealed record 공개상품이력감사대상조회Query(
    long 상품Id,
    long? 주문Id,
    long? 통관절차Id) : IQuery<IReadOnlyList<감사대상응답>>;

public sealed class 감사대상응답
{
    public string 대상역할 { get; init; } = string.Empty;
    public string? 대상참여자Id { get; init; }
    public string 대상표시명 { get; init; } = string.Empty;
    public string 역할설명 { get; init; } = string.Empty;
    public DateTimeOffset? 처리일시 { get; init; }
}
