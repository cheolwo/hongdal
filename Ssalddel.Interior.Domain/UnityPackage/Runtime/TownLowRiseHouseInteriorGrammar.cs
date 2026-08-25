using System;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Interior.Domain
{
    /// <summary>
    /// 실제 Town 건물 치수를 확정하기 전 코드와 조립 경계를 검증하는 첫 승인 후보 문법이다.
    /// Scene 좌표나 Synty Prefab 경로를 포함하지 않는다.
    /// </summary>
    public static class TownLowRiseHouseInteriorGrammar
    {
        public const string DefinitionRevision = "town-low-rise-house-interior.r1";
        public const string GeneratorRevision = "interior-layout-engine.r1";

        public static InteriorDefinition CreateDefinition(string h1StableId)
        {
            if (string.IsNullOrWhiteSpace(h1StableId))
                throw new ArgumentException("Town 주거 H1 StableId가 필요합니다.", nameof(h1StableId));
            return new InteriorDefinition
            {
                StableId = "interior-definition:town-low-rise-house",
                Revision = DefinitionRevision,
                H1StableId = h1StableId.Trim(),
                Structure = new InteriorStructureDefinition
                {
                    StableId = "interior-structure:town-low-rise-house",
                    UsableBounds = Bounds(0, 0, 20, 8),
                    TraversalAnchors = new[] { Point(0, 3) },
                },
                Zones = new[]
                {
                    Zone("living", InteriorLayoutCodes.Living, -8, "Sofa", "Lighting", "Book"),
                    Zone("kitchen", InteriorLayoutCodes.Kitchen, -4, "Counter", "Kitchenware"),
                    Zone("bedroom", InteriorLayoutCodes.Bedroom, 0, "Bed", "Lighting", "Book"),
                    Zone("bathroom", InteriorLayoutCodes.Bathroom, 4, "Sink", "BathroomItem"),
                    Zone("work", InteriorLayoutCodes.Work, 8, "Desk", "Tool", "Book"),
                },
                Constraints = new InteriorConstraintProfile
                {
                    GridStepMeters = 0.5d,
                    ObjectClearanceMeters = 0.1d,
                    TraversalClearanceMeters = 0.25d,
                },
            };
        }

        public static InteriorFixtureArchetype[] CreateFixtureArchetypes()
            => new[]
            {
                Fixture("sofa-small", "Sofa", "Residential.Sofa.Small", 1.8, 0.9, InteriorLayoutCodes.Living),
                Fixture("counter-small", "Counter", "Residential.Counter.Small", 1.6, 0.7,
                    InteriorLayoutCodes.Kitchen, Surface("counter-top", "CounterItem", "Kitchenware")),
                Fixture("bed-single", "Bed", "Residential.Bed.Single", 2, 1,
                    InteriorLayoutCodes.Bedroom, Surface("bedside-top", "BedsideLighting", "Lighting")),
                Fixture("bathroom-sink", "Sink", "Residential.Bathroom.Sink.Small", 1, 0.6,
                    InteriorLayoutCodes.Bathroom, Surface("sink-top", "BathroomTopItem", "BathroomItem")),
                Fixture("work-desk-small", "Desk", "Residential.Table.Work.Small", 1.4, 0.7,
                    InteriorLayoutCodes.Work, Surface("desk-top", "DeskItem", "Tool", "Book")),
            };

        public static InteriorLooseItemArchetype[] CreateLooseItemArchetypes()
            => new[]
            {
                Loose("cup-small", "Kitchenware", "CounterItem", "Residential.Kitchen.Cup.Small", 0.15),
                Loose("lamp-table-small", "Lighting", "BedsideLighting", "Residential.Light.Table.Small", 0.3),
                Loose("bathroom-storage-small", "BathroomItem", "BathroomTopItem", "Residential.Bathroom.Storage.Small", 0.25),
                Loose("hand-tool-small", "Tool", "DeskItem", "Residential.Work.Tool.Small", 0.3),
                Loose("book-small", "Book", "DeskItem", "Residential.Book.Small", 0.25),
            };

        private static InteriorZoneDefinition Zone(
            string suffix,
            string role,
            double x,
            string fixtureRole,
            params string[] looseCategories)
            => new()
            {
                StableId = "town-low-rise-house:zone:" + suffix,
                RoleCode = role,
                Bounds = Bounds(x, 0, 4, 8),
                RequiredFixtureRoleCodes = new[] { fixtureRole },
                AllowedFixtureRoleCodes = new[] { fixtureRole },
                AllowedLooseItemCategoryCodes = looseCategories,
                TraversalAnchors = new[] { Point(x, 3) },
            };

        private static InteriorFixtureArchetype Fixture(
            string suffix,
            string role,
            string visualKey,
            double sizeX,
            double sizeZ,
            string zoneRole,
            params InteriorSurfaceDefinition[] surfaces)
            => new()
            {
                StableId = "town-low-rise-house:fixture:" + suffix,
                FixtureRoleCode = role,
                VisualKey = visualKey,
                Size = new InteriorSize3 { X = sizeX, Y = 1, Z = sizeZ },
                AllowedZoneRoleCodes = new[] { zoneRole },
                Surfaces = surfaces,
            };

        private static InteriorSurfaceDefinition Surface(
            string suffix,
            string placementRole,
            params string[] categories)
            => new()
            {
                StableId = suffix,
                LocalPosition = new InteriorVector3 { Y = 1 },
                Slots = new[]
                {
                    new InteriorSurfaceSlotDefinition
                    {
                        StableId = suffix + ":slot-01",
                        AllowedPlacementRoleCodes = new[] { placementRole },
                        AllowedCategoryCodes = categories,
                        MaximumSize = new InteriorSize3 { X = 0.5, Y = 0.6, Z = 0.5 },
                        DetailLevelCode = InteriorLayoutCodes.ObjectFocus,
                    },
                },
            };

        private static InteriorLooseItemArchetype Loose(
            string suffix,
            string category,
            string placementRole,
            string visualKey,
            double size)
            => new()
            {
                StableId = "town-low-rise-house:loose:" + suffix,
                CategoryCode = category,
                PlacementRoleCode = placementRole,
                VisualKey = visualKey,
                Size = new InteriorSize3 { X = size, Y = size, Z = size },
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
}
