using Ssalddel.Contracts.Common.Operations;

namespace Ssalddel.Contracts.Common.Orderer;

public static class 주문자3PL보관온도코드
{
    public const string 상온 = "Ambient";
    public const string 냉장 = "Refrigerated";
    public const string 냉동 = "Frozen";

    public static IReadOnlyList<string> 지원목록 { get; } = [상온, 냉장, 냉동];

    public static string 정규화(string? value)
        => 지원목록.Contains(value?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            ? value!.Trim()
            : 상온;
}

public sealed class 주문자3PL후보필터조건
{
    public bool 식품류여부 { get; set; }
    public string 보관온도코드 { get; set; } = 주문자3PL보관온도코드.상온;
}

public static class 주문자3PL후보필터
{
    public static string 요구상품취급코드(주문자3PL후보필터조건 condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        return 주문자3PL보관온도코드.정규화(condition.보관온도코드) switch
        {
            주문자3PL보관온도코드.냉장 =>
                CollectivePurchaseProductHandlingCodes.RefrigeratedFoodByFacilityReview,
            주문자3PL보관온도코드.냉동 =>
                CollectivePurchaseProductHandlingCodes.FrozenFoodByFacilityReview,
            _ when condition.식품류여부 =>
                CollectivePurchaseProductHandlingCodes.ShelfStablePackagedFoodByReview,
            _ => CollectivePurchaseProductHandlingCodes.GeneralMerchandise
        };
    }

    public static IReadOnlyList<CollectivePurchaseLogisticsProviderCandidate> 적합후보(
        IEnumerable<CollectivePurchaseLogisticsProviderCandidate> candidates,
        주문자3PL후보필터조건 condition)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(condition);

        var handlingCode = 요구상품취급코드(condition);
        var temperature = 주문자3PL보관온도코드.정규화(condition.보관온도코드);

        return candidates
            .Where(candidate => candidate.CollectivePurchaseProfile.StageCodes.Contains(
                CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage,
                StringComparer.OrdinalIgnoreCase))
            .Where(candidate => candidate.CollectivePurchaseProfile.ProductHandlingCodes.Contains(
                handlingCode,
                StringComparer.OrdinalIgnoreCase))
            .Where(candidate => !온도취급제한(candidate, temperature))
            .Where(candidate => 온도역량충족(candidate, temperature))
            .OrderBy(candidate => candidate.Provider.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool 온도취급제한(
        CollectivePurchaseLogisticsProviderCandidate candidate,
        string temperature)
    {
        var restrictions = candidate.CollectivePurchaseProfile.ExplicitRestrictionCodes;
        if (temperature == 주문자3PL보관온도코드.냉동)
        {
            return restrictions.Contains(
                       CollectivePurchaseLogisticsRestrictionCodes.FrozenFoodNotAccepted,
                       StringComparer.OrdinalIgnoreCase)
                   || restrictions.Contains(
                       CollectivePurchaseLogisticsRestrictionCodes.ClimateControlledGoodsNotAccepted,
                       StringComparer.OrdinalIgnoreCase)
                   || restrictions.Contains(
                       CollectivePurchaseLogisticsRestrictionCodes.PerishableFoodNotAccepted,
                       StringComparer.OrdinalIgnoreCase);
        }

        return temperature == 주문자3PL보관온도코드.냉장
               && (restrictions.Contains(
                       CollectivePurchaseLogisticsRestrictionCodes.ClimateControlledGoodsNotAccepted,
                       StringComparer.OrdinalIgnoreCase)
                   || restrictions.Contains(
                       CollectivePurchaseLogisticsRestrictionCodes.PerishableFoodNotAccepted,
                       StringComparer.OrdinalIgnoreCase));
    }

    private static bool 온도역량충족(
        CollectivePurchaseLogisticsProviderCandidate candidate,
        string temperature)
    {
        if (temperature == 주문자3PL보관온도코드.상온)
        {
            return true;
        }

        var capabilities = candidate.Provider.CapabilityCodes;
        return capabilities.Contains(
                   ThirdPartyLogisticsProviderCapabilityCodes.ColdChain,
                   StringComparer.OrdinalIgnoreCase)
               || capabilities.Contains(
                   ThirdPartyLogisticsProviderCapabilityCodes.TemperatureControlledStorage,
                   StringComparer.OrdinalIgnoreCase);
    }
}
