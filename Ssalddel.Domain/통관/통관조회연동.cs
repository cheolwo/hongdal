using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.통관;

[Table("통관조회연동")]
public class 통관조회연동
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("주문_id")]
    public long 주문Id { get; set; }

    [Column("사용자_id")]
    [MaxLength(450)]
    public string 사용자Id { get; set; } = string.Empty;

    [Column("통관절차_id")]
    public long 통관절차Id { get; set; }

    [Column("개인통관고유부호_암호문")]
    [MaxLength(1000)]
    public string? 개인통관고유부호암호문 { get; set; }

    [Column("화물관리번호")]
    [MaxLength(100)]
    public string? 화물관리번호 { get; set; }

    [Column("master_bl")]
    [MaxLength(100)]
    public string? MasterBl { get; set; }

    [Column("house_bl")]
    [MaxLength(100)]
    public string? HouseBl { get; set; }

    [Column("사용자조회동의여부")]
    public bool 사용자조회동의여부 { get; set; }

    [Column("동의시각")]
    public DateTimeOffset? 동의시각 { get; set; }

    [Column("연동상태")]
    public 통관연동상태 연동상태 { get; set; } = 통관연동상태.미등록;

    [Column("마지막진행단계")]
    public 통관진행단계 마지막진행단계 { get; set; } = 통관진행단계.알수없음;

    [Column("마지막조회시각")]
    public DateTimeOffset? 마지막조회시각 { get; set; }

    [Column("마지막오류")]
    [MaxLength(1000)]
    public string? 마지막오류 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
