using Hongdal.Contracts.Admin.Settlement;

namespace Hongdal.Application.Admin.Settlement;

public sealed record 기사월정산관리목록조회Query(int? Year, int? Month, string? DriverId) : IRequest<IReadOnlyList<기사월정산관리응답>>;
