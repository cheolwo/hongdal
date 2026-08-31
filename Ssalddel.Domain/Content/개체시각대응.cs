namespace Ssalddel.Domain.Content;

/// <summary>업무 원천과 별개인 표현 검토 상태. 대상 식별은 권한별 서버 조회에서만 얻는다.</summary>
public sealed class 개체시각대응
{
    public string BindingId { get; set; } = "";
    public string ContextHash { get; set; } = "";
    public long Revision { get; set; }
    public string Kind { get; set; } = "";
    public string AccessScope { get; set; } = "";
    public string ReviewState { get; set; } = "";
    public string StateJson { get; set; } = "";
    // D431: 기존 JSON 사본은 유지하며 DB 자산판본과 조회 가능한 문맥을 명시한다.
    public string? AssetVersionId { get; set; }
    public string? SourceKey { get; set; }
    public string? SourceStableId { get; set; }
    public string? SourceRevision { get; set; }
    public string? StateCode { get; set; }
    public string? Purpose { get; set; }
    public string? Representation { get; set; }
    public bool? TypeDefault { get; set; }
}

/// <summary>파일로 확인한 불변 자산판본. 등록은 외형 적합성 승인이나 실제 배치가 아니다.</summary>
public sealed class 개체시각자산판본
{
    public string AssetVersionId { get; set; } = "";
    public string VisualKey { get; set; } = "";
    public string CatalogRevision { get; set; } = "";
    public string PrefabGuid { get; set; } = "";
    public string MetadataHash { get; set; } = "";
    public string MetadataJson { get; set; } = "";
    public string VerificationState { get; set; } = "FileVerified_FitnessUnreviewed";
    public string RegisteredBy { get; set; } = "";
    public DateTime RegisteredAtUtc { get; set; }
}

public sealed class 개체시각대응이력
{
    public string RequestKeyHash { get; set; } = "";
    public string BindingId { get; set; } = "";
    public long Revision { get; set; }
    public string RequestHash { get; set; } = "";
    public string ReviewerId { get; set; } = "";
    public string Action { get; set; } = "";
    public string Note { get; set; } = "";
    public DateTime AtUtc { get; set; }
    public string StateJson { get; set; } = "";
}
