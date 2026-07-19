using MediatR;

namespace Ssalddel.Application.Warehouse;

public sealed record 주문자상품입고완료됨Event(
    long? 주문Id,
    string 주문참조번호,
    string 주문자UserId,
    IReadOnlyList<long> 입고요청Ids,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
