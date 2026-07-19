using FluentResults;

namespace Ssalddel.Application.Warehouse;

public sealed record 판매자출고처리Command(
    string 판매자UserId,
    long? 주문Id,
    string 주문참조번호) : IRequest<Result<Unit>>;
