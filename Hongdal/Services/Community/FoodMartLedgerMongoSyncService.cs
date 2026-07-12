using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Food;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using 홍달.Data;
using 홍달.도메인.마트;
using 홍달.도메인.창고;

namespace Hongdal.Services.Community;

public interface I음식마트원장Mongo동기화Service
{
    Task<커뮤니티원장Dto?> 음식주문동기화Async(
        음식주문응답 주문,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<커뮤니티원장Dto?> 출고원장동기화Async(
        IReadOnlyList<출고예정> 출고목록,
        IReadOnlyList<입고요청> 입고목록,
        string updatedBy,
        string? 현재단계Key = null,
        string? 원장템플릿Key = null,
        CancellationToken cancellationToken = default);
}

public sealed class 음식마트원장Mongo동기화Service : I음식마트원장Mongo동기화Service
{
    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly HongdalContext _db;
    private readonly ILogger<음식마트원장Mongo동기화Service> _logger;

    public 음식마트원장Mongo동기화Service(
        I커뮤니티원장저장소 원장저장소,
        HongdalContext db,
        ILogger<음식마트원장Mongo동기화Service> logger)
    {
        _원장저장소 = 원장저장소;
        _db = db;
        _logger = logger;
    }

    public async Task<커뮤니티원장Dto?> 음식주문동기화Async(
        음식주문응답 주문,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = 음식마트원장Mongo동기화Builder.음식주문저장요청생성(주문);
            return await _원장저장소.원장저장Async(request, updatedBy, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "음식 주문 원장 Mongo 동기화에 실패했습니다. 주문번호={주문번호}", 주문.주문번호);
            return null;
        }
    }

    public async Task<커뮤니티원장Dto?> 출고원장동기화Async(
        IReadOnlyList<출고예정> 출고목록,
        IReadOnlyList<입고요청> 입고목록,
        string updatedBy,
        string? 현재단계Key = null,
        string? 원장템플릿Key = null,
        CancellationToken cancellationToken = default)
    {
        if (출고목록.Count == 0)
        {
            return null;
        }

        try
        {
            var request = 음식마트원장Mongo동기화Builder.출고원장저장요청생성(
                출고목록,
                입고목록,
                현재단계Key,
                원장템플릿Key);
            var ledger = await _원장저장소.원장저장Async(request, updatedBy, cancellationToken);
            if (ledger is not null
                && string.Equals(request.원장템플릿Key, CommunityLedgerTemplateKeys.HongdalMart, StringComparison.OrdinalIgnoreCase))
            {
                await 마트주문투영Async(request, ledger, 출고목록, cancellationToken);
            }

            return ledger;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            var orderRef = 출고목록.Select(x => x.주문참조번호).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            _logger.LogWarning(ex, "마트/창고 출고 원장 Mongo 동기화에 실패했습니다. 주문참조번호={주문참조번호}", orderRef);
            return null;
        }
    }

    private async Task 마트주문투영Async(
        커뮤니티원장저장요청 request,
        커뮤니티원장Dto ledger,
        IReadOnlyList<출고예정> 출고목록,
        CancellationToken cancellationToken)
    {
        var 주문참조번호 = TryGet(request.외부참조, "주문참조번호", "주문번호", "원천Id")
                         ?? 출고목록.Select(x => Clean(x.주문참조번호)).FirstOrDefault(x => x is not null)
                         ?? Clean(ledger.원장Id)
                         ?? throw new InvalidOperationException("마트 주문 projection을 만들려면 주문참조번호가 필요합니다.");
        var first = 출고목록[0];
        var now = DateTime.UtcNow;
        var 상태 = Clean(request.현재단계Key) ?? Clean(ledger.상태) ?? "출고 예정";
        var order = await _db.마트주문
            .Include(x => x.상품목록)
            .FirstOrDefaultAsync(x => x.주문참조번호 == 주문참조번호, cancellationToken);

        if (order is null)
        {
            order = new 마트주문
            {
                주문참조번호 = 주문참조번호,
                CreatedAt = now
            };
            _db.마트주문.Add(order);
        }

        order.주문Id = 출고목록.Select(x => x.주문Id).FirstOrDefault(x => x.HasValue);
        order.주문자UserId = Clean(first.주문자UserId) ?? order.주문자UserId;
        order.판매자UserId = Clean(first.판매자UserId) ?? order.판매자UserId;
        order.상태 = 상태;
        order.현재단계 = Clean(request.현재단계Key);
        order.커뮤니티원장Id = Clean(ledger.원장Id);
        order.커뮤니티원장템플릿Key = Clean(ledger.원장템플릿Key);
        order.커뮤니티원장상태 = Clean(ledger.상태);
        order.커뮤니티원장동기화시각Utc = now;
        order.UpdatedAt = now;

        foreach (var outbound in 출고목록)
        {
            var item = order.상품목록.FirstOrDefault(x => x.출고예정Id == outbound.Id);
            if (item is null)
            {
                item = new 마트주문상품
                {
                    출고예정Id = outbound.Id,
                    CreatedAt = now
                };
                order.상품목록.Add(item);
            }

            item.상품명 = Clean(outbound.상품명) ?? item.상품명;
            item.SKU = Clean(outbound.SKU) ?? item.SKU;
            item.수량 = outbound.수량;
            item.상태 = Clean(outbound.상태) ?? 상태;
            item.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string? TryGet(IReadOnlyDictionary<string, string>? data, params string[] keys)
    {
        if (data is null)
        {
            return null;
        }

        foreach (var key in keys)
        {
            foreach (var item in data)
            {
                if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return Clean(item.Value);
                }
            }
        }

        return null;
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public static class 음식마트원장Mongo동기화Builder
{
    public static string 음식주문원장Id생성(string 주문번호)
        => $"food-order:{주문번호.Trim()}";

    public static string 출고원장Id생성(string 주문참조번호, string 원장템플릿Key)
        => string.Equals(원장템플릿Key, CommunityLedgerTemplateKeys.HongdalMart, StringComparison.OrdinalIgnoreCase)
            ? $"hongdal-mart:{주문참조번호.Trim()}"
            : $"warehouse-outbound:{주문참조번호.Trim()}";

    public static 커뮤니티원장저장요청 음식주문저장요청생성(음식주문응답 주문)
    {
        if (string.IsNullOrWhiteSpace(주문.주문번호))
        {
            throw new InvalidOperationException("음식 주문 원장을 만들려면 주문번호가 필요합니다.");
        }

        var 원장Id = 음식주문원장Id생성(주문.주문번호);
        var 현재단계 = ResolveFoodStage(주문);

        return new 커뮤니티원장저장요청
        {
            원장Id = 원장Id,
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.FoodOrder,
            제목 = $"음식 주문 원장 {주문.주문번호}",
            원함 = BuildFoodWish(주문),
            상태 = ResolveFoodLedgerState(주문),
            현재단계Key = 현재단계,
            대상OsCode = CommunityLedgerOperatingSystemCodes.FoodDelivery,
            대상OsName = "음식 배달 OS",
            생성자UserId = 주문.주문자UserId,
            생성자표시명 = "음식 주문자",
            블록목록 = BuildFoodBlocks(주문),
            참여자목록 = BuildFoodParticipants(주문),
            다이어그램스냅샷 = BuildFoodDiagram(원장Id),
            외부참조 = Data(
                ("음식주문번호", 주문.주문번호),
                ("주문번호", 주문.주문번호),
                ("음식점Id", Format(주문.음식점Id)),
                ("배차대기Id", Format(주문.배차대기Id)),
                ("원천유형", "FoodOrder"),
                ("원천Id", 주문.주문번호),
                ("RdbFoodProjectionType", "음식주문")),
            확장속성 = Data(
                ("원장원본저장소", "MongoDB"),
                ("동기화정책", "Mongo 음식 주문 원장을 원본으로 보고 음식 주문 저장소는 조회 투영으로 갱신합니다."),
                ("입력Api", "POST api/v1/food-orders"),
                ("수락Api", "POST api/v1/food-orders/{orderNo}/restaurant-accept"))
        };
    }

    public static 커뮤니티원장저장요청 출고원장저장요청생성(
        IReadOnlyList<출고예정> 출고목록,
        IReadOnlyList<입고요청> 입고목록,
        string? 현재단계Key = null,
        string? 원장템플릿Key = null)
    {
        if (출고목록.Count == 0)
        {
            throw new InvalidOperationException("출고 원장을 만들려면 출고예정 목록이 필요합니다.");
        }

        var 주문참조번호 = FirstNonEmpty(출고목록.Select(x => x.주문참조번호).ToArray())
                         ?? 출고목록[0].Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var templateKey = ResolveOutboundTemplateKey(주문참조번호, 원장템플릿Key);
        var 원장Id = 출고원장Id생성(주문참조번호, templateKey);
        var stage = FirstNonEmpty(현재단계Key, ResolveOutboundStage(출고목록), ResolveInboundStage(입고목록));
        var isMart = string.Equals(templateKey, CommunityLedgerTemplateKeys.HongdalMart, StringComparison.OrdinalIgnoreCase);

        return new 커뮤니티원장저장요청
        {
            원장Id = 원장Id,
            커뮤니티Id = "platform",
            원장템플릿Key = templateKey,
            제목 = isMart ? $"알뜰살뜰 마트 배송 원장 {주문참조번호}" : $"창고 출고 원장 {주문참조번호}",
            원함 = isMart
                ? "도심 재고를 피킹/포장해 주문자에게 전달하고 싶습니다."
                : "창고 출고 품목을 준비하고 운송 인계까지 정리하고 싶습니다.",
            상태 = ResolveOutboundLedgerState(출고목록),
            현재단계Key = stage,
            대상OsCode = isMart
                ? CommunityLedgerOperatingSystemCodes.HongdalMartUrbanLogistics
                : CommunityLedgerOperatingSystemCodes.WarehouseCommerceFulfillment,
            대상OsName = isMart ? "알뜰살뜰 마트 도심 물류 OS" : "창고·커머스 이행 OS",
            생성자UserId = FirstNonEmpty(출고목록.Select(x => x.주문자UserId).ToArray()),
            생성자표시명 = isMart ? "마트 주문자" : "출고 요청자",
            블록목록 = BuildOutboundBlocks(출고목록, 입고목록, isMart, stage),
            참여자목록 = BuildOutboundParticipants(출고목록),
            다이어그램스냅샷 = BuildOutboundDiagram(원장Id, templateKey, isMart),
            외부참조 = Data(
                ("주문참조번호", 주문참조번호),
                ("주문번호", 주문참조번호),
                ("출고예정Id", 출고목록.Count == 1 ? Format(출고목록[0].Id) : null),
                ("출고예정Ids", JoinIds(출고목록.Select(x => x.Id))),
                ("입고요청Id", 입고목록.Count == 1 ? Format(입고목록[0].Id) : null),
                ("입고요청Ids", JoinIds(입고목록.Select(x => x.Id))),
                ("운송의뢰Id", FirstNonEmpty(출고목록.Select(x => x.운송의뢰Id).ToArray())),
                ("원천유형", isMart ? "HongdalMartOrder" : "WarehouseOutboundPlanned"),
                ("원천Id", 주문참조번호),
                ("RdbWarehouseProjectionTable", "출고예정"),
                ("RdbWarehouseProjectionType", "출고예정")),
            확장속성 = Data(
                ("원장원본저장소", "MongoDB"),
                ("동기화정책", "Mongo 출고 원장을 원본으로 보고 RDB 창고 출고/입고 데이터는 작업 조회를 위한 투영으로 갱신합니다."),
                ("분류정책", "주문참조번호나 명시 템플릿이 마트 흐름이면 hongdal-mart, 아니면 warehouse-outbound로 분류합니다."),
                ("입력Api", "POST api/v1/hongdal-mart/orders 또는 내부 주문결제완료됨Event"),
                ("포장Api", "POST api/v1/warehouse-operations/inventory-items/{id}/pack"),
                ("운송인계Api", "POST api/v1/warehouse-operations/reconsignment-requests"))
        };
    }

    private static IReadOnlyList<커뮤니티원장블록Dto> BuildFoodBlocks(음식주문응답 주문)
        =>
        [
            new()
            {
                BlockId = "food-order",
                BlockType = CommunityLedgerBlockTypes.Order,
                Title = "음식 주문",
                State = 주문.상태,
                Data = Data(
                    ("업무엔티티", "음식주문"),
                    ("주문번호", 주문.주문번호),
                    ("음식점Id", Format(주문.음식점Id)),
                    ("주문자UserId", 주문.주문자UserId),
                    ("총주문금액", Format(주문.총주문금액)),
                    ("메뉴요약", FoodOrderSampleData.BuildMenuSummary(주문.상품목록)),
                    ("입력Api", "POST api/v1/food-orders"))
            },
            new()
            {
                BlockId = "restaurant",
                BlockType = CommunityLedgerBlockTypes.Place,
                Title = "음식점",
                State = 주문.음식점수락시각Utc is null ? "수락 전" : "수락 완료",
                Data = Data(
                    ("업무엔티티", "음식점"),
                    ("음식점Id", Format(주문.음식점Id)),
                    ("음식점명", 주문.음식점명),
                    ("주소", 주문.음식점주소),
                    ("상세주소", 주문.음식점상세주소),
                    ("위도", Format(주문.음식점위도)),
                    ("경도", Format(주문.음식점경도)),
                    ("조리예상완료시각Utc", Format(주문.조리예상완료시각Utc)),
                    ("수락Api", "POST api/v1/food-orders/{orderNo}/restaurant-accept"))
            },
            new()
            {
                BlockId = "recipient",
                BlockType = CommunityLedgerBlockTypes.Place,
                Title = "수령지",
                State = 주문.수령인정보.주문자본인수령여부 ? "본인 수령" : "대리 수령",
                Data = Data(
                    ("업무엔티티", "음식주문.수령지"),
                    ("수령인명", 주문.수령인정보.수령인명),
                    ("주소", 주문.수령인정보.주소),
                    ("상세주소", 주문.수령인정보.상세주소),
                    ("요청사항", 주문.수령인정보.요청사항))
            },
            new()
            {
                BlockId = "delivery-handoff",
                BlockType = CommunityLedgerBlockTypes.Handoff,
                Title = "배달 인계",
                State = 주문.배차상태,
                Data = Data(
                    ("업무엔티티", "음식배달.배차"),
                    ("배차상태", 주문.배차상태),
                    ("배차대기Id", Format(주문.배차대기Id)),
                    ("배차요청시각Utc", Format(주문.배차요청시각Utc)),
                    ("연결원장템플릿Key", CommunityLedgerTemplateKeys.FoodDelivery))
            },
            new()
            {
                BlockId = "food-settlement",
                BlockType = CommunityLedgerBlockTypes.Settlement,
                Title = "결제 표시",
                State = 주문.결제수단,
                Data = Data(
                    ("업무엔티티", "음식주문.결제표시"),
                    ("결제수단", 주문.결제수단),
                    ("총주문금액", Format(주문.총주문금액)))
            }
        ];

    private static IReadOnlyList<커뮤니티원장참여자Dto> BuildFoodParticipants(음식주문응답 주문)
    {
        var participants = new List<커뮤니티원장참여자Dto>();
        AddParticipant(participants, 주문.주문자UserId, "주문자", "주문자");
        AddParticipant(participants, $"restaurant:{주문.음식점Id}", string.IsNullOrWhiteSpace(주문.음식점명) ? "음식점" : 주문.음식점명, "음식점");
        AddParticipant(participants, null, string.IsNullOrWhiteSpace(주문.수령인정보.수령인명) ? "수령자" : 주문.수령인정보.수령인명, "수령 확인자");
        return participants;
    }

    private static IReadOnlyList<커뮤니티원장블록Dto> BuildOutboundBlocks(
        IReadOnlyList<출고예정> 출고목록,
        IReadOnlyList<입고요청> 입고목록,
        bool isMart,
        string? stage)
    {
        var first = 출고목록[0];
        var 주문참조번호 = FirstNonEmpty(출고목록.Select(x => x.주문참조번호).ToArray()) ?? first.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lineSummary = string.Join(", ", 출고목록.Select(x => $"{x.상품명} {x.수량}"));

        return
        [
            new()
            {
                BlockId = isMart ? "mart-order" : "outbound-order",
                BlockType = CommunityLedgerBlockTypes.Order,
                Title = isMart ? "마트 주문" : "출고 요청",
                State = ResolveOutboundStage(출고목록),
                Data = Data(
                    ("업무엔티티", isMart ? "HongdalMartOrder" : "출고예정"),
                    ("주문참조번호", 주문참조번호),
                    ("출고예정Ids", JoinIds(출고목록.Select(x => x.Id))),
                    ("판매자UserId", first.판매자UserId),
                    ("주문자UserId", first.주문자UserId),
                    ("상품요약", lineSummary),
                    ("라인수", Format(출고목록.Count)))
            },
            new()
            {
                BlockId = isMart ? "urban-inventory" : "warehouse-inventory",
                BlockType = CommunityLedgerBlockTypes.Inventory,
                Title = isMart ? "도심 재고" : "재고 근거",
                State = 입고목록.Count == 0 ? "입고 근거 미연결" : ResolveInboundStage(입고목록),
                Data = Data(
                    ("업무엔티티", "입고요청/입고상품"),
                    ("입고요청Ids", JoinIds(입고목록.Select(x => x.Id))),
                    ("출고창고Id", Format(first.출고창고Id)),
                    ("입고요청수", Format(입고목록.Count)))
            },
            new()
            {
                BlockId = "picking-packing",
                BlockType = CommunityLedgerBlockTypes.State,
                Title = "피킹/포장",
                State = stage,
                Data = Data(
                    ("업무엔티티", "출고묶음/피킹포장"),
                    ("상태", stage),
                    ("포장Api", "POST api/v1/warehouse-operations/inventory-items/{id}/pack"))
            },
            new()
            {
                BlockId = isMart ? "mart-delivery" : "transport-handoff",
                BlockType = CommunityLedgerBlockTypes.Handoff,
                Title = isMart ? "기사 픽업" : "운송 인계",
                State = FirstNonEmpty(출고목록.Select(x => x.운송의뢰Id).ToArray()) is null ? "운송 인계 전" : "운송 인계됨",
                Data = Data(
                    ("업무엔티티", "운송인계"),
                    ("운송의뢰Id", FirstNonEmpty(출고목록.Select(x => x.운송의뢰Id).ToArray())),
                    ("연결원장템플릿Key", isMart ? CommunityLedgerTemplateKeys.HongdalMart : CommunityLedgerTemplateKeys.CargoTransport))
            }
        ];
    }

    private static IReadOnlyList<커뮤니티원장참여자Dto> BuildOutboundParticipants(IReadOnlyList<출고예정> 출고목록)
    {
        var first = 출고목록[0];
        var participants = new List<커뮤니티원장참여자Dto>();
        AddParticipant(participants, first.주문자UserId, "주문자", "주문자");
        AddParticipant(participants, first.판매자UserId, "판매자", "판매자");
        AddParticipant(participants, null, "창고 작업자", "피킹/포장 담당자");
        return participants;
    }

    private static DiagramSnapshotDto BuildFoodDiagram(string 원장Id)
        => new()
        {
            DiagramId = $"{원장Id}:diagram",
            DiagramName = "음식 주문 원장 흐름",
            LedgerId = 원장Id,
            LedgerTemplateKey = CommunityLedgerTemplateKeys.FoodOrder,
            WorkflowModeKey = "food-order",
            Nodes =
            [
                Node("food-order", "음식 주문", CommunityLedgerBlockTypes.Order, "주문", 120, 160),
                Node("restaurant", "음식점", CommunityLedgerBlockTypes.Place, "조리", 360, 160),
                Node("delivery-handoff", "배달 인계", CommunityLedgerBlockTypes.Handoff, "배달", 600, 160),
                Node("recipient", "수령지", CommunityLedgerBlockTypes.Place, "수령", 840, 160),
                Node("food-settlement", "결제 표시", CommunityLedgerBlockTypes.Settlement, "결제", 600, 320)
            ],
            Edges =
            [
                Edge("edge-food-restaurant", "food-order", "restaurant", "주문 수락", CommunityLedgerRelationTypes.Flow),
                Edge("edge-restaurant-delivery", "restaurant", "delivery-handoff", "조리 후 배달", CommunityLedgerRelationTypes.Handoff),
                Edge("edge-delivery-recipient", "delivery-handoff", "recipient", "전달", CommunityLedgerRelationTypes.Flow),
                Edge("edge-order-settlement", "food-order", "food-settlement", "결제 표시", CommunityLedgerRelationTypes.Reference)
            ],
            Metadata = Data(
                ("Source", "FoodMartLedgerMongoSync"),
                ("PrimaryStore", "MongoDB community_ledgers"))
        };

    private static DiagramSnapshotDto BuildOutboundDiagram(string 원장Id, string templateKey, bool isMart)
        => new()
        {
            DiagramId = $"{원장Id}:diagram",
            DiagramName = isMart ? "알뜰살뜰 마트 배송 원장 흐름" : "창고 출고 원장 흐름",
            LedgerId = 원장Id,
            LedgerTemplateKey = templateKey,
            WorkflowModeKey = isMart ? "hongdal-mart" : "warehouse-outbound",
            Nodes =
            [
                Node(isMart ? "mart-order" : "outbound-order", isMart ? "마트 주문" : "출고 요청", CommunityLedgerBlockTypes.Order, "주문", 120, 160),
                Node(isMart ? "urban-inventory" : "warehouse-inventory", isMart ? "도심 재고" : "재고 근거", CommunityLedgerBlockTypes.Inventory, "재고", 360, 160),
                Node("picking-packing", "피킹/포장", CommunityLedgerBlockTypes.State, "작업", 600, 160),
                Node(isMart ? "mart-delivery" : "transport-handoff", isMart ? "기사 픽업" : "운송 인계", CommunityLedgerBlockTypes.Handoff, "인계", 840, 160)
            ],
            Edges =
            [
                Edge("edge-order-inventory", isMart ? "mart-order" : "outbound-order", isMart ? "urban-inventory" : "warehouse-inventory", "재고 확인", CommunityLedgerRelationTypes.Requires),
                Edge("edge-inventory-work", isMart ? "urban-inventory" : "warehouse-inventory", "picking-packing", "피킹/포장", CommunityLedgerRelationTypes.Flow),
                Edge("edge-work-delivery", "picking-packing", isMart ? "mart-delivery" : "transport-handoff", "포장 후 인계", CommunityLedgerRelationTypes.Handoff)
            ],
            Metadata = Data(
                ("Source", "FoodMartLedgerMongoSync"),
                ("PrimaryStore", "MongoDB community_ledgers"))
        };

    private static string ResolveFoodLedgerState(음식주문응답 주문)
    {
        var state = 음식주문상태코드.Normalize(주문.상태);
        return state switch
        {
            음식주문상태코드.전달완료 => 커뮤니티원장상태.완료,
            음식주문상태코드.취소 => 커뮤니티원장상태.닫힘,
            _ => 커뮤니티원장상태.진행중
        };
    }

    private static string ResolveFoodStage(음식주문응답 주문)
        => !string.IsNullOrWhiteSpace(주문.배차상태) && 주문.배차상태 != 음식주문배차상태코드.미요청
            ? 주문.배차상태
            : 음식주문상태코드.Normalize(주문.상태);

    private static string BuildFoodWish(음식주문응답 주문)
    {
        var menu = FoodOrderSampleData.BuildMenuSummary(주문.상품목록);
        return string.IsNullOrWhiteSpace(menu)
            ? "음식 주문을 음식점, 배달, 수령 흐름으로 처리하고 싶습니다."
            : $"{menu} 주문을 음식점, 배달, 수령 흐름으로 처리하고 싶습니다.";
    }

    private static string ResolveOutboundTemplateKey(string 주문참조번호, string? 원장템플릿Key)
    {
        if (string.Equals(원장템플릿Key, CommunityLedgerTemplateKeys.HongdalMart, StringComparison.OrdinalIgnoreCase)
            || string.Equals(원장템플릿Key, CommunityLedgerTemplateKeys.WarehouseOutbound, StringComparison.OrdinalIgnoreCase))
        {
            return 원장템플릿Key!;
        }

        return ContainsAny(주문참조번호, "MART", "HONGDAL-MART", "홍달마트", "알뜰마트", "살뜰마트", "마트")
            ? CommunityLedgerTemplateKeys.HongdalMart
            : CommunityLedgerTemplateKeys.WarehouseOutbound;
    }

    private static string ResolveOutboundLedgerState(IReadOnlyList<출고예정> 출고목록)
    {
        if (출고목록.All(x => x.상태 == 출고상태.출고완료))
        {
            return 커뮤니티원장상태.완료;
        }

        if (출고목록.Any(x => x.상태 != 출고상태.예정))
        {
            return 커뮤니티원장상태.진행중;
        }

        return 커뮤니티원장상태.초안;
    }

    private static string? ResolveOutboundStage(IReadOnlyList<출고예정> 출고목록)
    {
        if (출고목록.Count == 0)
        {
            return null;
        }

        if (출고목록.All(x => x.상태 == 출고상태.출고완료))
        {
            return "출고 완료";
        }

        if (출고목록.Any(x => x.상태 == 출고상태.준비중))
        {
            return "출고 준비중";
        }

        return "출고 예정";
    }

    private static string? ResolveInboundStage(IReadOnlyList<입고요청> 입고목록)
    {
        if (입고목록.Count == 0)
        {
            return null;
        }

        if (입고목록.All(x => x.상태 == 입고상태.입고완료))
        {
            return "입고 완료";
        }

        if (입고목록.Any(x => x.상태 == 입고상태.운송중))
        {
            return "입고 운송중";
        }

        return "입고 예정";
    }

    private static void AddParticipant(List<커뮤니티원장참여자Dto> participants, string? userId, string displayName, string roleLabel)
    {
        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(displayName))
        {
            return;
        }

        participants.Add(new 커뮤니티원장참여자Dto
        {
            UserId = Clean(userId),
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "익명 참여자" : displayName.Trim(),
            RoleLabel = roleLabel,
            ParticipationState = "참여중"
        });
    }

    private static DiagramNodeDto Node(string nodeId, string title, string kind, string groupLabel, double x, double y)
        => new()
        {
            NodeId = nodeId,
            Kind = kind,
            Title = title,
            GroupLabel = groupLabel,
            X = x,
            Y = y,
            Data = Data(("원장블록Id", nodeId))
        };

    private static DiagramEdgeDto Edge(string edgeId, string from, string to, string label, string relationType)
        => new()
        {
            EdgeId = edgeId,
            FromNodeId = from,
            ToNodeId = to,
            Label = label,
            MeaningCode = relationType,
            Data = Data(("관계유형", relationType))
        };

    private static IReadOnlyDictionary<string, string> Data(params (string Key, string? Value)[] values)
        => values
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().Value!.Trim(), StringComparer.OrdinalIgnoreCase);

    private static string? FirstNonEmpty(params string?[] values)
        => values.Select(Clean).FirstOrDefault(value => value is not null);

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Format(long? value)
        => value?.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Format(long value)
        => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string? Format(int? value)
        => value?.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Format(int value)
        => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string? Format(decimal? value)
        => value?.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string? Format(DateTime? value)
        => value?.ToString("O", System.Globalization.CultureInfo.InvariantCulture);

    private static string? JoinIds(IEnumerable<long> values)
    {
        var ids = values.Distinct().Select(Format).ToArray();
        return ids.Length == 0 ? null : string.Join(",", ids);
    }

    private static bool ContainsAny(string? source, params string[] candidates)
    {
        var text = Clean(source);
        return text is not null && candidates.Any(candidate => text.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }
}
