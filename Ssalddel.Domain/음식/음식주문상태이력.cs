using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.음식;

[Table("음식주문상태이력")]
public class 음식주문상태이력
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("음식주문_id")]
    public long 음식주문Id { get; set; }

    [Column("이전상태")]
    [MaxLength(50)]
    public string 이전상태 { get; set; } = string.Empty;

    [Column("다음상태")]
    [MaxLength(50)]
    public string 다음상태 { get; set; } = string.Empty;

    [Column("사유")]
    [MaxLength(200)]
    public string 사유 { get; set; } = string.Empty;

    [Column("전이시각_utc")]
    public DateTime 전이시각Utc { get; set; } = DateTime.UtcNow;

    public 음식주문? 음식주문 { get; set; }
}
