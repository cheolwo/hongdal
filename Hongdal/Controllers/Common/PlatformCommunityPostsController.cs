using Hongdal.Contracts.Common.Community;
using Hongdal.Controllers;
using Hongdal.Domain.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 홍달.Data;
using 홍달.Services.External.Google;
using 홍달.Services.Options;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/posts")]
public sealed class PlatformCommunityPostsController : ControllerBase
{
    private readonly HongdalContext _db;
    private readonly IGoogleCloudStorageService _storageService;
    private readonly CommunityPostStorageOptions _storageOptions;

    public PlatformCommunityPostsController(
        HongdalContext db,
        IGoogleCloudStorageService storageService,
        IOptions<CommunityPostStorageOptions> storageOptions)
    {
        _db = db;
        _storageService = storageService;
        _storageOptions = storageOptions.Value;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PlatformCommunityPostListResponse>> List(
        [FromQuery] string? appKey,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
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
            .ThenByDescending(x => x.RecommendationCount)
            .ThenByDescending(x => x.LastEngagedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return Ok(new PlatformCommunityPostListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(
        [FromBody] PlatformCommunityPostCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePost(request.Nickname, request.Password, request.Title, request.Body, request.SharedLinkUrl);
        if (validation is not null)
        {
            return this.ToProblemActionResult(validation.Title ?? "게시글 입력값을 확인해야 합니다.");
        }

        var now = DateTime.UtcNow;
        var normalizedCategory = Normalize(request.Category, "자유", 60);
        var isReportBoardPost = request.IsReportBoardPost || IsReportCategory(normalizedCategory);
        var normalizedNickname = Normalize(request.Nickname, "익명", 40);
        var entity = new PlatformCommunityPost
        {
            AppKey = Normalize(request.AppKey, "platform", 80),
            Category = Normalize(request.Category, "자유", 60),
            WorkflowTag = Normalize(request.WorkflowTag, "국내 화물 운송", 60),
            RoleTag = Normalize(request.RoleTag, "플랫폼 구성원", 40),
            Title = Normalize(request.Title, string.Empty, 160),
            Body = Normalize(request.Body, string.Empty, 4000),
            SharedLinkUrl = NormalizeOptionalUrl(request.SharedLinkUrl),
            Nickname = Normalize(request.Nickname, "익명", 40),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim()),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        entity.Category = normalizedCategory;
        entity.Nickname = normalizedNickname;
        entity.IsReportBoardPost = isReportBoardPost;
        entity.ReporterDisplayName = isReportBoardPost
            ? Normalize(request.ReporterDisplayName, normalizedNickname, 40)
            : null;
        entity.ReportedDisplayName = isReportBoardPost
            ? Normalize(request.ReportedDisplayName, string.Empty, 40)
            : null;

        _db.PlatformCommunityPosts.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, ToResponse(entity));
    }

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        var entity = await _db.PlatformCommunityPosts
            .AsNoTracking()
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Comments)
            .Include(x => x.Comments.Where(comment => !comment.IsDeleted && !comment.IsOperatorHidden))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        return entity is null ? this.ToNotFoundProblem("게시글을 찾을 수 없습니다.") : Ok(ToResponse(entity));
    }

    [HttpPost("{id:long}/attachments")]
    [AllowAnonymous]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadAttachment(
        long id,
        [FromForm] PlatformCommunityPostAttachmentUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length <= 0)
        {
            return this.ToProblemActionResult("업로드할 이미지 파일을 선택해야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return this.ToProblemActionResult("게시글 비밀번호를 입력해야 합니다.");
        }

        var entity = await _db.PlatformCommunityPosts
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return this.ToNotFoundProblem("게시글을 찾을 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), entity.PasswordHash))
        {
            return this.ToForbiddenProblem("게시글 비밀번호가 일치하지 않습니다.");
        }

        if (entity.Attachments.Count >= _storageOptions.MaxAttachmentsPerPost)
        {
            return this.ToProblemActionResult($"게시글당 이미지는 최대 {_storageOptions.MaxAttachmentsPerPost}개까지 업로드할 수 있습니다.");
        }

        if (request.File.Length > _storageOptions.MaxImageBytes)
        {
            return this.ToProblemActionResult($"이미지 크기는 최대 {_storageOptions.MaxImageBytes / 1024 / 1024}MB까지 허용됩니다.");
        }

        if (!_storageOptions.AllowedContentTypes.Contains(request.File.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return this.ToProblemActionResult("허용되지 않은 이미지 형식입니다.");
        }

        await using var stream = request.File.OpenReadStream();
        var folder = $"{_storageOptions.Folder.Trim().Trim('/')}/{entity.Id}";
        var uploadResult = await _storageService.UploadAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            folder,
            cancellationToken);

        var attachment = new PlatformCommunityPostAttachment
        {
            PostId = entity.Id,
            BucketName = uploadResult.BucketName,
            ObjectName = uploadResult.ObjectName,
            Url = uploadResult.PublicUrl,
            OriginalFileName = Path.GetFileName(request.File.FileName),
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            UploadedAtUtc = DateTime.UtcNow
        };

        _db.PlatformCommunityPostAttachments.Add(attachment);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ToAttachmentResponse(attachment));
    }

    [HttpPut("{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] PlatformCommunityPostUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePost(request.Nickname, request.Password, request.Title, request.Body, request.SharedLinkUrl);
        if (validation is not null)
        {
            return this.ToProblemActionResult(validation.Title ?? "게시글 입력값을 확인해야 합니다.");
        }

        var entity = await _db.PlatformCommunityPosts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return this.ToNotFoundProblem("게시글을 찾을 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), entity.PasswordHash))
        {
            return this.ToForbiddenProblem("게시글 비밀번호가 일치하지 않습니다.");
        }

        entity.Category = Normalize(request.Category, "자유", 60);
        entity.WorkflowTag = Normalize(request.WorkflowTag, "국내 화물 운송", 60);
        entity.RoleTag = Normalize(request.RoleTag, "플랫폼 구성원", 40);
        entity.Title = Normalize(request.Title, string.Empty, 160);
        entity.Body = Normalize(request.Body, string.Empty, 4000);
        entity.SharedLinkUrl = NormalizeOptionalUrl(request.SharedLinkUrl);
        entity.Nickname = Normalize(request.Nickname, "익명", 40);
        entity.IsReportBoardPost = request.IsReportBoardPost || IsReportCategory(entity.Category);
        entity.ReporterDisplayName = entity.IsReportBoardPost
            ? Normalize(request.ReporterDisplayName, entity.Nickname, 40)
            : null;
        entity.ReportedDisplayName = entity.IsReportBoardPost
            ? Normalize(request.ReportedDisplayName, string.Empty, 40)
            : null;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(entity));
    }

    [HttpPost("{id:long}/operator-pin")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetOperatorPin(
        long id,
        [FromBody] PlatformCommunityPostOperatorPinRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.PlatformCommunityPosts
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Comments)
            .Include(x => x.Comments.Where(comment => !comment.IsDeleted && !comment.IsOperatorHidden))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return this.ToNotFoundProblem("게시글을 찾을 수 없습니다.");
        }

        entity.IsOperatorPinned = request.IsOperatorPinned;
        entity.OperatorPinnedAtUtc = request.IsOperatorPinned ? DateTime.UtcNow : null;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(entity));
    }

    [HttpPost("{id:long}/recommendations")]
    [AllowAnonymous]
    public async Task<IActionResult> Recommend(
        long id,
        [FromBody] PlatformCommunityPostRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var recommenderKey = Normalize(request.RecommenderKey, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous", 120);
        var entity = await _db.PlatformCommunityPosts
            .Include(x => x.Attachments)
                .ThenInclude(x => x.Comments)
            .Include(x => x.Comments.Where(comment => !comment.IsDeleted && !comment.IsOperatorHidden))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return this.ToNotFoundProblem("게시글을 찾을 수 없습니다.");
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

        return Ok(ToResponse(entity));
    }

    [HttpGet("{id:long}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> ListComments(
        long id,
        CancellationToken cancellationToken)
    {
        var exists = await _db.PlatformCommunityPosts
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (!exists)
        {
            return this.ToNotFoundProblem("게시글을 찾을 수 없습니다.");
        }

        var comments = await _db.PlatformCommunityPostComments
            .AsNoTracking()
            .Where(x => x.PostId == id && !x.IsDeleted && !x.IsOperatorHidden)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .Select(x => ToCommentResponse(x))
            .ToListAsync(cancellationToken);

        return Ok(comments);
    }

    [HttpPost("{id:long}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateComment(
        long id,
        [FromBody] PlatformCommunityPostCommentCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateComment(request);
        if (validation is not null)
        {
            return this.ToProblemActionResult(validation.Title ?? "댓글 입력값을 확인해야 합니다.");
        }

        var entity = await _db.PlatformCommunityPosts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return this.ToNotFoundProblem("게시글을 찾을 수 없습니다.");
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

        return Ok(ToCommentResponse(comment));
    }

    [HttpDelete("{id:long}/comments/{commentId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteComment(
        long id,
        long commentId,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return this.ToProblemActionResult("Password is required.");
        }

        var comment = await _db.PlatformCommunityPostComments
            .Include(x => x.Post)
            .FirstOrDefaultAsync(x => x.Id == commentId && x.PostId == id && !x.IsDeleted && x.Post != null && !x.Post.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return this.ToNotFoundProblem("댓글을 찾을 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), comment.PasswordHash))
        {
            return this.ToForbiddenProblem("댓글 비밀번호가 일치하지 않습니다.");
        }

        comment.IsDeleted = true;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        comment.Post.CommentCount = Math.Max(0, comment.Post.CommentCount - 1);
        comment.Post.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("comments/{commentId:long}/reports")]
    [AllowAnonymous]
    public async Task<IActionResult> ReportComment(long commentId, CancellationToken cancellationToken)
    {
        var comment = await _db.PlatformCommunityPostComments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return this.ToNotFoundProblem("댓글을 찾을 수 없습니다.");
        }

        comment.ReportCount += 1;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("comments/{commentId:long}/operator-hidden")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetCommentOperatorHidden(
        long commentId,
        [FromBody] PlatformCommunityOperatorHiddenRequest request,
        CancellationToken cancellationToken)
    {
        var comment = await _db.PlatformCommunityPostComments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return this.ToNotFoundProblem("댓글을 찾을 수 없습니다.");
        }

        comment.IsOperatorHidden = request.IsOperatorHidden;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("attachments/{attachmentId:long}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> ListAttachmentComments(
        long attachmentId,
        CancellationToken cancellationToken)
    {
        var exists = await _db.PlatformCommunityPostAttachments
            .AnyAsync(x => x.Id == attachmentId && x.Post != null && !x.Post.IsDeleted, cancellationToken);
        if (!exists)
        {
            return this.ToNotFoundProblem("첨부 이미지를 찾을 수 없습니다.");
        }

        var comments = await _db.PlatformCommunityPostAttachmentComments
            .AsNoTracking()
            .Where(x => x.AttachmentId == attachmentId && !x.IsDeleted && !x.IsOperatorHidden)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .Select(x => ToAttachmentCommentResponse(x))
            .ToListAsync(cancellationToken);

        return Ok(comments);
    }

    [HttpPost("attachments/{attachmentId:long}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAttachmentComment(
        long attachmentId,
        [FromBody] PlatformCommunityPostAttachmentCommentCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateAttachmentComment(request);
        if (validation is not null)
        {
            return this.ToProblemActionResult(validation.Title ?? "첨부 댓글 입력값을 확인해야 합니다.");
        }

        var attachment = await _db.PlatformCommunityPostAttachments
            .Include(x => x.Post)
            .FirstOrDefaultAsync(x => x.Id == attachmentId && x.Post != null && !x.Post.IsDeleted, cancellationToken);
        if (attachment is null)
        {
            return this.ToNotFoundProblem("첨부 이미지를 찾을 수 없습니다.");
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

        return Ok(ToAttachmentCommentResponse(comment));
    }

    [HttpDelete("attachments/{attachmentId:long}/comments/{commentId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteAttachmentComment(
        long attachmentId,
        long commentId,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return this.ToProblemActionResult("Password is required.");
        }

        var comment = await _db.PlatformCommunityPostAttachmentComments
            .Include(x => x.Attachment)
                .ThenInclude(x => x.Post)
            .FirstOrDefaultAsync(x => x.Id == commentId && x.AttachmentId == attachmentId && !x.IsDeleted && x.Attachment.Post != null && !x.Attachment.Post.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return this.ToNotFoundProblem("첨부 댓글을 찾을 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), comment.PasswordHash))
        {
            return this.ToForbiddenProblem("첨부 댓글 비밀번호가 일치하지 않습니다.");
        }

        comment.IsDeleted = true;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        comment.Attachment.CommentCount = Math.Max(0, comment.Attachment.CommentCount - 1);
        comment.Attachment.Post!.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("attachments/comments/{commentId:long}/reports")]
    [AllowAnonymous]
    public async Task<IActionResult> ReportAttachmentComment(long commentId, CancellationToken cancellationToken)
    {
        var comment = await _db.PlatformCommunityPostAttachmentComments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return this.ToNotFoundProblem("첨부 댓글을 찾을 수 없습니다.");
        }

        comment.ReportCount += 1;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("attachments/comments/{commentId:long}/operator-hidden")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> SetAttachmentCommentOperatorHidden(
        long commentId,
        [FromBody] PlatformCommunityOperatorHiddenRequest request,
        CancellationToken cancellationToken)
    {
        var comment = await _db.PlatformCommunityPostAttachmentComments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted, cancellationToken);
        if (comment is null)
        {
            return this.ToNotFoundProblem("첨부 댓글을 찾을 수 없습니다.");
        }

        comment.IsOperatorHidden = request.IsOperatorHidden;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(
        long id,
        [FromBody] PlatformCommunityPostPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return this.ToProblemActionResult("비밀번호를 입력해야 합니다.");
        }

        var entity = await _db.PlatformCommunityPosts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return this.ToNotFoundProblem("게시글을 찾을 수 없습니다.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password.Trim(), entity.PasswordHash))
        {
            return this.ToForbiddenProblem("게시글 비밀번호가 일치하지 않습니다.");
        }

        entity.IsDeleted = true;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static ProblemDetails? ValidatePost(string nickname, string password, string title, string body, string? sharedLinkUrl)
    {
        if (string.IsNullOrWhiteSpace(nickname) || nickname.Trim().Length > 40)
        {
            return new ProblemDetails { Title = "닉네임은 1자 이상 40자 이하로 입력해야 합니다." };
        }

        if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < 4 || password.Trim().Length > 100)
        {
            return new ProblemDetails { Title = "비밀번호는 4자 이상 100자 이하로 입력해야 합니다." };
        }

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 160)
        {
            return new ProblemDetails { Title = "제목은 1자 이상 160자 이하로 입력해야 합니다." };
        }

        if (string.IsNullOrWhiteSpace(body) && string.IsNullOrWhiteSpace(sharedLinkUrl))
        {
            return new ProblemDetails { Title = "본문 또는 공유 링크 중 하나는 입력해야 합니다." };
        }

        if (!string.IsNullOrWhiteSpace(body) && body.Trim().Length > 4000)
        {
            return new ProblemDetails { Title = "본문은 1자 이상 4000자 이하로 입력해야 합니다." };
        }

        if (!string.IsNullOrWhiteSpace(sharedLinkUrl) &&
            (!Uri.TryCreate(sharedLinkUrl.Trim(), UriKind.Absolute, out var uri) ||
             (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
             sharedLinkUrl.Trim().Length > 1000))
        {
            return new ProblemDetails { Title = "공유 링크는 http 또는 https URL로 입력해야 합니다." };
        }

        return null;
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

    private static ProblemDetails? ValidateComment(PlatformCommunityPostCommentCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nickname) || request.Nickname.Trim().Length > 40)
        {
            return new ProblemDetails { Title = "닉네임은 1자 이상 40자 이하로 입력해야 합니다." };
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Trim().Length < 4 || request.Password.Trim().Length > 100)
        {
            return new ProblemDetails { Title = "비밀번호는 4자 이상 100자 이하로 입력해야 합니다." };
        }

        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1000)
        {
            return new ProblemDetails { Title = "댓글은 1자 이상 1000자 이하로 입력해야 합니다." };
        }

        return null;
    }

    private static ProblemDetails? ValidateAttachmentComment(PlatformCommunityPostAttachmentCommentCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nickname) || request.Nickname.Trim().Length > 40)
        {
            return new ProblemDetails { Title = "Nickname must be 1 to 40 characters." };
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Trim().Length < 4 || request.Password.Trim().Length > 100)
        {
            return new ProblemDetails { Title = "Password must be 4 to 100 characters." };
        }

        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Trim().Length > 1000)
        {
            return new ProblemDetails { Title = "Comment must be 1 to 1000 characters." };
        }

        return null;
    }

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

    private static PlatformCommunityPostResponse ToResponse(PlatformCommunityPost entity)
    {
        var isReportBoardPost = entity.IsReportBoardPost || IsReportCategory(entity.Category);
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
            Nickname = isReportBoardPost ? reporterDisplayName : entity.Nickname,
            IsReportBoardPost = isReportBoardPost,
            ReporterDisplayName = reporterDisplayName,
            ReportedDisplayName = reportedDisplayName,
            ViewerReportRole = PlatformCommunityReportViewerRoles.Observer,
            IsReportSubjectMasked = isReportBoardPost,
            IsOperatorPinned = entity.IsOperatorPinned,
            OperatorPinnedAtUtc = entity.OperatorPinnedAtUtc,
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
    {
        return new PlatformCommunityPostCommentResponse
        {
            Id = entity.Id,
            Nickname = entity.Nickname,
            Body = entity.Body,
            ReportCount = entity.ReportCount,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    private static PlatformCommunityPostAttachmentResponse ToAttachmentResponse(PlatformCommunityPostAttachment entity)
    {
        return new PlatformCommunityPostAttachmentResponse
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
    }

    private static PlatformCommunityPostAttachmentCommentResponse ToAttachmentCommentResponse(PlatformCommunityPostAttachmentComment entity)
    {
        return new PlatformCommunityPostAttachmentCommentResponse
        {
            Id = entity.Id,
            AttachmentId = entity.AttachmentId,
            Nickname = entity.Nickname,
            Body = entity.Body,
            ReportCount = entity.ReportCount,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}

public sealed class PlatformCommunityPostAttachmentUploadRequest
{
    public string Password { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}
