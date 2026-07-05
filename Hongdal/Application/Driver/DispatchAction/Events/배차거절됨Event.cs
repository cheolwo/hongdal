using MediatR;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed record 배차거절됨Event(
    string 기사Id,
    string 의뢰Id,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
