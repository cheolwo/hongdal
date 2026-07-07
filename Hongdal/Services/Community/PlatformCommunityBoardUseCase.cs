using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;
using Hongdal.Domain.Community;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Services.Community;

public interface IPlatformCommunityBoardUseCase
{
    Task<Result<PlatformCommunityBoardListResponse>> 목록Async(string? appKey, string? status, CancellationToken cancellationToken);
    Task<Result<PlatformCommunityBoardResponse>> 신청Async(PlatformCommunityBoardCreateRequest? request, CancellationToken cancellationToken);
    Task<Result<PlatformCommunityBoardResponse>> 승인Async(long id, PlatformCommunityBoardReviewRequest? request, CancellationToken cancellationToken);
    Task<Result<PlatformCommunityBoardResponse>> 반려Async(long id, PlatformCommunityBoardReviewRequest? request, CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalUseCase("커뮤니티 게시판 개설", Summary = "커뮤니티 참여자가 게시판 개설을 신청하고 운영자가 승인 또는 반려합니다.")]
[HongdalUseCaseActor(HongdalActor.CommunityMember)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
public sealed class PlatformCommunityBoardUseCase : IPlatformCommunityBoardUseCase
{
    private readonly HongdalContext _db;

    public PlatformCommunityBoardUseCase(HongdalContext db)
    {
        _db = db;
    }

    public async Task<Result<PlatformCommunityBoardListResponse>> 목록Async(
        string? appKey,
        string? status,
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

        return new PlatformCommunityBoardListResponse { Items = items };
    }

    public async Task<Result<PlatformCommunityBoardResponse>> 신청Async(
        PlatformCommunityBoardCreateRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result.Fail<PlatformCommunityBoardResponse>("request body is required");
        }

        var title = Normalize(request.Title, string.Empty, 60);
        var requestedBy = Normalize(request.RequestedBy, string.Empty, 40);
        var reason = Normalize(request.RequestReason, string.Empty, 1000);
        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(requestedBy) ||
            string.IsNullOrWhiteSpace(reason))
        {
            return Result.Fail<PlatformCommunityBoardResponse>("게시판 이름, 신청자, 개설 이유를 입력해야 합니다.");
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
            return Result.Fail<PlatformCommunityBoardResponse>("이미 신청되었거나 승인된 게시판 이름입니다.");
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

        return ToResponse(entity);
    }

    public Task<Result<PlatformCommunityBoardResponse>> 승인Async(
        long id,
        PlatformCommunityBoardReviewRequest? request,
        CancellationToken cancellationToken)
        => 검토Async(id, request, PlatformCommunityBoardRequestStatuses.Approved, cancellationToken);

    public Task<Result<PlatformCommunityBoardResponse>> 반려Async(
        long id,
        PlatformCommunityBoardReviewRequest? request,
        CancellationToken cancellationToken)
        => 검토Async(id, request, PlatformCommunityBoardRequestStatuses.Rejected, cancellationToken);

    private async Task<Result<PlatformCommunityBoardResponse>> 검토Async(
        long id,
        PlatformCommunityBoardReviewRequest? request,
        string status,
        CancellationToken cancellationToken)
    {
        var entity = await _db.PlatformCommunityBoardRequests
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return Result.Fail<PlatformCommunityBoardResponse>("게시판 개설 신청을 찾을 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        entity.Status = status;
        entity.OperatorMemo = Normalize(request?.OperatorMemo, string.Empty, 1000);
        entity.ApprovedAtUtc = status == PlatformCommunityBoardRequestStatuses.Approved ? now : null;
        entity.RejectedAtUtc = status == PlatformCommunityBoardRequestStatuses.Rejected ? now : null;
        entity.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(entity);
    }

    private static PlatformCommunityBoardResponse ToResponse(PlatformCommunityBoardRequest entity)
        => new()
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

    private static string ToStatusName(string status)
        => status switch
        {
            PlatformCommunityBoardRequestStatuses.Pending => "승인 대기",
            PlatformCommunityBoardRequestStatuses.Approved => "개설 승인",
            PlatformCommunityBoardRequestStatuses.Rejected => "반려",
            _ => status
        };

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
