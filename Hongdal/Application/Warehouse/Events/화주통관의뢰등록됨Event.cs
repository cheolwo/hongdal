using MediatR;
using 홍달.도메인.통관;

namespace Hongdal.Application.Warehouse;

public sealed record 화주통관의뢰등록됨Event(
    long 통관절차Id,
    string 화주UserId,
    string 의뢰유형,
    물류거래방향 물류거래방향,
    string? 대상관세사참여자Id,
    string? 대표상품명,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
