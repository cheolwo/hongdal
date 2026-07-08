using Hongdal.Contracts.Common.Warehouse;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.공통;
using 홍달.도메인.배차;
using 홍달.Services.Dispatch.Engine;

namespace 홍달.Services.Dispatch.Queue;

public sealed class 운송의뢰배차대기생성옵션
{
    public string? 의뢰Id { get; init; }

    public string? 화주Id { get; init; }

    public int? 배차업무유형 { get; init; }

    public string? 원본의뢰유형 { get; init; }

    public string? 원본의뢰Id { get; init; }

    public string? 픽업상세주소 { get; init; }

    public string? 하차상세주소 { get; init; }

    public string? 상태 { get; init; }

    public string? 공동구매도착지유형코드 { get; init; }

    public bool? 공동구매기사세대배송여부 { get; init; }

    public string? 공동구매세대배송방식코드 { get; init; }

    public int? 공동구매세대배송건수 { get; init; }

    public string? 공동구매분배책임코드 { get; init; }
}

public interface I운송의뢰배차대기Service
{
    Task<배차대기> 생성또는조회Async(
        출고예정운송대상 target,
        운송의뢰배차대기생성옵션? options = null,
        CancellationToken cancellationToken = default);
}

public sealed class 운송의뢰배차대기Service : I운송의뢰배차대기Service
{
    private readonly HongdalContext _db;
    private readonly I운송의뢰배차원천분류Service _sourceClassifier;

    public 운송의뢰배차대기Service(
        HongdalContext db,
        I운송의뢰배차원천분류Service sourceClassifier)
    {
        _db = db;
        _sourceClassifier = sourceClassifier;
    }

    public async Task<배차대기> 생성또는조회Async(
        출고예정운송대상 target,
        운송의뢰배차대기생성옵션? options = null,
        CancellationToken cancellationToken = default)
    {
        var requestId = NormalizeRequired(options?.의뢰Id, target.운송의뢰Id, target.원천참조번호, "운송 의뢰 ID가 필요합니다.");
        var existing = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTime.UtcNow;
        var sourceType = NormalizeOptional(options?.원본의뢰유형)
                         ?? To배차원천유형(target.원천유형);

        var entity = new 배차대기
        {
            의뢰Id = requestId,
            화주Id = NormalizeRequired(options?.화주Id, target.판매자UserId, null, "배차대기 화주 ID가 필요합니다."),
            원본의뢰유형 = sourceType,
            원본의뢰Id = NormalizeOptional(options?.원본의뢰Id) ?? target.원천참조번호,
            공동구매도착지유형코드 = NormalizeOptional(options?.공동구매도착지유형코드),
            공동구매기사세대배송여부 = options?.공동구매기사세대배송여부,
            공동구매세대배송방식코드 = NormalizeOptional(options?.공동구매세대배송방식코드),
            공동구매세대배송건수 = options?.공동구매세대배송건수 is > 0 ? options.공동구매세대배송건수 : null,
            공동구매분배책임코드 = NormalizeOptional(options?.공동구매분배책임코드),
            픽업_도로명주소 = target.상차주소,
            픽업_상세주소 = NormalizeOptional(options?.픽업상세주소) ?? string.Empty,
            픽업_위도 = target.상차위도,
            픽업_경도 = target.상차경도,
            하차_도로명주소 = target.하차주소,
            하차_상세주소 = NormalizeOptional(options?.하차상세주소) ?? string.Empty,
            하차_위도 = target.하차위도,
            하차_경도 = target.하차경도,
            상태 = NormalizeOptional(options?.상태) ?? 상태값.배차대기상태.대기,
            배차큐단계 = 상태값.배차큐단계.계획배차,
            배차노출상태 = 상태값.배차노출상태.계획대기,
            CreatedAt = now,
            UpdatedAt = now
        };

        var source = _sourceClassifier.분류(entity);
        entity.배차업무유형 = options?.배차업무유형 ?? source.배차업무유형;

        _db.배차대기.Add(entity);
        return entity;
    }

    private static string To배차원천유형(string? sourceType)
    {
        return sourceType switch
        {
            출고예정운송대상원천유형.화주운송의뢰 => 운송의뢰배차원천유형.화주운송의뢰,
            출고예정운송대상원천유형.창고출고예정 => 운송의뢰배차원천유형.창고출고연계운송,
            출고예정운송대상원천유형.판매채널주문 => 운송의뢰배차원천유형.판매채널출고,
            출고예정운송대상원천유형.공동주문수입 => 운송의뢰배차원천유형.공동주문국내운송,
            _ when string.IsNullOrWhiteSpace(sourceType) => 운송의뢰배차원천유형.화주운송의뢰,
            _ => sourceType.Trim()
        };
    }

    private static string NormalizeRequired(string? primary, string? secondary, string? fallback, string errorMessage)
    {
        var value = NormalizeOptional(primary) ?? NormalizeOptional(secondary) ?? NormalizeOptional(fallback);
        return value ?? throw new InvalidOperationException(errorMessage);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
