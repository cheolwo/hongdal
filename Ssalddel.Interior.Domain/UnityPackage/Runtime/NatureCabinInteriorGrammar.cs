using System;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Interior.Domain
{
    /// <summary>
    /// Nature 오두막의 고정 Fixture만 정의한다. 보관량·건설 상태에 따라
    /// 달라지는 통나무와 공구는 세계 변화 Overlay가 별도로 배치한다.
    /// </summary>
    public static class NatureCabinInteriorGrammar
    {
        public const string DefinitionRevision =
            "nature-cabin-interior.r1";
        public const string GeneratorRevision =
            "interior-layout-engine.r1";
        public const string RestZone = "nature-cabin:zone:rest";
        public const string StorageZone = "nature-cabin:zone:storage";
        public const string WorkZone = "nature-cabin:zone:work";

        public static InteriorLayoutGenerationRequest CreateRequest(
            string worldSeed,
            string buildingPlacementStableId,
            string h1StableId)
        {
            if (string.IsNullOrWhiteSpace(worldSeed)
                || string.IsNullOrWhiteSpace(buildingPlacementStableId)
                || string.IsNullOrWhiteSpace(h1StableId))
                throw new ArgumentException(
                    "NatureCabinInteriorIdentityMissing");
            var references = new ApprovedInteriorReferenceCatalog
            {
                StableId = "catalog:nature-cabin:no-marketplace",
                Revision = "nature-cabin-reference.empty.r1",
            };
            references.CatalogHashSha256 =
                InteriorLayoutHash.ComputeCatalogHash(references);
            return new InteriorLayoutGenerationRequest
            {
                SchemaVersion = InteriorLayoutCodes.SchemaVersionV1,
                WorldSeed = worldSeed.Trim(),
                BuildingPlacementStableId =
                    buildingPlacementStableId.Trim(),
                GeneratorRevision = GeneratorRevision,
                Definition = CreateDefinition(h1StableId.Trim()),
                ReferenceCatalog = references,
                FixtureArchetypes = CreateFixtureArchetypes(),
                LooseItemArchetypes =
                    Array.Empty<InteriorLooseItemArchetype>(),
            };
        }

        public static InteriorDefinition CreateDefinition(string h1StableId)
            => new InteriorDefinition
            {
                StableId = "interior-definition:nature-cabin",
                Revision = DefinitionRevision,
                H1StableId = h1StableId,
                Structure = new InteriorStructureDefinition
                {
                    StableId = "interior-structure:nature-cabin:6x5",
                    UsableBounds = Bounds(0d, 0d, 6d, 5d),
                    TraversalAnchors = new[] { Point(0d, 2d) },
                },
                Zones = new[]
                {
                    Zone(RestZone, "NatureRestArea", -2d,
                        "Bedroll"),
                    Zone(StorageZone, "NatureStorageArea", 0d,
                        "StorageRack"),
                    Zone(WorkZone, "NatureWorkArea", 2d,
                        "WorkSurface"),
                },
                Constraints = new InteriorConstraintProfile
                {
                    GridStepMeters = .25d,
                    ObjectClearanceMeters = .1d,
                    TraversalClearanceMeters = .35d,
                },
            };

        public static InteriorFixtureArchetype[] CreateFixtureArchetypes()
            => new[]
            {
                Fixture("bedroll", "Bedroll", "Nature.Shelter.Bedroll",
                    1.6d, .8d, "NatureRestArea"),
                Fixture("storage-rack", "StorageRack",
                    "Nature.Storage.Rack.Small", 1.4d, .6d,
                    "NatureStorageArea", Surface("storage-surface",
                        "StoredMaterialOverlay")),
                Fixture("work-surface", "WorkSurface",
                    "Nature.Work.Table.Small", 1.4d, .7d,
                    "NatureWorkArea", Surface("work-surface",
                        "WorkToolOverlay")),
            };

        private static InteriorZoneDefinition Zone(
            string stableId,
            string roleCode,
            double x,
            string fixtureRoleCode)
            => new InteriorZoneDefinition
            {
                StableId = stableId,
                RoleCode = roleCode,
                Bounds = Bounds(x, 0d, 2d, 5d),
                RequiredFixtureRoleCodes = new[] { fixtureRoleCode },
                AllowedFixtureRoleCodes = new[] { fixtureRoleCode },
                TraversalAnchors = new[] { Point(x, 2d) },
            };

        private static InteriorFixtureArchetype Fixture(
            string suffix,
            string roleCode,
            string visualKey,
            double sizeX,
            double sizeZ,
            string zoneRoleCode,
            params InteriorSurfaceDefinition[] surfaces)
            => new InteriorFixtureArchetype
            {
                StableId = "nature-cabin:fixture:" + suffix,
                FixtureRoleCode = roleCode,
                VisualKey = visualKey,
                Size = new InteriorSize3
                {
                    X = sizeX,
                    Y = 1d,
                    Z = sizeZ,
                },
                AllowedZoneRoleCodes = new[] { zoneRoleCode },
                Surfaces = surfaces,
            };

        private static InteriorSurfaceDefinition Surface(
            string suffix,
            string placementRoleCode)
            => new InteriorSurfaceDefinition
            {
                StableId = "nature-cabin:surface:" + suffix,
                LocalPosition = new InteriorVector3 { Y = 1d },
                Slots = new[]
                {
                    new InteriorSurfaceSlotDefinition
                    {
                        StableId = "nature-cabin:slot:" + suffix,
                        AllowedPlacementRoleCodes =
                            new[] { placementRoleCode },
                        MaximumSize = new InteriorSize3
                        {
                            X = .8d,
                            Y = .8d,
                            Z = .6d,
                        },
                        DetailLevelCode = InteriorLayoutCodes.ObjectFocus,
                    },
                },
            };

        private static InteriorBounds Bounds(
            double x, double z, double sizeX, double sizeZ)
            => new InteriorBounds
            {
                Center = Point(x, z),
                Size = new InteriorSize3
                {
                    X = sizeX,
                    Y = 2.5d,
                    Z = sizeZ,
                },
            };

        private static InteriorVector3 Point(double x, double z)
            => new InteriorVector3 { X = x, Z = z };
    }
}
