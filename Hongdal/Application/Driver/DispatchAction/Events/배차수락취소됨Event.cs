using MediatR;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed record 배차수락취소됨Event(
    string 기사Id,
    string 의뢰Id,
    string? 사유,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
