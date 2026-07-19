using Ssalddel.Contracts.Common.Exploration;
using Microsoft.EntityFrameworkCore;

namespace Ssalddel.Application.Exploration;

public sealed class 탐색캠페인추천대상조회QueryHandler : IRequestHandler<탐색캠페인추천대상조회Query, IReadOnlyList<탐색캠페인추천대상응답>>
{
    private readonly SsalddelContext _db;
    private readonly I탐색대상추천Service _recommendationService;

    public 탐색캠페인추천대상조회QueryHandler(SsalddelContext db, I탐색대상추천Service recommendationService)
    {
        _db = db;
        _recommendationService = recommendationService;
    }

    public async Task<IReadOnlyList<탐색캠페인추천대상응답>> Handle(탐색캠페인추천대상조회Query request, CancellationToken cancellationToken)
    {
        var campaign = await _db.탐색캠페인
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.탐색캠페인Id && x.개시자UserId == request.개시자UserId && x.개시자역할 == request.개시자역할, cancellationToken);

        if (campaign is null)
        {
            return Array.Empty<탐색캠페인추천대상응답>();
        }

        return await _recommendationService.추천Async(campaign, cancellationToken);
    }
}
