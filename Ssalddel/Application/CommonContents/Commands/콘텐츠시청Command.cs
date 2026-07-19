using Ssalddel.Contracts.CommonContents;

namespace Ssalddel.Application.CommonContents.Commands;

public sealed record 콘텐츠시청시작Command(long 콘텐츠Id, int 영상전체초) : IRequest<콘텐츠시청시작Result?>;

public sealed record 콘텐츠시청진행Command(long 세션Id, int 현재시청초) : IRequest<bool>;

public sealed record 콘텐츠시청완료Command(long 세션Id) : IRequest<콘텐츠시청완료Result?>;

public sealed record 결제혜택견적조회Query(string 사용자Id, int 원운임) : IRequest<결제혜택견적응답>;