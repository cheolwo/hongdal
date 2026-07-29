using MediatR;
using Ssalddel.Contracts.Food;

namespace Ssalddel.Application.Food.Events;

public sealed record 음식점주문진행변경됨Event(
    음식주문응답 주문,
    string 처리UserId,
    string 작업,
    string 변경사유,
    DateTime 발생시각Utc,
    string EventId) : INotification;
