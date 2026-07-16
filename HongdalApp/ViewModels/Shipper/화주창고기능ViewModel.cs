using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using HongdalApp.Services;

namespace HongdalApp.ViewModels.Shipper;

public sealed class 화주창고기능ViewModel : 조립ViewModelBase
{
    public 화주창고기능ViewModel(IShipperOperationsService operations)
    {
        창고목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<창고요약응답>>(operations.GetWarehousesAsync));
        입고목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<입고요청항목응답>>(operations.GetInboundsAsync));
        재고목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<재고항목응답>>(operations.GetInventoryAsync));
    }

    public Api작업ViewModel<IReadOnlyList<창고요약응답>> 창고목록조회 { get; }
    public Api작업ViewModel<IReadOnlyList<입고요청항목응답>> 입고목록조회 { get; }
    public Api작업ViewModel<IReadOnlyList<재고항목응답>> 재고목록조회 { get; }
}
