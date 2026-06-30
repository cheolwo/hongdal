using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed record 차량단가단건조회Query(long Id) : IRequest<차량단가응답?>;
