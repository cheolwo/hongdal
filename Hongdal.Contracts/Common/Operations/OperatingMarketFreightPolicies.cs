namespace Hongdal.Contracts.Common.Operations;

public static class OperatingMarketFreightDecisionCodes
{
    public const string Allowed = "Allowed";
    public const string VerifiedLicensedBrokerPartnerRequired =
        "VerifiedLicensedBrokerPartnerRequired";
}

public sealed class OperatingMarketFreightWorkflowRequest
{
    public bool RequestsTransportationArrangement { get; init; }
}

public sealed class OperatingMarketFreightWorkflowDecision
{
    public string MarketCode { get; init; } = string.Empty;

    public string ArrangementModeCode { get; init; } = string.Empty;

    public bool CanProceed { get; init; }

    public bool RequiresVerifiedLicensedBrokerPartner { get; init; }

    public string DecisionCode { get; init; } = string.Empty;
}
