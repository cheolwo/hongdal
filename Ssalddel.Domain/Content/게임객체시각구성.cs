namespace Ssalddel.Domain.Content;

/// <summary>편집용 정의의 현재 판본 포인터. 실제 World/Session 객체나 운영 시설이 아니다.</summary>
public sealed class 게임객체시각정의
{
    public string DefinitionId { get; set; } = "";
    public long Revision { get; set; }
}

/// <summary>이전 판본은 수정하지 않는다. 현재 제품 적용/승인 포인터를 제공하지 않는다.</summary>
public sealed class 게임객체시각구성판본
{
    public string CompositionId { get; set; } = "";
    public string DefinitionId { get; set; } = "";
    public long Revision { get; set; }
    public string SnapshotJson { get; set; } = "";
    public string SnapshotHash { get; set; } = "";
    public string ReviewerId { get; set; } = "";
    public DateTime AtUtc { get; set; }
}

public sealed class 게임객체시각구성항목
{
    public string CompositionId { get; set; } = "";
    public string ItemId { get; set; } = "";
    public string Role { get; set; } = "";
    public string SlotKey { get; set; } = "";
    public string? AssetVersionId { get; set; }
    public string? AnchorIntent { get; set; }
    public string? InventorySnapshotId { get; set; }
    public string? SelectionEvidenceJson { get; set; }
}

public sealed class 게임객체시각구성이력
{
    public string RequestKeyHash { get; set; } = "";
    public string RequestHash { get; set; } = "";
    public string CompositionId { get; set; } = "";
}
