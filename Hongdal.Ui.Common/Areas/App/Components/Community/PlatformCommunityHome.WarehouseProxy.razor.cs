using System.Globalization;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Versioning;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Ui.Common.Areas.App.Models;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private async Task 창고대행후보목록불러오기Async()
    {
        창고대행후보목록로딩중 = true;
        창고대행후보목록.Clear();
        선택창고대행후보키 = null;

        try
        {
            var warehouseService = Services.GetService<IWarehouseWorkspaceService>();
            if (warehouseService is not null)
            {
                var response = await warehouseService.GetWarehousesAsync();
                foreach (var warehouse in (response?.Items ?? []).Where(warehouse => warehouse.IsActive))
                {
                    창고대행후보목록.Add(창고대행후보매핑(warehouse));
                }
            }
        }
        catch (Exception ex)
        {
            창고대행신청알림수준 = Severity.Warning;
            창고대행신청알림문구 = $"창고 목록을 불러오지 못했습니다. 기본 후보로 신청 초안을 만들 수 있습니다: {ex.Message}";
        }
        finally
        {
            if (창고대행후보목록.Count == 0)
            {
                창고대행후보목록.AddRange(기본창고대행후보목록);
            }

            var defaultCandidate = 창고대행후보목록[0];
            선택창고대행후보키 = defaultCandidate.Key;
            창고대행신청양식미리채우기(defaultCandidate);
            창고대행후보목록로딩중 = false;
        }
    }

    private static 다이어그램창고대행후보 창고대행후보매핑(창고요약응답 warehouse)
    {
        var proxyTypeCode = LogisticsProxySiteTypes.Normalize(warehouse.물류대행지분류);
        var proxyTypeLabel = LogisticsProxySiteTypes.GetDisplayName(proxyTypeCode);
        var scopeLabel = warehouse.기본창고여부 ? "내 기본 창고 후보" : "공유/대행 창고 후보";
        var description = $"{warehouse.창고명}에 입고, 보관, 피킹/포장, 출고 대행 가능 여부를 신청서로 작성합니다.";

        return new(
            $"warehouse:{warehouse.Id}",
            warehouse.Id,
            warehouse.창고명,
            scopeLabel,
            proxyTypeCode,
            proxyTypeLabel,
            warehouse.주소,
            description,
            IsWorkspaceWarehouse: true);
    }

    private static readonly IReadOnlyList<다이어그램창고대행후보> 기본창고대행후보목록 =
    [
        new(
            "default:own",
            null,
            "내 기본 창고",
            "내 창고 후보",
            LogisticsProxySiteTypes.DeliveryAgency,
            LogisticsProxySiteTypes.GetDisplayName(LogisticsProxySiteTypes.DeliveryAgency),
            "주소 미정",
            "내가 관리하는 창고에 입고, 보관, 출고 대행을 맡기는 신청서로 이동합니다.",
            IsWorkspaceWarehouse: false),
        new(
            "default:shared-nearby",
            null,
            "가까운 공유 창고",
            "다른 사용자 창고 후보",
            LogisticsProxySiteTypes.DeliveryAgency,
            LogisticsProxySiteTypes.GetDisplayName(LogisticsProxySiteTypes.DeliveryAgency),
            "배송권 기준 조회 필요",
            "다른 사용자가 공개한 가까운 창고에 물류 대행 가능 여부를 신청서로 작성합니다.",
            IsWorkspaceWarehouse: false),
        new(
            "default:market-fulfillment",
            null,
            "마켓 물류 대행 창고",
            "판매채널 대행 후보",
            LogisticsProxySiteTypes.MarketFulfillment,
            LogisticsProxySiteTypes.GetDisplayName(LogisticsProxySiteTypes.MarketFulfillment),
            "스마트스토어/쿠팡 출고권역 기준 조회 필요",
            "스마트스토어, 쿠팡 같은 판매채널 주문의 입고, 피킹, 포장, 출고 대행을 신청서로 작성합니다.",
            IsWorkspaceWarehouse: false)
    ];

    private string 창고대행후보클래스생성(다이어그램창고대행후보 candidate)
    {
        var selected = string.Equals(candidate.Key, 선택창고대행후보키, StringComparison.OrdinalIgnoreCase)
            ? " platform-diagram-warehouse-candidate--selected"
            : string.Empty;

        return $"platform-diagram-warehouse-candidate{selected}";
    }

    private void 창고대행후보선택(string candidateKey)
    {
        var candidate = 창고대행후보목록.FirstOrDefault(candidate =>
            string.Equals(candidate.Key, candidateKey, StringComparison.OrdinalIgnoreCase));
        if (candidate is null)
        {
            창고대행신청알림수준 = Severity.Warning;
            창고대행신청알림문구 = "선택한 창고 후보를 찾지 못했습니다.";
            return;
        }

        선택창고대행후보키 = candidateKey;
        창고대행신청양식미리채우기(candidate);
        창고대행신청알림수준 = Severity.Info;
        창고대행신청알림문구 = candidate.WarehouseId is null
            ? "이 후보는 아직 실제 창고가 아니므로 업무 화면에서 창고 등록 후 신청을 이어가세요."
            : "이 창고 후보로 다이어그램 안에서 입고/물류 대행 요청을 등록할 수 있습니다.";
    }

    private void 창고대행신청패널닫기()
    {
        창고대행신청노드 = null;
        선택창고대행후보키 = null;
        창고대행신청알림문구 = null;
        창고대행후보목록.Clear();
        창고대행신청양식초기화();
    }

    private async Task 창고대행입고신청생성Async()
    {
        var node = 창고대행신청노드;
        var candidate = 선택창고대행후보;
        if (node is null || candidate is null)
        {
            창고대행신청알림수준 = Severity.Warning;
            창고대행신청알림문구 = "물류 대행을 신청할 창고 블록과 후보를 먼저 선택하세요.";
            return;
        }

        if (candidate.WarehouseId is null)
        {
            창고대행신청알림수준 = Severity.Warning;
            창고대행신청알림문구 = "실제 창고 ID가 없는 후보입니다. 업무 화면에서 창고를 등록한 뒤 신청을 이어가세요.";
            return;
        }

        var warehouseService = Services.GetService<IWarehouseWorkspaceService>();
        if (warehouseService is null)
        {
            창고대행신청알림수준 = Severity.Warning;
            창고대행신청알림문구 = "현재 화면에는 창고 업무 서비스가 연결되어 있지 않습니다. 업무 화면에서 신청서를 작성하세요.";
            return;
        }

        if (string.IsNullOrWhiteSpace(warehouseProxySupplierName) ||
            string.IsNullOrWhiteSpace(warehouseProxyOrderReference))
        {
            창고대행신청알림수준 = Severity.Warning;
            창고대행신청알림문구 = "공급처명과 원주문 참조번호를 입력해야 신청할 수 있습니다.";
            return;
        }

        창고대행신청제출중 = true;
        창고대행신청알림수준 = Severity.Info;
        창고대행신청알림문구 = "다이어그램 신청 값을 창고 입고 API로 전송하고 있습니다.";

        try
        {
            var response = await warehouseService.CreateInboundAsync(new 입고요청저장요청
            {
                창고Id = candidate.WarehouseId.Value,
                입고흐름유형 = 입고흐름유형코드.계약기반입고,
                입고생성경로 = $"다이어그램 창고 블록/{node.Title}",
                계약선행여부 = true,
                자동생성여부 = false,
                공급처코드 = warehouseProxySupplierCode.Trim(),
                공급처명 = warehouseProxySupplierName.Trim(),
                원주문참조번호 = warehouseProxyOrderReference.Trim(),
                예정도착일 = warehouseProxyExpectedArrivalDate,
                비고 = 창고대행메모.Trim(),
                계약정보 = new 입고계약스냅샷
                {
                    계약번호 = warehouseProxyContractNo,
                    계약유형 = 창고대행계약유형,
                    계약상대방명 = string.IsNullOrWhiteSpace(warehouseProxyContractCounterpartyName)
                        ? candidate.Name
                        : warehouseProxyContractCounterpartyName,
                    정산방식 = warehouseProxyContractSettlementType,
                    판매수수료율 = warehouseProxyContractCommissionRate,
                    보관료일단가 = warehouseProxyContractDailyStorageFee,
                    통관필요여부 = 입고계약유형코드.RequiresCustoms(창고대행계약유형),
                    계약시작일 = DateTime.Today,
                    계약메모 = 창고대행계약메모생성(node, candidate)
                }.Normalize()
            });

            창고대행신청알림수준 = Severity.Success;
            창고대행신청알림문구 = response is null
                ? "입고/물류 대행 요청을 등록했습니다. 창고 업무 화면에서 목록을 새로고침해 확인하세요."
                : $"입고/물류 대행 요청 #{response.Id.ToString(CultureInfo.InvariantCulture)}을 등록했습니다. 상태: {response.상태}";
        }
        catch (Exception ex)
        {
            창고대행신청알림수준 = Severity.Error;
            창고대행신청알림문구 = $"입고/물류 대행 요청 등록에 실패했습니다: {ex.Message}";
        }
        finally
        {
            창고대행신청제출중 = false;
        }
    }

    private void 창고대행신청양식미리채우기(다이어그램창고대행후보 candidate)
    {
        warehouseProxySupplierCode = "DIAGRAM-SUPPLIER";
        warehouseProxySupplierName = "다이어그램 물류 대행 신청";
        warehouseProxyOrderReference = $"DIAGRAM-{DateTime.Today:yyyyMMdd}";
        warehouseProxyExpectedArrivalDate = DateTime.Today.AddDays(1);
        warehouseProxyContractNo = string.Empty;
        창고대행계약유형 = 창고대행입고계약유형해결(candidate.ProxyTypeCode);
        warehouseProxyContractCounterpartyName = candidate.Name;
        warehouseProxyContractSettlementType = "보관료/작업비 협의";
        warehouseProxyContractCommissionRate = 0m;
        warehouseProxyContractDailyStorageFee = 0m;
        창고대행메모 = 창고대행신청메모생성(창고대행신청노드, candidate);
    }

    private void 창고대행신청양식초기화()
    {
        창고대행신청제출중 = false;
        warehouseProxySupplierCode = string.Empty;
        warehouseProxySupplierName = string.Empty;
        warehouseProxyOrderReference = string.Empty;
        warehouseProxyExpectedArrivalDate = DateTime.Today.AddDays(1);
        warehouseProxyContractNo = string.Empty;
        창고대행계약유형 = 입고계약유형코드.보관대행;
        warehouseProxyContractCounterpartyName = string.Empty;
        warehouseProxyContractSettlementType = "보관료/작업비 협의";
        warehouseProxyContractCommissionRate = 0m;
        warehouseProxyContractDailyStorageFee = 0m;
        창고대행메모 = string.Empty;
    }

    private static string 창고대행입고계약유형해결(string proxyType)
        => LogisticsProxySiteTypes.Normalize(proxyType) switch
        {
            LogisticsProxySiteTypes.MarketFulfillment => 입고계약유형코드.마켓풀필먼트,
            LogisticsProxySiteTypes.OverseasCustomsAgency => 입고계약유형코드.수입통관풀필먼트,
            _ => 입고계약유형코드.보관대행
        };

    private static string 창고대행신청메모생성(
        원장블록노드? node,
        다이어그램창고대행후보 candidate)
        => string.Join(Environment.NewLine, new[]
        {
            "다이어그램 창고 블록에서 시작한 판매자 물류 대행 신청입니다.",
            $"- 창고 후보: {candidate.Name}",
            $"- 물류 대행 유형: {candidate.ProxyTypeLabel}",
            $"- 후보 범위: {candidate.ScopeLabel}",
            $"- 다이어그램 노드: {node?.Title ?? "창고 블록"}",
            $"- 블록 그룹: {node?.GroupLabel ?? string.Empty}",
            $"- 노드 설명: {node?.Description ?? string.Empty}",
            "- 보완 필요: 품목, 수량, 보관 기간, 피킹/포장/출고 대행 필요 여부"
        });

    private static string 창고대행계약메모생성(
        원장블록노드 node,
        다이어그램창고대행후보 candidate)
        => $"다이어그램 노드 '{node.Title}'에서 {candidate.Name} 후보로 생성한 물류 대행 계약 초안입니다.";

    private void 창고대행신청으로이동()
    {
        var node = 창고대행신청노드;
        var candidate = 선택창고대행후보;
        if (node is null || candidate is null)
        {
            창고대행신청알림수준 = Severity.Warning;
            창고대행신청알림문구 = "물류 대행을 신청할 창고 후보를 먼저 선택하세요.";
            return;
        }

        var targetUrl = 창고대행신청Url생성(node, candidate);
        창고대행신청패널닫기();
        HomeModeState.SetWorkMode(true);
        DiagramPalette.SetDiagramMode(false);
        Navigation.NavigateTo(targetUrl);
    }

    private static string 창고대행신청Url생성(
        원장블록노드 node,
        다이어그램창고대행후보 candidate)
    {
        var values = new Dictionary<string, string?>
        {
            ["source"] = "diagram-warehouse-proxy",
            ["warehouseId"] = candidate.WarehouseId?.ToString(CultureInfo.InvariantCulture),
            ["warehouseName"] = candidate.Name,
            ["proxyType"] = candidate.ProxyTypeCode,
            ["warehouseAddress"] = candidate.Address,
            ["nodeTitle"] = node.Title,
            ["nodeGroup"] = node.GroupLabel,
            ["nodeDescription"] = node.Description,
            ["scope"] = candidate.ScopeLabel
        };

        return BuildUrlWithQuery("/shipper/inbound/requests", values);
    }

    private static string BuildUrlWithQuery(string path, IReadOnlyDictionary<string, string?> values)
    {
        var query = string.Join(
            "&",
            values
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}"));

        return string.IsNullOrWhiteSpace(query) ? path : $"{path}?{query}";
    }
}
