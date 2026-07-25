using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityEditorialBatchRegistrationPlanTests
{
    [Fact]
    public void 주기데이터게시판의_자동작성Source를_0_0등록계획에모두포함한다()
    {
        var plan = CommunityEditorialBatchRegistrationPlan.Create(
            new AgriculturalFisheriesBatchOptions
            {
                Enabled = true,
                PublishCommunityPriceBriefs = true,
                KamisDailyEnabled = true,
                UsdaMonthlyEnabled = true,
                IngredientCompanyResearchEnabled = true,
                PublishChinaImportedFoodRegionBriefs = true,
                PublishUnitedStatesImportedFoodStateBriefs = true
            },
            new CommunityEditorialBatchOptions
            {
                Enabled = true,
                KamisPriceBriefEnabled = true,
                UsdaNassPriceBriefEnabled = true
            });

        var publicationSources = CommunityPeriodicDataBoardCatalog.All
            .SelectMany(board => board.PublicationSourceKeys.Select(sourceKey => new
            {
                board.BoardKey,
                SourceKey = sourceKey
            }))
            .ToArray();

        Assert.NotEmpty(publicationSources);
        Assert.All(publicationSources, source =>
        {
            var registration = plan.Get(source.SourceKey);
            Assert.Equal(source.BoardKey, registration.CanonicalBoardKey);
            Assert.True(
                registration.QuartzRegistrationEnabled
                || registration.CollectionHandoffEnabled);
        });
        Assert.DoesNotContain(
            plan.Registrations,
            registration => registration.CanonicalBoardKey
                            == CommunityBoardKeys.PeriodicDataCustomsImportUnitPrice);
    }

    [Fact]
    public void 수집후가격게시를사용하면_독립Quartz를막고_편집배치가인계를소유한다()
    {
        var plan = CommunityEditorialBatchRegistrationPlan.Create(
            new AgriculturalFisheriesBatchOptions
            {
                Enabled = true,
                PublishCommunityPriceBriefs = true,
                KamisDailyEnabled = true,
                UsdaMonthlyEnabled = true
            },
            new CommunityEditorialBatchOptions
            {
                Enabled = true,
                KamisPriceBriefEnabled = true,
                UsdaNassPriceBriefEnabled = true
            });

        var kamis = plan.Get(CommunityAutomatedPostSourceKeys.KamisPriceBrief);
        var usda = plan.Get(CommunityAutomatedPostSourceKeys.UsdaNassPriceBrief);

        Assert.True(kamis.CollectionHandoffEnabled);
        Assert.False(kamis.QuartzRegistrationEnabled);
        Assert.True(usda.CollectionHandoffEnabled);
        Assert.False(usda.QuartzRegistrationEnabled);
    }

    [Fact]
    public void 문화교통은_공동구매Os와무관한_독립편집Quartz로만등록한다()
    {
        var disabled = CommunityEditorialBatchRegistrationPlan.Create(
            new AgriculturalFisheriesBatchOptions(),
            new CommunityEditorialBatchOptions
            {
                Enabled = true,
                CultureTransportEnabled = false
            });
        var enabled = CommunityEditorialBatchRegistrationPlan.Create(
            new AgriculturalFisheriesBatchOptions(),
            new CommunityEditorialBatchOptions
            {
                Enabled = true,
                CultureTransportEnabled = true
            });

        Assert.False(disabled.ShouldRegisterQuartz(
            CommunityAutomatedPostSourceKeys.CultureTransport));
        var registration = enabled.Get(
            CommunityAutomatedPostSourceKeys.CultureTransport);
        Assert.True(registration.QuartzRegistrationEnabled);
        Assert.False(registration.CollectionHandoffEnabled);
    }
}
