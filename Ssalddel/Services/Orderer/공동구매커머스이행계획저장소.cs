using Ssalddel.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Orderer;

public interface I공동구매커머스이행계획저장소
{
    Task<IReadOnlyList<공동구매커머스이행계획Dto>> ListAsync(
        공동구매커머스이행계획조회조건 query,
        CancellationToken cancellationToken = default);

    Task<공동구매커머스이행계획Dto?> GetAsync(
        string planId,
        CancellationToken cancellationToken = default);

    Task<공동구매커머스이행계획Dto> UpsertAsync(
        공동구매커머스이행계획저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public sealed class Mongo공동구매커머스이행계획저장소 : I공동구매커머스이행계획저장소
{
    private const string CollectionName = "orderer_group_purchase_commerce_fulfillment_plans";
    private readonly IMongoCollection<공동구매커머스이행계획문서> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public Mongo공동구매커머스이행계획저장소(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<공동구매커머스이행계획문서>(CollectionName);
    }

    public async Task<IReadOnlyList<공동구매커머스이행계획Dto>> ListAsync(
        공동구매커머스이행계획조회조건 query,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var items = await _collection
            .Find(BuildFilter(query))
            .SortByDescending(x => x.수정시각Utc)
            .Limit(200)
            .ToListAsync(cancellationToken);

        return items.Select(ToDto).ToArray();
    }

    public async Task<공동구매커머스이행계획Dto?> GetAsync(
        string planId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var normalized = NormalizeRequired(planId, "planId");
        var item = await _collection
            .Find(x => x.계획Id정규화 == normalized)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : ToDto(item);
    }

    public async Task<공동구매커머스이행계획Dto> UpsertAsync(
        공동구매커머스이행계획저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        Validate(request);

        var now = DateTime.UtcNow;
        var documentManagementNumberNormalized = NormalizeOptional(request.문서관리번호);
        var skuNormalized = NormalizeOptional(request.Sku);
        var inventoryLotCodeNormalized = NormalizeOptional(request.재고로트코드);
        var existing = await FindExistingAsync(request.계획Id, request.공동구매Id, documentManagementNumberNormalized, skuNormalized, inventoryLotCodeNormalized, cancellationToken);
        var planId = string.IsNullOrWhiteSpace(request.계획Id)
            ? existing?.계획Id ?? ObjectId.GenerateNewId().ToString()
            : request.계획Id.Trim();

        var document = new 공동구매커머스이행계획문서
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            계획Id = planId,
            계획Id정규화 = NormalizeRequired(planId, "planId"),
            공동구매Id = request.공동구매Id.Trim(),
            주문자집단배송권키 = request.주문자집단배송권키.Trim(),
            주문자집단배송권명 = request.주문자집단배송권명.Trim(),
            문서관리번호 = request.문서관리번호.Trim(),
            문서관리번호정규화 = documentManagementNumberNormalized,
            플랫폼물류대행사용 = request.플랫폼물류대행사용,
            물류대행사명 = request.물류대행사명.Trim(),
            물류대행거점명 = request.물류대행거점명.Trim(),
            창고Id = request.창고Id,
            창고명 = request.창고명.Trim(),
            입고요청Id = request.입고요청Id,
            입고상품Id = request.입고상품Id,
            판매상품Id = request.판매상품Id,
            재고로트코드 = request.재고로트코드.Trim(),
            재고로트코드정규화 = inventoryLotCodeNormalized,
            Sku = request.Sku.Trim(),
            SkuNormalized = skuNormalized,
            상품명 = request.상품명.Trim(),
            예상입고수량 = Math.Max(0, request.예상입고수량),
            판매가능수량 = Math.Max(0, request.판매가능수량),
            현재상태코드 = NormalizeStatus(request.현재상태코드),
            입고상태코드 = request.입고상태코드.Trim(),
            출품상태코드 = request.출품상태코드.Trim(),
            출고배치상태코드 = request.출고배치상태코드.Trim(),
            판매채널목록 = request.판매채널목록.Select(ToDocument).ToArray(),
            판매채널유형목록 = request.판매채널목록.Select(x => Normalize채널유형(x.채널유형)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            출고배치정책코드 = request.출고배치정책코드.Trim(),
            관리자메모 = request.관리자메모.Trim(),
            수정자 = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            생성시각Utc = existing?.생성시각Utc ?? now,
            수정시각Utc = now
        };

        await _collection.ReplaceOneAsync(
            x => x.계획Id정규화 == document.계획Id정규화,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return ToDto(document);
    }

    private async Task<공동구매커머스이행계획문서?> FindExistingAsync(
        string? planId,
        string groupPurchaseId,
        string documentManagementNumberNormalized,
        string skuNormalized,
        string inventoryLotCodeNormalized,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(planId))
        {
            var planIdNormalized = NormalizeRequired(planId, "planId");
            return await _collection.Find(x => x.계획Id정규화 == planIdNormalized).FirstOrDefaultAsync(cancellationToken);
        }

        return await _collection
            .Find(x =>
                x.공동구매Id == groupPurchaseId.Trim()
                && x.문서관리번호정규화 == documentManagementNumberNormalized
                && x.SkuNormalized == skuNormalized
                && x.재고로트코드정규화 == inventoryLotCodeNormalized)
            .SortByDescending(x => x.수정시각Utc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private FilterDefinition<공동구매커머스이행계획문서> BuildFilter(
        공동구매커머스이행계획조회조건 query)
    {
        var builder = Builders<공동구매커머스이행계획문서>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(query.공동구매Id))
        {
            filter &= builder.Eq(x => x.공동구매Id, query.공동구매Id.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.주문자집단배송권키))
        {
            filter &= builder.Eq(x => x.주문자집단배송권키, query.주문자집단배송권키.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.문서관리번호))
        {
            filter &= builder.Eq(x => x.문서관리번호정규화, NormalizeOptional(query.문서관리번호));
        }

        if (!string.IsNullOrWhiteSpace(query.현재상태코드))
        {
            filter &= builder.Eq(x => x.현재상태코드, NormalizeStatus(query.현재상태코드));
        }

        if (!string.IsNullOrWhiteSpace(query.판매채널유형))
        {
            filter &= builder.AnyEq(x => x.판매채널유형목록, Normalize채널유형(query.판매채널유형));
        }

        if (query.창고Id.HasValue)
        {
            filter &= builder.Eq(x => x.창고Id, query.창고Id.Value);
        }

        if (query.입고상품Id.HasValue)
        {
            filter &= builder.Eq(x => x.입고상품Id, query.입고상품Id.Value);
        }

        if (query.플랫폼물류대행사용.HasValue)
        {
            filter &= builder.Eq(x => x.플랫폼물류대행사용, query.플랫폼물류대행사용.Value);
        }

        return filter;
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (_indexesReady)
        {
            return;
        }

        await _indexLock.WaitAsync(cancellationToken);
        try
        {
            if (_indexesReady)
            {
                return;
            }

            var indexes = new[]
            {
                new CreateIndexModel<공동구매커머스이행계획문서>(
                    Builders<공동구매커머스이행계획문서>.IndexKeys.Ascending(x => x.계획Id정규화),
                    new CreateIndexOptions { Unique = true, Name = "ux_plan_id" }),
                new CreateIndexModel<공동구매커머스이행계획문서>(
                    Builders<공동구매커머스이행계획문서>.IndexKeys
                        .Ascending(x => x.공동구매Id)
                        .Ascending(x => x.주문자집단배송권키)
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_group_purchase_scope_updated" }),
                new CreateIndexModel<공동구매커머스이행계획문서>(
                    Builders<공동구매커머스이행계획문서>.IndexKeys
                        .Ascending(x => x.문서관리번호정규화)
                        .Ascending(x => x.SkuNormalized)
                        .Ascending(x => x.재고로트코드정규화),
                    new CreateIndexOptions { Name = "ix_document_sku_lot" }),
                new CreateIndexModel<공동구매커머스이행계획문서>(
                    Builders<공동구매커머스이행계획문서>.IndexKeys
                        .Ascending(x => x.현재상태코드)
                        .Ascending(x => x.판매채널유형목록)
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_status_channel_updated" })
            };

            await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static void Validate(공동구매커머스이행계획저장요청 request)
    {
        if (string.IsNullOrWhiteSpace(request.공동구매Id)) throw new InvalidOperationException("groupPurchaseId is required.");
        if (string.IsNullOrWhiteSpace(request.주문자집단배송권키)) throw new InvalidOperationException("ordererGroupScopeKey is required.");
        if (string.IsNullOrWhiteSpace(request.Sku)) throw new InvalidOperationException("sku is required.");
        if (string.IsNullOrWhiteSpace(request.상품명)) throw new InvalidOperationException("productName is required.");
        if (request.예상입고수량 < 0) throw new InvalidOperationException("expectedInboundQuantity must be zero or greater.");
        if (request.판매가능수량 < 0) throw new InvalidOperationException("availableForMarketQuantity must be zero or greater.");
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return NormalizeOptional(value);
    }

    private static string NormalizeOptional(string? value)
        => (value ?? string.Empty).Trim().Replace(" ", string.Empty).ToUpperInvariant();

    private static string NormalizeStatus(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, 공동구매커머스이행상태코드.물류대행선택, StringComparison.OrdinalIgnoreCase)) return 공동구매커머스이행상태코드.물류대행선택;
        if (string.Equals(normalized, 공동구매커머스이행상태코드.입고요청, StringComparison.OrdinalIgnoreCase)) return 공동구매커머스이행상태코드.입고요청;
        if (string.Equals(normalized, 공동구매커머스이행상태코드.입고완료, StringComparison.OrdinalIgnoreCase)) return 공동구매커머스이행상태코드.입고완료;
        if (string.Equals(normalized, 공동구매커머스이행상태코드.출품준비, StringComparison.OrdinalIgnoreCase)) return 공동구매커머스이행상태코드.출품준비;
        if (string.Equals(normalized, 공동구매커머스이행상태코드.판매채널출품완료, StringComparison.OrdinalIgnoreCase)) return 공동구매커머스이행상태코드.판매채널출품완료;
        if (string.Equals(normalized, 공동구매커머스이행상태코드.출고배치준비, StringComparison.OrdinalIgnoreCase)) return 공동구매커머스이행상태코드.출고배치준비;
        return string.Equals(normalized, 공동구매커머스이행상태코드.보류, StringComparison.OrdinalIgnoreCase)
            ? 공동구매커머스이행상태코드.보류
            : 공동구매커머스이행상태코드.초안;
    }

    private static string Normalize채널유형(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, 공동구매판매채널유형코드.쿠팡, StringComparison.OrdinalIgnoreCase)) return 공동구매판매채널유형코드.쿠팡;
        return string.Equals(normalized, 공동구매판매채널유형코드.기타, StringComparison.OrdinalIgnoreCase)
            ? 공동구매판매채널유형코드.기타
            : 공동구매판매채널유형코드.네이버스마트스토어;
    }

    private static 공동구매판매채널계획문서 ToDocument(
        공동구매판매채널계획Dto source)
        => new()
        {
            채널유형 = Normalize채널유형(source.채널유형),
            판매채널계정Id = source.판매채널계정Id,
            스토어명 = source.스토어명.Trim(),
            출품Id = source.출품Id,
            채널상품번호 = source.채널상품번호.Trim(),
            출품상태코드 = source.출품상태코드.Trim(),
            외부상품Url = source.외부상품Url.Trim()
        };

    private static 공동구매커머스이행계획Dto ToDto(
        공동구매커머스이행계획문서 source)
        => new()
        {
            계획Id = source.계획Id,
            공동구매Id = source.공동구매Id,
            주문자집단배송권키 = source.주문자집단배송권키,
            주문자집단배송권명 = source.주문자집단배송권명,
            문서관리번호 = source.문서관리번호,
            플랫폼물류대행사용 = source.플랫폼물류대행사용,
            물류대행사명 = source.물류대행사명,
            물류대행거점명 = source.물류대행거점명,
            창고Id = source.창고Id,
            창고명 = source.창고명,
            입고요청Id = source.입고요청Id,
            입고상품Id = source.입고상품Id,
            판매상품Id = source.판매상품Id,
            재고로트코드 = source.재고로트코드,
            Sku = source.Sku,
            상품명 = source.상품명,
            예상입고수량 = source.예상입고수량,
            판매가능수량 = source.판매가능수량,
            현재상태코드 = source.현재상태코드,
            입고상태코드 = source.입고상태코드,
            출품상태코드 = source.출품상태코드,
            출고배치상태코드 = source.출고배치상태코드,
            판매채널목록 = source.판매채널목록.Select(ToDto).ToArray(),
            출고배치정책코드 = source.출고배치정책코드,
            관리자메모 = source.관리자메모,
            수정자 = source.수정자,
            생성시각Utc = source.생성시각Utc,
            수정시각Utc = source.수정시각Utc
        };

    private static 공동구매판매채널계획Dto ToDto(
        공동구매판매채널계획문서 source)
        => new()
        {
            채널유형 = source.채널유형,
            판매채널계정Id = source.판매채널계정Id,
            스토어명 = source.스토어명,
            출품Id = source.출품Id,
            채널상품번호 = source.채널상품번호,
            출품상태코드 = source.출품상태코드,
            외부상품Url = source.외부상품Url
        };
}

public static class 공동구매커머스이행계획공개변환기
{
    public static 공동구매커머스이행계획공개Dto ToPublicDto(공동구매커머스이행계획Dto source)
        => new()
        {
            공동구매Id = source.공동구매Id,
            주문자집단배송권키 = source.주문자집단배송권키,
            주문자집단배송권명 = source.주문자집단배송권명,
            문서관리번호 = source.문서관리번호,
            플랫폼물류대행사용 = source.플랫폼물류대행사용,
            물류대행사명 = source.물류대행사명,
            물류대행거점명 = source.물류대행거점명,
            창고명 = source.창고명,
            재고로트코드 = source.재고로트코드,
            Sku = source.Sku,
            상품명 = source.상품명,
            예상입고수량 = source.예상입고수량,
            판매가능수량 = source.판매가능수량,
            현재상태코드 = source.현재상태코드,
            입고상태코드 = source.입고상태코드,
            출품상태코드 = source.출품상태코드,
            출고배치상태코드 = source.출고배치상태코드,
            판매채널목록 = source.판매채널목록.Select(ToPublicDto).ToArray(),
            수정시각Utc = source.수정시각Utc
        };

    private static 공동구매판매채널계획공개Dto ToPublicDto(공동구매판매채널계획Dto source)
        => new()
        {
            채널유형 = source.채널유형,
            스토어명 = source.스토어명,
            채널상품번호 = source.채널상품번호,
            출품상태코드 = source.출품상태코드,
            외부상품Url = source.외부상품Url
        };
}

public sealed class 공동구매커머스이행계획문서
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string 계획Id { get; set; } = string.Empty;
    public string 계획Id정규화 { get; set; } = string.Empty;
    public string 공동구매Id { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 문서관리번호 { get; set; } = string.Empty;
    public string 문서관리번호정규화 { get; set; } = string.Empty;
    public bool 플랫폼물류대행사용 { get; set; } = true;
    public string 물류대행사명 { get; set; } = string.Empty;
    public string 물류대행거점명 { get; set; } = string.Empty;
    public long? 창고Id { get; set; }
    public string 창고명 { get; set; } = string.Empty;
    public long? 입고요청Id { get; set; }
    public long? 입고상품Id { get; set; }
    public long? 판매상품Id { get; set; }
    public string 재고로트코드 { get; set; } = string.Empty;
    public string 재고로트코드정규화 { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string SkuNormalized { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public int 예상입고수량 { get; set; }
    public int 판매가능수량 { get; set; }
    public string 현재상태코드 { get; set; } = 공동구매커머스이행상태코드.초안;
    public string 입고상태코드 { get; set; } = string.Empty;
    public string 출품상태코드 { get; set; } = string.Empty;
    public string 출고배치상태코드 { get; set; } = string.Empty;
    public IReadOnlyList<공동구매판매채널계획문서> 판매채널목록 { get; set; } = [];
    public IReadOnlyList<string> 판매채널유형목록 { get; set; } = [];
    public string 출고배치정책코드 { get; set; } = string.Empty;
    public string 관리자메모 { get; set; } = string.Empty;
    public string 수정자 { get; set; } = string.Empty;
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 공동구매판매채널계획문서
{
    public string 채널유형 { get; set; } = 공동구매판매채널유형코드.네이버스마트스토어;
    public long? 판매채널계정Id { get; set; }
    public string 스토어명 { get; set; } = string.Empty;
    public long? 출품Id { get; set; }
    public string 채널상품번호 { get; set; } = string.Empty;
    public string 출품상태코드 { get; set; } = string.Empty;
    public string 외부상품Url { get; set; } = string.Empty;
}
