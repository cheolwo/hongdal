using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I공동구매체험Client
{
    Task<IReadOnlyList<공동구매체험시나리오응답>> 시나리오목록Async(
        CancellationToken cancellationToken = default);

    Task<공동구매체험응답?> 시뮬레이션Async(
        공동구매체험요청 request,
        CancellationToken cancellationToken = default);
}

public sealed class 공동구매체험Client(ISsalddelJsonApiClient client) : I공동구매체험Client
{
    private const string BasePath = "api/v1/orderer/group-purchase-practice";

    public async Task<IReadOnlyList<공동구매체험시나리오응답>> 시나리오목록Async(
        CancellationToken cancellationToken = default)
        => await client.GetAsync<IReadOnlyList<공동구매체험시나리오응답>>(
               $"{BasePath}/scenarios",
               "체험 공동구매 시나리오 조회",
               cancellationToken: cancellationToken)
           ?? [];

    public Task<공동구매체험응답?> 시뮬레이션Async(
        공동구매체험요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<공동구매체험요청, 공동구매체험응답>(
            HttpMethod.Post,
            $"{BasePath}/simulate",
            request,
            "체험 공동구매 진행",
            cancellationToken: cancellationToken);
}
