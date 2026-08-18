using Ssalddel.Unity.ImmersiveWorld;

namespace Ssalddel.Unity.Tests;

public sealed class 몰입WorldLayoutTests
{
    [Fact]
    public void 자연권압력경고는_Apocalypse팩이없어도보이고_몬스터대체표현은생기지않는다()
    {
        var gate = 몰입World자산GatePolicy.Evaluate(Array.Empty<string>());
        var decision = Nature위협RoutePresentationPolicy.Evaluate(
            "Threatened",
            Nature위험단계Codes.EncounterBand,
            Nature위협PresentationKeys.TacticalZombiePressure,
            gate,
            2,
            simulationOnly: true,
            isOperationalState: false);

        Assert.True(decision.ShowWarning);
        Assert.False(decision.Encounter.ShowMonsterActors);
        Assert.Equal(몰입World자산GateCodes.WaitingForApocalypseAssetPack,
            decision.Encounter.BlockReasonCode);
        Assert.False(decision.ChangesWorldState);
        Assert.True(decision.PresentationOnly);
    }

    [Fact]
    public void Apocalypse가없으면_몬스터표현을차단하고_대체자산을허용하지않는다()
    {
        var gate = 몰입World자산GatePolicy.Evaluate(Array.Empty<string>());
        var decision = Nature조우PresentationPolicy.Evaluate(
            Nature위험단계Codes.EncounterBand,
            Nature위협PresentationKeys.TacticalZombiePressure,
            gate, 3, simulationOnly: true, isOperationalState: false);

        Assert.Equal(몰입World자산GateCodes.WaitingForApocalypseAssetPack,
            gate.StateCode);
        Assert.Equal(몰입World자산GateCodes.FallbackForbidden,
            gate.FallbackPolicyCode);
        Assert.False(decision.ShowMonsterActors);
        Assert.Equal(몰입World자산GateCodes.WaitingForApocalypseAssetPack,
            decision.BlockReasonCode);
        Assert.True(decision.PresentationOnly);
        Assert.False(decision.ChangesWorldState);
    }

    [Fact]
    public void 안전중심부와운영상태는_Apocalypse가있어도몬스터를차단한다()
    {
        var gate = 몰입World자산GatePolicy.Evaluate(new[]
        {
            몰입World자산GateCodes.PolygonApocalypsePack,
        });

        var safe = Nature조우PresentationPolicy.Evaluate(
            Nature위험단계Codes.SafeCore,
            Nature위협PresentationKeys.ZombieWarning,
            gate, 3, simulationOnly: true, isOperationalState: false);
        var operational = Nature조우PresentationPolicy.Evaluate(
            Nature위험단계Codes.EncounterBand,
            Nature위협PresentationKeys.ZombieWarning,
            gate, 3, simulationOnly: true, isOperationalState: true);

        Assert.False(safe.ShowMonsterActors);
        Assert.Equal("EncounterBandRequired", safe.BlockReasonCode);
        Assert.False(operational.ShowMonsterActors);
        Assert.Equal("OperationalThreatForbidden", operational.BlockReasonCode);
    }

    [Fact]
    public void 조우외곽은_설치된Apocalypse와Simulation사건만표현한다()
    {
        var gate = 몰입World자산GatePolicy.Evaluate(new[]
        {
            몰입World자산GateCodes.PolygonApocalypsePack,
        });

        var decision = Nature조우PresentationPolicy.Evaluate(
            Nature위험단계Codes.EncounterBand,
            Nature위협PresentationKeys.TacticalZombiePressure,
            gate, 3, simulationOnly: true, isOperationalState: false);

        Assert.True(decision.ShowMonsterActors);
        Assert.Empty(decision.BlockReasonCode);
        Assert.Equal(3, decision.ThreatUnitCount);
    }

    [Fact]
    public void 경관전환은_준비완료시에만원자적으로활성대상을바꾼다()
    {
        var coordinator = new 몰입WorldTransitionCoordinator(
            몰입WorldInstanceCodes.All);

        var requested = coordinator.Request(몰입WorldInstanceCodes.Farm);
        Assert.Equal(몰입WorldInstanceCodes.NatureHome,
            requested.ActiveInstanceStableId);
        Assert.Equal(몰입WorldInstanceCodes.Farm,
            requested.PendingInstanceStableId);

        var failed = coordinator.Complete(
            몰입WorldInstanceCodes.Farm, traversalReady: false);
        Assert.Equal(몰입WorldInstanceCodes.NatureHome,
            failed.ActiveInstanceStableId);
        Assert.False(failed.IsTransitioning);

        coordinator.Request(몰입WorldInstanceCodes.Farm);
        var completed = coordinator.Complete(
            몰입WorldInstanceCodes.Farm, traversalReady: true);
        Assert.Equal(몰입WorldInstanceCodes.Farm,
            completed.ActiveInstanceStableId);
        Assert.False(completed.ChangesWorldState);
        Assert.True(completed.PresentationOnly);
    }

    [Fact]
    public void 경관전환중_다른대상요청과알수없는인스턴스를거부한다()
    {
        var coordinator = new 몰입WorldTransitionCoordinator(
            몰입WorldInstanceCodes.All);
        coordinator.Request(몰입WorldInstanceCodes.Farm);

        Assert.Equal("ImmersiveWorldTransitionInProgress",
            Assert.Throws<InvalidOperationException>(() => coordinator.Request(
                몰입WorldInstanceCodes.Town)).Message);
        coordinator.Cancel();
        Assert.Equal("ImmersiveWorldInstanceUnknown",
            Assert.Throws<InvalidOperationException>(() => coordinator.Request(
                "immersive-instance:unknown")).Message);
    }
}
