using Hongdal.Contracts.Food;
using MediatR;

namespace Hongdal.Application.Food.Queries;

public sealed record 음식주문상세조회Query(string 주문번호) : IRequest<음식주문응답?>;
