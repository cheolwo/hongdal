using Hongdal.Contracts.Common.Exploration;

namespace Hongdal.Application.Exploration;

public sealed record 탐색캠페인목록조회Query(string 개시자UserId, string 개시자역할) : IRequest<IReadOnlyList<탐색캠페인목록항목응답>>;
