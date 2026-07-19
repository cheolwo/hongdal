using Ssalddel.Contracts.Admin.Progress;

namespace Ssalddel.Application.Admin.Operating;

public sealed record 운송이벤트생성Command(string 의뢰Id, string 이벤트타입, DateTime 이벤트시각, string? 메타데이터) : IRequest<운송이벤트로그응답>;
