using System;
using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "광복기·자기 진입 행위·공명·잔향을 읽어 유지 보조와 자기 회복 갱신 필요를 판정한다.",
        StepKey = "domain.gwangbok-resonance-maintenance-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.gwangbok-self-recovery-action-candidate",
            "domain.gwangbok-self-recovery-action-candidate",
            "contract.gwangbok-resonance-maintenance-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 34,
        Boundary = "유지 준비도만 판정하며 Recovery·기간·WorldTick·Save 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q016 공명·잔향 유지 보조와 주기적 자기 회복 갱신 관문을 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 감쇠·기간 이탈·Runtime 증거가 아니다.")]
    public sealed class SimulationGwangbokResonanceMaintenanceCandidateEvaluator
    {
        public Simulation광복기공명유지CandidateSnapshot Evaluate(
            Simulation광복기공명유지CandidateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var period = request.Period ?? new SimulationNaturePeriodStateSnapshot();
            var entry = request.EntryAction
                ?? new Simulation광복기자기회복행위CandidateSnapshot();
            var missing = new List<string>();
            if (!string.Equals(period.PeriodStateCode,
                    SimulationNaturePeriodCodes.GwangbokPeriod,
                    StringComparison.Ordinal))
                missing.Add(Simulation광복기공명유지CandidateCodes
                    .GwangbokPeriodRequired);
            if (!entry.EligibleForGwangbokEntryTrigger)
                missing.Add(Simulation광복기공명유지CandidateCodes
                    .SelfEntryActionRequired);
            if (string.IsNullOrWhiteSpace(request.MaintenancePolicyRevision))
                missing.Add(Simulation광복기공명유지CandidateCodes
                    .MaintenancePolicyRevisionRequired);
            if (string.IsNullOrWhiteSpace(request.AuthorityTimeRevision))
                missing.Add(Simulation광복기공명유지CandidateCodes
                    .AuthorityTimeRevisionRequired);
            if (!request.PeriodicSelfRecoveryRefreshPresent)
                missing.Add(Simulation광복기공명유지CandidateCodes
                    .SelfRecoveryRefreshRequired);

            return new Simulation광복기공명유지CandidateSnapshot
            {
                ReadinessCode = missing.Count == 0
                    ? Simulation광복기공명유지CandidateCodes.Ready
                    : Simulation광복기공명유지CandidateCodes.Gap,
                PlayerStableId = period.PlayerStableId,
                PeriodInstanceStableId = period.PeriodInstanceStableId,
                MaintenancePolicyRevision =
                    request.MaintenancePolicyRevision?.Trim() ?? string.Empty,
                AuthorityTimeRevision =
                    request.AuthorityTimeRevision?.Trim() ?? string.Empty,
                MissingRequirementCodes = missing.ToArray(),
                ResonanceMaySlowRecoveryDecay =
                    request.ActiveResonancePresent,
                AfterglowMaySlowRecoveryDecay = request.AfterglowPresent,
                AllowsResonanceOnlyPermanentMaintenance = false,
                RequiresPeriodicSelfRecoveryAction = true,
                PeriodicSelfRecoveryRefreshPresent =
                    request.PeriodicSelfRecoveryRefreshPresent,
                AppliesRecoveryDecay = false,
                AppliesPeriodTransition = false,
                ChangesWorldState = false,
            };
        }
    }
}
