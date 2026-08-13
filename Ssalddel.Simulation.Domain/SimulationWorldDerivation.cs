using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Simulation.Domain
{
public static class SimulationWorld근거종류Codes
{
    public const string 관측 = "Observed";
    public const string 파생 = "Derived";
    public const string 통계배분 = "StatisticallyAllocated";
    public const string 시나리오 = "Scenario";
    public const string 장식 = "Decorative";
}

public static class SimulationWorld건물배치근거Codes
{
    public const string 관측도형 = "ObservedFootprint";
    public const string 관측대표점 = "ObservedRepresentativePoint";
    public const string 영역구성 = "AreaComposition";
    public const string 시나리오 = "Scenario";
}

public static class SimulationWorld그림자정책Codes
{
    public const string 없음 = "None";
    public const string 접지 = "Blob";
    public const string 실시간 = "Realtime";
    public const string 혼합 = "Mixed";
    public const string 원거리통합 = "HlodBaked";
}

public static class SimulationWorldUnity변환상태Codes
{
    public const string 타일Manifest대기 = "WaitingForTileManifest";
    public const string 변환가능 = "Ready";
    public const string 자료부족 = "InsufficientSourceData";
    public const string 실패 = "Failed";
}

public static class SimulationWorldUnity산출물상태Codes
{
    public const string 준비 = "Pending";
    public const string 완료 = "Completed";
    public const string 자료부족 = "InsufficientSourceData";
    public const string 실패 = "Failed";
    public const string 성능예산초과 = "PerformanceBudgetExceeded";
}

public sealed class SimulationWorld파생원장
{
    public int SchemaVersion { get; set; } = 2;
    public string BuildStableId { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string RecipeRevision { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public string? VisualCatalogRevision { get; set; }
    public int Seed { get; set; }
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public IReadOnlyList<SimulationWorld원본계보> Sources { get; set; } =
        Array.Empty<SimulationWorld원본계보>();
    public IReadOnlyList<SimulationWorld파생Node> Nodes { get; set; } =
        Array.Empty<SimulationWorld파생Node>();
    public IReadOnlyList<SimulationWorld파생Relation> Relations { get; set; } =
        Array.Empty<SimulationWorld파생Relation>();
    public IReadOnlyList<SimulationWorld건물배치계획> BuildingPlacements { get; set; } =
        Array.Empty<SimulationWorld건물배치계획>();
    public IReadOnlyList<SimulationWorld그래픽표현계획> GraphicsPlans { get; set; } =
        Array.Empty<SimulationWorld그래픽표현계획>();
    public IReadOnlyList<SimulationWorldUnity공간변환Profile> UnityTransformProfiles { get; set; } =
        Array.Empty<SimulationWorldUnity공간변환Profile>();
    public IReadOnlyList<SimulationWorldUnity타일Manifest> UnityTileManifests { get; set; } =
        Array.Empty<SimulationWorldUnity타일Manifest>();
    public IReadOnlyList<SimulationWorldUnity산출물> UnityArtifacts { get; set; } =
        Array.Empty<SimulationWorldUnity산출물>();
    public IReadOnlyList<SimulationWorld시각배치계획> VisualPlacements { get; set; } =
        Array.Empty<SimulationWorld시각배치계획>();
}

public sealed class SimulationWorld원본계보
{
    public string SourceStableId { get; set; } = string.Empty;
    public string SourceDatabaseCode { get; set; } = string.Empty;
    public string DatasetCode { get; set; } = string.Empty;
    public string SourceRevision { get; set; } = string.Empty;
    public string SourceHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset? ReferenceTimeUtc { get; set; }
}

public sealed class SimulationWorld파생Node
{
    public string StableId { get; set; } = string.Empty;
    public string NodeKindCode { get; set; } = string.Empty;
    public string? SourceStableId { get; set; }
    public string? SourceRecordStableId { get; set; }
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public string? TileKey { get; set; }
    public string? AreaStableId { get; set; }
    public string? DisplayName { get; set; }
    public string? RepresentativeGroupCode { get; set; }
    public int? RepresentedRecordCount { get; set; }
    public int? RepresentativeRank { get; set; }
}

public sealed class SimulationWorld건물배치계획
{
    public string StableId { get; set; } = string.Empty;
    public string AreaNodeStableId { get; set; } = string.Empty;
    public string BuildingNodeStableId { get; set; } = string.Empty;
    public string PlacementBasisCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string BuildingCategoryCode { get; set; } = string.Empty;
    public string VisualFamilyCode { get; set; } = string.Empty;
    public int FloorCount { get; set; }
    public decimal? FootprintAreaSquareMeters { get; set; }
    public decimal? HeightMeters { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal PositionZ { get; set; }
    public decimal RotationY { get; set; }
    public bool PresentationOnly { get; set; } = true;
}

public sealed class SimulationWorld그래픽표현계획
{
    public string StableId { get; set; } = string.Empty;
    public string TargetNodeStableId { get; set; } = string.Empty;
    public string PresentationScopeCode { get; set; } = string.Empty;
    public string TextureSetKey { get; set; } = string.Empty;
    public string MaterialVariantKey { get; set; } = string.Empty;
    public string ColorPaletteKey { get; set; } = string.Empty;
    public string BackgroundProfileKey { get; set; } = string.Empty;
    public string LightingProfileKey { get; set; } = string.Empty;
    public string TimeOfDayProfileKey { get; set; } = string.Empty;
    public string ShadowPolicyCode { get; set; } = string.Empty;
    public bool CastShadows { get; set; }
    public bool ReceiveShadows { get; set; }
    public decimal ContactShadowStrength { get; set; }
    public decimal? ShadowDistanceMeters { get; set; }
    public decimal AmbientOcclusionStrength { get; set; }
    public string LodCode { get; set; } = string.Empty;
    public string QualityTierCode { get; set; } = string.Empty;
    public bool PresentationOnly { get; set; } = true;
}

public sealed class SimulationWorldUnity공간변환Profile
{
    public string StableId { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string SourceCrsCode { get; set; } = string.Empty;
    public string AxisMappingCode { get; set; } = string.Empty;
    public decimal? OriginEastingMeters { get; set; }
    public decimal? OriginNorthingMeters { get; set; }
    public decimal? ReferenceElevationMeters { get; set; }
    public decimal HorizontalScale { get; set; } = 1m;
    public decimal VerticalExaggeration { get; set; } = 1m;
    public decimal MetersPerUnityUnit { get; set; } = 1m;
    public string RuleRevision { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string ProfileHashSha256 { get; set; } = string.Empty;
}

public sealed class SimulationWorldUnity타일Manifest
{
    public string StableId { get; set; } = string.Empty;
    public string TransformProfileStableId { get; set; } = string.Empty;
    public string TileKey { get; set; } = string.Empty;
    public int Level { get; set; }
    public decimal SizeMeters { get; set; }
    public decimal HaloMeters { get; set; }
    public decimal MinEastingMeters { get; set; }
    public decimal MinNorthingMeters { get; set; }
    public decimal MaxEastingMeters { get; set; }
    public decimal MaxNorthingMeters { get; set; }
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public string ManifestHashSha256 { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
}

public sealed class SimulationWorldUnity산출물
{
    public string StableId { get; set; } = string.Empty;
    public string TileManifestStableId { get; set; } = string.Empty;
    public string ArtifactKindCode { get; set; } = string.Empty;
    public string LodCode { get; set; } = string.Empty;
    public string? StorageObjectKey { get; set; }
    public string? ArtifactHashSha256 { get; set; }
    public long? VertexCount { get; set; }
    public long? TriangleCount { get; set; }
    public int? MaterialSlotCount { get; set; }
    public int? EstimatedDrawCallCount { get; set; }
    public string? BoundaryVertexHashSha256 { get; set; }
    public string StatusCode { get; set; } = string.Empty;
}

public sealed class SimulationWorld파생Relation
{
    public string StableId { get; set; } = string.Empty;
    public string FromNodeStableId { get; set; } = string.Empty;
    public string RelationCode { get; set; } = string.Empty;
    public string ToNodeStableId { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string? SourceStableId { get; set; }
    public decimal Confidence { get; set; }
}

public sealed class SimulationWorld시각배치계획
{
    public string StableId { get; set; } = string.Empty;
    public string TargetNodeStableId { get; set; } = string.Empty;
    public string VisualKey { get; set; } = string.Empty;
    public string LodCode { get; set; } = string.Empty;
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal PositionZ { get; set; }
    public decimal RotationY { get; set; }
    public decimal UniformScale { get; set; } = 1m;
    public bool PresentationOnly { get; set; } = true;
}

public static class SimulationWorld파생원장Validator
{
    public const string InvalidCode = "SimulationWorldDerivationInvalid";

    public static void Validate(SimulationWorld파생원장 ledger)
    {
        if (ledger == null)
            throw new ArgumentNullException(nameof(ledger));
        Require(ledger.SchemaVersion is 1 or 2, "지원하지 않는 파생 원장 schema입니다.");
        RequireText(ledger.BuildStableId, "파생 실행 식별자");
        RequireText(ledger.AreaSetStableId, "AreaSet 식별자");
        RequireText(ledger.RecipeRevision, "Recipe 개정 번호");
        RequireText(ledger.RuleRevision, "규칙 개정 번호");
        if (ledger.SchemaVersion == 1)
        {
            RequireText(ledger.VisualCatalogRevision, "시각 자산 대장 개정 번호");
        }
        else
        {
            Require(string.IsNullOrWhiteSpace(ledger.VisualCatalogRevision),
                "공간 실행본에는 시각 자산 대장 개정 번호를 저장할 수 없습니다.");
            Require(ledger.GraphicsPlans.Count == 0,
                "공간 실행본에는 그래픽 표현 계획을 저장할 수 없습니다.");
            Require(ledger.VisualPlacements.Count == 0,
                "공간 실행본에는 Synty 시각 배치 계획을 저장할 수 없습니다.");
        }
        RequireSha256(ledger.InputFingerprintSha256, "입력 fingerprint");
        Require(ledger.GeneratedAtUtc != default, "생성 시각이 필요합니다.");
        Require(ledger.Sources.Count > 0, "최소 한 개의 원본 계보가 필요합니다.");

        RequireDistinct(ledger.Sources.Select(item => item.SourceStableId), "원본 계보");
        foreach (var source in ledger.Sources)
        {
            RequireText(source.SourceStableId, "원본 계보 식별자");
            RequireText(source.SourceDatabaseCode, "원본 DB 코드");
            RequireText(source.DatasetCode, "원본 자료 코드");
            RequireText(source.SourceRevision, "원본 개정 번호");
            RequireSha256(source.SourceHashSha256, "원본 SHA-256");
        }

        RequireDistinct(ledger.Nodes.Select(item => item.StableId), "파생 node");
        var nodeIds = new HashSet<string>(ledger.Nodes.Select(item => item.StableId), StringComparer.Ordinal);
        var sourceIds = new HashSet<string>(ledger.Sources.Select(item => item.SourceStableId), StringComparer.Ordinal);
        foreach (var node in ledger.Nodes)
        {
            RequireText(node.StableId, "파생 node 식별자");
            RequireText(node.NodeKindCode, "파생 node 종류");
            RequireEvidence(node.EvidenceKindCode);
            Require(node.SourceStableId == null || sourceIds.Contains(node.SourceStableId),
                "파생 node의 원본 계보 참조가 유효하지 않습니다.");
            Require(node.SourceRecordStableId == null || node.SourceStableId != null,
                "원본 레코드 식별자가 있으면 원본 계보 참조도 필요합니다.");
            var hasRepresentativeGroup = !string.IsNullOrWhiteSpace(node.RepresentativeGroupCode);
            Require(hasRepresentativeGroup == node.RepresentedRecordCount.HasValue
                && hasRepresentativeGroup == node.RepresentativeRank.HasValue,
                "대표 객체 node는 대표군·대표 원본 건수·대표 순위를 함께 가져야 합니다.");
            Require(node.RepresentedRecordCount == null || node.RepresentedRecordCount > 0,
                "대표 원본 건수는 0보다 커야 합니다.");
            Require(node.RepresentativeRank == null || node.RepresentativeRank > 0,
                "대표 순위는 0보다 커야 합니다.");
        }

        RequireDistinct(ledger.Relations.Select(item => item.StableId), "파생 관계");
        foreach (var relation in ledger.Relations)
        {
            RequireText(relation.StableId, "파생 관계 식별자");
            RequireText(relation.RelationCode, "파생 관계 코드");
            Require(nodeIds.Contains(relation.FromNodeStableId), "관계 시작 node가 없습니다.");
            Require(nodeIds.Contains(relation.ToNodeStableId), "관계 도착 node가 없습니다.");
            RequireEvidence(relation.EvidenceKindCode);
            Require(relation.SourceStableId == null || sourceIds.Contains(relation.SourceStableId),
                "파생 관계의 원본 계보 참조가 유효하지 않습니다.");
            Require(relation.Confidence >= 0m && relation.Confidence <= 1m,
                "관계 신뢰도는 0~1이어야 합니다.");
        }

        RequireDistinct(ledger.BuildingPlacements.Select(item => item.StableId), "건물 배치 계획");
        foreach (var placement in ledger.BuildingPlacements)
        {
            RequireText(placement.StableId, "건물 배치 계획 식별자");
            Require(nodeIds.Contains(placement.AreaNodeStableId), "건물 배치 대상 영역 node가 없습니다.");
            Require(nodeIds.Contains(placement.BuildingNodeStableId), "건물 배치 대상 건물 node가 없습니다.");
            RequireBuildingPlacementBasis(placement.PlacementBasisCode);
            RequireEvidence(placement.EvidenceKindCode);
            RequireText(placement.BuildingCategoryCode, "건물 분류 코드");
            RequireText(placement.VisualFamilyCode, "건물 시각 Family 코드");
            Require(placement.FloorCount > 0, "표현 층수는 0보다 커야 합니다.");
            Require(placement.FootprintAreaSquareMeters == null || placement.FootprintAreaSquareMeters > 0m,
                "건물 footprint 면적은 0보다 커야 합니다.");
            Require(placement.HeightMeters == null || placement.HeightMeters > 0m,
                "건물 높이는 0보다 커야 합니다.");
            Require(placement.PresentationOnly, "건물 배치 계획은 표현 전용이어야 합니다.");
        }

        RequireDistinct(ledger.GraphicsPlans.Select(item => item.StableId), "그래픽 표현 계획");
        foreach (var plan in ledger.GraphicsPlans)
        {
            RequireText(plan.StableId, "그래픽 표현 계획 식별자");
            Require(nodeIds.Contains(plan.TargetNodeStableId), "그래픽 표현 대상 node가 없습니다.");
            RequireText(plan.PresentationScopeCode, "그래픽 표현 범위 코드");
            RequireSemanticKey(plan.TextureSetKey, "질감 세트 키");
            RequireSemanticKey(plan.MaterialVariantKey, "재질 변형 키");
            RequireSemanticKey(plan.ColorPaletteKey, "색조 팔레트 키");
            RequireSemanticKey(plan.BackgroundProfileKey, "배경 Profile 키");
            RequireSemanticKey(plan.LightingProfileKey, "조명 Profile 키");
            RequireSemanticKey(plan.TimeOfDayProfileKey, "시간대 Profile 키");
            RequireShadowPolicy(plan.ShadowPolicyCode);
            Require(plan.ContactShadowStrength >= 0m && plan.ContactShadowStrength <= 1m,
                "접지 그림자 강도는 0~1이어야 합니다.");
            Require(plan.ShadowDistanceMeters == null || plan.ShadowDistanceMeters > 0m,
                "그림자 거리는 0보다 커야 합니다.");
            Require(plan.AmbientOcclusionStrength >= 0m && plan.AmbientOcclusionStrength <= 1m,
                "주변광 차폐 강도는 0~1이어야 합니다.");
            RequireText(plan.LodCode, "그래픽 LOD 코드");
            RequireText(plan.QualityTierCode, "그래픽 품질 단계 코드");
            Require(plan.PresentationOnly, "그래픽 표현 계획은 표현 전용이어야 합니다.");
        }

        RequireDistinct(ledger.UnityTransformProfiles.Select(item => item.StableId), "Unity 공간 변환 Profile");
        var transformIds = new HashSet<string>(
            ledger.UnityTransformProfiles.Select(item => item.StableId), StringComparer.Ordinal);
        foreach (var profile in ledger.UnityTransformProfiles)
        {
            RequireText(profile.StableId, "Unity 공간 변환 Profile 식별자");
            Require(profile.AreaSetStableId == ledger.AreaSetStableId,
                "Unity 공간 변환 Profile의 AreaSet이 파생 실행본과 일치해야 합니다.");
            RequireText(profile.SourceCrsCode, "원본 좌표계 코드");
            RequireText(profile.AxisMappingCode, "좌표축 변환 코드");
            Require(profile.HorizontalScale > 0m, "수평 축척률은 0보다 커야 합니다.");
            Require(profile.VerticalExaggeration > 0m, "높이 과장률은 0보다 커야 합니다.");
            Require(profile.MetersPerUnityUnit > 0m, "Unity 단위당 미터는 0보다 커야 합니다.");
            RequireText(profile.RuleRevision, "Unity 공간 변환 규칙 개정 번호");
            RequireUnityTransformStatus(profile.StatusCode);
            RequireSha256(profile.ProfileHashSha256, "Unity 공간 변환 Profile SHA-256");
            if (profile.StatusCode == SimulationWorldUnity변환상태Codes.변환가능)
            {
                Require(profile.OriginEastingMeters != null && profile.OriginNorthingMeters != null,
                    "변환 가능한 Profile에는 Unity 원점 좌표가 필요합니다.");
                Require(profile.ReferenceElevationMeters != null,
                    "변환 가능한 Profile에는 기준 표고가 필요합니다.");
            }
        }

        RequireDistinct(ledger.UnityTileManifests.Select(item => item.StableId), "Unity 타일 Manifest");
        var tileManifestIds = new HashSet<string>(
            ledger.UnityTileManifests.Select(item => item.StableId), StringComparer.Ordinal);
        foreach (var manifest in ledger.UnityTileManifests)
        {
            RequireText(manifest.StableId, "Unity 타일 Manifest 식별자");
            Require(transformIds.Contains(manifest.TransformProfileStableId),
                "Unity 타일 Manifest의 공간 변환 Profile이 없습니다.");
            RequireText(manifest.TileKey, "Unity 타일 키");
            Require(manifest.Level is >= 0 and <= 2, "Unity 타일 단계는 L0~L2여야 합니다.");
            Require(manifest.SizeMeters > 0m && manifest.HaloMeters >= 0m,
                "Unity 타일 크기와 Halo가 유효하지 않습니다.");
            Require(manifest.MaxEastingMeters > manifest.MinEastingMeters
                    && manifest.MaxNorthingMeters > manifest.MinNorthingMeters,
                "Unity 타일 경계가 유효하지 않습니다.");
            RequireSha256(manifest.InputFingerprintSha256, "Unity 타일 입력 fingerprint");
            RequireSha256(manifest.ManifestHashSha256, "Unity 타일 Manifest SHA-256");
            RequireUnityTransformStatus(manifest.StatusCode);
        }

        RequireDistinct(ledger.UnityArtifacts.Select(item => item.StableId), "Unity 산출물");
        foreach (var artifact in ledger.UnityArtifacts)
        {
            RequireText(artifact.StableId, "Unity 산출물 식별자");
            Require(tileManifestIds.Contains(artifact.TileManifestStableId),
                "Unity 산출물의 타일 Manifest가 없습니다.");
            RequireText(artifact.ArtifactKindCode, "Unity 산출물 종류 코드");
            RequireText(artifact.LodCode, "Unity 산출물 LOD 코드");
            RequireUnityArtifactStatus(artifact.StatusCode);
            Require(artifact.ArtifactHashSha256 == null
                    || artifact.ArtifactHashSha256.Length == 64 && artifact.ArtifactHashSha256.All(Uri.IsHexDigit),
                "Unity 산출물 SHA-256이 유효하지 않습니다.");
            Require(artifact.BoundaryVertexHashSha256 == null
                    || artifact.BoundaryVertexHashSha256.Length == 64 && artifact.BoundaryVertexHashSha256.All(Uri.IsHexDigit),
                "경계 정점 SHA-256이 유효하지 않습니다.");
            Require(artifact.VertexCount == null || artifact.VertexCount >= 0,
                "정점 수는 음수일 수 없습니다.");
            Require(artifact.TriangleCount == null || artifact.TriangleCount >= 0,
                "삼각형 수는 음수일 수 없습니다.");
            if (artifact.StatusCode == SimulationWorldUnity산출물상태Codes.완료)
            {
                RequireText(artifact.StorageObjectKey, "완료 Unity 산출물 저장 객체 키");
                Require(artifact.ArtifactHashSha256 != null, "완료 Unity 산출물 SHA-256이 필요합니다.");
            }
        }

        RequireDistinct(ledger.VisualPlacements.Select(item => item.StableId), "시각 배치 계획");
        foreach (var placement in ledger.VisualPlacements)
        {
            RequireText(placement.StableId, "시각 배치 식별자");
            Require(nodeIds.Contains(placement.TargetNodeStableId), "시각 배치 대상 node가 없습니다.");
            RequireText(placement.VisualKey, "VisualKey");
            Require(!placement.VisualKey.Contains("/") && !placement.VisualKey.Contains("\\"),
                "VisualKey에는 Prefab 경로를 저장할 수 없습니다.");
            RequireText(placement.LodCode, "LOD 코드");
            Require(placement.UniformScale > 0m, "시각 배치 축척은 0보다 커야 합니다.");
            Require(placement.PresentationOnly, "시각 배치는 표현 전용이어야 합니다.");
        }
    }

    private static void RequireEvidence(string code) =>
        Require(code == SimulationWorld근거종류Codes.관측
                || code == SimulationWorld근거종류Codes.파생
                || code == SimulationWorld근거종류Codes.통계배분
                || code == SimulationWorld근거종류Codes.시나리오
                || code == SimulationWorld근거종류Codes.장식,
            "지원하지 않는 근거 종류입니다.");

    private static void RequireBuildingPlacementBasis(string code) =>
        Require(code == SimulationWorld건물배치근거Codes.관측도형
                || code == SimulationWorld건물배치근거Codes.관측대표점
                || code == SimulationWorld건물배치근거Codes.영역구성
                || code == SimulationWorld건물배치근거Codes.시나리오,
            "지원하지 않는 건물 배치 근거입니다.");

    private static void RequireShadowPolicy(string code) =>
        Require(code == SimulationWorld그림자정책Codes.없음
                || code == SimulationWorld그림자정책Codes.접지
                || code == SimulationWorld그림자정책Codes.실시간
                || code == SimulationWorld그림자정책Codes.혼합
                || code == SimulationWorld그림자정책Codes.원거리통합,
            "지원하지 않는 그림자 정책입니다.");

    private static void RequireUnityTransformStatus(string code) =>
        Require(code == SimulationWorldUnity변환상태Codes.타일Manifest대기
                || code == SimulationWorldUnity변환상태Codes.변환가능
                || code == SimulationWorldUnity변환상태Codes.자료부족
                || code == SimulationWorldUnity변환상태Codes.실패,
            "지원하지 않는 Unity 공간 변환 상태입니다.");

    private static void RequireUnityArtifactStatus(string code) =>
        Require(code == SimulationWorldUnity산출물상태Codes.준비
                || code == SimulationWorldUnity산출물상태Codes.완료
                || code == SimulationWorldUnity산출물상태Codes.자료부족
                || code == SimulationWorldUnity산출물상태Codes.실패
                || code == SimulationWorldUnity산출물상태Codes.성능예산초과,
            "지원하지 않는 Unity 산출물 상태입니다.");

    private static void RequireSemanticKey(string value, string name)
    {
        RequireText(value, name);
        Require(!value.Contains("/") && !value.Contains("\\"),
            name + "에는 자산 파일 경로를 저장할 수 없습니다.");
    }

    private static void RequireDistinct(IEnumerable<string> values, string name)
    {
        var items = values.ToArray();
        Require(items.Distinct(StringComparer.Ordinal).Count() == items.Length,
            name + " 식별자가 중복되었습니다.");
    }

    private static void RequireText(string? value, string name) =>
        Require(!string.IsNullOrWhiteSpace(value), name + "이(가) 필요합니다.");

    private static void RequireSha256(string value, string name) =>
        Require(value != null
                && value.Length == 64
                && value.All(Uri.IsHexDigit),
            name + "은(는) 64자리 SHA-256이어야 합니다.");

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(InvalidCode + ":" + message);
    }
}

public static class SimulationWorld파생원장Hash
{
    public static string Compute(SimulationWorld파생원장 ledger)
    {
        SimulationWorld파생원장Validator.Validate(ledger);
        var canonical = new StringBuilder()
            .Append(ledger.SchemaVersion).Append('|')
            .Append(ledger.BuildStableId).Append('|')
            .Append(ledger.AreaSetStableId).Append('|')
            .Append(ledger.RecipeRevision).Append('|')
            .Append(ledger.RuleRevision).Append('|');
        if (ledger.SchemaVersion == 1)
            canonical.Append(ledger.VisualCatalogRevision).Append('|');
        canonical
            .Append(ledger.Seed.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(ledger.InputFingerprintSha256.ToLowerInvariant());
        foreach (var source in ledger.Sources.OrderBy(item => item.SourceStableId, StringComparer.Ordinal))
            canonical.Append("|S:").Append(source.SourceStableId).Append(':')
                .Append(source.SourceDatabaseCode).Append(':').Append(source.DatasetCode).Append(':')
                .Append(source.SourceRevision).Append(':').Append(source.SourceHashSha256.ToLowerInvariant())
                .Append(':').Append(source.ReferenceTimeUtc.HasValue
                    ? source.ReferenceTimeUtc.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
                    : string.Empty);
        foreach (var node in ledger.Nodes.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|N:").Append(node.StableId).Append(':').Append(node.NodeKindCode).Append(':')
                .Append(node.SourceStableId).Append(':').Append(node.SourceRecordStableId).Append(':')
                .Append(node.EvidenceKindCode).Append(':')
                .Append(node.RegionCode).Append(':').Append(node.TileKey).Append(':').Append(node.AreaStableId)
                .Append(':').Append(node.DisplayName).Append(':').Append(node.RepresentativeGroupCode)
                .Append(':').Append(node.RepresentedRecordCount?.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(node.RepresentativeRank?.ToString(CultureInfo.InvariantCulture));
        foreach (var relation in ledger.Relations.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|R:").Append(relation.StableId).Append(':').Append(relation.FromNodeStableId)
                .Append(':').Append(relation.RelationCode).Append(':').Append(relation.ToNodeStableId)
                .Append(':').Append(relation.EvidenceKindCode).Append(':').Append(relation.SourceStableId)
                .Append(':').Append(relation.Confidence.ToString(CultureInfo.InvariantCulture));
        foreach (var placement in ledger.BuildingPlacements.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|B:").Append(placement.StableId).Append(':').Append(placement.AreaNodeStableId)
                .Append(':').Append(placement.BuildingNodeStableId).Append(':').Append(placement.PlacementBasisCode)
                .Append(':').Append(placement.EvidenceKindCode).Append(':').Append(placement.BuildingCategoryCode)
                .Append(':').Append(placement.VisualFamilyCode).Append(':')
                .Append(placement.FloorCount.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(placement.FootprintAreaSquareMeters?.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(placement.HeightMeters?.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(placement.PositionX.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(placement.PositionY.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(placement.PositionZ.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(placement.RotationY.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(placement.PresentationOnly ? "1" : "0");
        foreach (var plan in ledger.GraphicsPlans.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|G:").Append(plan.StableId).Append(':').Append(plan.TargetNodeStableId)
                .Append(':').Append(plan.PresentationScopeCode).Append(':').Append(plan.TextureSetKey)
                .Append(':').Append(plan.MaterialVariantKey).Append(':').Append(plan.ColorPaletteKey)
                .Append(':').Append(plan.BackgroundProfileKey).Append(':').Append(plan.LightingProfileKey)
                .Append(':').Append(plan.TimeOfDayProfileKey).Append(':').Append(plan.ShadowPolicyCode)
                .Append(':').Append(plan.CastShadows ? "1" : "0").Append(':')
                .Append(plan.ReceiveShadows ? "1" : "0").Append(':')
                .Append(plan.ContactShadowStrength.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(plan.ShadowDistanceMeters?.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(plan.AmbientOcclusionStrength.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(plan.LodCode).Append(':').Append(plan.QualityTierCode).Append(':')
                .Append(plan.PresentationOnly ? "1" : "0");
        foreach (var profile in ledger.UnityTransformProfiles.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|UT:").Append(profile.StableId).Append(':').Append(profile.AreaSetStableId)
                .Append(':').Append(profile.SourceCrsCode).Append(':').Append(profile.AxisMappingCode)
                .Append(':').Append(profile.OriginEastingMeters?.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(profile.OriginNorthingMeters?.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(profile.ReferenceElevationMeters?.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(profile.HorizontalScale.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(profile.VerticalExaggeration.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(profile.MetersPerUnityUnit.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(profile.RuleRevision).Append(':').Append(profile.StatusCode)
                .Append(':').Append(profile.ProfileHashSha256.ToLowerInvariant());
        foreach (var manifest in ledger.UnityTileManifests.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|UM:").Append(manifest.StableId).Append(':').Append(manifest.TransformProfileStableId)
                .Append(':').Append(manifest.TileKey).Append(':').Append(manifest.Level)
                .Append(':').Append(manifest.SizeMeters.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(manifest.HaloMeters.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(manifest.MinEastingMeters.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(manifest.MinNorthingMeters.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(manifest.MaxEastingMeters.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(manifest.MaxNorthingMeters.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(manifest.InputFingerprintSha256.ToLowerInvariant())
                .Append(':').Append(manifest.ManifestHashSha256.ToLowerInvariant()).Append(':').Append(manifest.StatusCode);
        foreach (var artifact in ledger.UnityArtifacts.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|UA:").Append(artifact.StableId).Append(':').Append(artifact.TileManifestStableId)
                .Append(':').Append(artifact.ArtifactKindCode).Append(':').Append(artifact.LodCode)
                .Append(':').Append(artifact.StorageObjectKey).Append(':').Append(artifact.ArtifactHashSha256)
                .Append(':').Append(artifact.VertexCount).Append(':').Append(artifact.TriangleCount)
                .Append(':').Append(artifact.MaterialSlotCount).Append(':').Append(artifact.EstimatedDrawCallCount)
                .Append(':').Append(artifact.BoundaryVertexHashSha256).Append(':').Append(artifact.StatusCode);
        foreach (var placement in ledger.VisualPlacements.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|V:").Append(placement.StableId).Append(':').Append(placement.TargetNodeStableId)
                .Append(':').Append(placement.VisualKey).Append(':').Append(placement.LodCode)
                .Append(':').Append(placement.PositionX.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.PositionY.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.PositionZ.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.RotationY.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.UniformScale.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.PresentationOnly ? "1" : "0");
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
}
