using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Participants;
using Hongdal.Contracts.Food;
using Hongdal.Services.Community;
using 홍달.도메인.창고;

namespace Hongdal.Tests.Services.Community;

public sealed class FoodMartLedgerMongoSyncBuilderTests
{
    [Fact]
    public void Food_order_builder_creates_food_order_ledger_with_order_reference()
    {
        var order = new 음식주문응답
        {
            주문번호 = "FOOD-100",
            음식점Id = 10,
            음식점명 = "동네식당",
            음식점주소 = "서울 강서구 화곡로",
            주문자UserId = "orderer-1",
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = "홍길동",
                주소 = "서울 강서구 공항대로",
                상세주소 = "101호",
                주문자본인수령여부 = true
            },
            상품목록 =
            [
                new 음식주문상품Dto { 상품명 = "김치찌개", 수량 = 1, 단가 = 9000m }
            ],
            총주문금액 = 9000m,
            상태 = 음식주문상태코드.주문대기,
            배차상태 = 음식주문배차상태코드.미요청,
            결제수단 = "FakePG",
            CreatedAt = DateTime.UtcNow
        };

        var request = 음식마트원장Mongo동기화Builder.음식주문저장요청생성(order);

        Assert.Equal("food-order:FOOD-100", request.원장Id);
        Assert.Equal(CommunityLedgerTemplateKeys.FoodOrder, request.원장템플릿Key);
        Assert.Equal(CommunityLedgerOperatingSystemCodes.FoodDelivery, request.대상OsCode);
        Assert.Equal(커뮤니티원장상태.진행중, request.상태);
        Assert.Equal("FOOD-100", request.외부참조["음식주문번호"]);
        Assert.Contains(request.블록목록, block => block.BlockId == "food-preparation" && block.BlockType == CommunityLedgerBlockTypes.State);
        Assert.DoesNotContain(request.블록목록, block => block.BlockId == "delivery-handoff");
        Assert.DoesNotContain(request.블록목록, block => block.BlockId == "recipient");
        Assert.NotNull(request.다이어그램스냅샷);
        Assert.Equal(CommunityLedgerTemplateKeys.FoodOrder, request.다이어그램스냅샷!.LedgerTemplateKey);
        Assert.DoesNotContain(request.다이어그램스냅샷.Nodes, node => node.NodeId == "delivery-handoff");
    }

    [Fact]
    public void Food_order_builder_keeps_order_complete_when_delivery_progresses()
    {
        var order = new 음식주문응답
        {
            주문번호 = "FOOD-READY-1",
            음식점Id = 10,
            주문자UserId = "orderer-1",
            수령인정보 = new 음식주문수령인정보Dto(),
            상태 = 음식주문상태코드.기사배정,
            배차상태 = 음식주문배차상태코드.기사배정,
            CreatedAt = DateTime.UtcNow
        };

        var request = 음식마트원장Mongo동기화Builder.음식주문저장요청생성(order);

        Assert.Equal(커뮤니티원장상태.완료, request.상태);
        Assert.Equal(음식주문상태코드.픽업대기, request.현재단계Key);
        Assert.False(request.외부참조.ContainsKey("배차대기Id"));
    }

    [Fact]
    public void Outbound_builder_classifies_mart_reference_as_hongdal_mart_ledger()
    {
        var outbound = CreateOutbound("MART-ORDER-1");
        var inbound = CreateInbound("MART-ORDER-1", outbound.Id);

        var request = 음식마트원장Mongo동기화Builder.출고원장저장요청생성(
            [outbound],
            [inbound],
            "피킹 시작");

        Assert.Equal("hongdal-mart:MART-ORDER-1", request.원장Id);
        Assert.Equal(CommunityLedgerTemplateKeys.HongdalMart, request.원장템플릿Key);
        Assert.Equal(CommunityLedgerOperatingSystemCodes.HongdalMartUrbanLogistics, request.대상OsCode);
        Assert.Equal("피킹 시작", request.현재단계Key);
        Assert.Equal("MART-ORDER-1", request.외부참조["주문참조번호"]);
        Assert.Equal("HongdalMartOrder", request.외부참조["원천유형"]);
        Assert.Contains(request.블록목록, block => block.BlockId == "mart-order");
        Assert.Contains(request.블록목록, block => block.BlockId == "urban-inventory");
    }

    [Fact]
    public void Outbound_builder_keeps_plain_reference_as_warehouse_outbound_ledger()
    {
        var outbound = CreateOutbound("ORDER-200");

        var request = 음식마트원장Mongo동기화Builder.출고원장저장요청생성(
            [outbound],
            [],
            "출고 예정");

        Assert.Equal("warehouse-outbound:ORDER-200", request.원장Id);
        Assert.Equal(CommunityLedgerTemplateKeys.WarehouseOutbound, request.원장템플릿Key);
        Assert.Equal(CommunityLedgerOperatingSystemCodes.WarehouseCommerceFulfillment, request.대상OsCode);
        Assert.Equal("WarehouseOutboundPlanned", request.외부참조["원천유형"]);
        Assert.Contains(request.블록목록, block => block.BlockId == "outbound-order");
        Assert.DoesNotContain(request.블록목록, block => block.BlockId == "mart-order");
    }

    private static 출고예정 CreateOutbound(string orderReference)
        => new()
        {
            Id = 55,
            주문Id = 77,
            주문참조번호 = orderReference,
            판매자UserId = "seller-1",
            주문자UserId = "orderer-1",
            출고창고Id = 3,
            상품명 = "생활용품",
            SKU = "SKU-1",
            수량 = 2,
            상태 = 출고상태.예정,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static 입고요청 CreateInbound(string orderReference, long outboundId)
        => new()
        {
            Id = 91,
            창고Id = 4,
            주문참조번호 = orderReference,
            주문자UserId = "orderer-1",
            판매자UserId = "seller-1",
            출고예정Id = outboundId,
            공급처명 = "seller-1",
            원주문참조번호 = orderReference,
            상태 = 입고상태.예정,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
}
