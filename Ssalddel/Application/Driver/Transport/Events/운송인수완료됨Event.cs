using MediatR;

namespace Ssalddel.Application.Driver.Transport;

public sealed record 운송인수완료됨Event(
    long 운송Id,
    string 운송번호,
    string 기사Id,
    string 출발지,
    string 도착지,
    string 상태,
    DateTime 발생시각Utc,
    string TraceId,
    운송하차완료증빙? 하차완료증빙) : INotification;

public sealed record 운송하차완료증빙(
    string 하차사진ObjectName,
    string? 하차사진Url);
