using Ssalddel.Contracts.Admin.Inbound;
using Ssalddel.Services.Community;
using 살뜰.도메인.운송;

namespace Ssalddel.Application.Admin.Inbound;

public sealed class 배차대기수정CommandHandler : IRequestHandler<배차대기수정Command, 운송원장?>
{
    private readonly SsalddelContext _db;
    private readonly I운송원장Mongo동기화Service _transportLedgerSync;

    public 배차대기수정CommandHandler(
        SsalddelContext db,
        I운송원장Mongo동기화Service transportLedgerSync)
    {
        _db = db;
        _transportLedgerSync = transportLedgerSync;
    }

    public async Task<운송원장?> Handle(배차대기수정Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.운송원장.FindAsync([request.Id], cancellationToken);
        if (entity == null)
        {
            return null;
        }

        entity.의뢰Id = request.의뢰Id;
        entity.화주Id = request.화주Id;
        if (request.배차업무유형.HasValue)
        {
            entity.배차업무유형 = request.배차업무유형.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.원본의뢰유형))
        {
            entity.원본의뢰유형 = request.원본의뢰유형.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.원본의뢰Id))
        {
            entity.원본의뢰Id = request.원본의뢰Id.Trim();
        }

        entity.공동구매도착지유형코드 = NormalizeOptional(request.공동구매도착지유형코드);
        entity.공동구매기사세대배송여부 = request.공동구매기사세대배송여부;
        entity.공동구매세대배송방식코드 = NormalizeOptional(request.공동구매세대배송방식코드);
        entity.공동구매세대배송건수 = request.공동구매세대배송건수 is > 0 ? request.공동구매세대배송건수 : null;
        entity.공동구매분배책임코드 = NormalizeOptional(request.공동구매분배책임코드);
        entity.픽업_도로명주소 = request.픽업_도로명주소;
        entity.픽업_상세주소 = request.픽업_상세주소;
        entity.픽업_위도 = request.픽업_위도;
        entity.픽업_경도 = request.픽업_경도;
        entity.하차_도로명주소 = request.하차_도로명주소;
        entity.하차_상세주소 = request.하차_상세주소;
        entity.하차_위도 = request.하차_위도;
        entity.하차_경도 = request.하차_경도;
        entity.상태 = string.IsNullOrWhiteSpace(request.상태) ? entity.상태 : request.상태;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await _transportLedgerSync.운송실행투영동기화Async(entity, "admin", cancellationToken);
        return entity;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
