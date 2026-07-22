using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class OfficialFoodIngredientCompanyResearchServiceTests
{
    private static readonly DateTimeOffset ResearchedAtUtc =
        new(2026, 7, 22, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task 공식근거는_국내제조사_수입사_해외제조업소를분리하고_자동선정을금지한다()
    {
        var service = new OfficialFoodIngredientCompanyResearchService(
            new FakeDomesticSource([
                new OfficialFoodIngredientDomesticCompanyRecord(
                    "20010000001",
                    "국내식품",
                    "200100000011",
                    "20260701",
                    "참깨 소스",
                    "소스",
                    "참깨, 소금",
                    "1",
                    "20260720")
            ]),
            new FakeImportedSource(new OfficialFoodIngredientImportedCompanySourceResult(
                [
                    new OfficialFoodIngredientImportedCompanyRecord(
                        "한국수입",
                        "GLOBAL SESAME FOODS",
                        "미국",
                        "참깨 페이스트",
                        "기타가공품",
                        "참깨 100%",
                        "20260718",
                        "US-FOOD-10",
                        true,
                        false,
                        string.Empty)
                ],
                true,
                false)),
            new FixedTimeProvider(ResearchedAtUtc),
            NullLogger<OfficialFoodIngredientCompanyResearchService>.Instance);

        var result = await service.ResearchAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientKey = "ingredient:sesame",
            IngredientName = "참깨",
            Take = 20
        });

        Assert.Equal(OfficialFoodIngredientCompanyResearchStatusCodes.Available, result.StatusCode);
        Assert.Equal(ResearchedAtUtc, result.ResearchedAtUtc);
        Assert.Equal(3, result.Candidates.Count);
        Assert.Contains(result.Candidates, candidate =>
            candidate.RelationCode == OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer
            && candidate.OfficialIdentifier == "20010000001"
            && candidate.RawIngredientText == "참깨, 소금"
            && candidate.EvidenceRecordIdentifier == "200100000011"
            && candidate.EvidenceDate == "20260701"
            && candidate.EvidenceLastChangedDate == "20260720");
        Assert.Contains(result.Candidates, candidate =>
            candidate.RelationCode == OfficialFoodIngredientCompanyRelationCodes.DomesticImporter
            && candidate.OrganizationName == "한국수입");
        Assert.Contains(result.Candidates, candidate =>
            candidate.RelationCode == OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer
            && candidate.CountryCode == "US"
            && candidate.OfficialIdentifier == "US-FOOD-10"
            && candidate.VerificationStatusCode
                == OfficialFoodIngredientCompanyVerificationStatusCodes.OverseasFacilityMatched);
        Assert.All(result.Candidates, candidate =>
        {
            Assert.False(candidate.CanAutoSelect);
            Assert.False(candidate.CanAutoContact);
            Assert.True(candidate.RequiresLiveRecheck);
        });
        Assert.DoesNotContain(result.Candidates, candidate =>
            candidate.EvidenceSummary.Contains("전화", StringComparison.Ordinal)
            || candidate.EvidenceSummary.Contains("대표자", StringComparison.Ordinal));
    }

    [Fact]
    public async Task 일부원천미설정은_확인된후보를유지하고_부분상태를반환한다()
    {
        var service = new OfficialFoodIngredientCompanyResearchService(
            new FakeDomesticSource([], configured: false),
            new FakeImportedSource(new OfficialFoodIngredientImportedCompanySourceResult(
                [
                    new OfficialFoodIngredientImportedCompanyRecord(
                        "한국수입",
                        string.Empty,
                        string.Empty,
                        "들깨 오일",
                        "식용유",
                        "들깨",
                        "20260718",
                        string.Empty,
                        false,
                        false,
                        string.Empty)
                ],
                false,
                false)),
            new FixedTimeProvider(ResearchedAtUtc),
            NullLogger<OfficialFoodIngredientCompanyResearchService>.Instance);

        var result = await service.ResearchAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientName = "들깨"
        });

        Assert.Equal(OfficialFoodIngredientCompanyResearchStatusCodes.Partial, result.StatusCode);
        Assert.Single(result.Candidates);
        Assert.Contains(result.Sources, source =>
            source.SourceKey == MfdsIngredientProductCompanySource.SourceKey
            && source.StatusCode == OfficialFoodIngredientCompanySourceStatusCodes.NotConfigured);
    }

    [Fact]
    public async Task 조회상한이작아도_국내제조_수입_해외제조역할을균형있게유지한다()
    {
        var domesticRecords = Enumerable.Range(1, 6)
            .Select(index => new OfficialFoodIngredientDomesticCompanyRecord(
                $"license-{index}",
                $"국내제조-{index}",
                $"report-{index}",
                "20260701",
                $"양파제품-{index}",
                "소스",
                "양파",
                "1",
                "20260720"))
            .ToArray();
        var service = new OfficialFoodIngredientCompanyResearchService(
            new FakeDomesticSource(domesticRecords),
            new FakeImportedSource(new OfficialFoodIngredientImportedCompanySourceResult(
                [
                    new OfficialFoodIngredientImportedCompanyRecord(
                        "국내수입-1",
                        "FOREIGN ONION FOODS",
                        "프랑스",
                        "양파 페이스트",
                        "기타가공품",
                        "양파",
                        "20260718",
                        "FR-10",
                        true,
                        false,
                        string.Empty)
                ],
                true,
                false)),
            new FixedTimeProvider(ResearchedAtUtc),
            NullLogger<OfficialFoodIngredientCompanyResearchService>.Instance);

        var result = await service.ResearchAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientName = "양파",
            Take = 3
        });

        Assert.Equal(3, result.Candidates.Count);
        Assert.Contains(result.Candidates, candidate =>
            candidate.RelationCode == OfficialFoodIngredientCompanyRelationCodes.DomesticManufacturer);
        Assert.Contains(result.Candidates, candidate =>
            candidate.RelationCode == OfficialFoodIngredientCompanyRelationCodes.DomesticImporter);
        Assert.Contains(result.Candidates, candidate =>
            candidate.RelationCode == OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer);
    }

    [Fact]
    public async Task 한글자재료는_전산화키가있을때만_공식조사를허용한다()
    {
        var service = new OfficialFoodIngredientCompanyResearchService(
            new FakeDomesticSource([]),
            new FakeImportedSource(new OfficialFoodIngredientImportedCompanySourceResult(
                [],
                false,
                false)),
            new FixedTimeProvider(ResearchedAtUtc),
            NullLogger<OfficialFoodIngredientCompanyResearchService>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ResearchAsync(
            new OfficialFoodIngredientCompanyQuery { IngredientName = "쌀" }));
        var result = await service.ResearchAsync(new OfficialFoodIngredientCompanyQuery
        {
            IngredientKey = "ingredient:rice",
            IngredientName = "쌀"
        });

        Assert.Equal(OfficialFoodIngredientCompanyResearchStatusCodes.NoResults, result.StatusCode);
    }

    private sealed class FakeDomesticSource(
        IReadOnlyList<OfficialFoodIngredientDomesticCompanyRecord> records,
        bool configured = true) : IOfficialFoodIngredientDomesticCompanySource
    {
        public bool IsConfigured => configured;

        public Task<IReadOnlyList<OfficialFoodIngredientDomesticCompanyRecord>> SearchAsync(
            string ingredientName,
            int take,
            CancellationToken cancellationToken = default)
            => Task.FromResult(records);
    }

    private sealed class FakeImportedSource(
        OfficialFoodIngredientImportedCompanySourceResult result,
        bool labelConfigured = true,
        bool facilityConfigured = true) : IOfficialFoodIngredientImportedCompanySource
    {
        public bool IsLabelSourceConfigured => labelConfigured;

        public bool IsForeignFacilitySourceConfigured => facilityConfigured;

        public Task<OfficialFoodIngredientImportedCompanySourceResult> SearchAsync(
            string ingredientName,
            int take,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
