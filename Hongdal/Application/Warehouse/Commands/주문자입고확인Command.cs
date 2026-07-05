using FluentResults;

namespace Hongdal.Application.Warehouse;

public sealed record 주문자입고확인Command(
    string 주문자UserId,
    long? 주문Id,
    string 주문참조번호) : IRequest<Result<Unit>>;
