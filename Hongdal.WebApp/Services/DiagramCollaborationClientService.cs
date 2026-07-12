using System.Net.Http.Json;
using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace Hongdal.WebApp.Services;

public sealed class DiagramCollaborationClientService : IDiagramCollaborationClientService, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private HubConnection? _connection;
    private string? _roomId;

    public DiagramCollaborationClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public event Func<DiagramChatMessageResponse, Task>? 메시지수신;

    public event Func<string, Task>? 상태변경;

    public string 연결상태 => _connection?.State.ToString() ?? "Disconnected";

    public string? 현재사용자Id { get; private set; }

    public string 현재사용자표시명 { get; private set; } = "나";

    public bool 연결됨 => _connection?.State == HubConnectionState.Connected;

    public async Task<bool> 방입장Async(DiagramRoomJoinRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.RoomId))
        {
            await Publish상태Async("다이어그램 대화방 ID가 없어 실시간 연결을 시작하지 않았습니다.");
            return false;
        }

        if (연결됨 && string.Equals(_roomId, request.RoomId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        await 연결해제Async(cancellationToken);

        try
        {
            var baseAddress = _httpClient.BaseAddress
                              ?? throw new InvalidOperationException("Hongdal API BaseAddress가 설정되어 있지 않습니다.");
            var hubUri = new Uri(baseAddress, "hubs/diagram-collaboration");

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUri)
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();

            await _connection.StartAsync(cancellationToken);
            await _connection.InvokeAsync("JoinRoom", request, cancellationToken);
            _roomId = request.RoomId.Trim();
            await Publish상태Async("다이어그램 대화방 SignalR 허브에 연결되었습니다.");
            return true;
        }
        catch (Exception ex)
        {
            await 연결해제Async(cancellationToken);
            await Publish상태Async($"SignalR 대화방 연결에 실패해 로컬 미리보기로 표시합니다. {ex.Message}");
            return false;
        }
    }

    public async Task<IReadOnlyList<DiagramChatMessageResponse>> 메시지목록조회Async(
        string roomId,
        int limit = 80,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            return [];
        }

        try
        {
            var safeLimit = limit <= 0 ? 80 : Math.Min(limit, 200);
            var path = $"api/v1/community/diagram-conversations/{Uri.EscapeDataString(roomId.Trim())}/messages?limit={safeLimit}";
            var response = await _httpClient.GetFromJsonAsync<다이어그램대화메시지목록Response>(path, cancellationToken);
            return response?.Items ?? [];
        }
        catch (Exception ex)
        {
            await Publish상태Async($"저장된 대화 이력을 불러오지 못했습니다. {ex.Message}");
            return [];
        }
    }

    public async Task<bool> 메시지전송Async(DiagramChatMessageRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (_connection?.State != HubConnectionState.Connected)
        {
            await Publish상태Async("실시간 연결 전이라 메시지를 로컬 말풍선으로만 표시합니다.");
            return false;
        }

        try
        {
            await _connection.InvokeAsync("SendMessage", request, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            await Publish상태Async($"메시지 전송에 실패해 로컬 말풍선으로 표시합니다. {ex.Message}");
            return false;
        }
    }

    public async Task 연결해제Async(CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            if (_connection.State == HubConnectionState.Connected && !string.IsNullOrWhiteSpace(_roomId))
            {
                await _connection.InvokeAsync("LeaveRoom", _roomId, cancellationToken);
            }
        }
        catch
        {
            // 연결 종료 중 실패는 사용자가 취할 조치가 없어 조용히 정리합니다.
        }
        finally
        {
            await _connection.DisposeAsync();
            _connection = null;
            _roomId = null;
            현재사용자Id = null;
            현재사용자표시명 = "나";
        }
    }

    public async ValueTask DisposeAsync()
        => await 연결해제Async();

    private void RegisterHandlers()
    {
        if (_connection is null)
        {
            return;
        }

        _connection.On<DiagramRoomJoinedResponse>(
            DiagramCollaborationClientMethods.RoomJoined,
            async joined =>
            {
                현재사용자Id = string.IsNullOrWhiteSpace(joined.UserId) ? joined.ConnectionId : joined.UserId;
                현재사용자표시명 = string.IsNullOrWhiteSpace(joined.DisplayName) ? "나" : joined.DisplayName;
                await Publish상태Async($"{joined.DiagramName} 대화방에 입장했습니다.");
            });

        _connection.On<DiagramChatMessageResponse>(
            DiagramCollaborationClientMethods.ReceiveMessage,
            async message => await Publish메시지Async(message));

        _connection.On<DiagramChatMessageResponse>(
            DiagramCollaborationClientMethods.ParticipantJoined,
            async message => await Publish메시지Async(message));

        _connection.On<DiagramChatMessageResponse>(
            DiagramCollaborationClientMethods.ParticipantLeft,
            async message => await Publish메시지Async(message));

        _connection.On<DiagramSnapshotSharedResponse>(
            DiagramCollaborationClientMethods.ReceiveSnapshot,
            async snapshot => await Publish메시지Async(new DiagramChatMessageResponse
            {
                MessageId = snapshot.ShareId,
                RoomId = snapshot.RoomId,
                SenderUserId = snapshot.SenderUserId,
                SenderDisplayName = snapshot.SenderDisplayName,
                Message = string.IsNullOrWhiteSpace(snapshot.Message)
                    ? $"{snapshot.Snapshot.DiagramName} 다이어그램을 공유했습니다."
                    : snapshot.Message,
                DiagramId = snapshot.Snapshot.DiagramId,
                DiagramName = snapshot.Snapshot.DiagramName,
                MessageKind = DiagramCollaborationMessageKinds.DiagramNote,
                SentAtUtc = snapshot.SharedAtUtc
            }));

        _connection.On<DiagramWorkActionResponse>(
            DiagramCollaborationClientMethods.ReceiveWorkAction,
            async action => await Publish메시지Async(new DiagramChatMessageResponse
            {
                MessageId = action.ActionId,
                RoomId = action.RoomId,
                SenderUserId = action.SenderUserId,
                SenderDisplayName = action.SenderDisplayName,
                Message = $"{action.ActionLabel} 업무 화면을 열 수 있습니다.",
                DiagramId = action.DiagramId,
                MessageKind = DiagramCollaborationMessageKinds.DiagramNote,
                SentAtUtc = action.RequestedAtUtc
            }));

        _connection.Reconnecting += async error =>
        {
            await Publish상태Async(error is null
                ? "다이어그램 대화방 재연결을 시도합니다."
                : $"다이어그램 대화방 연결이 끊겼습니다. 재연결을 시도합니다. {error.Message}");
        };

        _connection.Reconnected += async _ =>
        {
            await Publish상태Async("다이어그램 대화방에 다시 연결되었습니다.");
        };

        _connection.Closed += async error =>
        {
            await Publish상태Async(error is null
                ? "다이어그램 대화방 연결이 종료되었습니다."
                : $"다이어그램 대화방 연결이 종료되었습니다. {error.Message}");
        };
    }

    private async Task Publish메시지Async(DiagramChatMessageResponse message)
    {
        var handler = 메시지수신;
        if (handler is null)
        {
            return;
        }

        foreach (Func<DiagramChatMessageResponse, Task> callback in handler.GetInvocationList())
        {
            await callback(message);
        }
    }

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
