using MediatR;

namespace Ssalddel.Application.Food.Events;

/// <summary>
/// 음식 배달의 기사 배정·픽업·고객 인계가 저장된 뒤 발행하는 비식별 활동 이벤트입니다.
/// 식별자는 활동 투영의 멱등 키 계산에만 쓰이며 공개 응답에는 포함되지 않습니다.
/// </summary>
public sealed record 음식배달인계상태변경됨Event(
    string 배차의뢰Id,
    string 주문번호,
    string 상태,
    DateTime 발생시각Utc,
    string EventId) : INotification;
