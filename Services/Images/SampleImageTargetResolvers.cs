using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.도메인.판매;

namespace 홍달.Services.Images;

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

    private readonly HongdalContext _db;

    public 판매상품샘플이미지대상Resolver(HongdalContext db)
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
