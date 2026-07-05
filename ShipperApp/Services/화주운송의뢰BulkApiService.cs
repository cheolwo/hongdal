using Hongdal.Contracts.Shipper.Request;

namespace ShipperApp.Services;

public sealed class 화주운송의뢰BulkApiService : IShipperBulkRequestService
{
    private readonly InMemoryShipperStore _store;
    private readonly IAuthSession _authSession;

    public 화주운송의뢰BulkApiService(InMemoryShipperStore store, IAuthSession authSession)
    {
        _store = store;
        _authSession = authSession;
    }

    public Task<화주운송의뢰일괄미리보기응답?> 미리보기Async(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<화주운송의뢰일괄미리보기응답?>(_store.CreateBulkPreview(ResolveUserId()));
    }

    public Task<화주운송의뢰일괄등록결과응답?> 등록Async(화주운송의뢰일괄확정등록요청 confirmRequest, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<화주운송의뢰일괄등록결과응답?>(_store.ConfirmBulk(confirmRequest, ResolveUserId()));
    }

    private string ResolveUserId()
    {
        return string.IsNullOrWhiteSpace(_authSession.UserId) ? "shipper-demo" : _authSession.UserId!;
    }
}
