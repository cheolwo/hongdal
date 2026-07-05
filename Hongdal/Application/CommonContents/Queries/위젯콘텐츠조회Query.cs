using Hongdal.Contracts.CommonContents;

namespace Hongdal.Application.CommonContents.Queries;

public sealed record 위젯콘텐츠조회Query(string 역할, string 위치) : IRequest<홍달위젯콘텐츠Dto?>;