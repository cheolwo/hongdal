using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.마트;

/// <summary>
/// 공개 상품에서 사용자가 직접 제출한 비구속 주문 요청입니다.
/// 출고·재고 예약·결제 원장과 분리해 주문 의향과 서버 계산 스냅샷만 보존합니다.
/// </summary>
[Table("마트주문요청")]
public sealed class 마트주문요청
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("요청자_user_id")]
    [MaxLength(450)]
    public string 요청자UserId { get; set; } = string.Empty;

    [Column("클라이언트_요청_id")]
    public Guid 클라이언트요청Id { get; set; }

    [Column("공개상품_id")]
    public long 공개상품Id { get; set; }

    [Column("상품명_snapshot")]
    [MaxLength(200)]
    public string 상품명Snapshot { get; set; } = string.Empty;

    [Column("판매단위_snapshot")]
    [MaxLength(100)]
    public string 판매단위Snapshot { get; set; } = string.Empty;

    [Column("단가_snapshot", TypeName = "decimal(18,2)")]
    public decimal 단가Snapshot { get; set; }

    [Column("수량")]
    public int 수량 { get; set; }

    [Column("합계_snapshot", TypeName = "decimal(18,2)")]
    public decimal 합계Snapshot { get; set; }

    [Column("통화")]
    [MaxLength(3)]
    public string 통화 { get; set; } = "KRW";

    [Column("제출시_판매가능수량")]
    public int 제출시판매가능수량 { get; set; }

    [Column("재고기준시각_utc")]
    public DateTime 재고기준시각Utc { get; set; }

    [Column("상태_code")]
    [MaxLength(32)]
    public string 상태코드 { get; set; } = string.Empty;

    [Column("비구속_주문요청_확인")]
    public bool 비구속주문요청확인 { get; set; }

    [Column("안내_version")]
    [MaxLength(32)]
    public string 안내버전 { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }
}
