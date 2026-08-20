using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationLocalCombatTests
{
    [Fact]
    public void 사건규모는_소규모_현장_독립전장을_결정적으로_구분한다()
    {
        Assert.Equal(SimulationLocalCombatCodes.Instant,
            SimulationCombatScalePolicy.Evaluate(3, 0, "Wolf")
                .EncounterScaleCode);
        Assert.Equal(SimulationLocalCombatCodes.Field,
            SimulationCombatScalePolicy.Evaluate(4, 0, "Wolf")
                .EncounterScaleCode);
        Assert.Equal(SimulationLocalCombatCodes.DerivedBattlefield,
            SimulationCombatScalePolicy.Evaluate(6, 0, "Wolf")
                .CombatSpaceCode);
        Assert.Equal(SimulationLocalCombatCodes.DerivedBattlefield,
            SimulationCombatScalePolicy.Evaluate(1, 0, "BossOperation")
                .CombatSpaceCode);
    }

    [Fact]
    public void 현장전투는_WorldTick을_고정하고_행동과_예고를_저장재생한다()
    {
        var state = new SimulationBattleInstanceState(Context());
        var initial = state.Snapshot();
        Assert.Equal(SimulationLocalCombatCodes.WorldLocal,
            initial.CombatSpaceCode);
        Assert.Equal(SimulationBattleInstanceCodes.Active, initial.PhaseCode);
        Assert.Equal(3, initial.LocalCombat.Actors.Length);

        var attacked = state.ConfirmLocalAction(
            new SimulationLocalCombatActionConfirmRequest
            {
                CommandId = "command:attack:1",
                ExpectedBattleRevision = initial.BattleRevision,
                RequestingActorStableId = "actor:player",
                TargetActorStableId = "threat:encounter:local:00",
                ActionCode = SimulationLocalCombatCodes.BasicAttack,
            });
        Assert.Single(attacked.LocalCombat.Actions);
        Assert.Equal(750, attacked.LocalCombat.Actors.Single(value =>
            value.ActorStableId == "threat:encounter:local:00").HealthPermille);

        var advanced = state.Advance(new SimulationBattleAdvanceRequest
        {
            CommandId = "command:ticks:1",
            ExpectedBattleRevision = attacked.BattleRevision,
            CombatTickCount = 1,
        }, 7, 0);
        Assert.True(advanced.LocalCombat.HostileTelegraphActive);
        Assert.Equal(7, advanced.LocalCombat.FrozenWorldTick);
        Assert.Throws<SimulationConflictException>(() => state.Advance(
            new SimulationBattleAdvanceRequest
            {
                CommandId = "command:ticks:wrong-world",
                ExpectedBattleRevision = advanced.BattleRevision,
                CombatTickCount = 1,
            }, 8, 0));

        var save = state.CreateSaveRecord();
        var restored = SimulationBattleInstanceState.Restore(save).Snapshot();
        Assert.Equal(advanced.ReplayHashSha256, restored.ReplayHashSha256);
        Assert.True(restored.LocalCombat.HostileTelegraphActive);
        Assert.Single(restored.LocalCombat.Actions);
    }

    [Fact]
    public void 조작방식은_직접기술과_전술카드를_서버에서_분리한다()
    {
        var state = new SimulationBattleInstanceState(Context());
        var tactical = state.ConfirmLocalControlMode(
            new SimulationLocalCombatControlModeConfirmRequest
            {
                CommandId = "command:mode:tactical",
                ExpectedBattleRevision = state.Snapshot().BattleRevision,
                RequestingActorStableId = "actor:player",
                ControlModeCode = SimulationLocalCombatCodes.TacticalCommand,
            });
        Assert.Equal(SimulationLocalCombatCodes.TacticalCommand,
            tactical.LocalCombat.ControlModeCode);
        Assert.Contains("FarmDefensiveReadiness",
            tactical.LocalCombat.ActiveCardModifierCodes);
        Assert.DoesNotContain("DirectSkillPower",
            tactical.LocalCombat.ActiveCardModifierCodes);

        Assert.Throws<SimulationConflictException>(() => state.ConfirmLocalAction(
            new SimulationLocalCombatActionConfirmRequest
            {
                CommandId = "command:tactical:skill-denied",
                ExpectedBattleRevision = tactical.BattleRevision,
                RequestingActorStableId = "actor:player",
                TargetActorStableId = "threat:encounter:local:00",
                ActionCode = SimulationLocalCombatCodes.RoleCardSkill,
            }));

        var ordered = state.ConfirmLocalAction(
            new SimulationLocalCombatActionConfirmRequest
            {
                CommandId = "command:tactical:basic",
                ExpectedBattleRevision = tactical.BattleRevision,
                RequestingActorStableId = "actor:player",
                TargetActorStableId = "threat:encounter:local:00",
                ActionCode = SimulationLocalCombatCodes.BasicAttack,
            });
        var action = Assert.Single(ordered.LocalCombat.Actions);
        Assert.Equal("TacticalBasicAttackOrdered", action.ResultCode);
        Assert.Equal(SimulationLocalCombatCodes.TacticalCommand,
            action.ControlModeCode);
        Assert.Contains("FarmDefensiveReadiness",
            action.AppliedCardModifierCodes);
        Assert.Equal(790, ordered.LocalCombat.Actors.Single(value =>
            value.ActorStableId == "threat:encounter:local:00").HealthPermille);

        var restored = SimulationBattleInstanceState.Restore(
            state.CreateSaveRecord()).Snapshot();
        Assert.Equal(SimulationLocalCombatCodes.TacticalCommand,
            restored.LocalCombat.ControlModeCode);
        Assert.Equal(ordered.ReplayHashSha256, restored.ReplayHashSha256);

        var directState = new SimulationBattleInstanceState(Context());
        var directSkill = directState.ConfirmLocalAction(
            new SimulationLocalCombatActionConfirmRequest
            {
                CommandId = "command:direct:skill",
                ExpectedBattleRevision = directState.Snapshot().BattleRevision,
                RequestingActorStableId = "actor:player",
                TargetActorStableId = "threat:encounter:local:00",
                ActionCode = SimulationLocalCombatCodes.RoleCardSkill,
            });
        var skillAction = Assert.Single(directSkill.LocalCombat.Actions);
        Assert.Equal(SimulationLocalCombatCodes.DirectAction,
            skillAction.ControlModeCode);
        Assert.Equal(new[] { "DirectSkillPower" },
            skillAction.AppliedCardModifierCodes);
        Assert.Equal(615, directSkill.LocalCombat.Actors.Single(value =>
            value.ActorStableId == "threat:encounter:local:00").HealthPermille);
    }

    private static SimulationBattleCreationContext Context() => new()
    {
        BattleStableId = "battle:local:one",
        SessionStableId = "session:one",
        EncounterStableId = "encounter:local",
        AreaStableId = "area:farm",
        CommanderActorStableId = "actor:player",
        StartedWorldTick = 7,
        StartedWorldRevision = 11,
        ScenarioSeed = 42,
        AlliedStrength = 2,
        HostileStrength = 2,
        CombatSpaceCode = SimulationLocalCombatCodes.WorldLocal,
        EncounterScaleCode = SimulationLocalCombatCodes.Instant,
        ScaleReasonCodes = new[] { "SmallHostileGroup" },
        LocalWorldContext = new SimulationLocalCombatWorldContextSnapshot
        {
            AreaSetInstanceStableId = "area:farm",
            ContextHashSha256 = new string('a', 64),
        },
        InitialResourceStableIds = Array.Empty<string>(),
        ReinforcementCandidateStableIds = Array.Empty<string>(),
        BattlefieldDerivation = new SimulationBattlefieldDerivationSnapshot(),
        UnitRoster = new SimulationBattleUnitRosterSnapshot
        {
            BattleUnitRosterHashSha256 = new string('b', 64),
            CardModifierHashSha256 = new string('c', 64),
            CombatSeedHashSha256 = new string('d', 64),
            CardModifiers = new[]
            {
                new SimulationBattleCardModifierSnapshot
                {
                    CardCopyStableId = "card:direct",
                    ActorStableId = "actor:player",
                    ApplicableControlModeCode = SimulationLocalCombatCodes.DirectAction,
                    ModifierCode = "DirectSkillPower",
                    BasisPoints = 1000,
                },
                new SimulationBattleCardModifierSnapshot
                {
                    CardCopyStableId = "card:tactical",
                    ActorStableId = "actor:player",
                    ApplicableControlModeCode = SimulationLocalCombatCodes.TacticalCommand,
                    ModifierCode = "FarmDefensiveReadiness",
                    BasisPoints = 1000,
                },
                new SimulationBattleCardModifierSnapshot
                {
                    CardCopyStableId = "card:tactical",
                    ActorStableId = "actor:player",
                    ApplicableControlModeCode = SimulationLocalCombatCodes.TacticalCommand,
                    ModifierCode = "FormationCohesion",
                    BasisPoints = -500,
                },
            },
            Units = new[]
            {
                new SimulationBattleUnitSnapshot
                {
                    UnitStableId = "unit:player",
                    SideCode = SimulationFarmTacticalCombatCodes.Allied,
                    MemberActorStableIds = new[] { "actor:player" },
                    MemberCount = 1,
                    RoleCodes = new[] { "Exploration" },
                },
                new SimulationBattleUnitSnapshot
                {
                    UnitStableId = "unit:hostile",
                    SideCode = SimulationFarmTacticalCombatCodes.Hostile,
                    ThreatTypeCode = "Wolf",
                    MemberCount = 2,
                    RoleCodes = new[] { "Threat" },
                },
            },
        },
        CreateCommandId = "command:create:local",
    };
}
