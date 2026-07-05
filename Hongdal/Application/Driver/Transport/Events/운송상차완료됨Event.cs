using MediatR;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송상차완료됨Event(
    string 기사Id,
    long 운송Id,
    string 이전상태,
    string 현재상태,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
