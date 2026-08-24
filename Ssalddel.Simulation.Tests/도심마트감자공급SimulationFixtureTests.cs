using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 도심마트감자공급SimulationFixtureTests
{
    [Fact]
    public void 감자Fixture는_서로다른조건의_세공급처를제공한다()
    {
        var snapshot = 도심마트감자공급SimulationFixture.CreateSupplySnapshot();

        Assert.Equal(3, snapshot.Suppliers.Length);
        Assert.Equal(3, snapshot.Offers.Length);
        Assert.Equal(3, snapshot.ContractDrafts.Length);
        Assert.All(snapshot.Offers, offer =>
        {
            Assert.Equal(도심마트감자공급SimulationFixture.ProductStableId, offer.ProductStableId);
            Assert.Equal("kg", offer.QuantityUnitCode);
            Assert.Equal("KRW", offer.CurrencyCode);
        });

        var cooperative = Assert.Single(snapshot.Offers, value => value.SupplierStableId == "supplier:local-coop");
        var wholesaler = Assert.Single(snapshot.Offers, value => value.SupplierStableId == "supplier:national-wholesaler");
        var spot = Assert.Single(snapshot.Offers, value => value.SupplierStableId == "supplier:spot-market");
        Assert.True(wholesaler.UnitPrice < cooperative.UnitPrice);
        Assert.True(wholesaler.MinimumOrderQuantity > cooperative.MinimumOrderQuantity);
        Assert.True(cooperative.DeliveriesPerWeek > wholesaler.DeliveriesPerWeek);
        Assert.Equal(0, spot.LeadTimeTicks);
        Assert.Equal("Variable-Simulation", spot.QualityStandardCode);
    }

    [Fact]
    public void Fixture는_Simulation표시와출처계보를보존한다()
    {
        var snapshot = 도심마트감자공급SimulationFixture.CreateSupplySnapshot();

        Assert.Equal(SimulationModeCodes.Simulation, snapshot.ModeCode);
        Assert.False(snapshot.IsOperationalState);
        Assert.StartsWith("simulation-fixture:", snapshot.DataRevision, StringComparison.Ordinal);
        var lineage = Assert.Single(snapshot.SourceLineage);
        Assert.StartsWith("fixture:", lineage.SourceStableId, StringComparison.Ordinal);
        Assert.Equal("supply-fixture-rule:1", lineage.RuleRevision);
    }

    [Fact]
    public void 공급Graph는_상품공급처Offer계약안과_명시적관계를만든다()
    {
        var graph = new 도심마트공급SimulationWorldGraphBuilder()
            .Build(도심마트감자공급SimulationFixture.CreateSupplySnapshot());

        Assert.Equal(10, graph.NodesById.Count);
        Assert.Equal(15, graph.Relations.Length);
        Assert.Equal(3, graph.NodesById.Values.Count(value => value.Kind == 도심마트공급WorldNodeKind.Supplier));
        Assert.Equal(3, graph.Relations.Count(value => value.Kind == 도심마트공급WorldRelationKind.Provides));
        Assert.Equal(3, graph.Relations.Count(value => value.Kind == 도심마트공급WorldRelationKind.Targets));
        Assert.Equal(3, graph.Relations.Count(value => value.Kind == 도심마트공급WorldRelationKind.ProvidedBy));
        Assert.Equal(3, graph.Relations.Count(value => value.Kind == 도심마트공급WorldRelationKind.Covers));
        Assert.Equal(3, graph.Relations.Count(value => value.Kind == 도심마트공급WorldRelationKind.DerivedFrom));
    }

    [Fact]
    public void 공급처에서Offer로_계약안에서원천Offer로_양방향탐색할수있다()
    {
        var graph = new 도심마트공급SimulationWorldGraphBuilder()
            .Build(도심마트감자공급SimulationFixture.CreateSupplySnapshot());

        var suppliedOffer = Assert.Single(graph.GetOutgoing("supplier:local-coop"));
        Assert.Equal(도심마트공급WorldRelationKind.Provides, suppliedOffer.Kind);
        Assert.Equal("offer:local-coop:potato:1", suppliedOffer.ToStableId);

        var incoming = graph.GetIncoming("offer:local-coop:potato:1");
        Assert.Contains(incoming, value => value.Kind == 도심마트공급WorldRelationKind.Provides);
        Assert.Contains(incoming, value => value.Kind == 도심마트공급WorldRelationKind.DerivedFrom);
    }

    [Fact]
    public void 같은Fixture는_같은Node와Relation순서를만든다()
    {
        var builder = new 도심마트공급SimulationWorldGraphBuilder();
        var first = builder.Build(도심마트감자공급SimulationFixture.CreateSupplySnapshot());
        var second = builder.Build(도심마트감자공급SimulationFixture.CreateSupplySnapshot());

        Assert.Equal(first.NodesById.Keys, second.NodesById.Keys);
        Assert.Equal(
            first.Relations.Select(RelationKey),
            second.Relations.Select(RelationKey));
    }

    [Fact]
    public void TypedGraph는_관계의Node종류가맞지않으면_거부한다()
    {
        var supplier = Node("supplier:a", 도심마트공급WorldNodeKind.Supplier);
        var product = Node("product:potato", 도심마트공급WorldNodeKind.Product);
        var relation = new 도심마트공급WorldRelation(
            supplier.StableId,
            product.StableId,
            도심마트공급WorldRelationKind.Provides);

        AssertError("SupplyWorldRelationSemanticMismatch",
            () => new 도심마트공급SimulationWorldGraph(new[] { supplier, product }, new[] { relation }));
    }

    [Fact]
    public void TypedGraph는_존재하지않는Node와중복Relation을_거부한다()
    {
        var supplier = Node("supplier:a", 도심마트공급WorldNodeKind.Supplier);
        var offer = Node("offer:a", 도심마트공급WorldNodeKind.Offer);
        var relation = new 도심마트공급WorldRelation(
            supplier.StableId,
            offer.StableId,
            도심마트공급WorldRelationKind.Provides);

        AssertError("SupplyWorldRelationTargetUnknown",
            () => new 도심마트공급SimulationWorldGraph(new[] { supplier }, new[] { relation }));
        AssertError("SupplyWorldRelationDuplicate",
            () => new 도심마트공급SimulationWorldGraph(new[] { supplier, offer }, new[] { relation, relation }));
    }

    private static 도심마트공급WorldNode Node(string stableId, 도심마트공급WorldNodeKind kind)
        => new(stableId, kind, stableId);

    private static string RelationKey(도심마트공급WorldRelation relation)
        => relation.FromStableId + "|" + relation.Kind + "|" + relation.ToStableId;

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
