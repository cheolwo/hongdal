using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.기사
{
    [Table("배차계획신청")]
    public class 배차계획신청
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("기사Id")]
        public string 기사Id { get; set; } = string.Empty;

        [Column("출발지")]
        public string 출발지 { get; set; } = string.Empty;

        [Column("복귀지")]
        public string 복귀지 { get; set; } = string.Empty;

        [Column("희망복귀시각")]
        public DateTime? 희망복귀시각 { get; set; }

        [Column("배차가능시각")]
        public DateTime? 배차가능시각 { get; set; }

        [Column("상태")]
        public string 상태 { get; set; } = "신청";

        [Column("메모")]
        public string 메모 { get; set; } = string.Empty;

        [Column("신청일시")]
        public DateTime 신청일시 { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}