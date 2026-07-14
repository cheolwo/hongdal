using Hongdal.Contracts.Common.Operations;
using Hongdal.Services.Operations;

namespace Hongdal.Tests.Services.Operations;

public sealed class OperatingMarketFreightWorkflowPolicyTests
{
    [Fact]
    public void KoreaPolicy_UsesDomesticWorkflow()
    {
        var policy = new KoreaOperatingMarketFreightWorkflowPolicy();

        var decision = policy.Evaluate(new OperatingMarketFreightWorkflowRequest
        {
            RequestsTransportationArrangement = true
        });

        Assert.True(decision.CanProceed);
        Assert.False(decision.RequiresVerifiedLicensedBrokerPartner);
        Assert.Equal(
            FreightArrangementModeCodes.KoreaDomesticTransport,
            decision.ArrangementModeCode);
    }

    [Fact]
    public void UnitedStatesPolicy_AllowsSoftwareOnlyWorkflowWithoutPartner()
    {
        var policy = CreateUnitedStatesPolicy();

        var decision = policy.Evaluate(new OperatingMarketFreightWorkflowRequest());

        Assert.True(decision.CanProceed);
        Assert.False(decision.RequiresVerifiedLicensedBrokerPartner);
        Assert.Equal(OperatingMarketFreightDecisionCodes.Allowed, decision.DecisionCode);
    }

    [Fact]
    public void UnitedStatesPolicy_BlocksArrangementWithoutVerifiedPartner()
    {
        var policy = CreateUnitedStatesPolicy();

        var decision = policy.Evaluate(new OperatingMarketFreightWorkflowRequest
        {
            RequestsTransportationArrangement = true
        });

        Assert.False(decision.CanProceed);
        Assert.True(decision.RequiresVerifiedLicensedBrokerPartner);
        Assert.Equal(
            OperatingMarketFreightDecisionCodes.VerifiedLicensedBrokerPartnerRequired,
            decision.DecisionCode);
    }

    [Fact]
    public void UnitedStatesPolicy_AllowsArrangementWithVerifiedPartner()
    {
        var policy = CreateUnitedStatesPolicy("broker-partner-1");

        var decision = policy.Evaluate(new OperatingMarketFreightWorkflowRequest
        {
            RequestsTransportationArrangement = true
        });

        Assert.True(decision.CanProceed);
        Assert.True(decision.RequiresVerifiedLicensedBrokerPartner);
        Assert.Equal(OperatingMarketFreightDecisionCodes.Allowed, decision.DecisionCode);
    }

    private static UnitedStatesOperatingMarketFreightWorkflowPolicy CreateUnitedStatesPolicy(
        string? verifiedLicensedBrokerPartnerId = null)
        => new(new OperatingMarketDeployment(
            OperatingMarketCodes.UnitedStates,
            verifiedLicensedBrokerPartnerId));
}
