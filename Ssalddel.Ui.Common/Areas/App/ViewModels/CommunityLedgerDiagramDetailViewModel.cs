using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Components.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class CommunityLedgerDiagramDetailViewModel : ObservableObject, IAsyncDisposable
{
    private readonly IDiagramCollaborationClientService _collaborationClient;
    private PlatformCommunityPostLedgerContextResponse? _context;
    private string? _selectedNodeId;
    private string? _joinedLedgerRoomId;
    private string _realtimeStatus = "실시간 연결 준비 중";
    private bool _realtimeConnected;
    private bool _isJoiningLedgerRoom;
    private bool _isRefreshing;
    private bool _disposed;

    public CommunityLedgerDiagramDetailViewModel(IDiagramCollaborationClientService collaborationClient)
    {
        _collaborationClient = collaborationClient;
        _collaborationClient.원장변경수신 += HandleLedgerChangedAsync;
        _collaborationClient.상태변경 += HandleRealtimeStatusChangedAsync;
    }

    public event Func<Task>? RefreshRequested;

    public PlatformCommunityPostLedgerContextResponse? Context
    {
        get => _context;
        private set
        {
            _context = value;
            OnPropertyChanged();
            RaiseSelectionProperties();
        }
    }

    public string? SelectedNodeId
    {
        get => _selectedNodeId;
        private set
        {
            if (SetProperty(ref _selectedNodeId, value))
            {
                RaiseSelectionProperties();
            }
        }
    }

    public DiagramNodeDto? SelectedNode
        => CommunityLedgerDiagramPresentation.FindNode(Context, SelectedNodeId);

    public PlatformCommunityLedgerBlockResponse? SelectedBlock
        => CommunityLedgerDiagramPresentation.FindBlock(Context, SelectedNodeId);

    public IReadOnlyList<PlatformCommunityLedgerNodeActionResponse> SelectedNodeActions
        => CommunityLedgerDiagramPresentation.FindNodeActions(Context, SelectedNodeId);

    public string RealtimeStatus
    {
        get => _realtimeStatus;
        private set => SetProperty(ref _realtimeStatus, value);
    }

    public bool RealtimeConnected
    {
        get => _realtimeConnected;
        private set => SetProperty(ref _realtimeConnected, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        private set => SetProperty(ref _isRefreshing, value);
    }

    public async Task ApplyContextAsync(
        PlatformCommunityPostLedgerContextResponse? context,
        CancellationToken cancellationToken = default)
    {
        var previousLedgerId = Context?.원장Id;
        Context = context;
        if (context is null)
        {
            SelectedNodeId = null;
            return;
        }

        var selectedNodeStillExists = CommunityLedgerDiagramPresentation.FindNode(context, SelectedNodeId) is not null;
        if (!string.Equals(previousLedgerId, context.원장Id, StringComparison.OrdinalIgnoreCase)
            || !selectedNodeStillExists)
        {
            SelectedNodeId = context.다이어그램?.Nodes.FirstOrDefault()?.NodeId;
        }

        await JoinLedgerRoomAsync(context, cancellationToken);
    }

    public void SelectNode(string nodeId)
    {
        if (CommunityLedgerDiagramPresentation.FindNode(Context, nodeId) is not null)
        {
            SelectedNodeId = nodeId;
        }
    }

    public async Task RequestRefreshAsync()
    {
        var handler = RefreshRequested;
        if (IsRefreshing || handler is null)
        {
            return;
        }

        IsRefreshing = true;
        try
        {
            foreach (Func<Task> callback in handler.GetInvocationList())
            {
                await callback();
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _collaborationClient.원장변경수신 -= HandleLedgerChangedAsync;
        _collaborationClient.상태변경 -= HandleRealtimeStatusChangedAsync;
        if (!string.IsNullOrWhiteSpace(_joinedLedgerRoomId))
        {
            await _collaborationClient.연결해제Async();
        }
    }

    private async Task JoinLedgerRoomAsync(
        PlatformCommunityPostLedgerContextResponse context,
        CancellationToken cancellationToken)
    {
        var roomId = DiagramLedgerRoomIds.Build(context.원장Id);
        if (string.Equals(_joinedLedgerRoomId, roomId, StringComparison.OrdinalIgnoreCase)
            && _collaborationClient.연결됨)
        {
            RealtimeConnected = true;
            RealtimeStatus = "원장 변경 실시간 반영 중";
            return;
        }

        if (_isJoiningLedgerRoom)
        {
            return;
        }

        _isJoiningLedgerRoom = true;
        try
        {
            RealtimeStatus = "실시간 연결 중";
            RealtimeConnected = await _collaborationClient.방입장Async(new DiagramRoomJoinRequest
            {
                RoomId = roomId,
                CommunityId = "platform",
                LedgerId = context.원장Id,
                DiagramId = context.다이어그램?.DiagramId,
                DiagramName = context.다이어그램?.DiagramName ?? context.제목,
                LedgerTemplateKey = context.원장템플릿Key
            }, cancellationToken);
            _joinedLedgerRoomId = RealtimeConnected ? roomId : null;
            RealtimeStatus = RealtimeConnected ? "원장 변경 실시간 반영 중" : "실시간 연결 대기 중";
        }
        finally
        {
            _isJoiningLedgerRoom = false;
        }
    }

    private async Task HandleLedgerChangedAsync(DiagramLedgerChangedResponse changed)
    {
        var context = Context;
        if (_disposed
            || context is null
            || !string.Equals(context.원장Id, changed.LedgerId, StringComparison.OrdinalIgnoreCase)
            || changed.Revision > 0 && changed.Revision <= context.Revision)
        {
            return;
        }

        RealtimeStatus = string.IsNullOrWhiteSpace(changed.CurrentStep)
            ? "원장 변경을 반영하는 중"
            : $"{changed.CurrentStep} 변경을 반영하는 중";
        await RequestRefreshAsync();
        if (!_disposed)
        {
            RealtimeStatus = "원장 변경 실시간 반영 중";
        }
    }

    private Task HandleRealtimeStatusChangedAsync(string message)
    {
        if (!_disposed)
        {
            RealtimeConnected = _collaborationClient.연결됨;
            RealtimeStatus = RealtimeConnected ? "원장 변경 실시간 반영 중" : message;
        }

        return Task.CompletedTask;
    }

    private void RaiseSelectionProperties()
    {
        OnPropertyChanged(nameof(SelectedNode));
        OnPropertyChanged(nameof(SelectedBlock));
        OnPropertyChanged(nameof(SelectedNodeActions));
    }
}
