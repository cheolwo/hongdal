using MediatR;

namespace Ssalddel.Application.Driver.Transport;

public sealed record 운송하차지도착됨Event(
    string 기사Id,
    long 운송Id,
    string 이전상태,
    string 현재상태,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
