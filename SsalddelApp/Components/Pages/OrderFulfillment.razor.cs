using MudBlazor;
using Ssalddel.Contracts.Common.Sales;
using SsalddelApp.ViewModels.Shipper;
using MudColor = MudBlazor.Color;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillment
{
    private OrderFulfillmentReadViewModel Read => ViewModel.조회;

    private static readonly IReadOnlyList<OrderFulfillmentDestination> Destinations =
    [
        new("판매채널 주문 동기화", "연결된 판매채널 주문을 서버 출고 원장으로 동기화하고 같은 원장을 다시 조회합니다.", OrderFulfillmentSimulationPageRoutes.Samples, Icons.Material.Filled.Sync, MudColor.Primary),
        new("Simulation 주문", "로컬 주문 후보를 검색하고 stable 주문 key 상세로 이동합니다.", OrderFulfillmentSimulationPageRoutes.Orders, Icons.Material.Filled.ReceiptLong, MudColor.Primary),
        new("재고·입고 신호", "마켓 주문 가능 재고와 입고 필요 검토 신호를 읽습니다.", OrderFulfillmentSimulationPageRoutes.Inventory, Icons.Material.Filled.Inventory2, MudColor.Info),
        new("피킹 작업", "피킹 task 목록에서 정확한 task ID의 스캔·예외 화면을 엽니다.", OrderFulfillmentSimulationPageRoutes.Picking, Icons.Material.Filled.QrCodeScanner, MudColor.Warning),
        new("포장 작업", "포장 task 목록에서 정확한 task ID의 시작·완료 화면을 엽니다.", OrderFulfillmentSimulationPageRoutes.Packing, Icons.Material.Filled.Inventory, MudColor.Secondary),
        new("입고 알림 정책", "판매자별 동의 상태와 로컬 발송 의도를 별도 화면에서 검토합니다.", OrderFulfillmentSimulationPageRoutes.RestockPolicy, Icons.Material.Filled.NotificationsActive, MudColor.Tertiary)
    ];

    protected override Task OnInitializedAsync()
        => ViewModel.새로고침Async();

    private Task RefreshAsync()
        => ViewModel.새로고침Async();

    private sealed record OrderFulfillmentDestination(
        string Title,
        string Description,
        string Href,
        string Icon,
        MudColor Color);
}
