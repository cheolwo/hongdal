using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssalddel.Domain.HumanResources;

[Table("hr_role_applications")]
public sealed class HrRoleApplicationRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("applicant_user_id")]
    [MaxLength(450)]
    public string ApplicantUserId { get; set; } = string.Empty;

    [Column("participant_category")]
    [MaxLength(100)]
    public string ParticipantCategory { get; set; } = string.Empty;

    [Column("requested_role_code")]
    [MaxLength(100)]
    public string RequestedRoleCode { get; set; } = string.Empty;

    [Column("requested_role_name")]
    [MaxLength(200)]
    public string RequestedRoleName { get; set; } = string.Empty;

    [Column("scope_type")]
    [MaxLength(100)]
    public string ScopeType { get; set; } = string.Empty;

    [Column("scope_id")]
    [MaxLength(200)]
    public string ScopeId { get; set; } = string.Empty;

    [Column("status_code")]
    [MaxLength(32)]
    public string StatusCode { get; set; } = string.Empty;

    [Column("submission_request_id")]
    public Guid SubmissionRequestId { get; set; }

    [Column("active_application_key")]
    [MaxLength(64)]
    public string? ActiveApplicationKey { get; set; }

    [Column("confirmed_voluntary_application")]
    public bool ConfirmedVoluntaryApplication { get; set; }

    [Column("confirmed_no_role_or_employment_guarantee")]
    public bool ConfirmedNoRoleOrEmploymentGuarantee { get; set; }

    [Column("confirmed_review_data_use")]
    public bool ConfirmedReviewDataUse { get; set; }

    [Column("consent_version")]
    [MaxLength(32)]
    public string ConsentVersion { get; set; } = string.Empty;

    [Column("submitted_at_utc")]
    public DateTime SubmittedAtUtc { get; set; }

    [Column("withdrawn_at_utc")]
    public DateTime? WithdrawnAtUtc { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
