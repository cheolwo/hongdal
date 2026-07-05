using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.사용자;

[Table("홍달참여자")]
public class 홍달참여자
{
    [Key]
    [Column("id")]
    [MaxLength(450)]
    public string Id { get; set; } = string.Empty;

    [Column("표시이름")]
    [MaxLength(100)]
    public string 표시이름 { get; set; } = string.Empty;

    [Column("가입시각")]
    public DateTimeOffset 가입시각 { get; set; } = DateTimeOffset.UtcNow;

    [Column("활성화여부")]
    public bool 활성화여부 { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<홍달참여자역할> 역할목록 { get; set; } = new List<홍달참여자역할>();
}
