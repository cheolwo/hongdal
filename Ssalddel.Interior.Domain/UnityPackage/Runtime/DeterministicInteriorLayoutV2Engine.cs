using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Interior.Domain
{
    /// <summary>
    /// v1의 결정적 Zone 배치를 보존하면서 H1 소속, Synty 실측 크기와
    /// 제작자 제한 조정을 하나의 불변 계획으로 정제합니다.
    /// </summary>
    internal static class DeterministicInteriorLayoutV2Engine
    {
        public static InteriorPlacementPlan Generate(
            InteriorLayoutGenerationRequest request)
        {
            ValidateRequest(request);
            var metricHash = InteriorLayoutHash.ComputeVisualMetricCatalogHash(
                request.VisualMetricCatalog);
            var legacyRequest = new InteriorLayoutGenerationRequest
            {
                SchemaVersion = InteriorLayoutCodes.SchemaVersionV1,
                WorldSeed = request.WorldSeed,
                BuildingPlacementStableId = request.BuildingPlacementStableId,
                GeneratorRevision = request.GeneratorRevision,
                Definition = request.Definition,
                ReferenceCatalog = request.ReferenceCatalog,
                FixtureArchetypes = request.FixtureArchetypes,
                LooseItemArchetypes = request.LooseItemArchetypes,
            };
            var plan = new DeterministicInteriorLayoutEngine().Generate(
                legacyRequest);
            plan.SchemaVersion = InteriorLayoutCodes.SchemaVersionV2;
            plan.PlacementControlRuleRevision =
                request.PlacementControlRuleRevision.Trim();
            plan.VisualMetricCatalogRevision =
                request.VisualMetricCatalog.Revision.Trim();
            plan.VisualMetricCatalogHashSha256 = metricHash;
            plan.AdjustmentRevision = request.AdjustmentRevision?.Trim()
                                      ?? string.Empty;
            plan.SeedFingerprintSha256 =
                DeterministicInteriorLayoutEngine.Hash(string.Join("|", new[]
                {
                    request.WorldSeed.Trim(),
                    request.BuildingPlacementStableId.Trim(),
                    request.Definition.Revision.Trim(),
                    request.ReferenceCatalog.Revision.Trim(),
                    request.GeneratorRevision.Trim(),
                    plan.PlacementControlRuleRevision,
                    plan.VisualMetricCatalogRevision,
                    metricHash,
                }));

            var zones = request.Definition.Zones.ToDictionary(
                value => value.StableId.Trim(), StringComparer.Ordinal);
            foreach (var zone in plan.Zones)
                zone.OwningH1StableId = RequiredOwningH1(
                    zones[zone.ZoneStableId], request.Definition);
            foreach (var placement in plan.Placements)
            {
                placement.OwningH1StableId = RequiredOwningH1(
                    zones[placement.ZoneStableId], request.Definition);
                placement.RequestedTransform = Transform(
                    placement.LocalPosition,
                    placement.LocalRotationDegrees,
                    1d);
                placement.AppliedTransform = Transform(
                    placement.LocalPosition,
                    placement.LocalRotationDegrees,
                    1d);
                placement.ValidationCodes = new[]
                {
                    InteriorLayoutCodes.PlacementAccepted,
                };
                if (string.IsNullOrWhiteSpace(placement.VisualKey))
                    continue;
                var metric = Metric(request, placement.VisualKey);
                var scale = RequiredUniformScale(placement.Size, metric);
                placement.VisualMetricStableId = metric.StableId.Trim();
                placement.SourceAssetFingerprintSha256 =
                    metric.SourceAssetFingerprintSha256.Trim();
                placement.AppliedTransform.UniformScale = scale;
                placement.Size = Scaled(metric.SourceBoundsSize, scale);
            }

            plan.BaseInteriorPlacementPlanHashSha256 = string.Empty;
            plan.InteriorPlacementPlanHashSha256 =
                InteriorLayoutHash.ComputePlanHash(plan);
            var baseHash = plan.InteriorPlacementPlanHashSha256;
            ApplyAdjustments(plan, request, baseHash);
            ValidateAppliedPlan(plan, request);
            plan.BaseInteriorPlacementPlanHashSha256 = baseHash;
            plan.InteriorPlacementPlanHashSha256 =
                InteriorLayoutHash.ComputePlanHash(plan);
            return plan;
        }

        private static void ApplyAdjustments(
            InteriorPlacementPlan plan,
            InteriorLayoutGenerationRequest request,
            string baseHash)
        {
            var placements = plan.Placements.ToDictionary(
                value => value.PlacementStableId, StringComparer.Ordinal);
            foreach (var adjustment in request.Adjustments
                         .OrderBy(value => value.AdjustmentStableId,
                             StringComparer.Ordinal))
            {
                if (!string.Equals(adjustment.ExpectedBasePlanHashSha256,
                        baseHash, StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "StalePlacementOverride:" + adjustment.AdjustmentStableId);
                if (!placements.TryGetValue(adjustment.PlacementStableId,
                        out var target))
                    throw new InvalidOperationException(
                        "PlacementOverrideTargetMissing:"
                        + adjustment.PlacementStableId);
                if (target.PlacementLayerCode == InteriorLayoutCodes.Surface)
                    throw new InvalidOperationException(
                        "PlacementOverrideSurfaceForbidden:"
                        + adjustment.PlacementStableId);
                ValidateAdjustment(adjustment, target, request);

                var oldPosition = Clone(target.LocalPosition);
                var oldRotation = target.LocalRotationDegrees;
                var oldScale = target.AppliedTransform.UniformScale;
                var newPosition = Add(oldPosition, adjustment.PositionDelta);
                var newRotation = NormalizeDegrees(
                    oldRotation + adjustment.RotationDeltaDegrees);
                var newScale = adjustment.UniformScale;
                ApplyTransform(target, newPosition, newRotation, newScale,
                    adjustment.AdjustmentStableId);

                var descendants = Descendants(target.PlacementStableId,
                    placements.Values).ToArray();
                foreach (var descendant in descendants)
                {
                    var relative = Subtract(descendant.LocalPosition,
                        oldPosition);
                    relative = Rotate(relative,
                        adjustment.RotationDeltaDegrees);
                    relative = Scale(relative, newScale / oldScale);
                    ApplyTransform(
                        descendant,
                        Add(newPosition, relative),
                        NormalizeDegrees(descendant.LocalRotationDegrees
                                         + adjustment.RotationDeltaDegrees),
                        descendant.AppliedTransform.UniformScale
                        * newScale / oldScale,
                        adjustment.AdjustmentStableId);
                }
            }
        }

        private static IEnumerable<InteriorPlacement> Descendants(
            string parentStableId,
            IEnumerable<InteriorPlacement> values)
        {
            var direct = values.Where(value => string.Equals(
                    value.ParentPlacementStableId,
                    parentStableId,
                    StringComparison.Ordinal))
                .OrderBy(value => value.PlacementStableId,
                    StringComparer.Ordinal).ToArray();
            foreach (var value in direct)
            {
                yield return value;
                foreach (var child in Descendants(value.PlacementStableId,
                             values))
                    yield return child;
            }
        }

        private static void ValidateAdjustment(
            InteriorPlacementAdjustment adjustment,
            InteriorPlacement target,
            InteriorLayoutGenerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(adjustment.AdjustmentStableId)
                || string.IsNullOrWhiteSpace(adjustment.ReasonCode))
                throw new InvalidOperationException(
                    "PlacementOverrideIdentityInvalid");
            var constraints = request.Definition.Constraints;
            var horizontalDistance = Math.Sqrt(
                adjustment.PositionDelta.X * adjustment.PositionDelta.X
                + adjustment.PositionDelta.Z * adjustment.PositionDelta.Z);
            if (horizontalDistance
                > constraints.MaximumAuthoringAdjustmentMeters + 0.0001d
                || Math.Abs(adjustment.PositionDelta.Y)
                > constraints.MaximumAuthoringAdjustmentMeters + 0.0001d
                || !IsSnapped(adjustment.PositionDelta.X,
                    constraints.FineAdjustmentStepMeters)
                || !IsSnapped(adjustment.PositionDelta.Y,
                    constraints.FineAdjustmentStepMeters)
                || !IsSnapped(adjustment.PositionDelta.Z,
                    constraints.FineAdjustmentStepMeters))
                throw new InvalidOperationException(
                    "PlacementOverrideDeltaInvalid:"
                    + adjustment.AdjustmentStableId);

            var metric = Metric(request, target.VisualKey);
            if (!IsSnapped(adjustment.RotationDeltaDegrees,
                    metric.RotationSnapDegrees)
                || adjustment.UniformScale
                < metric.MinimumUniformScale - 0.0001d
                || adjustment.UniformScale
                > metric.MaximumUniformScale + 0.0001d)
                throw new InvalidOperationException(
                    "PlacementOverrideScaleOrRotationInvalid:"
                    + adjustment.AdjustmentStableId);
        }

        private static void ValidateAppliedPlan(
            InteriorPlacementPlan plan,
            InteriorLayoutGenerationRequest request)
        {
            var zones = plan.Zones.ToDictionary(value => value.ZoneStableId,
                StringComparer.Ordinal);
            var fixtures = plan.Placements.Where(value =>
                    value.PlacementLayerCode == InteriorLayoutCodes.Fixture)
                .OrderBy(value => value.PlacementStableId,
                    StringComparer.Ordinal).ToArray();
            foreach (var fixture in fixtures)
            {
                var bounds = Bounds(fixture);
                if (!Contains(zones[fixture.ZoneStableId].Bounds, bounds))
                    throw new InvalidOperationException(
                        "PlacementOverrideZoneViolation:"
                        + fixture.PlacementStableId);
                if (request.Definition.Structure.ExclusionBounds.Any(value =>
                        Overlaps(value, bounds)))
                    throw new InvalidOperationException(
                        "PlacementOverrideExclusionViolation:"
                        + fixture.PlacementStableId);
            }

            for (var left = 0; left < fixtures.Length; left++)
            for (var right = left + 1; right < fixtures.Length; right++)
                if (Overlaps(Bounds(fixtures[left]), Bounds(fixtures[right]),
                        request.Definition.Constraints.ObjectClearanceMeters))
                    throw new InvalidOperationException(
                        "PlacementOverrideClearanceViolation:"
                        + fixtures[left].PlacementStableId + ":"
                        + fixtures[right].PlacementStableId);
        }

        private static void ValidateRequest(
            InteriorLayoutGenerationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!string.Equals(request.SchemaVersion,
                    InteriorLayoutCodes.SchemaVersionV2,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(
                    request.PlacementControlRuleRevision)
                || request.VisualMetricCatalog == null
                || string.IsNullOrWhiteSpace(
                    request.VisualMetricCatalog.StableId)
                || string.IsNullOrWhiteSpace(
                    request.VisualMetricCatalog.Revision))
                throw new ArgumentException(
                    "InteriorPlacementPlanV2InputInvalid", nameof(request));
            var metrics = request.VisualMetricCatalog.Items;
            if (metrics.Select(value => value.StableId)
                    .Distinct(StringComparer.Ordinal).Count() != metrics.Length
                || metrics.Select(value => value.VisualKey)
                    .Distinct(StringComparer.Ordinal).Count() != metrics.Length
                || metrics.Any(value =>
                    string.IsNullOrWhiteSpace(value.StableId)
                    || string.IsNullOrWhiteSpace(value.VisualKey)
                    || value.SourceAssetFingerprintSha256.Length != 64
                    || value.SourceBoundsSize.X <= 0d
                    || value.SourceBoundsSize.Y <= 0d
                    || value.SourceBoundsSize.Z <= 0d
                    || value.MinimumUniformScale <= 0d
                    || value.MaximumUniformScale
                    < value.MinimumUniformScale
                    || value.RotationSnapDegrees <= 0d))
                throw new ArgumentException(
                    "InteriorVisualMetricCatalogInvalid", nameof(request));
            var expectedHash =
                InteriorLayoutHash.ComputeVisualMetricCatalogHash(
                    request.VisualMetricCatalog);
            if (!string.Equals(expectedHash,
                    request.VisualMetricCatalog.CatalogHashSha256,
                    StringComparison.Ordinal))
                throw new ArgumentException(
                    "InteriorVisualMetricCatalogHashMismatch", nameof(request));
            if (request.Definition.Zones.Any(value =>
                    string.IsNullOrWhiteSpace(RequiredOwningH1(
                        value, request.Definition))))
                throw new ArgumentException(
                    "InteriorZoneH1OwnershipMissing", nameof(request));
            if (request.Adjustments.Select(value => value.AdjustmentStableId)
                    .Distinct(StringComparer.Ordinal).Count()
                != request.Adjustments.Length)
                throw new ArgumentException(
                    "InteriorPlacementAdjustmentDuplicate", nameof(request));
            var constraints = request.Definition.Constraints;
            if (constraints.FineAdjustmentStepMeters <= 0d
                || constraints.MaximumAuthoringAdjustmentMeters < 0d
                || constraints.RotationSnapDegrees <= 0d)
                throw new ArgumentException(
                    "InteriorPlacementConstraintV2Invalid", nameof(request));
        }

        private static string RequiredOwningH1(
            InteriorZoneDefinition zone,
            InteriorDefinition definition)
            => string.IsNullOrWhiteSpace(zone.OwningH1StableId)
                ? definition.H1StableId.Trim()
                : zone.OwningH1StableId.Trim();

        private static InteriorVisualMetric Metric(
            InteriorLayoutGenerationRequest request,
            string visualKey)
            => request.VisualMetricCatalog.Items.SingleOrDefault(value =>
                   string.Equals(value.VisualKey, visualKey,
                       StringComparison.Ordinal))
               ?? throw new InvalidOperationException(
                   "InteriorVisualMetricMissing:" + visualKey);

        private static double RequiredUniformScale(
            InteriorSize3 target,
            InteriorVisualMetric metric)
        {
            var scale = Math.Min(
                target.X / metric.SourceBoundsSize.X,
                Math.Min(target.Y / metric.SourceBoundsSize.Y,
                    target.Z / metric.SourceBoundsSize.Z));
            if (scale < metric.MinimumUniformScale - 0.0001d
                || scale > metric.MaximumUniformScale + 0.0001d)
                throw new InvalidOperationException(
                    "InteriorVisualMetricScaleUnavailable:"
                    + metric.VisualKey);
            return scale;
        }

        private static void ApplyTransform(
            InteriorPlacement target,
            InteriorVector3 position,
            double rotation,
            double scale,
            string adjustmentStableId)
        {
            var ratio = scale / target.AppliedTransform.UniformScale;
            target.LocalPosition = Clone(position);
            target.LocalRotationDegrees = rotation;
            target.Size = Scaled(target.Size, ratio);
            target.AppliedTransform = Transform(position, rotation, scale);
            target.AdjustmentStableId = adjustmentStableId;
        }

        private static InteriorPlacementTransform Transform(
            InteriorVector3 position,
            double rotation,
            double scale)
            => new()
            {
                LocalPosition = Clone(position),
                LocalRotationDegrees = NormalizeDegrees(rotation),
                UniformScale = scale,
            };

        private static bool IsSnapped(double value, double step)
            => Math.Abs(value / step - Math.Round(value / step)) < 0.0001d;

        private static double NormalizeDegrees(double value)
        {
            var normalized = value % 360d;
            return normalized < 0d ? normalized + 360d : normalized;
        }

        private static InteriorBounds Bounds(InteriorPlacement placement)
        {
            var quarterTurn = ((int)Math.Round(
                placement.LocalRotationDegrees / 90d) % 2 + 2) % 2 == 1;
            return new InteriorBounds
            {
                Center = Clone(placement.LocalPosition),
                Size = new InteriorSize3
                {
                    X = quarterTurn ? placement.Size.Z : placement.Size.X,
                    Y = placement.Size.Y,
                    Z = quarterTurn ? placement.Size.X : placement.Size.Z,
                },
            };
        }

        private static bool Contains(InteriorBounds outer, InteriorBounds inner)
            => Math.Abs(inner.Center.X - outer.Center.X)
                   + inner.Size.X / 2d <= outer.Size.X / 2d + 0.0001d
               && Math.Abs(inner.Center.Z - outer.Center.Z)
                   + inner.Size.Z / 2d <= outer.Size.Z / 2d + 0.0001d;

        private static bool Overlaps(
            InteriorBounds left,
            InteriorBounds right,
            double clearance = 0d)
            => Math.Abs(left.Center.X - right.Center.X) * 2d
                   < left.Size.X + right.Size.X + clearance * 2d
               && Math.Abs(left.Center.Z - right.Center.Z) * 2d
                   < left.Size.Z + right.Size.Z + clearance * 2d;

        private static InteriorVector3 Rotate(
            InteriorVector3 value,
            double degrees)
        {
            var radians = degrees * Math.PI / 180d;
            return new InteriorVector3
            {
                X = value.X * Math.Cos(radians)
                    - value.Z * Math.Sin(radians),
                Y = value.Y,
                Z = value.X * Math.Sin(radians)
                    + value.Z * Math.Cos(radians),
            };
        }

        private static InteriorVector3 Add(
            InteriorVector3 left,
            InteriorVector3 right)
            => new()
            {
                X = left.X + right.X,
                Y = left.Y + right.Y,
                Z = left.Z + right.Z,
            };

        private static InteriorVector3 Subtract(
            InteriorVector3 left,
            InteriorVector3 right)
            => new()
            {
                X = left.X - right.X,
                Y = left.Y - right.Y,
                Z = left.Z - right.Z,
            };

        private static InteriorVector3 Scale(
            InteriorVector3 value,
            double scale)
            => new()
            {
                X = value.X * scale,
                Y = value.Y * scale,
                Z = value.Z * scale,
            };

        private static InteriorSize3 Scaled(
            InteriorSize3 value,
            double scale)
            => new()
            {
                X = value.X * scale,
                Y = value.Y * scale,
                Z = value.Z * scale,
            };

        private static InteriorVector3 Clone(InteriorVector3 value)
            => new() { X = value.X, Y = value.Y, Z = value.Z };
    }
}
