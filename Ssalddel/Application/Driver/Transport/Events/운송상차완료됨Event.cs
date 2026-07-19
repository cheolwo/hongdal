using MediatR;

namespace Ssalddel.Application.Driver.Transport;

public sealed record 운송상차완료됨Event(
    string 기사Id,
    long 운송Id,
    string 운송번호,
    string 출발지,
    string 도착지,
    string 이전상태,
    string 현재상태,
    DateTime 발생시각Utc,
    string TraceId,
    운송상차인수증증빙? 인수증증빙) : INotification;

public sealed record 운송상차인수증증빙(
    bool 서명확보됨,
    bool 서명필수여부,
    string 증빙방식,
    string? 인수자명,
    string? 인수자소속,
    string? 인수자서명,
    string? 기사서명,
    string? 서명생략사유,
    string? 상차사진ObjectName,
    string? 상차사진Url);
