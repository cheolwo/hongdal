using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum InboundRequestPageMessageTone
{
    Info,
    Success,
    Warning,
    Error
}

public sealed record InboundRequestListFilter(string? Search = null, string? Status = null, string? FlowType = null);

public sealed class InboundRequestPageState
{
    public bool IsBusy { get; internal set; }
    public string? Message { get; internal set; }
    public InboundRequestPageMessageTone MessageTone { get; internal set; } = InboundRequestPageMessageTone.Info;
    public IReadOnlyList<입고요청항목응답> Items { get; internal set; } = [];
    public int TotalCount { get; internal set; }
    public IReadOnlyList<창고요약응답> Warehouses { get; internal set; } = [];
    public 입고요청항목응답? Current { get; internal set; }
    public IReadOnlyList<입고상품항목응답> CompletionItems { get; internal set; } = [];
    public InboundRequestListFilter ListFilter { get; internal set; } = new();
    public bool Created { get; internal set; }
}

public sealed class InboundRequestCreateDraft
{
    public Guid? ApplicationPrivacyConsentEvidenceId { get; set; }
    public string ApplicationSourceCode { get; set; } = string.Empty;
    public long? WarehouseId { get; set; }
    public string FlowType { get; set; } = 입고흐름유형코드.계약기반입고;
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string OrderReference { get; set; } = string.Empty;
    public DateTime? ExpectedArrivalDate { get; set; } = DateTime.Today;
    public string Notes { get; set; } = string.Empty;
    public string ContractNo { get; set; } = string.Empty;
    public string ContractType { get; set; } = 입고계약유형코드.보관대행;
    public string ContractCounterpartyName { get; set; } = string.Empty;
    public string ContractSettlementType { get; set; } = string.Empty;
    public decimal ContractCommissionRate { get; set; }
    public decimal ContractDailyStorageFee { get; set; }
}

public sealed class WarehouseRegistrationDraft
{
    public string Name { get; set; } = string.Empty;
    public string ProxyType { get; set; } = LogisticsProxySiteTypes.DeliveryAgency;
    public string Address { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public sealed class InboundRequestCompletionDraft
{
    public string ItemName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public int DefectQuantity { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
}

/// <summary>
/// 입고 목록·신청·상세·완료와 별도 창고 등록 route의 조회·검증·상태 전이를 담당합니다.
/// Route Page는 URL과 navigation만 조립합니다.
/// </summary>
public sealed class InboundRequestPageViewModel(IWarehouseWorkspaceService warehouseService)
{
    public InboundRequestPageState State { get; } = new();
    public InboundRequestCreateDraft CreateDraft { get; } = new();
    public WarehouseRegistrationDraft WarehouseDraft { get; } = new();
    public InboundRequestCompletionDraft CompletionDraft { get; } = new();
    public 판매상품항목응답? SelectedSalesProduct { get; private set; }

    public bool CanComplete
        => State.Current is not null
           && !string.Equals(State.Current.상태, 입고상태코드.완료, StringComparison.OrdinalIgnoreCase)
           && !string.Equals(State.Current.상태, 입고상태코드.취소, StringComparison.OrdinalIgnoreCase);

    public async Task LoadListAsync(
        InboundRequestListFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (State.IsBusy)
        {
            return;
        }

        State.IsBusy = true;
        State.ListFilter = filter ?? State.ListFilter;
        try
        {
            var response = await warehouseService.QueryInboundsAsync(new 입고요청목록조회요청
            {
                Page = 0,
                PageSize = 100,
                Search = State.ListFilter.Search,
                Status = State.ListFilter.Status,
                FlowType = State.ListFilter.FlowType,
                SortBy = nameof(입고요청항목응답.Id),
                SortDescending = true
            }, cancellationToken);
            State.Items = response?.Items ?? [];
            State.TotalCount = response?.TotalCount ?? 0;
            SetMessage($"입고 요청 {State.TotalCount:N0}건을 조회했습니다.", InboundRequestPageMessageTone.Success);
        }
        catch (Exception ex)
        {
            State.Items = [];
            State.TotalCount = 0;
            SetMessage($"입고 요청 목록을 불러오지 못했습니다. {ex.Message}", InboundRequestPageMessageTone.Error);
        }
        finally
        {
            State.IsBusy = false;
        }
    }

    public async Task LoadCreateAsync(
        InboundRequestNavigationContext context,
        CancellationToken cancellationToken = default)
    {
        await RefreshWarehousesAsync(cancellationToken);
        ApplyCreateContext(context);
    }

    public async Task RefreshWarehousesAsync(CancellationToken cancellationToken = default)
    {
        if (State.IsBusy)
        {
            return;
        }

        State.IsBusy = true;
        try
        {
            State.Warehouses = (await warehouseService.GetWarehousesAsync(cancellationToken))?.Items
                .Where(item => item.IsActive)
                .OrderByDescending(item => item.기본창고여부)
                .ThenBy(item => item.창고명)
                .ToArray() ?? [];
            if (CreateDraft.WarehouseId is null && State.Warehouses.Count > 0)
            {
                CreateDraft.WarehouseId = State.Warehouses[0].Id;
            }

            SetMessage(
                State.Warehouses.Count == 0
                    ? "사용 가능한 창고가 없습니다. 창고 등록 화면에서 먼저 등록해 주세요."
                    : $"입고 가능한 창고 {State.Warehouses.Count:N0}곳을 조회했습니다.",
                State.Warehouses.Count == 0 ? InboundRequestPageMessageTone.Warning : InboundRequestPageMessageTone.Info);
        }
        catch (Exception ex)
        {
            State.Warehouses = [];
            SetMessage($"창고 목록을 불러오지 못했습니다. {ex.Message}", InboundRequestPageMessageTone.Error);
        }
        finally
        {
            State.IsBusy = false;
        }
    }

    public async Task LoadDetailAsync(
        long inboundId,
        bool created = false,
        CancellationToken cancellationToken = default)
    {
        if (State.IsBusy)
        {
            return;
        }

        State.IsBusy = true;
        State.Created = created;
        try
        {
            State.Current = inboundId > 0
                ? await warehouseService.GetInboundAsync(inboundId, cancellationToken)
                : null;
            SetMessage(
                State.Current is null
                    ? "입고 요청을 찾을 수 없거나 현재 계정의 조회 범위에 없습니다."
                    : created
                        ? $"입고 요청 #{inboundId}을 저장하고 같은 ID로 다시 조회했습니다."
                        : $"입고 요청 #{inboundId}을 같은 ID로 다시 조회했습니다.",
                State.Current is null ? InboundRequestPageMessageTone.Warning : InboundRequestPageMessageTone.Success);
        }
        catch (Exception ex)
        {
            State.Current = null;
            SetMessage($"입고 요청 상세를 불러오지 못했습니다. {ex.Message}", InboundRequestPageMessageTone.Error);
        }
        finally
        {
            State.IsBusy = false;
        }
    }

    public async Task LoadCompletionAsync(long inboundId, CancellationToken cancellationToken = default)
    {
        await LoadDetailAsync(inboundId, cancellationToken: cancellationToken);
        if (State.Current is not null && string.IsNullOrWhiteSpace(CompletionDraft.ItemName))
        {
            CompletionDraft.ItemName = string.IsNullOrWhiteSpace(State.Current.예정상품명)
                ? State.Current.공급처명
                : State.Current.예정상품명;
            CompletionDraft.Sku = string.IsNullOrWhiteSpace(State.Current.예정SKU)
                ? State.Current.원주문참조번호
                : State.Current.예정SKU;
            CompletionDraft.Quantity = Math.Max(1, State.Current.예정수량 ?? 1);
        }
    }

    public async Task<long?> CreateInboundAsync(CancellationToken cancellationToken = default)
    {
        if (State.IsBusy)
        {
            return null;
        }

        if (CreateDraft.WarehouseId is not > 0)
        {
            SetMessage("입고할 창고를 먼저 선택해 주세요.", InboundRequestPageMessageTone.Warning);
            return null;
        }

        if (string.IsNullOrWhiteSpace(CreateDraft.SupplierName))
        {
            SetMessage("입고 공급처명을 입력해 주세요.", InboundRequestPageMessageTone.Warning);
            return null;
        }

        var requestedFlowType = 입고흐름유형코드.Normalize(CreateDraft.FlowType);
        if (requestedFlowType != 입고흐름유형코드.계약기반입고)
        {
            SetMessage(
                "이 신청 화면은 계약 기반 입고만 등록합니다. 현장 임시 입고는 안내 동의·멱등 ID가 있는 입고상품 수령 화면에서, 주문 자동 예정은 주문 workflow에서 생성해 주세요.",
                InboundRequestPageMessageTone.Warning);
            return null;
        }

        State.IsBusy = true;
        try
        {
            var flowType = requestedFlowType;
            var response = await warehouseService.CreateInboundAsync(new 입고요청저장요청
            {
                신청개인정보동의증적Id = CreateDraft.ApplicationPrivacyConsentEvidenceId,
                신청출처Code = CreateDraft.ApplicationSourceCode,
                창고Id = CreateDraft.WarehouseId.Value,
                입고흐름유형 = flowType,
                입고생성경로 = BuildInboundSourceLabel(flowType),
                계약선행여부 = 입고흐름유형코드.RequiresExistingContract(flowType),
                자동생성여부 = 입고흐름유형코드.IsOrderGenerated(flowType),
                공급처코드 = CreateDraft.SupplierCode.Trim(),
                공급처명 = CreateDraft.SupplierName.Trim(),
                원주문참조번호 = CreateDraft.OrderReference.Trim(),
                예정도착일 = CreateDraft.ExpectedArrivalDate,
                비고 = CreateDraft.Notes.Trim(),
                계약정보 = new 입고계약스냅샷
                {
                    계약번호 = CreateDraft.ContractNo,
                    계약유형 = CreateDraft.ContractType,
                    계약상대방명 = string.IsNullOrWhiteSpace(CreateDraft.ContractCounterpartyName)
                        ? CreateDraft.SupplierName
                        : CreateDraft.ContractCounterpartyName,
                    정산방식 = CreateDraft.ContractSettlementType,
                    판매수수료율 = CreateDraft.ContractCommissionRate,
                    보관료일단가 = CreateDraft.ContractDailyStorageFee,
                    통관필요여부 = 입고계약유형코드.RequiresCustoms(CreateDraft.ContractType),
                    계약시작일 = DateTime.Today,
                    계약메모 = CreateDraft.Notes
                }.Normalize()
            }, cancellationToken) ?? throw new InvalidOperationException("입고 요청 생성 응답이 비어 있습니다.");

            State.Current = await warehouseService.GetInboundAsync(response.Id, cancellationToken)
                ?? throw new InvalidOperationException("저장한 입고 요청을 같은 ID로 다시 조회하지 못했습니다.");
            State.Created = true;
            SetMessage(
                $"입고 요청 #{response.Id}을 저장하고 같은 ID로 다시 조회했습니다.",
                InboundRequestPageMessageTone.Success);
            return response.Id;
        }
        catch (Exception ex)
        {
            SetMessage($"입고 요청을 저장하지 못했습니다. {ex.Message}", InboundRequestPageMessageTone.Error);
            return null;
        }
        finally
        {
            State.IsBusy = false;
        }
    }

    public async Task<long?> CreateWarehouseAsync(CancellationToken cancellationToken = default)
    {
        if (State.IsBusy)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(WarehouseDraft.Name) || string.IsNullOrWhiteSpace(WarehouseDraft.Address))
        {
            SetMessage("창고명과 주소를 입력해 주세요.", InboundRequestPageMessageTone.Warning);
            return null;
        }

        State.IsBusy = true;
        try
        {
            var response = await warehouseService.CreateWarehouseAsync(new 창고저장요청
            {
                창고명 = WarehouseDraft.Name.Trim(),
                물류대행지분류 = LogisticsProxySiteTypes.Normalize(WarehouseDraft.ProxyType),
                주소 = WarehouseDraft.Address.Trim(),
                담당자명 = WarehouseDraft.ManagerName.Trim(),
                연락처 = WarehouseDraft.Phone.Trim()
            }, cancellationToken) ?? throw new InvalidOperationException("창고 생성 응답이 비어 있습니다.");
            SetMessage($"창고 #{response.Id}을 등록했습니다. 입고 신청서로 돌아갑니다.", InboundRequestPageMessageTone.Success);
            return response.Id;
        }
        catch (Exception ex)
        {
            SetMessage($"창고를 등록하지 못했습니다. {ex.Message}", InboundRequestPageMessageTone.Error);
            return null;
        }
        finally
        {
            State.IsBusy = false;
        }
    }

    public async Task<bool> CompleteInboundAsync(long inboundId, CancellationToken cancellationToken = default)
    {
        if (State.IsBusy || State.Current?.Id != inboundId || !CanComplete)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(CompletionDraft.ItemName)
            || CompletionDraft.Quantity <= 0
            || CompletionDraft.DefectQuantity < 0
            || CompletionDraft.DefectQuantity > CompletionDraft.Quantity)
        {
            SetMessage("상품명, 1개 이상의 입고 수량과 그 이하의 불량 수량을 확인해 주세요.", InboundRequestPageMessageTone.Warning);
            return false;
        }

        State.IsBusy = true;
        try
        {
            var result = await warehouseService.CompleteInboundAsync(inboundId, new 입고완료요청
            {
                Items =
                [
                    new 입고상품저장요청
                    {
                        상품명 = CompletionDraft.ItemName.Trim(),
                        SKU = CompletionDraft.Sku.Trim(),
                        옵션명 = CompletionDraft.OptionName.Trim(),
                        입고수량 = CompletionDraft.Quantity,
                        불량수량 = CompletionDraft.DefectQuantity,
                        보관위치 = CompletionDraft.StorageLocation.Trim()
                    }
                ]
            }, cancellationToken) ?? throw new InvalidOperationException("입고 완료 응답이 비어 있습니다.");
            State.Current = await warehouseService.GetInboundAsync(inboundId, cancellationToken)
                ?? throw new InvalidOperationException("완료한 입고 요청을 같은 ID로 다시 조회하지 못했습니다.");
            State.CompletionItems = result.Items;
            SetMessage(
                $"입고 요청 #{inboundId}을 완료하고 같은 ID로 다시 조회했습니다.",
                InboundRequestPageMessageTone.Success);
            return true;
        }
        catch (Exception ex)
        {
            SetMessage($"입고 완료 처리에 실패했습니다. {ex.Message}", InboundRequestPageMessageTone.Error);
            return false;
        }
        finally
        {
            State.IsBusy = false;
        }
    }

    public void SelectSalesProduct(판매상품항목응답? product)
    {
        SelectedSalesProduct = product;
        if (product is not null)
        {
            CompletionDraft.ItemName = product.대표상품명;
            CompletionDraft.Sku = product.판매SKU;
        }
    }

    public void ApplyWarehouseContext(InboundRequestNavigationContext context)
    {
        WarehouseDraft.Name = context.WarehouseName ?? string.Empty;
        WarehouseDraft.ProxyType = LogisticsProxySiteTypes.Normalize(context.ProxyType);
        WarehouseDraft.Address = context.WarehouseAddress ?? string.Empty;
    }

    private void ApplyCreateContext(InboundRequestNavigationContext context)
    {
        var fromDiagram = string.Equals(context.Source, "diagram-warehouse-proxy", StringComparison.OrdinalIgnoreCase);
        if (context.WarehouseId is > 0 && State.Warehouses.Any(item => item.Id == context.WarehouseId.Value))
        {
            CreateDraft.WarehouseId = context.WarehouseId;
        }
        else if (fromDiagram)
        {
            CreateDraft.WarehouseId = null;
        }

        CreateDraft.SupplierCode = context.SupplierCode ?? CreateDraft.SupplierCode;
        CreateDraft.SupplierName = context.SupplierName
                                   ?? (fromDiagram
                                       ? "다이어그램 물류 대행 신청"
                                       : CreateDraft.SupplierName);
        CreateDraft.OrderReference = context.OrderReference
                                     ?? (fromDiagram
                                         ? $"DIAGRAM-{DateTime.Today:yyyyMMdd}"
                                         : CreateDraft.OrderReference);
        CreateDraft.ExpectedArrivalDate = context.ExpectedArrivalDate ?? CreateDraft.ExpectedArrivalDate;
        CreateDraft.Notes = context.Notes ?? BuildDiagramMemo(context) ?? CreateDraft.Notes;
        CreateDraft.ContractNo = context.ContractNo ?? CreateDraft.ContractNo;
        CreateDraft.ContractType = string.IsNullOrWhiteSpace(context.ContractType)
            ? ResolveContractType(context.ProxyType)
            : 입고계약유형코드.Normalize(context.ContractType);
        CreateDraft.ContractCounterpartyName = context.ContractCounterpartyName
                                               ?? context.WarehouseName
                                               ?? CreateDraft.ContractCounterpartyName;
        CreateDraft.ContractSettlementType = context.ContractSettlementType ?? CreateDraft.ContractSettlementType;
        CreateDraft.ContractCommissionRate = context.ContractCommissionRate ?? CreateDraft.ContractCommissionRate;
        CreateDraft.ContractDailyStorageFee = context.ContractDailyStorageFee ?? CreateDraft.ContractDailyStorageFee;

        if (fromDiagram)
        {
            SetMessage(
                CreateDraft.WarehouseId is > 0
                    ? "다이어그램 창고 후보와 신청 초안을 불러왔습니다. 검토한 뒤 명시적으로 등록해 주세요."
                    : "다이어그램 후보에 실제 창고 ID가 없습니다. 창고를 등록한 뒤 신청서를 계속 작성해 주세요.",
                CreateDraft.WarehouseId is > 0 ? InboundRequestPageMessageTone.Info : InboundRequestPageMessageTone.Warning);
        }
    }

    private static string BuildInboundSourceLabel(string flowType)
        => flowType switch
        {
            입고흐름유형코드.현장임시입고 => "창고 관리자 수기 등록",
            입고흐름유형코드.주문자동입고예정 => "주문/구매 흐름 자동 생성",
            _ => "계약 DB 기반 등록"
        };

    private static string ResolveContractType(string? proxyType)
        => LogisticsProxySiteTypes.Normalize(proxyType) switch
        {
            LogisticsProxySiteTypes.MarketFulfillment => 입고계약유형코드.마켓풀필먼트,
            LogisticsProxySiteTypes.OverseasCustomsAgency => 입고계약유형코드.수입통관풀필먼트,
            _ => 입고계약유형코드.보관대행
        };

    private static string? BuildDiagramMemo(InboundRequestNavigationContext context)
    {
        if (!string.Equals(context.Source, "diagram-warehouse-proxy", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return string.Join(Environment.NewLine, new[]
        {
            "다이어그램 창고 블록에서 시작한 판매자 물류 대행 신청입니다.",
            $"- 창고 후보: {context.WarehouseName}",
            $"- 후보 범위: {context.Scope}",
            $"- 다이어그램 노드: {context.NodeTitle}",
            $"- 블록 그룹: {context.NodeGroup}",
            $"- 노드 설명: {context.NodeDescription}"
        });
    }

    private void SetMessage(string message, InboundRequestPageMessageTone tone)
    {
        State.Message = message;
        State.MessageTone = tone;
    }
}
