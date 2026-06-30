using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.차량
{
    [Table("차량제원")]
    public class 차량제원
    {
        [Key]
        [Column("차량코드")]
        public string 차량코드 { get; set; } = string.Empty;

        [Column("차량명")]
        public string 차량명 { get; set; } = string.Empty;

        [Column("제조사")]
        public string 제조사 { get; set; } = string.Empty;

        [Column("모델명")]
        public string 모델명 { get; set; } = string.Empty;

        [Column("차급")]
        public string 차급 { get; set; } = string.Empty;

        [Column("차체형태")]
        public string 차체형태 { get; set; } = string.Empty;

        [Column("적재함길이Mm")]
        public int 적재함길이Mm { get; set; }

        [Column("적재함폭Mm")]
        public int 적재함폭Mm { get; set; }

        [Column("적재함높이Mm")]
        public int? 적재함높이Mm { get; set; }

        [Column("최대적재중량Kg")]
        public int 최대적재중량Kg { get; set; }

        [Column("운영권장중량Kg")]
        public int? 운영권장중량Kg { get; set; }

        [Column("차량전체높이Mm")]
        public int? 차량전체높이Mm { get; set; }

        [Column("바닥높이Mm")]
        public int? 바닥높이Mm { get; set; }

        [Column("비눈보호가능")]
        public bool 비눈보호가능 { get; set; }

        [Column("냉장가능")]
        public bool 냉장가능 { get; set; }

        [Column("냉동가능")]
        public bool 냉동가능 { get; set; }

        [Column("측면상하차가능")]
        public bool 측면상하차가능 { get; set; }

        [Column("리프트가능")]
        public bool 리프트가능 { get; set; }

        [Column("장재물유리")]
        public bool 장재물유리 { get; set; }

        [Column("팔레트적재개수")]
        public int? 팔레트적재개수 { get; set; }

        [Column("권장최대CBM")]
        public decimal? 권장최대CBM { get; set; }

        [Column("추천우선순위")]
        public int 추천우선순위 { get; set; } = 100;

        [Column("추천사용여부")]
        public bool 추천사용여부 { get; set; } = true;

        [Column("기준연비KmPerLiter")]
        public decimal? 기준연비KmPerLiter { get; set; }

        [Column("장점메모")]
        public string 장점메모 { get; set; } = string.Empty;

        [Column("단점메모")]
        public string 단점메모 { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}