using Hongdal.Contracts.Common.Sales;

namespace Hongdal.Ui.Common.Areas.App.Services;

/// <summary>
/// 국내 판매와 해외 수출 ViewModel이 함께 사용하는 판매채널 API 경계입니다.
/// </summary>
public interface I판매채널Client
{
    Task<IReadOnlyList<판매채널계정항목응답>> 계정목록조회Async(
        CancellationToken cancellationToken = default);

    Task<판매채널계정항목응답?> 계정생성Async(
        판매채널계정저장요청 request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<판매상품항목응답>> 상품목록조회Async(
        CancellationToken cancellationToken = default);

    Task<판매상품항목응답?> 상품생성Async(
        판매상품저장요청 request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<채널출품항목응답>> 출품목록조회Async(
        CancellationToken cancellationToken = default);

    Task<채널출품항목응답?> 출품생성Async(
        채널출품저장요청 request,
        CancellationToken cancellationToken = default);
}

public sealed class 판매채널Client(IHongdalJsonApiClient client) : I판매채널Client
{
    private const string BasePath = "api/v1/sales-channels";

    public async Task<IReadOnlyList<판매채널계정항목응답>> 계정목록조회Async(
        CancellationToken cancellationToken = default)
        => (await client.GetAsync<판매채널계정목록응답>(
                $"{BasePath}/accounts",
                "판매채널 계정 목록 조회",
                cancellationToken: cancellationToken))?.Items
           ?? [];

    public Task<판매채널계정항목응답?> 계정생성Async(
        판매채널계정저장요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<판매채널계정저장요청, 판매채널계정항목응답>(
            HttpMethod.Post,
            $"{BasePath}/accounts",
            request,
            "판매채널 계정 생성",
            cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<판매상품항목응답>> 상품목록조회Async(
        CancellationToken cancellationToken = default)
        => (await client.GetAsync<판매상품목록응답>(
                $"{BasePath}/products",
                "판매상품 목록 조회",
                cancellationToken: cancellationToken))?.Items
           ?? [];

    public Task<판매상품항목응답?> 상품생성Async(
        판매상품저장요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<판매상품저장요청, 판매상품항목응답>(
            HttpMethod.Post,
            $"{BasePath}/products",
            request,
            "판매상품 생성",
            cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<채널출품항목응답>> 출품목록조회Async(
        CancellationToken cancellationToken = default)
        => (await client.GetAsync<채널출품목록응답>(
                $"{BasePath}/listings",
                "판매채널 출품 목록 조회",
                cancellationToken: cancellationToken))?.Items
           ?? [];

    public Task<채널출품항목응답?> 출품생성Async(
        채널출품저장요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<채널출품저장요청, 채널출품항목응답>(
            HttpMethod.Post,
            $"{BasePath}/listings",
            request,
            "판매채널 출품 생성",
            cancellationToken: cancellationToken);
}
