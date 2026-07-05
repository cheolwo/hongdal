using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.운송
{
    [Table("차량단가")]
    public class 차량단가
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("차량종류")]
        public string 차량종류 { get; set; } = string.Empty;

        [Column("기본운임")]
        public decimal 기본운임 { get; set; }

        [Column("Km당단가")]
        public decimal Km당단가 { get; set; }

        [Column("야간할증")]
        public decimal 야간할증 { get; set; }

        [Column("우천할증")]
        public decimal 우천할증 { get; set; }

        [Column("최소운임")]
        public decimal 최소운임 { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
