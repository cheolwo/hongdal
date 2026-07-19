using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Application.Community;
using Hongdal.Application.CommandProcessing;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Metadata;
using Hongdal.Domain.Community;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.Json;
using 홍달.Data;
using 홍달.Services.External.Google;
using 홍달.Services.Options;

namespace Hongdal.Services.Community;

public interface I커뮤니티게시글UseCase
{
    Task<Result<PlatformCommunityPostListResponse>> 목록Async(
        string? appKey,
        string? category,
        string? boardKey,
        string? workflowTag,
        string? roleTag,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CommunityBoardSummaryResponse>>> 게시판요약목록Async(
        string? appKey,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostResponse>> 생성Async(
        PlatformCommunityPostCreateRequest? request,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostResponse>> 예약Async(
        PlatformCommunityPostScheduleCreateRequest? request,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<PlatformCommunityPostResponse>>> 예약목록Async(
        string? status,
        int take,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostResponse>> 예약취소Async(
        long id,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostResponse>> 상세Async(long id, CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostAttachmentResponse>> 첨부업로드Async(
        long id,
        커뮤니티게시글첨부업로드Command? command,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostResponse>> 수정Async(
        long id,
        PlatformCommunityPostUpdateRequest? request,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostResponse>> 운영자고정Async(
        long id,
        PlatformCommunityPostOperatorPinRequest? request,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostResponse>> 추천Async(
        long id,
        PlatformCommunityPostRecommendationRequest? request,
        string fallbackRecommenderKey,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<PlatformCommunityPostCommentResponse>>> 댓글목록Async(
        long id,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostCommentResponse>> 댓글작성Async(
        long id,
        PlatformCommunityPostCommentCreateRequest? request,
        CancellationToken cancellationToken);

    Task<Result> 댓글삭제Async(
        long id,
        long commentId,
        PlatformCommunityPostPasswordRequest? request,
        CancellationToken cancellationToken);

    Task<Result> 댓글신고Async(long commentId, CancellationToken cancellationToken);

    Task<Result> 댓글운영자숨김Async(
        long commentId,
        PlatformCommunityOperatorHiddenRequest? request,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse>>> 첨부댓글목록Async(
        long attachmentId,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostAttachmentCommentResponse>> 첨부댓글작성Async(
        long attachmentId,
        PlatformCommunityPostAttachmentCommentCreateRequest? request,
        CancellationToken cancellationToken);

    Task<Result> 첨부댓글삭제Async(
        long attachmentId,
        long commentId,
        PlatformCommunityPostPasswordRequest? request,
        CancellationToken cancellationToken);

    Task<Result> 첨부댓글신고Async(long commentId, CancellationToken cancellationToken);

    Task<Result> 첨부댓글운영자숨김Async(
        long commentId,
        PlatformCommunityOperatorHiddenRequest? request,
        CancellationToken cancellationToken);

    Task<Result> 삭제Async(
        long id,
        PlatformCommunityPostPasswordRequest? request,
        CancellationToken cancellationToken);
}

public sealed record 커뮤니티게시글첨부업로드Command(
    string Password,
    Stream FileStream,
    string FileName,
    string ContentType,
    long Length);

[HongdalCommunityV0Module(
    HongdalCommunityV0ModuleKeys.Content,
    HongdalModuleKind.Application,
    "게시글·댓글·첨부·추천·신고·예약 발행의 권한과 영속 상태를 처리",
    ReleaseStage = HongdalCommunityV0ReleaseStages.Persistence,
    Boundary = "게시글은 정보와 참여 의사를 기록하며 특정 상대 추천, 계약 대리, 배차 확정 또는 대금 정산을 수행하지 않습니다.")]
[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalUseCase("커뮤니티 게시글 운영", Summary = "참여자가 워크플로우/역할 태그가 붙은 게시글, 첨부, 댓글, 추천, 신고를 처리합니다.")]
[HongdalUseCaseActor(HongdalActor.CommunityMember)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "커뮤니티투표UseCase",
    Condition = "게시글 토론이 투표, 결의문, 전자서명 필요 상태로 발전하는 경우",
    Summary = "커뮤니티 게시글을 투표와 결의문 작성 흐름으로 확장합니다.")]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "인연스냅샷조회UseCase",
    Condition = "게시글 작성자 또는 참여자의 업무 관계 신뢰 신호를 함께 보여주는 경우",
    Summary = "게시글의 역할 태그와 활동 신호를 업무 인연 스냅샷 조회로 확장합니다.")]
public sealed class 커뮤니티게시글UseCase : I커뮤니티게시글UseCase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HongdalContext _db;
    private readonly IGoogleCloudStorageService _storageService;
    private readonly CommunityPostStorageOptions _storageOptions;
    private readonly I커뮤니티게시글음성작업예약Service _음성작업예약Service;
    private readonly ICommunityKeywordNotificationQueue _keywordNotificationQueue;
    private readonly I게시글원장ContextService _원장ContextService;
    private readonly ICommunityBoardWritePolicy _boardWritePolicy;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IPublisher _publisher;
    private readonly ILogger<커뮤니티게시글UseCase> _logger;

    public 커뮤니티게시글UseCase(
        HongdalContext db,
        IGoogleCloudStorageService storageService,
        IOptions<CommunityPostStorageOptions> storageOptions,
        I커뮤니티게시글음성작업예약Service 음성작업예약Service,
        ICommunityKeywordNotificationQueue keywordNotificationQueue,
        I게시글원장ContextService 원장ContextService,
        ICommunityBoardWritePolicy boardWritePolicy,
        ICurrentUserAccessor currentUserAccessor,
        IPublisher publisher,
        ILogger<커뮤니티게시글UseCase> logger)
    {
        _db = db;
        _storageService = storageService;
        _storageOptions = storageOptions.Value;
        _음성작업예약Service = 음성작업예약Service;
        _keywordNotificationQueue = keywordNotificationQueue;
        _원장ContextService = 원장ContextService;
        _boardWritePolicy = boardWritePolicy;
        _currentUserAccessor = currentUserAccessor;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result<PlatformCommunityPostListResponse>> 목록Async(
        string? appKey,
        string? category,
        string? boardKey,
        string? workflowTag,
        string? roleTag,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var protectedCategoryNames = CommunityBoardCatalog
            .CategoryNamesFor(CommunityBoardKeys.SafetyReport);
        var query = _db.PlatformCommunityPosts
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && !x.IsReportBoardPost
                        && !protectedCategoryNames.Contains(x.Category));

        if (!string.IsNullOrWhiteSpace(appKey))
        {
            var normalizedAppKey = Normalize(appKey, "platform", 80);
            query = query.Where(x => x.AppKey == normalizedAppKey || x.AppKey == "platform");
        }

        if (!string.IsNullOrWhiteSpace(boardKey))
        {
            var boardCategoryNames = await ResolveBoardCategoryNamesAsync(
                appKey,
                boardKey,
                cancellationToken);
            query = query.Where(x => boardCategoryNames.Contains(x.Category));
        }
        else if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryNames = CommunityBoardCatalog.CategoryNamesFor(category);
            query = query.Where(x => categoryNames.Contains(x.Category));
        }

        if (!string.IsNullOrWhiteSpace(workflowTag))
        {
            var normalizedWorkflowTag = Normalize(workflowTag, string.Empty, 60);
            query = query.Where(x => x.WorkflowTag == normalizedWorkflowTag);
        }

        if (!string.IsNullOrWhiteSpace(roleTag))
        {
            var normalizedRoleTag = Normalize(roleTag, string.Empty, 40);
            query = query.Where(x => x.RoleTag == normalizedRoleTag);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Comments)
            .Include(x => x.Comments.Where(comment => !comment.IsDeleted && !comment.IsOperatorHidden))
            .OrderByDescending(x => x.IsOperatorPinned)
            .ThenByDescending(x => x.OperatorPinnedAtUtc)
            .ThenByDescending(x => x.IsCommunityMomentumPromoted)
            .ThenByDescending(x => x.CommunityMomentumUpdatedAtUtc)
            .ThenByDescending(x => x.RecommendationCount)
            .ThenByDescending(x => x.LastEngagedAtUtc)
            .ThenByDescending(x => x.PublishedAtUtc ?? x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return Result.Ok(new PlatformCommunityPostListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<IReadOnlyList<CommunityBoardSummaryResponse>>> 게시판요약목록Async(
        string? appKey,
        CancellationToken cancellationToken)
    {
        var protectedCategoryNames = CommunityBoardCatalog
            .CategoryNamesFor(CommunityBoardKeys.SafetyReport);
        var query = _db.PlatformCommunityPosts
            .AsNoTracking()
            .Where(post => !post.IsDeleted
                           && !post.IsReportBoardPost
                           && !protectedCategoryNames.Contains(post.Category));

        var normalizedAppKey = string.IsNullOrWhiteSpace(appKey)
            ? null
            : Normalize(appKey, "platform", 80);
        if (normalizedAppKey is not null)
        {
            query = query.Where(post => post.AppKey == normalizedAppKey || post.AppKey == "platform");
        }

        var categoryCounts = await query
            .GroupBy(post => post.Category)
            .Select(group => new CommunityBoardCategoryCount(
                group.Key,
                group.Count(),
                group.Max(post => post.PublishedAtUtc ?? post.CreatedAtUtc)))
            .ToListAsync(cancellationToken);

        var summaries = CommunityBoardCatalog.PublicBoards
            .Select(board => BuildBoardSummary(board, categoryCounts))
            .ToList();

        var customBoardsQuery = _db.PlatformCommunityBoardRequests
            .AsNoTracking()
            .Where(board => !board.IsDeleted
                            && board.Status == PlatformCommunityBoardRequestStatuses.Approved);
        if (normalizedAppKey is not null)
        {
            customBoardsQuery = customBoardsQuery.Where(board =>
                board.AppKey == normalizedAppKey || board.AppKey == "platform");
        }

        var customBoards = await customBoardsQuery
            .OrderBy(board => board.Title)
            .ToListAsync(cancellationToken);
        foreach (var board in customBoards)
        {
            if (CommunityBoardCatalog.Find(board.BoardKey) is not null
                || CommunityBoardCatalog.Find(board.Title) is not null)
            {
                continue;
            }

            var count = categoryCounts.FirstOrDefault(item =>
                string.Equals(item.Category, board.Title, StringComparison.OrdinalIgnoreCase));
            summaries.Add(new CommunityBoardSummaryResponse
            {
                BoardKey = board.BoardKey,
                DisplayName = board.Title,
                Description = board.Description,
                GroupCode = CommunityBoardGroupCodes.PeopleAndInformation,
                GroupDisplayName = "구성원 게시판",
                IsUserCreatable = true,
                IsCustom = true,
                PostingAccessCode = CommunityBoardPostingAccessCodes.Authenticated,
                PostingAccessDisplayName = CommunityBoardPostingAccessCodes.DisplayName(
                    CommunityBoardPostingAccessCodes.Authenticated),
                AllowsAnonymousPosting = false,
                PostCount = count?.Count ?? 0,
                LatestPostAtUtc = count?.LatestPostAtUtc
            });
        }

        return Result.Ok<IReadOnlyList<CommunityBoardSummaryResponse>>(summaries);
    }

    public Task<Result<PlatformCommunityPostResponse>> 생성Async(
        PlatformCommunityPostCreateRequest? request,
        CancellationToken cancellationToken)
        => 저장Async(request, null, cancellationToken);

    public Task<Result<PlatformCommunityPostResponse>> 예약Async(
        PlatformCommunityPostScheduleCreateRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Task.FromResult(BadRequest<PlatformCommunityPostResponse>("request body is required"));
        }

        var scheduledPublishAtUtc = EnsureUtc(request.ScheduledPublishAtUtc);
        var now = DateTime.UtcNow;
        if (scheduledPublishAtUtc < now.Add(PlatformCommunityPostSchedulePolicy.MinimumLeadTime)
            || scheduledPublishAtUtc > now.Add(PlatformCommunityPostSchedulePolicy.MaximumLeadTime))
        {
            return Task.FromResult(BadRequest<PlatformCommunityPostResponse>(
                "예약 발행 시각은 현재부터 1분 이후, 365일 이내여야 합니다."));
        }

        return 저장Async(request.Post, scheduledPublishAtUtc, cancellationToken);
    }

    private async Task<Result<PlatformCommunityPostResponse>> 저장Async(
        PlatformCommunityPostCreateRequest? request,
        DateTime? scheduledPublishAtUtc,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<PlatformCommunityPostResponse>("request body is required");
        }

        var normalizedCategory = ResolvePostCategory(request.Category, request.SalesOffer);
        var validation = ValidatePost(
            request.Nickname,
            request.Password,
            request.Title,
            request.Body,
            request.SharedLinkUrl,
            request.SalesOffer,
            RequiresSuppliedNickname(normalizedCategory));
        if (validation is not null)
        {
            return BadRequest<PlatformCommunityPostResponse>(validation);
        }

        var countryValidation = ValidateAuthorDisplayCountry(
            request.IsAuthorDisplayCountryPublic,
            request.AuthorDisplayCountryCode,
            request.AuthorDisplayCountryName);
        if (countryValidation is not null)
        {
            return BadRequest<PlatformCommunityPostResponse>(countryValidation);
        }

        커뮤니티원장Dto? 연결원장 = null;
        if (!string.IsNullOrWhiteSpace(request.커뮤니티원장Id))
        {
            var 원장결과 = await _원장ContextService.연결가능원장조회Async(
                request.커뮤니티원장Id,
                _currentUserAccessor.UserId,
                request.WorkflowTag,
                cancellationToken);
            if (원장결과.IsFailed)
            {
                return Result.Fail<PlatformCommunityPostResponse>(원장결과.Errors);
            }

            연결원장 = 원장결과.Value;
        }

        var now = DateTime.UtcNow;
        var normalizedAppKey = Normalize(request.AppKey, "platform", 80);
        if (!await _boardWritePolicy.CanWriteAsync(
                normalizedAppKey,
                normalizedCategory,
                _currentUserAccessor.UserId,
                cancellationToken))
        {
            return WriteRejected<PlatformCommunityPostResponse>(normalizedCategory);
        }

        var isReportBoardPost = request.SalesOffer is null
                                && (request.IsReportBoardPost || IsReportCategory(normalizedCategory));
        var normalizedNickname = ResolvePostingNickname(normalizedCategory, request.Nickname);
        var normalizedTitle = Normalize(request.Title, string.Empty, 160);
        var normalizedBody = Normalize(request.Body, string.Empty, 4000);
        var entity = new PlatformCommunityPost
        {
            AppKey = normalizedAppKey,
            Category = normalizedCategory,
            WorkflowTag = Normalize(request.WorkflowTag, "국내 화물 운송", 60),
            RoleTag = Normalize(request.RoleTag, "플랫폼 구성원", 40),
            Title = normalizedTitle,
            Body = normalizedBody,
            OriginalLanguageCode = CommunityPostLanguageResolver.Resolve(
                request.OriginalLanguageCode,
                normalizedTitle,
                normalizedBody),
            SharedLinkUrl = NormalizeOptionalUrl(request.SharedLinkUrl),
            SalesOfferJson = SerializeSalesOffer(request.SalesOffer),
            커뮤니티원장Id = 연결원장?.원장Id,
            AuthorUserId = NormalizeOptional(_currentUserAccessor.UserId, 450),
            Nickname = normalizedNickname,
            IsAuthorDisplayCountryPublic = !isReportBoardPost && request.IsAuthorDisplayCountryPublic,
            AuthorDisplayCountryCode = !isReportBoardPost && request.IsAuthorDisplayCountryPublic
                ? NormalizeCountryCode(request.AuthorDisplayCountryCode)
                : null,
            AuthorDisplayCountryName = !isReportBoardPost && request.IsAuthorDisplayCountryPublic
                ? Normalize(request.AuthorDisplayCountryName, string.Empty, 80)
                : null,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim()),
            IsReportBoardPost = isReportBoardPost,
            ReporterDisplayName = isReportBoardPost
                ? Normalize(request.ReporterDisplayName, normalizedNickname, 40)
                : null,
            ReportedDisplayName = isReportBoardPost
                ? Normalize(request.ReportedDisplayName, string.Empty, 40)
                : null,
            PublicationStatusCode = scheduledPublishAtUtc.HasValue
                ? PlatformCommunityPostPublicationStatusCodes.Scheduled
                : PlatformCommunityPostPublicationStatusCodes.Published,
            ScheduledPublishAtUtc = scheduledPublishAtUtc,
            PublishedAtUtc = scheduledPublishAtUtc.HasValue ? null : now,
            PublicationNextAttemptAtUtc = scheduledPublishAtUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.PlatformCommunityPosts.Add(entity);
        if (!scheduledPublishAtUtc.HasValue)
        {
            _음성작업예약Service.예약(entity, now);
            _keywordNotificationQueue.Enqueue(entity, now);
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (!scheduledPublishAtUtc.HasValue)
        {
            try
            {
                await _publisher.Publish(new 커뮤니티게시글등록됨Event(entity.Id), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "게시글 등록 후속 이벤트 발행에 실패했습니다. 음성 및 키워드 작업은 DB 대기열에서 복구됩니다. PostId={PostId}",
                    entity.Id);
            }
        }

        var 원장Context = 연결원장 is null
            ? null
            : await _원장ContextService.조회Async(연결원장.원장Id, _currentUserAccessor.UserId, cancellationToken);
        return Result.Ok(ToResponse(entity, 원장Context));
    }

    public async Task<Result<IReadOnlyList<PlatformCommunityPostResponse>>> 예약목록Async(
        string? status,
        int take,
        CancellationToken cancellationToken)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToLowerInvariant();
        if (normalizedStatus is not null
            && !PlatformCommunityPostPublicationStatuses.IsSupported(normalizedStatus))
        {
            return BadRequest<IReadOnlyList<PlatformCommunityPostResponse>>("지원하지 않는 예약 발행 상태입니다.");
        }

        var query = _db.PlatformCommunityPosts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(post => !post.IsDeleted);
        query = normalizedStatus is null
            ? query.Where(post => post.PublicationStatusCode != PlatformCommunityPostPublicationStatusCodes.Published)
            : query.Where(post => post.PublicationStatusCode == normalizedStatus);
        var items = await query
            .OrderBy(post => post.ScheduledPublishAtUtc ?? DateTime.MaxValue)
            .ThenByDescending(post => post.CreatedAtUtc)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<PlatformCommunityPostResponse>>(
            items.Select(post => ToResponse(post)).ToArray());
    }

    public async Task<Result<PlatformCommunityPostResponse>> 예약취소Async(
        long id,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var cancelled = await _db.PlatformCommunityPosts
            .IgnoreQueryFilters()
            .Where(post => post.Id == id
                           && !post.IsDeleted
                           && post.PublicationStatusCode == PlatformCommunityPostPublicationStatusCodes.Scheduled)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        post => post.PublicationStatusCode,
                        PlatformCommunityPostPublicationStatusCodes.Cancelled)
                    .SetProperty(post => post.PublicationNextAttemptAtUtc, (DateTime?)null)
                    .SetProperty(post => post.PublicationClaimedAtUtc, (DateTime?)null)
                    .SetProperty(post => post.UpdatedAtUtc, now),
                cancellationToken);
        if (cancelled == 0)
        {
            var exists = await _db.PlatformCommunityPosts
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(post => post.Id == id && !post.IsDeleted, cancellationToken);
            return exists
                ? BadRequest<PlatformCommunityPostResponse>("발행 대기 중인 예약 게시글만 취소할 수 있습니다.")
                : NotFound<PlatformCommunityPostResponse>("예약 게시글을 찾을 수 없습니다.");
        }

        var post = await _db.PlatformCommunityPosts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(item => item.Id == id, cancellationToken);
        return Result.Ok(ToResponse(post));
    }

    public async Task<Result<PlatformCommunityPostResponse>> 상세Async(long id, CancellationToken cancellationToken)
    {
        var entity = await PostWithDisplayIncludes()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity is null)
        {
            return NotFound<PlatformCommunityPostResponse>("게시글을 찾을 수 없습니다.");
        }

        var isProtectedReport = entity.IsReportBoardPost || IsReportCategory(entity.Category);
        var 원장Context = isProtectedReport
            ? null
            : CommunityLedgerCompletionPublication.IsSystemPost(entity)
                ? await _원장ContextService.비식별성립사례조회Async(
                    entity.커뮤니티원장Id,
                    cancellationToken)
                : await _원장ContextService.조회Async(
                    entity.커뮤니티원장Id,
                    _currentUserAccessor.UserId,
                    cancellationToken);
        return Result.Ok(ToResponse(entity, 원장Context));
    }

    public async Task<Result<PlatformCommunityPostAttachmentResponse>> 첨부업로드Async(
        long id,
        커뮤니티게시글첨부업로드Command? command,
        CancellationToken cancellationToken)
    {
        if (command is null || command.Length <= 0)
        {
            return BadRequest<PlatformCommunityPostAttachmentResponse>("업로드할 이미지 파일을 선택해야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            return BadRequest<PlatformCommunityPostAttachmentResponse>("게시글 비밀번호를 입력해야 합니다.");
        }

        var entity = await _db.PlatformCommunityPosts
            .IgnoreQueryFilters()
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound<PlatformCommunityPostAttachmentResponse>("게시글을 찾을 수 없습니다.");
        }
        if (entity.PublicationStatusCode is PlatformCommunityPostPublicationStatusCodes.Cancelled
            or PlatformCommunityPostPublicationStatusCodes.Failed)
        {
            return BadRequest<PlatformCommunityPostAttachmentResponse>("취소되거나 실패한 예약 게시글에는 첨부할 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(command.Password.Trim(), entity.PasswordHash))
        {
            return Forbidden<PlatformCommunityPostAttachmentResponse>("게시글 비밀번호가 일치하지 않습니다.");
        }

        if (entity.Attachments.Count >= _storageOptions.MaxAttachmentsPerPost)
        {
            return BadRequest<PlatformCommunityPostAttachmentResponse>($"게시글당 이미지는 최대 {_storageOptions.MaxAttachmentsPerPost}개까지 업로드할 수 있습니다.");
        }

        if (command.Length > _storageOptions.MaxImageBytes)
        {
            return BadRequest<PlatformCommunityPostAttachmentResponse>($"이미지 크기는 최대 {_storageOptions.MaxImageBytes / 1024 / 1024}MB까지 허용됩니다.");
        }

        if (!_storageOptions.AllowedContentTypes.Contains(command.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest<PlatformCommunityPostAttachmentResponse>("허용되지 않은 이미지 형식입니다.");
        }

        var folder = $"{_storageOptions.Folder.Trim().Trim('/')}/{entity.Id}";
        var uploadResult = await _storageService.UploadAsync(
            command.FileStream,
            command.FileName,
            command.ContentType,
            folder,
            cancellationToken);

        var attachment = new PlatformCommunityPostAttachment
        {
            PostId = entity.Id,
            BucketName = uploadResult.BucketName,
            ObjectName = uploadResult.ObjectName,
            Url = uploadResult.PublicUrl,
            OriginalFileName = Path.GetFileName(command.FileName),
            ContentType = command.ContentType,
            FileSizeBytes = command.Length,
            UploadedAtUtc = DateTime.UtcNow
        };

        _db.PlatformCommunityPostAttachments.Add(attachment);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(ToAttachmentResponse(attachment));
    }

    public async Task<Result<PlatformCommunityPostResponse>> 수정Async(
        long id,
        PlatformCommunityPostUpdateRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<PlatformCommunityPostResponse>("request body is required");
        }

        var normalizedCategory = ResolvePostCategory(request.Category, request.SalesOffer);
        var validation = ValidatePost(
            request.Nickname,
            request.Password,
            request.Title,
            request.Body,
            request.SharedLinkUrl,
            request.SalesOffer,
            RequiresSuppliedNickname(normalizedCategory));
        if (validation is not null)
        {
            return BadRequest<PlatformCommunityPostResponse>(validation);
        }

        var countryValidation = ValidateAuthorDisplayCountry(
            request.IsAuthorDisplayCountryPublic,
            request.AuthorDisplayCountryCode,
            request.AuthorDisplayCountryName);
        if (countryValidation is not null)
        {
            return BadRequest<PlatformCommunityPostResponse>(countryValidation);
        }

        var entity = await _db.PlatformCommunityPosts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound<PlatformCommunityPostResponse>("게시글을 찾을 수 없습니다.");
        }

        if (CommunityLedgerCompletionPublication.IsSystemPost(entity))
        {
            return Forbidden<PlatformCommunityPostResponse>("원장 성립 시스템 기록은 수정할 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), entity.PasswordHash))
        {
            return Forbidden<PlatformCommunityPostResponse>("게시글 비밀번호가 일치하지 않습니다.");
        }

        커뮤니티원장Dto? 연결원장 = null;
        if (!string.IsNullOrWhiteSpace(request.커뮤니티원장Id))
        {
            var 원장결과 = await _원장ContextService.연결가능원장조회Async(
                request.커뮤니티원장Id,
                _currentUserAccessor.UserId,
                request.WorkflowTag,
                cancellationToken);
            if (원장결과.IsFailed)
            {
                return Result.Fail<PlatformCommunityPostResponse>(원장결과.Errors);
            }

            연결원장 = 원장결과.Value;
        }

        if (!await _boardWritePolicy.CanWriteAsync(
                entity.AppKey,
                normalizedCategory,
                _currentUserAccessor.UserId,
                cancellationToken))
        {
            return WriteRejected<PlatformCommunityPostResponse>(normalizedCategory);
        }

        entity.Category = normalizedCategory;
        var isReportBoardPost = request.SalesOffer is null
                                && (request.IsReportBoardPost || IsReportCategory(entity.Category));
        entity.WorkflowTag = Normalize(request.WorkflowTag, "국내 화물 운송", 60);
        entity.RoleTag = Normalize(request.RoleTag, "플랫폼 구성원", 40);
        entity.Title = Normalize(request.Title, string.Empty, 160);
        entity.Body = Normalize(request.Body, string.Empty, 4000);
        entity.OriginalLanguageCode = CommunityPostLanguageResolver.Resolve(
            request.OriginalLanguageCode,
            entity.Title,
            entity.Body);
        entity.SharedLinkUrl = NormalizeOptionalUrl(request.SharedLinkUrl);
        entity.SalesOfferJson = SerializeSalesOffer(request.SalesOffer);
        if (request.커뮤니티원장Id is not null)
        {
            entity.커뮤니티원장Id = 연결원장?.원장Id;
        }
        entity.Nickname = ResolvePostingNickname(
            normalizedCategory,
            request.Nickname,
            entity.Nickname);
        entity.IsReportBoardPost = isReportBoardPost;
        entity.IsAuthorDisplayCountryPublic = !entity.IsReportBoardPost && request.IsAuthorDisplayCountryPublic;
        entity.AuthorDisplayCountryCode = entity.IsAuthorDisplayCountryPublic
            ? NormalizeCountryCode(request.AuthorDisplayCountryCode)
            : null;
        entity.AuthorDisplayCountryName = entity.IsAuthorDisplayCountryPublic
            ? Normalize(request.AuthorDisplayCountryName, string.Empty, 80)
            : null;
        entity.ReporterDisplayName = entity.IsReportBoardPost
            ? Normalize(request.ReporterDisplayName, entity.Nickname, 40)
            : null;
        entity.ReportedDisplayName = entity.IsReportBoardPost
            ? Normalize(request.ReportedDisplayName, string.Empty, 40)
            : null;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        var 원장Context = await _원장ContextService.조회Async(
            entity.커뮤니티원장Id,
            _currentUserAccessor.UserId,
            cancellationToken);
        return Result.Ok(ToResponse(entity, 원장Context));
    }

    public async Task<Result<PlatformCommunityPostResponse>> 운영자고정Async(
        long id,
        PlatformCommunityPostOperatorPinRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<PlatformCommunityPostResponse>("request body is required");
        }

        var entity = await PostWithDisplayIncludes()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound<PlatformCommunityPostResponse>("게시글을 찾을 수 없습니다.");
        }

        entity.IsOperatorPinned = request.IsOperatorPinned;
        entity.OperatorPinnedAtUtc = request.IsOperatorPinned ? DateTime.UtcNow : null;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(ToResponse(entity));
    }

    public async Task<Result<PlatformCommunityPostResponse>> 추천Async(
        long id,
        PlatformCommunityPostRecommendationRequest? request,
        string fallbackRecommenderKey,
        CancellationToken cancellationToken)
    {
        var recommenderKey = Normalize(request?.RecommenderKey, fallbackRecommenderKey, 120);
        var entity = await PostWithDisplayIncludes()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound<PlatformCommunityPostResponse>("게시글을 찾을 수 없습니다.");
        }

        var alreadyRecommended = await _db.PlatformCommunityPostRecommendations
            .AnyAsync(x => x.PostId == id && x.RecommenderKey == recommenderKey, cancellationToken);
        if (!alreadyRecommended)
        {
            _db.PlatformCommunityPostRecommendations.Add(new PlatformCommunityPostRecommendation
            {
                PostId = id,
                RecommenderKey = recommenderKey,
                CreatedAtUtc = DateTime.UtcNow
            });
            entity.RecommendationCount += 1;
            entity.LastEngagedAtUtc = DateTime.UtcNow;
            entity.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok(ToResponse(entity));
    }

    public async Task<Result<IReadOnlyList<PlatformCommunityPostCommentResponse>>> 댓글목록Async(
        long id,
        CancellationToken cancellationToken)
    {
        var exists = await _db.PlatformCommunityPosts
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (!exists)
        {
            return NotFound<IReadOnlyList<PlatformCommunityPostCommentResponse>>("게시글을 찾을 수 없습니다.");
        }

        var comments = await _db.PlatformCommunityPostComments
            .AsNoTracking()
            .Where(x => x.PostId == id && !x.IsDeleted && !x.IsOperatorHidden)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .Select(x => ToCommentResponse(x))
            .ToListAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<PlatformCommunityPostCommentResponse>>(comments);
    }

    public async Task<Result<PlatformCommunityPostCommentResponse>> 댓글작성Async(
        long id,
        PlatformCommunityPostCommentCreateRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<PlatformCommunityPostCommentResponse>("request body is required");
        }

        var entity = await _db.PlatformCommunityPosts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound<PlatformCommunityPostCommentResponse>("게시글을 찾을 수 없습니다.");
        }

        var validation = ValidateComment(request, RequiresSuppliedNickname(entity.Category));
        if (validation is not null)
        {
            return BadRequest<PlatformCommunityPostCommentResponse>(validation);
        }

        var now = DateTime.UtcNow;
        var comment = new PlatformCommunityPostComment
        {
            PostId = id,
            Nickname = ResolvePostingNickname(entity.Category, request.Nickname),
            Body = Normalize(request.Body, string.Empty, 1000),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim()),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.PlatformCommunityPostComments.Add(comment);
        entity.CommentCount += 1;
        entity.LastEngagedAtUtc = now;
        entity.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(ToCommentResponse(comment));
    }

    public async Task<Result> 댓글삭제Async(
        long id,
        long commentId,
        PlatformCommunityPostPasswordRequest? request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Password))
        {
            return BadRequest("Password is required.");
        }

        var comment = await _db.PlatformCommunityPostComments
            .Include(x => x.Post)
            .FirstOrDefaultAsync(
                x => x.Id == commentId && x.PostId == id && !x.IsDeleted && x.Post != null && !x.Post.IsDeleted,
                cancellationToken);
        if (comment is null)
        {
            return NotFound("댓글을 찾을 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), comment.PasswordHash))
        {
            return Forbidden("댓글 비밀번호가 일치하지 않습니다.");
        }

        comment.IsDeleted = true;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        comment.Post!.CommentCount = Math.Max(0, comment.Post.CommentCount - 1);
        comment.Post.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> 댓글신고Async(long commentId, CancellationToken cancellationToken)
    {
        var comment = await _db.PlatformCommunityPostComments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return NotFound("댓글을 찾을 수 없습니다.");
        }

        comment.ReportCount += 1;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> 댓글운영자숨김Async(
        long commentId,
        PlatformCommunityOperatorHiddenRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request body is required");
        }

        var comment = await _db.PlatformCommunityPostComments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return NotFound("댓글을 찾을 수 없습니다.");
        }

        comment.IsOperatorHidden = request.IsOperatorHidden;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse>>> 첨부댓글목록Async(
        long attachmentId,
        CancellationToken cancellationToken)
    {
        var exists = await _db.PlatformCommunityPostAttachments
            .AnyAsync(x => x.Id == attachmentId && x.Post != null && !x.Post.IsDeleted, cancellationToken);
        if (!exists)
        {
            return NotFound<IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse>>("첨부 이미지를 찾을 수 없습니다.");
        }

        var comments = await _db.PlatformCommunityPostAttachmentComments
            .AsNoTracking()
            .Where(x => x.AttachmentId == attachmentId && !x.IsDeleted && !x.IsOperatorHidden)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .Select(x => ToAttachmentCommentResponse(x))
            .ToListAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse>>(comments);
    }

    public async Task<Result<PlatformCommunityPostAttachmentCommentResponse>> 첨부댓글작성Async(
        long attachmentId,
        PlatformCommunityPostAttachmentCommentCreateRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<PlatformCommunityPostAttachmentCommentResponse>("request body is required");
        }

        var attachment = await _db.PlatformCommunityPostAttachments
            .Include(x => x.Post)
            .FirstOrDefaultAsync(x => x.Id == attachmentId && x.Post != null && !x.Post.IsDeleted, cancellationToken);
        if (attachment is null)
        {
            return NotFound<PlatformCommunityPostAttachmentCommentResponse>("첨부 이미지를 찾을 수 없습니다.");
        }

        var validation = ValidateAttachmentComment(
            request,
            RequiresSuppliedNickname(attachment.Post!.Category));
        if (validation is not null)
        {
            return BadRequest<PlatformCommunityPostAttachmentCommentResponse>(validation);
        }

        var now = DateTime.UtcNow;
        var comment = new PlatformCommunityPostAttachmentComment
        {
            AttachmentId = attachmentId,
            Nickname = ResolvePostingNickname(attachment.Post!.Category, request.Nickname),
            Body = Normalize(request.Body, string.Empty, 1000),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim()),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.PlatformCommunityPostAttachmentComments.Add(comment);
        attachment.CommentCount += 1;
        attachment.Post!.LastEngagedAtUtc = now;
        attachment.Post.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(ToAttachmentCommentResponse(comment));
    }

    public async Task<Result> 첨부댓글삭제Async(
        long attachmentId,
        long commentId,
        PlatformCommunityPostPasswordRequest? request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Password))
        {
            return BadRequest("Password is required.");
        }

        var comment = await _db.PlatformCommunityPostAttachmentComments
            .Include(x => x.Attachment)
                .ThenInclude(x => x.Post)
            .FirstOrDefaultAsync(
                x => x.Id == commentId && x.AttachmentId == attachmentId && !x.IsDeleted && x.Attachment.Post != null && !x.Attachment.Post.IsDeleted,
                cancellationToken);
        if (comment is null)
        {
            return NotFound("첨부 댓글을 찾을 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), comment.PasswordHash))
        {
            return Forbidden("첨부 댓글 비밀번호가 일치하지 않습니다.");
        }

        comment.IsDeleted = true;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        comment.Attachment.CommentCount = Math.Max(0, comment.Attachment.CommentCount - 1);
        comment.Attachment.Post!.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> 첨부댓글신고Async(long commentId, CancellationToken cancellationToken)
    {
        var comment = await _db.PlatformCommunityPostAttachmentComments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return NotFound("첨부 댓글을 찾을 수 없습니다.");
        }

        comment.ReportCount += 1;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> 첨부댓글운영자숨김Async(
        long commentId,
        PlatformCommunityOperatorHiddenRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request body is required");
        }

        var comment = await _db.PlatformCommunityPostAttachmentComments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return NotFound("첨부 댓글을 찾을 수 없습니다.");
        }

        comment.IsOperatorHidden = request.IsOperatorHidden;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> 삭제Async(
        long id,
        PlatformCommunityPostPasswordRequest? request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Password))
        {
            return BadRequest("비밀번호를 입력해야 합니다.");
        }

        var entity = await _db.PlatformCommunityPosts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound("게시글을 찾을 수 없습니다.");
        }

        if (CommunityLedgerCompletionPublication.IsSystemPost(entity))
        {
            return Forbidden("원장 성립 시스템 기록은 삭제할 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), entity.PasswordHash))
        {
            return Forbidden("게시글 비밀번호가 일치하지 않습니다.");
        }

        entity.IsDeleted = true;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private IQueryable<PlatformCommunityPost> PostWithDisplayIncludes()
        => _db.PlatformCommunityPosts
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Comments)
            .Include(x => x.Comments.Where(comment => !comment.IsDeleted && !comment.IsOperatorHidden));

    private static string? ValidatePost(
        string nickname,
        string password,
        string title,
        string body,
        string? sharedLinkUrl,
        PlatformCommunityPostSalesOfferRequest? salesOffer,
        bool requiresSuppliedNickname)
    {
        if ((requiresSuppliedNickname && string.IsNullOrWhiteSpace(nickname))
            || (!string.IsNullOrWhiteSpace(nickname) && nickname.Trim().Length > 40))
        {
            return "닉네임은 1자 이상 40자 이하로 입력해야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < 4 || password.Trim().Length > 100)
        {
            return "비밀번호는 4자 이상 100자 이하로 입력해야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 160)
        {
            return "제목은 1자 이상 160자 이하로 입력해야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(body) && string.IsNullOrWhiteSpace(sharedLinkUrl) && salesOffer is null)
        {
            return "본문, 공유 링크 또는 판매 정보 중 하나는 입력해야 합니다.";
        }

        if (!string.IsNullOrWhiteSpace(body) && body.Trim().Length > 4000)
        {
            return "본문은 1자 이상 4000자 이하로 입력해야 합니다.";
        }

        if (!string.IsNullOrWhiteSpace(sharedLinkUrl) &&
            (!Uri.TryCreate(sharedLinkUrl.Trim(), UriKind.Absolute, out var uri) ||
             (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
             sharedLinkUrl.Trim().Length > 1000))
        {
            return "공유 링크는 http 또는 https URL로 입력해야 합니다.";
        }

        return ValidateSalesOffer(salesOffer);
    }

    private static string? ValidateSalesOffer(PlatformCommunityPostSalesOfferRequest? salesOffer)
    {
        if (salesOffer is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(salesOffer.ProductTitle) || salesOffer.ProductTitle.Trim().Length > 160)
        {
            return "판매 상품명은 1자 이상 160자 이하로 입력해야 합니다.";
        }

        if (salesOffer.AvailableQuantity <= 0 || salesOffer.AvailableQuantity > 1_000_000)
        {
            return "판매 가능 수량은 0보다 크고 1,000,000 이하여야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(salesOffer.QuantityUnit) || salesOffer.QuantityUnit.Trim().Length > 20)
        {
            return "수량 단위는 1자 이상 20자 이하로 입력해야 합니다.";
        }

        if (salesOffer.UnitPrice <= 0 || salesOffer.UnitPrice > 1_000_000_000)
        {
            return "판매 가격은 0보다 크고 1,000,000,000 이하여야 합니다.";
        }

        var currencyCode = salesOffer.CurrencyCode?.Trim() ?? string.Empty;
        if (currencyCode.Length != 3 || currencyCode.Any(character => !char.IsAsciiLetter(character)))
        {
            return "통화 코드는 KRW, USD처럼 ISO 영문 세 자리로 입력해야 합니다.";
        }

        var paymentMethods = (salesOffer.AcceptedPaymentMethods ?? [])
            .Where(method => !string.IsNullOrWhiteSpace(method))
            .Select(method => method.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (paymentMethods.Length == 0)
        {
            return "협의 가능한 결제 방법을 하나 이상 선택해야 합니다.";
        }

        if (paymentMethods.Any(method => !PlatformCommunitySalesPaymentMethodCodes.All.Contains(method, StringComparer.OrdinalIgnoreCase)))
        {
            return "지원하지 않는 결제 방법이 포함되어 있습니다.";
        }

        if (string.IsNullOrWhiteSpace(salesOffer.Status)
            || !PlatformCommunitySalesOfferStatuses.All.Contains(salesOffer.Status, StringComparer.OrdinalIgnoreCase))
        {
            return "판매 상태가 올바르지 않습니다.";
        }

        return null;
    }

    private static string? SerializeSalesOffer(PlatformCommunityPostSalesOfferRequest? source)
    {
        if (source is null)
        {
            return null;
        }

        var normalized = new PlatformCommunityPostSalesOfferResponse
        {
            ProductTitle = Normalize(source.ProductTitle, string.Empty, 160),
            AvailableQuantity = source.AvailableQuantity,
            QuantityUnit = Normalize(source.QuantityUnit, "개", 20),
            UnitPrice = source.UnitPrice,
            CurrencyCode = source.CurrencyCode.Trim().ToUpperInvariant(),
            AcceptedPaymentMethods = (source.AcceptedPaymentMethods ?? [])
                .Where(method => !string.IsNullOrWhiteSpace(method))
                .Select(method => method.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            AllowsGroupPurchase = source.AllowsGroupPurchase,
            Status = source.Status.Trim().ToLowerInvariant()
        };
        return JsonSerializer.Serialize(normalized, JsonOptions);
    }

    private static PlatformCommunityPostSalesOfferResponse? DeserializeSalesOffer(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PlatformCommunityPostSalesOfferResponse>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Normalize(string? value, string fallback, int maxLength)
    {
        var text = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private string ResolvePostingNickname(
        string category,
        string? requestedNickname,
        string? existingNickname = null)
    {
        if (!string.IsNullOrWhiteSpace(_currentUserAccessor.UserId)
            || CommunityBoardCatalog.Find(category)?.AllowsAnonymousPosting != true)
        {
            return Normalize(requestedNickname, "익명", 40);
        }

        var baseName = CommunityAnonymousNicknameCatalog.ResolveBaseName(category);
        if (!string.IsNullOrWhiteSpace(existingNickname)
            && existingNickname.StartsWith(baseName, StringComparison.Ordinal))
        {
            return Normalize(existingNickname, baseName, 40);
        }

        var discriminator = Convert.ToHexString(RandomNumberGenerator.GetBytes(2));
        return CommunityAnonymousNicknameCatalog.Create(category, discriminator);
    }

    private bool RequiresSuppliedNickname(string category)
        => !string.IsNullOrWhiteSpace(_currentUserAccessor.UserId)
           || CommunityBoardCatalog.Find(category)?.AllowsAnonymousPosting != true;

    private static string? NormalizeOptionalUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        return text.Length <= 1000 ? text : text[..1000];
    }

    private static DateTime EnsureUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private static string? ValidateComment(
        PlatformCommunityPostCommentCreateRequest request,
        bool requiresSuppliedNickname)
    {
        if ((requiresSuppliedNickname && string.IsNullOrWhiteSpace(request.Nickname))
            || (!string.IsNullOrWhiteSpace(request.Nickname) && request.Nickname.Trim().Length > 40))
        {
            return "닉네임은 1자 이상 40자 이하로 입력해야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Trim().Length < 4 || request.Password.Trim().Length > 100)
        {
            return "비밀번호는 4자 이상 100자 이하로 입력해야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1000)
        {
            return "댓글은 1자 이상 1000자 이하로 입력해야 합니다.";
        }

        return null;
    }

    private static string? ValidateAttachmentComment(
        PlatformCommunityPostAttachmentCommentCreateRequest request,
        bool requiresSuppliedNickname)
    {
        if ((requiresSuppliedNickname && string.IsNullOrWhiteSpace(request.Nickname))
            || (!string.IsNullOrWhiteSpace(request.Nickname) && request.Nickname.Trim().Length > 40))
        {
            return "닉네임은 1자 이상 40자 이하로 입력해야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Trim().Length < 4 || request.Password.Trim().Length > 100)
        {
            return "비밀번호는 4자 이상 100자 이하로 입력해야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1000)
        {
            return "첨부 댓글은 1자 이상 1000자 이하로 입력해야 합니다.";
        }

        return null;
    }

    private static string? ValidateAuthorDisplayCountry(
        bool isPublic,
        string? countryCode,
        string? countryName)
    {
        if (!isPublic)
        {
            return null;
        }

        var code = countryCode?.Trim() ?? string.Empty;
        if (code.Length != 2 || code.Any(character => !char.IsAsciiLetter(character)))
        {
            return "활동 국가 코드는 ISO 알파-2 영문 두 자리로 입력해야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(countryName) || countryName.Trim().Length > 80)
        {
            return "공개할 활동 국가 이름은 1자 이상 80자 이하로 입력해야 합니다.";
        }

        return null;
    }

    private static string NormalizeCountryCode(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static bool IsReportCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return false;
        }

        return category.Contains("신고", StringComparison.OrdinalIgnoreCase)
            || category.Contains("분쟁", StringComparison.OrdinalIgnoreCase)
            || category.Contains("report", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolvePostCategory(
        string? requestedCategory,
        PlatformCommunityPostSalesOfferRequest? salesOffer)
        => Normalize(
            PlatformCommunityPostCategoryPolicy.Resolve(requestedCategory, salesOffer is not null),
            PlatformCommunityPostCategories.General,
            60);

    private async Task<IReadOnlyList<string>> ResolveBoardCategoryNamesAsync(
        string? appKey,
        string boardKey,
        CancellationToken cancellationToken)
    {
        var catalogBoard = CommunityBoardCatalog.Find(boardKey);
        if (catalogBoard is not null)
        {
            return CommunityBoardCatalog.CategoryNamesFor(catalogBoard.Key);
        }

        var normalizedBoardKey = Normalize(boardKey, string.Empty, 80);
        var normalizedAppKey = string.IsNullOrWhiteSpace(appKey)
            ? null
            : Normalize(appKey, "platform", 80);
        var query = _db.PlatformCommunityBoardRequests
            .AsNoTracking()
            .Where(board => !board.IsDeleted
                            && board.Status == PlatformCommunityBoardRequestStatuses.Approved
                            && board.BoardKey == normalizedBoardKey);
        if (normalizedAppKey is not null)
        {
            query = query.Where(board => board.AppKey == normalizedAppKey);
        }

        var customBoardTitle = await query
            .Select(board => board.Title)
            .FirstOrDefaultAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(customBoardTitle)
            ? [normalizedBoardKey]
            : [customBoardTitle];
    }

    private static CommunityBoardSummaryResponse BuildBoardSummary(
        CommunityBoardDefinition board,
        IReadOnlyList<CommunityBoardCategoryCount> categoryCounts)
    {
        var matchingCounts = categoryCounts
            .Where(item => CommunityBoardCatalog.MatchesCategory(board.Key, item.Category))
            .ToArray();
        return new CommunityBoardSummaryResponse
        {
            BoardKey = board.Key,
            DisplayName = board.DisplayName,
            Description = board.Description,
            GroupCode = board.GroupCode,
            GroupDisplayName = board.GroupDisplayName,
            IsUserCreatable = board.IsUserCreatable,
            PostingAccessCode = board.PostingAccessCode,
            PostingAccessDisplayName = board.PostingAccessDisplayName,
            AllowsAnonymousPosting = board.AllowsAnonymousPosting,
            PostCount = matchingCounts.Sum(item => item.Count),
            LatestPostAtUtc = matchingCounts
                .Select(item => (DateTime?)item.LatestPostAtUtc)
                .Max()
        };
    }

    private static PlatformCommunityPostResponse ToResponse(
        PlatformCommunityPost entity,
        PlatformCommunityPostLedgerContextResponse? 원장Context = null)
    {
        var isReportBoardPost = entity.IsReportBoardPost || IsReportCategory(entity.Category);
        var systemPostKind = CommunityAutomatedPostPublication.GetSystemPostKind(entity);
        var isSystemGenerated = systemPostKind is not null;
        var reporterDisplayName = isReportBoardPost ? "신고자" : entity.Nickname;
        var reportedDisplayName = isReportBoardPost ? "피신고자" : string.Empty;

        return new PlatformCommunityPostResponse
        {
            Id = entity.Id,
            AppKey = entity.AppKey,
            Category = entity.Category,
            WorkflowTag = isReportBoardPost ? "안전센터" : entity.WorkflowTag,
            RoleTag = isReportBoardPost ? "보호 기록" : entity.RoleTag,
            Title = isReportBoardPost ? "보호된 신고·분쟁 기록" : entity.Title,
            Body = isReportBoardPost
                ? "신고 원문과 첨부·댓글은 공개 게시판에서 제공하지 않습니다."
                : entity.Body,
            OriginalLanguageCode = isReportBoardPost
                ? CommunityDisplayLanguageCodes.Korean
                : CommunityPostLanguageResolver.Resolve(
                    entity.OriginalLanguageCode,
                    entity.Title,
                    entity.Body),
            SharedLinkUrl = isReportBoardPost ? null : entity.SharedLinkUrl,
            SalesOffer = isReportBoardPost ? null : DeserializeSalesOffer(entity.SalesOfferJson),
            커뮤니티원장Id = isReportBoardPost ? null : entity.커뮤니티원장Id,
            원장Context = isReportBoardPost ? null : 원장Context,
            Nickname = isReportBoardPost ? reporterDisplayName : entity.Nickname,
            IsAuthorDisplayCountryPublic = !isReportBoardPost && entity.IsAuthorDisplayCountryPublic,
            AuthorDisplayCountryCode = !isReportBoardPost && entity.IsAuthorDisplayCountryPublic
                ? entity.AuthorDisplayCountryCode
                : null,
            AuthorDisplayCountryName = !isReportBoardPost && entity.IsAuthorDisplayCountryPublic
                ? entity.AuthorDisplayCountryName
                : null,
            IsSystemGenerated = isSystemGenerated,
            SystemPostKind = systemPostKind,
            PrivacyNotice = isReportBoardPost
                ? "신고·분쟁 기록은 공개 목록에서 제외되며 원문과 첨부·댓글을 공개하지 않습니다."
                : CommunityAutomatedPostPublication.GetPrivacyNotice(systemPostKind),
            IsReportBoardPost = isReportBoardPost,
            ReporterDisplayName = reporterDisplayName,
            ReportedDisplayName = reportedDisplayName,
            ViewerReportRole = PlatformCommunityReportViewerRoles.Observer,
            IsReportSubjectMasked = isReportBoardPost,
            IsOperatorPinned = entity.IsOperatorPinned,
            OperatorPinnedAtUtc = entity.OperatorPinnedAtUtc,
            IsCommunityMomentumPromoted = !isReportBoardPost && entity.IsCommunityMomentumPromoted,
            CommunityMomentumCode = !isReportBoardPost && entity.IsCommunityMomentumPromoted
                ? entity.CommunityMomentumCode
                : null,
            CommunityMomentumMessage = !isReportBoardPost && entity.IsCommunityMomentumPromoted
                ? entity.CommunityMomentumMessage
                : null,
            CommunityMomentumRoleParticipantCount = !isReportBoardPost && entity.IsCommunityMomentumPromoted
                ? entity.CommunityMomentumRoleParticipantCount
                : 0,
            CommunityMomentumUpdatedAtUtc = !isReportBoardPost && entity.IsCommunityMomentumPromoted
                ? entity.CommunityMomentumUpdatedAtUtc
                : null,
            RecommendationCount = isReportBoardPost ? 0 : entity.RecommendationCount,
            CommentCount = isReportBoardPost ? 0 : entity.CommentCount,
            LastEngagedAtUtc = isReportBoardPost ? null : entity.LastEngagedAtUtc,
            IsTrending = !isReportBoardPost
                         && !entity.IsOperatorPinned
                         && (entity.RecommendationCount >= 3 || entity.CommentCount >= 3),
            PublicationStatusCode = entity.PublicationStatusCode,
            ScheduledPublishAtUtc = entity.ScheduledPublishAtUtc,
            PublishedAtUtc = entity.PublishedAtUtc,
            PublicationAttemptCount = entity.PublicationAttemptCount,
            PublicationLastError = entity.PublicationLastError,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            Attachments = isReportBoardPost
                ? []
                : entity.Attachments
                    .OrderBy(x => x.UploadedAtUtc)
                    .Select(ToAttachmentResponse)
                    .ToArray(),
            RecentComments = isReportBoardPost
                ? []
                : entity.Comments
                    .Where(x => !x.IsDeleted && !x.IsOperatorHidden)
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .Take(3)
                    .Select(ToCommentResponse)
                    .ToArray()
        };
    }

    private static PlatformCommunityPostCommentResponse ToCommentResponse(PlatformCommunityPostComment entity)
        => new()
        {
            Id = entity.Id,
            Nickname = entity.Nickname,
            Body = entity.Body,
            ReportCount = entity.ReportCount,
            CreatedAtUtc = entity.CreatedAtUtc
        };

    private static PlatformCommunityPostAttachmentResponse ToAttachmentResponse(PlatformCommunityPostAttachment entity)
        => new()
        {
            Id = entity.Id,
            Url = entity.Url,
            BucketName = entity.BucketName,
            ObjectName = entity.ObjectName,
            OriginalFileName = entity.OriginalFileName,
            ContentType = entity.ContentType,
            FileSizeBytes = entity.FileSizeBytes,
            CommentCount = entity.CommentCount,
            UploadedAtUtc = entity.UploadedAtUtc,
            RecentComments = entity.Comments
                .Where(x => !x.IsDeleted && !x.IsOperatorHidden)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(3)
                .Select(ToAttachmentCommentResponse)
                .ToArray()
        };

    private static PlatformCommunityPostAttachmentCommentResponse ToAttachmentCommentResponse(PlatformCommunityPostAttachmentComment entity)
        => new()
        {
            Id = entity.Id,
            AttachmentId = entity.AttachmentId,
            Nickname = entity.Nickname,
            Body = entity.Body,
            ReportCount = entity.ReportCount,
            CreatedAtUtc = entity.CreatedAtUtc
        };

    private static Result<T> BadRequest<T>(string message) => Result.Fail<T>(message);

    private static Result BadRequest(string message) => Result.Fail(message);

    private static Result<T> NotFound<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));

    private static Result NotFound(string message)
        => Result.Fail(new Error(message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));

    private static Result<T> Forbidden<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status403Forbidden));

    private static Result Forbidden(string message)
        => Result.Fail(new Error(message).WithMetadata("StatusCode", StatusCodes.Status403Forbidden));

    private Result<T> WriteRejected<T>(string category)
    {
        var board = CommunityBoardCatalog.Find(category);
        var loginRequired = board?.RequiresAuthenticatedPosting == true || board is null;
        if (loginRequired && string.IsNullOrWhiteSpace(_currentUserAccessor.UserId))
        {
            return Result.Fail<T>(new Error(
                    "이 게시판은 로그인한 사용자만 글을 작성할 수 있습니다. 공개 화면에는 실명 대신 닉네임이 표시됩니다.")
                .WithMetadata("StatusCode", StatusCodes.Status401Unauthorized));
        }

        return BadRequest<T>(
            "사용자 작성이 허용된 기본 게시판 또는 운영자가 승인한 사용자 게시판에만 글을 작성할 수 있습니다.");
    }

    private sealed record CommunityBoardCategoryCount(
        string Category,
        int Count,
        DateTime LatestPostAtUtc);
}
