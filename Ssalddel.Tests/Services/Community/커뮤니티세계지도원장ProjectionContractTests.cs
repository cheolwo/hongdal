using System.Text.Json;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도원장ProjectionContractTests
{
    [Fact]
    public void Maturity_codes_reuse_existing_provisional_and_established_ledger_codes()
    {
        Assert.Equal(
            CommunityPostProvisionalLedgerPolicy.LedgerMaturityCode,
            커뮤니티세계지도원장성숙도Codes.Provisional);
        Assert.Equal(
            지도신청가원장정책.실원장성숙도Code,
            커뮤니티세계지도원장성숙도Codes.Established);
        Assert.Equal(3, 커뮤니티세계지도원장성숙도Codes.All.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Projection_code_catalogs_have_unique_stable_values()
    {
        AssertUnique(커뮤니티세계지도원장공개상태Codes.All);
        AssertUnique(커뮤니티세계지도원장ViewerScopeCodes.All);
        AssertUnique(커뮤니티세계지도원장집계BucketCodes.All);
        AssertUnique(커뮤니티세계지도원장위치공개ModeCodes.All);
        AssertUnique(커뮤니티세계지도원장ActionCodes.All);

        Assert.Contains(커뮤니티세계지도원장ViewerScopeCodes.Public, 커뮤니티세계지도원장ViewerScopeCodes.All);
        Assert.Contains(커뮤니티세계지도원장공개상태Codes.Withdrawn, 커뮤니티세계지도원장공개상태Codes.All);
        Assert.Contains(커뮤니티세계지도원장공개상태Codes.Cancelled, 커뮤니티세계지도원장공개상태Codes.All);
    }

    [Fact]
    public void Projection_contract_contains_only_map_safe_summary_fields()
    {
        var propertyNames = typeof(커뮤니티세계지도원장ProjectionDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        string[] forbiddenFragments =
        [
            "LedgerId",
            "UserId",
            "Name",
            "Phone",
            "Contact",
            "Address",
            "Latitude",
            "Longitude",
            "Route",
            "Vehicle",
            "Inventory",
            "Lot",
            "Price",
            "Amount",
            "Data",
            "Attributes"
        ];

        Assert.DoesNotContain(
            propertyNames,
            propertyName => forbiddenFragments.Any(fragment =>
                propertyName.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Public_projection_serialization_keeps_evidence_and_ledger_status_as_separate_axes()
    {
        var projection = new 커뮤니티세계지도원장ProjectionDto
        {
            ProjectionId = "map-ledger:marker-1:group-purchase",
            ProjectionVersion = 3,
            MapMarkerId = "marker-1",
            LedgerTemplateKey = CommunityLedgerTemplateKeys.GroupPurchase,
            LedgerMaturityCode = 커뮤니티세계지도원장성숙도Codes.Established,
            PublicStatusCode = 커뮤니티세계지도원장공개상태Codes.Completed,
            EvidenceFreshnessCode = 커뮤니티세계지도FreshnessCodes.Stale,
            EvidenceSnapshotVersion = "source-v2",
            PublicAggregateCount = 12,
            AggregateBucketCode = 커뮤니티세계지도원장집계BucketCodes.ThresholdMet,
            AvailableActionCodes = [커뮤니티세계지도원장ActionCodes.ViewEvidence],
            LastProjectedAtUtc = new DateTimeOffset(2026, 8, 4, 9, 0, 0, TimeSpan.Zero),
            SourceEventId = "event-3",
            ViewerScopeCode = 커뮤니티세계지도원장ViewerScopeCodes.Public
        };

        var json = JsonSerializer.Serialize(projection);
        var restored = JsonSerializer.Deserialize<커뮤니티세계지도원장ProjectionDto>(json);

        Assert.NotNull(restored);
        Assert.Equal(커뮤니티세계지도원장공개상태Codes.Completed, restored.PublicStatusCode);
        Assert.Equal(커뮤니티세계지도FreshnessCodes.Stale, restored.EvidenceFreshnessCode);
        Assert.Equal(커뮤니티세계지도원장ViewerScopeCodes.Public, restored.ViewerScopeCode);
        Assert.Equal(CommunityLedgerTemplateKeys.GroupPurchase, restored.LedgerTemplateKey);
    }

    private static void AssertUnique(IReadOnlyList<string> values)
    {
        Assert.DoesNotContain(values, string.IsNullOrWhiteSpace);
        Assert.Equal(values.Count, values.Distinct(StringComparer.Ordinal).Count());
    }
}
