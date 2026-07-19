using Ssalddel.Contracts.Common.Community;
using Ssalddel.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ssalddel.Services.Community;

public interface I원장다이어그램실시간알림Service
{
    Task 변경알림Async(
        커뮤니티원장Dto 원장,
        string? 변경블록Id,
        CancellationToken cancellationToken = default);
}

public sealed class 원장다이어그램SignalR알림Service(
    IHubContext<DiagramCollaborationHub> hubContext,
    ILogger<원장다이어그램SignalR알림Service> logger) : I원장다이어그램실시간알림Service
{
    public async Task 변경알림Async(
        커뮤니티원장Dto 원장,
        string? 변경블록Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(원장);

        try
        {
            var roomId = DiagramLedgerRoomIds.Build(원장.원장Id);
            await hubContext.Clients
                .Group(DiagramCollaborationHub.BuildRoomGroup(roomId))
                .SendAsync(
                    DiagramCollaborationClientMethods.ReceiveLedgerChanged,
                    new DiagramLedgerChangedResponse
                    {
                        LedgerId = 원장.원장Id,
                        Revision = 원장.Revision,
                        State = 원장.상태,
                        CurrentStep = 원장.현재단계Key,
                        NodeId = 변경블록Id,
                        ChangedAtUtc = 원장.수정시각Utc == default ? DateTime.UtcNow : 원장.수정시각Utc
                    },
                    cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "원장 다이어그램 실시간 알림에 실패했습니다. 원장Id={LedgerId}", 원장.원장Id);
        }
    }
}
