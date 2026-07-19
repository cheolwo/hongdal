using Ssalddel.Contracts.Admin.Management;

namespace Ssalddel.Application.Admin.Management;

public sealed record 관리자기사배차내역조회Query(string DriverId) : IRequest<IReadOnlyList<기사배차내역응답>>;
