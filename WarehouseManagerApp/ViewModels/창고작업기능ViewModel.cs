using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace WarehouseManagerApp.ViewModels;

/// <summary>
/// WarehouseOperationsController의 각 액션과 타입으로 대응하는 하위 ViewModel입니다.
/// </summary>
public sealed class 창고작업기능ViewModel : 조립ViewModelBase
{
    private const string BasePath = "api/v1/warehouse-operations";

    public 창고작업기능ViewModel(IHongdalJsonApiClient api)
    {
        창고목록조회 = 하위ViewModel등록(new Api작업ViewModel<창고목록응답?>(
            cancellationToken => api.GetAsync<창고목록응답>($"{BasePath}/warehouses", "창고 목록 조회", cancellationToken: cancellationToken)));
        창고생성 = 하위ViewModel등록(new Api작업ViewModel<창고저장요청, 창고요약응답?>(
            (request, cancellationToken) => api.SendAsync<창고저장요청, 창고요약응답>(
                HttpMethod.Post, $"{BasePath}/warehouses", request, "창고 생성", cancellationToken: cancellationToken)));
        창고사용자목록조회 = 하위ViewModel등록(new Api작업ViewModel<long, 창고사용자목록응답?>(
            (warehouseId, cancellationToken) => api.GetAsync<창고사용자목록응답>(
                $"{BasePath}/warehouses/{warehouseId}/users", "창고 사용자 목록 조회", cancellationToken: cancellationToken)));
        창고사용자추가 = 하위ViewModel등록(new Api작업ViewModel<창고사용자추가조건, 창고사용자항목응답?>(
            (condition, cancellationToken) => api.SendAsync<창고사용자저장요청, 창고사용자항목응답>(
                HttpMethod.Post,
                $"{BasePath}/warehouses/{condition.창고Id}/users",
                condition.요청,
                "창고 사용자 추가",
                cancellationToken: cancellationToken)));
        입고목록조회 = 하위ViewModel등록(new Api작업ViewModel<입고요청목록응답?>(
            cancellationToken => api.GetAsync<입고요청목록응답>($"{BasePath}/inbounds", "입고 목록 조회", cancellationToken: cancellationToken)));
        입고생성 = 하위ViewModel등록(new Api작업ViewModel<입고요청저장요청, 입고요청항목응답?>(
            (request, cancellationToken) => api.SendAsync<입고요청저장요청, 입고요청항목응답>(
                HttpMethod.Post, $"{BasePath}/inbounds", request, "입고 생성", cancellationToken: cancellationToken)));
        입고완료 = 하위ViewModel등록(new Api작업ViewModel<입고완료조건, 입고상품목록응답?>(
            (condition, cancellationToken) => api.SendAsync<입고완료요청, 입고상품목록응답>(
                HttpMethod.Post,
                $"{BasePath}/inbounds/{condition.입고Id}/complete",
                condition.요청,
                "입고 완료",
                cancellationToken: cancellationToken)));
        재고목록조회 = 하위ViewModel등록(new Api작업ViewModel<재고목록응답?>(
            cancellationToken => api.GetAsync<재고목록응답>($"{BasePath}/inventory", "재고 목록 조회", cancellationToken: cancellationToken)));
        입고검수 = 하위ViewModel등록(new Api작업ViewModel<입고검수조건, 창고작업결과응답?>(
            (condition, cancellationToken) => api.SendAsync<입고검수요청, 창고작업결과응답>(
                HttpMethod.Post,
                $"{BasePath}/inventory/{condition.입고항목Id}/inspect",
                condition.요청,
                "입고 검수",
                cancellationToken: cancellationToken)));
        적재위치배정 = 하위ViewModel등록(new Api작업ViewModel<적재위치배정조건, 창고작업결과응답?>(
            (condition, cancellationToken) => api.SendAsync<적재위치배정요청, 창고작업결과응답>(
                HttpMethod.Post,
                $"{BasePath}/inventory/{condition.입고항목Id}/put-away",
                condition.요청,
                "적재 위치 배정",
                cancellationToken: cancellationToken)));
        포장작업 = 하위ViewModel등록(new Api작업ViewModel<포장작업조건, 창고작업결과응답?>(
            (condition, cancellationToken) => api.SendAsync<포장작업요청, 창고작업결과응답>(
                HttpMethod.Post,
                $"{BasePath}/inventory/{condition.입고항목Id}/pack",
                condition.요청,
                "포장 작업",
                cancellationToken: cancellationToken)));
        재위탁운송생성 = 하위ViewModel등록(new Api작업ViewModel<재고운송의뢰생성요청, 화주운송의뢰응답?>(
            (request, cancellationToken) => api.SendAsync<재고운송의뢰생성요청, 화주운송의뢰응답>(
                HttpMethod.Post,
                $"{BasePath}/inventory/reconsignment",
                request,
                "재위탁 운송 생성",
                cancellationToken: cancellationToken)));
    }

    public Api작업ViewModel<창고목록응답?> 창고목록조회 { get; }
    public Api작업ViewModel<창고저장요청, 창고요약응답?> 창고생성 { get; }
    public Api작업ViewModel<long, 창고사용자목록응답?> 창고사용자목록조회 { get; }
    public Api작업ViewModel<창고사용자추가조건, 창고사용자항목응답?> 창고사용자추가 { get; }
    public Api작업ViewModel<입고요청목록응답?> 입고목록조회 { get; }
    public Api작업ViewModel<입고요청저장요청, 입고요청항목응답?> 입고생성 { get; }
    public Api작업ViewModel<입고완료조건, 입고상품목록응답?> 입고완료 { get; }
    public Api작업ViewModel<재고목록응답?> 재고목록조회 { get; }
    public Api작업ViewModel<입고검수조건, 창고작업결과응답?> 입고검수 { get; }
    public Api작업ViewModel<적재위치배정조건, 창고작업결과응답?> 적재위치배정 { get; }
    public Api작업ViewModel<포장작업조건, 창고작업결과응답?> 포장작업 { get; }
    public Api작업ViewModel<재고운송의뢰생성요청, 화주운송의뢰응답?> 재위탁운송생성 { get; }
}

public sealed record 창고사용자추가조건(long 창고Id, 창고사용자저장요청 요청);
public sealed record 입고완료조건(long 입고Id, 입고완료요청 요청);
public sealed record 입고검수조건(long 입고항목Id, 입고검수요청 요청);
public sealed record 적재위치배정조건(long 입고항목Id, 적재위치배정요청 요청);
public sealed record 포장작업조건(long 입고항목Id, 포장작업요청 요청);

public sealed class 창고Api기능모음ViewModel : 조립ViewModelBase
{
    public 창고Api기능모음ViewModel(
        창고작업기능ViewModel 작업,
        창고Controller기능모음ViewModel 창고Controllers,
        공통Controller기능모음ViewModel 공통Controllers)
    {
        this.작업 = 하위ViewModel등록(작업);
        this.창고Controllers = 하위ViewModel등록(창고Controllers);
        this.공통Controllers = 하위ViewModel등록(공통Controllers);
    }

    public 창고작업기능ViewModel 작업 { get; }
    public 창고Controller기능모음ViewModel 창고Controllers { get; }
    public 공통Controller기능모음ViewModel 공통Controllers { get; }
}
