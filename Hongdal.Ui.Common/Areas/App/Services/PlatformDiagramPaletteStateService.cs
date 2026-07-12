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

    public void SetDiagramMode(bool isDiagramMode)
    {
        if (IsDiagramMode == isDiagramMode)
        {
            return;
        }

        IsDiagramMode = isDiagramMode;
        Changed?.Invoke();
    }

    public void SetWorkflowMode(string? workflowModeKey)
    {
        var nextMode = string.IsNullOrWhiteSpace(workflowModeKey)
            ? CommonWorkflowModeKey
            : workflowModeKey.Trim();
        if (string.Equals(WorkflowModeKey, nextMode, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        WorkflowModeKey = nextMode;
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
    string Kind);

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
