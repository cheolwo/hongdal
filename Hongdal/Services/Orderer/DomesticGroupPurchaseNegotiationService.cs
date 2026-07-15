using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Services.Orderer;

public interface IDomesticGroupPurchaseNegotiationClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemDomesticGroupPurchaseNegotiationClock : IDomesticGroupPurchaseNegotiationClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class DomesticGroupPurchaseNegotiationCampaignState
{
    public object SyncRoot { get; } = new();
    public List<DomesticGroupPurchaseNegotiationEventResponse> Events { get; } = [];
    public List<DomesticGroupPurchaseNegotiationIssueState> Issues { get; } = [];
}

public sealed class DomesticGroupPurchaseNegotiationIssueState
{
    public required DomesticGroupPurchaseNegotiationIssueResponse PublicIssue { get; init; }
    public List<DomesticGroupPurchaseNegotiationPositionState> Positions { get; } = [];
}

public sealed class DomesticGroupPurchaseNegotiationPositionState
{
    public required string AuthorUserId { get; init; }
    public required DomesticGroupPurchaseDeliberationPositionResponse PublicPosition { get; init; }
}

public interface IDomesticGroupPurchaseNegotiationStore
{
    DomesticGroupPurchaseNegotiationCampaignState GetOrCreate(Guid campaignId);
}

public sealed class InMemoryDomesticGroupPurchaseNegotiationStore : IDomesticGroupPurchaseNegotiationStore
{
    private readonly ConcurrentDictionary<Guid, DomesticGroupPurchaseNegotiationCampaignState> campaigns = new();

    public DomesticGroupPurchaseNegotiationCampaignState GetOrCreate(Guid campaignId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(campaignId, Guid.Empty);
        return campaigns.GetOrAdd(campaignId, static _ => new DomesticGroupPurchaseNegotiationCampaignState());
    }
}

public interface IDomesticGroupPurchaseNegotiationService
{
    DomesticGroupPurchaseNegotiationTimelineResponse GetTimeline(Guid campaignId);
    DomesticGroupPurchaseNegotiationEventResponse AppendEvent(Guid campaignId, string userId, DomesticGroupPurchaseNegotiationEventRequest request);
    DomesticGroupPurchaseNegotiationIssueResponse OpenIssue(Guid campaignId, string userId, DomesticGroupPurchaseNegotiationIssueRequest request);
    DomesticGroupPurchaseNegotiationIssueResponse AddPosition(Guid campaignId, Guid issueId, string userId, DomesticGroupPurchaseDeliberationPositionRequest request);
    DomesticGroupPurchaseNegotiationIssueResponse ResolveIssue(Guid campaignId, Guid issueId, string userId, DomesticGroupPurchaseNegotiationResolutionRequest request);
}

public sealed partial class DomesticGroupPurchaseNegotiationService : IDomesticGroupPurchaseNegotiationService
{
    private const int MinimumDeliberationHours = 1;
    private const int MaximumDeliberationHours = 168;
    private const int MinimumDistinctParticipants = 2;

    private static readonly HashSet<string> PublicEventTypes =
    [
        DomesticGroupPurchaseNegotiationEventTypeCodes.Proposal,
        DomesticGroupPurchaseNegotiationEventTypeCodes.CounterProposal,
        DomesticGroupPurchaseNegotiationEventTypeCodes.Clarification,
        DomesticGroupPurchaseNegotiationEventTypeCodes.Agreement
    ];

    private static readonly HashSet<string> PositionCodes =
    [
        DomesticGroupPurchaseDeliberationPositionCodes.Support,
        DomesticGroupPurchaseDeliberationPositionCodes.Concern,
        DomesticGroupPurchaseDeliberationPositionCodes.Alternative
    ];

    private readonly IDomesticGroupPurchaseNegotiationStore store;
    private readonly IDomesticGroupPurchaseNegotiationClock clock;

    public DomesticGroupPurchaseNegotiationService(
        IDomesticGroupPurchaseNegotiationStore store,
        IDomesticGroupPurchaseNegotiationClock clock)
    {
        this.store = store;
        this.clock = clock;
    }

    public DomesticGroupPurchaseNegotiationTimelineResponse GetTimeline(Guid campaignId)
    {
        var state = store.GetOrCreate(campaignId);
        lock (state.SyncRoot)
        {
            return BuildTimeline(campaignId, state);
        }
    }

    public DomesticGroupPurchaseNegotiationEventResponse AppendEvent(
        Guid campaignId,
        string userId,
        DomesticGroupPurchaseNegotiationEventRequest request)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(request);
        if (!PublicEventTypes.Contains(request.EventTypeCode))
        {
            throw new ArgumentException("공개 협의에는 제안, 수정 제안, 확인 질문, 합의 이벤트만 등록할 수 있습니다.", nameof(request));
        }

        var created = CreateEvent(
            request.EventTypeCode,
            request.MaskedActorDisplayName,
            request.ActorRoleLabel,
            request.PublicSummary);
        var state = store.GetOrCreate(campaignId);
        lock (state.SyncRoot)
        {
            state.Events.Add(created);
        }

        return CopyEvent(created);
    }

    public DomesticGroupPurchaseNegotiationIssueResponse OpenIssue(
        Guid campaignId,
        string userId,
        DomesticGroupPurchaseNegotiationIssueRequest request)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(request);
        if (request.DeliberationHours is < MinimumDeliberationHours or > MaximumDeliberationHours)
        {
            throw new ArgumentOutOfRangeException(nameof(request), $"숙고 기간은 {MinimumDeliberationHours}시간 이상 {MaximumDeliberationHours}시간 이하여야 합니다.");
        }

        var now = clock.UtcNow;
        var issue = new DomesticGroupPurchaseNegotiationIssueResponse
        {
            IssueId = Guid.NewGuid(),
            Title = RequirePublicText(request.Title, "쟁점 제목", 120),
            PublicSummary = RequirePublicText(request.PublicSummary, "쟁점 설명", 2000),
            MaskedReporterDisplayName = RequirePublicText(request.MaskedReporterDisplayName, "작성자 표시명", 80),
            ReporterRoleLabel = RequirePublicText(request.ReporterRoleLabel, "작성자 역할", 80),
            OpenedAtUtc = now,
            DeliberationClosesAtUtc = now.AddHours(request.DeliberationHours)
        };

        var state = store.GetOrCreate(campaignId);
        lock (state.SyncRoot)
        {
            state.Issues.Add(new DomesticGroupPurchaseNegotiationIssueState { PublicIssue = issue });
            state.Events.Add(CreateEvent(
                DomesticGroupPurchaseNegotiationEventTypeCodes.IssueRaised,
                issue.MaskedReporterDisplayName,
                issue.ReporterRoleLabel,
                $"{issue.Title}: {issue.PublicSummary}",
                issue.IssueId));
            return BuildIssue(state.Issues[^1]);
        }
    }

    public DomesticGroupPurchaseNegotiationIssueResponse AddPosition(
        Guid campaignId,
        Guid issueId,
        string userId,
        DomesticGroupPurchaseDeliberationPositionRequest request)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(request);
        if (!PositionCodes.Contains(request.PositionCode))
        {
            throw new ArgumentException("의견은 지지, 우려, 대안 중 하나여야 합니다.", nameof(request));
        }

        var state = store.GetOrCreate(campaignId);
        lock (state.SyncRoot)
        {
            var issueState = FindIssue(state, issueId);
            if (issueState.PublicIssue.StatusCode == DomesticGroupPurchaseNegotiationIssueStatusCodes.Resolved)
            {
                throw new InvalidOperationException("이미 해결된 쟁점에는 의견을 추가할 수 없습니다.");
            }

            var position = new DomesticGroupPurchaseDeliberationPositionResponse
            {
                PositionId = Guid.NewGuid(),
                PositionCode = request.PositionCode,
                MaskedParticipantDisplayName = RequirePublicText(request.MaskedParticipantDisplayName, "참여자 표시명", 80),
                ParticipantRoleLabel = RequirePublicText(request.ParticipantRoleLabel, "참여자 역할", 80),
                PublicRationale = RequirePublicText(request.PublicRationale, "공개 의견", 2000),
                CreatedAtUtc = clock.UtcNow
            };
            issueState.Positions.Add(new DomesticGroupPurchaseNegotiationPositionState
            {
                AuthorUserId = userId.Trim(),
                PublicPosition = position
            });
            state.Events.Add(CreateEvent(
                DomesticGroupPurchaseNegotiationEventTypeCodes.DeliberationOpinion,
                position.MaskedParticipantDisplayName,
                position.ParticipantRoleLabel,
                position.PublicRationale,
                issueId));
            return BuildIssue(issueState);
        }
    }

    public DomesticGroupPurchaseNegotiationIssueResponse ResolveIssue(
        Guid campaignId,
        Guid issueId,
        string userId,
        DomesticGroupPurchaseNegotiationResolutionRequest request)
    {
        ValidateUser(userId);
        ArgumentNullException.ThrowIfNull(request);
        var state = store.GetOrCreate(campaignId);
        lock (state.SyncRoot)
        {
            var issueState = FindIssue(state, issueId);
            var issue = issueState.PublicIssue;
            if (issue.StatusCode == DomesticGroupPurchaseNegotiationIssueStatusCodes.Resolved)
            {
                throw new InvalidOperationException("이미 해결된 쟁점입니다.");
            }

            if (clock.UtcNow < issue.DeliberationClosesAtUtc)
            {
                throw new InvalidOperationException("숙고 종료 시각 전에는 쟁점을 해결 처리할 수 없습니다.");
            }

            if (DistinctParticipants(issueState) < MinimumDistinctParticipants)
            {
                throw new InvalidOperationException($"서로 다른 구성원 {MinimumDistinctParticipants}명 이상의 공개 의견이 필요합니다.");
            }

            if (!issueState.Positions.Any(item => string.Equals(item.AuthorUserId, userId.Trim(), StringComparison.Ordinal)))
            {
                throw new InvalidOperationException("공개 숙고에 의견을 남긴 구성원만 해결안을 기록할 수 있습니다.");
            }

            issue.StatusCode = DomesticGroupPurchaseNegotiationIssueStatusCodes.Resolved;
            issue.Resolution = new DomesticGroupPurchaseNegotiationResolutionResponse
            {
                MaskedResolverDisplayName = RequirePublicText(request.MaskedResolverDisplayName, "결정자 표시명", 80),
                ResolverRoleLabel = RequirePublicText(request.ResolverRoleLabel, "결정자 역할", 80),
                ResolutionSummary = RequirePublicText(request.ResolutionSummary, "결정 내용", 1000),
                DecisionRationale = RequirePublicText(request.DecisionRationale, "결정 근거", 2000),
                ResolvedAtUtc = clock.UtcNow
            };
            state.Events.Add(CreateEvent(
                DomesticGroupPurchaseNegotiationEventTypeCodes.Resolution,
                issue.Resolution.MaskedResolverDisplayName,
                issue.Resolution.ResolverRoleLabel,
                $"{issue.Resolution.ResolutionSummary} 결정 근거: {issue.Resolution.DecisionRationale}",
                issueId));
            return BuildIssue(issueState);
        }
    }

    private DomesticGroupPurchaseNegotiationTimelineResponse BuildTimeline(
        Guid campaignId,
        DomesticGroupPurchaseNegotiationCampaignState state)
        => new()
        {
            GroupPurchaseCampaignId = campaignId,
            Events = state.Events.OrderBy(item => item.CreatedAtUtc).Select(CopyEvent).ToList(),
            Issues = state.Issues.OrderByDescending(item => item.PublicIssue.OpenedAtUtc).Select(BuildIssue).ToList()
        };

    private DomesticGroupPurchaseNegotiationIssueResponse BuildIssue(DomesticGroupPurchaseNegotiationIssueState state)
    {
        var source = state.PublicIssue;
        var distinctParticipants = DistinctParticipants(state);
        return new DomesticGroupPurchaseNegotiationIssueResponse
        {
            IssueId = source.IssueId,
            Title = source.Title,
            PublicSummary = source.PublicSummary,
            MaskedReporterDisplayName = source.MaskedReporterDisplayName,
            ReporterRoleLabel = source.ReporterRoleLabel,
            StatusCode = source.StatusCode,
            OpenedAtUtc = source.OpenedAtUtc,
            DeliberationClosesAtUtc = source.DeliberationClosesAtUtc,
            DistinctParticipantCount = distinctParticipants,
            CanResolve = source.StatusCode == DomesticGroupPurchaseNegotiationIssueStatusCodes.Deliberating
                         && clock.UtcNow >= source.DeliberationClosesAtUtc
                         && distinctParticipants >= MinimumDistinctParticipants,
            Positions = state.Positions.Select(item => CopyPosition(item.PublicPosition)).ToList(),
            Resolution = CopyResolution(source.Resolution)
        };
    }

    private DomesticGroupPurchaseNegotiationEventResponse CreateEvent(
        string eventTypeCode,
        string maskedActorDisplayName,
        string actorRoleLabel,
        string publicSummary,
        Guid? relatedIssueId = null)
        => new()
        {
            EventId = Guid.NewGuid(),
            EventTypeCode = eventTypeCode,
            MaskedActorDisplayName = RequirePublicText(maskedActorDisplayName, "작성자 표시명", 80),
            ActorRoleLabel = RequirePublicText(actorRoleLabel, "작성자 역할", 80),
            PublicSummary = RequirePublicText(publicSummary, "공개 내용", 3000),
            RelatedIssueId = relatedIssueId,
            CreatedAtUtc = clock.UtcNow
        };

    private static DomesticGroupPurchaseNegotiationIssueState FindIssue(
        DomesticGroupPurchaseNegotiationCampaignState state,
        Guid issueId)
        => state.Issues.FirstOrDefault(item => item.PublicIssue.IssueId == issueId)
           ?? throw new KeyNotFoundException("공개 협의 쟁점을 찾을 수 없습니다.");

    private static int DistinctParticipants(DomesticGroupPurchaseNegotiationIssueState state)
        => state.Positions.Select(item => item.AuthorUserId).Distinct(StringComparer.Ordinal).Count();

    private static void ValidateUser(string userId)
        => ArgumentException.ThrowIfNullOrWhiteSpace(userId);

    private static string RequirePublicText(string? value, string fieldLabel, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldLabel}을(를) 입력해야 합니다.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maximumLength)
        {
            throw new ArgumentException($"{fieldLabel}은(는) {maximumLength}자 이하여야 합니다.");
        }

        if (EmailAddressPattern().IsMatch(trimmed) || PhoneNumberPattern().IsMatch(trimmed))
        {
            throw new ArgumentException($"{fieldLabel}에는 이메일이나 전화번호를 공개할 수 없습니다.");
        }

        return trimmed;
    }

    private static DomesticGroupPurchaseNegotiationEventResponse CopyEvent(DomesticGroupPurchaseNegotiationEventResponse source)
        => new()
        {
            EventId = source.EventId,
            EventTypeCode = source.EventTypeCode,
            MaskedActorDisplayName = source.MaskedActorDisplayName,
            ActorRoleLabel = source.ActorRoleLabel,
            PublicSummary = source.PublicSummary,
            RelatedIssueId = source.RelatedIssueId,
            CreatedAtUtc = source.CreatedAtUtc
        };

    private static DomesticGroupPurchaseDeliberationPositionResponse CopyPosition(DomesticGroupPurchaseDeliberationPositionResponse source)
        => new()
        {
            PositionId = source.PositionId,
            PositionCode = source.PositionCode,
            MaskedParticipantDisplayName = source.MaskedParticipantDisplayName,
            ParticipantRoleLabel = source.ParticipantRoleLabel,
            PublicRationale = source.PublicRationale,
            CreatedAtUtc = source.CreatedAtUtc
        };

    private static DomesticGroupPurchaseNegotiationResolutionResponse? CopyResolution(DomesticGroupPurchaseNegotiationResolutionResponse? source)
        => source is null
            ? null
            : new DomesticGroupPurchaseNegotiationResolutionResponse
            {
                MaskedResolverDisplayName = source.MaskedResolverDisplayName,
                ResolverRoleLabel = source.ResolverRoleLabel,
                ResolutionSummary = source.ResolutionSummary,
                DecisionRationale = source.DecisionRationale,
                ResolvedAtUtc = source.ResolvedAtUtc
            };

    [GeneratedRegex(@"[^\s@]+@[^\s@]+\.[^\s@]+", RegexOptions.CultureInvariant)]
    private static partial Regex EmailAddressPattern();

    [GeneratedRegex(@"(?<!\d)(?:01[016789]|0\d{1,2})[-\s]?\d{3,4}[-\s]?\d{4}(?!\d)", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneNumberPattern();
}
