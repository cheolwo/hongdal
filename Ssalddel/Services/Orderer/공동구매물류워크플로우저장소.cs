using System.Text.RegularExpressions;
using Ssalddel.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Orderer;

public interface I공동구매물류워크플로우저장소
{
    Task<IReadOnlyList<공동구매물류워크플로우정의Dto>> ListAsync(
        공동구매물류워크플로우조회조건 query,
        CancellationToken cancellationToken = default);

    Task<공동구매물류워크플로우정의Dto?> GetAsync(
        string workflowId,
        string? version = null,
        CancellationToken cancellationToken = default);

    Task<공동구매물류워크플로우정의Dto?> ResolveAsync(
        공동구매물류워크플로우조회조건 query,
        CancellationToken cancellationToken = default);

    Task<공동구매물류워크플로우정의Dto> UpsertAsync(
        공동구매물류워크플로우저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task SeedDefaultsAsync(CancellationToken cancellationToken = default);
}

public sealed class Mongo공동구매물류워크플로우저장소 : I공동구매물류워크플로우저장소
{
    private const string CollectionName = "orderer_group_purchase_logistics_workflows";
    private readonly IMongoCollection<공동구매물류워크플로우문서> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public Mongo공동구매물류워크플로우저장소(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<공동구매물류워크플로우문서>(CollectionName);
    }

    public async Task<IReadOnlyList<공동구매물류워크플로우정의Dto>> ListAsync(
        공동구매물류워크플로우조회조건 query,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var filter = BuildFilter(query);
        var items = await _collection
            .Find(filter)
            .SortBy(x => x.품목분류코드)
            .ThenBy(x => x.온도코드)
            .ThenBy(x => x.물류방식)
            .ThenBy(x => x.판매자출처유형)
            .ThenByDescending(x => x.수정시각Utc)
            .ToListAsync(cancellationToken);

        return items.Select(ToDto).ToArray();
    }

    public async Task<공동구매물류워크플로우정의Dto?> GetAsync(
        string workflowId,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(workflowId))
        {
            return null;
        }

        var builder = Builders<공동구매물류워크플로우문서>.Filter;
        var filter = builder.Eq(x => x.워크플로우Id, workflowId.Trim());
        if (!string.IsNullOrWhiteSpace(version))
        {
            filter &= builder.Eq(x => x.버전, version.Trim());
        }

        var item = await _collection
            .Find(filter)
            .SortByDescending(x => x.수정시각Utc)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : ToDto(item);
    }

    public async Task<공동구매물류워크플로우정의Dto?> ResolveAsync(
        공동구매물류워크플로우조회조건 query,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        query.활성만 = true;
        var item = await _collection
            .Find(BuildFilter(query))
            .SortByDescending(x => x.수정시각Utc)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : ToDto(item);
    }

    public async Task<공동구매물류워크플로우정의Dto> UpsertAsync(
        공동구매물류워크플로우저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        Validate(request);

        var now = DateTime.UtcNow;
        var workflowId = string.IsNullOrWhiteSpace(request.워크플로우Id)
            ? Build워크플로우Id(request)
            : NormalizeKey(request.워크플로우Id);
        var version = string.IsNullOrWhiteSpace(request.버전) ? "1.0" : request.버전.Trim();

        var existing = await _collection
            .Find(x => x.워크플로우Id == workflowId && x.버전 == version)
            .FirstOrDefaultAsync(cancellationToken);

        var document = new 공동구매물류워크플로우문서
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            워크플로우Id = workflowId,
            버전 = version,
            표시명 = request.표시명.Trim(),
            품목분류코드 = request.품목분류코드.Trim(),
            온도코드 = request.온도코드.Trim(),
            물류방식 = request.물류방식.Trim(),
            판매자출처유형 = Normalize판매자출처유형(request.판매자출처유형),
            주문자집단배송권유형 = request.주문자집단배송권유형.Trim(),
            활성여부 = request.활성여부,
            단계목록 = request.단계목록.Select(ToDocument).OrderBy(x => x.순서).ToArray(),
            책임구간목록 = request.책임구간목록.Select(ToDocument).ToArray(),
            태그목록 = request.태그목록.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            메모 = request.메모.Trim(),
            수정자 = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            생성시각Utc = existing?.생성시각Utc ?? now,
            수정시각Utc = now
        };

        await _collection.ReplaceOneAsync(
            x => x.워크플로우Id == workflowId && x.버전 == version,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return ToDto(document);
    }

    public async Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var defaults = new[]
        {
            CreateDefaultDomesticColdChainApartmentWorkflow(),
            CreateDefaultOverseasColdChainApartmentWorkflow()
        };

        foreach (var item in defaults)
        {
            var exists = await _collection
                .Find(x => x.워크플로우Id == item.워크플로우Id && x.버전 == (item.버전 ?? "1.0"))
                .AnyAsync(cancellationToken);
            if (!exists)
            {
                await UpsertAsync(item, "seed", cancellationToken);
            }
        }
    }

    private FilterDefinition<공동구매물류워크플로우문서> BuildFilter(공동구매물류워크플로우조회조건 query)
    {
        var builder = Builders<공동구매물류워크플로우문서>.Filter;
        var filter = builder.Empty;

        if (query.활성만)
        {
            filter &= builder.Eq(x => x.활성여부, true);
        }

        if (!string.IsNullOrWhiteSpace(query.품목분류코드))
        {
            filter &= builder.Eq(x => x.품목분류코드, query.품목분류코드.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.온도코드))
        {
            filter &= builder.Eq(x => x.온도코드, query.온도코드.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.물류방식))
        {
            filter &= builder.Eq(x => x.물류방식, query.물류방식.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.판매자출처유형))
        {
            filter &= builder.Eq(x => x.판매자출처유형, Normalize판매자출처유형(query.판매자출처유형));
        }

        if (!string.IsNullOrWhiteSpace(query.주문자집단배송권유형))
        {
            filter &= builder.Eq(x => x.주문자집단배송권유형, query.주문자집단배송권유형.Trim());
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
                new CreateIndexModel<공동구매물류워크플로우문서>(
                    Builders<공동구매물류워크플로우문서>.IndexKeys
                        .Ascending(x => x.워크플로우Id)
                        .Ascending(x => x.버전),
                    new CreateIndexOptions { Unique = true, Name = "ux_workflow_version" }),
                new CreateIndexModel<공동구매물류워크플로우문서>(
                    Builders<공동구매물류워크플로우문서>.IndexKeys
                        .Ascending(x => x.품목분류코드)
                        .Ascending(x => x.온도코드)
                        .Ascending(x => x.물류방식)
                        .Ascending(x => x.판매자출처유형)
                        .Ascending(x => x.주문자집단배송권유형)
                        .Ascending(x => x.활성여부),
                    new CreateIndexOptions { Name = "ix_workflow_match" })
            };

            await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static void Validate(공동구매물류워크플로우저장요청 request)
    {
        if (string.IsNullOrWhiteSpace(request.표시명)) throw new InvalidOperationException("displayName is required.");
        if (string.IsNullOrWhiteSpace(request.품목분류코드)) throw new InvalidOperationException("productCategoryCode is required.");
        if (string.IsNullOrWhiteSpace(request.온도코드)) throw new InvalidOperationException("temperatureCode is required.");
        if (string.IsNullOrWhiteSpace(request.물류방식)) throw new InvalidOperationException("logisticsMode is required.");
        if (string.IsNullOrWhiteSpace(request.판매자출처유형)) throw new InvalidOperationException("sellerOriginType is required.");
        if (string.IsNullOrWhiteSpace(request.주문자집단배송권유형)) throw new InvalidOperationException("ordererGroupScopeType is required.");
        if (request.단계목록.Count == 0) throw new InvalidOperationException("steps are required.");
        if (request.책임구간목록.Count == 0) throw new InvalidOperationException("responsibilitySegments are required.");
    }

    private static string Build워크플로우Id(공동구매물류워크플로우저장요청 request)
        => NormalizeKey($"{request.품목분류코드}-{request.온도코드}-{request.물류방식}-{request.판매자출처유형}-{request.주문자집단배송권유형}");

    private static string Normalize판매자출처유형(string? value)
        => string.Equals(value?.Trim(), 공동구매판매자출처유형코드.해외, StringComparison.OrdinalIgnoreCase)
            ? 공동구매판매자출처유형코드.해외
            : 공동구매판매자출처유형코드.국내;

    private static string NormalizeKey(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        return Regex.Replace(trimmed, "[^a-z0-9가-힣]+", "-").Trim('-');
    }

    private static 공동구매물류워크플로우단계문서 ToDocument(공동구매물류워크플로우단계Dto source)
        => new()
        {
            단계코드 = source.단계코드.Trim(),
            표시명 = source.표시명.Trim(),
            순서 = source.순서,
            책임주체코드 = source.책임주체코드.Trim(),
            설명 = source.설명.Trim(),
            필요증빙코드목록 = source.필요증빙코드목록.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray(),
            오류대응코드목록 = source.오류대응코드목록.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray()
        };

    private static 공동구매책임구간문서 ToDocument(공동구매책임구간Dto source)
        => new()
        {
            구간코드 = source.구간코드.Trim(),
            From단계코드 = source.From단계코드.Trim(),
            To단계코드 = source.To단계코드.Trim(),
            책임주체코드 = source.책임주체코드.Trim(),
            책임범위 = source.책임범위.Trim(),
            필요증빙코드목록 = source.필요증빙코드목록.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray()
        };

    private static 공동구매물류워크플로우정의Dto ToDto(공동구매물류워크플로우문서 source)
        => new()
        {
            워크플로우Id = source.워크플로우Id,
            버전 = source.버전,
            표시명 = source.표시명,
            품목분류코드 = source.품목분류코드,
            온도코드 = source.온도코드,
            물류방식 = source.물류방식,
            판매자출처유형 = source.판매자출처유형,
            주문자집단배송권유형 = source.주문자집단배송권유형,
            활성여부 = source.활성여부,
            단계목록 = source.단계목록.Select(ToDto).ToArray(),
            책임구간목록 = source.책임구간목록.Select(ToDto).ToArray(),
            태그목록 = source.태그목록,
            메모 = source.메모,
            수정자 = source.수정자,
            생성시각Utc = source.생성시각Utc,
            수정시각Utc = source.수정시각Utc
        };

    private static 공동구매물류워크플로우단계Dto ToDto(공동구매물류워크플로우단계문서 source)
        => new()
        {
            단계코드 = source.단계코드,
            표시명 = source.표시명,
            순서 = source.순서,
            책임주체코드 = source.책임주체코드,
            설명 = source.설명,
            필요증빙코드목록 = source.필요증빙코드목록,
            오류대응코드목록 = source.오류대응코드목록
        };

    private static 공동구매책임구간Dto ToDto(공동구매책임구간문서 source)
        => new()
        {
            구간코드 = source.구간코드,
            From단계코드 = source.From단계코드,
            To단계코드 = source.To단계코드,
            책임주체코드 = source.책임주체코드,
            책임범위 = source.책임범위,
            필요증빙코드목록 = source.필요증빙코드목록
        };

    private static 공동구매물류워크플로우저장요청 CreateDefaultDomesticColdChainApartmentWorkflow()
        => new()
        {
            워크플로우Id = "food-cold-chain-domestic-apartment-v1",
            버전 = "1.0",
            표시명 = "공동주택 국내 판매자 냉장/냉동 먹거리 공동주문 기본 흐름",
            품목분류코드 = "FoodColdChain",
            온도코드 = "Frozen",
            물류방식 = "DomesticBulk",
            판매자출처유형 = 공동구매판매자출처유형코드.국내,
            주문자집단배송권유형 = "ApartmentComplex",
            활성여부 = true,
            태그목록 = ["orderer-group", "apartment", "cold-chain", "domestic-seller", "responsibility"],
            메모 = "국내 판매자 공동주문에서 판매자 출고, 국내 기사 운송, 대표 수령, 세대별 배분 책임 구간을 분리한다.",
            단계목록 =
            [
                new()
                {
                    단계코드 = "GroupOrderConfirmed",
                    표시명 = "공동주문 확정",
                    순서 = 10,
                    책임주체코드 = 공동구매물류워크플로우주체코드.플랫폼,
                    설명 = "참여자, 수량, 결제 상태를 확정한다.",
                    필요증빙코드목록 = ["PaymentSnapshot"],
                    오류대응코드목록 = ["ExcludeUnpaidOrderer", "RecalculateQuantity"]
                },
                new()
                {
                    단계코드 = "SellerPacked",
                    표시명 = "국내 판매자 포장/출고 준비",
                    순서 = 20,
                    책임주체코드 = 공동구매물류워크플로우주체코드.판매자,
                    설명 = "국내 판매자가 품목, 수량, 온도 조건에 맞춰 포장한다.",
                    필요증빙코드목록 = [공동구매물류증빙코드.판매자포장명세],
                    오류대응코드목록 = ["SellerShortageClaim", "PackingDefectClaim"]
                },
                new()
                {
                    단계코드 = "CarrierPickup",
                    표시명 = "국내 기사 상차 인계",
                    순서 = 30,
                    책임주체코드 = 공동구매물류워크플로우주체코드.운송주체,
                    설명 = "국내 판매자 또는 국내 출고지에서 기사에게 화물을 인계하고 상차 증빙을 남긴다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.상차사진,
                        공동구매물류증빙코드.상차인계인수증,
                        공동구매물류증빙코드.온도로그
                    ],
                    오류대응코드목록 = ["PickupQuantityMismatch", "TemperatureOutOfRange"]
                },
                new()
                {
                    단계코드 = "ApartmentDropoff",
                    표시명 = "공동주택 거점 하차",
                    순서 = 40,
                    책임주체코드 = 공동구매물류워크플로우주체코드.운송주체,
                    설명 = "공동주택 지정 거점에 하차하고 대표 수령자에게 인계한다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.하차사진,
                        공동구매물류증빙코드.집단대표수령확인서
                    ],
                    오류대응코드목록 = ["DropoffDelay", "RepresentativeAbsent", "DamageAtDropoff"]
                },
                new()
                {
                    단계코드 = "UnitDistribution",
                    표시명 = "세대별 배분",
                    순서 = 50,
                    책임주체코드 = 공동구매물류워크플로우주체코드.집단대표,
                    설명 = "대표 수령자가 세대별 수량을 분류하고 미수령분을 관리한다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.세대별분배체크리스트,
                        공동구매물류증빙코드.개별수령확인
                    ],
                    오류대응코드목록 = ["UnitMissingItem", "UnclaimedStorage", "InternalDistributionDispute"]
                }
            ],
            책임구간목록 =
            [
                new()
                {
                    구간코드 = "SellerToCarrier",
                    From단계코드 = "SellerPacked",
                    To단계코드 = "CarrierPickup",
                    책임주체코드 = 공동구매물류워크플로우주체코드.판매자,
                    책임범위 = "포장 완료부터 기사 상차 인계 전까지 상품 수량, 포장 상태, 출고 가능 온도에 대한 책임",
                    필요증빙코드목록 = [공동구매물류증빙코드.판매자포장명세]
                },
                new()
                {
                    구간코드 = "CarrierTransit",
                    From단계코드 = "CarrierPickup",
                    To단계코드 = "ApartmentDropoff",
                    책임주체코드 = 공동구매물류워크플로우주체코드.운송주체,
                    책임범위 = "상차 인수 이후 공동주택 거점 하차 인계 전까지 운송 지연, 파손, 분실, 온도 유지에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.상차사진,
                        공동구매물류증빙코드.하차사진,
                        공동구매물류증빙코드.온도로그
                    ]
                },
                new()
                {
                    구간코드 = "RepresentativeDistribution",
                    From단계코드 = "ApartmentDropoff",
                    To단계코드 = "UnitDistribution",
                    책임주체코드 = 공동구매물류워크플로우주체코드.집단대표,
                    책임범위 = "대표 수령 이후 세대별 배분, 미수령 보관, 내부 누락 확인에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.집단대표수령확인서,
                        공동구매물류증빙코드.세대별분배체크리스트
                    ]
                }
            ]
        };

    private static 공동구매물류워크플로우저장요청 CreateDefaultOverseasColdChainApartmentWorkflow()
        => new()
        {
            워크플로우Id = "food-cold-chain-overseas-apartment-v1",
            버전 = "1.0",
            표시명 = "공동주택 해외 판매자 냉장/냉동 먹거리 공동주문 기본 흐름",
            품목분류코드 = "FoodColdChain",
            온도코드 = "Frozen",
            물류방식 = "InternationalToDomesticBulk",
            판매자출처유형 = 공동구매판매자출처유형코드.해외,
            주문자집단배송권유형 = "ApartmentComplex",
            활성여부 = true,
            태그목록 = ["orderer-group", "apartment", "cold-chain", "overseas-seller", "customs", "logistics-proxy", "marketplace", "responsibility"],
            메모 = "해외 판매자 공동주문은 해외 포장, 국제 운송/통관, 국내 물류대행 입고, 판매채널 출품, 출고 배치 가능 구간을 별도 책임 구간으로 분리한다.",
            단계목록 =
            [
                new()
                {
                    단계코드 = "GroupOrderConfirmed",
                    표시명 = "공동주문 확정",
                    순서 = 10,
                    책임주체코드 = 공동구매물류워크플로우주체코드.플랫폼,
                    설명 = "참여자, 수량, 결제 상태와 수입 가능 조건을 확정한다.",
                    필요증빙코드목록 = ["PaymentSnapshot"],
                    오류대응코드목록 = ["ExcludeUnpaidOrderer", "RecalculateQuantity", "ImportConditionReviewFailed"]
                },
                new()
                {
                    단계코드 = "OverseasSellerPacked",
                    표시명 = "해외 판매자 포장/출고 준비",
                    순서 = 20,
                    책임주체코드 = 공동구매물류워크플로우주체코드.해외판매자,
                    설명 = "해외 판매자가 수출 포장, 수량, 냉장/냉동 조건, 송장 정보를 준비한다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.해외판매자포장명세,
                        공동구매물류증빙코드.수출인보이스,
                        공동구매물류증빙코드.온도로그
                    ],
                    오류대응코드목록 = ["OverseasSellerShortageClaim", "ExportPackingDefectClaim", "ExportDocumentMismatch"]
                },
                new()
                {
                    단계코드 = "InternationalTransport",
                    표시명 = "국제 운송",
                    순서 = 30,
                    책임주체코드 = 공동구매물류워크플로우주체코드.수입자,
                    설명 = "해외 출고 이후 국내 반입 전까지 국제 운송 상태와 온도 이력을 관리한다.",
                    필요증빙코드목록 = [공동구매물류증빙코드.온도로그],
                    오류대응코드목록 = ["InternationalDelay", "TemperatureOutOfRange", "InTransitDamage"]
                },
                new()
                {
                    단계코드 = "CustomsCleared",
                    표시명 = "통관/검역 완료",
                    순서 = 40,
                    책임주체코드 = 공동구매물류워크플로우주체코드.관세사,
                    설명 = "수입 신고, 식품 검역 또는 검사 결과를 확인하고 국내 반입 가능 상태를 만든다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.수입신고서,
                        공동구매물류증빙코드.수입검사결과
                    ],
                    오류대응코드목록 = ["CustomsHold", "InspectionFailed", "AdditionalDocumentRequired"]
                },
                new()
                {
                    단계코드 = "DomesticWarehouseReceived",
                    표시명 = "국내 물류대행 입고/재포장 확인",
                    순서 = 50,
                    책임주체코드 = 공동구매물류워크플로우주체코드.국내물류대행,
                    설명 = "국내 물류대행 입고지에서 수량, 온도, 파손 여부를 확인하고 판매 및 국내 배송 가능한 단위로 정리한다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.국내창고입고보고,
                        공동구매물류증빙코드.물류대행입고확인서,
                        공동구매물류증빙코드.온도로그
                    ],
                    오류대응코드목록 = ["DomesticReceivingMismatch", "ColdChainBreakAfterImport", "RepackingRequired"]
                },
                new()
                {
                    단계코드 = "InventoryLotConfirmed",
                    표시명 = "공동수입 재고 로트 확정",
                    순서 = 60,
                    책임주체코드 = 공동구매물류워크플로우주체코드.국내물류대행,
                    설명 = "물류대행사가 입고상품, 판매 가능 수량, 보관 위치를 확정해 판매채널 주문을 출고 배치에 연결할 수 있게 한다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.재고로트스냅샷,
                        공동구매물류증빙코드.온도로그
                    ],
                    오류대응코드목록 = ["InventoryLotMismatch", "MarketableQuantityBlocked", "StorageLocationMissing"]
                },
                new()
                {
                    단계코드 = "SalesChannelListed",
                    표시명 = "스마트스토어/쿠팡 등 판매채널 등록",
                    순서 = 70,
                    책임주체코드 = 공동구매물류워크플로우주체코드.판매채널운영자,
                    설명 = "공동주문 참여자가 판매할 상품을 판매상품과 채널출품으로 연결하고 판매 가능 상태를 확인한다.",
                    필요증빙코드목록 = [공동구매물류증빙코드.판매채널출품스냅샷],
                    오류대응코드목록 = ["ListingRejected", "ChannelProductMappingMissing", "PriceOrComplianceReviewRequired"]
                },
                new()
                {
                    단계코드 = "OutboundBatchReady",
                    표시명 = "판매 주문 출고 배치 가능",
                    순서 = 80,
                    책임주체코드 = 공동구매물류워크플로우주체코드.플랫폼,
                    설명 = "판매채널 주문 동기화 시 입고상품 재고를 기준으로 출고예정을 만들 수 있는 상태를 검증한다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.재고로트스냅샷,
                        공동구매물류증빙코드.판매채널출품스냅샷,
                        공동구매물류증빙코드.출고배치계획스냅샷
                    ],
                    오류대응코드목록 = ["OutboundAllocationFailed", "InsufficientInventory", "WarehouseServiceAreaMismatch"]
                },
                new()
                {
                    단계코드 = "DomesticCarrierPickup",
                    표시명 = "국내 기사 상차 인계",
                    순서 = 90,
                    책임주체코드 = 공동구매물류워크플로우주체코드.운송주체,
                    설명 = "국내 입고지 또는 재포장 거점에서 공동주택 직배송 물량을 국내 기사에게 인계하고 상차 증빙을 남긴다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.상차사진,
                        공동구매물류증빙코드.상차인계인수증,
                        공동구매물류증빙코드.온도로그
                    ],
                    오류대응코드목록 = ["PickupQuantityMismatch", "TemperatureOutOfRange"]
                },
                new()
                {
                    단계코드 = "ApartmentDropoff",
                    표시명 = "공동주택 거점 하차",
                    순서 = 100,
                    책임주체코드 = 공동구매물류워크플로우주체코드.운송주체,
                    설명 = "공동주택 지정 거점에 하차하고 대표 수령자에게 인계한다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.하차사진,
                        공동구매물류증빙코드.집단대표수령확인서
                    ],
                    오류대응코드목록 = ["DropoffDelay", "RepresentativeAbsent", "DamageAtDropoff"]
                },
                new()
                {
                    단계코드 = "UnitDistribution",
                    표시명 = "세대별 배분",
                    순서 = 110,
                    책임주체코드 = 공동구매물류워크플로우주체코드.집단대표,
                    설명 = "대표 수령자가 세대별 수량을 분류하고 미수령분을 관리한다.",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.세대별분배체크리스트,
                        공동구매물류증빙코드.개별수령확인
                    ],
                    오류대응코드목록 = ["UnitMissingItem", "UnclaimedStorage", "InternalDistributionDispute"]
                }
            ],
            책임구간목록 =
            [
                new()
                {
                    구간코드 = "OverseasSellerExport",
                    From단계코드 = "OverseasSellerPacked",
                    To단계코드 = "InternationalTransport",
                    책임주체코드 = 공동구매물류워크플로우주체코드.해외판매자,
                    책임범위 = "해외 판매자 포장 완료부터 국제 운송 인계 전까지 수량, 포장, 수출 서류, 출고 온도에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.해외판매자포장명세,
                        공동구매물류증빙코드.수출인보이스
                    ]
                },
                new()
                {
                    구간코드 = "ImportAndCustoms",
                    From단계코드 = "InternationalTransport",
                    To단계코드 = "DomesticWarehouseReceived",
                    책임주체코드 = 공동구매물류워크플로우주체코드.수입자,
                    책임범위 = "국제 운송, 통관/검역, 국내 입고 전까지 지연, 반입 불가, 온도 이탈, 서류 보완에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.수입신고서,
                        공동구매물류증빙코드.수입검사결과,
                        공동구매물류증빙코드.온도로그
                    ]
                },
                new()
                {
                    구간코드 = "LogisticsProxyInventoryCustody",
                    From단계코드 = "DomesticWarehouseReceived",
                    To단계코드 = "InventoryLotConfirmed",
                    책임주체코드 = 공동구매물류워크플로우주체코드.국내물류대행,
                    책임범위 = "국내 물류대행 입고 이후 판매 가능 재고 로트 확정 전까지 보관, 재포장, 수량 확인, 냉장/냉동 유지에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.물류대행입고확인서,
                        공동구매물류증빙코드.재고로트스냅샷,
                        공동구매물류증빙코드.온도로그
                    ]
                },
                new()
                {
                    구간코드 = "MarketplaceListingToOutboundBatch",
                    From단계코드 = "InventoryLotConfirmed",
                    To단계코드 = "OutboundBatchReady",
                    책임주체코드 = 공동구매물류워크플로우주체코드.플랫폼,
                    책임범위 = "재고 로트 확정 이후 판매상품/채널출품 연결과 판매채널 주문의 출고 배치 가능 상태 검증에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.재고로트스냅샷,
                        공동구매물류증빙코드.판매채널출품스냅샷,
                        공동구매물류증빙코드.출고배치계획스냅샷
                    ]
                },
                new()
                {
                    구간코드 = "DomesticWarehouseToCarrier",
                    From단계코드 = "DomesticWarehouseReceived",
                    To단계코드 = "DomesticCarrierPickup",
                    책임주체코드 = 공동구매물류워크플로우주체코드.국내물류대행,
                    책임범위 = "공동주택 직배송 물량의 국내 입고 확인 이후 국내 기사 상차 전까지 보관, 재포장, 수량 확인, 냉장/냉동 유지에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.국내창고입고보고,
                        공동구매물류증빙코드.물류대행입고확인서,
                        공동구매물류증빙코드.온도로그
                    ]
                },
                new()
                {
                    구간코드 = "DomesticCarrierTransit",
                    From단계코드 = "DomesticCarrierPickup",
                    To단계코드 = "ApartmentDropoff",
                    책임주체코드 = 공동구매물류워크플로우주체코드.운송주체,
                    책임범위 = "국내 기사 상차 인수 이후 공동주택 거점 하차 인계 전까지 운송 지연, 파손, 분실, 온도 유지에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.상차사진,
                        공동구매물류증빙코드.하차사진,
                        공동구매물류증빙코드.온도로그
                    ]
                },
                new()
                {
                    구간코드 = "RepresentativeDistribution",
                    From단계코드 = "ApartmentDropoff",
                    To단계코드 = "UnitDistribution",
                    책임주체코드 = 공동구매물류워크플로우주체코드.집단대표,
                    책임범위 = "대표 수령 이후 세대별 배분, 미수령 보관, 내부 누락 확인에 대한 책임",
                    필요증빙코드목록 =
                    [
                        공동구매물류증빙코드.집단대표수령확인서,
                        공동구매물류증빙코드.세대별분배체크리스트
                    ]
                }
            ]
        };
}

public sealed class 공동구매물류워크플로우문서
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string 워크플로우Id { get; set; } = string.Empty;
    public string 버전 { get; set; } = "1.0";
    public string 표시명 { get; set; } = string.Empty;
    public string 품목분류코드 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 물류방식 { get; set; } = string.Empty;
    public string 판매자출처유형 { get; set; } = 공동구매판매자출처유형코드.국내;
    public string 주문자집단배송권유형 { get; set; } = string.Empty;
    public bool 활성여부 { get; set; } = true;
    public IReadOnlyList<공동구매물류워크플로우단계문서> 단계목록 { get; set; } = [];
    public IReadOnlyList<공동구매책임구간문서> 책임구간목록 { get; set; } = [];
    public IReadOnlyList<string> 태그목록 { get; set; } = [];
    public string 메모 { get; set; } = string.Empty;
    public string 수정자 { get; set; } = string.Empty;
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 공동구매물류워크플로우단계문서
{
    public string 단계코드 { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public int 순서 { get; set; }
    public string 책임주체코드 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public IReadOnlyList<string> 필요증빙코드목록 { get; set; } = [];
    public IReadOnlyList<string> 오류대응코드목록 { get; set; } = [];
}

public sealed class 공동구매책임구간문서
{
    public string 구간코드 { get; set; } = string.Empty;
    public string From단계코드 { get; set; } = string.Empty;
    public string To단계코드 { get; set; } = string.Empty;
    public string 책임주체코드 { get; set; } = string.Empty;
    public string 책임범위 { get; set; } = string.Empty;
    public IReadOnlyList<string> 필요증빙코드목록 { get; set; } = [];
}
