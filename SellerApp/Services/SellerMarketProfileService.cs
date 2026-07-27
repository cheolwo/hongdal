using Ssalddel.Contracts.Common.Operations;

namespace SellerApp.Services;

public sealed class SellerMarketProfileService
{
    private const string MarketCodeKey = "ssalddel.seller.market_code";

    public SellerMarketProfileService()
    {
        MarketCode = OperatingMarketCodes.Normalize(
            Preferences.Default.Get(MarketCodeKey, OperatingMarketCodes.Korea));
    }

    public event Action? Changed;

    public string MarketCode { get; private set; }
    public OperatingMarketProfile Profile => OperatingMarketProfileCatalog.Get(MarketCode);
    public bool IsKorea => MarketCode == OperatingMarketCodes.Korea;

    public void SetMarket(string marketCode)
    {
        var normalized = OperatingMarketCodes.Normalize(marketCode);
        if (string.Equals(MarketCode, normalized, StringComparison.Ordinal))
        {
            return;
        }

        MarketCode = normalized;
        Preferences.Default.Set(MarketCodeKey, normalized);
        Changed?.Invoke();
    }
}
