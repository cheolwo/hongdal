using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;
using Hongdal.Domain.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/boards")]
public sealed class PlatformCommunityBoardsController : ControllerBase
{
    private readonly HongdalContext _db;

    public PlatformCommunityBoardsController(HongdalContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PlatformCommunityBoardListResponse>> List(
        [FromQuery] string? appKey,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var normalizedAppKey = Normalize(appKey, "platform", 80);
        var normalizedStatus = Normalize(status, PlatformCommunityBoardRequestStatuses.Approved, 20);

        var query = _db.PlatformCommunityBoardRequests
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Where(x => x.AppKey == normalizedAppKey || x.AppKey == "platform");

        if (!string.Equals(normalizedStatus, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Status == normalizedStatus);
        }

        var items = await query
            .OrderBy(x => x.Status == PlatformCommunityBoardRequestStatuses.Pending ? 0 : 1)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return Ok(new PlatformCommunityBoardListResponse { Items = items });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(
        [FromBody] PlatformCommunityBoardCreateRequest request,
        CancellationToken cancellationToken)
    {
        var title = Normalize(request.Title, string.Empty, 60);
        var requestedBy = Normalize(request.RequestedBy, string.Empty, 40);
        var reason = Normalize(request.RequestReason, string.Empty, 1000);
        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(requestedBy) ||
            string.IsNullOrWhiteSpace(reason))
        {
            return this.ToProblemActionResult("게시판 이름, 신청자, 개설 이유를 입력해야 합니다.");
        }

        var appKey = Normalize(request.AppKey, "platform", 80);
        var boardKey = CreateBoardKey(title);
        var exists = await _db.PlatformCommunityBoardRequests
            .AnyAsync(x => x.AppKey == appKey &&
                           x.BoardKey == boardKey &&
                           !x.IsDeleted &&
                           x.Status != PlatformCommunityBoardRequestStatuses.Rejected,
                cancellationToken);
        if (exists)
        {
            return this.ToProblemActionResult("이미 신청되었거나 승인된 게시판 이름입니다.");
        }

        var now = DateTime.UtcNow;
        var entity = new PlatformCommunityBoardRequest
        {
            AppKey = appKey,
            BoardKey = boardKey,
            Title = title,
            Description = Normalize(request.Description, string.Empty, 500),
            RequestedBy = requestedBy,
            RequestReason = reason,
            Status = PlatformCommunityBoardRequestStatuses.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.PlatformCommunityBoardRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(List), new { appKey = entity.AppKey, status = entity.Status }, ToResponse(entity));
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> Approve(
        long id,
        [FromBody] PlatformCommunityBoardReviewRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.PlatformCommunityBoardRequests
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return this.ToNotFoundProblem("게시판 개설 신청을 찾을 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        entity.Status = PlatformCommunityBoardRequestStatuses.Approved;
        entity.OperatorMemo = Normalize(request.OperatorMemo, string.Empty, 1000);
        entity.ApprovedAtUtc = now;
        entity.RejectedAtUtc = null;
        entity.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(entity));
    }

    [HttpPost("{id:long}/reject")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> Reject(
        long id,
        [FromBody] PlatformCommunityBoardReviewRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.PlatformCommunityBoardRequests
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return this.ToNotFoundProblem("게시판 개설 신청을 찾을 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        entity.Status = PlatformCommunityBoardRequestStatuses.Rejected;
        entity.OperatorMemo = Normalize(request.OperatorMemo, string.Empty, 1000);
        entity.RejectedAtUtc = now;
        entity.ApprovedAtUtc = null;
        entity.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(entity));
    }

    private static PlatformCommunityBoardResponse ToResponse(PlatformCommunityBoardRequest entity)
    {
        return new PlatformCommunityBoardResponse
        {
            Id = entity.Id,
            AppKey = entity.AppKey,
            BoardKey = entity.BoardKey,
            Title = entity.Title,
            Description = entity.Description,
            RequestedBy = entity.RequestedBy,
            RequestReason = entity.RequestReason,
            Status = entity.Status,
            StatusName = ToStatusName(entity.Status),
            OperatorMemo = entity.OperatorMemo,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            ApprovedAtUtc = entity.ApprovedAtUtc,
            RejectedAtUtc = entity.RejectedAtUtc
        };
    }

    private static string ToStatusName(string status)
    {
        return status switch
        {
            PlatformCommunityBoardRequestStatuses.Pending => "승인 대기",
            PlatformCommunityBoardRequestStatuses.Approved => "개설 승인",
            PlatformCommunityBoardRequestStatuses.Rejected => "반려",
            _ => status
        };
    }

    private static string CreateBoardKey(string title)
    {
        var normalized = new string(title.Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());

        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        return Normalize(normalized.Trim('-'), Guid.NewGuid().ToString("N")[..12], 80);
    }

    private static string Normalize(string? value, string fallback, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
