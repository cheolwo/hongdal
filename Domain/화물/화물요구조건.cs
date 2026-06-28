using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.화물
{
    [Table("화물요구조건")]
    public class 화물요구조건
    {
        [Key]
        [Column("의뢰Id")]
        public string 의뢰Id { get; set; } = string.Empty;

        [Column("화물길이Mm")]
        public int? 화물길이Mm { get; set; }

        [Column("화물폭Mm")]
        public int? 화물폭Mm { get; set; }

        [Column("화물높이Mm")]
        public int? 화물높이Mm { get; set; }

        [Column("화물무게Kg")]
        public int? 화물무게Kg { get; set; }

        [Column("팔레트개수")]
        public int? 팔레트개수 { get; set; }

        [Column("비맞으면안됨")]
        public bool 비맞으면안됨 { get; set; }

        [Column("냉장필요")]
        public bool 냉장필요 { get; set; }

        [Column("냉동필요")]
        public bool 냉동필요 { get; set; }

        [Column("리프트필요")]
        public bool 리프트필요 { get; set; }

        [Column("측면상하차필요")]
        public bool 측면상하차필요 { get; set; }

        [Column("장재물")]
        public bool 장재물 { get; set; }

        [Column("혼적허용")]
        public bool 혼적허용 { get; set; }

        [Column("독차필수")]
        public bool 독차필수 { get; set; }

        [Column("주의사항")]
        public string 주의사항 { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}