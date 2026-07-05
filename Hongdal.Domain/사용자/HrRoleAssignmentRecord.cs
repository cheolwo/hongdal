using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.사용자;

[Table("hr_role_assignments")]
public sealed class HrRoleAssignmentRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Column("scope_type")]
    [MaxLength(100)]
    public string ScopeType { get; set; } = string.Empty;

    [Column("scope_id")]
    [MaxLength(200)]
    public string ScopeId { get; set; } = string.Empty;

    [Column("participant_category")]
    [MaxLength(100)]
    public string ParticipantCategory { get; set; } = string.Empty;

    [Column("role_code")]
    [MaxLength(100)]
    public string RoleCode { get; set; } = string.Empty;

    [Column("role_name")]
    [MaxLength(200)]
    public string RoleName { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("assigned_at_utc")]
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("assigned_by_user_id")]
    [MaxLength(450)]
    public string AssignedByUserId { get; set; } = string.Empty;

    [Column("work_schedule_enabled")]
    public bool WorkScheduleEnabled { get; set; }

    [Column("time_zone_id")]
    [MaxLength(100)]
    public string TimeZoneId { get; set; } = "Asia/Seoul";

    [Column("allowed_days_of_week")]
    [MaxLength(100)]
    public string AllowedDaysOfWeekCsv { get; set; } = string.Empty;

    [Column("work_start_local_time")]
    [MaxLength(16)]
    public string? WorkStartLocalTimeText { get; set; }

    [Column("work_end_local_time")]
    [MaxLength(16)]
    public string? WorkEndLocalTimeText { get; set; }

    [Column("worksite_ip_restriction_enabled")]
    public bool WorksiteIpRestrictionEnabled { get; set; }

    [Column("allowed_worksite_ip_ranges")]
    [MaxLength(2000)]
    public string AllowedWorksiteIpRangesText { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
