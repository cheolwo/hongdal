using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Community;

public sealed record 커뮤니티세계지도원장ProjectionEvaluationInput(
    커뮤니티원장Dto Ledger,
    string ViewerScopeCode,
    string ProjectionSubjectKey,
    string? MapMarkerId,
    string? AdministrativeRegionKey,
    string? CountryCode,
    int? PublicAggregateCount,
    bool PublicAggregateMayBeTruncated,
    string EvidenceFreshnessCode,
    string? EvidenceSnapshotVersion,
    DateTimeOffset LastProjectedAtUtc,
    string SourceEventId);

/// <summary>
/// 저장소나 HTTP 문맥 없이 원장 상태를 지도용 최소 projection으로 축소합니다.
/// 미등록 code, 공개 임계값 미달, 위치 정밀도 불일치는 null로 차단합니다.
/// </summary>
public static class 커뮤니티세계지도원장ProjectionEvaluator
{
    public static 커뮤니티세계지도원장ProjectionDto? Evaluate(
        커뮤니티세계지도원장ProjectionEvaluationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Ledger);

        if (!TryNormalize(input.ProjectionSubjectKey, 240, out var projectionSubjectKey)
            || !TryNormalize(input.SourceEventId, 240, out var sourceEventId)
            || input.Ledger.Revision < 0
            || !커뮤니티세계지도원장ViewerScopeCodes.All.Contains(input.ViewerScopeCode, StringComparer.Ordinal)
            || !IsKnownFreshness(input.EvidenceFreshnessCode)
            || !커뮤니티세계지도원장ProjectionPolicy.TryFind(input.Ledger.원장템플릿Key, out var rule)
            || !TryResolveLedgerState(input.Ledger, out var maturityCode, out var publicStatusCode))
        {
            return null;
        }

        var isPublic = string.Equals(
            input.ViewerScopeCode,
            커뮤니티세계지도원장ViewerScopeCodes.Public,
            StringComparison.Ordinal);
        if (isPublic && !CanProjectPublic(rule!, publicStatusCode, input.PublicAggregateCount))
        {
            return null;
        }

        if (!TryResolveLocation(rule!, input, isPublic, out var markerId, out var regionKey, out var countryCode))
        {
            return null;
        }

        var actionCodes = ResolveActionCodes(
            rule!,
            input.ViewerScopeCode,
            publicStatusCode,
            hasEvidence: markerId is not null || !string.IsNullOrWhiteSpace(input.EvidenceSnapshotVersion));

        return new 커뮤니티세계지도원장ProjectionDto
        {
            ProjectionId = BuildProjectionId(projectionSubjectKey),
            ProjectionVersion = input.Ledger.Revision,
            MapMarkerId = markerId,
            AdministrativeRegionKey = regionKey,
            CountryCode = countryCode,
            LedgerTemplateKey = input.Ledger.원장템플릿Key,
            LedgerMaturityCode = maturityCode,
            PublicStatusCode = publicStatusCode,
            EvidenceFreshnessCode = input.EvidenceFreshnessCode,
            EvidenceSnapshotVersion = Clean(input.EvidenceSnapshotVersion, 200),
            PublicAggregateCount = isPublic ? input.PublicAggregateCount : null,
            AggregateBucketCode = !isPublic
                ? 커뮤니티세계지도원장집계BucketCodes.Suppressed
                : input.PublicAggregateMayBeTruncated
                    ? 커뮤니티세계지도원장집계BucketCodes.Coarsened
                    : 커뮤니티세계지도원장집계BucketCodes.ThresholdMet,
            AvailableActionCodes = actionCodes,
            LastProjectedAtUtc = input.LastProjectedAtUtc,
            SourceEventId = sourceEventId,
            ViewerScopeCode = input.ViewerScopeCode
        };
    }

    public static bool TryResolveLedgerState(
        커뮤니티원장Dto ledger,
        out string maturityCode,
        out string publicStatusCode)
    {
        maturityCode = string.Empty;
        publicStatusCode = string.Empty;
        if (!ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey,
                out var storedMaturity)
            || !커뮤니티세계지도원장성숙도Codes.All.Contains(storedMaturity, StringComparer.Ordinal))
        {
            return false;
        }

        maturityCode = storedMaturity;
        if (ReadBool(ledger.확장속성, "OperationalApplicationCancelled")
            || string.Equals(ledger.현재단계Key, 지도신청가원장정책.운영신청취소단계, StringComparison.Ordinal)
            || string.Equals(ledger.상태, 커뮤니티원장상태.닫힘, StringComparison.Ordinal))
        {
            publicStatusCode = 커뮤니티세계지도원장공개상태Codes.Cancelled;
            return true;
        }

        if (ReadBool(ledger.확장속성, 지도신청가원장정책.개인정보동의철회Key)
            || string.Equals(ledger.현재단계Key, 지도신청가원장정책.동의철회확인단계, StringComparison.Ordinal))
        {
            publicStatusCode = 커뮤니티세계지도원장공개상태Codes.ConsentReviewRequired;
            return true;
        }

        if (string.Equals(maturityCode, 커뮤니티세계지도원장성숙도Codes.Proposed, StringComparison.Ordinal))
        {
            publicStatusCode = 커뮤니티세계지도원장공개상태Codes.Proposed;
            return true;
        }

        if (string.Equals(maturityCode, 커뮤니티세계지도원장성숙도Codes.Provisional, StringComparison.Ordinal)
            && string.Equals(ledger.상태, 커뮤니티원장상태.초안, StringComparison.Ordinal))
        {
            publicStatusCode = 커뮤니티세계지도원장공개상태Codes.ProvisionalDraft;
            return true;
        }

        if (string.Equals(ledger.현재단계Key, 지도신청가원장정책.신청제출단계, StringComparison.Ordinal))
        {
            publicStatusCode = 커뮤니티세계지도원장공개상태Codes.Submitted;
            return true;
        }

        publicStatusCode = ledger.상태 switch
        {
            커뮤니티원장상태.초안 => 커뮤니티세계지도원장공개상태Codes.Proposed,
            커뮤니티원장상태.진행중 => 커뮤니티세계지도원장공개상태Codes.Active,
            커뮤니티원장상태.보류 => 커뮤니티세계지도원장공개상태Codes.OnHold,
            커뮤니티원장상태.완료 => 커뮤니티세계지도원장공개상태Codes.Completed,
            _ => string.Empty
        };
        return publicStatusCode.Length > 0;
    }

    private static bool CanProjectPublic(
        커뮤니티세계지도원장ProjectionPolicyRule rule,
        string publicStatusCode,
        int? aggregateCount)
        => rule.AllowsPublicProjection
           && rule.PublicStatusCodes.Contains(publicStatusCode, StringComparer.Ordinal)
           && aggregateCount.HasValue
           && rule.MinimumPublicAggregateCount.HasValue
           && aggregateCount.Value >= rule.MinimumPublicAggregateCount.Value;

    private static bool TryResolveLocation(
        커뮤니티세계지도원장ProjectionPolicyRule rule,
        커뮤니티세계지도원장ProjectionEvaluationInput input,
        bool isPublic,
        out string? markerId,
        out string? regionKey,
        out string? countryCode)
    {
        markerId = Clean(input.MapMarkerId, 160);
        regionKey = Clean(input.AdministrativeRegionKey, 120);
        countryCode = Clean(input.CountryCode, 8)?.ToUpperInvariant();
        if (!isPublic)
        {
            return markerId is not null || regionKey is not null || countryCode is not null;
        }

        if (string.Equals(
                rule.PublicLocationModeCode,
                커뮤니티세계지도원장위치공개ModeCodes.AdministrativeRegion,
                StringComparison.Ordinal)
            && regionKey is not null)
        {
            countryCode = null;
            markerId = null;
            return true;
        }

        if (string.Equals(
                rule.PublicLocationModeCode,
                커뮤니티세계지도원장위치공개ModeCodes.Country,
                StringComparison.Ordinal)
            && countryCode is not null)
        {
            regionKey = null;
            markerId = null;
            return true;
        }

        return false;
    }

    private static IReadOnlyList<string> ResolveActionCodes(
        커뮤니티세계지도원장ProjectionPolicyRule rule,
        string viewerScopeCode,
        string publicStatusCode,
        bool hasEvidence)
    {
        if (!rule.MaximumActionCodesByViewerScope.TryGetValue(viewerScopeCode, out var maximumActions))
        {
            return [];
        }

        return maximumActions.Where(actionCode => actionCode switch
        {
            커뮤니티세계지도원장ActionCodes.ViewEvidence => hasEvidence,
            커뮤니티세계지도원장ActionCodes.ViewLedger => !string.Equals(
                viewerScopeCode,
                커뮤니티세계지도원장ViewerScopeCodes.Public,
                StringComparison.Ordinal),
            커뮤니티세계지도원장ActionCodes.ContinueDraft => string.Equals(
                publicStatusCode,
                커뮤니티세계지도원장공개상태Codes.ProvisionalDraft,
                StringComparison.Ordinal),
            커뮤니티세계지도원장ActionCodes.ReviewConsent => string.Equals(
                publicStatusCode,
                커뮤니티세계지도원장공개상태Codes.ConsentReviewRequired,
                StringComparison.Ordinal),
            커뮤니티세계지도원장ActionCodes.Submit => string.Equals(
                publicStatusCode,
                커뮤니티세계지도원장공개상태Codes.ProvisionalDraft,
                StringComparison.Ordinal),
            커뮤니티세계지도원장ActionCodes.Withdraw => publicStatusCode is
                커뮤니티세계지도원장공개상태Codes.ProvisionalDraft
                or 커뮤니티세계지도원장공개상태Codes.Submitted
                or 커뮤니티세계지도원장공개상태Codes.Active
                or 커뮤니티세계지도원장공개상태Codes.OnHold,
            _ => false
        }).ToArray();
    }

    private static string BuildProjectionId(string subjectKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(subjectKey));
        return $"map-ledger:{Convert.ToHexString(hash).ToLowerInvariant()[..24]}";
    }

    private static bool ReadBool(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) && parsed;

    private static bool IsKnownFreshness(string value)
        => value is 커뮤니티세계지도FreshnessCodes.Fresh
            or 커뮤니티세계지도FreshnessCodes.Stale
            or 커뮤니티세계지도FreshnessCodes.Expired
            or 커뮤니티세계지도FreshnessCodes.Unknown;

    private static bool TryNormalize(string? value, int maximumLength, out string normalized)
    {
        normalized = value?.Trim() ?? string.Empty;
        return normalized.Length is > 0 && normalized.Length <= maximumLength && !normalized.Any(char.IsControl);
    }

    private static string? Clean(string? value, int maximumLength)
        => TryNormalize(value, maximumLength, out var normalized) ? normalized : null;
}
