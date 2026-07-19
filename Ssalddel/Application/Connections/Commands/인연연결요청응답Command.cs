using FluentResults;
using MediatR;

namespace Ssalddel.Application.Connections.Commands;

public sealed record 인연연결요청응답Command(
    long 인연연결요청Id,
    bool 수락,
    string? 거절사유,
    연락처공개동의입력? 공개동의) : IRequest<Result<Unit>>;

public sealed class 연락처공개동의입력
{
    public string 동의자참여자Id { get; init; } = string.Empty;
    public bool 프로필공개 { get; init; }
    public bool 업체명공개 { get; init; }
    public bool 이메일공개 { get; init; }
    public bool 전화번호공개 { get; init; }
    public bool 카카오채널공개 { get; init; }
    public bool 판매채널공개 { get; init; }
    public string 제공목적 { get; init; } = string.Empty;
}
