using MediatR;
using Ssalddel.Contracts.Food;

namespace Ssalddel.Application.Food.Commands;

public sealed record 주문자음식주문수령확인Command(
    string 주문번호,
    주문자음식주문수령확인요청 Payload,
    string 주문자UserId) : IRequest<음식주문응답?>;
