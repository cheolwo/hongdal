using System.Text.Json;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ViewSettings;
using 홍달.Services.Audit;

namespace Hongdal.Services.Community;

public interface ICommunityExperienceAwardService
{
    Task<CommunityExperienceAwardResult> RecordAsync(
        CommunityExperienceAwardRequest request,
        CancellationToken cancellationToken);
}

public sealed record CommunityExperienceAwardRequest(
    string UserId,
    string RoleName,
    string EventCode,
    string SourceKind,
    string SourceId,
    string SourceDisplayId,
    string Route,
    string TraceId,
    DateTime OccurredAtUtc,
    string AppKey = App식별자.DriverApp);

public sealed record CommunityExperienceAwardResult(
    bool 처리됨,
    string EventCode,
    int BaseExperience,
    string 사유);

public static class CommunityExperienceActionTypes
{
    public const string ExperienceAward = "ExperienceAward";
}

public sealed class CommunityExperienceAwardService : ICommunityExperienceAwardService
{
    private static readonly IReadOnlyDictionary<string, CommunityLedgerExperienceEventResponse> ExperienceEvents =
        CommunityLedgerExperiencePolicyResponse.Default()
            .ExperienceEvents
            .ToDictionary(x => x.EventCode, StringComparer.OrdinalIgnoreCase);

    private readonly I사용자행위로그Service _activityLogService;

    public CommunityExperienceAwardService(I사용자행위로그Service activityLogService)
    {
        _activityLogService = activityLogService;
    }

    public async Task<CommunityExperienceAwardResult> RecordAsync(
        CommunityExperienceAwardRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Skipped(request.EventCode, "경험치를 받을 사용자 id가 없습니다.");
        }

        if (!ExperienceEvents.TryGetValue(request.EventCode, out var experienceEvent))
        {
            return Skipped(request.EventCode, "등록되지 않은 경험치 이벤트입니다.");
        }

        if (experienceEvent.BaseExperience <= 0)
        {
            return Skipped(request.EventCode, "경험치가 0 이하인 이벤트입니다.");
        }

        await _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = Normalize(request.AppKey, App식별자.DriverApp),
            UserId = request.UserId.Trim(),
            RoleName = Normalize(request.RoleName, "플랫폼 구성원"),
            ActionType = CommunityExperienceActionTypes.ExperienceAward,
            ActionName = experienceEvent.DisplayName,
            Route = Normalize(request.Route, "community-experience"),
            TraceId = request.TraceId ?? string.Empty,
            IsSuccess = true,
            OccurredAtUtc = request.OccurredAtUtc == default ? DateTime.UtcNow : request.OccurredAtUtc,
            MetadataJson = BuildMetadataJson(request, experienceEvent)
        }, cancellationToken);

        return new CommunityExperienceAwardResult(
            true,
            experienceEvent.EventCode,
            experienceEvent.BaseExperience,
            "경험치 적립 이벤트를 기록했습니다.");
    }

    private static CommunityExperienceAwardResult Skipped(string eventCode, string reason)
        => new(false, eventCode ?? string.Empty, 0, reason);

    private static string BuildMetadataJson(
        CommunityExperienceAwardRequest request,
        CommunityLedgerExperienceEventResponse experienceEvent)
        => JsonSerializer.Serialize(new
        {
            experienceEvent.EventCode,
            experienceEvent.DisplayName,
            experienceEvent.BaseExperience,
            experienceEvent.AuditSource,
            request.SourceKind,
            request.SourceId,
            request.SourceDisplayId,
            Policy = nameof(CommunityLedgerExperiencePolicyResponse),
            Stage = "AwardRequested"
        });

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
