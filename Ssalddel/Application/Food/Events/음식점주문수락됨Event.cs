using Ssalddel.Contracts.Food;
using MediatR;

namespace Ssalddel.Application.Food.Events;

public sealed record 음식점주문수락됨Event(
    음식주문응답 주문,
    string? 처리UserId,
    DateTime 발생시각Utc,
    string EventId) : INotification;
