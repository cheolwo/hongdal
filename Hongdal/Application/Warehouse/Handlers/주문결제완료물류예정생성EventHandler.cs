using MediatR;
using Hongdal.Contracts.Common.Inbound;
using 홍달.도메인.창고;

namespace Hongdal.Application.Warehouse;

public sealed class 주문결제완료물류예정생성EventHandler : INotificationHandler<주문결제완료됨Event>
{
    private readonly HongdalContext _db;

    public 주문결제완료물류예정생성EventHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task Handle(주문결제완료됨Event notification, CancellationToken cancellationToken)
    {
        if (notification.상품목록.Count == 0)
        {
            return;
        }

        var 판매자창고 = await EnsureDefaultWarehouseAsync(
            notification.판매자UserId,
            창고소유자유형.판매자,
            "판매자 기본 출고창고",
            cancellationToken);

        var 주문자창고 = await EnsureDefaultWarehouseAsync(
            notification.주문자UserId,
            창고소유자유형.주문자,
            "주문자 기본 입고창고",
            cancellationToken);

        foreach (var item in notification.상품목록.Where(x => x.수량 > 0))
        {
            var alreadyExists = await _db.출고예정.AnyAsync(x =>
                x.주문Id == notification.주문Id &&
                x.주문참조번호 == notification.주문참조번호 &&
                x.판매자UserId == notification.판매자UserId &&
                x.SKU == item.SKU &&
                x.상태 != 출고상태.취소,
                cancellationToken);

            if (alreadyExists)
            {
                continue;
            }

            var 출고 = new 출고예정
            {
                주문Id = notification.주문Id,
                주문참조번호 = notification.주문참조번호,
                판매상품Id = item.판매상품Id,
                입고상품Id = item.입고상품Id,
                판매자UserId = notification.판매자UserId,
                주문자UserId = notification.주문자UserId,
                출고창고Id = 판매자창고.Id,
                상품명 = item.상품명,
                SKU = item.SKU,
                수량 = item.수량,
                상태 = 출고상태.예정,
                CreatedAt = notification.발생시각Utc,
                UpdatedAt = notification.발생시각Utc
            };

            _db.출고예정.Add(출고);

            var 입고 = new 입고요청
            {
                창고Id = 주문자창고.Id,
                입고흐름유형 = 입고흐름유형코드.주문자동입고예정,
                입고생성경로 = "주문/구매 흐름 자동 생성",
                계약선행여부 = false,
                자동생성여부 = true,
                주문Id = notification.주문Id,
                주문참조번호 = notification.주문참조번호,
                주문자UserId = notification.주문자UserId,
                판매자UserId = notification.판매자UserId,
                공급처명 = notification.판매자UserId,
                원주문참조번호 = notification.주문참조번호,
                상태 = 입고상태.예정,
                비고 = $"주문 결제 완료로 생성된 입고예정: {item.상품명}",
                CreatedAt = notification.발생시각Utc,
                UpdatedAt = notification.발생시각Utc
            };

            _db.입고요청.Add(입고);

            await _db.SaveChangesAsync(cancellationToken);

            출고.입고요청Id = 입고.Id;
            입고.출고예정Id = 출고.Id;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<창고> EnsureDefaultWarehouseAsync(
        string userId,
        string ownerType,
        string defaultName,
        CancellationToken cancellationToken)
    {
        var warehouse = await _db.창고.FirstOrDefaultAsync(x =>
            x.소유자UserId == userId &&
            x.소유자유형 == ownerType &&
            x.기본창고여부 &&
            x.IsActive,
            cancellationToken);

        if (warehouse is not null)
        {
            return warehouse;
        }

        warehouse = new 창고
        {
            소유자UserId = userId,
            소유자유형 = ownerType,
            창고유형 = 창고유형.가상창고,
            창고명 = defaultName,
            국가코드 = "KR",
            담당자명 = userId,
            기본창고여부 = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.창고.Add(warehouse);
        await _db.SaveChangesAsync(cancellationToken);
        return warehouse;
    }
}
