using MediatR;

namespace Ssalddel.Application.Driver.DispatchAction;

public sealed record 배차수락됨Event(
    string 기사Id,
    string 화주Id,
    string 의뢰Id,
    string 배차대기상태,
    string 의뢰배차상태,
    string 의뢰결제상태,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
