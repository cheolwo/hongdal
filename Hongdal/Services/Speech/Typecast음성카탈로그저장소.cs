using Hongdal.Domain.Speech;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Services.Speech;

public sealed record Typecast음성카탈로그검색조건(
    string? 모델,
    string? 성별,
    string? 연령대,
    string? 용도,
    string? 음성유형,
    bool 활성화된항목만 = true);

public interface ITypecast음성카탈로그저장소
{
    Task<List<Typecast음성>> 전체추적조회Async(CancellationToken cancellationToken);

    Task<IReadOnlyList<Typecast음성>> 검색Async(
        Typecast음성카탈로그검색조건 조건,
        CancellationToken cancellationToken);

    Task<Typecast음성?> 단건조회Async(string voiceId, CancellationToken cancellationToken);

    void 추가(Typecast음성 음성);

    Task 저장Async(CancellationToken cancellationToken);
}

public sealed class EfTypecast음성카탈로그저장소 : ITypecast음성카탈로그저장소
{
    private readonly HongdalContext _db;

    public EfTypecast음성카탈로그저장소(HongdalContext db)
    {
        _db = db;
    }

    public Task<List<Typecast음성>> 전체추적조회Async(CancellationToken cancellationToken)
        => _db.Typecast음성
            .Include(x => x.지원모델)
            .Include(x => x.용도)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Typecast음성>> 검색Async(
        Typecast음성카탈로그검색조건 조건,
        CancellationToken cancellationToken)
    {
        var query = _db.Typecast음성
            .AsNoTracking()
            .Include(x => x.지원모델)
            .Include(x => x.용도)
            .AsQueryable();

        if (조건.활성화된항목만)
        {
            query = query.Where(x => x.활성화여부);
        }

        if (!string.IsNullOrWhiteSpace(조건.모델))
        {
            query = query.Where(x => x.지원모델.Any(model => model.버전 == 조건.모델));
        }

        if (!string.IsNullOrWhiteSpace(조건.성별))
        {
            query = query.Where(x => x.성별 == 조건.성별);
        }

        if (!string.IsNullOrWhiteSpace(조건.연령대))
        {
            query = query.Where(x => x.연령대 == 조건.연령대);
        }

        if (!string.IsNullOrWhiteSpace(조건.용도))
        {
            query = query.Where(x => x.용도.Any(useCase => useCase.이름 == 조건.용도));
        }

        if (!string.IsNullOrWhiteSpace(조건.음성유형))
        {
            query = query.Where(x => x.음성유형 == 조건.음성유형);
        }

        return await query
            .OrderBy(x => x.이름)
            .ThenBy(x => x.VoiceId)
            .ToListAsync(cancellationToken);
    }

    public Task<Typecast음성?> 단건조회Async(string voiceId, CancellationToken cancellationToken)
        => _db.Typecast음성
            .AsNoTracking()
            .Include(x => x.지원모델)
            .Include(x => x.용도)
            .SingleOrDefaultAsync(x => x.VoiceId == voiceId, cancellationToken);

    public void 추가(Typecast음성 음성)
        => _db.Typecast음성.Add(음성);

    public Task 저장Async(CancellationToken cancellationToken)
        => _db.SaveChangesAsync(cancellationToken);
}
