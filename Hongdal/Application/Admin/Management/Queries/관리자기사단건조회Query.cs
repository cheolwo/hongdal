using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed record 관리자기사단건조회Query(string DriverId) : IRequest<기사상세응답?>;
