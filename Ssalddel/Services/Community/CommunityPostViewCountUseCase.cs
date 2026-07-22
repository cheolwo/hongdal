using Ssalddel.Contracts.Common.Metadata;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "공개 게시글 상세 조회가 성립할 때 누적 조회수를 원자적으로 기록",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "게시글 본문이나 참여 상태를 변경하지 않고 공개 상세 조회수만 증가시킵니다.")]
public sealed class 커뮤니티게시글조회수기록UseCase : I커뮤니티게시글조회수기록UseCase
{
    private readonly SsalddelContext _db;

    public 커뮤니티게시글조회수기록UseCase(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<bool> 조회기록Async(
        long id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return false;
        }

        var affectedRows = await _db.PlatformCommunityPosts
            .Where(post => post.Id == id && !post.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    post => post.ViewCount,
                    post => post.ViewCount + 1),
                cancellationToken);

        return affectedRows == 1;
    }
}
