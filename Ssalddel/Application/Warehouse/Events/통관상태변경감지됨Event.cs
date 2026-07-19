using MediatR;
using 살뜰.도메인.통관;

namespace Ssalddel.Application.Warehouse;

public sealed record 통관상태변경감지됨Event(
    long 주문Id,
    long 통관절차Id,
    통관진행단계 이전단계,
    통관진행단계 현재단계,
    string? 처리단계명,
    DateTime 발생시각Utc,
    string TraceId) : INotification;
