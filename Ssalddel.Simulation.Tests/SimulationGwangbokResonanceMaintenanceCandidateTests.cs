using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Q016 공명·잔향 유지 보조와 영구 유지 금지·주기적 자기 회복 필요를 회귀 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    Boundary = "후보 계약 시험이며 실제 감쇠·기간 이탈·Save/Replay·Play Mode 증거가 아니다.")]
public sealed class SimulationGwangbokResonanceMaintenanceCandidateTests
{
    [Fact]
    public void 공명과잔향은_광복기감쇠를늦출수있지만_영구유지하지않는다()
    {
        var result = new SimulationGwangbokResonanceMaintenanceCandidateEvaluator()
            .Evaluate(Request(true));

        Assert.Equal(Simulation광복기공명유지CandidateCodes.Ready,
            result.ReadinessCode);
        Assert.True(result.ResonanceMaySlowRecoveryDecay);
        Assert.True(result.AfterglowMaySlowRecoveryDecay);
        Assert.False(result.AllowsResonanceOnlyPermanentMaintenance);
        Assert.True(result.RequiresPeriodicSelfRecoveryAction);
        Assert.False(result.AppliesRecoveryDecay);
        Assert.False(result.AppliesPeriodTransition);
        Assert.False(result.ChangesWorldState);
        Assert.Contains(Simulation광복기공명유지CandidateCodes
            .DecayProfileOwnedByQ017, result.UnresolvedDecisionCodes);
    }

    [Fact]
    public void 주기적자기회복갱신이없으면_유지준비가성립하지않는다()
    {
        var result = new SimulationGwangbokResonanceMaintenanceCandidateEvaluator()
            .Evaluate(Request(false));

        Assert.Equal(Simulation광복기공명유지CandidateCodes.Gap,
            result.ReadinessCode);
        Assert.Contains(Simulation광복기공명유지CandidateCodes
            .SelfRecoveryRefreshRequired,
            result.MissingRequirementCodes);
        Assert.False(result.AllowsResonanceOnlyPermanentMaintenance);
    }

    private static Simulation광복기공명유지CandidateRequest Request(
        bool refresh) => new()
        {
            Period = new SimulationNaturePeriodStateSnapshot
            {
                PlayerStableId = "player:owner",
                PeriodStateCode = SimulationNaturePeriodCodes.GwangbokPeriod,
                PeriodInstanceStableId = "period:gwangbok:1",
            },
            EntryAction = new Simulation광복기자기회복행위CandidateSnapshot
            {
                EligibleForGwangbokEntryTrigger = true,
            },
            ActiveResonancePresent = true,
            AfterglowPresent = true,
            PeriodicSelfRecoveryRefreshPresent = refresh,
            MaintenancePolicyRevision = "gwangbok-maintenance.r1",
            AuthorityTimeRevision = "world-tick.r1",
        };
}
