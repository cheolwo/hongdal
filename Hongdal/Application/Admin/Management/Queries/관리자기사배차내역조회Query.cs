using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed record 관리자기사배차내역조회Query(string DriverId) : IRequest<IReadOnlyList<기사배차내역응답>>;
