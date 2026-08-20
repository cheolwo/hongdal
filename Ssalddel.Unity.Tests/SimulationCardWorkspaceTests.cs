using Ssalddel.Unity.Cards;

namespace Ssalddel.Unity.Tests;

public sealed class SimulationCardWorkspaceTests
{
    [Fact]
    public async Task LoadAsync_MergesMeaningLayersWithoutGrantingAuthority()
    {
        var coordinator = new CardWorkspaceCoordinator(new ICardFamilySource[]
        {
            Source(CardFamilyCodes.Tarot, CardHierarchyTierCodes.Meta,
                CardAuthorityCodes.ServerMutable, "tarot:tower", "탑"),
            Source(CardFamilyCodes.TeamRole, CardHierarchyTierCodes.Action,
                CardAuthorityCodes.ServerMutable, "role:defend", "Farm Gate 방어"),
        });

        var result = await coordinator.LoadAsync();

        Assert.True(result.PresentationOnly);
        Assert.Equal(2, result.Items.Length);
        Assert.Contains(result.Items, value => value.HierarchyTierCode == "Meta");
        Assert.Contains(result.Items, value => value.HierarchyTierCode == "Action");
    }

    [Fact]
    public async Task LoadAsync_RejectsRelationThatChangesAvailability()
    {
        var source = Source(CardFamilyCodes.Tarot, CardHierarchyTierCodes.Meta,
            CardAuthorityCodes.ServerMutable, "tarot:tower", "탑",
            new CardWorkspaceRelation
            {
                SourceCardStableId = "tarot:tower",
                TargetCardStableId = "role:defend",
                RelationCode = CardContextRelationCodes.Recommended,
                ChangesAvailability = true,
            });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new CardWorkspaceCoordinator(new[] { source }).LoadAsync());

        Assert.Equal("CardWorkspaceFamilyInvalid", error.Message);
    }

    private static ICardFamilySource Source(string family, string tier,
        string authority, string stableId, string title,
        params CardWorkspaceRelation[] relations)
        => new FakeSource(new CardWorkspaceFamilySnapshot
        {
            FamilyCode = family,
            Items = new[]
            {
                new CardWorkspaceItem
                {
                    CardStableId = stableId,
                    Title = title,
                    FamilyCode = family,
                    HierarchyTierCode = tier,
                    AuthorityCode = authority,
                    IsAvailable = true,
                },
            },
            Relations = relations,
        });

    private sealed class FakeSource : ICardFamilySource
    {
        private readonly CardWorkspaceFamilySnapshot snapshot;
        public FakeSource(CardWorkspaceFamilySnapshot value) => snapshot = value;
        public string FamilyCode => snapshot.FamilyCode;
        public Task<CardWorkspaceFamilySnapshot> LoadAsync(
            CancellationToken cancellationToken) => Task.FromResult(snapshot);
    }
}
