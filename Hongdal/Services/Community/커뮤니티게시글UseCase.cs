using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Application.Community;
using Hongdal.Application.CommandProcessing;
using Hongdal.Contracts.Common.Community;
using Hongdal.Domain.Community;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<PlatformCommunityPostResponse>> 생성Async(
        PlatformCommunityPostCreateRequest? request,
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
        _currentUserAccessor = currentUserAccessor;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Result<PlatformCommunityPostListResponse>> 목록Async(
        string? appKey,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.PlatformCommunityPosts
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(appKey))
        {
            var normalizedAppKey = Normalize(appKey, "platform", 80);
            query = query.Where(x => x.AppKey == normalizedAppKey || x.AppKey == "platform");
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = Normalize(category, "자유", 60);
            query = query.Where(x => x.Category == normalizedCategory);
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
            .ThenByDescending(x => x.CreatedAtUtc)
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

    public async Task<Result<PlatformCommunityPostResponse>> 생성Async(
        PlatformCommunityPostCreateRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest<PlatformCommunityPostResponse>("request body is required");
        }

        var validation = ValidatePost(
            request.Nickname,
            request.Password,
            request.Title,
            request.Body,
            request.SharedLinkUrl,
            request.SalesOffer);
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
        var normalizedCategory = Normalize(request.Category, "자유", 60);
        var isReportBoardPost = request.IsReportBoardPost || IsReportCategory(normalizedCategory);
        if (isReportBoardPost && request.SalesOffer is not null)
        {
            return BadRequest<PlatformCommunityPostResponse>("신고·분쟁 게시글에는 판매 정보를 함께 등록할 수 없습니다.");
        }
        var normalizedNickname = Normalize(request.Nickname, "익명", 40);
        var entity = new PlatformCommunityPost
        {
            AppKey = Normalize(request.AppKey, "platform", 80),
            Category = normalizedCategory,
            WorkflowTag = Normalize(request.WorkflowTag, "국내 화물 운송", 60),
            RoleTag = Normalize(request.RoleTag, "플랫폼 구성원", 40),
            Title = Normalize(request.Title, string.Empty, 160),
            Body = Normalize(request.Body, string.Empty, 4000),
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
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.PlatformCommunityPosts.Add(entity);
        _음성작업예약Service.예약(entity, now);
        _keywordNotificationQueue.Enqueue(entity, now);
        await _db.SaveChangesAsync(cancellationToken);

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

        var 원장Context = 연결원장 is null
            ? null
            : await _원장ContextService.조회Async(연결원장.원장Id, _currentUserAccessor.UserId, cancellationToken);
        return Result.Ok(ToResponse(entity, 원장Context));
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

        var 원장Context = CommunityLedgerCompletionPublication.IsSystemPost(entity)
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
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound<PlatformCommunityPostAttachmentResponse>("게시글을 찾을 수 없습니다.");
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

        var validation = ValidatePost(
            request.Nickname,
            request.Password,
            request.Title,
            request.Body,
            request.SharedLinkUrl,
            request.SalesOffer);
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

        entity.Category = Normalize(request.Category, "자유", 60);
        var isReportBoardPost = request.IsReportBoardPost || IsReportCategory(entity.Category);
        if (isReportBoardPost && request.SalesOffer is not null)
        {
            return BadRequest<PlatformCommunityPostResponse>("신고·분쟁 게시글에는 판매 정보를 함께 등록할 수 없습니다.");
        }
        entity.WorkflowTag = Normalize(request.WorkflowTag, "국내 화물 운송", 60);
        entity.RoleTag = Normalize(request.RoleTag, "플랫폼 구성원", 40);
        entity.Title = Normalize(request.Title, string.Empty, 160);
        entity.Body = Normalize(request.Body, string.Empty, 4000);
        entity.SharedLinkUrl = NormalizeOptionalUrl(request.SharedLinkUrl);
        entity.SalesOfferJson = SerializeSalesOffer(request.SalesOffer);
        if (request.커뮤니티원장Id is not null)
        {
            entity.커뮤니티원장Id = 연결원장?.원장Id;
        }
        entity.Nickname = Normalize(request.Nickname, "익명", 40);
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

        var validation = ValidateComment(request);
        if (validation is not null)
        {
            return BadRequest<PlatformCommunityPostCommentResponse>(validation);
        }

        var entity = await _db.PlatformCommunityPosts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound<PlatformCommunityPostCommentResponse>("게시글을 찾을 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        var comment = new PlatformCommunityPostComment
        {
            PostId = id,
            Nickname = Normalize(request.Nickname, "익명", 40),
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

        var validation = ValidateAttachmentComment(request);
        if (validation is not null)
        {
            return BadRequest<PlatformCommunityPostAttachmentCommentResponse>(validation);
        }

        var attachment = await _db.PlatformCommunityPostAttachments
            .Include(x => x.Post)
            .FirstOrDefaultAsync(x => x.Id == attachmentId && x.Post != null && !x.Post.IsDeleted, cancellationToken);
        if (attachment is null)
        {
            return NotFound<PlatformCommunityPostAttachmentCommentResponse>("첨부 이미지를 찾을 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        var comment = new PlatformCommunityPostAttachmentComment
        {
            AttachmentId = attachmentId,
            Nickname = Normalize(request.Nickname, "Anonymous", 40),
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
        PlatformCommunityPostSalesOfferRequest? salesOffer)
    {
        if (string.IsNullOrWhiteSpace(nickname) || nickname.Trim().Length > 40)
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

    private static string? NormalizeOptionalUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        return text.Length <= 1000 ? text : text[..1000];
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private static string? ValidateComment(PlatformCommunityPostCommentCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nickname) || request.Nickname.Trim().Length > 40)
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

    private static string? ValidateAttachmentComment(PlatformCommunityPostAttachmentCommentCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nickname) || request.Nickname.Trim().Length > 40)
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

    private static PlatformCommunityPostResponse ToResponse(
        PlatformCommunityPost entity,
        PlatformCommunityPostLedgerContextResponse? 원장Context = null)
    {
        var isReportBoardPost = entity.IsReportBoardPost || IsReportCategory(entity.Category);
        var isLedgerCompletionPost = CommunityLedgerCompletionPublication.IsSystemPost(entity);
        var reporterDisplayName = isReportBoardPost ? "신고자" : entity.Nickname;
        var reportedDisplayName = isReportBoardPost ? "피신고자" : string.Empty;

        return new PlatformCommunityPostResponse
        {
            Id = entity.Id,
            AppKey = entity.AppKey,
            Category = entity.Category,
            WorkflowTag = entity.WorkflowTag,
            RoleTag = entity.RoleTag,
            Title = entity.Title,
            Body = entity.Body,
            SharedLinkUrl = entity.SharedLinkUrl,
            SalesOffer = DeserializeSalesOffer(entity.SalesOfferJson),
            커뮤니티원장Id = entity.커뮤니티원장Id,
            원장Context = 원장Context,
            Nickname = isReportBoardPost ? reporterDisplayName : entity.Nickname,
            IsAuthorDisplayCountryPublic = !isReportBoardPost && entity.IsAuthorDisplayCountryPublic,
            AuthorDisplayCountryCode = !isReportBoardPost && entity.IsAuthorDisplayCountryPublic
                ? entity.AuthorDisplayCountryCode
                : null,
            AuthorDisplayCountryName = !isReportBoardPost && entity.IsAuthorDisplayCountryPublic
                ? entity.AuthorDisplayCountryName
                : null,
            IsSystemGenerated = isLedgerCompletionPost,
            SystemPostKind = isLedgerCompletionPost
                ? PlatformCommunitySystemPostKinds.LedgerCompletion
                : null,
            PrivacyNotice = isLedgerCompletionPost
                ? "원장 종류와 절차 구조만 공개되며 이름, 연락처, 상세 주소, 금액과 원문 증빙은 공개하지 않습니다."
                : null,
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
            RecommendationCount = entity.RecommendationCount,
            CommentCount = entity.CommentCount,
            LastEngagedAtUtc = entity.LastEngagedAtUtc,
            IsTrending = !entity.IsOperatorPinned && (entity.RecommendationCount >= 3 || entity.CommentCount >= 3),
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            Attachments = entity.Attachments
                .OrderBy(x => x.UploadedAtUtc)
                .Select(ToAttachmentResponse)
                .ToArray(),
            RecentComments = entity.Comments
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
}
