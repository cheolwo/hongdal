using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 도심마트주문자집단SimulationWorldGraphTests
{
    [Fact]
    public void 공동주택Fixture는_네종류Node와세관계를만든다()
    {
        var graph = Build();

        Assert.Equal(4, graph.NodesById.Count);
        Assert.Equal(3, graph.Relations.Length);
        Assert.Single(graph.NodesById.Values,
            node => node.Kind == 도심마트주문자집단WorldNodeKind.OrdererGroup);
        Assert.Single(graph.NodesById.Values,
            node => node.Kind == 도심마트주문자집단WorldNodeKind.DemandRequest);
        Assert.Single(graph.NodesById.Values,
            node => node.Kind == 도심마트주문자집단WorldNodeKind.Product);
        Assert.Single(graph.NodesById.Values,
            node => node.Kind == 도심마트주문자집단WorldNodeKind.PickupPoint);
        Assert.Single(graph.Relations,
            relation => relation.Kind == 도심마트주문자집단WorldRelationKind.RaisesDemand);
        Assert.Single(graph.Relations,
            relation => relation.Kind == 도심마트주문자집단WorldRelationKind.RequestsProduct);
        Assert.Single(graph.Relations,
            relation => relation.Kind == 도심마트주문자집단WorldRelationKind.RequestsFulfillmentAt);
    }

    [Fact]
    public void 주문자집단에서수요로_상품과수령후보로_양방향탐색한다()
    {
        var graph = Build();

        var raised = Assert.Single(graph.GetOutgoing("orderer-group:residential:potato:1"));
        Assert.Equal("demand-request:residential:potato:1", raised.ToStableId);
        var demandRelations = graph.GetOutgoing(raised.ToStableId);
        Assert.Contains(demandRelations,
            relation => relation.Kind == 도심마트주문자집단WorldRelationKind.RequestsProduct
                && relation.ToStableId == 도심마트감자공급SimulationFixture.ProductStableId);
        Assert.Contains(demandRelations,
            relation => relation.Kind == 도심마트주문자집단WorldRelationKind.RequestsFulfillmentAt
                && relation.ToStableId == "pickup-point:residential:sample-1");
        Assert.Single(graph.GetIncoming(도심마트감자공급SimulationFixture.ProductStableId));
    }

    [Fact]
    public void 같은Fixture는_같은Node와Relation순서를만든다()
    {
        var first = Build();
        var second = Build();

        Assert.Equal(first.NodesById.Keys, second.NodesById.Keys);
        Assert.Equal(
            first.Relations.Select(RelationKey),
            second.Relations.Select(RelationKey));
    }

    [Fact]
    public void 관계의Node종류가다르면_거부한다()
    {
        var group = Node("orderer-group:a", 도심마트주문자집단WorldNodeKind.OrdererGroup);
        var product = Node("product:potato", 도심마트주문자집단WorldNodeKind.Product);
        var relation = new 도심마트주문자집단WorldRelation(
            group.StableId,
            product.StableId,
            도심마트주문자집단WorldRelationKind.RaisesDemand);

        AssertError("OrdererGroupWorldRelationSemanticMismatch",
            () => new 도심마트주문자집단SimulationWorldGraph(
                new[] { group, product },
                new[] { relation }));
    }

    [Fact]
    public void 존재하지않는Node와중복Relation을_거부한다()
    {
        var group = Node("orderer-group:a", 도심마트주문자집단WorldNodeKind.OrdererGroup);
        var demand = Node("demand-request:a", 도심마트주문자집단WorldNodeKind.DemandRequest);
        var relation = new 도심마트주문자집단WorldRelation(
            group.StableId,
            demand.StableId,
            도심마트주문자집단WorldRelationKind.RaisesDemand);

        AssertError("OrdererGroupWorldRelationTargetUnknown",
            () => new 도심마트주문자집단SimulationWorldGraph(
                new[] { group },
                new[] { relation }));
        AssertError("OrdererGroupWorldRelationDuplicate",
            () => new 도심마트주문자집단SimulationWorldGraph(
                new[] { group, demand },
                new[] { relation, relation }));
    }

    [Fact]
    public void DemandRequestStableId가없으면_Graph생성전에거부한다()
    {
        var snapshot = 도심마트공동주택주문자집단SimulationFixture.Create();
        snapshot.Groups[0].DemandRequestStableId = string.Empty;

        AssertError("OrdererGroupDemandRequestStableIdInvalid",
            () => new 도심마트주문자집단SimulationWorldGraphBuilder().Build(snapshot));
    }

    [Fact]
    public void 공급Graph와주문자집단Graph는_ProductStableId로만결합한다()
    {
        var supply = new 도심마트공급SimulationWorldGraphBuilder()
            .Build(도심마트감자공급SimulationFixture.CreateSupplySnapshot());
        var demand = Build();
        var productStableId = 도심마트감자공급SimulationFixture.ProductStableId;

        Assert.True(supply.NodesById.ContainsKey(productStableId));
        Assert.True(demand.NodesById.ContainsKey(productStableId));
        Assert.Equal(10, supply.NodesById.Count);
        Assert.Equal(15, supply.Relations.Length);
        Assert.DoesNotContain(demand.NodesById.Keys,
            id => id == 도심마트공동주택주문자집단SimulationFixture.RepresentativeNpcStableId);
    }

    private static 도심마트주문자집단SimulationWorldGraph Build()
        => new 도심마트주문자집단SimulationWorldGraphBuilder()
            .Build(도심마트공동주택주문자집단SimulationFixture.Create());

    private static 도심마트주문자집단WorldNode Node(
        string stableId,
        도심마트주문자집단WorldNodeKind kind)
        => new(stableId, kind, stableId);

    private static string RelationKey(도심마트주문자집단WorldRelation relation)
        => relation.FromStableId + "|" + relation.Kind + "|" + relation.ToStableId;

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
