using MediatR;

namespace Hongdal.Application.Warehouse;

public sealed record 통관수임요청됨Event(
    long 통관절차Id,
    string 관세사참여자Id,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
