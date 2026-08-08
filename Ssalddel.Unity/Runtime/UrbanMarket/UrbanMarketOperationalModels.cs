using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class 도심마트ApiRoutes
    {
        public const string PublicProducts =
            "api/v1/orderer/mart/products?판매가능만=false&page=1&pageSize=50";
    }

    public sealed class 도심마트상품ApiModel
    {
        public long Id { get; set; }
        public string 상품명 { get; set; } = string.Empty;
        public string 판매단위 { get; set; } = string.Empty;
        public decimal 판매가 { get; set; }
        public int 판매가능수량 { get; set; }
        public bool 판매가능여부 { get; set; }
        public DateTimeOffset 재고기준시각 { get; set; }
        public DateTimeOffset 수정시각 { get; set; }
    }

    public sealed class 도심마트목록ApiModel
    {
        public 도심마트상품ApiModel[] Items { get; set; } = Array.Empty<도심마트상품ApiModel>();
        public int TotalCount { get; set; }
        public string 재고기준안내 { get; set; } = string.Empty;
    }

    public interface I도심마트ApiClient
    {
        Task<도심마트목록ApiModel> GetAsync(CancellationToken cancellationToken = default);
    }

    public sealed class 도심마트ApiMapper
    {
        public 도심마트ScreenModel Map(도심마트목록ApiModel source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Items == null)
            {
                throw new InvalidOperationException("UrbanMarketProductListMissing");
            }

            var products = source.Items.Select(MapProduct).ToArray();
            var generatedAt = products.Length == 0
                ? DateTimeOffset.UtcNow
                : products.Max(item => item.EvidenceAsOf);
            var revision = source.Items.Length == 0
                ? 0L
                : source.Items.Max(item => item.수정시각.UtcDateTime.Ticks);

            return new 도심마트ScreenModel
            {
                StableId = "market:urban-public",
                Revision = revision,
                마트명 = "살뜰 도심 마트",
                SourceTypeCode = 도심마트SourceTypeCodes.OperationalProjection,
                GeneratedAt = generatedAt,
                상품목록 = products,
            };
        }

        private static 도심마트상품ScreenModel MapProduct(도심마트상품ApiModel source)
        {
            if (source == null || source.Id <= 0)
            {
                throw new InvalidOperationException("UrbanMarketProductIdentityInvalid");
            }

            if (source.재고기준시각 == default || source.수정시각 == default)
            {
                throw new InvalidOperationException("UrbanMarketProductTimestampMissing");
            }

            var available = source.판매가능여부 && source.판매가능수량 > 0;
            return new 도심마트상품ScreenModel
            {
                StableId = "mart-product:" + source.Id,
                상품명 = source.상품명,
                포장표시 = source.판매단위,
                가격 = source.판매가,
                통화Code = "KRW",
                재고수량 = Math.Max(0, source.판매가능수량),
                재고단위 = source.판매단위,
                재고상태Code = available
                    ? 재고상태Codes.InStock
                    : 재고상태Codes.OutOfStock,
                SourceName = "Ssalddel 마트 공개 상품 API",
                SourceHref = 도심마트ApiRoutes.PublicProducts,
                EvidenceAsOf = source.재고기준시각,
                EvidenceStatusCode = "Operational",
            };
        }
    }

    public interface I도심마트Repository
    {
        Task<도심마트ScreenModel> LoadAsync(CancellationToken cancellationToken = default);
    }

    public sealed class 도심마트ApiRepository : I도심마트Repository
    {
        private readonly I도심마트ApiClient apiClient;
        private readonly 도심마트ApiMapper mapper;

        public 도심마트ApiRepository(I도심마트ApiClient client, 도심마트ApiMapper modelMapper)
        {
            apiClient = client;
            mapper = modelMapper;
        }

        public async Task<도심마트ScreenModel> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return mapper.Map(await apiClient.GetAsync(cancellationToken).ConfigureAwait(false));
        }
    }

    public sealed class Operational도심마트조회UseCase : I도심마트조회UseCase
    {
        private readonly I도심마트Repository repository;

        public Operational도심마트조회UseCase(I도심마트Repository marketRepository)
        {
            repository = marketRepository;
        }

        public Task<도심마트ScreenModel> 조회Async(
            CancellationToken cancellationToken = default)
        {
            return repository.LoadAsync(cancellationToken);
        }
    }
}
