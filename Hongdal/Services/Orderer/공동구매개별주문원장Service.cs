using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Services.Community;

namespace Hongdal.Services.Orderer;

public sealed record 공동구매개별주문원장연결결과(
    string 개별주문원장Id,
    string 입고예정원장Id);

public interface I공동구매개별주문원장Service
{
    Task<공동구매개별주문원장연결결과> 생성및연결Async(
        공동구매자동집단응답 group,
        공동구매자동수요응답 demand,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 예약 결제된 개인 수요를 공동구매 원장의 개별 주문으로 만들고,
/// 주문자의 도착 창고를 가리키는 입고 예정 원장을 하위 원장으로 연결합니다.
/// </summary>
public sealed class 공동구매개별주문원장Service : I공동구매개별주문원장Service
{
    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly I주문원장통합UseCase _주문원장통합UseCase;

    public 공동구매개별주문원장Service(
        I커뮤니티원장저장소 원장저장소,
        I주문원장통합UseCase 주문원장통합UseCase)
    {
        _원장저장소 = 원장저장소;
        _주문원장통합UseCase = 주문원장통합UseCase;
    }

    public async Task<공동구매개별주문원장연결결과> 생성및연결Async(
        공동구매자동집단응답 group,
        공동구매자동수요응답 demand,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(demand);
        if (demand.도착창고Id is not > 0)
        {
            throw new InvalidOperationException("개별 주문 원장을 만들려면 주문자의 도착 창고 ID가 필요합니다.");
        }

        var sourceLedger = await _원장저장소.원장조회Async(demand.커뮤니티원장Id, cancellationToken)
            ?? throw new InvalidOperationException("개별 주문을 연결할 공동구매 원장을 찾을 수 없습니다.");
        if (!string.Equals(
                sourceLedger.원장템플릿Key,
                CommunityLedgerTemplateKeys.GroupPurchase,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("개별 주문은 공동구매 원장에만 연결할 수 있습니다.");
        }

        var 주문원장Id = $"{sourceLedger.원장Id}-{demand.수요Id}-individual-order";
        var 입고원장Id = $"{sourceLedger.원장Id}-{demand.수요Id}-inbound-planned";
        var 변경자 = string.IsNullOrWhiteSpace(demand.주문자키) ? "system" : demand.주문자키;
        var 주문자표시명 = string.IsNullOrWhiteSpace(demand.주문자표시명) ? "공동구매 주문자" : demand.주문자표시명;

        if (await _원장저장소.원장조회Async(입고원장Id, cancellationToken) is null)
        {
            await _원장저장소.원장저장Async(
                new 커뮤니티원장저장요청
                {
                    원장Id = 입고원장Id,
                    커뮤니티Id = sourceLedger.커뮤니티Id,
                    원장템플릿Key = CommunityLedgerTemplateKeys.WarehouseInbound,
                    제목 = $"{주문자표시명} · {group.상품명} 입고 예정",
                    원함 = "주문 물품을 지정한 수령 창고에 인도받고 검수 뒤 입고 완료로 전환합니다.",
                    상태 = 커뮤니티원장상태.초안,
                    현재단계Key = "inbound-planned",
                    생성자UserId = 변경자,
                    생성자표시명 = 주문자표시명,
                    블록목록 =
                    [
                        new 커뮤니티원장블록Dto
                        {
                            BlockId = "receiving-right",
                            BlockType = CommunityLedgerBlockTypes.Generic,
                            Title = "입고 예정·수령 권리",
                            State = 공동구매개별주문입고상태코드.입고예정,
                            Data = 안전한입고참조(group, demand, 주문원장Id)
                        }
                    ],
                    외부참조 = 안전한입고참조(group, demand, 주문원장Id)
                },
                변경자,
                cancellationToken);
        }

        if (await _원장저장소.원장조회Async(주문원장Id, cancellationToken) is null)
        {
            await _원장저장소.원장저장Async(
                new 커뮤니티원장저장요청
                {
                    원장Id = 주문원장Id,
                    커뮤니티Id = sourceLedger.커뮤니티Id,
                    원장템플릿Key = CommunityLedgerTemplateKeys.Order,
                    제목 = $"{주문자표시명} · {group.상품명} 개별 주문",
                    원함 = "공동주문 안에서 개인 주문 수량과 수령 창고별 입고 예정 상태를 추적합니다.",
                    상태 = 커뮤니티원장상태.초안,
                    현재단계Key = "receiving-planned",
                    생성자UserId = 변경자,
                    생성자표시명 = 주문자표시명,
                    포함원장목록 =
                    [
                        new 커뮤니티포함원장참조Dto
                        {
                            원장Id = 입고원장Id,
                            원장템플릿Key = CommunityLedgerTemplateKeys.WarehouseInbound,
                            역할 = 주문원장포함역할.창고입고,
                            관계유형 = CommunityLedgerRelationTypes.Contains,
                            필수여부 = true,
                            표시순서 = 0
                        }
                    ],
                    블록목록 =
                    [
                        new 커뮤니티원장블록Dto
                        {
                            BlockId = "individual-order-receiving",
                            BlockType = CommunityLedgerBlockTypes.Generic,
                            Title = "개별 주문·도착 창고",
                            State = 공동구매개별주문입고상태코드.입고예정,
                            Data = 안전한입고참조(group, demand, 주문원장Id)
                        }
                    ],
                    외부참조 = 안전한입고참조(group, demand, 주문원장Id)
                },
                변경자,
                cancellationToken);
        }

        var alreadyLinked = sourceLedger.포함원장목록.Any(x =>
            string.Equals(x.원장Id, 주문원장Id, StringComparison.OrdinalIgnoreCase));
        if (!alreadyLinked)
        {
            var linked = await _주문원장통합UseCase.하위원장연결Async(
                sourceLedger.원장Id,
                new 주문하위원장연결요청
                {
                    하위원장Id = 주문원장Id,
                    역할 = 주문원장포함역할.개별주문,
                    필수여부 = true,
                    표시순서 = sourceLedger.포함원장목록.Count
                },
                변경자,
                cancellationToken);
            if (linked.IsFailed)
            {
                throw new InvalidOperationException(string.Join(" ", linked.Errors.Select(x => x.Message)));
            }
        }

        return new 공동구매개별주문원장연결결과(주문원장Id, 입고원장Id);
    }

    private static IReadOnlyDictionary<string, string> 안전한입고참조(
        공동구매자동집단응답 group,
        공동구매자동수요응답 demand,
        string 주문원장Id)
        => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AutomaticGroupId"] = group.자동집단Id,
            ["DemandId"] = demand.수요Id,
            ["IndividualOrderLedgerId"] = 주문원장Id,
            ["DestinationWarehouseId"] = demand.도착창고Id?.ToString() ?? string.Empty,
            ["DestinationWarehouseType"] = demand.도착창고유형,
            ["DestinationWarehouseName"] = demand.도착창고명,
            ["ReceivingAddressReference"] = demand.수령지주소참조키,
            ["InboundMeaningStatus"] = 공동구매개별주문입고상태코드.입고예정,
            ["VirtualWarehouse"] = string.Equals(
                demand.도착창고유형,
                창고유형코드.가상창고,
                StringComparison.OrdinalIgnoreCase).ToString(),
            ["ProductKey"] = group.상품키,
            ["ProductName"] = group.상품명,
            ["RequestedQuantity"] = demand.희망수량.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["QuantityUnit"] = demand.수량단위,
            ["SourceGroupPurchaseLedgerId"] = demand.커뮤니티원장Id
        };
}
