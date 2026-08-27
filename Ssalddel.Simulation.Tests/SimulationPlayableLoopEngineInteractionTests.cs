using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "폐루프 하나에서 WI 권위 실행과 LH·Sky·실내외 표현 순서 및 경계를 자동 검증한다.",
    Boundary = "자동 시험은 SimulationWorldShell 실제 입력·Game View E7 증거를 대신하지 않는다.")]
public sealed class SimulationPlayableLoopEngineInteractionTests
{
    [Fact]
    public void Nature수면WI는_권위뒤_LH_Sky_실외_실내_귀환순서로닫힌다()
    {
        var profile = SimulationNatureFirstDayEngineValidationProfiles.Get(
            "WI-NATURE-14");
        var trace = CompleteTrace("WI-NATURE-14", "command:sleep");

        var result = new SimulationPlayableLoopEngineInteractionValidator()
            .Validate(profile, trace, "command:sleep");

        Assert.True(result.Passed);
        Assert.Empty(result.FailureCodes);
        Assert.Equal(18, result.TraceEntries.Length);
    }

    [Fact]
    public void 필수Sky단계누락은_표현E5재검토로차단한다()
    {
        var profile = SimulationNatureFirstDayEngineValidationProfiles.Get(
            "WI-NATURE-14");
        var trace = CompleteTrace("WI-NATURE-14", "command:sleep")
            .Where(value => value.ComponentCode !=
                            SimulationEngineInteractionComponentCodes
                                .SkyPresentation).ToArray();

        var result = new SimulationPlayableLoopEngineInteractionValidator()
            .Validate(profile, trace, "command:sleep");

        Assert.False(result.Passed);
        Assert.Equal("E5", result.EarliestReopenEvidenceStageCode);
        Assert.Contains(result.FailureCodes, value => value.StartsWith(
            "EngineInteractionRequiredStepMissing:Sky.Presentation",
            StringComparison.Ordinal));
    }

    [Fact]
    public void Unity표현단계가_권위Revision을바꾸면차단한다()
    {
        var profile = SimulationNatureFirstDayEngineValidationProfiles.Get(
            "WI-NATURE-13");
        var trace = CompleteTrace("WI-NATURE-13", "command:store");
        trace.Single(value => value.ComponentCode ==
            SimulationEngineInteractionComponentCodes.InteriorPlacement
                              && value.PhaseCode ==
                              SimulationEngineInteractionPhaseCodes
                                  .InteriorPlacement)
            .AfterAuthorityRevision++;

        var result = new SimulationPlayableLoopEngineInteractionValidator()
            .Validate(profile, trace, "command:store");

        Assert.False(result.Passed);
        Assert.Contains("PresentationMutatedAuthorityRevision:Placement.Interior",
            result.FailureCodes);
    }

    [Fact]
    public void 재사용단계와_LocalProcess_RemoteHost는_같은순서계약을쓴다()
    {
        var profile = SimulationNatureFirstDayEngineValidationProfiles.Get(
            "WI-NATURE-15");
        var local = CompleteTrace("WI-NATURE-15", "command:plan",
            "LocalProcess");
        var remote = CompleteTrace("WI-NATURE-15", "command:plan",
            "RemoteHost");
        foreach (var entry in remote)
            if (entry.ComponentCode ==
                SimulationEngineInteractionComponentCodes.AuthorityCore)
                entry.StatusCode =
                    SimulationEngineInteractionStatusCodes.Reused;

        var validator = new SimulationPlayableLoopEngineInteractionValidator();
        var localResult = validator.Validate(profile, local, "command:plan");
        var remoteResult = validator.Validate(profile, remote, "command:plan");

        Assert.True(localResult.Passed);
        Assert.True(remoteResult.Passed);
        Assert.Equal(localResult.TraceEntries.Select(value =>
                (value.ComponentCode, value.PhaseCode)),
            remoteResult.TraceEntries.Select(value =>
                (value.ComponentCode, value.PhaseCode)));
    }

    [Fact]
    public void 행위원장누락은_논리E5를다시연다()
    {
        var profile = SimulationNatureFirstDayEngineValidationProfiles.Get(
            "WI-NATURE-15");
        var trace = CompleteTrace("WI-NATURE-15", "command:missing-journal")
            .Where(value => value.ComponentCode !=
                            SimulationEngineInteractionComponentCodes
                                .ActionJournal).ToArray();

        var result = new SimulationPlayableLoopEngineInteractionValidator()
            .Validate(profile, trace, "command:missing-journal");

        Assert.False(result.Passed);
        Assert.Equal("E5", result.EarliestReopenEvidenceStageCode);
        Assert.Contains(result.FailureCodes, value => value.StartsWith(
            "EngineInteractionRequiredStepMissing:Simulation.ActionJournal",
            StringComparison.Ordinal));
    }

    [Fact]
    public void 분야성장NotApplicable은_사유가있어야통과한다()
    {
        var profile = SimulationNatureFirstDayEngineValidationProfiles.Get(
            "WI-NATURE-15");
        var trace = CompleteTrace("WI-NATURE-15", "command:no-progress");
        var progression = trace.Single(value => value.ComponentCode ==
            SimulationEngineInteractionComponentCodes.PlayerDomainProgression);
        progression.StatusCode =
            SimulationEngineInteractionStatusCodes.NotApplicable;

        var validator = new SimulationPlayableLoopEngineInteractionValidator();
        var missingReason = validator.Validate(profile, trace,
            "command:no-progress");
        progression.ReasonCode = "WorldInteractionHasNoPlayerProgressBinding";
        var explained = validator.Validate(profile, trace,
            "command:no-progress");

        Assert.False(missingReason.Passed);
        Assert.Contains(
            "EngineInteractionNotApplicableReasonMissing:Simulation.PlayerDomainProgression",
            missingReason.FailureCodes);
        Assert.True(explained.Passed);
    }

    [Fact]
    public void 명상성장NotApplicable은_사유가있어야통과한다()
    {
        var profile = SimulationNatureFirstDayEngineValidationProfiles.Get(
            "WI-NATURE-15");
        var trace = CompleteTrace("WI-NATURE-15", "command:no-meditation");
        var progression = trace.Single(value => value.ComponentCode ==
            SimulationEngineInteractionComponentCodes
                .PlayerMeditationProgression);
        progression.StatusCode =
            SimulationEngineInteractionStatusCodes.NotApplicable;

        var validator = new SimulationPlayableLoopEngineInteractionValidator();
        var missingReason = validator.Validate(profile, trace,
            "command:no-meditation");
        progression.ReasonCode = "MeditationContributionNotPresent";
        var explained = validator.Validate(profile, trace,
            "command:no-meditation");

        Assert.False(missingReason.Passed);
        Assert.Contains(
            "EngineInteractionNotApplicableReasonMissing:Simulation.PlayerMeditationProgression",
            missingReason.FailureCodes);
        Assert.True(explained.Passed);
    }

    [Fact]
    public void 수집기는_명령별순서를부여하고_사본을반환한다()
    {
        var sink = new InMemorySimulationPlayableLoopEngineTraceSink();
        var first = Entry("WI-NATURE-13", "command:store", 0,
            SimulationEngineInteractionComponentCodes.WorldInteractionPipeline,
            SimulationEngineInteractionPhaseCodes.Preview,
            SimulationEngineInteractionComponentKinds.Orchestration);
        sink.Record(first);
        sink.Record(Entry("WI-NATURE-13", "command:store", 0,
            SimulationEngineInteractionComponentCodes.AuthorityCore,
            SimulationEngineInteractionPhaseCodes.AuthorityCommit,
            SimulationEngineInteractionComponentKinds.Authority));

        var snapshot = sink.Snapshot(
            SimulationNatureFirstDayEngineValidationProfiles
                .PlayableLoopStableId, "WI-NATURE-13", "command:store");
        snapshot[0].ComponentCode = "changed-by-caller";
        var secondRead = sink.Snapshot(
            SimulationNatureFirstDayEngineValidationProfiles
                .PlayableLoopStableId, "WI-NATURE-13", "command:store");

        Assert.Equal(new[] { 1, 2 }, secondRead.Select(value => value.Sequence));
        Assert.Equal(
            SimulationEngineInteractionComponentCodes.WorldInteractionPipeline,
            secondRead[0].ComponentCode);
    }

    private static SimulationPlayableLoopEngineTraceEntry[] CompleteTrace(
        string worldInteractionId, string commandId,
        string authorityLocationCode = "LocalProcess")
    {
        var phases = new List<(string component, string phase, string kind)>
        {
            (SimulationEngineInteractionComponentCodes.WorldInteractionPipeline,
                SimulationEngineInteractionPhaseCodes.Preview,
                SimulationEngineInteractionComponentKinds.Orchestration),
            (SimulationEngineInteractionComponentCodes.WorldInteractionPipeline,
                SimulationEngineInteractionPhaseCodes.Confirm,
                SimulationEngineInteractionComponentKinds.Orchestration),
            (SimulationEngineInteractionComponentCodes.WorldInteractionPipeline,
                SimulationEngineInteractionPhaseCodes.FocusEvidenceCollect,
                SimulationEngineInteractionComponentKinds.Orchestration),
            (SimulationEngineInteractionComponentCodes.AuthorityCore,
                SimulationEngineInteractionPhaseCodes.AuthorityCommit,
                SimulationEngineInteractionComponentKinds.Authority),
            (SimulationEngineInteractionComponentCodes.ActionJournal,
                SimulationEngineInteractionPhaseCodes.ActionRecordAppend,
                SimulationEngineInteractionComponentKinds.Authority),
            (SimulationEngineInteractionComponentCodes.PlayerDomainProgression,
                SimulationEngineInteractionPhaseCodes.PlayerProgressionApply,
                SimulationEngineInteractionComponentKinds.Authority),
            (SimulationEngineInteractionComponentCodes.PlayerMeditationProgression,
                SimulationEngineInteractionPhaseCodes.MeditationProgressionApply,
                SimulationEngineInteractionComponentKinds.Authority),
            (SimulationEngineInteractionComponentCodes.WorldInteractionPipeline,
                SimulationEngineInteractionPhaseCodes.ReturnProjection,
                SimulationEngineInteractionComponentKinds.Orchestration),
        };
        if (worldInteractionId == "WI-NATURE-14")
        {
            phases.Add((SimulationEngineInteractionComponentCodes.LhSurface,
                SimulationEngineInteractionPhaseCodes.ActionRecordRead,
                SimulationEngineInteractionComponentKinds.Presentation));
            phases.Add((SimulationEngineInteractionComponentCodes.LhSurface,
                SimulationEngineInteractionPhaseCodes.SurfacePreparation,
                SimulationEngineInteractionComponentKinds.Presentation));
            phases.Add((
                SimulationEngineInteractionComponentCodes.SkyPresentation,
                SimulationEngineInteractionPhaseCodes.ActionRecordRead,
                SimulationEngineInteractionComponentKinds.Presentation));
            phases.Add((
                SimulationEngineInteractionComponentCodes.SkyPresentation,
                SimulationEngineInteractionPhaseCodes.AtmosphereProjection,
                SimulationEngineInteractionComponentKinds.Presentation));
            phases.Add((
                SimulationEngineInteractionComponentCodes.ExteriorPlacement,
                SimulationEngineInteractionPhaseCodes.ActionRecordRead,
                SimulationEngineInteractionComponentKinds.Presentation));
            phases.Add((
                SimulationEngineInteractionComponentCodes.ExteriorPlacement,
                SimulationEngineInteractionPhaseCodes.ExteriorPlacement,
                SimulationEngineInteractionComponentKinds.Presentation));
        }
        phases.Add((
            SimulationEngineInteractionComponentCodes.InteriorPlacement,
            SimulationEngineInteractionPhaseCodes.ActionRecordRead,
            SimulationEngineInteractionComponentKinds.Presentation));
        phases.Add((
            SimulationEngineInteractionComponentCodes.InteriorPlacement,
            SimulationEngineInteractionPhaseCodes.InteriorPlacement,
            SimulationEngineInteractionComponentKinds.Presentation));
        phases.Add((
            SimulationEngineInteractionComponentCodes.WorldPresentation,
            SimulationEngineInteractionPhaseCodes.ActionRecordRead,
            SimulationEngineInteractionComponentKinds.Presentation));
        phases.Add((
            SimulationEngineInteractionComponentCodes.WorldPresentation,
            SimulationEngineInteractionPhaseCodes.ReturnProjection,
            SimulationEngineInteractionComponentKinds.Presentation));
        return phases.Select((value, index) => Entry(worldInteractionId,
            commandId, index + 1, value.component, value.phase, value.kind,
            authorityLocationCode)).ToArray();
    }

    private static SimulationPlayableLoopEngineTraceEntry Entry(
        string worldInteractionId, string commandId, int sequence,
        string componentCode, string phaseCode, string kindCode,
        string authorityLocationCode = "LocalProcess") => new()
    {
        PlayableLoopStableId = SimulationNatureFirstDayEngineValidationProfiles
            .PlayableLoopStableId,
        WorldInteractionId = worldInteractionId,
        CommandId = commandId,
        AuthorityLocationCode = authorityLocationCode,
        ComponentCode = componentCode,
        ComponentKindCode = kindCode,
        ComponentRevision = "test.r1",
        PhaseCode = phaseCode,
        Sequence = sequence,
        InputHashSha256 = "input",
        OutputHashSha256 = "output",
        StatusCode = SimulationEngineInteractionStatusCodes.Executed,
        BeforeAuthorityRevision = 10,
        AfterAuthorityRevision = kindCode ==
                                 SimulationEngineInteractionComponentKinds
                                     .Authority ? 11 : 10,
    };
}
