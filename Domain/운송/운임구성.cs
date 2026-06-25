using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.운송
{
    [Table("운임구성")]
    public class 운임구성
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("request_id")]
        public string 의뢰Id { get; set; } = string.Empty;

        [Column("기본운임")]
        public decimal 기본운임 { get; set; }

        [Column("거리운임")]
        public decimal 거리운임 { get; set; }

        [Column("할증")]
        public decimal 할증 { get; set; }

        [Column("대기료")]
        public decimal 대기료 { get; set; }

        [Column("수작업비")]
        public decimal 수작업비 { get; set; }

        [Column("최종운임")]
        public decimal 최종운임 { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
