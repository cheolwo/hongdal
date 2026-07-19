namespace Ssalddel.Contracts.Common.Operations;

public static class OperatingMarketContextKeys
{
    public const string ClaimType = "ssalddel:operating_market";
    public const string HeaderName = "X-Ssalddel-Operating-Market";
}

public static class OperatingMarketContextSourceCodes
{
    public const string Deployment = "Deployment";
    public const string Claim = "Claim";
    public const string Header = "Header";
    public const string Default = "Default";
}

public sealed record OperatingMarketContextSnapshot(
    string MarketCode,
    string SourceCode,
    string TimeZoneId = OperatingTimeZoneIds.CoordinatedUniversal)
{
    public OperatingMarketProfile Profile => OperatingMarketProfileCatalog.Get(MarketCode);
}
