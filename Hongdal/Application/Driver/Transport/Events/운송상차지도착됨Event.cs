using MediatR;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송상차지도착됨Event(
    string 기사Id,
    long 운송Id,
    string 이전상태,
    string 현재상태,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
