using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.WebApp.Services;

public sealed class WebShipperWarehouseWorkspaceService : IWarehouseWorkspaceService
{
    private readonly List<창고요약응답> _warehouses =
    [
        new()
        {
            Id = 1,
            창고명 = "김포 물류 허브",
            소유자UserId = "shipper-web-demo",
            소유자유형 = "화주",
            창고유형 = "위탁창고",
            물류대행지분류 = LogisticsProxySiteTypes.MarketFulfillment,
            주소 = "경기 김포시 고촌읍 아라육로",
            담당자명 = "운영팀",
            연락처 = "031-000-1000",
            기본창고여부 = true,
            IsActive = true
        },
        new()
        {
            Id = 2,
            창고명 = "인천 통관 대행지",
            소유자UserId = "shipper-web-demo",
            소유자유형 = "화주",
            창고유형 = "해외입고",
            물류대행지분류 = LogisticsProxySiteTypes.OverseasCustomsAgency,
            주소 = "인천 중구 공항동로",
            담당자명 = "통관팀",
            연락처 = "032-000-2000",
            IsActive = true
        }
    ];

    private readonly List<입고요청항목응답> _inbounds =
    [
        new()
        {
            Id = 1001,
            창고Id = 1,
            입고흐름유형 = 입고흐름유형코드.계약기반입고,
            입고생성경로 = "판매 채널 발주",
            계약선행여부 = true,
            공급처명 = "부산 식품 파트너",
            원주문참조번호 = "PO-FOOD-260710",
            상태 = "입고예정",
            예정도착일 = DateTime.Today.AddDays(1),
            계약정보 = new 입고계약스냅샷
            {
                계약번호 = "CT-MF-001",
                계약유형 = 입고계약유형코드.마켓풀필먼트,
                계약상대방명 = "부산 식품 파트너",
                정산방식 = "월말정산",
                판매수수료율 = 7m,
                보관료일단가 = 180m
            }.Normalize()
        },
        new()
        {
            Id = 1002,
            창고Id = 2,
            입고흐름유형 = 입고흐름유형코드.주문자동입고예정,
            입고생성경로 = "수입 공동구매",
            자동생성여부 = true,
            공급처명 = "Qingdao Supply",
            원주문참조번호 = "IMP-CAMP-240",
            상태 = "입고완료",
            예정도착일 = DateTime.Today.AddDays(-1),
            입고완료일시 = DateTime.Now.AddHours(-6),
            계약정보 = new 입고계약스냅샷
            {
                계약번호 = "CT-IMP-009",
                계약유형 = 입고계약유형코드.수입통관풀필먼트,
                계약상대방명 = "Qingdao Supply",
                정산방식 = "건별정산",
                판매수수료율 = 11m,
                보관료일단가 = 260m
            }.Normalize()
        }
    ];

    private readonly List<재고항목응답> _inventory =
    [
        new()
        {
            입고상품Id = 5001,
            창고Id = 1,
            창고명 = "김포 물류 허브",
            소유자UserId = "shipper-web-demo",
            판매자UserId = "seller-food",
            상품명 = "냉장 간편식 세트",
            SKU = "FOOD-SET-001",
            옵션명 = "6팩",
            가용수량 = 42,
            예약수량 = 8,
            상태 = "보관중",
            보관위치 = "A-01-03",
            계약정보 = new 입고계약스냅샷
            {
                계약번호 = "CT-MF-001",
                계약유형 = 입고계약유형코드.마켓풀필먼트,
                계약상대방명 = "부산 식품 파트너"
            }.Normalize()
        },
        new()
        {
            입고상품Id = 5002,
            창고Id = 2,
            창고명 = "인천 통관 대행지",
            소유자UserId = "shipper-web-demo",
            판매자UserId = "seller-import",
            상품명 = "접이식 캠핑 테이블",
            SKU = "CAMP-TABLE-240",
            옵션명 = "블랙",
            가용수량 = 126,
            예약수량 = 18,
            상태 = "보관중",
            보관위치 = "B-04-01",
            계약정보 = new 입고계약스냅샷
            {
                계약번호 = "CT-IMP-009",
                계약유형 = 입고계약유형코드.수입통관풀필먼트,
                계약상대방명 = "Qingdao Supply"
            }.Normalize()
        }
    ];

    private long _nextWarehouseId = 3;
    private long _nextInboundId = 1003;
    private long _nextInventoryId = 5003;

    public Task<창고목록응답?> GetWarehousesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<창고목록응답?>(new() { Items = _warehouses.ToArray() });

    public Task<창고요약응답?> CreateWarehouseAsync(창고저장요청 payload, CancellationToken cancellationToken = default)
    {
        var warehouse = new 창고요약응답
        {
            Id = _nextWarehouseId++,
            창고명 = string.IsNullOrWhiteSpace(payload.창고명) ? "신규 화주 창고" : payload.창고명.Trim(),
            소유자UserId = "shipper-web-demo",
            소유자유형 = string.IsNullOrWhiteSpace(payload.소유자유형) ? "화주" : payload.소유자유형,
            창고유형 = string.IsNullOrWhiteSpace(payload.창고유형) ? "위탁창고" : payload.창고유형,
            물류대행지분류 = LogisticsProxySiteTypes.Normalize(payload.물류대행지분류),
            주소 = payload.주소.Trim(),
            담당자명 = payload.담당자명.Trim(),
            연락처 = payload.연락처.Trim(),
            위도 = payload.위도,
            경도 = payload.경도,
            기본창고여부 = payload.기본창고여부,
            IsActive = true
        };

        _warehouses.Add(warehouse);
        return Task.FromResult<창고요약응답?>(warehouse);
    }

    public Task<입고요청목록응답?> GetInboundsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<입고요청목록응답?>(new() { Items = _inbounds.ToArray() });

    public Task<입고요청항목응답?> CreateInboundAsync(입고요청저장요청 payload, CancellationToken cancellationToken = default)
    {
        var inbound = new 입고요청항목응답
        {
            Id = _nextInboundId++,
            창고Id = payload.창고Id,
            입고흐름유형 = 입고흐름유형코드.Normalize(payload.입고흐름유형),
            입고생성경로 = payload.입고생성경로,
            계약선행여부 = payload.계약선행여부,
            자동생성여부 = payload.자동생성여부,
            주문Id = payload.주문Id,
            주문참조번호 = payload.주문참조번호,
            판매자UserId = payload.판매자UserId,
            출고예정Id = payload.출고예정Id,
            운송의뢰Id = payload.운송의뢰Id,
            공급처명 = payload.공급처명.Trim(),
            원주문참조번호 = payload.원주문참조번호.Trim(),
            상태 = "입고예정",
            예정도착일 = payload.예정도착일,
            계약정보 = payload.계약정보.Normalize()
        };

        _inbounds.Add(inbound);
        return Task.FromResult<입고요청항목응답?>(inbound);
    }

    public Task<입고상품목록응답?> CompleteInboundAsync(long inboundId, 입고완료요청 payload, CancellationToken cancellationToken = default)
    {
        var inbound = _inbounds.FirstOrDefault(x => x.Id == inboundId);
        if (inbound is null)
        {
            return Task.FromResult<입고상품목록응답?>(new() { Items = [] });
        }

        inbound.상태 = "입고완료";
        inbound.입고완료일시 = DateTime.Now;
        var warehouseName = _warehouses.FirstOrDefault(x => x.Id == inbound.창고Id)?.창고명 ?? "미지정 창고";
        var created = payload.Items.Select(item =>
        {
            var inventory = new 재고항목응답
            {
                입고상품Id = _nextInventoryId++,
                창고Id = inbound.창고Id,
                창고명 = warehouseName,
                소유자UserId = "shipper-web-demo",
                판매자UserId = inbound.판매자UserId,
                상품명 = item.상품명.Trim(),
                SKU = item.SKU.Trim(),
                옵션명 = item.옵션명.Trim(),
                가용수량 = Math.Max(0, item.입고수량 - item.불량수량),
                예약수량 = 0,
                상태 = "보관중",
                보관위치 = item.보관위치.Trim(),
                계약정보 = inbound.계약정보
            };

            _inventory.Add(inventory);
            return new 입고상품항목응답
            {
                Id = inventory.입고상품Id,
                입고요청Id = inbound.Id,
                창고Id = inbound.창고Id,
                소유자UserId = inventory.소유자UserId,
                판매자UserId = inventory.판매자UserId,
                상품명 = inventory.상품명,
                SKU = inventory.SKU,
                옵션명 = inventory.옵션명,
                입고수량 = item.입고수량,
                가용수량 = inventory.가용수량,
                불량수량 = item.불량수량,
                보관위치 = inventory.보관위치,
                상태 = inventory.상태,
                입고완료일시 = DateTime.Now,
                계약정보 = inventory.계약정보
            };
        }).ToArray();

        return Task.FromResult<입고상품목록응답?>(new() { Items = created });
    }

    public Task<재고목록응답?> GetInventoryAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<재고목록응답?>(new() { Items = _inventory.ToArray() });
}
