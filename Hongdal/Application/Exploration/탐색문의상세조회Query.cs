using Hongdal.Contracts.Common.Exploration;

namespace Hongdal.Application.Exploration;

public sealed record 탐색문의상세조회Query(string 대상UserId, string 대상역할, long 탐색캠페인Id) : IRequest<탐색문의상세응답?>;
