using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 대관령 Farm L2 공간의 자료 조사 기반 다단계 스트림 계약을 제공한다.
    /// 현재 물리 DEM 산출물은 등록되지 않았으므로 URL이나 높이를 꾸며내지 않는다.
    /// </summary>
    public sealed class SimulationWorldStreamingService
    {
        public const int CenterX = 700;
        public const int CenterY = 1145;
        public const int CoverageRadius = 5;
        public const int DetailRadius = 1;
        public const int ActiveRadius = 2;
        public const int PrefetchRadius = 4;
        public const int MaxConcurrentTileLoads = 4;
        public const double BoundaryPrefetchFraction = 0.25d;
        public const int L2HaloMeters = 60;
        public const string RecipeRevision = "world-stream.pyeongchang-farm.r2";
        public const string ManifestRevision = "world-stream.tile-manifest.r1";
        public const string ObjectPlacementRevision = "world-stream.object-placement.r1";

        private static readonly string[] LayerCodes =
        {
            SimulationWorldStreamCodes.ElevationLayer,
            SimulationWorldStreamCodes.LandCoverLayer,
            SimulationWorldStreamCodes.PlacementMaskLayer,
        };

        private readonly SimulationWorldStreamRecipeResponse recipe;
        private readonly HashSet<string> coverage;

        public SimulationWorldStreamingService()
        {
            var tileKeys = CreateCoverageTileKeys();
            coverage = new HashSet<string>(tileKeys, StringComparer.Ordinal);
            recipe = new SimulationWorldStreamRecipeResponse
            {
                RecipeStableId = SimulationWorldStreamCodes.PyeongchangFarmRecipe,
                RecipeRevision = RecipeRevision,
                CoordinateReferenceSystem = "EPSG:5186",
                TileLevel = 2,
                TileSizeMeters = 500,
                DetailRadius = DetailRadius,
                ActiveRadius = ActiveRadius,
                PrefetchRadius = PrefetchRadius,
                MaxConcurrentTileLoads = MaxConcurrentTileLoads,
                BoundaryPrefetchFraction = BoundaryPrefetchFraction,
                CenterTileX = CenterX,
                CenterTileY = CenterY,
                CoverageTileKeys = tileKeys,
                LayerCodes = LayerCodes.ToArray(),
                IsOperationalState = false,
                EvidenceKindCode = SimulationWorldStreamCodes.Derived,
            };
            recipe.RecipeHashSha256 = Hash(RecipeCanonical(recipe));
        }

        public bool TryGetRecipe(
            string recipeStableId,
            out SimulationWorldStreamRecipeResponse value)
        {
            value = recipe;
            return string.Equals(
                recipeStableId,
                SimulationWorldStreamCodes.PyeongchangFarmRecipe,
                StringComparison.Ordinal);
        }

        public bool TryGetManifest(
            string tileKey,
            out SimulationWorldTileStreamManifestResponse value)
        {
            value = new SimulationWorldTileStreamManifestResponse();
            if (!coverage.Contains(tileKey) || !TryParseTileKey(tileKey, out var x, out var y))
                return false;

            var layers = LayerCodes.Select(code => CreateLayer(code)).ToArray();
            value = new SimulationWorldTileStreamManifestResponse
            {
                RecipeStableId = recipe.RecipeStableId,
                TileKey = tileKey,
                TileLevel = 2,
                TileX = x,
                TileY = y,
                HaloMeters = L2HaloMeters,
                ManifestRevision = ManifestRevision,
                Layers = layers,
                IsOperationalState = false,
            };
            value.ManifestHashSha256 = Hash(ManifestCanonical(value));
            return true;
        }

        public bool TryGetArtifact(
            string tileKey,
            string layerCode,
            out SimulationWorldTileArtifactDescriptorResponse value)
        {
            value = new SimulationWorldTileArtifactDescriptorResponse();
            if (!coverage.Contains(tileKey) || !LayerCodes.Contains(layerCode, StringComparer.Ordinal))
                return false;

            var layer = CreateLayer(layerCode);
            value = new SimulationWorldTileArtifactDescriptorResponse
            {
                TileKey = tileKey,
                LayerCode = layer.LayerCode,
                StatusCode = layer.StatusCode,
                EvidenceKindCode = layer.EvidenceKindCode,
                SourceRevision = layer.SourceRevision,
                ArtifactHashSha256 = layer.ArtifactHashSha256,
                ArtifactRelativePath = layer.ArtifactRelativePath,
                PresentationOnly = layer.PresentationOnly,
                KoreanStatusLabel = "공간 산출물 자료 대기",
            };
            return true;
        }

        public bool TryGetActivities(
            string tileKey,
            out SimulationWorldTileActivityProjectionResponse value)
        {
            value = new SimulationWorldTileActivityProjectionResponse();
            if (!coverage.Contains(tileKey))
                return false;

            value = new SimulationWorldTileActivityProjectionResponse
            {
                TileKey = tileKey,
                ActivityRevision = 0,
                WorldTick = 0,
                ActivityStableIds = Array.Empty<string>(),
                PresentationOnly = true,
                IsOperationalState = false,
            };
            return true;
        }

        public bool TryGetObjects(
            string tileKey,
            out SimulationWorldTileObjectProjectionResponse value)
        {
            value = new SimulationWorldTileObjectProjectionResponse();
            if (!coverage.Contains(tileKey))
                return false;

            var objects = CreateScenarioObjects(tileKey);
            value = new SimulationWorldTileObjectProjectionResponse
            {
                TileKey = tileKey,
                PlacementRevision = ObjectPlacementRevision,
                Objects = objects,
                PresentationOnly = true,
                IsOperationalState = false,
            };
            value.PlacementHashSha256 = Hash(ObjectCanonical(value));
            return true;
        }

        public static string TileKey(int x, int y) => $"kr5186:l2:{x}:{y}";

        private static string[] CreateCoverageTileKeys()
        {
            var result = new List<string>();
            for (var y = CenterY - CoverageRadius; y <= CenterY + CoverageRadius; y++)
            for (var x = CenterX - CoverageRadius; x <= CenterX + CoverageRadius; x++)
                result.Add(TileKey(x, y));
            return result.ToArray();
        }

        private static bool TryParseTileKey(string tileKey, out int x, out int y)
        {
            x = 0;
            y = 0;
            var parts = tileKey.Split(':');
            return parts.Length == 4
                && parts[0] == "kr5186"
                && parts[1] == "l2"
                && int.TryParse(parts[2], out x)
                && int.TryParse(parts[3], out y);
        }

        private static SimulationWorldTileLayerDescriptorResponse CreateLayer(string code)
            => new SimulationWorldTileLayerDescriptorResponse
            {
                LayerCode = code,
                StatusCode = SimulationWorldStreamCodes.WaitingForSpatialArtifact,
                EvidenceKindCode = code == SimulationWorldStreamCodes.ElevationLayer
                    ? SimulationWorldStreamCodes.Observed
                    : SimulationWorldStreamCodes.Derived,
                SourceRevision = code == SimulationWorldStreamCodes.ElevationLayer
                    ? "dem-source-registered.runtime-artifact-missing"
                    : "spatial-derived-artifact-missing",
                ArtifactHashSha256 = null,
                ArtifactRelativePath = null,
                PresentationOnly = false,
            };

        private static SimulationWorldTileObjectPlacementResponse[] CreateScenarioObjects(
            string tileKey)
        {
            SimulationWorldTileObjectPlacementResponse Building(
                string stableId,
                string visualKey,
                double offsetX,
                double offsetY,
                double rotation,
                double width,
                double depth,
                double height)
                => new SimulationWorldTileObjectPlacementResponse
                {
                    ObjectStableId = stableId,
                    ObjectTypeCode = SimulationWorldStreamCodes.BuildingObject,
                    VisualKey = visualKey,
                    EvidenceKindCode = SimulationWorldStreamCodes.Scenario,
                    LandCoverCode = "Cropland",
                    RegionRoleCode = "Farm",
                    LocalOffsetXMeters = offsetX,
                    LocalOffsetYMeters = offsetY,
                    RotationDegrees = rotation,
                    FootprintWidthMeters = width,
                    FootprintDepthMeters = depth,
                    HeightMeters = height,
                    CollisionEligible = false,
                    PresentationOnly = true,
                };

            if (tileKey == TileKey(CenterX, CenterY))
                return new[]
                {
                    Building(PyeongchangWorldExplorationFixtureIds.Barn,
                        "legal.agriculture.building.barn", 78d, 42d, 18d, 32d, 24d, 18d),
                    Building(PyeongchangWorldExplorationFixtureIds.Silo,
                        "legal.agriculture.building.silo", 132d, 58d, 0d, 14d, 14d, 26d),
                };
            if (tileKey == TileKey(CenterX + 1, CenterY))
                return new[]
                {
                    Building("scenario-object:pyeongchang-farm:farmhouse-east",
                        "legal.rural.building.farmhouse", -122d, 36d, -12d, 26d, 20d, 15d),
                };
            if (tileKey == TileKey(CenterX - 1, CenterY))
                return new[]
                {
                    Building("scenario-object:pyeongchang-farm:greenhouse-west",
                        "legal.agriculture.building.greenhouse", 118d, -48d, 8d, 34d, 16d, 11d),
                };
            if (tileKey == TileKey(CenterX, CenterY + 1))
                return new[]
                {
                    Building("scenario-object:pyeongchang-farm:produce-stand-north",
                        "legal.rural.building.produce-stand", -68d, -136d, 24d, 16d, 12d, 9d),
                };
            return Array.Empty<SimulationWorldTileObjectPlacementResponse>();
        }

        private static string RecipeCanonical(SimulationWorldStreamRecipeResponse value)
            => string.Join("|", new[]
            {
                value.RecipeStableId, value.RecipeRevision, value.CoordinateReferenceSystem,
                value.TileLevel.ToString(), value.TileSizeMeters.ToString(),
                value.DetailRadius.ToString(), value.ActiveRadius.ToString(),
                value.PrefetchRadius.ToString(), value.MaxConcurrentTileLoads.ToString(),
                value.BoundaryPrefetchFraction.ToString(
                    "R", System.Globalization.CultureInfo.InvariantCulture),
                value.CenterTileX.ToString(), value.CenterTileY.ToString(),
                string.Join(",", value.CoverageTileKeys), string.Join(",", value.LayerCodes),
                value.EvidenceKindCode,
            });

        private static string ManifestCanonical(SimulationWorldTileStreamManifestResponse value)
            => string.Join("|", new[]
            {
                value.RecipeStableId, value.TileKey, value.TileLevel.ToString(),
                value.TileX.ToString(), value.TileY.ToString(), value.HaloMeters.ToString(),
                value.ManifestRevision,
                string.Join(",", value.Layers.Select(layer =>
                    layer.LayerCode + ":" + layer.StatusCode + ":" + layer.SourceRevision)),
            });

        private static string ObjectCanonical(SimulationWorldTileObjectProjectionResponse value)
            => string.Join("|", new[]
            {
                value.TileKey,
                value.PlacementRevision,
                string.Join(",", value.Objects.Select(item => string.Join(":", new[]
                {
                    item.ObjectStableId, item.ObjectTypeCode, item.VisualKey,
                    item.EvidenceKindCode, item.LandCoverCode, item.RegionRoleCode,
                    item.LocalOffsetXMeters.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    item.LocalOffsetYMeters.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    item.RotationDegrees.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    item.FootprintWidthMeters.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    item.FootprintDepthMeters.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    item.HeightMeters.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    item.CollisionEligible.ToString(), item.PresentationOnly.ToString(),
                }))),
                value.PresentationOnly.ToString(),
            });

        private static string Hash(string canonical)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var value in bytes) builder.Append(value.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
