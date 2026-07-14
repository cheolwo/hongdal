using Hongdal.Contracts.Common.Operations;

namespace Hongdal.Services.Operations;

public interface IOperatingMarketFreightWorkflowPolicy
{
    string MarketCode { get; }

    OperatingMarketFreightWorkflowDecision Evaluate(
        OperatingMarketFreightWorkflowRequest request);
}

public sealed class KoreaOperatingMarketFreightWorkflowPolicy
    : IOperatingMarketFreightWorkflowPolicy
{
    public string MarketCode => OperatingMarketCodes.Korea;

    public OperatingMarketFreightWorkflowDecision Evaluate(
        OperatingMarketFreightWorkflowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new OperatingMarketFreightWorkflowDecision
        {
            MarketCode = MarketCode,
            ArrangementModeCode = FreightArrangementModeCodes.KoreaDomesticTransport,
            CanProceed = true,
            RequiresVerifiedLicensedBrokerPartner = false,
            DecisionCode = OperatingMarketFreightDecisionCodes.Allowed
        };
    }
}

public sealed class UnitedStatesOperatingMarketFreightWorkflowPolicy
    : IOperatingMarketFreightWorkflowPolicy
{
    private readonly IOperatingMarketDeployment _deployment;

    public UnitedStatesOperatingMarketFreightWorkflowPolicy(
        IOperatingMarketDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        if (deployment.MarketCode != OperatingMarketCodes.UnitedStates)
        {
            throw new InvalidOperationException(
                "The United States freight policy requires a US deployment.");
        }

        _deployment = deployment;
    }

    public string MarketCode => OperatingMarketCodes.UnitedStates;

    public OperatingMarketFreightWorkflowDecision Evaluate(
        OperatingMarketFreightWorkflowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requiresPartner = request.RequestsTransportationArrangement;
        var hasVerifiedPartner = !string.IsNullOrWhiteSpace(
            _deployment.VerifiedLicensedBrokerPartnerId);
        var canProceed = !requiresPartner || hasVerifiedPartner;

        return new OperatingMarketFreightWorkflowDecision
        {
            MarketCode = MarketCode,
            ArrangementModeCode = FreightArrangementModeCodes.UnitedStatesLicensedBrokerPartner,
            CanProceed = canProceed,
            RequiresVerifiedLicensedBrokerPartner = requiresPartner,
            DecisionCode = canProceed
                ? OperatingMarketFreightDecisionCodes.Allowed
                : OperatingMarketFreightDecisionCodes.VerifiedLicensedBrokerPartnerRequired
        };
    }
}
