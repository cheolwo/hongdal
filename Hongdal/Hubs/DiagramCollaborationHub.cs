using System.Security.Claims;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.SignalR;

namespace Hongdal.Hubs;

public sealed class DiagramCollaborationHub : Hub
{
    public const string HubPath = "/hubs/diagram-collaboration";

    private readonly I커뮤니티대화저장소 _대화저장소;

    public DiagramCollaborationHub(I커뮤니티대화저장소 대화저장소)
    {
        _대화저장소 = 대화저장소;
    }

    public async Task JoinRoom(DiagramRoomJoinRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var roomId = RequireRoomId(request.RoomId);
        var groupName = BuildRoomGroup(roomId);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var joined = new DiagramRoomJoinedResponse
        {
            RoomId = roomId,
            ConnectionId = Context.ConnectionId,
            UserId = ResolveUserId(),
            DisplayName = ResolveDisplayName(),
            DiagramName = string.IsNullOrWhiteSpace(request.DiagramName) ? "이름 없는 다이어그램" : request.DiagramName.Trim(),
            LedgerId = Clean(request.LedgerId),
            WorkContext = request.WorkContext,
            JoinedAtUtc = DateTime.UtcNow
        };

        await Clients.Caller.SendAsync(DiagramCollaborationClientMethods.RoomJoined, joined);
        await Clients.OthersInGroup(groupName).SendAsync(
            DiagramCollaborationClientMethods.ParticipantJoined,
            BuildParticipantMessage(roomId, joined.DiagramName, DiagramCollaborationMessageKinds.System));
    }

    public async Task LeaveRoom(string roomId)
    {
        roomId = RequireRoomId(roomId);
        var groupName = BuildRoomGroup(roomId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.OthersInGroup(groupName).SendAsync(
            DiagramCollaborationClientMethods.ParticipantLeft,
            BuildParticipantMessage(roomId, "대화방을 나갔습니다.", DiagramCollaborationMessageKinds.System));
    }

    public async Task SendMessage(DiagramChatMessageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var roomId = RequireRoomId(request.RoomId);
        var message = Clean(request.Message);
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new HubException("메시지 내용이 필요합니다.");
        }

        var saved = await _대화저장소.메시지저장Async(
            new 커뮤니티메시지저장요청
            {
                대화방Id = roomId,
                커뮤니티Id = "platform",
                유형 = 커뮤니티대화방유형.Diagram,
                제목 = Clean(request.DiagramName),
                메시지 = message,
                메시지종류 = string.IsNullOrWhiteSpace(request.MessageKind)
                    ? 커뮤니티메시지종류.Text
                    : request.MessageKind.Trim(),
                다이어그램Id = Clean(request.DiagramId),
                다이어그램이름 = Clean(request.DiagramName)
            },
            ResolveUserId(),
            ResolveDisplayName(),
            Context.ConnectionAborted);

        var response = new DiagramChatMessageResponse
        {
            MessageId = saved.MessageId,
            RoomId = roomId,
            SenderUserId = saved.보낸사람UserId,
            SenderDisplayName = saved.보낸사람표시명,
            Message = saved.메시지,
            DiagramId = saved.다이어그램Id,
            DiagramName = saved.다이어그램이름,
            MessageKind = saved.메시지종류,
            SentAtUtc = saved.생성시각Utc
        };

        await Clients.Group(BuildRoomGroup(roomId)).SendAsync(DiagramCollaborationClientMethods.ReceiveMessage, response);
    }

    public async Task ShareDiagram(DiagramSnapshotShareRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var roomId = RequireRoomId(request.RoomId);
        ValidateSnapshot(request.Snapshot);

        var message = Clean(request.Message) ?? $"{request.Snapshot.DiagramName} 다이어그램 공유";
        var saved = await _대화저장소.메시지저장Async(
            new 커뮤니티메시지저장요청
            {
                대화방Id = roomId,
                커뮤니티Id = "platform",
                유형 = 커뮤니티대화방유형.Diagram,
                제목 = request.Snapshot.DiagramName,
                메시지 = message,
                메시지종류 = 커뮤니티메시지종류.Diagram,
                원장Id = Clean(request.Snapshot.LedgerId),
                원장템플릿Key = Clean(request.Snapshot.LedgerTemplateKey),
                다이어그램Id = request.Snapshot.DiagramId,
                다이어그램이름 = request.Snapshot.DiagramName,
                다이어그램스냅샷 = request.Snapshot
            },
            ResolveUserId(),
            ResolveDisplayName(),
            Context.ConnectionAborted);

        var response = new DiagramSnapshotSharedResponse
        {
            ShareId = saved.MessageId,
            RoomId = roomId,
            SenderUserId = saved.보낸사람UserId,
            SenderDisplayName = saved.보낸사람표시명,
            Snapshot = request.Snapshot,
            Message = saved.메시지,
            SharedAtUtc = saved.생성시각Utc
        };

        await Clients.Group(BuildRoomGroup(roomId)).SendAsync(DiagramCollaborationClientMethods.ReceiveSnapshot, response);
    }

    public async Task RequestWorkAction(DiagramWorkActionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var roomId = RequireRoomId(request.RoomId);
        if (string.IsNullOrWhiteSpace(request.TargetRoute))
        {
            throw new HubException("업무 화면 경로가 필요합니다.");
        }

        var actionLabel = string.IsNullOrWhiteSpace(request.ActionLabel) ? "업무 화면 열기" : request.ActionLabel.Trim();
        var saved = await _대화저장소.메시지저장Async(
            new 커뮤니티메시지저장요청
            {
                대화방Id = roomId,
                커뮤니티Id = "platform",
                유형 = 커뮤니티대화방유형.Diagram,
                제목 = request.WorkContext?.WorkLabel ?? "다이어그램 업무 요청",
                메시지 = actionLabel,
                메시지종류 = 커뮤니티메시지종류.WorkAction,
                원장Id = Clean(request.LedgerId),
                다이어그램Id = Clean(request.DiagramId),
                업무Context = request.WorkContext,
                확장속성 = new Dictionary<string, string>
                {
                    ["ActionCode"] = string.IsNullOrWhiteSpace(request.ActionCode)
                        ? DiagramWorkActionCodes.OpenWorkScreen
                        : request.ActionCode.Trim(),
                    ["TargetRoute"] = request.TargetRoute.Trim(),
                    ["NodeId"] = Clean(request.NodeId) ?? string.Empty
                }
            },
            ResolveUserId(),
            ResolveDisplayName(),
            Context.ConnectionAborted);

        var response = new DiagramWorkActionResponse
        {
            ActionId = saved.MessageId,
            RoomId = roomId,
            SenderUserId = saved.보낸사람UserId,
            SenderDisplayName = saved.보낸사람표시명,
            ActionCode = string.IsNullOrWhiteSpace(request.ActionCode) ? DiagramWorkActionCodes.OpenWorkScreen : request.ActionCode.Trim(),
            ActionLabel = actionLabel,
            TargetRoute = request.TargetRoute.Trim(),
            LedgerId = Clean(request.LedgerId),
            DiagramId = Clean(request.DiagramId),
            NodeId = Clean(request.NodeId),
            WorkContext = request.WorkContext,
            RequestedAtUtc = saved.생성시각Utc
        };

        await Clients.Group(BuildRoomGroup(roomId)).SendAsync(DiagramCollaborationClientMethods.ReceiveWorkAction, response);
    }

    public static string BuildRoomGroup(string roomId)
    {
        roomId = RequireRoomId(roomId);
        var normalized = string.Concat(roomId.Trim().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-'));
        return $"diagram-room-{normalized}";
    }

    private static void ValidateSnapshot(DiagramSnapshotDto snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.DiagramId))
        {
            throw new HubException("다이어그램 ID가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(snapshot.DiagramName))
        {
            throw new HubException("다이어그램 이름이 필요합니다.");
        }
    }

    private DiagramChatMessageResponse BuildParticipantMessage(string roomId, string message, string kind)
        => new()
        {
            MessageId = Guid.NewGuid().ToString("N"),
            RoomId = roomId,
            SenderUserId = ResolveUserId(),
            SenderDisplayName = ResolveDisplayName(),
            Message = message,
            MessageKind = kind,
            SentAtUtc = DateTime.UtcNow
        };

    private string ResolveUserId()
        => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? Context.UserIdentifier
           ?? Context.ConnectionId;

    private string ResolveDisplayName()
        => Context.User?.Identity?.Name
           ?? Context.User?.FindFirstValue("name")
           ?? "익명 참여자";

    private static string RequireRoomId(string? roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            throw new HubException("다이어그램 대화방 ID가 필요합니다.");
        }

        return roomId.Trim();
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
