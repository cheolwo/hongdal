using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using 살뜰.도메인.공통;

namespace Ssalddel.Hubs;

[Authorize]
public sealed class TransportRequestLedgerHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }
        if (Context.User?.IsInRole(역할명.서버관리자) == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(userId));
        }
        if (Context.User?.IsInRole(역할명.서버관리자) == true)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);
        }

        await base.OnDisconnectedAsync(exception);
    }

    internal static string UserGroup(string userId) => $"transport-ledger-user-{userId.Trim()}";
    internal const string AdminGroup = "transport-ledger-admins";

    private string? GetUserId()
        => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
