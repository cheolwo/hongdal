using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using 살뜰.도메인.공통;
using 살뜰.도메인.운송;
using 살뜰.도메인.화주;
using 살뜰.Services.Dispatch.Engine;

namespace Ssalddel.Tests.Services.Community;

public sealed class 운송원장Mongo동기화BuilderTests
{
    [Fact]
    public void Builder_maps_transport_request_and_projection_to_mongo_ledger_request()
    {
        var request = CreateRequest();
        var projection = CreateProjection(request.의뢰Id);

        var saveRequest = 운송원장Mongo동기화Builder.저장요청생성(request, projection);

        Assert.Equal("transport:REQ-100", saveRequest.원장Id);
        Assert.Equal(CommunityLedgerTemplateKeys.CargoTransport, saveRequest.원장템플릿Key);
        Assert.Equal(CommunityLedgerOperatingSystemCodes.DomesticCargoTransport, saveRequest.대상OsCode);
        Assert.Equal(커뮤니티원장상태.진행중, saveRequest.상태);
        Assert.Equal("상차완료", saveRequest.현재단계Key);
        Assert.Equal("운송실행투영", saveRequest.외부참조["RdbTransportProjectionTable"]);
        Assert.Equal("99", saveRequest.외부참조["운송실행투영Id"]);
        Assert.Equal("MongoDB", saveRequest.확장속성["원장원본저장소"]);

        Assert.Collection(
            saveRequest.블록목록,
            block => AssertBlock(block, "transport-request", CommunityLedgerBlockTypes.Order),
            block => AssertBlock(block, "pickup", CommunityLedgerBlockTypes.Place),
            block => AssertBlock(block, "dropoff", CommunityLedgerBlockTypes.Place),
            block => AssertBlock(block, "settlement", CommunityLedgerBlockTypes.Settlement));

        Assert.NotNull(saveRequest.다이어그램스냅샷);
        Assert.Equal(4, saveRequest.다이어그램스냅샷!.Nodes.Count);
        Assert.Contains(saveRequest.다이어그램스냅샷.Edges, edge =>
            edge.FromNodeId == "dropoff"
            && edge.ToNodeId == "settlement"
            && edge.Data.TryGetValue("관계유형", out var relationType)
            && relationType == CommunityLedgerRelationTypes.Requires);
    }

    [Fact]
    public void Builder_uses_the_ledger_linked_to_the_transport_projection()
    {
        var projection = CreateProjection("REQ-LINKED");
        projection.커뮤니티원장Id = "community-ledger-linked";

        var saveRequest = 운송원장Mongo동기화Builder.저장요청생성(null, projection);

        Assert.Equal("community-ledger-linked", saveRequest.원장Id);
        Assert.Equal("community-ledger-linked", saveRequest.외부참조["커뮤니티원장Id"]);
        Assert.Contains("/arrive-pickup", saveRequest.블록목록
            .Single(block => block.BlockId == "pickup")
            .Data["상태변경Api"]);
    }

    [Fact]
    public void Builder_preserves_complete_transport_request_node_input_for_rdb_roundtrip()
    {
        var request = CreateRequest();
        var saveRequest = 운송원장Mongo동기화Builder.저장요청생성(request, null);
        var ledger = new 커뮤니티원장Dto
        {
            원장Id = saveRequest.원장Id!,
            커뮤니티Id = saveRequest.커뮤니티Id,
            원장템플릿Key = saveRequest.원장템플릿Key,
            상태 = saveRequest.상태 ?? 커뮤니티원장상태.초안,
            현재단계Key = saveRequest.현재단계Key,
            블록목록 = saveRequest.블록목록,
            참여자목록 = saveRequest.참여자목록,
            외부참조 = saveRequest.외부참조
        };

        var snapshot = 운송원장업무투영Snapshot.생성(ledger);

        Assert.NotNull(snapshot);
        Assert.True(snapshot!.IsTransportRequestComplete);
        Assert.Empty(snapshot.MissingRequiredFields);
        Assert.Equal(3, snapshot.CargoQuantity);
        Assert.Equal(1200, snapshot.CargoLengthMm);
        Assert.Equal(350.5m, snapshot.CargoWeightKg);
        Assert.Equal(2, snapshot.PalletCount);
        Assert.True(snapshot.CargoFragile);
        Assert.Equal("냉장", snapshot.CargoTemperature);
        Assert.Equal("상차담당", snapshot.PickupContactName);
        Assert.Equal(new DateTime(2026, 7, 13, 9, 0, 0, DateTimeKind.Utc), snapshot.PickupWindowStart);
        Assert.Equal("FakePG", snapshot.PaymentMethod);
        Assert.Equal(55000, snapshot.EstimatedPaymentAmount);
        Assert.Equal(5000m, snapshot.WaitingFee);
    }

    [Fact]
    public void Snapshot_blocks_rdb_projection_when_transport_request_node_is_incomplete()
    {
        var ledger = new 커뮤니티원장Dto
        {
            원장Id = "transport:REQ-INCOMPLETE",
            원장템플릿Key = CommunityLedgerTemplateKeys.CargoTransport,
            블록목록 =
            [
                new()
                {
                    BlockId = "transport-request",
                    BlockType = CommunityLedgerBlockTypes.Order,
                    Title = "운송 의뢰",
                    Data = new Dictionary<string, string>
                    {
                        ["의뢰Id"] = "REQ-INCOMPLETE",
                        ["화주Id"] = "shipper-1"
                    }
                }
            ]
        };

        var snapshot = 운송원장업무투영Snapshot.생성(ledger);

        Assert.NotNull(snapshot);
        Assert.False(snapshot!.IsTransportRequestComplete);
        Assert.Contains("화물종류", snapshot.MissingRequiredFields);
        Assert.Contains("상차지", snapshot.MissingRequiredFields);
        Assert.Contains("하차지", snapshot.MissingRequiredFields);
    }

    [Fact]
    public void Builder_keeps_food_order_source_metadata_on_delivery_ledger()
    {
        var projection = CreateProjection("FOOD-100");
        projection.배차업무유형 = 상태값.배차업무유형.음식배달;
        projection.원본의뢰유형 = 운송의뢰배차원천유형.음식점주문;
        projection.원본의뢰Id = "ORDER-FOOD-100";

        var saveRequest = 운송원장Mongo동기화Builder.저장요청생성(null, projection);

        Assert.Equal(CommunityLedgerTemplateKeys.FoodDelivery, saveRequest.원장템플릿Key);
        Assert.Equal(CommunityLedgerOperatingSystemCodes.FoodDelivery, saveRequest.대상OsCode);
        Assert.Equal(CommunityLedgerTemplateKeys.FoodDelivery, saveRequest.다이어그램스냅샷!.LedgerTemplateKey);
        Assert.Equal(운송의뢰배차원천유형.음식점주문, saveRequest.외부참조["원천유형"]);
        Assert.Equal("ORDER-FOOD-100", saveRequest.외부참조["원천Id"]);
        Assert.Equal(CommunityLedgerTemplateKeys.FoodOrder, saveRequest.외부참조["원천원장템플릿Key"]);
        Assert.Equal(CommunityLedgerOperatingSystemCodes.FoodDelivery, saveRequest.외부참조["원천OS"]);
    }

    [Fact]
    public void Builder_keeps_warehouse_outbound_source_metadata_on_transport_ledger()
    {
        var projection = CreateProjection("WH-OUT-100");
        projection.원본의뢰유형 = 운송의뢰배차원천유형.창고출고연계운송;
        projection.원본의뢰Id = "INBOUND-ITEM-55";

        var saveRequest = 운송원장Mongo동기화Builder.저장요청생성(null, projection);

        Assert.Equal(CommunityLedgerTemplateKeys.CargoTransport, saveRequest.원장템플릿Key);
        Assert.Equal(CommunityLedgerOperatingSystemCodes.DomesticCargoTransport, saveRequest.대상OsCode);
        Assert.Equal(운송의뢰배차원천유형.창고출고연계운송, saveRequest.외부참조["원천유형"]);
        Assert.Equal("INBOUND-ITEM-55", saveRequest.외부참조["원천Id"]);
        Assert.Equal(CommunityLedgerTemplateKeys.WarehouseOutbound, saveRequest.외부참조["원천원장템플릿Key"]);
        Assert.Equal(CommunityLedgerOperatingSystemCodes.WarehouseCommerceFulfillment, saveRequest.외부참조["원천OS"]);
    }

    private static 화주운송의뢰 CreateRequest()
        => new()
        {
            Id = 7,
            의뢰Id = "REQ-100",
            화주Id = "shipper-1",
            주문자UserId = "orderer-1",
            화물종류 = "생활용품",
            화물설명 = "박스 3개",
            화물수량 = 3,
            화물길이Mm = 1200,
            화물폭Mm = 800,
            화물높이Mm = 900,
            화물팔레트개수 = 2,
            화물중량Kg = 350.5m,
            화물부피Cbm = 1.25m,
            화물파손주의여부 = true,
            화물온도조건 = "냉장",
            운송방식 = "혼적",
            차량종류 = "다마스",
            결제수단 = "FakePG",
            정산시점 = "운송완료후정산",
            수납주체 = "화주",
            정산상태 = "청구대기",
            결제상태 = 상태값.결제상태.결제완료,
            배차상태 = 상태값.배차상태.배차확정,
            픽업_도로명주소 = "서울 중구 세종대로",
            픽업_상세주소 = "1층",
            픽업_연락처_이름 = "상차담당",
            픽업_연락처_전화번호 = "010-0000-0001",
            픽업_시간창_시작일시 = new DateTime(2026, 7, 13, 9, 0, 0, DateTimeKind.Utc),
            픽업_시간창_종료일시 = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc),
            하차_도로명주소 = "서울 용산구 한강대로",
            하차_상세주소 = "2층",
            하차_연락처_이름 = "하차담당",
            하차_연락처_전화번호 = "010-0000-0002",
            하차_시간창_시작일시 = new DateTime(2026, 7, 13, 11, 0, 0, DateTimeKind.Utc),
            하차_시간창_종료일시 = new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc),
            결제예정금액 = 55000,
            대기료 = 5000m,
            수작업비 = 3000m,
            할증 = 2000m,
            최종운임 = 55000m,
            상태 = 상태값.의뢰상태.생성됨
        };

    private static 운송원장 CreateProjection(string requestId)
        => new()
        {
            Id = 99,
            운송번호 = requestId,
            의뢰Id = requestId,
            화주Id = "shipper-1",
            상태 = "상차완료",
            기사_운송자 = "driver-1",
            확정기사Id = "driver-1",
            출발지 = "서울 중구 세종대로",
            도착지 = "서울 용산구 한강대로",
            운임 = 55000m
        };

    private static void AssertBlock(커뮤니티원장블록Dto block, string blockId, string blockType)
    {
        Assert.Equal(blockId, block.BlockId);
        Assert.Equal(blockType, block.BlockType);
    }
}
