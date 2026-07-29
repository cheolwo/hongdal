using MediatR;
using Ssalddel.Contracts.Food;

namespace Ssalddel.Application.Food.Commands;

public sealed record 음식점주문진행변경Command(
    string 주문번호,
    음식점주문진행변경요청 Payload,
    string 처리UserId) : IRequest<음식주문응답?>;
