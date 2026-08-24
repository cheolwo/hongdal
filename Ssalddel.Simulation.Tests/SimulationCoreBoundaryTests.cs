using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationCoreBoundaryTests
{
    [Fact]
    public void Nature위협과지역인과_전용코드는_기존Facade값을보존한다()
    {
        Assert.Equal(SimulationRegionalIncidentCodes.Stable,
            SimulationNatureThreatCodes.Stable);
        Assert.Equal(SimulationRegionalIncidentCodes.Warning,
            SimulationNatureThreatCodes.Warning);
        Assert.Equal(SimulationRegionalIncidentCodes.Threatened,
            SimulationNatureThreatCodes.Threatened);
        Assert.Equal(SimulationRegionalIncidentCodes.Infested,
            SimulationNatureThreatCodes.Infested);
        Assert.Equal(SimulationRegionalIncidentCodes.Active,
            SimulationNatureThreatCodes.Active);

        Assert.Equal(SimulationRegionalIncidentCodes.NormalOutcome,
            SimulationRegionalCausalityCodes.NormalOutcome);
        Assert.Equal(SimulationRegionalIncidentCodes.OpportunityOutcome,
            SimulationRegionalCausalityCodes.OpportunityOutcome);
        Assert.Equal(SimulationRegionalIncidentCodes.ThreatOutcome,
            SimulationRegionalCausalityCodes.ThreatOutcome);
        Assert.Equal(SimulationRegionalIncidentCodes.RecoveryOutcome,
            SimulationRegionalCausalityCodes.RecoveryOutcome);
        Assert.Equal(SimulationRegionalIncidentCodes.SafeIncidentResponse,
            SimulationRegionalCausalityCodes.SafeIncidentResponse);
        Assert.Equal(SimulationRegionalIncidentCodes.UnsafeIncidentResponse,
            SimulationRegionalCausalityCodes.UnsafeIncidentResponse);
        Assert.Equal(SimulationRegionalIncidentCodes.IncidentDeadlineMissed,
            SimulationRegionalCausalityCodes.IncidentDeadlineMissed);
        Assert.Equal(SimulationRegionalIncidentCodes.NatureRestorationCompleted,
            SimulationRegionalCausalityCodes.NatureRestorationCompleted);
        Assert.Equal(SimulationRegionalIncidentCodes.NaturePartyRecoveryCompleted,
            SimulationRegionalCausalityCodes.NaturePartyRecoveryCompleted);
        Assert.Equal(SimulationRegionalIncidentCodes.PositiveTurnCard,
            SimulationRegionalCausalityCodes.PositiveTurnCard);
        Assert.Equal(SimulationRegionalIncidentCodes.ReversedTurnCard,
            SimulationRegionalCausalityCodes.ReversedTurnCard);
    }

    [Fact]
    public void Runtime목적은_실행권위위치와분리된다()
    {
        var playable = new SimulationRuntimeDescriptor
        {
            AuthorityLocation = SimulationAuthorityLocation.RemoteHost,
            Purpose = SimulationRuntimePurpose.Playable,
            RequiresNetwork = true,
        };
        var review = new SimulationRuntimeDescriptor
        {
            AuthorityLocation = SimulationAuthorityLocation.LocalProcess,
            Purpose = SimulationRuntimePurpose.ReviewFixture,
        };
        var legacyReview = new SimulationRuntimeDescriptor
        {
            AuthorityLocation = (SimulationAuthorityLocation)2,
        };

        Assert.True(playable.IsPlayableAuthority);
        Assert.False(review.IsPlayableAuthority);
        Assert.False(legacyReview.IsPlayableAuthority);

#pragma warning disable CS0618
        Assert.False(playable.IsReviewFixture);
        Assert.True(review.IsReviewFixture);
        Assert.True(legacyReview.IsReviewFixture);
#pragma warning restore CS0618
    }
}
