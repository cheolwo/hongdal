using Hongdal.Contracts.Admin.Inbound;

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
        var entity = new 홍달.도메인.배차.배차대기
        {
            의뢰Id = request.의뢰Id,
            화주Id = request.화주Id,
            픽업_도로명주소 = request.픽업_도로명주소,
            픽업_상세주소 = request.픽업_상세주소,
            픽업_위도 = request.픽업_위도,
            픽업_경도 = request.픽업_경도,
            하차_도로명주소 = request.하차_도로명주소,
            하차_상세주소 = request.하차_상세주소,
            하차_위도 = request.하차_위도,
            하차_경도 = request.하차_경도,
            상태 = string.IsNullOrWhiteSpace(request.상태) ? 홍달.도메인.공통.상태값.배차대기상태.대기 : request.상태,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _db.배차대기.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
