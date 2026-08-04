using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도원장ProjectionPolicyTests
{
    [Fact]
    public void Policy_catalog_covers_every_ledger_template_exactly_once()
    {
        Assert.Equal(
            CommunityLedgerTemplateKeys.All.Order(StringComparer.Ordinal),
            커뮤니티세계지도원장ProjectionPolicy.All
                .Select(rule => rule.LedgerTemplateKey)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            커뮤니티세계지도원장ProjectionPolicy.All.Count,
            커뮤니티세계지도원장ProjectionPolicy.All
                .Select(rule => rule.LedgerTemplateKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
    }

    [Theory]
    [InlineData(CommunityLedgerTemplateKeys.IndividualDemand)]
    [InlineData(CommunityLedgerTemplateKeys.Order)]
    [InlineData(CommunityLedgerTemplateKeys.IndividualImport)]
    [InlineData(CommunityLedgerTemplateKeys.IndividualExport)]
    [InlineData(CommunityLedgerTemplateKeys.Errand)]
    [InlineData(CommunityLedgerTemplateKeys.EducationFieldExperience)]
    public void Personal_or_safety_sensitive_ledgers_are_private_by_default(string templateKey)
    {
        var rule = 커뮤니티세계지도원장ProjectionPolicy.Find(templateKey);

        Assert.False(rule.AllowsPublicProjection);
        Assert.Equal(커뮤니티세계지도원장위치공개ModeCodes.None, rule.PublicLocationModeCode);
        Assert.Null(rule.MinimumPublicAggregateCount);
        Assert.Empty(rule.PublicStatusCodes);
        Assert.Empty(rule.PublicActionCodes);
        Assert.Empty(rule.MaximumActionCodesByViewerScope[커뮤니티세계지도원장ViewerScopeCodes.Public]);
    }

    [Fact]
    public void Public_projection_rules_only_allow_coarse_location_and_thresholded_aggregates()
    {
        var publicRules = 커뮤니티세계지도원장ProjectionPolicy.All
            .Where(rule => rule.AllowsPublicProjection)
            .ToArray();

        Assert.NotEmpty(publicRules);
        string[] allowedLocationModes =
        [
            커뮤니티세계지도원장위치공개ModeCodes.AdministrativeRegion,
            커뮤니티세계지도원장위치공개ModeCodes.Country
        ];
        Assert.All(publicRules, rule =>
        {
            Assert.Contains(rule.PublicLocationModeCode, allowedLocationModes);
            Assert.True(rule.MinimumPublicAggregateCount >= 커뮤니티활동공개Policy.최소공개활동수);
            Assert.Equal(
                [커뮤니티세계지도원장ActionCodes.ViewEvidence],
                rule.PublicActionCodes);
            Assert.Equal(
                rule.PublicActionCodes,
                rule.MaximumActionCodesByViewerScope[커뮤니티세계지도원장ViewerScopeCodes.Public]);
        });
    }

    [Fact]
    public void Public_projection_never_exposes_private_transition_states()
    {
        string[] privateStates =
        [
            커뮤니티세계지도원장공개상태Codes.ProvisionalDraft,
            커뮤니티세계지도원장공개상태Codes.ConsentReviewRequired,
            커뮤니티세계지도원장공개상태Codes.Submitted,
            커뮤니티세계지도원장공개상태Codes.Withdrawn,
            커뮤니티세계지도원장공개상태Codes.Cancelled
        ];

        Assert.All(
            커뮤니티세계지도원장ProjectionPolicy.All,
            rule => Assert.Empty(rule.PublicStatusCodes.Intersect(privateStates, StringComparer.Ordinal)));
    }

    [Fact]
    public void Every_template_forbids_all_protected_field_groups()
    {
        Assert.All(커뮤니티세계지도원장ProjectionPolicy.All, rule =>
            Assert.Equal(
                커뮤니티세계지도원장보호FieldGroupCodes.All.Order(StringComparer.Ordinal),
                rule.ForbiddenFieldGroupCodes.Order(StringComparer.Ordinal)));
    }

    [Fact]
    public void Every_template_declares_maximum_actions_for_every_viewer_scope()
    {
        Assert.All(커뮤니티세계지도원장ProjectionPolicy.All, rule =>
        {
            Assert.Equal(
                커뮤니티세계지도원장ViewerScopeCodes.All.Order(StringComparer.Ordinal),
                rule.MaximumActionCodesByViewerScope.Keys.Order(StringComparer.Ordinal));
            Assert.All(
                rule.MaximumActionCodesByViewerScope.Values.SelectMany(actions => actions),
                action => Assert.Contains(action, 커뮤니티세계지도원장ActionCodes.All));
        });
    }

    [Fact]
    public void Find_is_case_insensitive_and_unknown_template_fails_closed()
    {
        Assert.True(커뮤니티세계지도원장ProjectionPolicy.TryFind(" GROUP-PURCHASE ", out var rule));
        Assert.Equal(CommunityLedgerTemplateKeys.GroupPurchase, rule!.LedgerTemplateKey);

        Assert.False(커뮤니티세계지도원장ProjectionPolicy.TryFind("unknown-ledger", out _));
        Assert.Throws<KeyNotFoundException>(() =>
            커뮤니티세계지도원장ProjectionPolicy.Find("unknown-ledger"));
    }
}
