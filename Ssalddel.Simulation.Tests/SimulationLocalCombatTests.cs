using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Application;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
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

    [Fact]
    public void 관찰운영은_카드묶음을_동결하고_전투중_방식을_바꾸지_않는다()
    {
        var state = new SimulationBattleInstanceState(ObserverContext());
        var initial = state.Snapshot();

        Assert.Throws<SimulationConflictException>(() => state.ConfirmLocalControlMode(
            new SimulationLocalCombatControlModeConfirmRequest
            {
                CommandId = "command:observer:wrong-hash",
                ExpectedBattleRevision = initial.BattleRevision,
                RequestingActorStableId = "actor:player",
                ControlModeCode = SimulationLocalCombatCodes.ObserverOperation,
                ExpectedCardLoadoutHashSha256 = new string('f', 64),
            }));

        var selected = state.ConfirmLocalControlMode(
            new SimulationLocalCombatControlModeConfirmRequest
            {
                CommandId = "command:observer:select",
                ExpectedBattleRevision = initial.BattleRevision,
                RequestingActorStableId = "actor:player",
                ControlModeCode = SimulationLocalCombatCodes.ObserverOperation,
                ExpectedCardLoadoutHashSha256 = initial.LocalCombat
                    .FrozenCardLoadoutHashSha256,
            });

        Assert.True(selected.LocalCombat.ParticipationModeLocked);
        Assert.Equal(SimulationLocalCombatCodes.ObserverOperation,
            selected.LocalCombat.ControlModeCode);
        Assert.Equal(initial.LocalCombat.FrozenCardLoadoutHashSha256,
            selected.LocalCombat.FrozenCardLoadoutHashSha256);
        Assert.Throws<SimulationConflictException>(() => state.ConfirmLocalControlMode(
            new SimulationLocalCombatControlModeConfirmRequest
            {
                CommandId = "command:observer:change",
                ExpectedBattleRevision = selected.BattleRevision,
                RequestingActorStableId = "actor:player",
                ControlModeCode = SimulationLocalCombatCodes.DirectAction,
            }));
    }

    [Fact]
    public void 관찰운영은_결정적_자동행동과_한번의_비상카드를_저장재생한다()
    {
        var state = new SimulationBattleInstanceState(ObserverContext());
        var selected = SelectObserver(state);
        var advanced = state.Advance(new SimulationBattleAdvanceRequest
        {
            CommandId = "command:observer:ticks:10",
            ExpectedBattleRevision = selected.BattleRevision,
            CombatTickCount = 10,
        }, 7, 0);
        Assert.True(advanced.LocalCombat.ObserverOperation.AutomaticActionCount > 0);
        Assert.True(advanced.LocalCombat.Actors.Single(value => value.SideCode ==
            SimulationLocalCombatCodes.Player).HealthPermille < 1000);

        var paused = state.ConfirmObserverIntervention(
            new SimulationLocalCombatObserverInterventionConfirmRequest
            {
                CommandId = "command:observer:pause",
                ExpectedBattleRevision = advanced.BattleRevision,
                RequestingActorStableId = "actor:player",
                ActionCode = SimulationLocalCombatCodes.PauseObserverIntervention,
            });
        Assert.True(paused.LocalCombat.ObserverOperation.TacticalPauseActive);
        Assert.Throws<SimulationConflictException>(() => state.Advance(
            new SimulationBattleAdvanceRequest
            {
                CommandId = "command:observer:paused-tick",
                ExpectedBattleRevision = paused.BattleRevision,
                CombatTickCount = 1,
            }, 7, 0));

        var recovered = state.ConfirmObserverIntervention(
            new SimulationLocalCombatObserverInterventionConfirmRequest
            {
                CommandId = "command:observer:recover",
                ExpectedBattleRevision = paused.BattleRevision,
                RequestingActorStableId = "actor:player",
                ActionCode = SimulationLocalCombatCodes.ActivateObserverCard,
                CardCopyStableId = "card:observer:emergency",
            });
        Assert.True(recovered.LocalCombat.ObserverOperation
            .InterventionOpportunityConsumed);
        Assert.False(recovered.LocalCombat.ObserverOperation.TacticalPauseActive);
        Assert.Equal(SimulationLocalCombatCodes.ObserverFieldRecovery,
            recovered.LocalCombat.ObserverOperation.ActivatedModifierCode);
        Assert.Throws<SimulationConflictException>(() =>
            state.ConfirmObserverIntervention(
                new SimulationLocalCombatObserverInterventionConfirmRequest
                {
                    CommandId = "command:observer:pause-again",
                    ExpectedBattleRevision = recovered.BattleRevision,
                    RequestingActorStableId = "actor:player",
                    ActionCode = SimulationLocalCombatCodes.PauseObserverIntervention,
                }));

        var restored = SimulationBattleInstanceState.Restore(
            state.CreateSaveRecord()).Snapshot();
        Assert.Equal(recovered.ReplayHashSha256, restored.ReplayHashSha256);
        Assert.True(restored.LocalCombat.ObserverOperation
            .InterventionOpportunityConsumed);
        Assert.Equal(recovered.LocalCombat.ObserverOperation.AutomaticActionCount,
            restored.LocalCombat.ObserverOperation.AutomaticActionCount);
    }

    [Fact]
    public void 직접개입_승리는_성과등급과_추가보상을_권위적으로_계산한다()
    {
        var context = Context();
        context.UnitRoster.Units[0].MemberActorStableIds =
            new[] { "actor:player", "npc:companion:1", "npc:companion:2",
                "npc:companion:3" };
        context.UnitRoster.Units[1].MemberCount = 1;
        var state = new SimulationBattleInstanceState(context);
        var revision = state.Snapshot().BattleRevision;
        for (var index = 0; index < 5
            && state.Snapshot().PhaseCode == SimulationBattleInstanceCodes.Active;
            index++)
        {
            var dodged = state.ConfirmLocalAction(
                new SimulationLocalCombatActionConfirmRequest
                {
                    CommandId = "command:direct:dodge:" + index,
                    ExpectedBattleRevision = revision,
                    RequestingActorStableId = "actor:player",
                    ActionCode = SimulationLocalCombatCodes.Dodge,
                    ReactionOffsetMs = 100,
                });
            revision = dodged.BattleRevision;
            var advanced = state.Advance(new SimulationBattleAdvanceRequest
            {
                CommandId = "command:direct:tick:" + index,
                ExpectedBattleRevision = revision,
                CombatTickCount = 5,
            }, 7, 0);
            revision = advanced.BattleRevision;
        }
        while (state.Snapshot().PhaseCode == SimulationBattleInstanceCodes.Active)
        {
            var current = state.Snapshot();
            var target = current.LocalCombat.Actors.First(value => value.SideCode ==
                SimulationLocalCombatCodes.Hostile
                && value.StateCode == SimulationLocalCombatCodes.Active);
            var attacked = state.ConfirmLocalAction(
                new SimulationLocalCombatActionConfirmRequest
                {
                    CommandId = "command:direct:finish:" + current.BattleRevision,
                    ExpectedBattleRevision = current.BattleRevision,
                    RequestingActorStableId = "actor:player",
                    TargetActorStableId = target.ActorStableId,
                    ActionCode = SimulationLocalCombatCodes.BasicAttack,
                });
            if (attacked.PhaseCode == SimulationBattleInstanceCodes.Completed) break;
            state.Advance(new SimulationBattleAdvanceRequest
            {
                CommandId = "command:direct:finish-tick:" + attacked.BattleRevision,
                ExpectedBattleRevision = attacked.BattleRevision,
                CombatTickCount = 10,
            }, 7, 0);
        }

        var completed = state.Snapshot();
        Assert.True(completed.LocalCombat.Performance.IsFinal);
        Assert.True(completed.LocalCombat.Performance.GradeCode ==
            SimulationLocalCombatCodes.GradeS,
            $"grade={completed.LocalCombat.Performance.GradeCode};health={completed.LocalCombat.Performance.HealthScore};defense={completed.LocalCombat.Performance.DefenseScore};speed={completed.LocalCombat.Performance.SpeedScore};ticks={completed.LocalCombat.Performance.ElapsedCombatTicks};total={completed.LocalCombat.Performance.TotalScore}");
        Assert.Equal(2, completed.LocalCombat.Performance.RewardBonusQuantity);
        Assert.NotNull(completed.Outcome);
        Assert.False(completed.Outcome!.UsedDeterministicAutoCommand);
    }

    [Fact]
    public void 관찰운영_세슬롯은_정의코드와_효과계보를_정확히_결속한다()
    {
        var cards = new SimulationTeamRoleCardStateSnapshot
        {
            Revision = 4,
            Cards = new[]
            {
                Card("card:tactic", SimulationLocalCombatCodes
                    .FocusedAssaultCardDefinition),
                Card("card:support", SimulationLocalCombatCodes
                    .CabinCoverCardDefinition),
                Card("card:emergency", SimulationLocalCombatCodes
                    .SafeRetreatCardDefinition),
            },
            CombatLoadouts = new[]
            {
                new SimulationCombatCardLoadoutSnapshot
                {
                    ActorStableId = "actor:player",
                    CombatControlModeCode =
                        SimulationTeamRoleCardCodes.ObserverOperation,
                    Slots = new[]
                    {
                        Slot(SimulationTeamRoleCardCodes.ObserverTactic,
                            "card:tactic"),
                        Slot(SimulationTeamRoleCardCodes.ObserverSupport,
                            "card:support"),
                        Slot(SimulationTeamRoleCardCodes.ObserverEmergency,
                            "card:emergency"),
                    },
                },
            },
        };
        var roster = SimulationBattleUnitRosterBuilder.Build("encounter:observer",
            Array.Empty<SimulationFarmActorSnapshot>(), 1, "Wolf", cards);

        Assert.Equal(new[]
        {
            SimulationLocalCombatCodes.ObserverCabinCover,
            SimulationLocalCombatCodes.ObserverFocusedAssault,
            SimulationLocalCombatCodes.ObserverSafeRetreat,
        }, roster.CardModifiers.Select(value => value.ModifierCode)
            .OrderBy(value => value, StringComparer.Ordinal).ToArray());
        Assert.All(roster.CardModifiers, value =>
            Assert.Equal(SimulationLocalCombatCodes.ObserverOperation,
                value.ApplicableControlModeCode));
        Assert.All(roster.CardModifiers, value => Assert.Equal(4,
            value.SourceCardRevision));

        cards.CombatLoadouts[0].Slots[0].SlotCode =
            SimulationTeamRoleCardCodes.ObserverSupport;
        Assert.Throws<SimulationContractException>(() =>
            SimulationBattleUnitRosterBuilder.Build("encounter:observer:invalid",
                Array.Empty<SimulationFarmActorSnapshot>(), 1, "Wolf", cards));
    }

    private static SimulationTeamRoleCardSnapshot Card(string copyId,
        string definitionId) => new()
        {
            CardCopyStableId = copyId,
            CardDefinitionStableId = definitionId,
            ActivityRoleCodes = new[] { SimulationTeamRoleCardCodes.Exploration },
        };

    private static SimulationCombatCardLoadoutSlotSnapshot Slot(string slotCode,
        string cardCopyId) => new()
        {
            SlotCode = slotCode,
            CardCopyStableId = cardCopyId,
        };

    private static SimulationBattleInstanceSnapshot SelectObserver(
        SimulationBattleInstanceState state)
    {
        var current = state.Snapshot();
        return state.ConfirmLocalControlMode(
            new SimulationLocalCombatControlModeConfirmRequest
            {
                CommandId = "command:observer:select",
                ExpectedBattleRevision = current.BattleRevision,
                RequestingActorStableId = "actor:player",
                ControlModeCode = SimulationLocalCombatCodes.ObserverOperation,
                ExpectedCardLoadoutHashSha256 = current.LocalCombat
                    .FrozenCardLoadoutHashSha256,
            });
    }

    private static SimulationBattleCreationContext ObserverContext()
    {
        var context = Context();
        context.ScaleReasonCodes = new[] { "SmallHostileGroup", "CabinDefenseApplied" };
        context.UnitRoster.CardModifierHashSha256 = new string('e', 64);
        context.UnitRoster.CardModifiers = new[]
        {
            ObserverModifier("card:observer:tactic",
                SimulationLocalCombatCodes.ObserverFocusedAssault),
            ObserverModifier("card:observer:support",
                SimulationLocalCombatCodes.ObserverCabinCover),
            ObserverModifier("card:observer:emergency",
                SimulationLocalCombatCodes.ObserverFieldRecovery),
        };
        return context;
    }

    private static SimulationBattleCardModifierSnapshot ObserverModifier(
        string cardCopyStableId, string modifierCode) => new()
        {
            CardCopyStableId = cardCopyStableId,
            ActorStableId = "actor:player",
            ApplicableControlModeCode = SimulationLocalCombatCodes.ObserverOperation,
            ModifierCode = modifierCode,
        };

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
