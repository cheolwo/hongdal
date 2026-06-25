using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.기사
{
    [Table("driver_shifts")]
    public class 기사근무
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        // driver identifier as string to match Notion/article IDs
        [Column("driver_id")]
        public string 기사Id { get; set; } = string.Empty;

        [Column("start_mode")]
        public string 시작모드 { get; set; } = string.Empty; // immediate|reserved

        [Column("started_at")]
        public DateTime? 시작시각 { get; set; }

        [Column("start_location")]
        public string 시작위치 { get; set; } = string.Empty;

        [Column("return_destination")]
        public string? 복귀지 { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
