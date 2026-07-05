using 홍달.도메인.운송;

namespace Hongdal.Application.Admin.Management;

public sealed record 운임구성수정Command(long Id, string 의뢰Id, decimal 기본운임, decimal 거리운임, decimal 할증, decimal 대기료, decimal 수작업비, decimal 최종운임) : IRequest<운임구성?>;
