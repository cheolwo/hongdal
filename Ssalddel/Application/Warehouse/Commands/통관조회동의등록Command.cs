using Ssalddel.Application.Abstractions;
using FluentResults;
using MediatR;

namespace Ssalddel.Application.Warehouse;

public sealed record 통관조회동의등록Command(
    string 사용자Id,
    long 주문Id,
    long 통관절차Id,
    string 개인통관고유부호,
    string 수취인이름,
    string 휴대폰번호,
    string? 우편번호,
    string 참여자Id,
    살뜰.도메인.사용자.살뜰역할유형 실행역할) : IRequest<Result<Unit>>;
