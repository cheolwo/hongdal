using Ssalddel.Unity.TeamRoles;

namespace Ssalddel.Unity.Tests;

public sealed class SimulationTeamRoleCardPresentationTests
{
    [Fact]
    public void 공동카드와현재활동역할만표현하고_고정직업을만들지않는다()
    {
        var result = new TeamRoleCardPresentationMapper().Map(State());

        Assert.True(result.CanRequestRemoteEquip);
        Assert.False(result.CalculatesRoleLocally);
        Assert.True(result.PresentationOnly);
        Assert.False(Assert.Single(result.MemberRoles).IsPermanentProfession);
        Assert.False(Assert.Single(result.Cards).IsPhysicalItem);
    }

    [Fact]
    public void 물리아이템이나고정직업으로내려온카드는거절한다()
    {
        var physical = State();
        physical.Cards[0].IsPhysicalItem = true;
        var profession = State();
        profession.MemberRoles[0].IsPermanentProfession = true;

        Assert.Throws<InvalidOperationException>(() =>
            new TeamRoleCardPresentationMapper().Map(physical));
        Assert.Throws<InvalidOperationException>(() =>
            new TeamRoleCardPresentationMapper().Map(profession));
    }

    [Fact]
    public async Task Coordinator는서버개정을따르고_낡은상태를거절한다()
    {
        var authority = new FakeAuthority(State());
        var coordinator = new TeamRoleCardClientCoordinator(authority,
            new TeamRoleCardPresentationMapper());
        var loaded = await coordinator.LoadAsync("session:sim:team-role",
            "actor:sim:explorer-1");
        authority.State = State();
        authority.State.Revision = 0;

        Assert.Equal(2, loaded.Revision);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            coordinator.EquipAsync("session:sim:team-role",
                new TeamRoleCardEquipApiRequest()));
    }

    private static TeamRoleCardStateApiModel State() => new()
    {
        SessionStableId = "session:sim:team-role",
        TeamStableId = "team:sim:survivors",
        Revision = 2,
        TeamPolicyRevision = 7,
        RuleRevision = "team-role-card-loadout.r1",
        MemberActorStableIds = ["actor:sim:explorer-1"],
        Cards =
        [
            new TeamRoleCardApiModel
            {
                CardCopyStableId = "team-card-copy:exploration-1",
                CardDefinitionStableId = "team-card:exploration",
                Title = "탐험 기술",
                ActivityRoleCodes = ["Exploration"],
                EquippedActorStableId = "actor:sim:explorer-1",
                SlotCode = "Primary",
            },
        ],
        ActiveActivities =
        [
            new TeamActivityAssignmentApiModel
            {
                ActivityStableId = "activity:explore:jinbu",
                ActorStableId = "actor:sim:explorer-1",
                CardCopyStableId = "team-card-copy:exploration-1",
                ActivityRoleCode = "Exploration",
                LocationStableId = "region:jinbu-myeon",
                StateCode = "Active",
            },
        ],
        MemberRoles =
        [
            new TeamMemberRoleApiModel
            {
                ActorStableId = "actor:sim:explorer-1",
                CurrentRoleCode = "Exploration",
                ActivityStableId = "activity:explore:jinbu",
                EquippedCardCopyStableIds =
                    ["team-card-copy:exploration-1"],
                IsPermanentProfession = false,
            },
        ],
        SupportsRemoteEquip = true,
        SimulationOnly = true,
        IsOperationalState = false,
    };

    private sealed class FakeAuthority(TeamRoleCardStateApiModel state)
        : ITeamRoleCardAuthorityClient
    {
        public TeamRoleCardStateApiModel State { get; set; } = state;

        public Task<TeamRoleCardStateApiModel> LoadAsync(string sessionStableId,
            string actorStableId, CancellationToken cancellationToken)
            => Task.FromResult(State);

        public Task<TeamRoleCardStateApiModel> EquipAsync(string sessionStableId,
            TeamRoleCardEquipApiRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(State);

        public Task<TeamRoleCardStateApiModel> StartActivityAsync(
            string sessionStableId, TeamActivityStartApiRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(State);

        public Task<TeamRoleCardStateApiModel> EndActivityAsync(
            string sessionStableId, TeamActivityEndApiRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(State);
    }
}
