using Hongdal.Contracts.Common.Sales;
using HongdalApp.Services.Application;
using HongdalApp.Services.Commerce;
using HongdalApp.Services.Commerce.Listings.Commands;

namespace HongdalApp.Services;

public sealed class ShipperSalesService : IShipperSalesService
{
    private readonly InMemoryShipperStore _store;
    private readonly ICommerceChannelCatalog _channelCatalog;
    private readonly IAppCommandHandler<CreateChannelListingCommand, 채널출품항목응답?> _createListingHandler;

    public ShipperSalesService(
        InMemoryShipperStore store,
        ICommerceChannelCatalog channelCatalog,
        IAppCommandHandler<CreateChannelListingCommand, 채널출품항목응답?> createListingHandler)
    {
        _store = store;
        _channelCatalog = channelCatalog;
        _createListingHandler = createListingHandler;
    }

    public Task<IReadOnlyList<CommerceChannelDescriptor>> GetSupportedChannelsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_channelCatalog.GetSupportedChannels());
    }

    public Task<판매채널계정목록응답?> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<판매채널계정목록응답?>(new 판매채널계정목록응답 { Items = _store.GetAccounts() });
    }

    public Task<판매채널계정항목응답?> CreateAccountAsync(판매채널계정저장요청 payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var created = _store.CreateAccount(payload);
        return Task.FromResult<판매채널계정항목응답?>(created);
    }

    public Task<판매상품목록응답?> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<판매상품목록응답?>(new 판매상품목록응답 { Items = _store.GetProducts() });
    }

    public Task<판매상품항목응답?> CreateProductAsync(판매상품저장요청 payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var created = _store.CreateProduct(payload);
        return Task.FromResult<판매상품항목응답?>(created);
    }

    public Task<채널출품목록응답?> GetListingsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<채널출품목록응답?>(new 채널출품목록응답 { Items = _store.GetListings() });
    }

    public async Task<채널출품항목응답?> CreateListingAsync(채널출품저장요청 payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _createListingHandler.HandleAsync(new CreateChannelListingCommand(payload), cancellationToken);
    }
}
