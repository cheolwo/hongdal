using Ssalddel.Interior.Contracts;
using Ssalddel.Interior.Domain;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "실내 계획의 결정성·제약·hash 회귀를 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3결정성검증,
    WorkOrderIds = new[] { "E9-WO-TOWN-HOUSE-INTERIOR-LAYOUT" },
    Boundary = "자동 시험은 실제 Scene 통행이나 Game View 증거를 대신하지 않는다.")]
public sealed class InteriorLayoutEngineTests
{
    [Fact]
    public void HubWarehouseV2_GeneratesFiveWorkZonesTwoAislesAndH1Ownership()
    {
        var plan = new DeterministicInteriorLayoutEngine().Generate(
            HubWarehouseInteriorGrammar.CreateRequest());

        Assert.Equal(InteriorLayoutCodes.SchemaVersionV2, plan.SchemaVersion);
        Assert.Equal(7, plan.Zones.Length);
        Assert.Equal(5, plan.Zones.Count(value =>
            !value.RoleCode.EndsWith("Aisle", StringComparison.Ordinal)));
        Assert.Empty(plan.UnresolvedRequiredFixtureCodes);
        Assert.True(plan.TraversalValidated);
        Assert.Equal(InteriorLayoutCodes.PlacementControlRevisionV2,
            plan.PlacementControlRuleRevision);
        Assert.Equal(64, plan.VisualMetricCatalogHashSha256.Length);
        Assert.Equal(64, plan.BaseInteriorPlacementPlanHashSha256.Length);
        Assert.Equal(64, plan.InteriorPlacementPlanHashSha256.Length);
        Assert.Contains(plan.Zones, value =>
            value.ZoneStableId == HubWarehouseInteriorGrammar.StorageZone
            && value.OwningH1StableId
            == HubWarehouseInteriorGrammar.ReceivingH1StableId);
        Assert.Contains(plan.Zones, value =>
            value.ZoneStableId == HubWarehouseInteriorGrammar.PickingZone
            && value.OwningH1StableId
            == HubWarehouseInteriorGrammar.OutboundH1StableId);
        Assert.All(plan.Placements, value =>
        {
            Assert.NotEmpty(value.OwningH1StableId);
            Assert.Contains(InteriorLayoutCodes.PlacementAccepted,
                value.ValidationCodes);
            if (!string.IsNullOrEmpty(value.VisualKey))
            {
                Assert.NotEmpty(value.VisualMetricStableId);
                Assert.Equal(64,
                    value.SourceAssetFingerprintSha256.Length);
                Assert.InRange(value.AppliedTransform.UniformScale,
                    0.85d, 1.15d);
            }
        });
    }

    [Fact]
    public void HubWarehouseV2_RejectsStaleAuthoringAdjustment()
    {
        var baselineRequest = HubWarehouseInteriorGrammar.CreateRequest(
            adjustmentRevision: "hub-warehouse-adjustment.r1");
        var baseline = new DeterministicInteriorLayoutEngine().Generate(
            baselineRequest);
        var target = baseline.Placements.First(value =>
            value.PlacementLayerCode == InteriorLayoutCodes.Fixture);
        var adjustment = new InteriorPlacementAdjustment
        {
            AdjustmentStableId = "adjustment:hub:test-stale",
            PlacementStableId = target.PlacementStableId,
            ExpectedBasePlanHashSha256 = new string('0', 64),
            ReasonCode = "AuthoringReview",
            UniformScale = 1d,
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new DeterministicInteriorLayoutEngine().Generate(
                HubWarehouseInteriorGrammar.CreateRequest(
                    adjustments: new[] { adjustment },
                    adjustmentRevision: "hub-warehouse-adjustment.r1")));

        Assert.Contains("StalePlacementOverride", exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TownLowRiseHouse_GeneratesFiveZonesAndPresentationOnlyPlacements()
    {
        var request = TownHouseFixture.Request();
        var plan = new DeterministicInteriorLayoutEngine().Generate(request);

        Assert.Equal(5, plan.Zones.Length);
        Assert.Empty(plan.UnresolvedRequiredFixtureCodes);
        Assert.True(plan.TraversalValidated);
        Assert.Contains(plan.Placements, item => item.PlacementLayerCode == InteriorLayoutCodes.Fixture);
        Assert.Contains(plan.Placements, item => item.PlacementLayerCode == InteriorLayoutCodes.LooseItem);
        Assert.All(plan.Placements, item => Assert.Contains(
            InteriorLayoutCodes.PresentationOnly,
            item.PresentationFlags));
        Assert.All(plan.Placements.Where(item => item.PlacementLayerCode == InteriorLayoutCodes.LooseItem),
            item => Assert.DoesNotContain("price", item.VisualKey, StringComparison.OrdinalIgnoreCase));

        var handle = new InteriorPlacementPlanCatalog().Pin(plan);
        Assert.Equal(plan.ReferenceCatalogHashSha256, handle.ReferenceCatalogHashSha256);
        Assert.Equal(plan.InteriorPlacementPlanHashSha256, handle.InteriorPlacementPlanHashSha256);
    }

    [Fact]
    public void InputOrderDoesNotChangePlanOrHash()
    {
        var left = TownHouseFixture.Request(reverseOrder: false);
        var right = TownHouseFixture.Request(reverseOrder: true);
        var engine = new DeterministicInteriorLayoutEngine();

        var first = engine.Generate(left);
        var second = engine.Generate(right);

        Assert.Equal(first.InteriorPlacementPlanHashSha256, second.InteriorPlacementPlanHashSha256);
        Assert.Equal(
            first.Placements.Select(value => value.PlacementStableId),
            second.Placements.Select(value => value.PlacementStableId));
    }

    [Fact]
    public void CatalogHashMismatchIsRejectedBeforeGeneration()
    {
        var request = TownHouseFixture.Request();
        request.ReferenceCatalog.CatalogHashSha256 = new string('0', 64);

        var exception = Assert.Throws<ArgumentException>(() =>
            new DeterministicInteriorLayoutEngine().Generate(request));

        Assert.Contains("Catalog hash", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TownReferenceGrammarCanGenerateWithoutMarketplaceReferences()
    {
        var catalog = new ApprovedInteriorReferenceCatalog
        {
            StableId = "catalog:town-empty",
            Revision = "catalog.empty.r1",
        };
        catalog.CatalogHashSha256 = InteriorLayoutHash.ComputeCatalogHash(catalog);

        var plan = new DeterministicInteriorLayoutEngine().Generate(
            new InteriorLayoutGenerationRequest
            {
                WorldSeed = "town-reference-grammar-fixture",
                BuildingPlacementStableId = "town-building:reference-house:01",
                GeneratorRevision = TownLowRiseHouseInteriorGrammar.GeneratorRevision,
                Definition = TownLowRiseHouseInteriorGrammar.CreateDefinition(
                    "H1-TOWN-RESIDENTIAL-LIFE-REFERENCE"),
                FixtureArchetypes = TownLowRiseHouseInteriorGrammar.CreateFixtureArchetypes(),
                LooseItemArchetypes = TownLowRiseHouseInteriorGrammar.CreateLooseItemArchetypes(),
                ReferenceCatalog = catalog,
            });

        Assert.Equal(5, plan.Zones.Length);
        Assert.Empty(plan.UnresolvedRequiredFixtureCodes);
        Assert.True(plan.TraversalValidated);
        Assert.All(plan.Placements.Where(value => value.PlacementLayerCode == InteriorLayoutCodes.LooseItem),
            value => Assert.Empty(value.ReferenceStableId));
    }
}

internal static class TownHouseFixture
{
    public static InteriorLayoutGenerationRequest Request(bool reverseOrder = false)
    {
        var zones = new[]
        {
            Zone("town-house:living", InteriorLayoutCodes.Living, -8, "Sofa", "Book"),
            Zone("town-house:kitchen", InteriorLayoutCodes.Kitchen, -4, "Counter", "Kitchenware"),
            Zone("town-house:bedroom", InteriorLayoutCodes.Bedroom, 0, "Bed", "Book"),
            Zone("town-house:bathroom", InteriorLayoutCodes.Bathroom, 4, "Sink", "BathroomItem"),
            Zone("town-house:work", InteriorLayoutCodes.Work, 8, "Desk", "Tool"),
        };
        var fixtures = new[]
        {
            Fixture("fixture:sofa", "Sofa", "Residential.Sofa.Small", 1.5, 0.8, InteriorLayoutCodes.Living),
            Fixture("fixture:counter", "Counter", "Residential.Counter.Small", 1.5, 0.7, InteriorLayoutCodes.Kitchen,
                Slot("counter-top", "Kitchenware", "CounterItem")),
            Fixture("fixture:bed", "Bed", "Residential.Bed.Single", 2, 1, InteriorLayoutCodes.Bedroom),
            Fixture("fixture:sink", "Sink", "Residential.Sink.Small", 1, 0.6, InteriorLayoutCodes.Bathroom),
            Fixture("fixture:desk", "Desk", "Residential.Table.Work.Small", 1.4, 0.7, InteriorLayoutCodes.Work,
                Slot("desk-top", "Tool", "DeskItem")),
        };
        var looseItems = new[]
        {
            Loose("loose:cup", "Kitchenware", "CounterItem", "Residential.Kitchen.Cup"),
            Loose("loose:tool", "Tool", "DeskItem", "Residential.Work.Tool.Small"),
        };
        var references = new[]
        {
            Reference("ref:cup", "Kitchenware", InteriorLayoutCodes.Kitchen, "CounterItem"),
            Reference("ref:tool", "Tool", InteriorLayoutCodes.Work, "DeskItem"),
        };
        var catalog = new ApprovedInteriorReferenceCatalog
        {
            StableId = "catalog:town-house",
            Revision = "approved-reference-catalog.r1",
            Items = reverseOrder ? references.Reverse().ToArray() : references,
        };
        catalog.CatalogHashSha256 = InteriorLayoutHash.ComputeCatalogHash(catalog);
        return new InteriorLayoutGenerationRequest
        {
            WorldSeed = "town-fixture-world-01",
            BuildingPlacementStableId = "town-building:low-rise-house:01",
            GeneratorRevision = "interior-layout-engine.r1",
            Definition = new InteriorDefinition
            {
                StableId = "interior:town-low-rise-house",
                Revision = "town-low-rise-house.r1",
                H1StableId = "H1-TOWN-RESIDENTIAL-LIFE-01",
                Structure = new InteriorStructureDefinition
                {
                    StableId = "structure:town-low-rise-house",
                    UsableBounds = Bounds(0, 0, 20, 8),
                    TraversalAnchors = [Point(0, 3)],
                },
                Zones = reverseOrder ? zones.Reverse().ToArray() : zones,
                Constraints = new InteriorConstraintProfile
                {
                    GridStepMeters = 0.5,
                    ObjectClearanceMeters = 0.1,
                    TraversalClearanceMeters = 0.25,
                },
            },
            ReferenceCatalog = catalog,
            FixtureArchetypes = reverseOrder ? fixtures.Reverse().ToArray() : fixtures,
            LooseItemArchetypes = reverseOrder ? looseItems.Reverse().ToArray() : looseItems,
        };
    }

    private static InteriorZoneDefinition Zone(
        string id, string role, double x, string fixture, string looseCategory)
        => new()
        {
            StableId = id,
            RoleCode = role,
            Bounds = Bounds(x, 0, 4, 8),
            RequiredFixtureRoleCodes = [fixture],
            AllowedFixtureRoleCodes = [fixture],
            AllowedLooseItemCategoryCodes = [looseCategory],
            TraversalAnchors = [Point(x, 3)],
        };

    private static InteriorFixtureArchetype Fixture(
        string id,
        string role,
        string visualKey,
        double x,
        double z,
        string zone,
        InteriorSurfaceDefinition? surface = null)
        => new()
        {
            StableId = id,
            FixtureRoleCode = role,
            VisualKey = visualKey,
            Size = new InteriorSize3 { X = x, Y = 1, Z = z },
            AllowedZoneRoleCodes = [zone],
            Surfaces = surface is null ? [] : [surface],
        };

    private static InteriorSurfaceDefinition Slot(string id, string category, string placementRole)
        => new()
        {
            StableId = id,
            LocalPosition = new InteriorVector3 { Y = 1 },
            Slots =
            [
                new InteriorSurfaceSlotDefinition
                {
                    StableId = id + ":slot-01",
                    AllowedCategoryCodes = [category],
                    AllowedPlacementRoleCodes = [placementRole],
                    MaximumSize = new InteriorSize3 { X = 0.5, Y = 0.5, Z = 0.5 },
                },
            ],
        };

    private static InteriorLooseItemArchetype Loose(
        string id, string category, string role, string visualKey)
        => new()
        {
            StableId = id,
            CategoryCode = category,
            PlacementRoleCode = role,
            VisualKey = visualKey,
            Size = new InteriorSize3 { X = 0.2, Y = 0.2, Z = 0.2 },
        };

    private static ApprovedInteriorReference Reference(string id, string category, string room, string role)
        => new()
        {
            ReferenceStableId = id,
            MarketplaceCode = "Fixture",
            CategoryCode = category,
            RoomRoleCodes = [room],
            PlacementRoleCodes = [role],
            ApprovedOriginalTitle = id,
            SourceUrl = "https://example.invalid/" + id,
            ObservedAtUtc = "2026-08-24T00:00:00.0000000+00:00",
            RawObservationHashSha256 = new string('a', 64),
            SourceRevision = "fixture.r1",
        };

    private static InteriorBounds Bounds(double x, double z, double sizeX, double sizeZ)
        => new()
        {
            Center = Point(x, z),
            Size = new InteriorSize3 { X = sizeX, Y = 3, Z = sizeZ },
        };

    private static InteriorVector3 Point(double x, double z)
        => new() { X = x, Z = z };
}
