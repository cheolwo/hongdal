using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class HarvestDispositionBranchAdapterTests
{
    [Theory]
    [InlineData(
        HarvestDispositionChoiceCodes.CooperativeShipment,
        HarvestDispositionWorkflowCodes.CooperativeIntakeCandidate)]
    [InlineData(
        HarvestDispositionChoiceCodes.DirectOnlineSale,
        HarvestDispositionWorkflowCodes.ProducerPackingCandidate)]
    [InlineData(
        HarvestDispositionChoiceCodes.ExportAgent,
        HarvestDispositionWorkflowCodes.ExportReadinessCandidate)]
    [InlineData(
        HarvestDispositionChoiceCodes.ReserveStorage,
        HarvestDispositionWorkflowCodes.ReserveStockLotCandidate)]
    public void BRANCH_ADAPTER1_MapsEveryDispositionToServerPreviewAndTaskCandidate(
        string choiceCode,
        string workflowCode)
    {
        var decided = Decide(choiceCode);

        var envelope = Adapter().CreatePreviewEnvelope(decided, "actor:producer.fixture");

        Assert.Equal(decided.Decision!.StableId, envelope.PreviewRequest.DispositionDecisionStableId);
        Assert.Equal(decided.Decision.Revision, envelope.PreviewRequest.DispositionDecisionRevision);
        Assert.Equal(decided.HarvestLot.StableId, envelope.PreviewRequest.HarvestLotStableId);
        Assert.Equal(decided.HarvestLot.Revision, envelope.PreviewRequest.HarvestLotRevision);
        Assert.Equal(decided.HarvestLot.CanonicalProductStableId, envelope.PreviewRequest.ProductStableId);
        Assert.Equal(choiceCode, envelope.PreviewRequest.ChoiceCode);
        Assert.Equal(workflowCode, envelope.PreviewRequest.NextWorkflowCode);
        Assert.Equal("task:harvest-impact:" + decided.Decision.StableId,
            envelope.TaskCandidate.CandidateTaskStableId);
        Assert.Equal(choiceCode + "Work", envelope.TaskCandidate.TaskTypeCode);
        Assert.Equal(new[] { decided.HarvestLot.StableId }, envelope.TaskCandidate.InputLotStableIds);
        Assert.Equal(new[] { workflowCode }, envelope.TaskCandidate.OutputCandidateCodes);
    }

    [Fact]
    public void BRANCH_ADAPTER1_PreservesLineageWithoutInventingPolicyOrEffects()
    {
        var decided = Decide(HarvestDispositionChoiceCodes.DirectOnlineSale);

        var envelope = Adapter().CreatePreviewEnvelope(decided, "actor:producer.fixture");

        Assert.Contains(decided.HarvestLot.StableId, envelope.PreviewRequest.SourceStableIds);
        Assert.Contains(decided.Decision!.StableId, envelope.PreviewRequest.SourceStableIds);
        Assert.Equal(envelope.PreviewRequest.SourceStableIds, envelope.TaskCandidate.SourceStableIds);
        Assert.Equal(envelope.PreviewRequest.SourceStableIds.Distinct().Count(),
            envelope.PreviewRequest.SourceStableIds.Length);
        Assert.True(envelope.RequiresServerPreview);
        Assert.True(envelope.RequiresExplicitConfirmation);
        Assert.True(envelope.ServerMustRecalculatePolicy);
        Assert.True(envelope.DoesNotApplySettlementState);
        Assert.True(envelope.DoesNotCreateCargoOrSale);
    }

    [Fact]
    public void BRANCH_ADAPTER1_RejectsUndecidedInvalidActorAndWorkflowMismatch()
    {
        var undecided = Snapshot();
        Assert.Equal("HarvestDispositionDecisionRequired",
            Assert.Throws<InvalidOperationException>(() =>
                Adapter().CreatePreviewEnvelope(undecided, "actor:producer.fixture")).Message);

        var decided = Decide(HarvestDispositionChoiceCodes.CooperativeShipment);
        Assert.Equal("HarvestDispositionActorStableIdInvalid",
            Assert.Throws<InvalidOperationException>(() =>
                Adapter().CreatePreviewEnvelope(decided, "producer fixture")).Message);

        decided.Decision!.NextWorkflowCode = HarvestDispositionWorkflowCodes.ProducerPackingCandidate;
        decided.Options.Single(value =>
            value.ChoiceCode == HarvestDispositionChoiceCodes.CooperativeShipment).NextWorkflowCode
            = HarvestDispositionWorkflowCodes.ProducerPackingCandidate;
        Assert.Equal("HarvestDispositionWorkflowMismatch",
            Assert.Throws<InvalidOperationException>(() =>
                Adapter().CreatePreviewEnvelope(decided, "actor:producer.fixture")).Message);

        decided = Decide(HarvestDispositionChoiceCodes.CooperativeShipment);
        decided.Decision!.SourceStableIds = decided.Decision.SourceStableIds
            .Append("not a stable id").ToArray();
        Assert.Equal("HarvestDispositionSourceStableIdsInvalid",
            Assert.Throws<InvalidOperationException>(() =>
                Adapter().CreatePreviewEnvelope(decided, "actor:producer.fixture")).Message);
    }

    private static HarvestDispositionBranchAdapter Adapter()
        => new(new HarvestDispositionSimulationValidator());

    private static HarvestDispositionSimulationSnapshot Decide(string choiceCode)
    {
        var snapshot = Snapshot();
        var engine = new HarvestDispositionSimulationEngine(new HarvestDispositionSimulationValidator());
        return engine.Tick(snapshot, engine.Confirm(snapshot, engine.Preview(snapshot, choiceCode)));
    }

    private static HarvestDispositionSimulationSnapshot Snapshot()
        => HarvestDispositionSimulationFixture.Create(Harvested());

    private static 감자재배LifecycleSimulationSnapshot Harvested()
    {
        var validator = new 감자재배LifecycleSimulationValidator(
            new FarmSoilTileSimulationValidator(), new 재배달력ProfileValidator());
        var engine = new 감자재배LifecycleSimulationEngine(validator);
        var source = 감자재배LifecycleSimulationFixture.Create();
        var tile = source.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        source = engine.Tick(source, engine.Confirm(source, engine.PreviewSowing(source, tile.StableId)));
        source = engine.Tick(source, engine.CreateAdvanceDaysCommand(source, 6));
        return engine.Tick(source, engine.Confirm(source, engine.PreviewHarvest(source)));
    }
}
