using MediatR;
using 살뜰.도메인.통관;

namespace Ssalddel.Application.Warehouse;

public sealed record 통관절차생성됨Event(
    long 통관절차Id,
    long? 주문Id,
    string 주문참조번호,
    물류거래방향 물류거래방향,
    long 출고창고Id,
    long 입고창고Id,
    string? 대표상품명,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
