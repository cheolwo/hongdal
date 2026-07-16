using FDriverApp.Services;
using Hongdal.Contracts.Common.Drivers;
using Hongdal.Contracts.Driver.Food;
using Hongdal.Contracts.Driver.Work;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace FDriverApp.ViewModels;

/// <summary>
/// 음식 배달 기사 Controller의 작업 단위를 페이지에서 조립할 수 있게 나눈 하위 ViewModel입니다.
/// </summary>
public sealed class 음식배달기사업무기능ViewModel : 조립ViewModelBase
{
    public 음식배달기사업무기능ViewModel(IFoodDeliveryDriverApiService api)
    {
        작업공간조회 = 하위ViewModel등록(
            new Api작업ViewModel<FoodDeliveryDriverWorkspaceDto>(api.GetWorkspaceAsync));
        운행상태조회 = 하위ViewModel등록(
            new Api작업ViewModel<기사운행상태응답?>(api.GetWorkStatusAsync));
        운행시작 = 하위ViewModel등록(
            new Api작업ViewModel<string, Api작업완료>(async (startLocation, cancellationToken) =>
            {
                await api.StartWorkAsync(startLocation, cancellationToken);
                return Api작업완료.값;
            }));
        운행종료 = 하위ViewModel등록(
            new Api작업ViewModel<Api작업완료>(async cancellationToken =>
            {
                await api.StopWorkAsync(cancellationToken);
                return Api작업완료.값;
            }));
        위치갱신 = 하위ViewModel등록(
            new Api작업ViewModel<기사위치갱신요청, 기사위치갱신응답?>(api.UpdateLocationAsync));
        배달제안수락 = 하위ViewModel등록(
            new Api작업ViewModel<string, FoodDeliveryDriverActionResponse>(api.AcceptAsync));
        묶음배달수락 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<string>, FoodDeliveryDriverActionResponse>(api.AcceptBundleAsync));
        픽업완료 = 하위ViewModel등록(
            new Api작업ViewModel<string, FoodDeliveryDriverActionResponse>(api.ConfirmPickupAsync));
        배달완료 = 하위ViewModel등록(
            new Api작업ViewModel<string, FoodDeliveryDriverActionResponse>(api.CompleteAsync));
        경로조회 = 하위ViewModel등록(
            new Api작업ViewModel<FoodDeliveryDriverRouteRequestDto, FoodDeliveryDriverRouteResponseDto>(api.GetRouteAsync));
    }

    public Api작업ViewModel<FoodDeliveryDriverWorkspaceDto> 작업공간조회 { get; }
    public Api작업ViewModel<기사운행상태응답?> 운행상태조회 { get; }
    public Api작업ViewModel<string, Api작업완료> 운행시작 { get; }
    public Api작업ViewModel<Api작업완료> 운행종료 { get; }
    public Api작업ViewModel<기사위치갱신요청, 기사위치갱신응답?> 위치갱신 { get; }
    public Api작업ViewModel<string, FoodDeliveryDriverActionResponse> 배달제안수락 { get; }
    public Api작업ViewModel<IReadOnlyList<string>, FoodDeliveryDriverActionResponse> 묶음배달수락 { get; }
    public Api작업ViewModel<string, FoodDeliveryDriverActionResponse> 픽업완료 { get; }
    public Api작업ViewModel<string, FoodDeliveryDriverActionResponse> 배달완료 { get; }
    public Api작업ViewModel<FoodDeliveryDriverRouteRequestDto, FoodDeliveryDriverRouteResponseDto> 경로조회 { get; }
}

public sealed class 음식배달기사Api기능모음ViewModel : 조립ViewModelBase
{
    public 음식배달기사Api기능모음ViewModel(
        음식배달기사업무기능ViewModel 업무,
        음식배달기사Controller기능모음ViewModel 음식배달Controllers,
        기사Controller기능모음ViewModel 기사Controllers,
        공통Controller기능모음ViewModel 공통Controllers)
    {
        this.업무 = 하위ViewModel등록(업무);
        this.음식배달Controllers = 하위ViewModel등록(음식배달Controllers);
        this.기사Controllers = 하위ViewModel등록(기사Controllers);
        this.공통Controllers = 하위ViewModel등록(공통Controllers);
    }

    public 음식배달기사업무기능ViewModel 업무 { get; }
    public 음식배달기사Controller기능모음ViewModel 음식배달Controllers { get; }
    public 기사Controller기능모음ViewModel 기사Controllers { get; }
    public 공통Controller기능모음ViewModel 공통Controllers { get; }
}
