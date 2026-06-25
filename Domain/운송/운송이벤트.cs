using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.운송
{
    [Table("운송이벤트")]
    public class 운송이벤트
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("request_id")]
        public string 의뢰Id { get; set; } = string.Empty;

        [Column("event_type")]
        public string 이벤트타입 { get; set; } = string.Empty;

        [Column("event_time")]
        public DateTime 이벤트시각 { get; set; } = DateTime.UtcNow;

        [Column("metadata")]
        public string 메타데이터 { get; set; } = string.Empty;
    }
}
