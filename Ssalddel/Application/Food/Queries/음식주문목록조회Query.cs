using Ssalddel.Contracts.Food;
using MediatR;

namespace Ssalddel.Application.Food.Queries;

public sealed record 음식주문목록조회Query : IRequest<음식주문목록응답>;
