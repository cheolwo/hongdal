using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed record 의뢰목록조회Query(
    string? ShipperId,
    string? Status,
    string? PaymentStatus,
    string? DispatchStatus,
    int Page,
    int PageSize) : IRequest<IReadOnlyList<화주운송의뢰응답>>;
