namespace Ssalddel.Domain.Content;

/// <summary>기존 표현 후보와 별개인 파일 조사 사본. 실제 자산 권위/게임 선택은 아니다.</summary>
public sealed class 보유시각자산사본
{
    public string SnapshotId { get; set; } = "";
    public string Guid { get; set; } = "";
    public string SurveyRevision { get; set; } = "";
    public string ContentVersionId { get; set; } = "";
    public string SourceGroup { get; set; } = "";
    public string? PackCode { get; set; }
    public string AssetKind { get; set; } = "";
    public string Name { get; set; } = "";
    public string RelativePath { get; set; } = "";
    public string MetadataJson { get; set; } = "";
    public string MetadataHash { get; set; } = "";
    public string EvidenceRef { get; set; } = "";
    public string EvidenceHash { get; set; } = "";
    public string RegisteredBy { get; set; } = "";
    public DateTime RegisteredAtUtc { get; set; }
}

public sealed class 보유시각자산후보연결
{
    public string SnapshotId { get; set; } = "";
    public string AssetVersionId { get; set; } = "";
}

/// <summary>기존 분류 원본의 판본화 연결. 조사 MetadataJson을 수정하지 않는다.</summary>
public sealed class 보유시각분류주석
{
    public string AnnotationId { get; set; } = "";
    public string SnapshotId { get; set; } = "";
    public string ContentVersionId { get; set; } = "";
    public string TaxonomyHash { get; set; } = "";
    public string TaxonomyPath { get; set; } = "";
    public string State { get; set; } = "";
    public string Traits { get; set; } = "";
    public string InputJson { get; set; } = "";
    public string InputHash { get; set; } = "";
    public string RegisteredBy { get; set; } = "";
    public DateTime RegisteredAtUtc { get; set; }
}
