using MediatR;

namespace Ssalddel.Application.Warehouse;

public sealed record 판매자상품출고됨Event(
    long? 주문Id,
    string 주문참조번호,
    string 판매자UserId,
    IReadOnlyList<long> 출고예정Ids,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
