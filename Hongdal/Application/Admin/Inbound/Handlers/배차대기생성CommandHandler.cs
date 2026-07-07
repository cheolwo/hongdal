using Hongdal.Contracts.Admin.Inbound;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Admin.Inbound;

public sealed class 배차대기생성CommandHandler : IRequestHandler<배차대기생성Command, 홍달.도메인.배차.배차대기>
{
    private readonly HongdalContext _db;

    public 배차대기생성CommandHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<홍달.도메인.배차.배차대기> Handle(배차대기생성Command request, CancellationToken cancellationToken)
    {
        var existing = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == request.의뢰Id, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTime.UtcNow;
        var entity = new 홍달.도메인.배차.배차대기
        {
            의뢰Id = request.의뢰Id,
            화주Id = request.화주Id,
            배차업무유형 = request.배차업무유형 ?? 홍달.도메인.공통.상태값.배차업무유형.용달운송,
            원본의뢰유형 = string.IsNullOrWhiteSpace(request.원본의뢰유형) ? "CargoTransport" : request.원본의뢰유형.Trim(),
            원본의뢰Id = string.IsNullOrWhiteSpace(request.원본의뢰Id) ? request.의뢰Id : request.원본의뢰Id.Trim(),
            공동구매도착지유형코드 = NormalizeOptional(request.공동구매도착지유형코드),
            공동구매기사세대배송여부 = request.공동구매기사세대배송여부,
            공동구매세대배송방식코드 = NormalizeOptional(request.공동구매세대배송방식코드),
            공동구매세대배송건수 = request.공동구매세대배송건수 is > 0 ? request.공동구매세대배송건수 : null,
            공동구매분배책임코드 = NormalizeOptional(request.공동구매분배책임코드),
            픽업_도로명주소 = request.픽업_도로명주소,
            픽업_상세주소 = request.픽업_상세주소,
            픽업_위도 = request.픽업_위도,
            픽업_경도 = request.픽업_경도,
            하차_도로명주소 = request.하차_도로명주소,
            하차_상세주소 = request.하차_상세주소,
            하차_위도 = request.하차_위도,
            하차_경도 = request.하차_경도,
            상태 = string.IsNullOrWhiteSpace(request.상태) ? 홍달.도메인.공통.상태값.배차대기상태.대기 : request.상태,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _db.배차대기.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
