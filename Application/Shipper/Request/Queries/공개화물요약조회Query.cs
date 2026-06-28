using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed record 공개화물요약조회Query(int Page, int PageSize) : IRequest<IReadOnlyList<공개화물요약응답>>;
