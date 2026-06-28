using Hongdal.Contracts.Admin.Progress;

using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed record 운송이벤트수정Command(long Id, string 의뢰Id, string 이벤트타입, DateTime 이벤트시각, string? 메타데이터) : IRequest<운송이벤트로그응답?>;
