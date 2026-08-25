using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Interior.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "Structure·Zone·Fixture·Surface·Slot과 승인 Reference를 결정적 실내 배치 계획으로 조립한다.",
        StepKey = "domain.interior-layout-generate",
        DependsOnStepKeys = new[] { "contract.interior-layout-plan" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 19,
        Boundary = "같은 입력은 같은 hash를 만들며 WorldTick·재고·가격·소유권을 변경하지 않는다.")]
    public sealed class DeterministicInteriorLayoutEngine : I실내공간조립Engine
    {
        public InteriorPlacementPlan Generate(InteriorLayoutGenerationRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.Equals(
                    request.SchemaVersion,
                    InteriorLayoutCodes.SchemaVersionV2,
                    StringComparison.Ordinal))
                return DeterministicInteriorLayoutV2Engine.Generate(request);
            Validate(request);
            var definition = request.Definition;
            var seedFingerprint = Hash(string.Join("|", new[]
            {
                request.WorldSeed.Trim(),
                request.BuildingPlacementStableId.Trim(),
                definition.Revision.Trim(),
                request.ReferenceCatalog.Revision.Trim(),
                request.GeneratorRevision.Trim(),
            }));
            var placements = new List<InteriorPlacement>();
            var unresolved = new List<string>();
            var zones = definition.Zones
                .OrderBy(value => value.StableId, StringComparer.Ordinal)
                .ToArray();

            foreach (var zone in zones)
            {
                foreach (var fixtureRole in zone.RequiredFixtureRoleCodes
                             .Where(value => !string.IsNullOrWhiteSpace(value))
                             .Distinct(StringComparer.Ordinal)
                             .OrderBy(value => value, StringComparer.Ordinal))
                {
                    if (!TryPlaceFixture(
                            request,
                            zone,
                            fixtureRole,
                            seedFingerprint,
                            placements,
                            out var fixture))
                    {
                        unresolved.Add(zone.StableId + ":" + fixtureRole);
                        continue;
                    }

                    placements.Add(fixture);
                    AddSurfaceAndLooseItemPlacements(
                        request,
                        zone,
                        fixture,
                        seedFingerprint,
                        placements);
                }
            }

            var traversalValidated = HasTraversalRoutes(definition, placements);
            var plan = new InteriorPlacementPlan
            {
                BuildingPlacementStableId = request.BuildingPlacementStableId.Trim(),
                H1StableId = definition.H1StableId.Trim(),
                InteriorDefinitionRevision = definition.Revision.Trim(),
                ReferenceCatalogRevision = request.ReferenceCatalog.Revision.Trim(),
                ReferenceCatalogHashSha256 = InteriorLayoutHash.ComputeCatalogHash(request.ReferenceCatalog),
                GeneratorRevision = request.GeneratorRevision.Trim(),
                SeedFingerprintSha256 = seedFingerprint,
                Zones = zones.Select(value => new InteriorZonePlan
                {
                    ZoneStableId = value.StableId.Trim(),
                    RoleCode = value.RoleCode.Trim(),
                    Bounds = Clone(value.Bounds),
                }).ToArray(),
                Placements = placements
                    .OrderBy(value => value.PlacementStableId, StringComparer.Ordinal)
                    .ToArray(),
                UnresolvedRequiredFixtureCodes = unresolved
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                TraversalValidated = traversalValidated,
            };
            plan.InteriorPlacementPlanHashSha256 = InteriorLayoutHash.ComputePlanHash(plan);
            return plan;
        }

        private static bool TryPlaceFixture(
            InteriorLayoutGenerationRequest request,
            InteriorZoneDefinition zone,
            string fixtureRole,
            string seedFingerprint,
            IReadOnlyCollection<InteriorPlacement> existing,
            out InteriorPlacement fixture)
        {
            var archetypes = request.FixtureArchetypes
                .Where(value => string.Equals(value.FixtureRoleCode, fixtureRole, StringComparison.Ordinal))
                .Where(value => value.AllowedZoneRoleCodes.Length == 0
                                || value.AllowedZoneRoleCodes.Contains(zone.RoleCode, StringComparer.Ordinal))
                .OrderBy(value => value.StableId, StringComparer.Ordinal)
                .ToArray();
            foreach (var archetype in Rotate(archetypes, seedFingerprint + "|" + zone.StableId + "|" + fixtureRole))
            {
                foreach (var candidate in CandidatePositions(zone.Bounds, archetype.Size, request.Definition.Constraints))
                {
                    var candidateBounds = Bounds(candidate.Position, archetype.Size, candidate.RotationDegrees);
                    if (!Contains(zone.Bounds, candidateBounds)
                        || request.Definition.Structure.ExclusionBounds.Any(value => Overlaps(value, candidateBounds)))
                        continue;
                    if (existing.Where(value => value.PlacementLayerCode == InteriorLayoutCodes.Fixture)
                        .Any(value => Overlaps(
                            Bounds(value.LocalPosition, value.Size, value.LocalRotationDegrees),
                            candidateBounds,
                            request.Definition.Constraints.ObjectClearanceMeters)))
                        continue;

                    fixture = new InteriorPlacement
                    {
                        PlacementStableId = StablePlacementId(
                            request.BuildingPlacementStableId,
                            zone.StableId,
                            fixtureRole,
                            archetype.StableId),
                        ZoneStableId = zone.StableId.Trim(),
                        PlacementLayerCode = InteriorLayoutCodes.Fixture,
                        PlacementRoleCode = fixtureRole.Trim(),
                        VisualKey = archetype.VisualKey.Trim(),
                        LocalPosition = candidate.Position,
                        LocalRotationDegrees = candidate.RotationDegrees,
                        Size = Clone(archetype.Size),
                        PresentationFlags = new[] { InteriorLayoutCodes.PresentationOnly },
                    };
                    return true;
                }
            }

            fixture = new InteriorPlacement();
            return false;
        }

        private static void AddSurfaceAndLooseItemPlacements(
            InteriorLayoutGenerationRequest request,
            InteriorZoneDefinition zone,
            InteriorPlacement fixture,
            string seedFingerprint,
            ICollection<InteriorPlacement> output)
        {
            var archetype = request.FixtureArchetypes.Single(value =>
                string.Equals(value.VisualKey, fixture.VisualKey, StringComparison.Ordinal)
                && string.Equals(value.FixtureRoleCode, fixture.PlacementRoleCode, StringComparison.Ordinal));
            foreach (var surface in archetype.Surfaces.OrderBy(value => value.StableId, StringComparer.Ordinal))
            {
                var surfaceId = fixture.PlacementStableId + ":surface:" + surface.StableId.Trim();
                var surfacePosition = Add(fixture.LocalPosition, RotateLocal(surface.LocalPosition, fixture.LocalRotationDegrees));
                output.Add(new InteriorPlacement
                {
                    PlacementStableId = surfaceId,
                    ParentPlacementStableId = fixture.PlacementStableId,
                    ZoneStableId = zone.StableId,
                    PlacementLayerCode = InteriorLayoutCodes.Surface,
                    PlacementRoleCode = surface.SupportKindCode,
                    VisualKey = string.Empty,
                    LocalPosition = surfacePosition,
                    Size = new InteriorSize3(),
                    PresentationFlags = new[] { InteriorLayoutCodes.PresentationOnly },
                });

                foreach (var slot in surface.Slots.OrderBy(value => value.StableId, StringComparer.Ordinal))
                {
                    var chosen = ChooseLooseItem(request, zone, slot, seedFingerprint + "|" + surfaceId);
                    if (chosen is null)
                        continue;
                    var reference = ChooseReference(
                        request.ReferenceCatalog,
                        zone,
                        slot,
                        chosen,
                        seedFingerprint + "|" + surfaceId + "|" + slot.StableId);
                    var slotPosition = Add(surfacePosition, RotateLocal(slot.LocalPosition, fixture.LocalRotationDegrees));
                    output.Add(new InteriorPlacement
                    {
                        PlacementStableId = surfaceId + ":slot:" + slot.StableId.Trim(),
                        ParentPlacementStableId = surfaceId,
                        ZoneStableId = zone.StableId,
                        PlacementLayerCode = InteriorLayoutCodes.LooseItem,
                        PlacementRoleCode = chosen.PlacementRoleCode,
                        VisualKey = chosen.VisualKey,
                        LocalPosition = slotPosition,
                        LocalRotationDegrees = fixture.LocalRotationDegrees,
                        Size = Clone(chosen.Size),
                        ReferenceStableId = reference?.ReferenceStableId ?? string.Empty,
                        PresentationFlags = new[] { InteriorLayoutCodes.PresentationOnly, slot.DetailLevelCode },
                    });
                }
            }
        }

        private static InteriorLooseItemArchetype? ChooseLooseItem(
            InteriorLayoutGenerationRequest request,
            InteriorZoneDefinition zone,
            InteriorSurfaceSlotDefinition slot,
            string seed)
        {
            var candidates = request.LooseItemArchetypes
                .Where(value => slot.AllowedPlacementRoleCodes.Length == 0
                                || slot.AllowedPlacementRoleCodes.Contains(value.PlacementRoleCode, StringComparer.Ordinal))
                .Where(value => slot.AllowedCategoryCodes.Length == 0
                                || slot.AllowedCategoryCodes.Contains(value.CategoryCode, StringComparer.Ordinal))
                .Where(value => zone.AllowedLooseItemCategoryCodes.Length == 0
                                || zone.AllowedLooseItemCategoryCodes.Contains(value.CategoryCode, StringComparer.Ordinal))
                .Where(value => Fits(value.Size, slot.MaximumSize))
                .OrderBy(value => value.StableId, StringComparer.Ordinal)
                .ToArray();
            return candidates.Length == 0 ? null : candidates[Index(seed, candidates.Length)];
        }

        private static ApprovedInteriorReference? ChooseReference(
            ApprovedInteriorReferenceCatalog catalog,
            InteriorZoneDefinition zone,
            InteriorSurfaceSlotDefinition slot,
            InteriorLooseItemArchetype item,
            string seed)
        {
            var candidates = catalog.Items
                .Where(value => string.Equals(value.CategoryCode, item.CategoryCode, StringComparison.Ordinal))
                .Where(value => value.RoomRoleCodes.Length == 0
                                || value.RoomRoleCodes.Contains(zone.RoleCode, StringComparer.Ordinal))
                .Where(value => value.PlacementRoleCodes.Length == 0
                                || value.PlacementRoleCodes.Contains(item.PlacementRoleCode, StringComparer.Ordinal))
                .Where(value => slot.AllowedCategoryCodes.Length == 0
                                || slot.AllowedCategoryCodes.Contains(value.CategoryCode, StringComparer.Ordinal))
                .OrderBy(value => value.ReferenceStableId, StringComparer.Ordinal)
                .ToArray();
            return candidates.Length == 0 ? null : candidates[Index(seed, candidates.Length)];
        }

        private static bool HasTraversalRoutes(
            InteriorDefinition definition,
            IReadOnlyCollection<InteriorPlacement> placements)
        {
            var anchors = definition.Structure.TraversalAnchors
                .Concat(definition.Zones.SelectMany(value => value.TraversalAnchors))
                .Concat(definition.Zones.Select(value => value.Bounds.Center))
                .ToArray();
            if (anchors.Length < 2)
                return true;

            var step = Math.Max(0.25d, definition.Constraints.GridStepMeters);
            var bounds = definition.Structure.UsableBounds;
            var minimumX = bounds.Center.X - bounds.Size.X / 2d;
            var minimumZ = bounds.Center.Z - bounds.Size.Z / 2d;
            var width = Math.Max(1, (int)Math.Floor(bounds.Size.X / step) + 1);
            var depth = Math.Max(1, (int)Math.Floor(bounds.Size.Z / step) + 1);
            bool Blocked(int x, int z)
            {
                var point = new InteriorVector3
                {
                    X = minimumX + x * step,
                    Z = minimumZ + z * step,
                };
                return definition.Structure.ExclusionBounds.Any(value => ContainsPoint(value, point))
                       || placements.Where(value => value.PlacementLayerCode == InteriorLayoutCodes.Fixture)
                           .Any(value => ContainsPoint(
                               Expand(Bounds(value.LocalPosition, value.Size, value.LocalRotationDegrees),
                                   definition.Constraints.TraversalClearanceMeters),
                               point));
            }

            var start = Grid(anchors[0], minimumX, minimumZ, step, width, depth);
            if (Blocked(start.X, start.Z))
                return false;
            var queue = new Queue<(int X, int Z)>();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            queue.Enqueue(start);
            visited.Add(start.X + ":" + start.Z);
            var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var direction in directions)
                {
                    var next = (X: current.X + direction.Item1, Z: current.Z + direction.Item2);
                    var key = next.X + ":" + next.Z;
                    if (next.X < 0 || next.X >= width || next.Z < 0 || next.Z >= depth
                        || visited.Contains(key) || Blocked(next.X, next.Z))
                        continue;
                    visited.Add(key);
                    queue.Enqueue(next);
                }
            }

            return anchors.Skip(1).All(anchor =>
            {
                var cell = Grid(anchor, minimumX, minimumZ, step, width, depth);
                return visited.Contains(cell.X + ":" + cell.Z);
            });
        }

        private static IEnumerable<(InteriorVector3 Position, double RotationDegrees)> CandidatePositions(
            InteriorBounds zone,
            InteriorSize3 size,
            InteriorConstraintProfile constraints)
        {
            var step = Math.Max(0.25d, constraints.GridStepMeters);
            var minimumX = zone.Center.X - zone.Size.X / 2d + size.X / 2d;
            var maximumX = zone.Center.X + zone.Size.X / 2d - size.X / 2d;
            var minimumZ = zone.Center.Z - zone.Size.Z / 2d + size.Z / 2d;
            var maximumZ = zone.Center.Z + zone.Size.Z / 2d - size.Z / 2d;
            foreach (var rotation in new[] { 0d, 90d, 180d, 270d })
            {
                for (var x = minimumX; x <= maximumX + 0.0001d; x += step)
                for (var z = minimumZ; z <= maximumZ + 0.0001d; z += step)
                    yield return (new InteriorVector3 { X = x, Y = size.Y / 2d, Z = z }, rotation);
            }
        }

        private static IEnumerable<T> Rotate<T>(T[] values, string seed)
        {
            if (values.Length == 0)
                yield break;
            var start = Index(seed, values.Length);
            for (var index = 0; index < values.Length; index++)
                yield return values[(start + index) % values.Length];
        }

        private static void Validate(InteriorLayoutGenerationRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            Require(request.WorldSeed, nameof(request.WorldSeed));
            Require(request.BuildingPlacementStableId, nameof(request.BuildingPlacementStableId));
            Require(request.GeneratorRevision, nameof(request.GeneratorRevision));
            Require(request.Definition.StableId, "Definition.StableId");
            Require(request.Definition.Revision, "Definition.Revision");
            Require(request.Definition.H1StableId, "Definition.H1StableId");
            Require(request.ReferenceCatalog.Revision, "ReferenceCatalog.Revision");
            if (request.Definition.Zones.Select(value => value.StableId).Distinct(StringComparer.Ordinal).Count()
                != request.Definition.Zones.Length)
                throw new ArgumentException("Zone StableId는 중복될 수 없습니다.", nameof(request));
            if (request.Definition.Structure.UsableBounds.Size.X <= 0d
                || request.Definition.Structure.UsableBounds.Size.Z <= 0d)
                throw new ArgumentException("Structure usable bounds는 양수여야 합니다.", nameof(request));
            var expectedCatalogHash = InteriorLayoutHash.ComputeCatalogHash(request.ReferenceCatalog);
            if (!string.IsNullOrWhiteSpace(request.ReferenceCatalog.CatalogHashSha256)
                && !string.Equals(expectedCatalogHash, request.ReferenceCatalog.CatalogHashSha256, StringComparison.Ordinal))
                throw new ArgumentException("승인 Reference Catalog hash가 내용과 일치하지 않습니다.", nameof(request));
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.");
        }

        private static bool Fits(InteriorSize3 value, InteriorSize3 maximum)
            => value.X <= maximum.X && value.Y <= maximum.Y && value.Z <= maximum.Z;

        private static int Index(string seed, int length)
        {
            byte[] bytes;
            using (var sha256 = SHA256.Create())
                bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
            var value = BitConverter.ToUInt32(bytes, 0);
            return (int)(value % (uint)length);
        }

        private static string StablePlacementId(params string[] parts)
            => "interior:" + Hash(string.Join("|", parts.Select(value => value.Trim()))).Substring(0, 24);

        internal static string Hash(string value)
        {
            byte[] bytes;
            using (var sha256 = SHA256.Create())
                bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var item in bytes)
                builder.Append(item.ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static (int X, int Z) Grid(
            InteriorVector3 value,
            double minimumX,
            double minimumZ,
            double step,
            int width,
            int depth)
            => (
                Math.Max(0, Math.Min(width - 1, (int)Math.Round((value.X - minimumX) / step))),
                Math.Max(0, Math.Min(depth - 1, (int)Math.Round((value.Z - minimumZ) / step))));

        private static InteriorBounds Bounds(InteriorVector3 center, InteriorSize3 size, double rotation)
        {
            var quarterTurn = ((int)Math.Round(rotation / 90d) % 2 + 2) % 2 == 1;
            return new InteriorBounds
            {
                Center = Clone(center),
                Size = new InteriorSize3
                {
                    X = quarterTurn ? size.Z : size.X,
                    Y = size.Y,
                    Z = quarterTurn ? size.X : size.Z,
                },
            };
        }

        private static InteriorBounds Expand(InteriorBounds value, double amount)
            => new()
            {
                Center = Clone(value.Center),
                Size = new InteriorSize3
                {
                    X = value.Size.X + amount * 2d,
                    Y = value.Size.Y,
                    Z = value.Size.Z + amount * 2d,
                },
            };

        private static bool Contains(InteriorBounds outer, InteriorBounds inner)
            => Math.Abs(inner.Center.X - outer.Center.X) + inner.Size.X / 2d <= outer.Size.X / 2d + 0.0001d
               && Math.Abs(inner.Center.Z - outer.Center.Z) + inner.Size.Z / 2d <= outer.Size.Z / 2d + 0.0001d;

        private static bool ContainsPoint(InteriorBounds bounds, InteriorVector3 point)
            => Math.Abs(point.X - bounds.Center.X) <= bounds.Size.X / 2d
               && Math.Abs(point.Z - bounds.Center.Z) <= bounds.Size.Z / 2d;

        private static bool Overlaps(InteriorBounds left, InteriorBounds right, double clearance = 0d)
            => Math.Abs(left.Center.X - right.Center.X) * 2d < left.Size.X + right.Size.X + clearance * 2d
               && Math.Abs(left.Center.Z - right.Center.Z) * 2d < left.Size.Z + right.Size.Z + clearance * 2d;

        private static InteriorVector3 Add(InteriorVector3 left, InteriorVector3 right)
            => new() { X = left.X + right.X, Y = left.Y + right.Y, Z = left.Z + right.Z };

        private static InteriorVector3 RotateLocal(InteriorVector3 value, double rotation)
        {
            var radians = rotation * Math.PI / 180d;
            return new InteriorVector3
            {
                X = value.X * Math.Cos(radians) - value.Z * Math.Sin(radians),
                Y = value.Y,
                Z = value.X * Math.Sin(radians) + value.Z * Math.Cos(radians),
            };
        }

        private static InteriorBounds Clone(InteriorBounds value)
            => new() { Center = Clone(value.Center), Size = Clone(value.Size) };

        private static InteriorVector3 Clone(InteriorVector3 value)
            => new() { X = value.X, Y = value.Y, Z = value.Z };

        private static InteriorSize3 Clone(InteriorSize3 value)
            => new() { X = value.X, Y = value.Y, Z = value.Z };
    }

    public static class InteriorLayoutHash
    {
        public static string ComputeCatalogHash(ApprovedInteriorReferenceCatalog catalog)
        {
            if (catalog is null) throw new ArgumentNullException(nameof(catalog));
            var rows = catalog.Items
                .OrderBy(value => value.ReferenceStableId, StringComparer.Ordinal)
                .Select(value => string.Join("|", new[]
                {
                    value.ReferenceStableId.Trim(), value.MarketplaceCode.Trim(), value.CategoryCode.Trim(),
                    string.Join(",", value.RoomRoleCodes.OrderBy(item => item, StringComparer.Ordinal)),
                    string.Join(",", value.PlacementRoleCodes.OrderBy(item => item, StringComparer.Ordinal)),
                    value.ApprovedOriginalTitle.Trim(), value.SourceUrl.Trim(), value.ObservedAtUtc.Trim(),
                    value.RawObservationHashSha256.Trim(), value.SourceRevision.Trim(),
                    value.UsageRestrictionCode.Trim(),
                }));
            return DeterministicInteriorLayoutEngine.Hash(
                catalog.StableId.Trim() + "|" + catalog.Revision.Trim() + "|" + string.Join("\n", rows));
        }

        public static string ComputePlanHash(InteriorPlacementPlan plan)
        {
            if (plan is null) throw new ArgumentNullException(nameof(plan));
            if (string.Equals(plan.SchemaVersion,
                    InteriorLayoutCodes.SchemaVersionV2,
                    StringComparison.Ordinal))
                return ComputePlanHashV2(plan);
            var placements = plan.Placements
                .OrderBy(value => value.PlacementStableId, StringComparer.Ordinal)
                .Select(value => string.Join("|", new[]
                {
                    value.PlacementStableId, value.ParentPlacementStableId, value.ZoneStableId,
                    value.PlacementLayerCode, value.PlacementRoleCode, value.VisualKey,
                    Number(value.LocalPosition.X), Number(value.LocalPosition.Y), Number(value.LocalPosition.Z),
                    Number(value.LocalRotationDegrees), Number(value.Size.X), Number(value.Size.Y), Number(value.Size.Z),
                    value.ReferenceStableId,
                    string.Join(",", value.PresentationFlags.OrderBy(item => item, StringComparer.Ordinal)),
                }));
            var zones = plan.Zones.OrderBy(value => value.ZoneStableId, StringComparer.Ordinal)
                .Select(value => value.ZoneStableId + "|" + value.RoleCode + "|"
                                 + Number(value.Bounds.Center.X) + "|" + Number(value.Bounds.Center.Z) + "|"
                                 + Number(value.Bounds.Size.X) + "|" + Number(value.Bounds.Size.Z));
            return DeterministicInteriorLayoutEngine.Hash(string.Join("\n", new[]
            {
                plan.SchemaVersion, plan.BuildingPlacementStableId, plan.H1StableId,
                plan.InteriorDefinitionRevision, plan.ReferenceCatalogRevision,
                plan.ReferenceCatalogHashSha256, plan.GeneratorRevision, plan.SeedFingerprintSha256,
                string.Join(";", zones), string.Join(";", placements),
                string.Join(",", plan.UnresolvedRequiredFixtureCodes.OrderBy(value => value, StringComparer.Ordinal)),
                plan.TraversalValidated ? "1" : "0",
            }));
        }

        public static string ComputeVisualMetricCatalogHash(
            InteriorVisualMetricCatalog catalog)
        {
            if (catalog is null) throw new ArgumentNullException(nameof(catalog));
            var rows = catalog.Items
                .OrderBy(value => value.StableId, StringComparer.Ordinal)
                .Select(value => string.Join("|", new[]
                {
                    value.StableId.Trim(), value.VisualKey.Trim(),
                    value.SourceAssetFingerprintSha256.Trim(),
                    Number(value.SourceBoundsSize.X),
                    Number(value.SourceBoundsSize.Y),
                    Number(value.SourceBoundsSize.Z),
                    Number(value.MinimumUniformScale),
                    Number(value.MaximumUniformScale),
                    Number(value.RotationSnapDegrees),
                    value.RequiresProjectOwnedCollider ? "1" : "0",
                }));
            return DeterministicInteriorLayoutEngine.Hash(
                catalog.StableId.Trim() + "|" + catalog.Revision.Trim()
                + "|" + string.Join("\n", rows));
        }

        private static string ComputePlanHashV2(InteriorPlacementPlan plan)
        {
            var placements = plan.Placements
                .OrderBy(value => value.PlacementStableId,
                    StringComparer.Ordinal)
                .Select(value => string.Join("|", new[]
                {
                    value.PlacementStableId,
                    value.ParentPlacementStableId,
                    value.ZoneStableId,
                    value.OwningH1StableId,
                    value.PlacementLayerCode,
                    value.PlacementRoleCode,
                    value.VisualKey,
                    Number(value.RequestedTransform.LocalPosition.X),
                    Number(value.RequestedTransform.LocalPosition.Y),
                    Number(value.RequestedTransform.LocalPosition.Z),
                    Number(value.RequestedTransform.LocalRotationDegrees),
                    Number(value.RequestedTransform.UniformScale),
                    Number(value.AppliedTransform.LocalPosition.X),
                    Number(value.AppliedTransform.LocalPosition.Y),
                    Number(value.AppliedTransform.LocalPosition.Z),
                    Number(value.AppliedTransform.LocalRotationDegrees),
                    Number(value.AppliedTransform.UniformScale),
                    Number(value.Size.X), Number(value.Size.Y),
                    Number(value.Size.Z),
                    value.VisualMetricStableId,
                    value.SourceAssetFingerprintSha256,
                    value.AdjustmentStableId,
                    value.ReferenceStableId,
                    string.Join(",", value.ValidationCodes.OrderBy(
                        item => item, StringComparer.Ordinal)),
                    string.Join(",", value.PresentationFlags.OrderBy(
                        item => item, StringComparer.Ordinal)),
                }));
            var zones = plan.Zones
                .OrderBy(value => value.ZoneStableId,
                    StringComparer.Ordinal)
                .Select(value => string.Join("|", new[]
                {
                    value.ZoneStableId,
                    value.OwningH1StableId,
                    value.RoleCode,
                    Number(value.Bounds.Center.X),
                    Number(value.Bounds.Center.Y),
                    Number(value.Bounds.Center.Z),
                    Number(value.Bounds.Size.X),
                    Number(value.Bounds.Size.Y),
                    Number(value.Bounds.Size.Z),
                }));
            return DeterministicInteriorLayoutEngine.Hash(string.Join("\n",
                new[]
                {
                    plan.SchemaVersion,
                    plan.BuildingPlacementStableId,
                    plan.H1StableId,
                    plan.InteriorDefinitionRevision,
                    plan.ReferenceCatalogRevision,
                    plan.ReferenceCatalogHashSha256,
                    plan.GeneratorRevision,
                    plan.SeedFingerprintSha256,
                    plan.PlacementControlRuleRevision,
                    plan.VisualMetricCatalogRevision,
                    plan.VisualMetricCatalogHashSha256,
                    plan.AdjustmentRevision,
                    plan.BaseInteriorPlacementPlanHashSha256,
                    string.Join(";", zones),
                    string.Join(";", placements),
                    string.Join(",", plan.UnresolvedRequiredFixtureCodes
                        .OrderBy(value => value, StringComparer.Ordinal)),
                    plan.TraversalValidated ? "1" : "0",
                }));
        }

        private static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    }
}
