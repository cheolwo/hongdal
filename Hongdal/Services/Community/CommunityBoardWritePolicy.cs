using Hongdal.Contracts.Common.Community;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Services.Community;

public interface ICommunityBoardWritePolicy
{
    Task<bool> CanWriteAsync(
        string? appKey,
        string? category,
        CancellationToken cancellationToken = default);
}

public sealed class CommunityBoardWritePolicy(HongdalContext db) : ICommunityBoardWritePolicy
{
    public async Task<bool> CanWriteAsync(
        string? appKey,
        string? category,
        CancellationToken cancellationToken = default)
    {
        var normalizedCategory = category?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedCategory))
        {
            return false;
        }

        var catalogBoard = CommunityBoardCatalog.Find(normalizedCategory);
        if (catalogBoard is not null)
        {
            return (catalogBoard.IsPublic && catalogBoard.IsUserCreatable)
                   || catalogBoard.Key == CommunityBoardKeys.SafetyReport;
        }

        var normalizedAppKey = string.IsNullOrWhiteSpace(appKey)
            ? "platform"
            : appKey.Trim();
        return await db.PlatformCommunityBoardRequests
            .AsNoTracking()
            .AnyAsync(board => !board.IsDeleted
                               && board.Status == PlatformCommunityBoardRequestStatuses.Approved
                               && board.Title == normalizedCategory
                               && (board.AppKey == normalizedAppKey || board.AppKey == "platform"),
                cancellationToken);
    }
}
