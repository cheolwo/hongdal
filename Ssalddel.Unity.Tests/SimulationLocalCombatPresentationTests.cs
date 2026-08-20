using Ssalddel.Unity.Battles;

namespace Ssalddel.Unity.Tests;

public sealed class SimulationLocalCombatPresentationTests
{
    [Fact]
    public void 일인칭_우클릭은_평소_시점을_보존하고_예고중에만_회피한다()
    {
        var battle = LocalBattle(false, LocalCombatPresentationCodes.DirectAction);
        var look = LocalCombatInputCommandFactory.CreatePointerAction(battle,
            LocalCombatPresentationCodes.FirstPerson,
            LocalCombatPresentationCodes.RightPointer, false,
            "actor:player", "threat:1", "command:look", 0);
        Assert.Null(look);

        battle.LocalCombat.HostileTelegraphActive = true;
        var dodge = LocalCombatInputCommandFactory.CreatePointerAction(battle,
            LocalCombatPresentationCodes.FirstPerson,
            LocalCombatPresentationCodes.RightPointer, true,
            "actor:player", "threat:1", "command:dodge", 120);
        Assert.Equal(LocalCombatPresentationCodes.Dodge, dodge!.ActionCode);
        Assert.Equal(4, dodge.ExpectedBattleRevision);
    }

    [Fact]
    public void 삼인칭_우클릭은_접근이고_예고중에는_대형유지다()
    {
        var battle = LocalBattle(false, LocalCombatPresentationCodes.TacticalCommand);
        var approach = LocalCombatInputCommandFactory.CreatePointerAction(battle,
            LocalCombatPresentationCodes.TacticalThirdPerson,
            LocalCombatPresentationCodes.RightPointer, false,
            "actor:player", "threat:1", "command:approach", 0);
        Assert.Equal(LocalCombatPresentationCodes.Approach, approach!.ActionCode);

        var hold = LocalCombatInputCommandFactory.CreatePointerAction(battle,
            LocalCombatPresentationCodes.TacticalThirdPerson,
            LocalCombatPresentationCodes.RightPointer, true,
            "actor:player", "threat:1", "command:dodge", 180);
        Assert.Equal(LocalCombatPresentationCodes.HoldPosition, hold!.ActionCode);
    }

    [Fact]
    public void 행동슬롯은_일인칭기술과_삼인칭지휘를_분리한다()
    {
        var direct = LocalBattle(false, LocalCombatPresentationCodes.DirectAction);
        var guard = LocalCombatInputCommandFactory.CreateActionSlot(direct,
            LocalCombatPresentationCodes.FirstPerson, 2,
            "actor:player", "threat:1", "command:guard", 0);
        Assert.Equal(LocalCombatPresentationCodes.Guard, guard!.ActionCode);

        var tactical = LocalBattle(false,
            LocalCombatPresentationCodes.TacticalCommand);
        var hold = LocalCombatInputCommandFactory.CreateActionSlot(tactical,
            LocalCombatPresentationCodes.TacticalThirdPerson, 2,
            "actor:player", "threat:1", "command:hold", 0);
        var skill = LocalCombatInputCommandFactory.CreateActionSlot(tactical,
            LocalCombatPresentationCodes.TacticalThirdPerson, 4,
            "actor:player", "threat:1", "command:no-skill", 0);
        Assert.Equal(LocalCombatPresentationCodes.HoldPosition, hold!.ActionCode);
        Assert.Null(skill);
    }

    [Fact]
    public void 현장전투는_현재월드와_LH창을_유지한다()
    {
        var frame = new BattlePresentationMapper().Map(LocalBattle(false,
                LocalCombatPresentationCodes.TacticalCommand),
            "actor:player", BattlePresentationCodes.TacticalThirdPerson);
        Assert.True(frame.KeepsCurrentWorldVisible);
        Assert.False(frame.ShowBattleRoot);
        Assert.True(frame.PinsLhDetailWindow);
        Assert.True(frame.PinsLhActiveWindow);
        Assert.True(frame.FreezesWorldTick);
    }

    private static BattleInstanceApiModel LocalBattle(bool resolved,
        string controlModeCode) => new()
    {
        BattleStableId = "battle:local:1",
        AreaStableId = "area-set:farm",
        CombatSpaceCode = BattlePresentationCodes.WorldLocal,
        PhaseCode = resolved ? BattlePresentationCodes.Completed
            : BattlePresentationCodes.Active,
        BattleRevision = 4,
        CombatTick = 10,
        ReplayHashSha256 = new string('a', 64),
        SimulationOnly = true,
        Participants = new[]
        {
            new BattleParticipantApiModel
            {
                ActorStableId = "actor:player",
                ParticipationRoleCode = BattlePresentationCodes.Commander,
            },
        },
        LocalCombat = new LocalCombatStateApiModel
        {
            StateCode = LocalCombatPresentationCodes.Active,
            FocusedTargetStableId = "threat:1",
            ControlModeCode = controlModeCode,
            WorldContext = new LocalCombatWorldContextApiModel
            {
                PinsDetailWindow = true,
                PinsActiveWindow = true,
            },
            Actors = new[]
            {
                new LocalCombatActorApiModel
                {
                    ActorStableId = "actor:player",
                    StateCode = LocalCombatPresentationCodes.Active,
                },
            },
        },
    };
}
