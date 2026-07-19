using Microsoft.EntityFrameworkCore;
using 살뜰.Data;
using 살뜰.도메인.공통;
using 살뜰.도메인.판매;

namespace 살뜰.Services.Images;

public sealed class 샘플이미지대상항목
{
    public string 대상타입 { get; set; } = string.Empty;
    public string 대상식별자 { get; set; } = string.Empty;
    public string 이미지용도 { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string? 설명 { get; set; }
    public string? 추가맥락 { get; set; }
    public string 종횡비 { get; set; } = "auto";
    public string 해상도 { get; set; } = "1K";
    public bool 샘플데이터여부 { get; set; }
}

public sealed class 상품상세이미지생성작업대상Resolver : I샘플이미지대상Resolver
{
    public const string 대상타입값 = "상품상세이미지생성작업";

    private readonly SsalddelContext _db;

    public 상품상세이미지생성작업대상Resolver(SsalddelContext db)
    {
        _db = db;
    }

    public string 대상타입 => 대상타입값;
    public string 이미지용도 => 생성이미지용도.상품상세페이지생성이미지;

    public async Task<IReadOnlyList<샘플이미지대상항목>> GetMissingImageTargetsAsync(int maxCount, bool includeFailed, CancellationToken cancellationToken = default)
    {
        var query = _db.상품상세이미지생성작업
            .AsNoTracking()
            .Where(x => x.관련생성이미지작업Id == null)
            .Where(x => x.상태 == 살뜰.도메인.판매.상세이미지생성상태.프롬프트생성완료
                     || x.상태 == 살뜰.도메인.판매.상세이미지생성상태.이미지생성요청중
                     || (includeFailed && x.상태 == 살뜰.도메인.판매.상세이미지생성상태.실패))
            .OrderByDescending(x => x.생성시각)
            .Take(Math.Max(1, maxCount));

        var items = await query.ToListAsync(cancellationToken);
        return items.Select(x => new 샘플이미지대상항목
        {
            대상타입 = 대상타입,
            대상식별자 = x.Id.ToString(),
            이미지용도 = 이미지용도,
            제목 = $"상품 {x.상품Id} 상세이미지",
            설명 = x.생성프롬프트 ?? "물류자산 기반 상세이미지 생성",
            추가맥락 = x.원본자산참조Json,
            종횡비 = "1:1",
            해상도 = "1K",
            샘플데이터여부 = false
        }).ToArray();
    }

    public async Task MarkRequestedAsync(string 대상식별자, CancellationToken cancellationToken = default)
    {
        var task = await FindAsync(대상식별자, cancellationToken);
        if (task is null)
        {
            return;
        }

        task.상태 = 살뜰.도메인.판매.상세이미지생성상태.이미지생성요청중;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkCompletedAsync(string 대상식별자, string imageUrl, CancellationToken cancellationToken = default)
    {
        var task = await FindAsync(대상식별자, cancellationToken);
        if (task is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        _db.상품물류자산.Add(new 살뜰.도메인.판매.상품물류자산
        {
            상품Id = task.상품Id,
            주문Id = task.주문Id,
            통관절차Id = task.통관절차Id,
            자산유형 = 살뜰.도메인.판매.상품자산유형.상세이미지생성이미지,
            파일Url = imageUrl,
            설명 = "물류자산 기반으로 재생성된 판매 상세 이미지",
            등록자Id = task.요청자Id,
            상세이미지사용가능여부 = true,
            등록시각 = now
        });

        var draft = await _db.상품판매이미지초안
            .FirstOrDefaultAsync(x => x.생성작업Id == task.Id, cancellationToken);

        var imageList = new List<string>();
        if (draft is not null && !string.IsNullOrWhiteSpace(draft.이미지목록Json))
        {
            try
            {
                imageList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(draft.이미지목록Json) ?? new List<string>();
            }
            catch
            {
                imageList = new List<string>();
            }
        }

        if (!imageList.Contains(imageUrl, StringComparer.Ordinal))
        {
            imageList.Add(imageUrl);
        }

        if (draft is null)
        {
            draft = new 살뜰.도메인.판매.상품판매이미지초안
            {
                상품Id = task.상품Id,
                생성작업Id = task.Id,
                작성자Id = task.요청자Id,
                대표이미지Url = imageList.FirstOrDefault(),
                이미지목록Json = System.Text.Json.JsonSerializer.Serialize(imageList),
                원본자산참조Json = task.원본자산참조Json ?? "[]",
                생성근거요약 = "물류처리 데이터와 사진을 근거로 생성된 판매용 상세 이미지 초안",
                판매채널전송가능여부 = true,
                생성시각 = now
            };

            _db.상품판매이미지초안.Add(draft);
        }
        else
        {
            draft.대표이미지Url = imageList.FirstOrDefault();
            draft.이미지목록Json = System.Text.Json.JsonSerializer.Serialize(imageList);
        }

        task.상태 = 살뜰.도메인.판매.상세이미지생성상태.이미지생성완료;
        task.완료시각 = now;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(string 대상식별자, string? reason, CancellationToken cancellationToken = default)
    {
        var task = await FindAsync(대상식별자, cancellationToken);
        if (task is null)
        {
            return;
        }

        task.상태 = 살뜰.도메인.판매.상세이미지생성상태.실패;
        task.오류내용 = reason;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<살뜰.도메인.판매.상품상세이미지생성작업?> FindAsync(string 대상식별자, CancellationToken cancellationToken)
    {
        if (!long.TryParse(대상식별자, out var id))
        {
            return null;
        }

        return await _db.상품상세이미지생성작업.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}

public interface I샘플이미지대상Resolver
{
    string 대상타입 { get; }
    string 이미지용도 { get; }
    Task<IReadOnlyList<샘플이미지대상항목>> GetMissingImageTargetsAsync(int maxCount, bool includeFailed, CancellationToken cancellationToken = default);
    Task MarkRequestedAsync(string 대상식별자, CancellationToken cancellationToken = default);
    Task MarkCompletedAsync(string 대상식별자, string imageUrl, CancellationToken cancellationToken = default);
    Task MarkFailedAsync(string 대상식별자, string? reason, CancellationToken cancellationToken = default);
}

public interface I샘플이미지대상ResolverResolver
{
    I샘플이미지대상Resolver Resolve(string 대상타입, string 이미지용도);
}

public sealed class 샘플이미지대상ResolverResolver : I샘플이미지대상ResolverResolver
{
    private readonly IReadOnlyDictionary<string, I샘플이미지대상Resolver> _map;

    public 샘플이미지대상ResolverResolver(IEnumerable<I샘플이미지대상Resolver> resolvers)
    {
        _map = resolvers.ToDictionary(x => BuildKey(x.대상타입, x.이미지용도), StringComparer.Ordinal);
    }

    public I샘플이미지대상Resolver Resolve(string 대상타입, string 이미지용도)
    {
        var key = BuildKey(대상타입, 이미지용도);
        if (_map.TryGetValue(key, out var resolver))
        {
            return resolver;
        }

        throw new InvalidOperationException($"지원하지 않는 샘플 이미지 대상입니다. targetType={대상타입}, usage={이미지용도}");
    }

    private static string BuildKey(string 대상타입, string 이미지용도) => $"{대상타입}::{이미지용도}";
}

public sealed class 판매상품샘플이미지대상Resolver : I샘플이미지대상Resolver
{
    public const string 대상타입값 = "판매상품";

    private readonly SsalddelContext _db;

    public 판매상품샘플이미지대상Resolver(SsalddelContext db)
    {
        _db = db;
    }

    public string 대상타입 => 대상타입값;
    public string 이미지용도 => 생성이미지용도.화주상품사진;

    public async Task<IReadOnlyList<샘플이미지대상항목>> GetMissingImageTargetsAsync(int maxCount, bool includeFailed, CancellationToken cancellationToken = default)
    {
        var query = _db.판매상품
            .AsNoTracking()
            .Where(x => x.샘플데이터여부)
            .Where(x => string.IsNullOrWhiteSpace(x.이미지Url))
            .Where(x => x.이미지생성상태 == 판매상품이미지생성상태.미생성
                        || x.이미지생성상태 == 판매상품이미지생성상태.생성대기
                        || (includeFailed && x.이미지생성상태 == 판매상품이미지생성상태.실패))
            .OrderByDescending(x => x.UpdatedAt)
            .Take(Math.Max(1, maxCount));

        var products = await query.ToListAsync(cancellationToken);
        return products.Select(x => new 샘플이미지대상항목
        {
            대상타입 = 대상타입,
            대상식별자 = x.Id.ToString(),
            이미지용도 = 이미지용도,
            제목 = x.대표상품명,
            설명 = $"판매 SKU {x.판매SKU}, 판매가 {x.판매가}",
            추가맥락 = string.IsNullOrWhiteSpace(x.샘플데이터코드) ? null : $"sample code {x.샘플데이터코드}",
            종횡비 = "1:1",
            해상도 = "1K",
            샘플데이터여부 = true
        }).ToArray();
    }

    public async Task MarkRequestedAsync(string 대상식별자, CancellationToken cancellationToken = default)
    {
        var product = await FindProductAsync(대상식별자, cancellationToken);
        if (product is null)
        {
            return;
        }

        product.이미지생성상태 = 판매상품이미지생성상태.생성중;
        product.이미지생성요청시각 = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkCompletedAsync(string 대상식별자, string imageUrl, CancellationToken cancellationToken = default)
    {
        var product = await FindProductAsync(대상식별자, cancellationToken);
        if (product is null)
        {
            return;
        }

        product.이미지Url = imageUrl;
        product.이미지생성상태 = 판매상품이미지생성상태.완료;
        product.이미지생성완료시각 = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(string 대상식별자, string? reason, CancellationToken cancellationToken = default)
    {
        var product = await FindProductAsync(대상식별자, cancellationToken);
        if (product is null)
        {
            return;
        }

        product.이미지생성상태 = 판매상품이미지생성상태.실패;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<판매상품?> FindProductAsync(string 대상식별자, CancellationToken cancellationToken)
    {
        if (!long.TryParse(대상식별자, out var id))
        {
            return null;
        }

        return await _db.판매상품.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
