using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.Community;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

public interface I커뮤니티활동유료상세UseCase
{
    Task<Result<커뮤니티활동유료상세Response>> 등록Async(
        커뮤니티활동유료상세등록Request? request,
        CancellationToken cancellationToken);

    Task<Result<커뮤니티활동유료상세Response>> 조회Async(
        string 상세Id,
        bool 상세내용필수,
        CancellationToken cancellationToken);

    Task<Result<커뮤니티활동유료상세Response>> 게시글별조회Async(
        long 게시글Id,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<커뮤니티활동상세열람권Response>>> 내열람권목록Async(
        CancellationToken cancellationToken);

    Task<Result<커뮤니티활동상세FakePg결제승인Response>> 페이크결제승인Async(
        string 상세Id,
        커뮤니티활동상세FakePg결제승인Request? request,
        CancellationToken cancellationToken);

    Task<Result<커뮤니티활동상세구매WorkflowResponse>> 구매조회Async(
        string 구매Id,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<커뮤니티활동상세구매WorkflowResponse>>> 내구매목록Async(
        CancellationToken cancellationToken);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "커뮤니티 게시글 활동의 공개 미리보기와 유료 상세 열람권을 분리해 기록",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "개발용 FakePG 승인만 허용하며 실제 카드 승인, 판매자 정산과 외부 금전 이동을 수행하지 않습니다.")]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelUseCase("커뮤니티 활동 유료 상세", Summary = "게시글 작성자가 상세 자료를 등록하고 다른 사용자가 FakePG 구매 뒤 영구 열람권으로 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityActivityPaidDetail,
    SsalddelCodeLayer.Application,
    "작성자 등록 권한과 구매 후 상세 열람 권한을 검증하고 구매 프로세스 조율을 위임합니다.",
    ContractType = typeof(I커뮤니티활동유료상세UseCase),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "상세 본문은 작성자 또는 활성 열람권 보유자에게만 반환합니다.")]
public sealed class 커뮤니티활동유료상세UseCase : I커뮤니티활동유료상세UseCase
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I커뮤니티활동상세구매ProcessManager _구매ProcessManager;

    public 커뮤니티활동유료상세UseCase(
        SsalddelContext db,
        ICurrentUserAccessor currentUserAccessor,
        I커뮤니티활동상세구매ProcessManager 구매ProcessManager)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
        _구매ProcessManager = 구매ProcessManager;
    }

    public async Task<Result<커뮤니티활동유료상세Response>> 등록Async(
        커뮤니티활동유료상세등록Request? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<커뮤니티활동유료상세Response>("request body is required");
        }

        var 판매자UserId = Normalize(_currentUserAccessor.UserId);
        if (string.IsNullOrWhiteSpace(판매자UserId))
        {
            return Unauthorized<커뮤니티활동유료상세Response>("인증 정보가 필요합니다.");
        }

        var validation = 커뮤니티활동유료상세Policy.등록검증(
            request.게시글Id,
            request.공개미리보기,
            request.상세내용,
            request.가격금액,
            request.통화Code);
        if (!validation.허용)
        {
            return BadRequest<커뮤니티활동유료상세Response>(validation.메시지);
        }

        var post = await _db.PlatformCommunityPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.게시글Id && !x.IsDeleted, cancellationToken);
        if (post is null)
        {
            return NotFound<커뮤니티활동유료상세Response>("커뮤니티 활동 게시글을 찾을 수 없습니다.");
        }

        if (!string.Equals(post.AuthorUserId, 판매자UserId, StringComparison.Ordinal))
        {
            return Forbidden<커뮤니티활동유료상세Response>("작성자 본인만 활동 상세를 판매 등록할 수 있습니다.");
        }

        if (await _db.커뮤니티활동유료상세목록.AnyAsync(x => x.게시글Id == post.Id, cancellationToken))
        {
            return Conflict<커뮤니티활동유료상세Response>("이 활동 게시글에는 이미 유료 상세가 등록되어 있습니다.");
        }

        var now = DateTime.UtcNow;
        var entity = new 커뮤니티활동유료상세
        {
            상세Id = $"community-detail-{Guid.NewGuid():N}",
            게시글Id = post.Id,
            판매자UserId = 판매자UserId,
            공개미리보기 = request.공개미리보기.Trim(),
            상세내용 = request.상세내용.Trim(),
            가격금액 = request.가격금액,
            통화Code = request.통화Code.Trim().ToUpperInvariant(),
            판매상태 = 커뮤니티활동유료상세판매상태.판매중,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.커뮤니티활동유료상세목록.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(ToResponse(entity, post, true, 커뮤니티활동상세열람근거.작성자본인));
    }

    public async Task<Result<커뮤니티활동유료상세Response>> 조회Async(
        string 상세Id,
        bool 상세내용필수,
        CancellationToken cancellationToken)
    {
        var entity = await _db.커뮤니티활동유료상세목록
            .AsNoTracking()
            .Include(x => x.게시글)
            .FirstOrDefaultAsync(x => x.상세Id == 상세Id, cancellationToken);
        if (entity is null || entity.게시글.IsDeleted)
        {
            return NotFound<커뮤니티활동유료상세Response>("유료 활동 상세를 찾을 수 없습니다.");
        }

        var access = await ResolveAccessAsync(entity, cancellationToken);
        if (상세내용필수 && !access.열람가능)
        {
            return Forbidden<커뮤니티활동유료상세Response>("이 상세 내용을 열람하려면 구매가 필요합니다.");
        }

        return Result.Ok(ToResponse(entity, entity.게시글, access.열람가능, access.열람근거));
    }

    public async Task<Result<커뮤니티활동유료상세Response>> 게시글별조회Async(
        long 게시글Id,
        CancellationToken cancellationToken)
    {
        var entity = await _db.커뮤니티활동유료상세목록
            .AsNoTracking()
            .Include(x => x.게시글)
            .FirstOrDefaultAsync(x => x.게시글Id == 게시글Id, cancellationToken);
        if (entity is null || entity.게시글.IsDeleted)
        {
            return NotFound<커뮤니티활동유료상세Response>("이 커뮤니티 활동에는 등록된 유료 상세가 없습니다.");
        }

        var access = await ResolveAccessAsync(entity, cancellationToken);
        return Result.Ok(ToResponse(entity, entity.게시글, access.열람가능, access.열람근거));
    }

    public async Task<Result<IReadOnlyList<커뮤니티활동상세열람권Response>>> 내열람권목록Async(
        CancellationToken cancellationToken)
    {
        var 구매자UserId = Normalize(_currentUserAccessor.UserId);
        if (string.IsNullOrWhiteSpace(구매자UserId))
        {
            return Unauthorized<IReadOnlyList<커뮤니티활동상세열람권Response>>("인증 정보가 필요합니다.");
        }

        var entities = await _db.커뮤니티활동상세열람권목록
            .AsNoTracking()
            .Where(x => x.구매자UserId == 구매자UserId)
            .OrderByDescending(x => x.발급일시Utc)
            .ToArrayAsync(cancellationToken);
        var items = entities.Select(ToEntitlementResponse).ToArray();

        return Result.Ok<IReadOnlyList<커뮤니티활동상세열람권Response>>(items);
    }

    public Task<Result<커뮤니티활동상세FakePg결제승인Response>> 페이크결제승인Async(
        string 상세Id,
        커뮤니티활동상세FakePg결제승인Request? request,
        CancellationToken cancellationToken)
        => _구매ProcessManager.FakePg구매Async(상세Id, request, cancellationToken);

    public Task<Result<커뮤니티활동상세구매WorkflowResponse>> 구매조회Async(
        string 구매Id,
        CancellationToken cancellationToken)
        => _구매ProcessManager.구매조회Async(구매Id, cancellationToken);

    public Task<Result<IReadOnlyList<커뮤니티활동상세구매WorkflowResponse>>> 내구매목록Async(
        CancellationToken cancellationToken)
        => _구매ProcessManager.내구매목록Async(cancellationToken);

    private async Task<(bool 열람가능, string 열람근거)> ResolveAccessAsync(
        커뮤니티활동유료상세 detail,
        CancellationToken cancellationToken)
    {
        var userId = Normalize(_currentUserAccessor.UserId);
        if (!string.IsNullOrWhiteSpace(userId)
            && string.Equals(detail.판매자UserId, userId, StringComparison.Ordinal))
        {
            return (true, 커뮤니티활동상세열람근거.작성자본인);
        }

        if (!string.IsNullOrWhiteSpace(userId)
            && await _db.커뮤니티활동상세열람권목록.AsNoTracking().AnyAsync(
                x => x.상세Id == detail.상세Id
                     && x.구매자UserId == userId
                     && x.상태 == 커뮤니티활동상세열람권상태.활성,
                cancellationToken))
        {
            return (true, 커뮤니티활동상세열람근거.구매);
        }

        return (false, 커뮤니티활동상세열람근거.구매필요);
    }

    private static 커뮤니티활동유료상세Response ToResponse(
        커뮤니티활동유료상세 detail,
        PlatformCommunityPost post,
        bool canRead,
        string accessBasis)
        => new()
        {
            상세Id = detail.상세Id,
            게시글Id = detail.게시글Id,
            게시글제목 = post.Title,
            판매자표시명 = post.Nickname,
            공개미리보기 = detail.공개미리보기,
            상세내용 = canRead ? detail.상세내용 : null,
            가격금액 = detail.가격금액,
            통화Code = detail.통화Code,
            판매상태 = detail.판매상태,
            열람가능 = canRead,
            열람근거 = accessBasis
        };

    private static 커뮤니티활동상세열람권Response ToEntitlementResponse(커뮤니티활동상세열람권 entity)
        => new()
        {
            열람권Id = entity.열람권Id,
            상세Id = entity.상세Id,
            구매자UserId = entity.구매자UserId,
            결제Id = entity.결제Id,
            상태 = entity.상태,
            발급일시Utc = entity.발급일시Utc
        };

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    private static Result<T> BadRequest<T>(string message) => Fail<T>(message, StatusCodes.Status400BadRequest);
    private static Result<T> Unauthorized<T>(string message) => Fail<T>(message, StatusCodes.Status401Unauthorized);
    private static Result<T> Forbidden<T>(string message) => Fail<T>(message, StatusCodes.Status403Forbidden);
    private static Result<T> NotFound<T>(string message) => Fail<T>(message, StatusCodes.Status404NotFound);
    private static Result<T> Conflict<T>(string message) => Fail<T>(message, StatusCodes.Status409Conflict);
    private static Result<T> Fail<T>(string message, int statusCode)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", statusCode));
}
