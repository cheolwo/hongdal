using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Ui.Common.Areas.App.Services;

/// <summary>
/// 공동구매 실행 화면에서 창고 기준정보, 입고원장과 출고원장을 다루는 API 경계입니다.
/// </summary>
public interface I공동구매창고Service
{
    Task<IReadOnlyList<창고요약응답>> 창고목록조회Async(CancellationToken cancellationToken = default);

    Task<창고요약응답?> 창고생성Async(
        창고저장요청 request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<창고사용자항목응답>> 창고사용자목록조회Async(
        long warehouseId,
        CancellationToken cancellationToken = default);

    Task<창고사용자항목응답?> 창고사용자추가Async(
        long warehouseId,
        창고사용자저장요청 request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<입고요청항목응답>> 입고목록조회Async(CancellationToken cancellationToken = default);

    Task<입고요청항목응답?> 입고요청생성Async(
        입고요청저장요청 request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<입고상품항목응답>> 입고완료Async(
        long inboundId,
        입고완료요청 request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<재고항목응답>> 재고목록조회Async(CancellationToken cancellationToken = default);

    Task<창고작업결과응답?> 입고검수Async(
        long inboundItemId,
        입고검수요청 request,
        CancellationToken cancellationToken = default);

    Task<창고작업결과응답?> 적재위치배정Async(
        long inboundItemId,
        적재위치배정요청 request,
        CancellationToken cancellationToken = default);

    Task<창고작업결과응답?> 포장작업Async(
        long inboundItemId,
        포장작업요청 request,
        CancellationToken cancellationToken = default);

    Task<화주운송의뢰응답?> 운송인계Async(
        재고운송의뢰생성요청 request,
        CancellationToken cancellationToken = default);
}

public sealed class 공동구매창고Service(IHongdalJsonApiClient client) : I공동구매창고Service
{
    private const string BasePath = "api/v1/warehouse-operations";

    public async Task<IReadOnlyList<창고요약응답>> 창고목록조회Async(
        CancellationToken cancellationToken = default)
        => (await client.GetAsync<창고목록응답>(
                $"{BasePath}/warehouses",
                "공동구매 창고 목록 조회",
                cancellationToken: cancellationToken))?.Items
            ?? [];

    public Task<창고요약응답?> 창고생성Async(
        창고저장요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<창고저장요청, 창고요약응답>(
            HttpMethod.Post,
            $"{BasePath}/warehouses",
            request,
            "공동구매 창고 생성",
            cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<창고사용자항목응답>> 창고사용자목록조회Async(
        long warehouseId,
        CancellationToken cancellationToken = default)
        => (await client.GetAsync<창고사용자목록응답>(
                $"{BasePath}/warehouses/{warehouseId}/users",
                "공동구매 창고 사용자 목록 조회",
                cancellationToken: cancellationToken))?.Items
            ?? [];

    public Task<창고사용자항목응답?> 창고사용자추가Async(
        long warehouseId,
        창고사용자저장요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<창고사용자저장요청, 창고사용자항목응답>(
            HttpMethod.Post,
            $"{BasePath}/warehouses/{warehouseId}/users",
            request,
            "공동구매 창고 사용자 추가",
            cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<입고요청항목응답>> 입고목록조회Async(
        CancellationToken cancellationToken = default)
        => (await client.GetAsync<입고요청목록응답>(
                $"{BasePath}/inbounds",
                "공동구매 입고원장 목록 조회",
                cancellationToken: cancellationToken))?.Items
            ?? [];

    public Task<입고요청항목응답?> 입고요청생성Async(
        입고요청저장요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<입고요청저장요청, 입고요청항목응답>(
            HttpMethod.Post,
            $"{BasePath}/inbounds",
            request,
            "공동구매 입고 요청 생성",
            cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<입고상품항목응답>> 입고완료Async(
        long inboundId,
        입고완료요청 request,
        CancellationToken cancellationToken = default)
        => (await client.SendAsync<입고완료요청, 입고상품목록응답>(
                HttpMethod.Post,
                $"{BasePath}/inbounds/{inboundId}/complete",
                request,
                "공동구매 입고 완료",
                cancellationToken: cancellationToken))?.Items
            ?? [];

    public async Task<IReadOnlyList<재고항목응답>> 재고목록조회Async(
        CancellationToken cancellationToken = default)
        => (await client.GetAsync<재고목록응답>(
                $"{BasePath}/inventory",
                "공동구매 재고 목록 조회",
                cancellationToken: cancellationToken))?.Items
            ?? [];

    public Task<창고작업결과응답?> 입고검수Async(
        long inboundItemId,
        입고검수요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<입고검수요청, 창고작업결과응답>(
            HttpMethod.Post,
            $"{BasePath}/inventory/{inboundItemId}/inspect",
            request,
            "공동구매 입고 검수",
            cancellationToken: cancellationToken);

    public Task<창고작업결과응답?> 적재위치배정Async(
        long inboundItemId,
        적재위치배정요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<적재위치배정요청, 창고작업결과응답>(
            HttpMethod.Post,
            $"{BasePath}/inventory/{inboundItemId}/put-away",
            request,
            "공동구매 적재 위치 배정",
            cancellationToken: cancellationToken);

    public Task<창고작업결과응답?> 포장작업Async(
        long inboundItemId,
        포장작업요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<포장작업요청, 창고작업결과응답>(
            HttpMethod.Post,
            $"{BasePath}/inventory/{inboundItemId}/pack",
            request,
            "공동구매 출고 포장",
            cancellationToken: cancellationToken);

    public Task<화주운송의뢰응답?> 운송인계Async(
        재고운송의뢰생성요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<재고운송의뢰생성요청, 화주운송의뢰응답>(
            HttpMethod.Post,
            $"{BasePath}/inventory/reconsignment",
            request,
            "공동구매 출고 운송 인계",
            cancellationToken: cancellationToken);
}
