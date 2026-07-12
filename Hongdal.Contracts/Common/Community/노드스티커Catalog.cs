namespace Hongdal.Contracts.Common.Community;

public static class 노드스티커Catalog
{
    public static 노드스티커표준Response 표준 { get; } = 노드스티커표준Response.기본();

    public static IReadOnlyList<노드스티커팩Response> 기본팩목록 { get; } =
    [
        new()
        {
            팩Key = "hongdal-basic-work-node-stickers",
            제목 = "홍달 기본 업무 노드 스티커",
            창작자표시명 = "Hongdal",
            요약 = "원장 다이어그램에 바로 붙일 수 있는 기본 업무 노드 이미지 팩입니다.",
            원장템플릿Keys =
            [
                CommunityLedgerTemplateKeys.CargoTransport,
                CommunityLedgerTemplateKeys.HongdalMart,
                CommunityLedgerTemplateKeys.WarehouseInbound,
                CommunityLedgerTemplateKeys.WarehouseOutbound,
                CommunityLedgerTemplateKeys.FoodDelivery,
                CommunityLedgerTemplateKeys.FoodOrder
            ],
            스타일Tags = ["flat", "business", "sample"],
            이미지목록 =
            [
                이미지(
                    "basic-request",
                    "요청 노드",
                    "#2563eb",
                    "요",
                    "요청",
                    노드종류목록: ["product"],
                    노드제목목록: ["운송 의뢰", "마트 주문", "음식 주문", "출고 요청", "입고 요청", "요청"],
                    역할라벨목록: ["요청자"]),
                이미지(
                    "basic-warehouse",
                    "창고 노드",
                    "#16a34a",
                    "창",
                    "창고",
                    노드종류목록: ["warehouse"],
                    노드제목목록: ["도심 재고", "보관/재고화", "보관/준비", "거점"],
                    역할라벨목록: ["창고"]),
                이미지(
                    "basic-work",
                    "작업 노드",
                    "#d97706",
                    "작",
                    "작업",
                    노드종류목록: ["work"],
                    노드제목목록: ["피킹/포장", "피킹/검수", "포장", "검수/이상", "조리", "수행"],
                    상태라벨목록: ["진행 중", "처리 중"],
                    역할라벨목록: ["작업자"]),
                이미지(
                    "basic-delivery",
                    "배송 노드",
                    "#7c3aed",
                    "배",
                    "배송",
                    노드종류목록: ["delivery"],
                    노드제목목록: ["픽업/전달", "운송 인계", "납품/도착", "픽업/배달", "전달"],
                    역할라벨목록: ["기사", "운송자"]),
                이미지(
                    "basic-confirm",
                    "확인 노드",
                    "#0891b2",
                    "확",
                    "확인",
                    노드종류목록: ["confirm"],
                    노드제목목록: ["상차", "하차", "결제/정산", "전달/정산", "정산/확인", "확인/마감"],
                    상태라벨목록: ["처리 완료", "완료"],
                    역할라벨목록: ["확인자", "수령자"])
            ]
        },
        new()
        {
            팩Key = "creator-logistics-emotion-node-stickers",
            제목 = "물류 감정 업무 노드 스티커",
            창작자표시명 = "샘플 디자이너",
            요약 = "창고, 배송, 정산 흐름을 조금 더 개성 있게 표현하는 유료 샘플 이미지 팩입니다.",
            원장템플릿Keys =
            [
                CommunityLedgerTemplateKeys.CargoTransport,
                CommunityLedgerTemplateKeys.HongdalMart,
                CommunityLedgerTemplateKeys.WarehouseInbound,
                CommunityLedgerTemplateKeys.WarehouseOutbound
            ],
            스타일Tags = ["creator-sample", "friendly", "logistics"],
            거래정책 = new()
            {
                가격모드 = 노드스티커가격모드.유료,
                가격금액 = 1200,
                통화Code = "KRW",
                창작자정산정책 = "FakePG 개발 결제에서는 정산을 실제 수행하지 않고, 유료 팩 구매 흐름만 검증합니다."
            },
            이미지목록 =
            [
                이미지(
                    "creator-warehouse-ready",
                    "창고 준비 노드",
                    "#0f766e",
                    "준",
                    "준비",
                    원장템플릿Keys:
                    [
                        CommunityLedgerTemplateKeys.HongdalMart,
                        CommunityLedgerTemplateKeys.WarehouseOutbound
                    ],
                    노드종류목록: ["warehouse", "work"],
                    노드제목목록: ["도심 재고", "보관/준비", "피킹/포장", "출고 준비"],
                    상태라벨목록: ["준비 중", "진행 중"],
                    역할라벨목록: ["창고", "작업자"],
                    팩Key: "creator-logistics-emotion-node-stickers"),
                이미지(
                    "creator-delivery-complete",
                    "배송 완료 노드",
                    "#db2777",
                    "완",
                    "완료",
                    원장템플릿Keys:
                    [
                        CommunityLedgerTemplateKeys.CargoTransport,
                        CommunityLedgerTemplateKeys.HongdalMart
                    ],
                    노드종류목록: ["delivery", "confirm"],
                    노드제목목록: ["상차", "하차", "픽업/전달", "납품/도착", "결제/정산"],
                    상태라벨목록: ["처리 완료", "완료"],
                    역할라벨목록: ["기사", "수령자", "확인자"],
                    팩Key: "creator-logistics-emotion-node-stickers")
            ]
        }
    ];

    public static IReadOnlyList<노드스티커이미지Response> 전체이미지목록 { get; } =
        기본팩목록.SelectMany(팩 => 팩.이미지목록).ToArray();

    public static 노드스티커이미지Response? 이미지찾기(string? 이미지Key)
        => string.IsNullOrWhiteSpace(이미지Key)
            ? null
            : 전체이미지목록.FirstOrDefault(이미지 => string.Equals(이미지.이미지Key, 이미지Key, StringComparison.OrdinalIgnoreCase));

    public static 노드스티커이미지Response? 노드기본이미지찾기(노드스티커매칭Request request)
        => 전체이미지목록
            .Where(이미지 => string.Equals(이미지.검수상태, 노드스티커검수상태.승인, StringComparison.OrdinalIgnoreCase))
            .Where(무료팩이미지인가)
            .Select(이미지 => new
            {
                이미지,
                점수 = 점수계산(이미지, request)
            })
            .Where(후보 => 후보.점수 > 0)
            .OrderByDescending(후보 => 후보.점수)
            .ThenBy(후보 => 후보.이미지.이미지Key, StringComparer.OrdinalIgnoreCase)
            .Select(후보 => 후보.이미지)
            .FirstOrDefault();

    public static bool 표준준수여부(노드스티커이미지Response 이미지)
        => 이미지.원본너비Px == 표준.원본캔버스크기Px
           && 이미지.원본높이Px == 표준.원본캔버스크기Px
           && 표준.허용MimeTypes.Contains(이미지.MimeType, StringComparer.OrdinalIgnoreCase)
           && !string.IsNullOrWhiteSpace(이미지.이미지Url)
           && !string.IsNullOrWhiteSpace(이미지.대체Text)
           && 이미지.노드종류목록.Count + 이미지.노드제목목록.Count > 0;

    private static int 점수계산(노드스티커이미지Response 이미지, 노드스티커매칭Request request)
    {
        var 점수 = 0;
        점수 += 하나라도일치하는가(이미지.원장템플릿Keys, request.원장템플릿Key) ? 12 : 0;
        점수 += 하나라도일치하는가(이미지.노드종류목록, request.노드종류) ? 40 : 0;
        점수 += 하나라도일치하는가(이미지.노드제목목록, request.노드제목, 포함허용: true) ? 30 : 0;
        점수 += 하나라도일치하는가(이미지.상태라벨목록, request.상태라벨, 포함허용: true) ? 8 : 0;
        점수 += 하나라도일치하는가(이미지.역할라벨목록, request.역할라벨, 포함허용: true) ? 8 : 0;
        return 점수;
    }

    private static bool 하나라도일치하는가(IReadOnlyList<string> 값목록, string 후보값, bool 포함허용 = false)
    {
        if (값목록.Count == 0 || string.IsNullOrWhiteSpace(후보값))
        {
            return false;
        }

        return 값목록.Any(값 =>
            string.Equals(값, 후보값, StringComparison.OrdinalIgnoreCase) ||
            (포함허용 &&
             (후보값.Contains(값, StringComparison.OrdinalIgnoreCase) ||
              값.Contains(후보값, StringComparison.OrdinalIgnoreCase))));
    }

    private static bool 무료팩이미지인가(노드스티커이미지Response 이미지)
        => 기본팩목록.Any(팩 =>
            string.Equals(팩.팩Key, 이미지.팩Key, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(팩.거래정책.가격모드, 노드스티커가격모드.무료, StringComparison.OrdinalIgnoreCase));

    private static 노드스티커이미지Response 이미지(
        string 이미지Key,
        string 표시명,
        string 강조색상,
        string 글자,
        string 라벨,
        IReadOnlyList<string>? 원장템플릿Keys = null,
        IReadOnlyList<string>? 노드종류목록 = null,
        IReadOnlyList<string>? 노드제목목록 = null,
        IReadOnlyList<string>? 상태라벨목록 = null,
        IReadOnlyList<string>? 역할라벨목록 = null,
        string 팩Key = "hongdal-basic-work-node-stickers")
        => new()
        {
            이미지Key = 이미지Key,
            팩Key = 팩Key,
            표시명 = 표시명,
            이미지Url = 노드스티커샘플이미지UrlFactory.생성(강조색상, 글자, 라벨),
            대체Text = $"{라벨} 업무 노드 스티커",
            MimeType = "image/svg+xml",
            원본너비Px = 표준.원본캔버스크기Px,
            원본높이Px = 표준.원본캔버스크기Px,
            원장템플릿Keys = 원장템플릿Keys ?? [],
            노드종류목록 = 노드종류목록 ?? [],
            노드제목목록 = 노드제목목록 ?? [],
            상태라벨목록 = 상태라벨목록 ?? [],
            역할라벨목록 = 역할라벨목록 ?? [],
            스타일Tags = ["flat", "sample"],
            라이선스Code = 노드스티커라이선스Code.플랫폼노드사용,
            검수상태 = 노드스티커검수상태.승인
        };
}
