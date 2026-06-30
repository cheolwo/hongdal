using Hongdal.Application.CommandProcessing;
using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.배차;
using 홍달.도메인.운송;
using 홍달.도메인.창고;
using 홍달.도메인.화주;

namespace 홍달.Services.Warehouse;

public sealed class WarehouseOperationService : IWarehouseOperationService
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public WarehouseOperationService(HongdalContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<창고목록응답> GetWarehousesAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.UserId;
        var query = _db.창고.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(x => x.소유자UserId == userId || _db.창고사용자.Any(wu => wu.창고Id == x.Id && wu.UserId == userId));
        }

        var items = await query
            .OrderBy(x => x.창고명)
            .Select(x => new 창고요약응답
            {
                Id = x.Id,
                창고명 = x.창고명,
                소유자UserId = x.소유자UserId,
                주소 = x.주소,
                담당자명 = x.담당자명,
                연락처 = x.연락처,
                IsActive = x.IsActive
            })
            .ToArrayAsync(cancellationToken);

        return new 창고목록응답 { Items = items };
    }

    public async Task<창고요약응답> CreateWarehouseAsync(창고저장요청 request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var entity = new 창고
        {
            소유자UserId = userId,
            창고명 = request.창고명.Trim(),
            주소 = request.주소.Trim(),
            담당자명 = request.담당자명.Trim(),
            연락처 = request.연락처.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.창고.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new 창고요약응답
        {
            Id = entity.Id,
            창고명 = entity.창고명,
            소유자UserId = entity.소유자UserId,
            주소 = entity.주소,
            담당자명 = entity.담당자명,
            연락처 = entity.연락처,
            IsActive = entity.IsActive
        };
    }

    public async Task<창고사용자목록응답> GetWarehouseUsersAsync(long warehouseId, CancellationToken cancellationToken)
    {
        var items = await _db.창고사용자.AsNoTracking()
            .Where(x => x.창고Id == warehouseId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.역할명)
            .Select(x => new 창고사용자항목응답
            {
                Id = x.Id,
                창고Id = x.창고Id,
                UserId = x.UserId,
                사용자명 = x.UserId,
                역할명 = x.역할명,
                IsPrimary = x.IsPrimary
            })
            .ToArrayAsync(cancellationToken);

        return new 창고사용자목록응답 { Items = items };
    }

    public async Task<창고사용자항목응답> AddWarehouseUserAsync(long warehouseId, 창고사용자저장요청 request, CancellationToken cancellationToken)
    {
        var entity = new 창고사용자
        {
            창고Id = warehouseId,
            UserId = request.UserId.Trim(),
            역할명 = request.역할명.Trim(),
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.창고사용자.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new 창고사용자항목응답
        {
            Id = entity.Id,
            창고Id = entity.창고Id,
            UserId = entity.UserId,
            사용자명 = entity.UserId,
            역할명 = entity.역할명,
            IsPrimary = entity.IsPrimary
        };
    }

    public async Task<입고요청목록응답> GetInboundsAsync(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var items = await _db.입고요청.AsNoTracking()
            .Where(x => x.주문자UserId == userId || _db.창고.Any(w => w.Id == x.창고Id && w.소유자UserId == userId))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new 입고요청항목응답
            {
                Id = x.Id,
                창고Id = x.창고Id,
                주문자UserId = x.주문자UserId,
                공급처명 = x.공급처명,
                원주문참조번호 = x.원주문참조번호,
                상태 = x.상태,
                예정도착일 = x.예정도착일,
                입고완료일시 = x.입고완료일시
            })
            .ToArrayAsync(cancellationToken);

        return new 입고요청목록응답 { Items = items };
    }

    public async Task<입고요청항목응답> CreateInboundAsync(입고요청저장요청 request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var entity = new 입고요청
        {
            창고Id = request.창고Id,
            주문자UserId = userId,
            공급처명 = request.공급처명.Trim(),
            원주문참조번호 = request.원주문참조번호.Trim(),
            예정도착일 = request.예정도착일,
            비고 = request.비고.Trim(),
            상태 = "입고예정",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.입고요청.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new 입고요청항목응답
        {
            Id = entity.Id,
            창고Id = entity.창고Id,
            주문자UserId = entity.주문자UserId,
            공급처명 = entity.공급처명,
            원주문참조번호 = entity.원주문참조번호,
            상태 = entity.상태,
            예정도착일 = entity.예정도착일,
            입고완료일시 = entity.입고완료일시
        };
    }

    public async Task<입고상품목록응답> CompleteInboundAsync(long inboundId, 입고완료요청 request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var inbound = await _db.입고요청.FirstOrDefaultAsync(x => x.Id == inboundId, cancellationToken)
            ?? throw new InvalidOperationException("입고요청을 찾을 수 없습니다.");

        inbound.상태 = "입고완료";
        inbound.입고완료일시 = DateTime.UtcNow;
        inbound.UpdatedAt = DateTime.UtcNow;

        var createdItems = new List<입고상품>();
        foreach (var item in request.Items)
        {
            var inboundItem = new 입고상품
            {
                입고요청Id = inbound.Id,
                창고Id = inbound.창고Id,
                소유자UserId = inbound.주문자UserId,
                판매자UserId = userId,
                상품명 = item.상품명.Trim(),
                SKU = item.SKU.Trim(),
                옵션명 = item.옵션명.Trim(),
                입고수량 = item.입고수량,
                가용수량 = Math.Max(0, item.입고수량 - item.불량수량),
                예약수량 = 0,
                불량수량 = item.불량수량,
                보관위치 = item.보관위치.Trim(),
                상태 = "보관중",
                입고완료일시 = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            createdItems.Add(inboundItem);
            _db.입고상품.Add(inboundItem);
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var item in createdItems)
        {
            _db.재고이력.Add(new 재고이력
            {
                입고상품Id = item.Id,
                이력유형 = "입고",
                변경수량 = item.가용수량,
                변경후수량 = item.가용수량,
                원인유형 = "입고완료",
                원인Id = inbound.Id,
                처리UserId = userId,
                메모 = "입고완료로 재고 생성",
                처리일시 = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new 입고상품목록응답
        {
            Items = createdItems.Select(item => new 입고상품항목응답
            {
                Id = item.Id,
                입고요청Id = item.입고요청Id,
                창고Id = item.창고Id,
                소유자UserId = item.소유자UserId,
                판매자UserId = item.판매자UserId,
                상품명 = item.상품명,
                SKU = item.SKU,
                옵션명 = item.옵션명,
                입고수량 = item.입고수량,
                가용수량 = item.가용수량,
                불량수량 = item.불량수량,
                보관위치 = item.보관위치,
                상태 = item.상태,
                입고완료일시 = item.입고완료일시
            }).ToArray()
        };
    }

    public async Task<재고목록응답> GetInventoryAsync(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var warehouseNames = _db.창고.AsNoTracking();
        var items = await _db.입고상품.AsNoTracking()
            .Where(x => x.소유자UserId == userId || x.판매자UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new 재고항목응답
            {
                입고상품Id = x.Id,
                창고Id = x.창고Id,
                창고명 = warehouseNames.Where(w => w.Id == x.창고Id).Select(w => w.창고명).FirstOrDefault() ?? string.Empty,
                소유자UserId = x.소유자UserId,
                판매자UserId = x.판매자UserId,
                상품명 = x.상품명,
                SKU = x.SKU,
                옵션명 = x.옵션명,
                가용수량 = x.가용수량,
                예약수량 = x.예약수량,
                상태 = x.상태,
                보관위치 = x.보관위치
            })
            .ToArrayAsync(cancellationToken);

        return new 재고목록응답 { Items = items };
    }

    public async Task<Hongdal.Contracts.Shipper.Request.화주운송의뢰응답> CreateReconsignmentRequestAsync(재고운송의뢰생성요청 request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var item = await _db.입고상품.FirstOrDefaultAsync(x => x.Id == request.입고상품Id, cancellationToken)
            ?? throw new InvalidOperationException("입고상품을 찾을 수 없습니다.");

        if (item.가용수량 < request.요청수량 || request.요청수량 <= 0)
        {
            throw new InvalidOperationException("가용수량보다 많은 수량을 재위탁할 수 없습니다.");
        }

        var warehouse = await _db.창고.FirstOrDefaultAsync(x => x.Id == item.창고Id, cancellationToken)
            ?? throw new InvalidOperationException("창고 정보를 찾을 수 없습니다.");

        var now = DateTime.UtcNow;
        var requestId = Guid.NewGuid().ToString();
        var shipRequest = new 화주운송의뢰
        {
            의뢰Id = requestId,
            화주Id = item.판매자UserId,
            주문자UserId = userId,
            화물종류 = string.IsNullOrWhiteSpace(request.화물종류) ? item.상품명 : request.화물종류.Trim(),
            화물설명 = $"입고상품 재위탁: {item.상품명}",
            화물수량 = request.요청수량,
            화물중량Kg = null,
            화물부피Cbm = null,
            화물파손주의여부 = false,
            화물온도조건 = "상온",
            운송방식 = "재위탁",
            차량종류 = request.차량종류.Trim(),
            결제수단 = Hongdal.Contracts.Shipper.Request.결제수단.별도정산.ToString(),
            정산시점 = Hongdal.Contracts.Shipper.Request.정산시점.운송완료후정산.ToString(),
            증빙방식 = Hongdal.Contracts.Shipper.Request.증빙방식.없음.ToString(),
            수납주체 = Hongdal.Contracts.Shipper.Request.수납주체.플랫폼.ToString(),
            정산상태 = Hongdal.Contracts.Shipper.Request.운임정산상태.청구대기.ToString(),
            정산메모 = "입고상품 재위탁 운송",
            결제예정금액 = null,
            픽업_도로명주소 = warehouse.주소,
            픽업_상세주소 = item.보관위치,
            픽업_연락처_이름 = warehouse.담당자명,
            픽업_연락처_전화번호 = warehouse.연락처,
            픽업_시간창_시작일시 = now,
            픽업_시간창_종료일시 = now.AddDays(1),
            하차_도로명주소 = request.하차지주소.Trim(),
            하차_상세주소 = request.하차지상세주소.Trim(),
            하차_연락처_이름 = userId,
            하차_연락처_전화번호 = warehouse.연락처,
            하차_시간창_시작일시 = now,
            하차_시간창_종료일시 = now.AddDays(1),
            서비스레벨 = "일반",
            요청사항 = $"재위탁 출고 상품 SKU: {item.SKU}",
            클라이언트요청Id = $"reconsignment-{item.Id}-{now:yyyyMMddHHmmss}",
            상태 = 상태값.의뢰상태.생성됨,
            결제상태 = 상태값.결제상태.결제대기,
            배차상태 = 상태값.배차상태.미시작,
            CreatedAt = now,
            UpdatedAt = now
        };

        item.가용수량 -= request.요청수량;
        item.예약수량 += request.요청수량;
        item.상태 = item.가용수량 == 0 ? "재위탁대기" : item.상태;
        item.UpdatedAt = now;

        _db.화주운송의뢰.Add(shipRequest);
        _db.운송의뢰상품연결.Add(new 운송의뢰상품연결
        {
            운송의뢰Id = shipRequest.의뢰Id,
            입고상품Id = item.Id,
            할당수량 = request.요청수량,
            CreatedAt = now
        });

        _db.배차대기.Add(new 배차대기
        {
            의뢰Id = shipRequest.의뢰Id,
            화주Id = shipRequest.화주Id,
            픽업_도로명주소 = shipRequest.픽업_도로명주소,
            픽업_상세주소 = shipRequest.픽업_상세주소,
            픽업_위도 = shipRequest.픽업_위도,
            픽업_경도 = shipRequest.픽업_경도,
            하차_도로명주소 = shipRequest.하차_도로명주소,
            하차_상세주소 = shipRequest.하차_상세주소,
            하차_위도 = shipRequest.하차_위도,
            하차_경도 = shipRequest.하차_경도,
            상태 = 상태값.배차대기상태.대기,
            CreatedAt = now,
            UpdatedAt = now
        });

        _db.재고이력.Add(new 재고이력
        {
            입고상품Id = item.Id,
            이력유형 = "예약",
            변경수량 = -request.요청수량,
            변경후수량 = item.가용수량,
            원인유형 = "재위탁운송생성",
            처리UserId = userId,
            메모 = $"재위탁 운송의뢰 생성: {shipRequest.의뢰Id}",
            처리일시 = now
        });

        await Hongdal.Application.Shipper.Request.화주운송의뢰매퍼.UpsertCargoRequirementAsync(_db, shipRequest, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Hongdal.Application.Shipper.Request.화주운송의뢰매퍼.To응답(shipRequest);
    }

    private string RequireUserId()
    {
        return _currentUserAccessor.UserId ?? throw new InvalidOperationException("로그인 사용자를 확인할 수 없습니다.");
    }
}
