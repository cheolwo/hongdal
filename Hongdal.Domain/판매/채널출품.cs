using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.판매;

[Table("채널출품")]
public class 채널출품
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("판매상품_id")]
    public long 판매상품Id { get; set; }

    [Column("판매채널계정_id")]
    public long 판매채널계정Id { get; set; }

    [Column("채널상품번호")]
    [MaxLength(100)]
    public string 채널상품번호 { get; set; } = string.Empty;

    [Column("출품상태")]
    [MaxLength(50)]
    public string 출품상태 { get; set; } = "준비";

    [Column("동기화상태")]
    [MaxLength(50)]
    public string 동기화상태 { get; set; } = "대기";

    [Column("에러메시지")]
    [MaxLength(1000)]
    public string 에러메시지 { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
