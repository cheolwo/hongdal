using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.통관;

[Table("통관수임")]
public class 통관수임
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("통관절차_id")]
    public long 통관절차Id { get; set; }

    [Column("관세사_참여자_id")]
    [MaxLength(450)]
    public string 관세사참여자Id { get; set; } = string.Empty;

    [Column("상태")]
    public 통관수임상태 상태 { get; set; } = 통관수임상태.수임요청;

    [Column("요청시각")]
    public DateTimeOffset 요청시각 { get; set; } = DateTimeOffset.UtcNow;

    [Column("확정시각")]
    public DateTimeOffset? 확정시각 { get; set; }

    [Column("메모")]
    [MaxLength(1000)]
    public string? 메모 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
