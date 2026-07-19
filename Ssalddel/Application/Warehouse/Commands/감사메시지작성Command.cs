using FluentResults;
using MediatR;
using Ssalddel.Application.Abstractions;
using 살뜰.도메인.사용자;

namespace Ssalddel.Application.Warehouse;

public sealed record 감사메시지작성Command(
    long 상품Id,
    long? 주문Id,
    long? 통관절차Id,
    string 발신자구분,
    string? 발신참여자Id,
    string 대상역할,
    string? 대상참여자Id,
    string 대상표시명,
    string 메시지내용,
    bool 공개가능여부,
    string 참여자Id,
    살뜰역할유형 실행역할) : IRequest<Result<long>>;
