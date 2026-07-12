using Hongdal.Contracts.Common.Community;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private static 원장블록흐름도 원장블록흐름도생성(CommunityLedgerTemplateResponse template)
    {
        var nodes = template.Key switch
        {
            CommunityLedgerTemplateKeys.HongdalMart =>
            [
                상품노드("마트 주문", "주문 블록", "주문 상품, 수량, 수령 조건을 먼저 구조화합니다."),
                창고노드("도심 재고", "재고 거점 블록", "가까운 창고의 재고 근거와 예약 수량을 잡습니다."),
                작업노드("피킹/포장", "피킹·포장 블록", "재고를 꺼내고 포장 완료 상태를 한 노드에서 처리합니다.", "포장 완료 뒤 픽업과 전달이 열립니다."),
                배송노드("픽업/전달", "기사 픽업/고객 전달", "기사 픽업, 이동, 고객 전달, 수령 증빙을 한 노드에서 닫습니다.")
            ],
            CommunityLedgerTemplateKeys.WarehouseOutbound =>
            [
                상품노드("출고 요청", "출고 품목 블록", "출고할 상품, 수량, 목적, 도착 조건을 먼저 정리합니다."),
                작업노드("피킹/검수", "피킹·검수 블록", "창고 안에서 찾고 수량과 상태를 확인하는 단계를 묶습니다."),
                작업노드("포장", "포장 블록", "포장 완료, 라벨, 송장, 출고 가능 상태를 처리합니다.", "포장 완료 뒤 운송 인계가 열립니다."),
                배송노드("운송 인계", "운송 인계/화물 운송 원장", "창고 밖으로 나가는 재위탁 운송, 택배, 기사 픽업을 한 노드에서 넘깁니다.")
            ],
            CommunityLedgerTemplateKeys.WarehouseInbound =>
            [
                상품노드("입고 요청", "입고 예정 블록", "입고될 상품, 수량, 납품 조건을 먼저 정리합니다."),
                배송노드("납품/도착", "납품 상태 블록", "외부에서 창고로 들어오는 도착 예정, 도착, 지연 상태를 묶습니다."),
                작업노드("검수/이상", "검수·이상 블록", "수량, 파손, 누락, 보완 조치를 한 노드에서 확인합니다."),
                창고노드("보관/재고화", "보관 위치/재고 전환", "검수 뒤 보관 위치를 잡고 실제 재고로 전환합니다.")
            ],
            CommunityLedgerTemplateKeys.LocalSale =>
            [
                상품노드("판매/예약", "판매 물건/예약", "판매 물건, 예약 조건, 구매 의향을 정리합니다."),
                창고노드("보관/준비", "판매자 보관 또는 창고", "상품 보관 위치와 전달 전 준비 상태를 확인합니다."),
                배송노드("전달", "직거래/배송 블록", "직거래, 택배, 가까운 배송을 한 노드에서 처리합니다."),
                확인노드("정산/확인", "확인/정산 표시", "수령 확인, 입금 표시, 거래 마감 메모를 남깁니다.")
            ],
            CommunityLedgerTemplateKeys.GroupPurchase =>
            [
                상품노드("수요 모집", "수요/참여 블록", "참여자, 희망 수량, 희망가, 비확정 의향을 모읍니다."),
                작업노드("수입 결정", "수량·가격·방식 결정", "모인 수요를 보고 수입 진행 여부, FCL/LCL, 공급자 조건을 정합니다."),
                작업노드("해외 구매/선적", "해외 발주·선적 블록", "발주, 인보이스, 패킹리스트, B/L 또는 AWB 추적을 묶습니다."),
                작업노드("통관/검토", "통관·관세사 검토", "HS 코드, 통관 리스크, 관세사 검토, 보완 서류를 한 노드에서 봅니다."),
                배송노드("국내 인계/마감", "창고·운송·판매 전환", "창고 입고, 국내 운송, 실제 주문/결제 전환, 정산 마감을 묶습니다.")
            ],
            CommunityLedgerTemplateKeys.FoodOrder =>
            [
                상품노드("음식 주문", "메뉴/주문 블록", "메뉴, 수량, 수령 또는 배달 조건을 먼저 정리합니다."),
                작업노드("조리", "조리 상태 블록", "접수, 조리 중, 조리 완료, 품절 변경을 한 노드에서 봅니다."),
                배송노드("픽업/배달", "픽업·배달 블록", "픽업 대기, 픽업 완료, 이동 중 상태를 묶습니다."),
                확인노드("전달/정산", "전달 확인/정산 표시", "전달 결과, 수령 증빙, 정산 상태를 한 노드에서 닫습니다.")
            ],
            CommunityLedgerTemplateKeys.FoodDelivery =>
            [
                상품노드("음식 주문", "픽업 의뢰 블록", "배달할 음식, 픽업지, 전달 조건을 먼저 정리합니다."),
                작업노드("조리", "픽업 준비 블록", "조리 완료 또는 픽업 준비 상태를 확인합니다."),
                배송노드("픽업/배달", "픽업·배달 블록", "픽업, 이동, 전달 전 상태를 한 노드에서 처리합니다."),
                확인노드("전달/정산", "전달 확인/정산 표시", "전달 완료, 수령 증빙, 배달비 정산 표시를 묶습니다.")
            ],
            CommunityLedgerTemplateKeys.Errand =>
            [
                상품노드("요청", "요청 내용 블록", "도움이 필요한 일, 시간, 장소, 조건을 먼저 정리합니다."),
                작업노드("수행", "진행 상태 블록", "참여, 진행 중, 지연, 보류 같은 수행 상태를 한 노드에서 봅니다."),
                확인노드("확인/마감", "확인/증빙 블록", "완료 확인, 증빙, 후속 메모를 남겨 생활 요청을 닫습니다.")
            ],
            CommunityLedgerTemplateKeys.CargoTransport =>
            [
                상품노드("운송 의뢰", "의뢰/배차 블록", "화물, 상하차지, 시간, 운임 조건과 배차 진입 조건을 정리합니다."),
                확인노드("상차", "상차 블록", "상차지 도착, 상차 완료, 상차 증빙을 한 노드 안에서 처리합니다."),
                확인노드("하차", "하차 블록", "하차지 도착, 하차 완료, 수령/인수 확인을 한 노드 안에서 처리합니다."),
                확인노드("결제/정산", "결제/정산 블록", "결제 확보, 후불 승인, 정산대기, 보류, 완료, 환불 상태를 한 노드 안에서 표시합니다.")
            ],
            _ => 기본원장블록노드생성(template)
        };

        return new 원장블록흐름도(nodes, 원장블록흐름규칙생성(template));
    }

    private static IReadOnlyList<string> 원장블록흐름규칙생성(CommunityLedgerTemplateResponse template)
    {
        var rules = new List<string>();

        if (상품수요원장인가(template))
        {
            rules.Add("상품을 원하는 원장은 상품/주문 블록, 창고 또는 재고 근거 블록, 배송/전달 블록이 함께 있어야 완결됩니다.");
            rules.Add("판매자 창고인지 대행업체 창고인지는 나중에 정해져도 되지만, 어떤 창고/재고에서 나가는지는 반드시 연결해야 합니다.");
        }

        if (창고또는재고를사용하는가(template))
        {
            rules.Add("창고와 창고를 잇는 흐름은 중간에 배송/운송 블록을 끼워 창고 A -> 배송 -> 창고 B로 표시합니다.");
        }

        rules.Add("배송 블록은 포장, 검수, 출고 준비처럼 앞선 조건이 충족된 뒤 열리는 후속 블록입니다.");
        rules.Add("이 다이어그램은 화면 구성과 원장 조건을 보여주는 연결도이며, 실제 처리는 아래 API 경로 후보나 OS 스케줄링 단계에서 이어집니다.");

        return rules;
    }

    private static IReadOnlyList<원장블록노드> 기본원장블록노드생성(CommunityLedgerTemplateResponse template)
    {
        var nodes = new List<원장블록노드>();

        if (template.UiSectionHints.Any(상품성격섹션인가))
        {
            nodes.Add(상품노드("요청", "상품/요청 블록", "사용자가 원하는 대상과 조건을 먼저 구조화합니다."));
        }

        if (template.UiSectionHints.Any(창고성격섹션인가))
        {
            nodes.Add(창고노드("거점", "창고/재고 블록", "상품이나 물건이 머무는 장소와 근거를 잡습니다."));
        }

        if (template.UiSectionHints.Any(작업성격섹션인가))
        {
            nodes.Add(작업노드("작업", "상태/작업 블록", "피킹, 검수, 포장, 조리 같은 중간 작업을 구조화합니다."));
        }

        if (template.UiSectionHints.Any(배송성격섹션인가))
        {
            nodes.Add(배송노드("배송/전달", "배송 블록", "장소와 장소 사이의 이동을 별도 배송 블록으로 표시합니다."));
        }

        if (template.UiSectionHints.Any(확인성격섹션인가) || nodes.Count == 0)
        {
            nodes.Add(확인노드("확인", "확인/증빙 블록", "완료, 수령, 증빙, 메모를 남깁니다."));
        }

        return nodes.Count > 1
            ? nodes
            : [상품노드("요청", "원장 시작 블록", "사용자가 원하는 일을 먼저 적습니다."), 확인노드("확인", "완료 확인 블록", "처리 결과와 증빙을 남깁니다.")];
    }

    private static bool 상품수요원장인가(CommunityLedgerTemplateResponse template)
        => template.Key is CommunityLedgerTemplateKeys.HongdalMart
            or CommunityLedgerTemplateKeys.WarehouseOutbound
            or CommunityLedgerTemplateKeys.WarehouseInbound
            or CommunityLedgerTemplateKeys.LocalSale
            or CommunityLedgerTemplateKeys.GroupPurchase
            or CommunityLedgerTemplateKeys.FoodOrder
            || template.UiSectionHints.Any(상품성격섹션인가);

    private static bool 창고또는재고를사용하는가(CommunityLedgerTemplateResponse template)
        => template.UiSectionHints.Any(창고성격섹션인가)
            || template.EngineHints.Any(engine => engine.Contains("출고", StringComparison.Ordinal) || engine.Contains("피킹", StringComparison.Ordinal));

    private static bool 상품성격섹션인가(string section)
        => ContainsAny(section, "상품", "물건", "품목", "메뉴", "주문", "화물", "모집 수량");

    private static bool 판매채널성격섹션인가(string section)
        => ContainsAny(section, "판매", "판매채널", "채널", "스토어", "스마트스토어", "쿠팡", "마켓", "상점", "주문 연동", "sales", "channel", "commerce", "store", "marketplace");

    private static bool 창고성격섹션인가(string section)
        => ContainsAny(section, "창고", "재고", "입고", "출고", "보관", "상차지", "픽업지", "도심 재고", "분배");

    private static bool 작업성격섹션인가(string section)
        => ContainsAny(section, "피킹", "검수", "포장", "조리", "진행 상태", "상태", "납품");

    private static bool 배송성격섹션인가(string section)
        => ContainsAny(section, "배송", "전달", "운송", "인계", "픽업", "하차", "도착지", "고객 전달");

    private static bool 확인성격섹션인가(string section)
        => ContainsAny(section, "확인", "증빙", "수령", "정산", "마감", "이상 기록", "타임라인");

    private static bool ContainsAny(string value, params string[] keywords)
        => keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));

    private static 원장블록노드 상품노드(string title, string groupLabel, string description, string? condition = null)
        => new(title, groupLabel, description, "product", Color.Primary, condition);

    private static 원장블록노드 창고노드(string title, string groupLabel, string description, string? condition = null)
        => new(title, groupLabel, description, "warehouse", Color.Success, condition);

    private static 원장블록노드 작업노드(string title, string groupLabel, string description, string? condition = null)
        => new(title, groupLabel, description, "work", Color.Warning, condition);

    private static 원장블록노드 배송노드(string title, string groupLabel, string description, string? condition = null)
        => new(title, groupLabel, description, "delivery", Color.Secondary, condition);

    private static 원장블록노드 확인노드(string title, string groupLabel, string description, string? condition = null)
        => new(title, groupLabel, description, "confirm", Color.Info, condition);
}
