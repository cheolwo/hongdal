using System.Security.Cryptography;
using System.Text;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Services.Community;

public sealed record 커뮤니티세계지도원장ProjectionQuery(
    string LedgerTemplateKey,
    string MapMarkerId,
    string? ViewerUserId = null,
    bool OperatorAuthorized = false,
    bool ReviewerAuthorized = false,
    string? AdministrativeRegionKey = null,
    string? CountryCode = null,
    string EvidenceFreshnessCode = 커뮤니티세계지도FreshnessCodes.Unknown,
    string? EvidenceSnapshotVersion = null);

public interface I커뮤니티세계지도원장ProjectionUseCase
{
    Task<IReadOnlyList<커뮤니티세계지도원장ProjectionDto>> 조회Async(
        커뮤니티세계지도원장ProjectionQuery query,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ledger,
    SsalddelModuleKind.Application,
    "지도 마커와 연결된 원장을 viewer 권한과 공개 집계 정책에 따라 최소 projection으로 조회",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "원장 원문과 개인 식별자·상세 주소·정밀 경로·계약·재고·금액을 지도 응답에 전달하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Application,
    "원장 조회 결과를 소유자·참여자·운영자·검토자와 공개 집계 범위로 축소",
    ContractType = typeof(I커뮤니티세계지도원장ProjectionUseCase),
    FlowOrder = 21,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "공개 요청은 단일 원장 존재를 반환하지 않고 template별 최소 집계 임계값을 통과한 상태만 반환합니다.")]
public sealed class 커뮤니티세계지도원장ProjectionUseCase(
    I커뮤니티원장저장소 ledgerStore,
    I커뮤니티세계지도원장ProjectionCache? publicProjectionCache = null) : I커뮤니티세계지도원장ProjectionUseCase
{
    private const int QueryLimit = 200;
    private readonly I커뮤니티세계지도원장ProjectionCache _projectionCache =
        publicProjectionCache ?? new 커뮤니티세계지도원장ProjectionCache();

    public async Task<IReadOnlyList<커뮤니티세계지도원장ProjectionDto>> 조회Async(
        커뮤니티세계지도원장ProjectionQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (!커뮤니티세계지도원장ProjectionPolicy.TryFind(query.LedgerTemplateKey, out var rule)
            || !TryNormalize(query.MapMarkerId, 160, out var markerId))
        {
            return [];
        }

        var viewerUserId = Clean(query.ViewerUserId, 160);
        IReadOnlyList<커뮤니티세계지도원장ProjectionDto> publicProjections;
        IReadOnlyList<커뮤니티원장Dto> publicLedgers;
        if (_projectionCache.TryGetPublic(query, out var cachedPublicProjections))
        {
            publicProjections = cachedPublicProjections;
            publicLedgers = [];
        }
        else
        {
            publicLedgers = FilterMatching(
                await LoadAsync(accessUserId: null),
                rule!,
                markerId);
            publicProjections = BuildPublicProjections(publicLedgers, rule!, markerId, query);
            _projectionCache.SetPublic(query, publicProjections);
        }
        var viewerLedgers = viewerUserId is not null && !query.OperatorAuthorized && !query.ReviewerAuthorized
            ? FilterMatching(await LoadAsync(viewerUserId), rule!, markerId)
            : query.OperatorAuthorized || query.ReviewerAuthorized
                ? publicLedgers.Count > 0
                    ? publicLedgers
                    : FilterMatching(await LoadAsync(accessUserId: null), rule!, markerId)
                : [];

        var projections = new List<커뮤니티세계지도원장ProjectionDto>(publicProjections);
        foreach (var ledger in viewerLedgers)
        {
            var viewerScope = ResolveViewerScope(ledger, viewerUserId, query.OperatorAuthorized, query.ReviewerAuthorized);
            if (string.Equals(viewerScope, 커뮤니티세계지도원장ViewerScopeCodes.Public, StringComparison.Ordinal))
            {
                continue;
            }

            var projection = Evaluate(
                ledger,
                viewerScope,
                projectionSubjectKey: $"ledger:{ledger.원장Id}:{viewerScope}",
                markerId,
                query,
                publicAggregateCount: null,
                aggregateMayBeTruncated: false,
                sourceEventId: ledger.투영EventId ?? $"ledger-revision:{ledger.Revision}");
            if (projection is not null)
            {
                projections.Add(projection);
            }
        }

        return projections
            .OrderBy(projection => projection.ViewerScopeCode, StringComparer.Ordinal)
            .ThenBy(projection => projection.PublicStatusCode, StringComparer.Ordinal)
            .ThenBy(projection => projection.ProjectionId, StringComparer.Ordinal)
            .ToArray();

        Task<IReadOnlyList<커뮤니티원장Dto>> LoadAsync(string? accessUserId)
            => ledgerStore.원장목록조회Async(
                new 커뮤니티원장조회조건
                {
                    원장템플릿Key = rule!.LedgerTemplateKey,
                    외부참조조건 = new Dictionary<string, string>
                    {
                        ["MapMarkerId"] = markerId
                    },
                    접근UserId = accessUserId,
                    Limit = QueryLimit
                },
                cancellationToken);
    }

    private static IReadOnlyList<커뮤니티세계지도원장ProjectionDto> BuildPublicProjections(
        IReadOnlyList<커뮤니티원장Dto> publicLedgers,
        커뮤니티세계지도원장ProjectionPolicyRule rule,
        string markerId,
        커뮤니티세계지도원장ProjectionQuery query)
    {
        var projections = new List<커뮤니티세계지도원장ProjectionDto>();
        if (rule.AllowsPublicProjection)
        {
            var publicGroups = publicLedgers
                .Select(ResolveLedgerState)
                .OfType<ResolvedLedgerState>()
                .Where(item => rule.PublicStatusCodes.Contains(item.StatusCode, StringComparer.Ordinal))
                .GroupBy(item => (item.MaturityCode, item.StatusCode));

            foreach (var group in publicGroups)
            {
                var count = group.Count();
                if (!rule.MinimumPublicAggregateCount.HasValue
                    || count < rule.MinimumPublicAggregateCount.Value)
                {
                    continue;
                }

                var representative = group.OrderByDescending(item => item.Ledger.Revision).First().Ledger;
                var sourceEventId = BuildAggregateEventId(group.Select(item => item.Ledger));
                var projection = Evaluate(
                    representative,
                    커뮤니티세계지도원장ViewerScopeCodes.Public,
                    projectionSubjectKey: $"aggregate:{markerId}:{rule.LedgerTemplateKey}:{group.Key.MaturityCode}:{group.Key.StatusCode}",
                    markerId,
                    query,
                    publicAggregateCount: count,
                    aggregateMayBeTruncated: publicLedgers.Count >= QueryLimit,
                    sourceEventId);
                if (projection is not null)
                {
                    projections.Add(projection);
                }
            }
        }

        return projections;
    }

    private static IReadOnlyList<커뮤니티원장Dto> FilterMatching(
        IReadOnlyList<커뮤니티원장Dto> ledgers,
        커뮤니티세계지도원장ProjectionPolicyRule rule,
        string markerId)
        => ledgers
            .Where(ledger => string.Equals(ledger.원장템플릿Key, rule.LedgerTemplateKey, StringComparison.OrdinalIgnoreCase))
            .Where(ledger => ledger.외부참조.TryGetValue("MapMarkerId", out var storedMarkerId)
                             && string.Equals(storedMarkerId, markerId, StringComparison.Ordinal))
            .ToArray();

    private static 커뮤니티세계지도원장ProjectionDto? Evaluate(
        커뮤니티원장Dto ledger,
        string viewerScope,
        string projectionSubjectKey,
        string markerId,
        커뮤니티세계지도원장ProjectionQuery query,
        int? publicAggregateCount,
        bool aggregateMayBeTruncated,
        string sourceEventId)
        => 커뮤니티세계지도원장ProjectionEvaluator.Evaluate(
            new 커뮤니티세계지도원장ProjectionEvaluationInput(
                ledger,
                viewerScope,
                projectionSubjectKey,
                markerId,
                query.AdministrativeRegionKey,
                query.CountryCode,
                publicAggregateCount,
                aggregateMayBeTruncated,
                query.EvidenceFreshnessCode,
                query.EvidenceSnapshotVersion,
                ResolveProjectedAt(ledger),
                sourceEventId));

    private static string ResolveViewerScope(
        커뮤니티원장Dto ledger,
        string? viewerUserId,
        bool operatorAuthorized,
        bool reviewerAuthorized)
    {
        if (viewerUserId is not null
            && string.Equals(ledger.생성자UserId, viewerUserId, StringComparison.Ordinal))
        {
            return 커뮤니티세계지도원장ViewerScopeCodes.Owner;
        }

        if (viewerUserId is not null
            && ledger.참여자목록.Any(participant =>
                string.Equals(participant.UserId, viewerUserId, StringComparison.Ordinal)))
        {
            return 커뮤니티세계지도원장ViewerScopeCodes.Participant;
        }

        if (operatorAuthorized)
        {
            return 커뮤니티세계지도원장ViewerScopeCodes.Operator;
        }

        return reviewerAuthorized
            ? 커뮤니티세계지도원장ViewerScopeCodes.Reviewer
            : 커뮤니티세계지도원장ViewerScopeCodes.Public;
    }

    private static ResolvedLedgerState? ResolveLedgerState(커뮤니티원장Dto ledger)
        => 커뮤니티세계지도원장ProjectionEvaluator.TryResolveLedgerState(
            ledger,
            out var maturityCode,
            out var statusCode)
            ? new ResolvedLedgerState(ledger, maturityCode, statusCode)
            : null;

    private static DateTimeOffset ResolveProjectedAt(커뮤니티원장Dto ledger)
        => ledger.수정시각Utc == default
            ? DateTimeOffset.UnixEpoch
            : new DateTimeOffset(DateTime.SpecifyKind(ledger.수정시각Utc, DateTimeKind.Utc));

    private static string BuildAggregateEventId(IEnumerable<커뮤니티원장Dto> ledgers)
    {
        var source = string.Join('|', ledgers
            .OrderBy(ledger => ledger.원장Id, StringComparer.Ordinal)
            .Select(ledger => $"{ledger.원장Id}:{ledger.Revision}:{ledger.투영EventId}"));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return $"aggregate:{Convert.ToHexString(hash).ToLowerInvariant()[..24]}";
    }

    private static bool TryNormalize(string? value, int maximumLength, out string normalized)
    {
        normalized = value?.Trim() ?? string.Empty;
        return normalized.Length is > 0 && normalized.Length <= maximumLength && !normalized.Any(char.IsControl);
    }

    private static string? Clean(string? value, int maximumLength)
        => TryNormalize(value, maximumLength, out var normalized) ? normalized : null;

    private sealed record ResolvedLedgerState(
        커뮤니티원장Dto Ledger,
        string MaturityCode,
        string StatusCode);
}
