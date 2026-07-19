using Ssalddel.Contracts.Food;
using MediatR;

namespace Ssalddel.Application.Food.Commands;

public sealed record 음식주문등록Command(음식주문등록요청 Payload)
    : IRequest<음식주문응답>;
