namespace Hongdal.ApiMetadata;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class HongdalApiVersionAttribute : Attribute
{
    public HongdalApiVersionAttribute(HongdalProductVersion version)
    {
        Version = version;
    }

    public HongdalProductVersion Version { get; }

    public string VersionLabel => HongdalProductVersionLabels.GetLabel(Version);

    public string? FeatureKey { get; set; }

    public string? WorkflowKey { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class HongdalApiWorkflowAttribute : Attribute
{
    public HongdalApiWorkflowAttribute(HongdalWorkflow workflow)
    {
        Workflow = workflow;
    }

    public HongdalWorkflow Workflow { get; }

    public string WorkflowLabel => HongdalWorkflowLabels.GetLabel(Workflow);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class HongdalApiGrowthTrackAttribute : Attribute
{
    public HongdalApiGrowthTrackAttribute(HongdalApiGrowthTrack track)
    {
        Track = track;
    }

    public HongdalApiGrowthTrack Track { get; }

    public string TrackLabel => HongdalApiGrowthTrackLabels.GetLabel(Track);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class HongdalUseCaseAttribute : Attribute
{
    public HongdalUseCaseAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string Summary { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = true;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class HongdalUseCaseActorAttribute : Attribute
{
    public HongdalUseCaseActorAttribute(HongdalActor actor, HongdalUseCaseActorRole role = HongdalUseCaseActorRole.Primary)
    {
        Actor = actor;
        Role = role;
    }

    public HongdalActor Actor { get; }

    public HongdalUseCaseActorRole Role { get; }

    public string ActorCode => Actor.ToString();

    public string ActorLabel => HongdalActorLabels.GetLabel(Actor);

    public string RoleLabel => HongdalUseCaseActorRoleLabels.GetLabel(Role);
}

public enum HongdalProductVersion
{
    V1_0 = 100,
    V1_5 = 150,
    V2_0 = 200,
    V2_5 = 250,
    V3_0 = 300,
    V3_5 = 350
}

public enum HongdalApiGrowthTrack
{
    CoreLogistics = 100,
    Community = 200,
    Warehouse = 300,
    Customs = 400,
    OrdererGroupCommerce = 500,
    FoodDelivery = 600,
    HongdalMart = 700,
    PlatformOperations = 800
}

public enum HongdalWorkflow
{
    DomesticTransport = 100,
    WarehouseFulfillment = 200,
    CustomsAndTradeData = 300,
    GroupPurchaseImport = 400,
    SalesChannelFulfillment = 500,
    CommunityTrust = 600,
    HrParticipation = 700,
    FoodDelivery = 800,
    HongdalMart = 900
}

public enum HongdalWorkflowRelationKind
{
    References = 100,
    Calls = 200,
    HandsOffTo = 300,
    Feeds = 400,
    PublishesSignalTo = 500,
    OperatesWith = 600
}

public enum HongdalActor
{
    Shipper = 100,
    Driver = 200,
    Recipient = 300,
    PlatformOperator = 400,
    WarehouseManager = 500,
    ShipperOrSeller = 600,
    CustomsBroker = 700,
    OrdererGroupLeader = 800,
    Orderer = 900,
    OverseasSellerOrForwarder = 1000,
    Seller = 1100,
    CommunityMember = 1200,
    Worker = 1300,
    EmployerOrOperatingEntity = 1400,
    Restaurant = 1500,
    FoodDeliveryDriver = 1600,
    MartOperator = 1700
}

public enum HongdalUseCaseActorRole
{
    Primary = 100,
    Supporting = 200
}

public sealed record HongdalWorkflowRelation(
    HongdalWorkflow Source,
    HongdalWorkflow Target,
    HongdalWorkflowRelationKind Kind,
    string Summary);

public sealed record HongdalWorkflowParticipant(
    HongdalWorkflow Workflow,
    string ActorCode,
    string ActorName,
    bool IsPrimary,
    string Responsibility);

public sealed record HongdalWorkflowScreen(
    HongdalWorkflow Workflow,
    string ActorCode,
    string AppCode,
    string AppName,
    string ScreenName,
    string Route,
    string Purpose);

public static class HongdalProductVersionLabels
{
    public static string GetLabel(HongdalProductVersion version)
    {
        return version switch
        {
            HongdalProductVersion.V1_0 => "1.0",
            HongdalProductVersion.V1_5 => "1.5",
            HongdalProductVersion.V2_0 => "2.0",
            HongdalProductVersion.V2_5 => "2.5",
            HongdalProductVersion.V3_0 => "3.0",
            HongdalProductVersion.V3_5 => "3.5",
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Unknown Hongdal product version.")
        };
    }
}

public static class HongdalWorkflowLabels
{
    public static string GetLabel(HongdalWorkflow workflow)
    {
        return workflow switch
        {
            HongdalWorkflow.DomesticTransport => "국내 화물 운송",
            HongdalWorkflow.WarehouseFulfillment => "창고 입출고",
            HongdalWorkflow.CustomsAndTradeData => "통관·무역 데이터",
            HongdalWorkflow.GroupPurchaseImport => "공동주문 수입",
            HongdalWorkflow.SalesChannelFulfillment => "판매채널 출고",
            HongdalWorkflow.CommunityTrust => "커뮤니티 신뢰",
            HongdalWorkflow.HrParticipation => "참여 인력 관리",
            HongdalWorkflow.FoodDelivery => "음식 배달",
            HongdalWorkflow.HongdalMart => "홍달마트",
            _ => throw new ArgumentOutOfRangeException(nameof(workflow), workflow, "Unknown Hongdal workflow.")
        };
    }
}

public static class HongdalWorkflowRelationKindLabels
{
    public static string GetLabel(HongdalWorkflowRelationKind kind)
    {
        return kind switch
        {
            HongdalWorkflowRelationKind.References => "참조",
            HongdalWorkflowRelationKind.Calls => "호출",
            HongdalWorkflowRelationKind.HandsOffTo => "인계",
            HongdalWorkflowRelationKind.Feeds => "공급",
            HongdalWorkflowRelationKind.PublishesSignalTo => "신호 공개",
            HongdalWorkflowRelationKind.OperatesWith => "공동 운영",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown Hongdal workflow relation kind.")
        };
    }
}

public static class HongdalActorLabels
{
    public static string GetLabel(HongdalActor actor)
    {
        return actor switch
        {
            HongdalActor.Shipper => "화주",
            HongdalActor.Driver => "기사",
            HongdalActor.Recipient => "수령자",
            HongdalActor.PlatformOperator => "플랫폼 운영자",
            HongdalActor.WarehouseManager => "창고 관리자",
            HongdalActor.ShipperOrSeller => "화주·판매자",
            HongdalActor.CustomsBroker => "관세사",
            HongdalActor.OrdererGroupLeader => "주문자 집단 대표",
            HongdalActor.Orderer => "주문자",
            HongdalActor.OverseasSellerOrForwarder => "해외 판매자·배송대행지",
            HongdalActor.Seller => "판매자",
            HongdalActor.CommunityMember => "커뮤니티 참여자",
            HongdalActor.Worker => "참여 인력",
            HongdalActor.EmployerOrOperatingEntity => "고용·운영 주체",
            HongdalActor.Restaurant => "음식점",
            HongdalActor.FoodDeliveryDriver => "배달 기사",
            HongdalActor.MartOperator => "홍달마트 운영자",
            _ => throw new ArgumentOutOfRangeException(nameof(actor), actor, "Unknown Hongdal actor.")
        };
    }
}

public static class HongdalUseCaseActorRoleLabels
{
    public static string GetLabel(HongdalUseCaseActorRole role)
    {
        return role switch
        {
            HongdalUseCaseActorRole.Primary => "주 액터",
            HongdalUseCaseActorRole.Supporting => "보조 액터",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown Hongdal use case actor role.")
        };
    }
}

public static class HongdalWorkflowRelations
{
    private static readonly IReadOnlyList<HongdalWorkflowRelation> Items =
    [
        new(
            HongdalWorkflow.GroupPurchaseImport,
            HongdalWorkflow.CustomsAndTradeData,
            HongdalWorkflowRelationKind.References,
            "공동주문 수입은 HS 코드, BL/AWB, 문서관리번호, 통관 단계, 수출입 단가 데이터를 참조합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            HongdalWorkflow.DomesticTransport,
            HongdalWorkflowRelationKind.HandsOffTo,
            "보세구역 반출 뒤 아파트 직행 배송이나 국내 3PL 이동이 필요하면 국내 화물 운송으로 인계합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            HongdalWorkflow.WarehouseFulfillment,
            HongdalWorkflowRelationKind.HandsOffTo,
            "국내 3PL 입고를 선택하면 공동수입 물품을 창고 입고, 재고, 출고 가능 상태로 넘깁니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            HongdalWorkflow.SalesChannelFulfillment,
            HongdalWorkflowRelationKind.Feeds,
            "공동수입 재고를 스마트스토어, 쿠팡, Amazon 같은 판매채널 출품 후보로 공급합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            HongdalWorkflow.HrParticipation,
            HongdalWorkflowRelationKind.OperatesWith,
            "공동주문 분류, 배분, 단지 내부 보조 업무가 필요하면 참여 인력 관리와 함께 운영합니다."),
        new(
            HongdalWorkflow.SalesChannelFulfillment,
            HongdalWorkflow.WarehouseFulfillment,
            HongdalWorkflowRelationKind.Calls,
            "판매채널 주문이 들어오면 재고 확인과 출고 배치를 창고 입출고 워크플로우에 요청합니다."),
        new(
            HongdalWorkflow.SalesChannelFulfillment,
            HongdalWorkflow.DomesticTransport,
            HongdalWorkflowRelationKind.HandsOffTo,
            "출고 뒤 화물 배송이나 재위탁 운송이 필요하면 국내 화물 운송으로 인계합니다."),
        new(
            HongdalWorkflow.HongdalMart,
            HongdalWorkflow.WarehouseFulfillment,
            HongdalWorkflowRelationKind.Calls,
            "홍달마트 주문은 도심 재고, 피킹, 포장 처리를 창고 입출고 흐름과 연결합니다."),
        new(
            HongdalWorkflow.HongdalMart,
            HongdalWorkflow.DomesticTransport,
            HongdalWorkflowRelationKind.HandsOffTo,
            "포장 완료 뒤 기사 인계와 배송 증빙이 필요하면 국내 화물 운송으로 인계합니다."),
        new(
            HongdalWorkflow.FoodDelivery,
            HongdalWorkflow.DomesticTransport,
            HongdalWorkflowRelationKind.HandsOffTo,
            "음식점 픽업과 고객 전달은 운송 실행, 위치, 완료 증빙 흐름으로 합류합니다."),
        new(
            HongdalWorkflow.DomesticTransport,
            HongdalWorkflow.CommunityTrust,
            HongdalWorkflowRelationKind.PublishesSignalTo,
            "상하차 완료, 감사, 후기 같은 공개 가능한 운송 활동 신호를 커뮤니티 신뢰 흐름으로 보냅니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            HongdalWorkflow.CommunityTrust,
            HongdalWorkflowRelationKind.PublishesSignalTo,
            "공동주문 모집, 투표, 진행 상태, 분배 후기를 개인정보 보호 범위 안에서 커뮤니티 신호로 보냅니다."),
        new(
            HongdalWorkflow.SalesChannelFulfillment,
            HongdalWorkflow.CommunityTrust,
            HongdalWorkflowRelationKind.PublishesSignalTo,
            "판매 후기와 상품 여정 신호를 동의된 범위에서 커뮤니티 신뢰 흐름으로 보냅니다.")
    ];

    public static IReadOnlyList<HongdalWorkflowRelation> GetAll() => Items;

    public static IReadOnlyList<HongdalWorkflowRelation> GetOutgoing(HongdalWorkflow source)
        => Items.Where(item => item.Source == source).ToArray();

    public static IReadOnlyList<HongdalWorkflowRelation> GetIncoming(HongdalWorkflow target)
        => Items.Where(item => item.Target == target).ToArray();
}

public static class HongdalWorkflowParticipants
{
    private static readonly IReadOnlyList<HongdalWorkflowParticipant> Items =
    [
        new(
            HongdalWorkflow.DomesticTransport,
            "Shipper",
            "화주",
            true,
            "운송 의뢰, 상차·하차 조건, 결제 조건을 제시합니다."),
        new(
            HongdalWorkflow.DomesticTransport,
            "Driver",
            "기사",
            true,
            "추천 의뢰를 수락하고 상차, 운행, 하차, 증빙 제출을 수행합니다."),
        new(
            HongdalWorkflow.DomesticTransport,
            "Recipient",
            "수령자",
            false,
            "하차 확인, 인수 확인, 수령 관련 정보를 확인합니다."),
        new(
            HongdalWorkflow.DomesticTransport,
            "PlatformOperator",
            "플랫폼 운영자",
            false,
            "배차, 예외, 정산 지연, 분쟁 상태를 관리합니다."),
        new(
            HongdalWorkflow.WarehouseFulfillment,
            "WarehouseManager",
            "창고 관리자",
            true,
            "입고, 검수, 적재, 재고, 포장, 출고 작업을 관리합니다."),
        new(
            HongdalWorkflow.WarehouseFulfillment,
            "ShipperOrSeller",
            "화주·판매자",
            false,
            "입고 상품과 출고 요청의 기준 정보를 제공합니다."),
        new(
            HongdalWorkflow.CustomsAndTradeData,
            "CustomsBroker",
            "관세사",
            true,
            "HS 코드, 통관 단계, 수출입 검토, 서류 보정 의견을 제공합니다."),
        new(
            HongdalWorkflow.CustomsAndTradeData,
            "PlatformOperator",
            "플랫폼 운영자",
            false,
            "공공 데이터 조회 결과와 내부 원장을 연결합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            "OrdererGroupLeader",
            "주문자 집단 대표",
            true,
            "공동주문 개설, 배송 방식 선택, 분배 기준 합의를 이끕니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            "Orderer",
            "주문자",
            true,
            "구매 의사, 비용 분담, 수령 확인, 투표에 참여합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            "OverseasSellerOrForwarder",
            "해외 판매자·배송대행지",
            false,
            "상품 정보, 포장 정보, BL/AWB, 송장·스티커 정보를 제공합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            "PlatformOperator",
            "플랫폼 운영자",
            false,
            "원장, 통관 조회, 국내 운송 인계, 비용 정산 기준을 관리합니다."),
        new(
            HongdalWorkflow.SalesChannelFulfillment,
            "Seller",
            "판매자",
            true,
            "판매채널 계정, 상품 출품, 가격, 재고 판매 정책을 관리합니다."),
        new(
            HongdalWorkflow.SalesChannelFulfillment,
            "WarehouseManager",
            "창고 관리자",
            false,
            "주문 발생 뒤 재고 확인과 출고 작업을 수행합니다."),
        new(
            HongdalWorkflow.CommunityTrust,
            "CommunityMember",
            "커뮤니티 참여자",
            true,
            "후기, 문의, 투표, 활동 신호를 확인하고 소통합니다."),
        new(
            HongdalWorkflow.CommunityTrust,
            "PlatformOperator",
            "플랫폼 운영자",
            false,
            "개인정보 보호, 신고, 숨김, 공개 범위 정책을 관리합니다."),
        new(
            HongdalWorkflow.HrParticipation,
            "Worker",
            "참여 인력",
            true,
            "분류, 배분, 보조 업무, 경비·관리 보조 같은 실제 일을 수행합니다."),
        new(
            HongdalWorkflow.HrParticipation,
            "EmployerOrOperatingEntity",
            "고용·운영 주체",
            true,
            "역할, 근로계약, 보상, 4대보험 신고 준비 책임을 가집니다."),
        new(
            HongdalWorkflow.FoodDelivery,
            "Restaurant",
            "음식점",
            true,
            "주문 접수, 조리, 픽업 가능 상태를 관리합니다."),
        new(
            HongdalWorkflow.FoodDelivery,
            "FoodDeliveryDriver",
            "배달 기사",
            true,
            "픽업, 이동, 고객 전달, 완료 증빙을 수행합니다."),
        new(
            HongdalWorkflow.HongdalMart,
            "MartOperator",
            "홍달마트 운영자",
            true,
            "상품, 도심 재고, 피킹·포장 기준을 관리합니다."),
        new(
            HongdalWorkflow.HongdalMart,
            "Driver",
            "기사",
            false,
            "포장 완료 물품을 인계받아 배송합니다.")
    ];

    public static IReadOnlyList<HongdalWorkflowParticipant> GetAll() => Items;

    public static IReadOnlyList<HongdalWorkflowParticipant> GetByWorkflow(HongdalWorkflow workflow)
        => Items.Where(item => item.Workflow == workflow).ToArray();

    public static string GetBoundarySummary(HongdalWorkflow workflow)
    {
        return workflow switch
        {
            HongdalWorkflow.DomesticTransport => "운송 의뢰가 배차되어 상차, 하차, 증빙, 정산 후보 상태까지 진행되는 범위를 책임집니다.",
            HongdalWorkflow.WarehouseFulfillment => "물품이 창고에 들어온 뒤 재고화되고 출고 가능 상태가 되거나 출고 배치로 넘어가는 범위를 책임집니다.",
            HongdalWorkflow.CustomsAndTradeData => "수출입 판단에 필요한 공공 데이터, HS 코드, 통관 상태, 관세사 검토 정보를 제공하는 범위를 책임집니다.",
            HongdalWorkflow.GroupPurchaseImport => "주문자 집단이 해외 상품을 공동으로 들여와 통관, 국내 반출, 분배 또는 3PL 입고로 넘기는 범위를 책임집니다.",
            HongdalWorkflow.SalesChannelFulfillment => "상품을 판매채널에 출품하고 주문을 창고 출고 또는 운송 인계로 연결하는 범위를 책임집니다.",
            HongdalWorkflow.CommunityTrust => "업무 활동에서 공개 가능한 신뢰 신호, 후기, 투표, 관계 기록을 개인정보 보호 범위 안에서 다루는 책임을 가집니다.",
            HongdalWorkflow.HrParticipation => "플랫폼 업무에 참여하는 인력의 역할, 계약, 보상, 신고 준비 상태를 관리하는 범위를 책임집니다.",
            HongdalWorkflow.FoodDelivery => "음식 주문이 조리, 픽업, 고객 전달, 완료 증빙으로 이어지는 범위를 책임집니다.",
            HongdalWorkflow.HongdalMart => "홍달마트 주문이 도심 재고, 피킹, 포장, 기사 인계로 이어지는 범위를 책임집니다.",
            _ => throw new ArgumentOutOfRangeException(nameof(workflow), workflow, "Unknown Hongdal workflow.")
        };
    }
}

public static class HongdalWorkflowScreens
{
    private static readonly IReadOnlyList<HongdalWorkflowScreen> Items =
    [
        new(
            HongdalWorkflow.DomesticTransport,
            "Shipper",
            "ShipperApp",
            "화주 앱",
            "운송 의뢰",
            "/shipper/request",
            "화주가 상차지, 하차지, 화물 조건, 결제 조건을 입력합니다."),
        new(
            HongdalWorkflow.DomesticTransport,
            "Driver",
            "DriverApp",
            "기사 앱",
            "추천 목록",
            "/driver/recommendations",
            "기사가 추천된 일반 화물, 공동주문 운송, 배송 의뢰를 구분해 확인합니다."),
        new(
            HongdalWorkflow.DomesticTransport,
            "Driver",
            "DriverApp",
            "기사 앱",
            "진행 중 운송",
            "/driver/transports/current",
            "기사의 상차, 하차, 인수 확인, 증빙 제출 흐름을 진행합니다."),
        new(
            HongdalWorkflow.DomesticTransport,
            "Driver",
            "DriverApp",
            "기사 앱",
            "월 정산",
            "/driver/settlements/current-month",
            "기사 정산 후보, 지급 상태, 이용료 정보를 확인합니다."),
        new(
            HongdalWorkflow.DomesticTransport,
            "PlatformOperator",
            "HongdalAdmin",
            "관리자 앱",
            "운송 관리",
            "/transports",
            "운송 진행, 예외, 분쟁, 운영 상태를 관리합니다."),
        new(
            HongdalWorkflow.WarehouseFulfillment,
            "WarehouseManager",
            "WarehouseManagerApp",
            "창고 관리자 앱",
            "입고 작업",
            "/work/inbound",
            "창고 작업자가 입고 시작과 입고 검수 흐름으로 진입합니다."),
        new(
            HongdalWorkflow.WarehouseFulfillment,
            "WarehouseManager",
            "WarehouseManagerApp",
            "창고 관리자 앱",
            "작업 보드",
            "/work-board",
            "대기 중인 입고, 포장, 출고 작업을 확인합니다."),
        new(
            HongdalWorkflow.WarehouseFulfillment,
            "WarehouseManager",
            "WarehouseManagerApp",
            "창고 관리자 앱",
            "스캔 스테이션",
            "/scan",
            "입고, 출고, 포장 공정의 스캔과 현장 확인을 수행합니다."),
        new(
            HongdalWorkflow.WarehouseFulfillment,
            "ShipperOrSeller",
            "ShipperApp",
            "화주 앱",
            "창고 재고",
            "/shipper/warehouse/inventory",
            "화주나 판매자가 입고상품, 재고, 출고 가능 상태를 확인합니다."),
        new(
            HongdalWorkflow.CustomsAndTradeData,
            "CustomsBroker",
            "CustomsBrokerApp",
            "관세사 앱",
            "관세사 홈",
            "/",
            "관세사가 HS 코드, 식품/일반화물 분류, 통관 주의 태그를 보정합니다."),
        new(
            HongdalWorkflow.CustomsAndTradeData,
            "ShipperOrSeller",
            "ShipperApp",
            "화주 앱",
            "HS 코드 검토",
            "/shipper/customs/hs-reviews",
            "화주가 상품의 HS 코드 후보, 통관 리스크, 관세사 검토 필요성을 확인합니다."),
        new(
            HongdalWorkflow.CustomsAndTradeData,
            "PlatformOperator",
            "HongdalAdmin",
            "관리자 앱",
            "HS 코드 운영",
            "/customs/hs-codes",
            "운영자가 HS 코드 데이터와 통관 보정 정보를 관리합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            "Orderer",
            "OrdererApp",
            "주문자 앱",
            "수입 공동구매",
            "/group-purchase",
            "주문자가 공동주문 상품, 비용, 선적·통관 상태, 분배 조건을 확인합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            "OrdererGroupLeader",
            "OrdererApp",
            "주문자 앱",
            "수입 공동구매",
            "/group-purchase",
            "주문자 집단 대표가 공동주문 개설, 운송 방식, 분배 기준을 조정합니다."),
        new(
            HongdalWorkflow.GroupPurchaseImport,
            "PlatformOperator",
            "HongdalAdmin",
            "관리자 앱",
            "공동주문 운영",
            "/dashboard",
            "운영자가 공동주문 원장, 통관 연계, 국내 운송 인계, 예외를 추적합니다."),
        new(
            HongdalWorkflow.SalesChannelFulfillment,
            "Seller",
            "ShipperApp",
            "화주 앱",
            "판매채널 연결",
            "/shipper/sales/channels",
            "판매자가 스마트스토어, 쿠팡, Amazon 같은 판매채널 계정을 연결합니다."),
        new(
            HongdalWorkflow.SalesChannelFulfillment,
            "Seller",
            "ShipperApp",
            "화주 앱",
            "상품 출품",
            "/shipper/sales/listings",
            "판매자가 판매상품을 채널별 출품 후보와 상세 정보로 준비합니다."),
        new(
            HongdalWorkflow.SalesChannelFulfillment,
            "Seller",
            "ShipperApp",
            "화주 앱",
            "주문 이행",
            "/shipper/sales/orders",
            "판매채널 주문을 창고 출고와 운송 인계로 연결합니다."),
        new(
            HongdalWorkflow.CommunityTrust,
            "CommunityMember",
            "OrdererApp",
            "주문자 앱",
            "홈 커뮤니티",
            "/",
            "주문자가 커뮤니티 글, 후기, 활동 신호를 확인합니다."),
        new(
            HongdalWorkflow.CommunityTrust,
            "PlatformOperator",
            "HongdalAdmin",
            "관리자 앱",
            "홈 커뮤니티",
            "/",
            "운영자가 커뮤니티 글, 신고, 숨김, 고정 상태를 관리합니다."),
        new(
            HongdalWorkflow.HrParticipation,
            "EmployerOrOperatingEntity",
            "HongdalAdmin",
            "관리자 앱",
            "인력·4대보험 신고 준비",
            "/dashboard",
            "운영자가 역할, 근로계약, 4대보험 신고 준비 상태를 관리합니다."),
        new(
            HongdalWorkflow.FoodDelivery,
            "Restaurant",
            "OrdererApp",
            "주문자 앱",
            "음식점",
            "/food/restaurants",
            "주문자가 음식점과 메뉴를 확인하고 음식 주문 흐름으로 진입합니다."),
        new(
            HongdalWorkflow.FoodDelivery,
            "FoodDeliveryDriver",
            "DriverApp",
            "기사 앱",
            "추천 목록",
            "/driver/recommendations",
            "배달 기사가 음식 픽업·전달 추천 의뢰를 확인합니다."),
        new(
            HongdalWorkflow.HongdalMart,
            "MartOperator",
            "WarehouseManagerApp",
            "창고 관리자 앱",
            "홍달마트 작업 홈",
            "/mart",
            "마트 운영자가 도심 재고, 피킹, 포장, 기사 인계 작업으로 진입합니다."),
        new(
            HongdalWorkflow.HongdalMart,
            "Orderer",
            "OrdererApp",
            "주문자 앱",
            "홍달마트 주문",
            "/food/mart",
            "주문자가 도심 창고 재고 기반 마트 상품을 주문합니다.")
    ];

    public static IReadOnlyList<HongdalWorkflowScreen> GetAll() => Items;

    public static IReadOnlyList<HongdalWorkflowScreen> GetByWorkflow(HongdalWorkflow workflow)
        => Items.Where(item => item.Workflow == workflow).ToArray();

    public static IReadOnlyList<HongdalWorkflowScreen> GetByWorkflowAndActor(HongdalWorkflow workflow, string actorCode)
        => Items
            .Where(item => item.Workflow == workflow &&
                string.Equals(item.ActorCode, actorCode, StringComparison.OrdinalIgnoreCase))
            .ToArray();
}

public static class HongdalApiGrowthTrackLabels
{
    public static string GetLabel(HongdalApiGrowthTrack track)
    {
        return track switch
        {
            HongdalApiGrowthTrack.CoreLogistics => "Core Logistics",
            HongdalApiGrowthTrack.Community => "Community",
            HongdalApiGrowthTrack.Warehouse => "Warehouse",
            HongdalApiGrowthTrack.Customs => "Customs",
            HongdalApiGrowthTrack.OrdererGroupCommerce => "Orderer Group Commerce",
            HongdalApiGrowthTrack.FoodDelivery => "Food Delivery",
            HongdalApiGrowthTrack.HongdalMart => "Hongdal Mart",
            HongdalApiGrowthTrack.PlatformOperations => "Platform Operations",
            _ => throw new ArgumentOutOfRangeException(nameof(track), track, "Unknown Hongdal API growth track.")
        };
    }
}
