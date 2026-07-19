using FluentResults;
using 살뜰.도메인.통관;

namespace Ssalddel.Application.Warehouse;

public sealed record 화주HsCode검토요청Command(
    string 화주UserId,
    string 대표상품명,
    물류거래방향 물류거래방향,
    string? 주문참조번호,
    long? 주문Id,
    string? 대상관세사참여자Id,
    string? 요청메모) : IRequest<Result<화주통관의뢰등록결과>>;
