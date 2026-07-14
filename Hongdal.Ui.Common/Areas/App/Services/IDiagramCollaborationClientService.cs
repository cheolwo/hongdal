using Hongdal.Contracts.Common.Community;

namespace Hongdal.Ui.Common.Areas.App.Services;

public interface IDiagramCollaborationClientService
{
    event Func<DiagramChatMessageResponse, Task>? 메시지수신;

    event Func<DiagramLedgerChangedResponse, Task>? 원장변경수신;

    event Func<string, Task>? 상태변경;

    string 연결상태 { get; }

    string? 현재사용자Id { get; }

    string 현재사용자표시명 { get; }

    bool 연결됨 { get; }

    Task<bool> 방입장Async(DiagramRoomJoinRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DiagramChatMessageResponse>> 메시지목록조회Async(
        string roomId,
        int limit = 80,
        CancellationToken cancellationToken = default);

    Task<bool> 메시지전송Async(DiagramChatMessageRequest request, CancellationToken cancellationToken = default);

    Task 연결해제Async(CancellationToken cancellationToken = default);
}

public sealed class NoopDiagramCollaborationClientService : IDiagramCollaborationClientService
{
    public static NoopDiagramCollaborationClientService Instance { get; } = new();

    public event Func<DiagramChatMessageResponse, Task>? 메시지수신
    {
        add { }
        remove { }
    }

    public event Func<DiagramLedgerChangedResponse, Task>? 원장변경수신
    {
        add { }
        remove { }
    }

    public event Func<string, Task>? 상태변경;

    public string 연결상태 => "LocalPreview";

    public string? 현재사용자Id => null;

    public string 현재사용자표시명 => "나";

    public bool 연결됨 => false;

    public async Task<bool> 방입장Async(DiagramRoomJoinRequest request, CancellationToken cancellationToken = default)
    {
        await Publish상태Async("SignalR 대화방 클라이언트가 등록되지 않아 로컬 미리보기 모드로 표시합니다.");
        return false;
    }

    public Task<IReadOnlyList<DiagramChatMessageResponse>> 메시지목록조회Async(
        string roomId,
        int limit = 80,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DiagramChatMessageResponse>>([]);

    public async Task<bool> 메시지전송Async(DiagramChatMessageRequest request, CancellationToken cancellationToken = default)
    {
        await Publish상태Async("실시간 연결 전이라 메시지를 로컬 말풍선으로만 표시합니다.");
        return false;
    }

    public Task 연결해제Async(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    private async Task Publish상태Async(string message)
    {
        var handler = 상태변경;
        if (handler is null)
        {
            return;
        }

        foreach (Func<string, Task> callback in handler.GetInvocationList())
        {
            await callback(message);
        }
    }
}
