using Hongdal.Contracts.Food;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using RestaurantDeskApp.Models.Restaurant;
using RestaurantDeskApp.Services;

namespace RestaurantDeskApp.ViewModels;

/// <summary>
/// 서버 음식 주문 조회와 음식점 데스크의 수락·전표 흐름을 한 업무 경계로 묶습니다.
/// </summary>
public sealed class 음식점주문기능ViewModel : 조립ViewModelBase
{
    public 음식점주문기능ViewModel(
        I음식주문ApiClient api,
        I음식점주문DeskService desk)
    {
        서버주문목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<음식주문응답>>(api.주문목록조회Async));
        서버주문상세조회 = 하위ViewModel등록(
            new Api작업ViewModel<string, 음식주문응답?>(api.주문상세조회Async));
        데스크주문목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<음식점주문DeskItem>>(desk.주문목록조회Async));
        주문알림수신 = 하위ViewModel등록(
            new Api작업ViewModel<음식점주문수신Payload, 음식점주문DeskItem>(desk.주문알림수신Async));
        주문수락및전표준비 = 하위ViewModel등록(
            new Api작업ViewModel<string, 음식점주문수락결과>(desk.주문수락후전표준비Async));
        전표출력완료 = 하위ViewModel등록(
            new Api작업ViewModel<string, Api작업완료>(async (orderNumber, cancellationToken) =>
            {
                await desk.전표출력완료Async(orderNumber, cancellationToken);
                return Api작업완료.값;
            }));
    }

    public Api작업ViewModel<IReadOnlyList<음식주문응답>> 서버주문목록조회 { get; }
    public Api작업ViewModel<string, 음식주문응답?> 서버주문상세조회 { get; }
    public Api작업ViewModel<IReadOnlyList<음식점주문DeskItem>> 데스크주문목록조회 { get; }
    public Api작업ViewModel<음식점주문수신Payload, 음식점주문DeskItem> 주문알림수신 { get; }
    public Api작업ViewModel<string, 음식점주문수락결과> 주문수락및전표준비 { get; }
    public Api작업ViewModel<string, Api작업완료> 전표출력완료 { get; }
}

public sealed class 음식점Api기능모음ViewModel : 조립ViewModelBase
{
    public 음식점Api기능모음ViewModel(
        음식점주문기능ViewModel 주문,
        음식Controller기능모음ViewModel 음식Controllers,
        공통Controller기능모음ViewModel 공통Controllers)
    {
        this.주문 = 하위ViewModel등록(주문);
        this.음식Controllers = 하위ViewModel등록(음식Controllers);
        this.공통Controllers = 하위ViewModel등록(공통Controllers);
    }

    public 음식점주문기능ViewModel 주문 { get; }
    public 음식Controller기능모음ViewModel 음식Controllers { get; }
    public 공통Controller기능모음ViewModel 공통Controllers { get; }
}
