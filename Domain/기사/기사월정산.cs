using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.기사
{
    [Table("기사월정산")]
    public class 기사월정산
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("driver_id")]
        public string 기사Id { get; set; } = string.Empty;

        [Column("year")]
        public int 년도 { get; set; }

        [Column("month")]
        public int 월 { get; set; }

        [Column("dispatch_count")]
        public int 배차건수 { get; set; }

        [Column("usage_fee")]
        public decimal 이용료 { get; set; }

        [Column("is_paid")]
        public bool 결제완료 { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
