using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Hr;
using 살뜰.Data;

namespace Ssalddel.Application.HumanResources;

public interface IHR역할검토조회UseCase
{
    Task<Result<HrRoleReviewListResponse>> 목록Async(
        HrRoleReviewListRequest request,
        CancellationToken cancellationToken);

    Task<Result<HrRoleReviewDetailResponse>> 상세Async(
        Guid reviewId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.HrParticipation)]
[SsalddelUseCase(
    "HR 역할 지원·배정 검토 조회",
    Summary = "인력 관리자가 영속 역할 지원·철회와 역할 배정·해제 원장을 하나의 읽기 투영에서 검색합니다.")]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator)]
public sealed class HR역할검토조회UseCase(SsalddelContext db) : IHR역할검토조회UseCase
{
    public async Task<Result<HrRoleReviewListResponse>> 목록Async(
        HrRoleReviewListRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sourceCode = request.SourceCode.Trim();
        if (!string.IsNullOrWhiteSpace(sourceCode) && !HrRoleReviewSourceCodes.IsKnown(sourceCode))
        {
            return BadRequest<HrRoleReviewListResponse>("지원하지 않는 역할 검토 원장입니다.");
        }

        var statusCode = request.StatusCode.Trim();
        if (!string.IsNullOrWhiteSpace(statusCode) && !HrRoleReviewStatusCodes.IsKnown(statusCode))
        {
            return BadRequest<HrRoleReviewListResponse>("지원하지 않는 역할 검토 상태입니다.");
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = 검토Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            if (Guid.TryParse(search, out var reviewId))
            {
                query = query.Where(item => item.ReviewId == reviewId);
            }
            else
            {
                query = query.Where(item =>
                    item.UserId.Contains(search)
                    || item.UserName.Contains(search)
                    || item.ParticipantDisplayName.Contains(search)
                    || item.RoleCode.Contains(search)
                    || item.RoleName.Contains(search)
                    || item.ScopeId.Contains(search));
            }
        }

        if (!string.IsNullOrWhiteSpace(sourceCode))
        {
            query = query.Where(item => item.SourceCode == sourceCode);
        }

        if (!string.IsNullOrWhiteSpace(statusCode))
        {
            query = query.Where(item => item.StatusCode == statusCode);
        }

        if (!string.IsNullOrWhiteSpace(request.ParticipantCategory))
        {
            var participantCategory = request.ParticipantCategory.Trim();
            query = query.Where(item => item.ParticipantCategory == participantCategory);
        }

        if (!string.IsNullOrWhiteSpace(request.ScopeType))
        {
            var scopeType = request.ScopeType.Trim();
            query = query.Where(item => item.ScopeType == scopeType);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(item => item.RecordedAtUtc)
            .ThenByDescending(item => item.UpdatedAtUtc)
            .ThenBy(item => item.ReviewId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return Result.Ok(new HrRoleReviewListResponse
        {
            Items = rows.Select(요약생성).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<HrRoleReviewDetailResponse>> 상세Async(
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        if (reviewId == Guid.Empty)
        {
            return BadRequest<HrRoleReviewDetailResponse>("조회할 역할 검토 ID를 확인해 주세요.");
        }

        var row = await 검토Query()
            .SingleOrDefaultAsync(item => item.ReviewId == reviewId, cancellationToken);
        if (row is null)
        {
            return Result.Fail<HrRoleReviewDetailResponse>(new Error("HR 역할 검토 기록을 찾을 수 없습니다.")
                .WithMetadata("StatusCode", StatusCodes.Status404NotFound));
        }

        var recordedByDisplayName = string.Equals(
            row.SourceCode,
            HrRoleReviewSourceCodes.RoleApplication,
            StringComparison.Ordinal)
            ? "지원자 본인"
            : await 처리자표시이름Async(row.RecordedByUserId, cancellationToken);

        return Result.Ok(new HrRoleReviewDetailResponse
        {
            ReviewId = row.ReviewId,
            SourceCode = row.SourceCode,
            SourceName = HrRoleReviewSourceCodes.GetDisplayName(row.SourceCode),
            ParticipantDisplayName = 표시이름(row),
            ParticipantUserName = 사용자이름(row),
            ParticipantCategory = row.ParticipantCategory,
            ParticipantCategoryName = HrParticipantCategoryCodes.GetDisplayName(row.ParticipantCategory),
            ScopeType = row.ScopeType,
            ScopeId = row.ScopeId,
            RoleCode = row.RoleCode,
            RoleName = row.RoleName,
            StatusCode = row.StatusCode,
            StatusName = HrRoleReviewStatusCodes.GetDisplayName(row.StatusCode),
            RecordedByDisplayName = recordedByDisplayName,
            WorkScheduleEnabled = row.WorkScheduleEnabled,
            TimeZoneId = string.IsNullOrWhiteSpace(row.TimeZoneId)
                ? HrWorkScheduleDefaults.TimeZoneId
                : row.TimeZoneId,
            AllowedDaysOfWeek = ParseDays(row.AllowedDaysOfWeekCsv),
            WorkStartLocalTime = ParseTime(row.WorkStartLocalTimeText),
            WorkEndLocalTime = ParseTime(row.WorkEndLocalTimeText),
            WorksiteIpRestrictionEnabled = row.WorksiteIpRestrictionEnabled,
            AllowedWorksiteIpRangeCount = CountIpRanges(row.AllowedWorksiteIpRangesText),
            ConfirmedVoluntaryApplication = row.ConfirmedVoluntaryApplication,
            ConfirmedNoRoleOrEmploymentGuarantee = row.ConfirmedNoRoleOrEmploymentGuarantee,
            ConfirmedReviewDataUse = row.ConfirmedReviewDataUse,
            ConsentVersion = row.ConsentVersion,
            WithdrawnAtUtc = row.WithdrawnAtUtc,
            RecordedAtUtc = row.RecordedAtUtc,
            UpdatedAtUtc = row.UpdatedAtUtc
        });
    }

    internal IQueryable<RoleReviewQueryRow> 검토Query()
    {
        var assignments = db.HrRoleAssignments.AsNoTracking().Select(assignment => new RoleReviewSourceRow
        {
            ReviewId = assignment.Id,
            SourceCode = HrRoleReviewSourceCodes.RoleAssignment,
            UserId = assignment.UserId,
            ParticipantCategory = assignment.ParticipantCategory,
            ScopeType = assignment.ScopeType,
            ScopeId = assignment.ScopeId,
            RoleCode = assignment.RoleCode,
            RoleName = assignment.RoleName,
            StatusCode = assignment.IsActive
                ? HrRoleReviewStatusCodes.Assigned
                : HrRoleReviewStatusCodes.Revoked,
            RecordedByUserId = assignment.AssignedByUserId,
            WorkScheduleEnabled = assignment.WorkScheduleEnabled,
            TimeZoneId = assignment.TimeZoneId,
            AllowedDaysOfWeekCsv = assignment.AllowedDaysOfWeekCsv,
            WorkStartLocalTimeText = assignment.WorkStartLocalTimeText,
            WorkEndLocalTimeText = assignment.WorkEndLocalTimeText,
            WorksiteIpRestrictionEnabled = assignment.WorksiteIpRestrictionEnabled,
            AllowedWorksiteIpRangesText = assignment.AllowedWorksiteIpRangesText,
            ConfirmedVoluntaryApplication = false,
            ConfirmedNoRoleOrEmploymentGuarantee = false,
            ConfirmedReviewDataUse = false,
            ConsentVersion = string.Empty,
            WithdrawnAtUtc = null,
            RecordedAtUtc = assignment.AssignedAtUtc,
            UpdatedAtUtc = assignment.UpdatedAt
        });

        var applications = db.HrRoleApplications.AsNoTracking().Select(application => new RoleReviewSourceRow
        {
            ReviewId = application.Id,
            SourceCode = HrRoleReviewSourceCodes.RoleApplication,
            UserId = application.ApplicantUserId,
            ParticipantCategory = application.ParticipantCategory,
            ScopeType = application.ScopeType,
            ScopeId = application.ScopeId,
            RoleCode = application.RequestedRoleCode,
            RoleName = application.RequestedRoleName,
            StatusCode = application.StatusCode,
            RecordedByUserId = application.ApplicantUserId,
            WorkScheduleEnabled = false,
            TimeZoneId = HrWorkScheduleDefaults.TimeZoneId,
            AllowedDaysOfWeekCsv = string.Empty,
            WorkStartLocalTimeText = null,
            WorkEndLocalTimeText = null,
            WorksiteIpRestrictionEnabled = false,
            AllowedWorksiteIpRangesText = string.Empty,
            ConfirmedVoluntaryApplication = application.ConfirmedVoluntaryApplication,
            ConfirmedNoRoleOrEmploymentGuarantee = application.ConfirmedNoRoleOrEmploymentGuarantee,
            ConfirmedReviewDataUse = application.ConfirmedReviewDataUse,
            ConsentVersion = application.ConsentVersion,
            WithdrawnAtUtc = application.WithdrawnAtUtc,
            RecordedAtUtc = application.SubmittedAtUtc,
            UpdatedAtUtc = application.UpdatedAt
        });

        var sources = assignments.Concat(applications);
        return from source in sources
               join user in db.Users.AsNoTracking()
                   on source.UserId equals user.Id into users
               from user in users.DefaultIfEmpty()
               join participant in db.살뜰참여자.AsNoTracking()
                   on source.UserId equals participant.Id into participants
               from participant in participants.DefaultIfEmpty()
               select new RoleReviewQueryRow
               {
                   ReviewId = source.ReviewId,
                   SourceCode = source.SourceCode,
                   UserId = source.UserId,
                   UserName = user == null ? string.Empty : user.UserName ?? string.Empty,
                   ParticipantDisplayName = participant == null ? string.Empty : participant.표시이름,
                   ParticipantCategory = source.ParticipantCategory,
                   ScopeType = source.ScopeType,
                   ScopeId = source.ScopeId,
                   RoleCode = source.RoleCode,
                   RoleName = source.RoleName,
                   StatusCode = source.StatusCode,
                   RecordedByUserId = source.RecordedByUserId,
                   WorkScheduleEnabled = source.WorkScheduleEnabled,
                   TimeZoneId = source.TimeZoneId,
                   AllowedDaysOfWeekCsv = source.AllowedDaysOfWeekCsv,
                   WorkStartLocalTimeText = source.WorkStartLocalTimeText,
                   WorkEndLocalTimeText = source.WorkEndLocalTimeText,
                   WorksiteIpRestrictionEnabled = source.WorksiteIpRestrictionEnabled,
                   AllowedWorksiteIpRangesText = source.AllowedWorksiteIpRangesText,
                   ConfirmedVoluntaryApplication = source.ConfirmedVoluntaryApplication,
                   ConfirmedNoRoleOrEmploymentGuarantee = source.ConfirmedNoRoleOrEmploymentGuarantee,
                   ConfirmedReviewDataUse = source.ConfirmedReviewDataUse,
                   ConsentVersion = source.ConsentVersion,
                   WithdrawnAtUtc = source.WithdrawnAtUtc,
                   RecordedAtUtc = source.RecordedAtUtc,
                   UpdatedAtUtc = source.UpdatedAtUtc
               };
    }

    private async Task<string> 처리자표시이름Async(string userId, CancellationToken cancellationToken)
    {
        var userName = await db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.UserName)
            .SingleOrDefaultAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(userName) ? "시스템 또는 탈퇴 계정" : userName;
    }

    private static HrRoleReviewSummaryResponse 요약생성(RoleReviewQueryRow row)
        => new()
        {
            ReviewId = row.ReviewId,
            SourceCode = row.SourceCode,
            SourceName = HrRoleReviewSourceCodes.GetDisplayName(row.SourceCode),
            ParticipantDisplayName = 표시이름(row),
            ParticipantUserName = 사용자이름(row),
            ParticipantCategory = row.ParticipantCategory,
            ParticipantCategoryName = HrParticipantCategoryCodes.GetDisplayName(row.ParticipantCategory),
            ScopeType = row.ScopeType,
            ScopeId = row.ScopeId,
            RoleCode = row.RoleCode,
            RoleName = row.RoleName,
            StatusCode = row.StatusCode,
            StatusName = HrRoleReviewStatusCodes.GetDisplayName(row.StatusCode),
            WorkScheduleEnabled = row.WorkScheduleEnabled,
            WorksiteIpRestrictionEnabled = row.WorksiteIpRestrictionEnabled,
            RecordedAtUtc = row.RecordedAtUtc,
            UpdatedAtUtc = row.UpdatedAtUtc
        };

    private static string 표시이름(RoleReviewQueryRow row)
        => !string.IsNullOrWhiteSpace(row.ParticipantDisplayName)
            ? row.ParticipantDisplayName.Trim()
            : 사용자이름(row);

    private static string 사용자이름(RoleReviewQueryRow row)
        => !string.IsNullOrWhiteSpace(row.UserName)
            ? row.UserName.Trim()
            : 마스킹참조(row.UserId);

    private static string 마스킹참조(string value)
    {
        var normalized = value.Trim();
        return normalized.Length switch
        {
            0 => "확인되지 않은 사용자",
            <= 3 => $"{normalized[0]}**",
            _ => $"{normalized[..3]}***"
        };
    }

    private static IReadOnlyList<DayOfWeek> ParseDays(string value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(item => Enum.TryParse<DayOfWeek>(item, out var day) ? day : (DayOfWeek?)null)
                .Where(day => day.HasValue)
                .Select(day => day!.Value)
                .Distinct()
                .OrderBy(day => day)
                .ToArray();

    private static TimeOnly? ParseTime(string? value)
        => TimeOnly.TryParse(value, out var time) ? time : null;

    private static int CountIpRanges(string value)
        => string.IsNullOrWhiteSpace(value)
            ? 0
            : value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

    private static Result<T> BadRequest<T>(string message)
        => Result.Fail<T>(new Error(message)
            .WithMetadata("StatusCode", StatusCodes.Status400BadRequest));

    internal class RoleReviewSourceRow
    {
        public Guid ReviewId { get; init; }
        public string SourceCode { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public string ParticipantCategory { get; init; } = string.Empty;
        public string ScopeType { get; init; } = string.Empty;
        public string ScopeId { get; init; } = string.Empty;
        public string RoleCode { get; init; } = string.Empty;
        public string RoleName { get; init; } = string.Empty;
        public string StatusCode { get; init; } = string.Empty;
        public string RecordedByUserId { get; init; } = string.Empty;
        public bool WorkScheduleEnabled { get; init; }
        public string TimeZoneId { get; init; } = string.Empty;
        public string AllowedDaysOfWeekCsv { get; init; } = string.Empty;
        public string? WorkStartLocalTimeText { get; init; }
        public string? WorkEndLocalTimeText { get; init; }
        public bool WorksiteIpRestrictionEnabled { get; init; }
        public string AllowedWorksiteIpRangesText { get; init; } = string.Empty;
        public bool ConfirmedVoluntaryApplication { get; init; }
        public bool ConfirmedNoRoleOrEmploymentGuarantee { get; init; }
        public bool ConfirmedReviewDataUse { get; init; }
        public string ConsentVersion { get; init; } = string.Empty;
        public DateTime? WithdrawnAtUtc { get; init; }
        public DateTime RecordedAtUtc { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
    }

    internal sealed class RoleReviewQueryRow : RoleReviewSourceRow
    {
        public string UserName { get; init; } = string.Empty;
        public string ParticipantDisplayName { get; init; } = string.Empty;
    }
}
