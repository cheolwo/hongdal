using System;
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
        private readonly 도심마트공개상품DataMapper dataMapper;
        private readonly 도심마트공개상품ScreenModelAdapter screenAdapter;

        public 도심마트ApiMapper()
            : this(
                new 도심마트공개상품DataMapper(),
                new 도심마트공개상품ScreenModelAdapter())
        {
        }

        public 도심마트ApiMapper(
            도심마트공개상품DataMapper dataMapper,
            도심마트공개상품ScreenModelAdapter screenAdapter)
        {
            this.dataMapper = dataMapper ?? throw new ArgumentNullException(nameof(dataMapper));
            this.screenAdapter = screenAdapter ?? throw new ArgumentNullException(nameof(screenAdapter));
        }

        public 도심마트ScreenModel Map(도심마트목록ApiModel source)
            => screenAdapter.Map(MapData(source));

        public 도심마트공개상품DataSnapshot MapData(도심마트목록ApiModel source)
            => dataMapper.Map(source);
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
