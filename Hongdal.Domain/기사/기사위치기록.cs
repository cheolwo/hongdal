using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.기사
{
    [Table("driver_location_history")]
    public class 기사위치기록
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("driver_id")]
        public string 기사Id { get; set; } = string.Empty;

        [Column("latitude")]
        public decimal 위도 { get; set; }

        [Column("longitude")]
        public decimal 경도 { get; set; }

        [Column("accuracy_m")]
        public decimal? 정확도_m { get; set; }

        [Column("recorded_at")]
        public DateTime 기록시각 { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
