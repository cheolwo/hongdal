using FluentResults;
using 홍달.도메인.통관;

namespace Hongdal.Application.Warehouse;

public sealed record 화주통관의뢰등록Command(
    string 화주UserId,
    string 의뢰유형,
    물류거래방향 물류거래방향,
    string 대표상품명,
    string? 주문참조번호,
    long? 주문Id,
    long? 출고창고Id,
    long? 입고창고Id,
    string? 대상관세사참여자Id,
    string? 요청메모) : IRequest<Result<화주통관의뢰등록결과>>;

public sealed record 화주통관의뢰등록결과(
    long 통관절차Id,
    string 의뢰유형,
    통관절차상태 상태);
