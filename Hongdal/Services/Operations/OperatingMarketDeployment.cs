using Hongdal.Application.Operations;
using Hongdal.Contracts.Common.Operations;

namespace Hongdal.Services.Operations;

public sealed class OperatingMarketDeploymentOptions
{
    public const string SectionName = "OperatingMarket";

    public string MarketCode { get; set; } = OperatingMarketCodes.Korea;

    public string? VerifiedLicensedBrokerPartnerId { get; set; }
}

public interface IOperatingMarketDeployment
{
    string MarketCode { get; }

    OperatingMarketProfile Profile { get; }

    string? VerifiedLicensedBrokerPartnerId { get; }
}

public sealed class OperatingMarketDeployment : IOperatingMarketDeployment
{
    public OperatingMarketDeployment(
        string? marketCode,
        string? verifiedLicensedBrokerPartnerId = null)
    {
        if (!OperatingMarketCodes.TryNormalize(marketCode, out var normalizedMarketCode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(marketCode),
                marketCode,
                "The deployment market must be KR or US.");
        }

        MarketCode = normalizedMarketCode;
        Profile = OperatingMarketProfileCatalog.Get(normalizedMarketCode);
        VerifiedLicensedBrokerPartnerId = normalizedMarketCode == OperatingMarketCodes.UnitedStates
            ? NullIfWhiteSpace(verifiedLicensedBrokerPartnerId)
            : null;
    }

    public string MarketCode { get; }

    public OperatingMarketProfile Profile { get; }

    public string? VerifiedLicensedBrokerPartnerId { get; }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class DeploymentOperatingMarketContextAccessor : IOperatingMarketContextAccessor
{
    private readonly OperatingMarketContextSnapshot _current;

    public DeploymentOperatingMarketContextAccessor(IOperatingMarketDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        _current = new OperatingMarketContextSnapshot(
            deployment.MarketCode,
            OperatingMarketContextSourceCodes.Deployment);
    }

    public OperatingMarketContextSnapshot Current => _current;
}
