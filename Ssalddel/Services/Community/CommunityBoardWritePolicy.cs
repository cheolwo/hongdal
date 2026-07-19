using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

public interface ICommunityBoardWritePolicy
{
    Task<bool> CanWriteAsync(
        string? appKey,
        string? category,
        string? userId,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Safety,
    SsalddelModuleKind.Application,
    "게시판 공개 범위·익명 허용·인증 요구·운영 승인 상태에 따라 쓰기 가능 여부를 판정",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.SafetyAndOperations,
    Boundary = "client 표시 상태와 무관하게 서버가 쓰기 권한을 다시 판정합니다.")]
public sealed class CommunityBoardWritePolicy(SsalddelContext db) : ICommunityBoardWritePolicy
{
    public async Task<bool> CanWriteAsync(
        string? appKey,
        string? category,
        string? userId,
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
            var userPostingEnabled = (catalogBoard.IsPublic && catalogBoard.IsUserCreatable)
                                     || catalogBoard.Key == CommunityBoardKeys.SafetyReport;
            if (!userPostingEnabled)
            {
                return false;
            }

            return catalogBoard.AllowsAnonymousPosting
                   || (catalogBoard.RequiresAuthenticatedPosting
                       && !string.IsNullOrWhiteSpace(userId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
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
