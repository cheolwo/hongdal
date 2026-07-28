using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 주문자3PL후보필터Tests
{
    [Theory]
    [InlineData(false, 주문자3PL보관온도코드.상온,
        CollectivePurchaseProductHandlingCodes.GeneralMerchandise)]
    [InlineData(true, 주문자3PL보관온도코드.상온,
        CollectivePurchaseProductHandlingCodes.ShelfStablePackagedFoodByReview)]
    [InlineData(true, 주문자3PL보관온도코드.냉장,
        CollectivePurchaseProductHandlingCodes.RefrigeratedFoodByFacilityReview)]
    [InlineData(true, 주문자3PL보관온도코드.냉동,
        CollectivePurchaseProductHandlingCodes.FrozenFoodByFacilityReview)]
    public void 보관온도와식품여부를_정확한상품취급코드로변환한다(
        bool 식품류여부,
        string 보관온도코드,
        string expected)
    {
        var result = 주문자3PL후보필터.요구상품취급코드(new 주문자3PL후보필터조건
        {
            식품류여부 = 식품류여부,
            보관온도코드 = 보관온도코드
        });

        Assert.Equal(expected, result);
    }

    [Fact]
    public void 냉동은_공동보관과냉동취급과온도역량을모두갖춘후보만남긴다()
    {
        var candidates = new[]
        {
            Candidate(
                "frozen-fit",
                "Frozen Fit",
                [CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage],
                [CollectivePurchaseProductHandlingCodes.FrozenFoodByFacilityReview],
                [ThirdPartyLogisticsProviderCapabilityCodes.ColdChain]),
            Candidate(
                "refrigerated-only",
                "Refrigerated Only",
                [CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage],
                [CollectivePurchaseProductHandlingCodes.RefrigeratedFoodByFacilityReview],
                [ThirdPartyLogisticsProviderCapabilityCodes.ColdChain]),
            Candidate(
                "no-shared-storage",
                "No Shared Storage",
                [CollectivePurchaseLogisticsStageCodes.BulkInboundReceiving],
                [CollectivePurchaseProductHandlingCodes.FrozenFoodByFacilityReview],
                [ThirdPartyLogisticsProviderCapabilityCodes.ColdChain]),
            Candidate(
                "no-temperature-capability",
                "No Temperature Capability",
                [CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage],
                [CollectivePurchaseProductHandlingCodes.FrozenFoodByFacilityReview],
                [ThirdPartyLogisticsProviderCapabilityCodes.WarehousingAndDistribution]),
            Candidate(
                "frozen-restricted",
                "Frozen Restricted",
                [CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage],
                [CollectivePurchaseProductHandlingCodes.FrozenFoodByFacilityReview],
                [ThirdPartyLogisticsProviderCapabilityCodes.ColdChain],
                [CollectivePurchaseLogisticsRestrictionCodes.FrozenFoodNotAccepted])
        };

        var result = 주문자3PL후보필터.적합후보(
            candidates,
            new 주문자3PL후보필터조건
            {
                식품류여부 = true,
                보관온도코드 = 주문자3PL보관온도코드.냉동
            });

        Assert.Equal("frozen-fit", Assert.Single(result).Provider.ProviderKey);
    }

    [Fact]
    public void 냉장은_부패식품취급불가후보를제외하고업체명순으로정렬한다()
    {
        var candidates = new[]
        {
            Candidate(
                "zeta",
                "Zeta Cold",
                [CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage],
                [CollectivePurchaseProductHandlingCodes.RefrigeratedFoodByFacilityReview],
                [ThirdPartyLogisticsProviderCapabilityCodes.TemperatureControlledStorage]),
            Candidate(
                "restricted",
                "Restricted Cold",
                [CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage],
                [CollectivePurchaseProductHandlingCodes.RefrigeratedFoodByFacilityReview],
                [ThirdPartyLogisticsProviderCapabilityCodes.ColdChain],
                [CollectivePurchaseLogisticsRestrictionCodes.PerishableFoodNotAccepted]),
            Candidate(
                "alpha",
                "Alpha Cold",
                [CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage],
                [CollectivePurchaseProductHandlingCodes.RefrigeratedFoodByFacilityReview],
                [ThirdPartyLogisticsProviderCapabilityCodes.ColdChain])
        };

        var result = 주문자3PL후보필터.적합후보(
            candidates,
            new 주문자3PL후보필터조건
            {
                식품류여부 = true,
                보관온도코드 = 주문자3PL보관온도코드.냉장
            });

        Assert.Equal(
            new[] { "alpha", "zeta" },
            result.Select(candidate => candidate.Provider.ProviderKey));
    }

    private static CollectivePurchaseLogisticsProviderCandidate Candidate(
        string providerKey,
        string displayName,
        IReadOnlyList<string> stageCodes,
        IReadOnlyList<string> productHandlingCodes,
        IReadOnlyList<string> capabilityCodes,
        IReadOnlyList<string>? restrictionCodes = null)
        => new()
        {
            Provider = new ThirdPartyLogisticsProviderDirectoryItem
            {
                ProviderKey = providerKey,
                DisplayName = displayName,
                CapabilityCodes = capabilityCodes
            },
            CollectivePurchaseProfile = new CollectivePurchaseLogisticsProfile
            {
                ProviderKey = providerKey,
                StageCodes = stageCodes,
                ProductHandlingCodes = productHandlingCodes,
                ExplicitRestrictionCodes = restrictionCodes ?? []
            }
        };
}
