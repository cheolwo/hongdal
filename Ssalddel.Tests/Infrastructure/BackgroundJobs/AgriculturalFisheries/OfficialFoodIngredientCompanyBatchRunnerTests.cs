using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;
using Ssalddel.Services.Community;
using Ssalddel.Services.FoodCulture;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Infrastructure.BackgroundJobs.AgriculturalFisheries;

public sealed class OfficialFoodIngredientCompanyBatchRunnerTests
{
    [Theory]
    [InlineData(false, false, 0, 0, 0)]
    [InlineData(true, false, 1, 0, 1)]
    [InlineData(false, true, 0, 1, 1)]
    [InlineData(true, true, 1, 1, 2)]
    public async Task 수집성공뒤_국가별로_명시적으로켠_월간권역글만게시한다(
        bool chinaPublicationEnabled,
        bool unitedStatesPublicationEnabled,
        int expectedChinaSourceCalls,
        int expectedUnitedStatesSourceCalls,
        int expectedPublishCalls)
    {
        var chinaSource = new RecordingChinaCommunityPostSource();
        var unitedStatesSource = new RecordingUnitedStatesCommunityPostSource();
        var publisher = new RecordingPublisher();
        var runner = new OfficialFoodIngredientCompanyBatchRunner(
            new StubArchiveService(),
            chinaSource,
            unitedStatesSource,
            publisher,
            Options.Create(new AgriculturalFisheriesBatchOptions
            {
                TimeZoneId = "Asia/Seoul",
                PublishChinaImportedFoodRegionBriefs = chinaPublicationEnabled,
                PublishUnitedStatesImportedFoodStateBriefs =
                    unitedStatesPublicationEnabled,
                IngredientCompanyResearchRequestDelayMilliseconds = 0
            }),
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 24, 3, 0, 0, TimeSpan.Zero)),
            NullLogger<OfficialFoodIngredientCompanyBatchRunner>.Instance);

        await runner.RunAsync(CancellationToken.None);

        Assert.Equal(expectedChinaSourceCalls, chinaSource.CallCount);
        Assert.Equal(expectedUnitedStatesSourceCalls, unitedStatesSource.CallCount);
        Assert.Equal(expectedPublishCalls, publisher.CallCount);
        foreach (var draft in publisher.Drafts)
        {
            Assert.Equal("202607", draft.PeriodKey);
        }
    }

    private sealed class StubArchiveService : IOfficialFoodIngredientCompanyArchiveService
    {
        public Task<OfficialFoodIngredientCompanyCollectionResponse> CollectCatalogAsync(
            OfficialFoodIngredientCompanyCollectionRequest request,
            string triggerCode = OfficialFoodIngredientCompanyResearchTriggerCodes.Batch,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new OfficialFoodIngredientCompanyCollectionResponse(
                "run-key",
                triggerCode,
                OfficialFoodIngredientCompanyResearchRunStatusCodes.Completed,
                10,
                10,
                0,
                10,
                0,
                0,
                0,
                0,
                20,
                new DateTimeOffset(2026, 7, 24, 2, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 7, 24, 3, 0, 0, TimeSpan.Zero)));

        public Task<OfficialFoodIngredientCompanyResearchResponse> ResearchAndArchiveAsync(
            OfficialFoodIngredientCompanyQuery query,
            string triggerCode = OfficialFoodIngredientCompanyResearchTriggerCodes.Manual,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<OfficialFoodIngredientCompanyArchiveResponse?> GetArchiveAsync(
            string? ingredientKey,
            string? ingredientName,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<OfficialFoodIngredientCompanyCoverageResponse> GetCoverageAsync(
            int staleAfterDays = 30,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class RecordingChinaCommunityPostSource
        : IChinaImportedFoodRegionCommunityPostSource
    {
        public int CallCount { get; private set; }

        public Task<CommunityAutomatedPostDraft?> BuildAsync(
            DateOnly publicationDate,
            TimeZoneInfo timeZone,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            CommunityAutomatedPostDraft draft = new(
                ChinaImportedFoodRegionCommunityPostSource.SourceKeyValue,
                publicationDate.ToString("yyyyMM"),
                CommunityBoardCatalog.InformationPrices.DisplayName,
                "중국 수입식품 공개근거",
                "자동 정보",
                "월간 중국 제조업소 권역 누적",
                "공식 근거 누적 본문",
                "살뜰 수입식품 정보봇");
            return Task.FromResult<CommunityAutomatedPostDraft?>(draft);
        }
    }

    private sealed class RecordingUnitedStatesCommunityPostSource
        : IUnitedStatesImportedFoodStateCommunityPostSource
    {
        public int CallCount { get; private set; }

        public Task<CommunityAutomatedPostDraft?> BuildAsync(
            DateOnly publicationDate,
            TimeZoneInfo timeZone,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            CommunityAutomatedPostDraft draft = new(
                UnitedStatesImportedFoodStateCommunityPostSource.SourceKeyValue,
                publicationDate.ToString("yyyyMM"),
                CommunityBoardCatalog.InformationPrices.DisplayName,
                "미국 수입식품 공개근거",
                "자동 정보",
                "월간 미국 제조업소 주별 누적",
                "공식 근거 누적 본문",
                "살뜰 수입식품 정보봇");
            return Task.FromResult<CommunityAutomatedPostDraft?>(draft);
        }
    }

    private sealed class RecordingPublisher : ICommunityAutomatedPostPublisher
    {
        public int CallCount { get; private set; }

        public List<CommunityAutomatedPostDraft> Drafts { get; } = [];

        public Task<CommunityAutomatedPostPublishResult> PublishIfMissingAsync(
            CommunityAutomatedPostDraft draft,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            Drafts.Add(draft);
            return Task.FromResult(new CommunityAutomatedPostPublishResult(10, true));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
