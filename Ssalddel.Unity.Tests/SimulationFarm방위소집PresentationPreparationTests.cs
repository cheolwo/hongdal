using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.Cards;
using Xunit;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "Farm 방위 소집 카드의 상태별 H 기준점·VisualKey·자산 후보·fallback과 읽기 전용 경계를 검증한다.",
    WorkOrderIds = new[] { "E7-WO-FARM-BARRACKS-DEFENSE" },
    WorldInteractionIds = new[] { "WI-FARM-DEFENSE-MOBILIZE" },
    Boundary = "자동 시험은 실제 Synty Prefab·Scene·위치·Renderer·Collider·Rig·입력·Game View를 대신하지 않는다.")]
public sealed class SimulationFarm방위소집PresentationPreparationTests
{
    private static Farm방위소집VisualBinding Binding(string status) => new() {
        StatusCode = status,
        VisualKey = status == SimulationFarm방위소집Codes.대기 ? Farm방위소집PresentationCodes.StationedVisualKey : Farm방위소집PresentationCodes.MobilizedVisualKey,
        PrimaryAssetCandidateRef = status == SimulationFarm방위소집Codes.대기 ? "Assets/Synty/PolygonFarm/Prefabs/Buildings/SM_Bld_Shelter_01.prefab" : "Assets/Synty/PolygonConstruction/Prefabs/Characters/SM_Chr_Builder_Male_01.prefab",
        AlternativeAssetCandidateRef = status == SimulationFarm방위소집Codes.대기 ? "Assets/Synty/PolygonFarm/Prefabs/Props/SM_Prop_Rack_01.prefab" : "Assets/Synty/PolygonConstruction/Prefabs/Props/SM_Prop_Barrier_Long_01.prefab",
        FallbackVisualKey = Farm방위소집PresentationCodes.FallbackVisualKey,
        CandidateRevisionOrFingerprint = "fixture:farm-defense.r1", AnimationRoleCode = "FarmDefenseSquad", ActionCueCode = "Squad.Mobilize",
        PrimaryAnimationClipRef = "Assets/Synty/PolygonStarter/Animations/Movement/AN_Standing_Run_Fwd.fbx", FallbackActionCueCode = "Squad.State.Static" };

    private static SimulationFarm방위소집CardSnapshot Card(string id, string status, long revision = 7) => new() {
        CardStableId = "card:" + id, SourceWorldRevision = revision, SquadStableId = id, StatusCode = status,
        ThreatStableId = status == SimulationFarm방위소집Codes.출동 ? "threat:1" : string.Empty,
        AssignedWorkerCount = 3, ProductionContributionSuspended = status == SimulationFarm방위소집Codes.출동 };

    [Fact] public void 대기_분대는_집결_기준점과_Preview를_사용한다() { var x = new Farm방위소집PresentationPreparationProjector().Project("world:1", new[] { Card("s1", SimulationFarm방위소집Codes.대기) }, new[] { Binding(SimulationFarm방위소집Codes.대기) }); Assert.Equal(Farm방위소집PresentationCodes.MusterAnchor, x.Squads[0].RequiredHCapability); Assert.True(x.Squads[0].CanRequestPreview); Assert.False(x.MutatesCanonicalState); }
    [Fact] public void 출동_분대는_감시_기준점과_생산중단을_읽는다() { var x = new Farm방위소집PresentationPreparationProjector().Project("world:1", new[] { Card("s1", SimulationFarm방위소집Codes.출동) }, new[] { Binding(SimulationFarm방위소집Codes.출동) }); Assert.Equal(Farm방위소집PresentationCodes.WatchAnchor, x.Squads[0].RequiredHCapability); Assert.True(x.Squads[0].ProductionContributionSuspended); Assert.False(x.Squads[0].CanConfirmAuthority); }
    [Fact] public void 결속이_없으면_fallback을_사용한다() { var x = new Farm방위소집PresentationPreparationProjector().Project("world:1", new[] { Card("s1", SimulationFarm방위소집Codes.대기) }, Array.Empty<Farm방위소집VisualBinding>()); Assert.Equal(Farm방위소집PresentationCodes.FallbackVisualKey, x.Squads[0].VisualKey); }
    [Fact] public void 분대_순서와_hash는_결정적이다() { var p = new Farm방위소집PresentationPreparationProjector(); var a = p.Project("world:1", new[] { Card("b", SimulationFarm방위소집Codes.대기), Card("a", SimulationFarm방위소집Codes.출동) }, new[] { Binding(SimulationFarm방위소집Codes.대기), Binding(SimulationFarm방위소집Codes.출동) }); var b = p.Project("world:1", new[] { Card("a", SimulationFarm방위소집Codes.출동), Card("b", SimulationFarm방위소집Codes.대기) }, new[] { Binding(SimulationFarm방위소집Codes.출동), Binding(SimulationFarm방위소집Codes.대기) }); Assert.Equal("a", a.Squads[0].SquadStableId); Assert.Equal(a.PlanHashSha256, b.PlanHashSha256); }
    [Fact] public void 서로_다른_revision은_거부한다() => Assert.Throws<InvalidOperationException>(() => new Farm방위소집PresentationPreparationProjector().Project("world:1", new[] { Card("a", SimulationFarm방위소집Codes.대기, 1), Card("b", SimulationFarm방위소집Codes.대기, 2) }, new[] { Binding(SimulationFarm방위소집Codes.대기) }));
    [Fact] public void 중복_상태_결속은_거부한다() => Assert.Throws<InvalidOperationException>(() => new Farm방위소집PresentationPreparationProjector().Project("world:1", new[] { Card("a", SimulationFarm방위소집Codes.대기) }, new[] { Binding(SimulationFarm방위소집Codes.대기), Binding(SimulationFarm방위소집Codes.대기) }));
    [Fact] public void 중복_분대는_거부한다() => Assert.Throws<InvalidOperationException>(() => new Farm방위소집PresentationPreparationProjector().Project("world:1", new[] { Card("a", SimulationFarm방위소집Codes.대기), Card("a", SimulationFarm방위소집Codes.대기) }, new[] { Binding(SimulationFarm방위소집Codes.대기) }));
}
