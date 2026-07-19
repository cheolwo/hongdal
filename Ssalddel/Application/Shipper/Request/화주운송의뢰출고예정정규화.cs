using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Application.Shipper.Request;

internal static class 화주운송의뢰출고예정정규화
{
    internal static 출고예정운송대상 To출고예정운송대상(살뜰.도메인.화주.화주운송의뢰 entity)
    {
        var quantity = entity.화물수량.GetValueOrDefault();
        if (quantity <= 0)
        {
            quantity = 1;
        }

        return new 출고예정운송대상
        {
            원천유형 = 출고예정운송대상원천유형.화주운송의뢰,
            원천참조번호 = entity.의뢰Id,
            표시명 = string.IsNullOrWhiteSpace(entity.화물종류) ? "화주 운송 의뢰 화물" : entity.화물종류,
            운송의뢰Id = entity.의뢰Id,
            판매자UserId = entity.화주Id,
            주문자UserId = entity.주문자UserId,
            상차주소 = entity.픽업_도로명주소,
            상차위도 = entity.픽업_위도,
            상차경도 = entity.픽업_경도,
            하차주소 = entity.하차_도로명주소,
            하차위도 = entity.하차_위도,
            하차경도 = entity.하차_경도,
            온도조건 = string.IsNullOrWhiteSpace(entity.화물온도조건) ? "상온" : entity.화물온도조건,
            파손주의 = entity.화물파손주의여부,
            Lines =
            [
                new 출고예정운송대상라인
                {
                    LineKey = $"{entity.의뢰Id}:cargo",
                    Sku = string.IsNullOrWhiteSpace(entity.화물종류) ? "SHIPPER-CARGO" : $"SHIPPER-CARGO-{entity.화물종류}",
                    ProductName = string.IsNullOrWhiteSpace(entity.화물종류) ? "화주 운송 의뢰 화물" : entity.화물종류,
                    Quantity = quantity,
                    WeightKg = entity.화물중량Kg
                }
            ]
        };
    }

    internal static OutboundBatchPlanRequest To출고배치계획요청(살뜰.도메인.화주.화주운송의뢰 entity)
        => To출고배치계획요청(To출고예정운송대상(entity));

    internal static OutboundBatchPlanRequest To출고배치계획요청(출고예정운송대상 target)
    {
        return new OutboundBatchPlanRequest
        {
            OrderReference = target.원천참조번호,
            SellerUserId = target.판매자UserId,
            OrdererUserId = target.주문자UserId,
            DestinationAddress = target.하차주소,
            DestinationLatitude = target.하차위도,
            DestinationLongitude = target.하차경도,
            Lines = target.Lines.Select(line => new OutboundBatchPlanLineRequest
            {
                LineKey = line.LineKey,
                SalesProductId = line.SalesProductId,
                PreferredInboundProductId = line.InboundProductId,
                Sku = line.Sku,
                ProductName = line.ProductName,
                Quantity = line.Quantity
            }).ToArray()
        };
    }
}
