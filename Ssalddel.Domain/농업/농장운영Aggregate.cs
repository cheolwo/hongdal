using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.농업;

[Table("농장")]
public sealed class 농장
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("stable_id")]
    [MaxLength(160)]
    public string StableId { get; set; } = string.Empty;

    [Column("소유자_user_id")]
    [MaxLength(450)]
    public string 소유자UserId { get; set; } = string.Empty;

    [Column("농장명")]
    [MaxLength(200)]
    public string 농장명 { get; set; } = string.Empty;

    [Column("운영상태_code")]
    [MaxLength(40)]
    public string 운영상태Code { get; set; } = 농장운영상태코드.운영중;

    [Column("revision")]
    public long Revision { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<농장구획> 구획들 { get; set; } = [];
    public ICollection<농장작업> 작업들 { get; set; } = [];
}

[Table("농장구획")]
public sealed class 농장구획
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("농장_id")]
    public long 농장Id { get; set; }

    [Column("stable_id")]
    [MaxLength(160)]
    public string StableId { get; set; } = string.Empty;

    [Column("구획명")]
    [MaxLength(120)]
    public string 구획명 { get; set; } = string.Empty;

    [Column("토양관리_profile_code")]
    [MaxLength(80)]
    public string? 토양관리ProfileCode { get; set; }

    [Column("revision")]
    public long Revision { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public 농장 농장 { get; set; } = null!;
    public ICollection<재배작기> 재배작기들 { get; set; } = [];
    public ICollection<농업센서> 센서들 { get; set; } = [];
}

[Table("재배작기")]
public sealed class 재배작기
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("농장구획_id")]
    public long 농장구획Id { get; set; }

    [Column("stable_id")]
    [MaxLength(160)]
    public string StableId { get; set; } = string.Empty;

    [Column("작물명")]
    [MaxLength(120)]
    public string 작물명 { get; set; } = string.Empty;

    [Column("작물기준_stable_id")]
    [MaxLength(200)]
    public string? 작물기준StableId { get; set; }

    [Column("작물기준_source_key")]
    [MaxLength(160)]
    public string? 작물기준SourceKey { get; set; }

    [Column("생육상태_code")]
    [MaxLength(40)]
    public string 생육상태Code { get; set; } = 재배생육상태코드.준비;

    [Column("파종일")]
    public DateOnly? 파종일 { get; set; }

    [Column("예상수확일")]
    public DateOnly? 예상수확일 { get; set; }

    [Column("revision")]
    public long Revision { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public 농장구획 농장구획 { get; set; } = null!;
}

[Table("농업센서")]
public sealed class 농업센서
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("농장구획_id")]
    public long 농장구획Id { get; set; }

    [Column("stable_id")]
    [MaxLength(160)]
    public string StableId { get; set; } = string.Empty;

    [Column("센서유형_code")]
    [MaxLength(60)]
    public string 센서유형Code { get; set; } = string.Empty;

    [Column("상태_code")]
    [MaxLength(40)]
    public string 상태Code { get; set; } = 농업센서상태코드.정상;

    [Column("revision")]
    public long Revision { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public 농장구획 농장구획 { get; set; } = null!;
    public ICollection<농업센서관측> 관측들 { get; set; } = [];
}

[Table("농업센서관측")]
public sealed class 농업센서관측
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("농업센서_id")]
    public long 농업센서Id { get; set; }

    [Column("관측값", TypeName = "decimal(18,4)")]
    public decimal 관측값 { get; set; }

    [Column("단위_code")]
    [MaxLength(40)]
    public string 단위Code { get; set; } = string.Empty;

    [Column("관측시각_utc")]
    public DateTime 관측시각Utc { get; set; }

    [Column("최신성상태_code")]
    [MaxLength(40)]
    public string 최신성상태Code { get; set; } = 센서관측최신성코드.최신;

    [Column("판정상태_code")]
    [MaxLength(40)]
    public string 판정상태Code { get; set; } = 센서관측판정코드.정상;

    [Column("판정규칙_revision")]
    [MaxLength(80)]
    public string 판정규칙Revision { get; set; } = string.Empty;

    [Column("근거카드_id")]
    [MaxLength(160)]
    public string? 근거카드Id { get; set; }

    [Column("확신도_code")]
    [MaxLength(40)]
    public string? 확신도Code { get; set; }

    [Column("판정한계")]
    [MaxLength(500)]
    public string? 판정한계 { get; set; }

    public 농업센서 농업센서 { get; set; } = null!;
}

[Table("농장작업")]
public sealed class 농장작업
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("농장_id")]
    public long 농장Id { get; set; }

    [Column("농장구획_id")]
    public long? 농장구획Id { get; set; }

    [Column("stable_id")]
    [MaxLength(160)]
    public string StableId { get; set; } = string.Empty;

    [Column("npc_stable_id")]
    [MaxLength(160)]
    public string NpcStableId { get; set; } = string.Empty;

    [Column("작업유형_code")]
    [MaxLength(60)]
    public string 작업유형Code { get; set; } = string.Empty;

    [Column("route_code")]
    [MaxLength(80)]
    public string RouteCode { get; set; } = "farm-producer-round";

    [Column("current_waypoint_key")]
    [MaxLength(100)]
    public string CurrentWaypointKey { get; set; } = string.Empty;

    [Column("destination_waypoint_key")]
    [MaxLength(100)]
    public string DestinationWaypointKey { get; set; } = string.Empty;

    [Column("movement_state_code")]
    [MaxLength(40)]
    public string MovementStateCode { get; set; } = "Moving";

    [Column("arrival_action_code")]
    [MaxLength(60)]
    public string ArrivalActionCode { get; set; } = string.Empty;

    [Column("revision")]
    public long Revision { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public 농장 농장 { get; set; } = null!;
    public 농장구획? 농장구획 { get; set; }
}

public static class 농장운영상태코드
{
    public const string 운영중 = "Operating";
    public const string 휴지 = "Inactive";
}

public static class 재배생육상태코드
{
    public const string 준비 = "Preparing";
    public const string 생육중 = "Growing";
    public const string 수확가능 = "HarvestReady";
    public const string 종료 = "Completed";
}

public static class 농업센서상태코드
{
    public const string 정상 = "Online";
    public const string 오프라인 = "Offline";
    public const string 점검필요 = "MaintenanceRequired";
}

public static class 센서관측최신성코드
{
    public const string 최신 = "Current";
    public const string 오래됨 = "Stale";
}

public static class 센서관측판정코드
{
    public const string 정상 = "Normal";
    public const string 건조 = "Dry";
    public const string 위험 = "Critical";
    public const string 과습 = "Waterlogged";
    public const string 판정불가 = "Unknown";
}
