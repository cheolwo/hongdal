using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed class 배차계획단건조회QueryHandler : IRequestHandler<배차계획단건조회Query, 배차계획관리상세응답?>
{
    private readonly HongdalContext _db;

    public 배차계획단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<배차계획관리상세응답?> Handle(배차계획단건조회Query request, CancellationToken cancellationToken)
    {
        return await _db.배차계획신청
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new 배차계획관리상세응답
            {
                Id = x.Id,
                기사Id = x.기사Id,
                기사명 = _db.용달기사.Where(d => d.기사Id == x.기사Id).Select(d => d.기사명).FirstOrDefault() ?? string.Empty,
                연락처 = _db.용달기사.Where(d => d.기사Id == x.기사Id).Select(d => d.연락처).FirstOrDefault() ?? string.Empty,
                출발지 = x.출발지,
                복귀지 = x.복귀지,
                희망복귀시각 = x.희망복귀시각,
                배차가능시각 = x.배차가능시각,
                상태 = x.상태,
                메모 = x.메모,
                신청일시 = x.신청일시,
                최근수정시각 = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
