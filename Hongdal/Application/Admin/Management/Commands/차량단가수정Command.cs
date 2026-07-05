using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed record 차량단가수정Command(long Id, string 차량종류, decimal 기본운임, decimal Km당단가, decimal 야간할증, decimal 우천할증, decimal 최소운임) : IRequest<차량단가응답?>;
