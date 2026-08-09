using System.Reflection;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class 도심마트공동주택주문자집단SimulationTests
{
    [Fact]
    public void 감자공동주택집단은_의향과확정수요를분리한다()
    {
        var group = Assert.Single(
            도심마트공동주택주문자집단SimulationFixture.Create().Groups);

        Assert.Equal("orderer-group:residential:potato:1", group.OrdererGroupStableId);
        Assert.Equal("demand-request:residential:potato:1", group.DemandRequestStableId);
        Assert.Equal(67, group.IntentParticipantCount);
        Assert.Equal(410m, group.IntentQuantity);
        Assert.Equal(61, group.ConfirmedParticipantCount);
        Assert.Equal(385m, group.ConfirmedQuantity);
        Assert.Equal("kg", group.QuantityUnitCode);
        Assert.Equal(
            도심마트주문자집단StateCodes.MemberConfirmationPending,
            group.StateCode);
    }

    [Fact]
    public void 대표는_사회적Context와CanonicalRole과NpcIdentity를분리한다()
    {
        var representative = Assert.Single(
            도심마트공동주택주문자집단SimulationFixture.Create().Groups)
            .Representative;

        Assert.Equal(
            도심마트대표SocialContextCodes.ResidentialCommunityRepresentative,
            representative.SocialContextCode);
        Assert.Equal("주민자치 대표", representative.DisplayLabel);
        Assert.Equal(
            도심마트대표CanonicalRoleCodes.GroupPurchaseRepresentative,
            representative.CanonicalRoleCode);
        Assert.Equal(
            도심마트대표RoleStateCodes.AssignedSimulatedCoordinator,
            representative.RoleStateCode);
        Assert.Equal(
            "npc:sim:residential-group-representative:1",
            representative.NpcStableId);
        Assert.Equal(
            "representative-visit:sim:potato:1",
            representative.RepresentativeVisitStableId);
    }

    [Fact]
    public void 공동수령지는_확정지점이아닌Candidate로만보존한다()
    {
        var group = Assert.Single(
            도심마트공동주택주문자집단SimulationFixture.Create().Groups);

        Assert.Equal(
            "pickup-point:residential:sample-1",
            group.RequestedPickupPointStableId);
        Assert.Equal(도심마트공동수령지StateCodes.Candidate, group.PickupPointStateCode);
        Assert.Equal(7, group.RequestedFulfillmentStartsAtTick);
        Assert.Equal(27, group.RequestedFulfillmentEndsAtTick);
    }

    [Fact]
    public void Fixture는_Simulation경계와명시적Lineage를보존한다()
    {
        var snapshot = 도심마트공동주택주문자집단SimulationFixture.Create();

        Assert.Equal(SimulationModeCodes.Simulation, snapshot.ModeCode);
        Assert.False(snapshot.IsOperationalState);
        var lineage = Assert.Single(snapshot.SourceLineage);
        Assert.Equal("fixture-definition:residential-potato-group:1", lineage.SourceStableId);
        Assert.Equal("residential-potato-group-fixture:1", lineage.SourceDataRevision);
        Assert.Equal("residential-group-fixture-rule:1", lineage.RuleRevision);
    }

    [Fact]
    public void 같은Fixture는_같은StableId와Data를만든다()
    {
        var first = 도심마트공동주택주문자집단SimulationFixture.Create();
        var second = 도심마트공동주택주문자집단SimulationFixture.Create();

        Assert.Equal(first.SnapshotStableId, second.SnapshotStableId);
        Assert.Equal(first.DataRevision, second.DataRevision);
        Assert.Equal(GroupKey(Assert.Single(first.Groups)), GroupKey(Assert.Single(second.Groups)));
    }

    [Fact]
    public void 확정참여자와수량은_의향집계를초과할수없다()
    {
        var snapshot = 도심마트공동주택주문자집단SimulationFixture.Create();
        snapshot.Groups[0].ConfirmedParticipantCount = 68;
        AssertError("OrdererGroupParticipantCountInvalid", () => Validate(snapshot));

        snapshot = 도심마트공동주택주문자집단SimulationFixture.Create();
        snapshot.Groups[0].ConfirmedQuantity = 411m;
        AssertError("OrdererGroupQuantityInvalid", () => Validate(snapshot));
    }

    [Fact]
    public void 대표의사회적Context는_CanonicalRole을대체할수없다()
    {
        var snapshot = 도심마트공동주택주문자집단SimulationFixture.Create();
        snapshot.Groups[0].Representative.CanonicalRoleCode = string.Empty;
        AssertError("OrdererGroupRepresentativeCanonicalRoleInvalid", () => Validate(snapshot));

        snapshot = 도심마트공동주택주문자집단SimulationFixture.Create();
        snapshot.Groups[0].Representative.SocialContextCode = "GroupPurchaseRepresentative";
        AssertError("OrdererGroupRepresentativeSocialContextInvalid", () => Validate(snapshot));
    }

    [Fact]
    public void 마트용집단계약에는_주민개인정보와개별결제필드가없다()
    {
        var properties = typeof(도심마트주문자집단수요SimulationData)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Concat(typeof(도심마트주문자집단대표SimulationData)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain(properties, name => name.Contains("User", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Contact", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Address", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("HouseholdUnit", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Payment", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("ResidentQuantity", StringComparison.OrdinalIgnoreCase));
    }

    private static void Validate(도심마트주문자집단수요SimulationDataSnapshot snapshot)
        => 도심마트공급경영SimulationDataValidator.Validate(snapshot);

    private static string GroupKey(도심마트주문자집단수요SimulationData group)
        => group.OrdererGroupStableId + "|" + group.ProductStableId
            + "|" + group.DemandRequestStableId
            + "|" + group.IntentParticipantCount + "|" + group.IntentQuantity
            + "|" + group.ConfirmedParticipantCount + "|" + group.ConfirmedQuantity
            + "|" + group.Representative.SocialContextCode
            + "|" + group.Representative.CanonicalRoleCode
            + "|" + group.Representative.NpcStableId;

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
