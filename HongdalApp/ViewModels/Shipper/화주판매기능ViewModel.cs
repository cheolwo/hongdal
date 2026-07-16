using Hongdal.Contracts.Common.Sales;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using HongdalApp.Services;
using HongdalApp.Services.Commerce;

namespace HongdalApp.ViewModels.Shipper;

public sealed class 화주판매기능ViewModel : 조립ViewModelBase
{
    public 화주판매기능ViewModel(IShipperSalesService sales)
    {
        지원채널조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<CommerceChannelDescriptor>>(sales.GetSupportedChannelsAsync));
        계정목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<판매채널계정목록응답?>(sales.GetAccountsAsync));
        계정등록 = 하위ViewModel등록(
            new Api작업ViewModel<판매채널계정저장요청, 판매채널계정항목응답?>(sales.CreateAccountAsync));
        상품목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<판매상품목록응답?>(sales.GetProductsAsync));
        상품등록 = 하위ViewModel등록(
            new Api작업ViewModel<판매상품저장요청, 판매상품항목응답?>(sales.CreateProductAsync));
        출품목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<채널출품목록응답?>(sales.GetListingsAsync));
        출품등록 = 하위ViewModel등록(
            new Api작업ViewModel<채널출품저장요청, 채널출품항목응답?>(sales.CreateListingAsync));
    }

    public Api작업ViewModel<IReadOnlyList<CommerceChannelDescriptor>> 지원채널조회 { get; }
    public Api작업ViewModel<판매채널계정목록응답?> 계정목록조회 { get; }
    public Api작업ViewModel<판매채널계정저장요청, 판매채널계정항목응답?> 계정등록 { get; }
    public Api작업ViewModel<판매상품목록응답?> 상품목록조회 { get; }
    public Api작업ViewModel<판매상품저장요청, 판매상품항목응답?> 상품등록 { get; }
    public Api작업ViewModel<채널출품목록응답?> 출품목록조회 { get; }
    public Api작업ViewModel<채널출품저장요청, 채널출품항목응답?> 출품등록 { get; }
}
