using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class 도심마트DataSetKeys
    {
        public const string PublicProducts = "urban-market-public-products.v1";
        public const string ManagerOperations = "urban-market-manager-operations.v1";
    }

    public static class 도심마트ProjectionAudienceCodes
    {
        public const string OrdererPublic = "OrdererPublic";
        public const string MarketOperatorAuthorized = "MarketOperatorAuthorized";
    }

    public static class 도심마트QuantityMeaningCodes
    {
        /// <summary>내부 보관·진열 재고가 아니라 서버가 주문자 공개용으로 별도 투영한 판매 가능 수량입니다.</summary>
        public const string ProjectedSaleAvailability = "ProjectedSaleAvailability";
    }

    /// <summary>
    /// 주문자용 공개 상품 Projection에서 받은 사실입니다.
    /// 물리 진열 수량, 보관 재고, 예약 재고 또는 진열 보충 가능 수량을 나타내지 않습니다.
    /// </summary>
    public sealed class 도심마트공개상품Data
    {
        public string StableId { get; set; } = string.Empty;
        public string 상품명 { get; set; } = string.Empty;
        public string 판매단위 { get; set; } = string.Empty;
        public decimal 판매가 { get; set; }
        public string 통화Code { get; set; } = string.Empty;
        public int 투영판매가능수량 { get; set; }
        public string 투영수량단위 { get; set; } = string.Empty;
        public bool 서버판매가능여부 { get; set; }
        public string QuantityMeaningCode { get; set; } = 도심마트QuantityMeaningCodes.ProjectedSaleAvailability;
        public string SourceName { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public DateTimeOffset EvidenceAsOf { get; set; }
        public string SourceRevision { get; set; } = string.Empty;
        public string EvidenceStatusCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트공개상품DataSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public long LegacyRevision { get; set; }
        public string 마트명 { get; set; } = string.Empty;
        public string ProjectionAudienceCode { get; set; } = 도심마트ProjectionAudienceCodes.OrdererPublic;
        public DataScopeKind ScopeKind { get; set; } = DataScopeKind.Global;
        public DataRuntimeMode Mode { get; set; } = DataRuntimeMode.Operational;
        public DateTimeOffset GeneratedAt { get; set; }
        public string QuantityDisclosure { get; set; } = string.Empty;
        public 도심마트공개상품Data[] 상품목록 { get; set; } = Array.Empty<도심마트공개상품Data>();
    }

    public interface I도심마트공개상품DataQuery
    {
        Task<도심마트공개상품DataSnapshot> 조회Async(
            CancellationToken cancellationToken = default);
    }

    public sealed class 도심마트공개상품DataSnapshotValidator
    {
        public string[] Validate(도심마트공개상품DataSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            var errors = new List<string>();
            if (!StableDataId.IsValid(snapshot.StableId)) errors.Add("MarketStableIdInvalid");
            Require(snapshot.DataRevision, "MarketDataRevisionMissing", errors);
            Require(snapshot.마트명, "MarketNameMissing", errors);
            Require(snapshot.QuantityDisclosure, "ProjectedQuantityDisclosureMissing", errors);
            if (!string.Equals(
                    snapshot.ProjectionAudienceCode,
                    도심마트ProjectionAudienceCodes.OrdererPublic,
                    StringComparison.Ordinal))
                errors.Add("PublicProjectionAudienceInvalid");
            if (snapshot.ScopeKind != DataScopeKind.Global)
                errors.Add("PublicProjectionScopeInvalid");
            if (snapshot.GeneratedAt == default) errors.Add("MarketGeneratedAtMissing");
            if (snapshot.상품목록 == null)
            {
                errors.Add("ProductListMissing");
                return errors.ToArray();
            }

            var duplicate = snapshot.상품목록
                .Where(item => item != null)
                .GroupBy(item => item.StableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null) errors.Add("DuplicateProductStableId:" + duplicate.Key);

            for (var index = 0; index < snapshot.상품목록.Length; index++)
            {
                var product = snapshot.상품목록[index];
                if (product == null)
                {
                    errors.Add("ProductMissing:" + index);
                    continue;
                }

                if (!StableDataId.IsValid(product.StableId))
                    errors.Add("ProductStableIdInvalid:" + product.StableId);
                Require(product.상품명, "ProductNameMissing:" + product.StableId, errors);
                Require(product.판매단위, "SaleUnitMissing:" + product.StableId, errors);
                Require(product.통화Code, "CurrencyMissing:" + product.StableId, errors);
                Require(product.투영수량단위, "ProjectedQuantityUnitMissing:" + product.StableId, errors);
                Require(product.SourceName, "SourceNameMissing:" + product.StableId, errors);
                Require(product.SourceHref, "SourceHrefMissing:" + product.StableId, errors);
                Require(product.SourceRevision, "SourceRevisionMissing:" + product.StableId, errors);
                Require(product.EvidenceStatusCode, "EvidenceStatusMissing:" + product.StableId, errors);
                if (product.EvidenceAsOf == default)
                    errors.Add("EvidenceAsOfMissing:" + product.StableId);
                if (product.판매가 < 0) errors.Add("PriceInvalid:" + product.StableId);
                if (product.투영판매가능수량 < 0)
                    errors.Add("ProjectedAvailableQuantityInvalid:" + product.StableId);
                if (!string.Equals(
                        product.QuantityMeaningCode,
                        도심마트QuantityMeaningCodes.ProjectedSaleAvailability,
                        StringComparison.Ordinal))
                    errors.Add("ProjectedQuantityMeaningInvalid:" + product.StableId);
            }

            return errors.ToArray();
        }

        private static void Require(string value, string error, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value)) errors.Add(error);
        }
    }

    /// <summary>기존 공개 상품 API wire model을 공급자 독립 Data Snapshot으로 옮깁니다.</summary>
    public sealed class 도심마트공개상품DataMapper
    {
        private readonly Func<DateTimeOffset> utcNow;
        private readonly 도심마트공개상품DataSnapshotValidator validator;

        public 도심마트공개상품DataMapper()
            : this(() => DateTimeOffset.UtcNow, new 도심마트공개상품DataSnapshotValidator())
        {
        }

        public 도심마트공개상품DataMapper(
            Func<DateTimeOffset> utcNow,
            도심마트공개상품DataSnapshotValidator validator)
        {
            this.utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public 도심마트공개상품DataSnapshot Map(도심마트목록ApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Items == null)
                throw new InvalidOperationException("UrbanMarketProductListMissing");

            var products = source.Items.Select(MapProduct).ToArray();
            var generatedAt = products.Length == 0
                ? utcNow()
                : products.Max(item => item.EvidenceAsOf);
            var legacyRevision = source.Items.Length == 0
                ? 0L
                : source.Items.Max(item => item.수정시각.UtcDateTime.Ticks);
            var snapshot = new 도심마트공개상품DataSnapshot
            {
                StableId = "market:urban-public",
                DataRevision = "public-products:" + legacyRevision.ToString(CultureInfo.InvariantCulture),
                LegacyRevision = legacyRevision,
                마트명 = "살뜰 도심 마트",
                ProjectionAudienceCode = 도심마트ProjectionAudienceCodes.OrdererPublic,
                ScopeKind = DataScopeKind.Global,
                Mode = DataRuntimeMode.Operational,
                GeneratedAt = generatedAt,
                QuantityDisclosure = source.재고기준안내,
                상품목록 = products,
            };

            var errors = validator.Validate(snapshot);
            if (errors.Length > 0) throw new InvalidOperationException(errors[0]);
            return snapshot;
        }

        private static 도심마트공개상품Data MapProduct(도심마트상품ApiModel source)
        {
            if (source == null || source.Id <= 0)
                throw new InvalidOperationException("UrbanMarketProductIdentityInvalid");
            if (source.재고기준시각 == default || source.수정시각 == default)
                throw new InvalidOperationException("UrbanMarketProductTimestampMissing");
            if (source.판매가능수량 < 0)
                throw new InvalidOperationException("UrbanMarketProjectedAvailableQuantityInvalid");

            return new 도심마트공개상품Data
            {
                StableId = "mart-product:" + source.Id,
                상품명 = source.상품명,
                판매단위 = source.판매단위,
                판매가 = source.판매가,
                통화Code = "KRW",
                투영판매가능수량 = source.판매가능수량,
                투영수량단위 = source.판매단위,
                서버판매가능여부 = source.판매가능여부,
                QuantityMeaningCode = 도심마트QuantityMeaningCodes.ProjectedSaleAvailability,
                SourceName = "Ssalddel 마트 공개 상품 API",
                SourceHref = 도심마트ApiRoutes.PublicProducts,
                EvidenceAsOf = source.재고기준시각,
                SourceRevision = source.수정시각.UtcDateTime.Ticks.ToString(CultureInfo.InvariantCulture),
                EvidenceStatusCode = "Operational",
            };
        }
    }

    public sealed class 도심마트공개상품ApiDataRepository : I도심마트공개상품DataQuery
    {
        private readonly I도심마트ApiClient apiClient;
        private readonly 도심마트공개상품DataMapper mapper;

        public 도심마트공개상품ApiDataRepository(
            I도심마트ApiClient apiClient,
            도심마트공개상품DataMapper mapper)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<도심마트공개상품DataSnapshot> 조회Async(
            CancellationToken cancellationToken = default)
            => mapper.Map(await apiClient.GetAsync(cancellationToken).ConfigureAwait(false));
    }

    public sealed class Simulated도심마트공개상품DataQuery : I도심마트공개상품DataQuery
    {
        private static readonly DateTimeOffset FixtureAsOf =
            DateTimeOffset.Parse("2026-08-08T09:00:00+09:00");

        public Task<도심마트공개상품DataSnapshot> 조회Async(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new 도심마트공개상품DataSnapshot
            {
                StableId = "market:urban-demo-001",
                DataRevision = "simulation:1",
                LegacyRevision = 1,
                마트명 = "살뜰 도심 마트",
                ProjectionAudienceCode = 도심마트ProjectionAudienceCodes.OrdererPublic,
                ScopeKind = DataScopeKind.Global,
                Mode = DataRuntimeMode.Simulation,
                GeneratedAt = FixtureAsOf,
                QuantityDisclosure = "SIMULATED 판매 가능 수량이며 실제 보관·진열 재고가 아닙니다.",
                상품목록 = new[]
                {
                    Product("product:potato-20kg", "감자", "20kg", 35000m, 12),
                    Product("product:rice-10kg", "쌀", "10kg", 42000m, 8),
                    Product("product:onion-10kg", "양파", "10kg", 18000m, 4),
                },
            });
        }

        private static 도심마트공개상품Data Product(
            string stableId,
            string name,
            string saleUnit,
            decimal price,
            int projectedAvailableQuantity)
            => new 도심마트공개상품Data
            {
                StableId = stableId,
                상품명 = name,
                판매단위 = saleUnit,
                판매가 = price,
                통화Code = "KRW",
                투영판매가능수량 = projectedAvailableQuantity,
                투영수량단위 = "상자",
                서버판매가능여부 = projectedAvailableQuantity > 0,
                QuantityMeaningCode = 도심마트QuantityMeaningCodes.ProjectedSaleAvailability,
                SourceName = "SIMULATED urban-market fixture",
                SourceHref = "simulation://urban-market/public-products",
                EvidenceAsOf = FixtureAsOf,
                SourceRevision = "simulation:1",
                EvidenceStatusCode = "Simulated",
            };
    }

    /// <summary>
    /// 기존 ScreenModel 소비자를 유지하기 위한 호환 adapter입니다.
    /// 관리자 재고·진열 보충 해석 계약으로 사용하지 않습니다.
    /// </summary>
    public sealed class 도심마트공개상품ScreenModelAdapter
    {
        public 도심마트ScreenModel Map(도심마트공개상품DataSnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new 도심마트ScreenModel
            {
                StableId = source.StableId,
                Revision = source.LegacyRevision,
                마트명 = source.마트명,
                SourceTypeCode = source.Mode == DataRuntimeMode.Simulation
                    ? 도심마트SourceTypeCodes.SimulatedFixture
                    : 도심마트SourceTypeCodes.OperationalProjection,
                GeneratedAt = source.GeneratedAt,
                상품목록 = source.상품목록.Select(product => new 도심마트상품ScreenModel
                {
                    StableId = product.StableId,
                    상품명 = product.상품명,
                    포장표시 = product.판매단위,
                    가격 = product.판매가,
                    통화Code = product.통화Code,
                    재고수량 = product.투영판매가능수량,
                    재고단위 = product.투영수량단위,
                    재고상태Code = ResolveLegacyState(source.Mode, product),
                    SourceName = product.SourceName,
                    SourceHref = product.SourceHref,
                    EvidenceAsOf = product.EvidenceAsOf,
                    EvidenceStatusCode = product.EvidenceStatusCode,
                }).ToArray(),
            };
        }

        private static string ResolveLegacyState(
            DataRuntimeMode mode,
            도심마트공개상품Data product)
        {
            if (!product.서버판매가능여부 || product.투영판매가능수량 == 0)
                return 재고상태Codes.OutOfStock;

            // 기존 primitive simulation 화면의 회귀 호환만 위한 규칙입니다.
            // operational 진열 보충 판정에는 사용하지 않습니다.
            if (mode == DataRuntimeMode.Simulation && product.투영판매가능수량 <= 4)
                return 재고상태Codes.LowStock;

            return 재고상태Codes.InStock;
        }
    }
}
