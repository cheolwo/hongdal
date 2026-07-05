using Hongdal.Contracts.Shipper.Payment;

namespace Hongdal.Application.Shipper.Payment;

public sealed record 결제목록조회Query(string? 결제상태, string? 의뢰Id, int Page, int PageSize) : IRequest<IReadOnlyList<결제목록응답>>;
