using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Mart;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Contracts.Common.Community;

internal static class CommunityWorkBoardCatalog
{
    internal static IReadOnlyList<CommunityActivityBoardBundleDefinition> Bundles { get; } =
    [
        WorkBoard(
            SsalddelProductRoadmapCatalog.FoundationVersion,
            CommunityActivityBoardKeys.FoundationEvidence,
            "공개 근거·학습",
            "공공데이터와 공식 콘텐츠를 확인하고 학습 완료까지 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkFoundation,
            "공개 근거와 학습",
            [CommunityActivityBoardKeys.LegacyFoundation],
            [
                Command(
                    "콘텐츠시청시작Command",
                    "relationship-content-watch-started",
                    "공개 콘텐츠 학습 시작",
                    "공개 콘텐츠 학습 세션을 시작하는 업무 관계입니다."),
                Command(
                    "콘텐츠시청진행Command",
                    "relationship-content-watch-progressed",
                    "공개 콘텐츠 학습 진행",
                    "공개 콘텐츠 학습 진행률을 기록하는 업무 관계입니다."),
                Command(
                    "콘텐츠시청완료Command",
                    "activity-content-watch-completed",
                    "공개 콘텐츠 학습 완료",
                    "한 이웃이 공개 콘텐츠 학습 흐름을 완료했습니다.",
                    publishesActivityPost: true)
            ],
            [
                WebPage("커뮤니티 게시판", CommunityPageRoutes.Boards, "공개 글 목록과 게시판 문맥 조회"),
                WebPage("지역 문화·특산물", RegionalCultureSpecialtyRoutes.Browse, "미국 주와 중국 지역의 문화·특산물 탐색"),
                WebPage("공공데이터", "/information/public-data", "농수축산 가격과 공공 근거 조회"),
                WebPage("공식 음식 재료", "/information/food-ingredients", "공식 레시피·재료·가격 근거 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.IndividualOrderVersion,
            CommunityActivityBoardKeys.IndividualDemand,
            "개별주문·내 원함",
            "한 사람의 철회 가능한 주문 의향과 개별 원장을 등록·변경·철회하는 업무 게시판",
            CommunityBoardGroupCodes.WorkGroupPurchase,
            "개별주문",
            [CommunityActivityBoardKeys.LegacyGroupPurchase],
            [
                Command(
                    "공동구매자동수요등록Command",
                    "relationship-group-purchase-demand-recorded",
                    "개별 수요 등록",
                    "주문자의 비구속 개별 수요를 등록하는 업무 관계입니다."),
                Command(
                    "공동구매자동수요철회Command",
                    "relationship-group-purchase-demand-withdrawn",
                    "개별 수요 철회",
                    "주문자가 비구속 개별 수요를 철회하는 업무 관계입니다.")
            ],
            [
                WebPage("공동구매 수요", CommunityPageRoutes.GroupPurchaseDemand, "비구속 수요 등록 진입"),
                AppPage("OrdererApp", "내 원함 목록", GroupPurchasePageRoutes.WishesRoot, "개별 원함 원장 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.GroupPurchaseVersion,
            CommunityActivityBoardKeys.CollectiveLedger,
            "집단화·공동 원장",
            "개별 원함이 집단 후보와 공동 원장으로 이어지는 과정을 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkGroupPurchase,
            "수요와 공동구매",
            [],
            [
                Event(
                    "커뮤니티원장변경됨Event",
                    "activity-collective-ledger-changed",
                    "공동 원장 변경",
                    "공동구매·같이 수입 원장의 공개 가능한 진행 단계가 변경되었습니다.")
            ],
            [
                WebPage("공동구매 둘러보기", CommunityPageRoutes.GroupPurchase, "공동구매 목록과 모집 흐름 조회"),
                AppPage("OrdererApp", "자동 집단 목록", GroupPurchasePageRoutes.GroupsRoot, "집단화 결과 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            CommunityActivityBoardKeys.HsClassification,
            "품목분류·HS 검토",
            "품목 근거와 HS 후보를 검토하고 전문 판단으로 넘기는 업무 게시판",
            CommunityBoardGroupCodes.WorkTrade,
            "무역 준비",
            [CommunityActivityBoardKeys.LegacyTradeReadiness],
            [
                Command(
                    "화주HsCode검토요청Command",
                    "activity-hs-review-requested",
                    "HS 코드 검토 요청",
                    "무역 준비 과정에서 HS 코드 검토 요청이 접수되었습니다.",
                    publishesActivityPost: true)
            ],
            [
                WebPage("HS 검토함", "/shipper/customs/hs-reviews", "검토가 필요한 HS 후보 조회"),
                WebPage("공식 음식 재료", "/information/food-ingredients", "품목명·재료·가격 근거 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            CommunityActivityBoardKeys.CustomsDelegation,
            "통관 의뢰·동의·수임",
            "통관 의뢰와 조회 동의, 전문 역할의 수임 요청을 분리해 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkTrade,
            "무역 준비",
            [],
            [
                Command(
                    "화주통관의뢰등록Command",
                    "relationship-customs-request-created",
                    "통관 의뢰 등록 요청",
                    "화주가 통관 의뢰를 등록하는 업무 관계입니다."),
                Command(
                    "통관조회동의등록Command",
                    "relationship-customs-consent-recorded",
                    "통관 조회 동의 등록",
                    "당사자가 통관 상태 조회 동의를 등록하는 업무 관계입니다."),
                Command(
                    "통관수임요청Command",
                    "relationship-customs-agent-requested",
                    "통관 수임 요청",
                    "전문 역할에 통관 수임을 요청하는 업무 관계입니다."),
                Event(
                    "화주통관의뢰등록됨Event",
                    "activity-customs-request-created",
                    "통관 의뢰 등록",
                    "같이 수입 준비 과정에서 통관 의뢰가 등록되었습니다."),
                Event(
                    "통관조회동의등록됨Event",
                    "activity-customs-consent-recorded",
                    "통관 조회 동의",
                    "통관 상태 조회를 위한 명시적 동의가 기록되었습니다."),
                Event(
                    "통관수임요청됨Event",
                    "activity-customs-agent-requested",
                    "통관 수임 요청",
                    "통관 절차를 맡을 전문 역할에 수임 요청이 전달되었습니다.")
            ],
            [
                WebPage("같이 수입 준비", CommunityPageRoutes.GroupImport, "같이 수입 원장 준비와 단계 확인"),
                AppPage("OrdererApp", "같이 수입 단계 상세", GroupPurchasePageRoutes.ImportOverviewTemplate, "stable-ID 같이 수입 상세")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            CommunityActivityBoardKeys.CustomsProcess,
            "수입 통관 절차",
            "생성된 통관 절차의 상태 변화와 수입·수출 원장 연결을 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkTrade,
            "무역 준비",
            [],
            [
                Event(
                    "통관절차생성됨Event",
                    "activity-customs-process-created",
                    "통관 절차 생성",
                    "수출입 이행을 위한 통관 절차가 생성되었습니다."),
                Event(
                    "통관상태변경감지됨Event",
                    "activity-customs-status-changed",
                    "통관 상태 변경",
                    "통관 절차의 상태 변경이 확인되었습니다.")
            ],
            [
                WebPage("개별수입 원장", "/orderer/ledgers/individual-import", "개별주문에서 확장된 수입 원장 조회"),
                WebPage("개별수출 원장", "/orderer/ledgers/individual-export", "개별 관계에서 확장된 수출 원장 조회"),
                WebPage("공동수출 원장", "/orderer/ledgers/group-export", "공동 수출 원장 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.TransportVersion,
            CommunityActivityBoardKeys.TransportRequest,
            "운송 의뢰·기사 예약",
            "화주의 운송 의뢰와 기사의 가능 시간 예약을 확정 전 단계에서 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkTransport,
            "운송 이행",
            [CommunityActivityBoardKeys.LegacyTransport],
            [
                Command(
                    "의뢰생성Command",
                    "activity-transport-request-created",
                    "운송 의뢰 등록",
                    "국내 화물 이행을 위한 운송 의뢰가 등록되었습니다.",
                    publishesActivityPost: true),
                Command(
                    "의뢰수정Command",
                    "relationship-transport-request-updated",
                    "운송 의뢰 수정",
                    "화주가 확정 전 운송 의뢰 조건을 수정하는 업무 관계입니다."),
                Command(
                    "의뢰삭제Command",
                    "relationship-transport-request-deleted",
                    "운송 의뢰 삭제",
                    "권한과 현재 상태를 검증해 운송 의뢰를 삭제하는 업무 관계입니다."),
                Command(
                    "예약생성Command",
                    "relationship-driver-reservation-created",
                    "기사 예약 생성",
                    "기사가 운송 가능 시간 예약을 생성하는 업무 관계입니다."),
                Command(
                    "예약취소Command",
                    "relationship-driver-reservation-cancelled",
                    "기사 예약 취소",
                    "기사가 운송 가능 시간 예약을 취소하는 업무 관계입니다.")
            ],
            [
                WebPage("운송 의뢰 작성", "/shipper/request", "화물·운송·절차 입력"),
                WebPage("운송 의뢰 검토", "/shipper/request/review", "의뢰 전 최종 확인"),
                WebPage("기사 추천 목록", "/driver/recommendations", "비구속 배차 후보 조회"),
                WebPage("운송 상세", "/shipper/request/{RequestId}", "stable-key 운송 의뢰 상세")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.TransportVersion,
            CommunityActivityBoardKeys.DispatchDecision,
            "배차 결정",
            "기사의 배차 수락·거절·취소와 결과 Event를 함께 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkTransport,
            "운송 이행",
            [],
            [
                Command("배차수락Command", "relationship-dispatch-accepted", "배차 수락", "기사가 배차 후보를 수락하는 업무 관계입니다."),
                Command("배차거절Command", "relationship-dispatch-declined", "배차 거절", "기사가 배차 후보를 거절하는 업무 관계입니다."),
                Command("배차수락취소Command", "relationship-dispatch-acceptance-cancelled", "배차 수락 취소", "기사가 수락한 배차를 취소하는 업무 관계입니다."),
                Event("배차수락됨Event", "activity-dispatch-accepted", "배차 수락", "운송 의뢰의 배차가 수락되었습니다."),
                Event("배차거절됨Event", "activity-dispatch-declined", "배차 거절", "운송 의뢰의 배차가 거절되어 다음 배차를 기다립니다."),
                Event("배차수락취소됨Event", "activity-dispatch-acceptance-cancelled", "배차 수락 취소", "수락했던 배차가 취소되어 운송 배정 상태가 변경되었습니다.")
            ],
            [
                WebPage("기사 추천 목록", "/driver/recommendations", "비구속 배차 후보 조회"),
                WebPage("기사 현재 운송", "/driver/transports/current", "배차 결과와 현재 운송 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.TransportVersion,
            CommunityActivityBoardKeys.LoadingJourney,
            "상차·운행",
            "상차지 도착과 상차 완료, 운행 시작·종료를 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkTransport,
            "운송 이행",
            [],
            [
                Command("운행시작Command", "relationship-driving-started", "운행 시작", "기사가 배정된 운송의 운행을 시작하는 업무 관계입니다."),
                Command("운행종료Command", "relationship-driving-finished", "운행 종료", "기사가 배정된 운송의 운행을 종료하는 업무 관계입니다."),
                Command("운송상차지도착Command", "relationship-pickup-arrived", "상차지 도착 처리", "기사가 상차지 도착 상태를 확정하는 업무 관계입니다."),
                Command("운송상차완료Command", "relationship-loading-completed", "상차 완료 처리", "기사가 상차 완료 상태를 확정하는 업무 관계입니다."),
                Event("운송상차지도착됨Event", "activity-pickup-arrived", "상차지 도착", "배차된 운송이 상차지 도착 단계에 들어갔습니다."),
                Event("운송상차완료됨Event", "activity-loading-completed", "상차 완료", "배차된 운송의 상차 단계가 완료되었습니다.")
            ],
            [
                WebPage("기사 현재 운송", "/driver/transports/current", "상차와 운행 상태 조회"),
                WebPage("운송 상세", "/shipper/request/{RequestId}", "화주 관점 운송 상태 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.TransportVersion,
            CommunityActivityBoardKeys.DeliveryHandover,
            "하차·인수",
            "하차지 도착과 인수 확인으로 운송을 닫는 업무 게시판",
            CommunityBoardGroupCodes.WorkTransport,
            "운송 이행",
            [],
            [
                Command("운송하차지도착Command", "relationship-dropoff-arrived", "하차지 도착 처리", "기사가 하차지 도착 상태를 확정하는 업무 관계입니다."),
                Command("운송인수완료Command", "relationship-transport-handover-completed", "운송 인수 완료 처리", "인수 확인을 거쳐 운송을 완료하는 업무 관계입니다."),
                Event("운송하차지도착됨Event", "activity-dropoff-arrived", "하차지 도착", "운송이 하차지 도착 단계에 들어갔습니다."),
                Event("운송인수완료됨Event", "activity-transport-handover-completed", "운송 인수 완료", "운송 물품의 인수 확인이 완료되었습니다.")
            ],
            [
                WebPage("기사 현재 운송", "/driver/transports/current", "하차·인수 상태 조회"),
                WebPage("기사 운송 이력", "/driver/transports/history", "완료·과거 운송 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            CommunityActivityBoardKeys.SellerWarehouseReceipt,
            "판매자 출고·주문자 입고",
            "판매자의 출고와 주문자의 입고 확인을 양 끝에서 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkFulfillment,
            "창고·판매 이행",
            [CommunityActivityBoardKeys.LegacyFulfillment],
            [
                Command("판매자출고처리Command", "relationship-seller-shipment-released", "판매자 출고 처리", "판매자가 상품 출고 상태를 확정하는 업무 관계입니다."),
                Command("주문자입고확인Command", "relationship-orderer-receipt-completed", "주문자 입고 확인", "주문자가 상품 입고 완료를 확인하는 업무 관계입니다."),
                Event("판매자상품출고됨Event", "activity-seller-shipment-released", "판매자 출고", "판매자가 물류 이행을 위해 상품을 출고했습니다."),
                Event("주문자상품입고완료됨Event", "activity-orderer-receipt-completed", "주문자 입고 확인", "주문자가 상품 입고 완료를 확인했습니다.")
            ],
            [
                WebPage("판매 주문", SalesOrderPageRoutes.Root, "판매 주문 목록"),
                WebPage("창고 재고", "/warehouse/general/inventory", "접근 가능한 재고 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            CommunityActivityBoardKeys.WarehouseInbound,
            "창고 입고·검수·적재",
            "창고 입고부터 수량·상태 검수와 적재 위치 배정까지 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkFulfillment,
            "창고·판매 이행",
            [],
            [
                Event("창고입고완료됨Event", "activity-warehouse-inbound-completed", "창고 입고 완료", "창고 입고 등록이 완료되었습니다."),
                Event("창고입고검수완료됨Event", "activity-warehouse-inspection-completed", "입고 검수 완료", "입고 상품의 수량·상태 검수가 완료되었습니다."),
                Event("창고적재위치배정됨Event", "activity-warehouse-location-assigned", "적재 위치 배정", "입고 상품의 창고 적재 위치가 배정되었습니다.")
            ],
            [
                WebPage("입고 요청 목록", InboundRequestPageRoutes.Root, "입고 요청 목록 조회"),
                WebPage("입고 검수", InboundInspectionPageRoutes.Root, "입고 검수 대상 목록"),
                WebPage("창고 재고", "/warehouse/general/inventory", "검수 뒤 재고 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            CommunityActivityBoardKeys.PickingHandover,
            "피킹·출고 인계",
            "피킹 작업과 출고 인계 준비, 후속 운송 생성을 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkFulfillment,
            "창고·판매 이행",
            [],
            [
                Event("창고출고인계준비완료됨Event", "activity-warehouse-handover-ready", "출고 인계 준비", "창고에서 다음 운송으로 인계할 출고 준비가 완료되었습니다."),
                Event("창고재위탁운송생성됨Event", "activity-warehouse-transport-created", "재위탁 운송 생성", "창고 이행 뒤 이어질 재위탁 운송 의뢰가 생성되었습니다.")
            ],
            [
                WebPage("피킹 작업", PickingTaskPageRoutes.Root, "피킹 작업 목록"),
                WebPage("출고 인계 검토", "/warehouse/general/outbound-plan-review", "출고예정 원장 읽기 검토"),
                WebPage("판매 주문", SalesOrderPageRoutes.Root, "출고 대상 판매 주문 조회")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.FoodDeliveryVersion,
            CommunityActivityBoardKeys.FoodOrderAcceptance,
            "음식 주문·음식점 수락",
            "주문 등록과 음식점 수락을 분리해 조리 시작 전까지 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkFoodDelivery,
            "음식 주문·배달",
            [CommunityActivityBoardKeys.LegacyFoodDelivery],
            [
                Command("음식주문등록Command", "relationship-food-order-created", "음식 주문 등록 요청", "주문자가 음식 주문을 등록하는 업무 관계입니다."),
                Command("음식점주문수락Command", "relationship-restaurant-order-accepted", "음식점 주문 수락", "음식점이 주문을 수락하는 업무 관계입니다."),
                Event("음식주문등록됨Event", "activity-food-order-created", "음식 주문 등록", "음식점 배달 흐름에 새 주문이 등록되었습니다."),
                Event("음식점주문수락됨Event", "activity-restaurant-order-accepted", "음식점 주문 수락", "음식점이 주문을 수락해 조리·배달 준비가 시작되었습니다.")
            ],
            [
                WebPage("음식 탐색", "/community/discover/food", "공개 음식·재료 탐색"),
                AppPage("OrdererApp", "음식 주문 내역", "/orders/food", "주문자의 음식 주문 목록·상세 진입"),
                AppPage("RestaurantDeskApp", "음식점 주문 수신함", "/orders", "음식점 주문 목록"),
                AppPage("RestaurantDeskApp", "음식점 주문 상세", "/orders/{OrderNo}", "정확한 주문번호 상세·수락")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.FoodDeliveryVersion,
            CommunityActivityBoardKeys.FoodDeliveryHandoff,
            "음식 배달 인계",
            "조리 완료 뒤 기사 픽업과 고객 인계 Command 경계를 보완할 업무 게시판",
            CommunityBoardGroupCodes.WorkFoodDelivery,
            "음식 주문·배달",
            [],
            [],
            [
                AppPage("RestaurantDeskApp", "음식점 주문 상세", "/orders/{OrderNo}", "조리 완료·기사 인계 진입"),
                AppPage("FDriverApp", "배달기사 홈", "home", "픽업·배송 기사 업무 진입")
            ]),

        WorkBoard(
            SsalddelProductRoadmapCatalog.MartVersion,
            CommunityActivityBoardKeys.MartFulfillment,
            "마트 주문·피킹·포장",
            "마트 주문 결제 이후 피킹과 포장, 즉시배송 인계 준비를 점검하는 업무 게시판",
            CommunityBoardGroupCodes.WorkMart,
            "마트·도심 물류",
            [CommunityActivityBoardKeys.LegacyMart],
            [
                Event("주문결제완료됨Event", "activity-mart-order-paid", "마트 주문 결제", "마트 주문의 결제 완료 사실이 물류 흐름에 전달되었습니다."),
                Event("창고피킹완료됨Event", "activity-mart-picking-completed", "상품 피킹 완료", "주문 상품의 피킹 작업이 완료되었습니다."),
                Event("창고포장완료됨Event", "activity-mart-packing-completed", "상품 포장 완료", "피킹된 주문 상품의 포장 작업이 완료되었습니다.")
            ],
            [
                WebPage("마트 공개 상품", MartProductPageRoutes.Root, "공개 상품 목록"),
                WebPage("마트 피킹 주문", "/warehouse/mart/picking", "접근 가능한 피킹 주문 목록"),
                WebPage("마트 업무 흐름", "/warehouse/mart/work-board", "입고·재고·피킹 책임별 진입"),
                WebPage("마트 주문 상세", "/warehouse/mart/picking/orders/{OrderId:long}", "stable-ID 피킹·포장 주문 상세"),
                AppPage("OrdererApp", "마트 주문 요청", MartProductPageRoutes.OrderTemplate, "비구속 상품 주문 요청")
            ])
    ];

    private static CommunityActivityBoardBundleDefinition WorkBoard(
        string productVersion,
        string boardKey,
        string displayName,
        string description,
        string groupCode,
        string groupDisplayName,
        IReadOnlyList<string> boardAliases,
        IReadOnlyList<ActivitySourceSeed> sources,
        IReadOnlyList<CommunityActivityPageDefinition> pages)
    {
        var aliases = boardAliases
            .Concat(sources
                .Where(source => source.PublishesActivityPost)
                .SelectMany(source => new[] { source.LegacyBoardKey, source.ActivityDisplayName }))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var board = new CommunityBoardDefinition(
            boardKey,
            displayName,
            description,
            groupCode,
            groupDisplayName,
            IsUserCreatable: false,
            IsPublic: true,
            PostingAccessCode: CommunityBoardPostingAccessCodes.OperatorOnly,
            LegacyCategoryNames: aliases);
        var activities = sources
            .Select(source => new CommunityActivityBoardDefinition(
                source.SourceKind,
                source.SourceName,
                source.ActivityDisplayName,
                productVersion,
                source.PublicActivitySummary,
                source.PublishesActivityPost,
                board))
            .ToArray();
        return new CommunityActivityBoardBundleDefinition(
            productVersion,
            board,
            activities,
            pages);
    }

    private static ActivitySourceSeed Command(
        string sourceName,
        string legacyBoardKey,
        string activityDisplayName,
        string publicActivitySummary,
        bool publishesActivityPost = false)
        => new(
            CommunityActivitySourceKinds.Command,
            sourceName,
            legacyBoardKey,
            activityDisplayName,
            publicActivitySummary,
            publishesActivityPost);

    private static ActivitySourceSeed Event(
        string sourceName,
        string legacyBoardKey,
        string activityDisplayName,
        string publicActivitySummary)
        => new(
            CommunityActivitySourceKinds.Event,
            sourceName,
            legacyBoardKey,
            activityDisplayName,
            publicActivitySummary,
            PublishesActivityPost: true);

    private static CommunityActivityPageDefinition WebPage(
        string pageName,
        string route,
        string responsibility)
        => new("통합 Web", pageName, route, responsibility, IsWebRoute: true);

    private static CommunityActivityPageDefinition AppPage(
        string surface,
        string pageName,
        string route,
        string responsibility)
        => new(surface, pageName, route, responsibility, IsWebRoute: false);

    private sealed record ActivitySourceSeed(
        string SourceKind,
        string SourceName,
        string LegacyBoardKey,
        string ActivityDisplayName,
        string PublicActivitySummary,
        bool PublishesActivityPost);
}
