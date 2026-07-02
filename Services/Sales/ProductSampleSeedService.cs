using Hongdal.Contracts.Common.Sales;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.판매;

namespace 홍달.Services.Sales;

public interface I판매상품샘플시드Service
{
    Task<판매상품목록응답> SeedSampleProductsAsync(int maxCount, CancellationToken cancellationToken = default);
}

public sealed class 판매상품샘플시드Service : I판매상품샘플시드Service
{
    private readonly HongdalContext _db;

    public 판매상품샘플시드Service(HongdalContext db)
    {
        _db = db;
    }

    public async Task<판매상품목록응답> SeedSampleProductsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(maxCount, 1, 200);
        var inboundItems = await _db.입고상품
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .Take(take * 3)
            .ToListAsync(cancellationToken);

        var created = new List<판매상품>();

        foreach (var inboundItem in inboundItems)
        {
            var sampleCode = $"SAMPLE-PRODUCT-{inboundItem.Id}";
            var sampleSku = BuildSampleSku(inboundItem.SKU, inboundItem.Id);

            var exists = await _db.판매상품.AnyAsync(
                x => x.입고상품Id == inboundItem.Id
                     && x.샘플데이터여부
                     && x.샘플데이터코드 == sampleCode,
                cancellationToken);

            if (exists)
            {
                continue;
            }

            var entity = new 판매상품
            {
                입고상품Id = inboundItem.Id,
                소유자UserId = string.IsNullOrWhiteSpace(inboundItem.판매자UserId) ? inboundItem.소유자UserId : inboundItem.판매자UserId,
                대표상품명 = $"샘플 {inboundItem.상품명}".Trim(),
                판매SKU = sampleSku,
                판매가 = 0m,
                상태 = "샘플준비",
                샘플데이터여부 = true,
                샘플데이터코드 = sampleCode,
                이미지Url = null,
                이미지생성상태 = 판매상품이미지생성상태.미생성,
                이미지생성요청시각 = null,
                이미지생성완료시각 = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.판매상품.Add(entity);
            created.Add(entity);

            if (created.Count >= take)
            {
                break;
            }
        }

        if (created.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new 판매상품목록응답
        {
            Items = created.Select(entity => new 판매상품항목응답
            {
                Id = entity.Id,
                입고상품Id = entity.입고상품Id,
                대표상품명 = entity.대표상품명,
                판매SKU = entity.판매SKU,
                판매가 = entity.판매가,
                상태 = entity.상태,
                샘플데이터여부 = entity.샘플데이터여부,
                샘플데이터코드 = entity.샘플데이터코드,
                Image_Url = entity.이미지Url,
                이미지생성상태 = entity.이미지생성상태
            }).ToArray()
        };
    }

    private static string BuildSampleSku(string sku, long inboundItemId)
    {
        var baseSku = string.IsNullOrWhiteSpace(sku) ? $"ITEM-{inboundItemId}" : sku.Trim();
        return $"SAMPLE-{baseSku}";
    }
}
