using Ssalddel.Unity.Farm;
using Ssalddel.Unity.PotatoJourney;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class PotatoHubReceivingLifecycleTests
{
    private readonly PotatoHubReceivingSimulationValidator validator=new();
    [Fact]public void HUB1_ReceivingPreview와Confirm은입고와검수를만들지않는다(){var s=Snapshot();var e=Engine();var p=e.PreviewReceiving(s);var c=e.Confirm(s,p);Assert.True(p.RequiresExplicitConfirmation);Assert.Equal(PotatoHubReceivingCommandCodes.StartInspection,c.CommandCode);Assert.Equal(PotatoHubReceivingStateCodes.ArrivedAtHub,s.StateCode);Assert.Null(s.InspectionResult);}
    [Fact]public void HUB1_ReceivingTick만Inspection상태를만든다(){var s=Snapshot();var e=Engine();var next=e.Tick(s,e.Confirm(s,e.PreviewReceiving(s)));Assert.Equal(PotatoHubReceivingStateCodes.ArrivedAtHub,s.StateCode);Assert.Equal(PotatoHubReceivingStateCodes.Inspection,next.StateCode);Assert.Null(next.InspectionResult);Assert.Equal(s.Cargo.StableId,next.Cargo.StableId);}
    [Fact]public void HUB1_InspectionTick은288kg합격12kg손실과이유를보존한다(){var inspection=Inspection();var e=Engine();var accepted=e.Tick(inspection,e.Confirm(inspection,e.PreviewInspection(inspection)));Assert.Equal(PotatoHubReceivingStateCodes.Accepted,accepted.StateCode);Assert.NotNull(accepted.InspectionResult);Assert.Equal(300m,accepted.InspectionResult!.ReceivedQuantityKg);Assert.Equal(288m,accepted.InspectionResult.AcceptedQuantityKg);Assert.Equal(12m,accepted.InspectionResult.RejectedQuantityKg);Assert.Equal("DamageFixture",accepted.InspectionResult.RejectionReasonCode);Assert.Equal(300m,accepted.InspectionResult.AcceptedQuantityKg+accepted.InspectionResult.RejectedQuantityKg);}
    [Fact]public void HUB1_수량변조와StalePreview를거부한다(){var s=Snapshot();s.Rule.AcceptedQuantityKg=290m;Assert.Equal("PotatoHubInspectionRuleInvalid",Assert.Throws<InvalidOperationException>(()=>validator.Validate(s)).Message);s=Snapshot();var p=Engine().PreviewReceiving(s);s.DataRevision++;Assert.Equal("PotatoHubReceivingPreviewStaleOrInvalid",Assert.Throws<InvalidOperationException>(()=>Engine().Confirm(s,p)).Message);}
    [Fact]public void HUB1_합격은Market재고나판매를만들지않는다(){var accepted=Accept();Assert.NotNull(accepted.InspectionResult);Assert.DoesNotContain("market",accepted.SourceStableIds);Assert.DoesNotContain("inventory",accepted.SourceStableIds);Assert.Equal(PotatoCargoJourneyStateCodes.ArrivedAtHub,accepted.Cargo.StateCode);}
    [Fact]public void HUB1_Projector는합격손실이유Lineage와Simulation제한을보존한다(){var accepted=Accept();var v=new PotatoHubReceivingProjector(validator).Project(accepted);Assert.Equal("Accepted",v.StateCode);Assert.Contains("accepted 288kg",v.InspectionText);Assert.Contains("rejected 12kg",v.InspectionText);Assert.Contains("DamageFixture",v.InspectionText);Assert.Contains(accepted.Cargo.StableId,v.LineageText);Assert.Contains("실제 품질 판정이나 재고 입고가 아닙니다",v.LimitationText);}
    private PotatoHubReceivingSimulationEngine Engine()=>new(validator);
    private PotatoHubReceivingSimulationSnapshot Inspection(){var s=Snapshot();var e=Engine();return e.Tick(s,e.Confirm(s,e.PreviewReceiving(s)));}
    private PotatoHubReceivingSimulationSnapshot Accept(){var s=Inspection();var e=Engine();return e.Tick(s,e.Confirm(s,e.PreviewInspection(s)));}
    private static PotatoHubReceivingSimulationSnapshot Snapshot()=>PotatoHubReceivingSimulationFixture.Create(Arrived());
    private static PotatoCargoJourneySimulationSnapshot Arrived(){var loaded=Loaded();var j=PotatoCargoJourneySimulationFixture.Create(loaded);var e=new PotatoCargoJourneySimulationEngine(new PotatoCargoJourneySimulationValidator());j=e.Tick(j,e.ConfirmDispatch(j,e.PreviewDispatch(j)));return e.Tick(j,e.CreateAdvanceRouteCommand(j,3));}
    private static 감자수확CargoSimulationSnapshot Loaded(){var lv=new 감자재배LifecycleSimulationValidator(new FarmSoilTileSimulationValidator(),new 재배달력ProfileValidator());var le=new 감자재배LifecycleSimulationEngine(lv);var s=감자재배LifecycleSimulationFixture.Create();var tile=s.Soil.Tiles.First(x=>x.CultivationStateCode==FarmSoilTileCultivationStateCodes.Tilled);s=le.Tick(s,le.Confirm(s,le.PreviewSowing(s,tile.StableId)));s=le.Tick(s,le.CreateAdvanceDaysCommand(s,6));s=le.Tick(s,le.Confirm(s,le.PreviewHarvest(s)));var c=감자수확CargoSimulationFixture.Create(s.HarvestLot!);var ce=new 감자수확CargoSimulationEngine(new 감자수확CargoSimulationValidator());c=ce.Tick(c,ce.Confirm(c,ce.PreviewPacking(c)));return ce.Tick(c,ce.Confirm(c,ce.PreviewLoading(c)));}
}
