using Ssalddel.Contracts.Food;
using MediatR;

namespace Ssalddel.Application.Food.Commands;

public sealed record 음식점주문수락Command(
    string 주문번호,
    음식점주문수락요청 Payload,
    string? 처리UserId) : IRequest<음식주문응답?>;
