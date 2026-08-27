using System;
using System.Linq;

namespace Ssalddel.Unity.Data
{
    public sealed class 영역건물발전ApiModel
    {
        public string CatalogRevision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public 건물발전NodeApiModel[] Nodes { get; set; }
            = Array.Empty<건물발전NodeApiModel>();
        public 승인가르침자료ApiModel[] ApprovedTeachingMaterials { get; set; }
            = Array.Empty<승인가르침자료ApiModel>();
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class 건물발전NodeApiModel
    {
        public string BlueprintStableId { get; set; } = string.Empty;
        public string StageCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool IsDay2Priority { get; set; }
        public int CompletedWorkSeconds { get; set; }
        public int RequiredWorkSeconds { get; set; }
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double YawDegrees { get; set; }
        public int CompletedLearningVisitCount { get; set; }
    }

    public sealed class 승인가르침자료ApiModel
    {
        public string TeachingMaterialStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string HashSha256 { get; set; } = string.Empty;
        public string TopicCode { get; set; } = string.Empty;
        public string KoreanTitle { get; set; } = string.Empty;
        public string ShortSummary { get; set; } = string.Empty;
        public string SourceKindCode { get; set; } = string.Empty;
        public string SourceReference { get; set; } = string.Empty;
        public string ViewpointAndLimitations { get; set; } = string.Empty;
        public bool AdminApproved { get; set; }
    }

    public sealed class 영역건물발전PresentationModel
    {
        public string AreaCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string CatalogRevision { get; set; } = string.Empty;
        public 건물발전NodePresentationModel[] Nodes { get; set; }
            = Array.Empty<건물발전NodePresentationModel>();
        public 가르침자료PresentationModel[] TeachingMaterials { get; set; }
            = Array.Empty<가르침자료PresentationModel>();
    }

    public sealed class 건물발전NodePresentationModel
    {
        public string BlueprintStableId { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string StageCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool IsRecommendedPriority { get; set; }
        public float WorkProgress01 { get; set; }
        public int CompletedLearningVisitCount { get; set; }
    }

    public sealed class 가르침자료PresentationModel
    {
        public string StableId { get; set; } = string.Empty;
        public string TopicCode { get; set; } = string.Empty;
        public string KoreanTitle { get; set; } = string.Empty;
        public string ShortSummary { get; set; } = string.Empty;
        public string SourceReference { get; set; } = string.Empty;
        public string ViewpointAndLimitations { get; set; } = string.Empty;
    }

    public static class 영역건물발전PresentationProjection
    {
        public static 영역건물발전PresentationModel Map(
            영역건물발전ApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!source.SimulationOnly || source.IsOperationalState)
                throw new InvalidOperationException(
                    "AreaBuildingProgressionAuthorityBoundaryInvalid");
            Require(source.CatalogRevision, "AreaBuildingCatalogRevisionMissing");
            RequireSha256(source.CatalogHashSha256,
                "AreaBuildingCatalogHashInvalid");
            Require(source.AreaCode, "AreaBuildingAreaCodeMissing");

            return new 영역건물발전PresentationModel
            {
                AreaCode = source.AreaCode,
                AreaSetStableId = source.AreaSetStableId,
                CatalogRevision = source.CatalogRevision,
                Nodes = source.Nodes.OrderBy(value => StageOrder(value.StageCode))
                    .ThenBy(value => value.BlueprintStableId,
                        StringComparer.Ordinal)
                    .Select(MapNode).ToArray(),
                TeachingMaterials = source.ApprovedTeachingMaterials
                    .Where(value => value.AdminApproved)
                    .OrderBy(value => value.TeachingMaterialStableId,
                        StringComparer.Ordinal)
                    .Select(value => new 가르침자료PresentationModel
                    {
                        StableId = value.TeachingMaterialStableId,
                        TopicCode = value.TopicCode,
                        KoreanTitle = value.KoreanTitle,
                        ShortSummary = value.ShortSummary,
                        SourceReference = value.SourceReference,
                        ViewpointAndLimitations = value.ViewpointAndLimitations,
                    }).ToArray(),
            };
        }

        private static 건물발전NodePresentationModel MapNode(
            건물발전NodeApiModel source)
        {
            Require(source.BlueprintStableId, "AreaBuildingBlueprintIdMissing");
            Require(source.KoreanName, "AreaBuildingKoreanNameMissing");
            Require(source.StageCode, "AreaBuildingStageMissing");
            Require(source.StateCode, "AreaBuildingStateMissing");
            var progress = source.RequiredWorkSeconds <= 0 ? 0f
                : Math.Max(0f, Math.Min(1f,
                    (float)source.CompletedWorkSeconds / source.RequiredWorkSeconds));
            return new 건물발전NodePresentationModel
            {
                BlueprintStableId = source.BlueprintStableId,
                KoreanName = source.KoreanName,
                StageCode = source.StageCode,
                StateCode = source.StateCode,
                BlockingReasonCodes = source.BlockingReasonCodes.ToArray(),
                IsRecommendedPriority = source.IsDay2Priority,
                WorkProgress01 = progress,
                CompletedLearningVisitCount = source.CompletedLearningVisitCount,
            };
        }

        private static void Require(string value, string code)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(code);
        }

        private static int StageOrder(string stageCode)
            => stageCode switch
            {
                "Foundation" => 0,
                "Operations" => 1,
                "Specialization" => 2,
                "Resilience" => 3,
                "Landmark" => 4,
                _ => int.MaxValue,
            };

        private static void RequireSha256(string value, string code)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 64
                || value.Any(character => !Uri.IsHexDigit(character)))
                throw new InvalidOperationException(code);
        }
    }
}
