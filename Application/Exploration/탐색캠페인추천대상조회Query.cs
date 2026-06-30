using Hongdal.Contracts.Common.Exploration;

namespace Hongdal.Application.Exploration;

public sealed record 탐색캠페인추천대상조회Query(string 개시자UserId, string 개시자역할, long 탐색캠페인Id) : IRequest<IReadOnlyList<탐색캠페인추천대상응답>>;
