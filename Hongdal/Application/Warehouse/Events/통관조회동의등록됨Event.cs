using MediatR;

namespace Hongdal.Application.Warehouse;

public sealed record 통관조회동의등록됨Event(
    long 주문Id,
    long 통관절차Id,
    string 사용자Id,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
