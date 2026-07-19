using MediatR;

namespace Ssalddel.Application.Driver.Transport;

public sealed record 운송문제신고됨Event(
    string 기사Id,
    long 운송Id,
    string 운송번호,
    string 단계,
    string 예외코드,
    string 사유,
    string? 메모,
    string? 증빙ObjectName,
    string? 증빙Url,
    bool 관리자확인필요,
    DateTime 발생시각Utc,
    string TraceId) : INotification;

