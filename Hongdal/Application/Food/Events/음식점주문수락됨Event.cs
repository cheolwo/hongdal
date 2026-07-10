using Hongdal.Contracts.Food;
using MediatR;

namespace Hongdal.Application.Food.Events;

public sealed record 음식점주문수락됨Event(
    음식주문응답 주문,
    DateTime 발생시각Utc,
    string EventId) : INotification;
