namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class PlatformDiagramPaletteStateService
{
    public const string CommonWorkflowModeKey = "common";

    private readonly List<PlatformDiagramPaletteBlock> pendingBlocks = [];
    private readonly List<PlatformDiagramWorkflowPreset> pendingWorkflowPresets = [];

    public event Action? Changed;

    public event Action? BlockRequested;

    public event Action? WorkflowPresetRequested;

    public bool IsDiagramMode { get; private set; }

    public string WorkflowModeKey { get; private set; } = CommonWorkflowModeKey;

    public string? LedgerTemplateKey { get; private set; }

    public void SetDiagramMode(bool isDiagramMode)
    {
        if (IsDiagramMode == isDiagramMode)
        {
            return;
        }

        IsDiagramMode = isDiagramMode;
        Changed?.Invoke();
    }

    public void SetWorkflowMode(string? workflowModeKey, string? ledgerTemplateKey = null)
    {
        var nextMode = string.IsNullOrWhiteSpace(workflowModeKey)
            ? CommonWorkflowModeKey
            : workflowModeKey.Trim();
        var nextLedgerTemplateKey = string.IsNullOrWhiteSpace(ledgerTemplateKey)
            ? LedgerTemplateKey
            : ledgerTemplateKey.Trim();
        if (string.Equals(WorkflowModeKey, nextMode, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(LedgerTemplateKey, nextLedgerTemplateKey, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        WorkflowModeKey = nextMode;
        LedgerTemplateKey = nextLedgerTemplateKey;
        Changed?.Invoke();
    }

    public void RequestBlock(PlatformDiagramPaletteBlock block)
    {
        pendingBlocks.Add(block);
        BlockRequested?.Invoke();
    }

    public IReadOnlyList<PlatformDiagramPaletteBlock> ConsumePendingBlocks()
    {
        if (pendingBlocks.Count == 0)
        {
            return [];
        }

        var blocks = pendingBlocks.ToList();
        pendingBlocks.Clear();
        return blocks;
    }

    public void RequestWorkflowPreset(PlatformDiagramWorkflowPreset preset)
    {
        pendingWorkflowPresets.Add(preset);
        WorkflowPresetRequested?.Invoke();
    }

    public IReadOnlyList<PlatformDiagramWorkflowPreset> ConsumePendingWorkflowPresets()
    {
        if (pendingWorkflowPresets.Count == 0)
        {
            return [];
        }

        var presets = pendingWorkflowPresets.ToList();
        pendingWorkflowPresets.Clear();
        return presets;
    }
}

public sealed record PlatformDiagramPaletteBlock(
    string Key,
    string Title,
    string GroupLabel,
    string Description,
    string Kind,
    string? FormKind = null);

public sealed record PlatformDiagramWorkflowPreset(
    string Key,
    string Title,
    string? LedgerTemplateKey,
    IReadOnlyList<PlatformDiagramPaletteBlock> Nodes,
    IReadOnlyList<PlatformDiagramWorkflowConnection> Connections);

public sealed record PlatformDiagramWorkflowConnection(
    string FromTitle,
    string ToTitle,
    string Label);

public sealed record PlatformDiagramFormTargetCondition(
    string? TargetNodeKind,
    IReadOnlyList<string> TargetTitleKeywords)
{
    public bool Matches(string targetNodeKind, string targetNodeTitle)
    {
        var kindMatches = string.IsNullOrWhiteSpace(TargetNodeKind) ||
            string.Equals(TargetNodeKind, targetNodeKind, StringComparison.OrdinalIgnoreCase);
        var titleMatches = TargetTitleKeywords.Count == 0 ||
            TargetTitleKeywords.Any(keyword =>
                targetNodeTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        return kindMatches && titleMatches;
    }
}

public sealed record PlatformDiagramFormConnectionRule(
    string FormKind,
    IReadOnlyList<PlatformDiagramFormTargetCondition> AllowedTargets,
    string ConnectionLabel,
    string Description)
{
    public bool Matches(string targetNodeKind, string targetNodeTitle)
        => AllowedTargets.Count == 0 ||
            AllowedTargets.Any(condition => condition.Matches(targetNodeKind, targetNodeTitle));
}

public static class PlatformDiagramFormKinds
{
    public const string Generic = "generic";
    public const string TransportRequest = "transport-request";
    public const string WarehouseOutbound = "warehouse-outbound";
    public const string WarehouseInbound = "warehouse-inbound";
    public const string TransportPickupConfirmation = "transport-pickup-confirmation";
    public const string TransportDropoffConfirmation = "transport-dropoff-confirmation";
}

public static class PlatformDiagramFormNodeCatalog
{
    public static IReadOnlyList<PlatformDiagramPaletteBlock> All { get; } =
    [
        new(
            "generic-input-form",
            "일반 입력 폼",
            "입력 폼",
            "업무 목적, 필수 입력 항목, 제출 뒤 연결할 단계를 직접 정하는 범용 폼입니다.",
            "form",
            PlatformDiagramFormKinds.Generic),
        new(
            "transport-request-form",
            "운송의뢰 폼",
            "입력 폼",
            "화물, 상차지, 하차지, 희망 시간과 배차 조건을 입력받아 운송 의뢰로 넘깁니다.",
            "form",
            PlatformDiagramFormKinds.TransportRequest),
        new(
            "warehouse-outbound-form",
            "창고 출고 폼",
            "입력 폼",
            "출고 창고, 품목, 수량, 목적지와 희망 출고 시간을 입력받습니다.",
            "form",
            PlatformDiagramFormKinds.WarehouseOutbound),
        new(
            "warehouse-inbound-form",
            "창고 입고 폼",
            "입력 폼",
            "입고 창고, 예정 품목, 수량, 납품·운송 정보를 입력받습니다.",
            "form",
            PlatformDiagramFormKinds.WarehouseInbound),
        new(
            "transport-pickup-confirmation-form",
            "상차 확인 폼",
            "입력 폼",
            "운송 원장, 상차 시각, 적재 상태와 상차 증빙을 입력받습니다.",
            "form",
            PlatformDiagramFormKinds.TransportPickupConfirmation),
        new(
            "transport-dropoff-confirmation-form",
            "하차 확인 폼",
            "입력 폼",
            "운송 원장, 하차 시각, 인수 상태와 하차 증빙을 입력받습니다.",
            "form",
            PlatformDiagramFormKinds.TransportDropoffConfirmation)
    ];

    public static IReadOnlyList<PlatformDiagramFormConnectionRule> ConnectionRules { get; } =
    [
        new(
            PlatformDiagramFormKinds.Generic,
            [],
            "폼 제출",
            "연결점 방향을 만족하는 모든 업무 노드에 연결할 수 있습니다."),
        new(
            PlatformDiagramFormKinds.TransportRequest,
            [
                new("delivery", []),
                new(null, ["운송", "배차", "상차"])
            ],
            "운송 의뢰 제출",
            "운송/전달 종류이거나 제목에 '운송', '배차', '상차'가 있는 노드 중 하나에 연결할 수 있습니다."),
        new(
            PlatformDiagramFormKinds.WarehouseOutbound,
            [
                new("warehouse", []),
                new(null, ["창고", "출고", "피킹", "포장"])
            ],
            "출고 요청 제출",
            "창고 종류이거나 제목에 '창고', '출고', '피킹', '포장'이 있는 노드 중 하나에 연결할 수 있습니다."),
        new(
            PlatformDiagramFormKinds.WarehouseInbound,
            [
                new("warehouse", []),
                new(null, ["창고", "입고", "검수", "하역"])
            ],
            "입고 요청 제출",
            "창고 종류이거나 제목에 '창고', '입고', '검수', '하역'이 있는 노드 중 하나에 연결할 수 있습니다."),
        new(
            PlatformDiagramFormKinds.TransportPickupConfirmation,
            [
                new(null, ["상차", "적재", "인계"])
            ],
            "상차 확인 제출",
            "제목에 '상차', '적재', '인계' 중 하나가 있는 노드에 연결할 수 있습니다."),
        new(
            PlatformDiagramFormKinds.TransportDropoffConfirmation,
            [
                new(null, ["하차", "인수", "도착"])
            ],
            "하차 확인 제출",
            "제목에 '하차', '인수', '도착' 중 하나가 있는 노드에 연결할 수 있습니다.")
    ];

    public static PlatformDiagramFormConnectionRule GetConnectionRule(string? formKind)
        => ConnectionRules.FirstOrDefault(rule =>
               string.Equals(rule.FormKind, formKind, StringComparison.OrdinalIgnoreCase))
           ?? ConnectionRules[0];

    public static bool CanConnect(
        string? formKind,
        string targetNodeKind,
        string targetNodeTitle)
        => GetConnectionRule(formKind).Matches(targetNodeKind, targetNodeTitle);
}
