using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed record 관리자기사목록조회Query(string? 운행상태, string? 차량종류, string? 활동지역검색어) : IRequest<IReadOnlyList<기사목록응답>>;
