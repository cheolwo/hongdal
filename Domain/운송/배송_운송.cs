using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.운송
{
    [Table("배송_운송")]
    public class 배송_운송
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("운송번호")]
        public string 운송번호 { get; set; } = string.Empty;

        [Column("상태")]
        public string 상태 { get; set; } = "배차대기";

        [Column("출발_픽업")]
        public DateTime? 출발_픽업 { get; set; }

        [Column("도착")]
        public DateTime? 도착 { get; set; }

        [Column("기사_운송자")]
        public string 기사_운송자 { get; set; } = string.Empty;

        [Column("출발지")]
        public string 출발지 { get; set; } = string.Empty;

        [Column("도착지")]
        public string 도착지 { get; set; } = string.Empty;

        [Column("운임")]
        public decimal? 운임 { get; set; }

        [Column("첨부_json")]
        public string 첨부_json { get; set; } = "[]";

        [Column("메모")]
        public string 메모 { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
