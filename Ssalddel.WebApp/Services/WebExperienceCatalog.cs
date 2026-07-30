using Ssalddel.WebApp.Models;
using MudBlazor;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.WebApp.Services;

public static class WebExperienceCatalog
{
    public static IReadOnlyList<WebExperienceRole> Roles { get; } =
    [
        new(
            "community",
            "role-experience--community",
            "01 · COMMUNITY",
            "커뮤니티",
            "생활·업무 게시판에서 이야기를 시작하고 공동구매 제안과 공동행동 원장으로 이어지는 공개 커뮤니티입니다.",
            "/images/role-previews/community-orderer.png",
            "모바일 살뜰 게시판 화면",
            Icons.Material.Filled.Groups,
            "/community",
            "/roles/01/",
            "01 커뮤니티 열기",
            [
                new("생활·업무 게시판", "공개 글과 댓글을 읽고 필요한 이야기를 시작합니다.", CommunityPageRoutes.Home, Icons.Material.Filled.Forum, "01A"),
                new("지역 문화·특산물", "지역의 문화와 특산물을 이해한 뒤 관심과 질문으로 이어갑니다.", CommunityPageRoutes.Regions, Icons.Material.Filled.TravelExplore, "01A.07"),
                new("공동구매 시작", "조건과 역할을 공개적으로 맞추는 비구속 제안을 만듭니다.", CommunityPageRoutes.GroupPurchaseCreate, Icons.Material.Filled.GroupAdd, "01B"),
                new("공동행동으로 전환", "참여 의향과 수량을 확인하고 공동 원장으로 이어지는 흐름을 봅니다.", CommunityPageRoutes.GroupPurchase, Icons.Material.Filled.AccountTree, "01C"),
                new("내 활동", "내 글, 참여, 원장과 알림을 개인정보 공개 범위와 분리해 확인합니다.", CommunityPageRoutes.Personal, Icons.Material.Filled.AccountCircle, "01 운영")
            ]),
        new(
            "orderer",
            "role-experience--orderer",
            "02 · ORDERER",
            "주문자",
            "국내 가격 탐색에서 같이 주문, 같이 수입·물류 검토와 원장 운영까지 맥락별 화면으로 나누어 확인합니다.",
            "/images/role-previews/community-orderer.png",
            "모바일 주문자·공동구매 화면",
            Icons.Material.Filled.ShoppingCartCheckout,
            WebOrdererRoutes.Home,
            "/roles/02/",
            "02 주문자 열기",
            [
                new("범위·우선순위", "국내 사용자의 개별 필요와 비구속 의향부터 시작하는 현재 범위를 확인합니다.", WebOrdererRoutes.Home, Icons.Material.Filled.Rule, "02 · 00"),
                new("국내 탐색·가격", "KAMIS 원문 품목과 거래 단위를 유지한 국내 유통단계 가격을 확인합니다.", "/information/kamis-domestic-price-comparison", Icons.Material.Filled.Insights, "02 · KR 01"),
                new("같이 주문", "공동구매를 둘러보고 품목·수량·수령 조건을 개별 수요로 등록합니다.", WebOrdererRoutes.GroupPurchaseDemand, Icons.Material.Filled.GroupWork, "02 · KR 02"),
                new("같이 수입·물류", "해외 공급 조건과 국내 수요를 FCL/LCL 목표와 실행 전 검토 원장으로 연결합니다.", CommunityPageRoutes.GroupImport, Icons.Material.Filled.Public, "02 · KR 03"),
                new("원장·운영", "개별 주문 원천과 수입 검토 상태를 보존하고 다음 역할 인계를 확인합니다.", WebOrdererRoutes.IndividualImportLedger, Icons.Material.Filled.Assignment, "02 · KR 04")
            ]),
        new(
            "shipper",
            "role-experience--shipper",
            "03 · SHIPPER",
            "화주",
            "운송 의뢰 작성과 입고·재고, 국제 운송 판단, 물류대행 조건 검토를 화주 업무 흐름으로 확인합니다.",
            "/images/role-previews/shipper-seller.png",
            "화주 역할이 선택된 살뜰 앱 화면",
            Icons.Material.Filled.LocalShipping,
            ShipperRoutes.Home,
            "/roles/03/",
            "03 화주 열기",
            [
                new("화주 모바일 업무 홈", "현재 운송·입고·재고 업무와 다음 행동을 한 화면에서 확인합니다.", ShipperRoutes.Home, Icons.Material.Filled.Dashboard, "03 · Mobile SRP"),
                new("운송 의뢰", "화물, 상하차지, 차량과 비용 조건을 단계별로 입력합니다.", ShipperRoutes.Request, Icons.Material.Filled.PostAdd, "03A"),
                new("입고·재고", "입고 예정과 완료, 보관 재고를 원장 기준으로 확인합니다.", ShipperRoutes.InboundDashboard, Icons.Material.Filled.MoveToInbox, "03B"),
                new("통관·FCL/LCL", "수입량과 비용을 비교하고 국제 운송 방식을 검토합니다.", ShipperRoutes.FclLclPlanner, Icons.Material.Filled.Public, "03C"),
                new("물류대행 조건 검토", "운송 의뢰의 조건과 책임 범위를 최종 확인하되 계약 확정은 별도 동의로 남깁니다.", ShipperRoutes.RequestReview, Icons.Material.Filled.FactCheck, "03P1")
            ]),
        new(
            "driver",
            "role-experience--driver",
            "04 · DRIVER",
            "기사",
            "추천 운송을 검토하고 현재 운송의 상차·하차·증빙과 정산 상태를 기사 업무 흐름으로 확인합니다.",
            "/images/role-previews/driver.png",
            "기사 운송 업무 화면",
            Icons.Material.Filled.Route,
            DriverRoutes.Home,
            "/roles/04/",
            "04 기사 열기",
            [
                new("기사 업무 홈", "오늘의 운행과 다음 행동을 확인합니다.", DriverRoutes.Home, Icons.Material.Filled.Dashboard, "04 · Mobile SRP"),
                new("운행 시작", "업무 가능 상태와 출발 조건을 확인합니다.", DriverRoutes.WorkStart, Icons.Material.Filled.PlayArrow, "04A"),
                new("추천 운송", "거리·차량·비용 조건을 확인하고 후보를 검토합니다.", DriverRoutes.Recommendations, Icons.Material.Filled.TaskAlt, "04B"),
                new("현재 운송", "배차 뒤 상차·이동·하차 상태와 증빙을 이어서 관리합니다.", DriverRoutes.CurrentTransport, Icons.Material.Filled.LocalShipping, "04C"),
                new("정산", "완료 운송과 지급 검토 상태를 확인합니다.", DriverRoutes.Settlements, Icons.Material.Filled.ReceiptLong, "04D")
            ]),
        new(
            "warehouse",
            "role-experience--warehouse",
            "05 · WAREHOUSE",
            "창고",
            "입고 검수와 재고, 피킹·포장, 출고 인계를 창고의 실제 작업 순서로 확인합니다.",
            "/images/role-previews/warehouse.png",
            "창고 작업 화면",
            Icons.Material.Filled.Warehouse,
            WarehouseManagerRoutes.Home,
            "/roles/05/",
            "05 창고 열기",
            [
                new("창고 업무 홈", "입고·출고·재고 업무와 다음 행동을 확인합니다.", WarehouseManagerRoutes.Home, Icons.Material.Filled.Dashboard, "05 · Mobile SRP"),
                new("작업 보드", "진행할 창고 작업을 상태와 우선순위로 확인합니다.", WarehouseManagerRoutes.WorkBoard, Icons.Material.Filled.ViewKanban, "05A"),
                new("입고 검수", "입고 품목과 수량을 확인하고 같은 원장을 다시 조회합니다.", WarehouseManagerRoutes.InboundInspection, Icons.Material.Filled.FactCheck, "05B"),
                new("재고", "보관 위치와 가용 재고를 확인합니다.", WarehouseManagerRoutes.GeneralInventory, Icons.Material.Filled.Inventory2, "05C"),
                new("피킹", "출고 대상의 피킹 묶음과 실행 상태를 관리합니다.", WarehouseManagerRoutes.PickingBatch, Icons.Material.Filled.Checklist, "05D")
            ])
    ];

    public static WebExperienceRole DefaultRole => Roles[0];

    public static WebExperienceRole Find(string? key)
        => Roles.FirstOrDefault(role =>
               string.Equals(role.Key, key, StringComparison.OrdinalIgnoreCase))
           ?? DefaultRole;
}
