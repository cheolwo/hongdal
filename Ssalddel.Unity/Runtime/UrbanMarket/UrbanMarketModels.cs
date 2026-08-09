using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class 도심마트SourceTypeCodes
    {
        public const string OperationalProjection = "OperationalProjection";
        public const string SimulatedFixture = "SimulatedFixture";
    }

    public static class 재고상태Codes
    {
        public const string InStock = "InStock";
        public const string LowStock = "LowStock";
        public const string OutOfStock = "OutOfStock";
        public const string Unknown = "Unknown";
    }

    public sealed class 도심마트상품ScreenModel
    {
        public string StableId { get; set; } = string.Empty;

        public string 상품명 { get; set; } = string.Empty;

        public string 포장표시 { get; set; } = string.Empty;

        public decimal 가격 { get; set; }

        public string 통화Code { get; set; } = string.Empty;

        public int 재고수량 { get; set; }

        public string 재고단위 { get; set; } = string.Empty;

        public string 재고상태Code { get; set; } = 재고상태Codes.Unknown;

        public string SourceName { get; set; } = string.Empty;

        public string SourceHref { get; set; } = string.Empty;

        public DateTimeOffset EvidenceAsOf { get; set; }

        public string EvidenceStatusCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트ScreenModel
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string 마트명 { get; set; } = string.Empty;

        public string SourceTypeCode { get; set; } = string.Empty;

        public DateTimeOffset GeneratedAt { get; set; }

        public 도심마트상품ScreenModel[] 상품목록 { get; set; } = Array.Empty<도심마트상품ScreenModel>();
    }

    public interface I도심마트조회UseCase
    {
        Task<도심마트ScreenModel> 조회Async(CancellationToken cancellationToken = default);
    }

    public sealed class Simulated도심마트조회UseCase : I도심마트조회UseCase
    {
        private readonly I도심마트공개상품DataQuery dataQuery;
        private readonly 도심마트공개상품ScreenModelAdapter screenAdapter;

        public Simulated도심마트조회UseCase()
            : this(
                new Simulated도심마트공개상품DataQuery(),
                new 도심마트공개상품ScreenModelAdapter())
        {
        }

        public Simulated도심마트조회UseCase(
            I도심마트공개상품DataQuery dataQuery,
            도심마트공개상품ScreenModelAdapter screenAdapter)
        {
            this.dataQuery = dataQuery ?? throw new ArgumentNullException(nameof(dataQuery));
            this.screenAdapter = screenAdapter ?? throw new ArgumentNullException(nameof(screenAdapter));
        }

        public async Task<도심마트ScreenModel> 조회Async(CancellationToken cancellationToken = default)
        {
            var data = await dataQuery.조회Async(cancellationToken).ConfigureAwait(false);
            return screenAdapter.Map(data);
        }
    }

    public sealed class 도심마트ScreenModelValidator
    {
        private static readonly HashSet<string> SourceTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            도심마트SourceTypeCodes.OperationalProjection,
            도심마트SourceTypeCodes.SimulatedFixture,
        };

        private static readonly HashSet<string> StockStates = new HashSet<string>(StringComparer.Ordinal)
        {
            재고상태Codes.InStock,
            재고상태Codes.LowStock,
            재고상태Codes.OutOfStock,
            재고상태Codes.Unknown,
        };

        public string[] Validate(도심마트ScreenModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var errors = new List<string>();
            if (!StableDataId.IsValid(model.StableId))
            {
                errors.Add("MarketStableIdInvalid");
            }

            if (model.Revision < 0)
            {
                errors.Add("MarketRevisionInvalid");
            }

            Require(model.마트명, "MarketNameMissing", errors);
            if (!SourceTypes.Contains(model.SourceTypeCode))
            {
                errors.Add("MarketSourceTypeInvalid");
            }

            if (model.GeneratedAt == default)
            {
                errors.Add("MarketGeneratedAtMissing");
            }

            if (model.상품목록 == null)
            {
                errors.Add("ProductListMissing");
                return errors.ToArray();
            }

            var duplicateIds = model.상품목록
                .OfType<도심마트상품ScreenModel>()
                .GroupBy(item => item.StableId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
            foreach (var duplicateId in duplicateIds)
            {
                errors.Add("DuplicateProductStableId:" + duplicateId);
            }

            for (var index = 0; index < model.상품목록.Length; index++)
            {
                var product = model.상품목록[index];
                if (product == null)
                {
                    errors.Add("ProductMissing:" + index);
                    continue;
                }

                ValidateProduct(product, errors);
            }

            return errors.ToArray();
        }

        private static void ValidateProduct(
            도심마트상품ScreenModel product,
            ICollection<string> errors)
        {
            if (!StableDataId.IsValid(product.StableId))
            {
                errors.Add("ProductStableIdInvalid:" + product.StableId);
            }

            Require(product.상품명, "ProductNameMissing:" + product.StableId, errors);
            Require(product.포장표시, "PackageLabelMissing:" + product.StableId, errors);
            Require(product.통화Code, "CurrencyMissing:" + product.StableId, errors);
            Require(product.재고단위, "StockUnitMissing:" + product.StableId, errors);
            Require(product.SourceName, "SourceNameMissing:" + product.StableId, errors);
            Require(product.EvidenceStatusCode, "EvidenceStatusMissing:" + product.StableId, errors);

            if (product.EvidenceAsOf == default)
            {
                errors.Add("EvidenceAsOfMissing:" + product.StableId);
            }

            if (product.가격 < 0)
            {
                errors.Add("PriceInvalid:" + product.StableId);
            }

            if (product.재고수량 < 0)
            {
                errors.Add("StockQuantityInvalid:" + product.StableId);
            }

            if (!StockStates.Contains(product.재고상태Code))
            {
                errors.Add("StockStateInvalid:" + product.StableId);
            }
        }

        private static void Require(string value, string error, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(error);
            }
        }
    }
}
