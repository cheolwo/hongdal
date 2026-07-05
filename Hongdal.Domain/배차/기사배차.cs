using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.배차
{
    [Table("기사배차")]
    public class 기사배차
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("notion_page_id")]
        public string NotionPageId { get; set; } = string.Empty;

        [Column("배차Id")]
        public long? 배차Id { get; set; }

        [Column("배차명")]
        public string 배차명 { get; set; } = string.Empty;

        [Column("상태")]
        public string 상태 { get; set; } = "배차대기";

        [Column("배차일")]
        public DateTime? 배차일 { get; set; }

        [Column("배달기사_id")]
        public long? 용달기사_id { get; set; }

        [Column("픽업지")]
        public string 픽업지 { get; set; } = string.Empty;

        [Column("배송지")]
        public string 배송지 { get; set; } = string.Empty;

        [Column("기본요금")]
        public long? 기본요금 { get; set; }

        [Column("거리추가_요금")]
        public long? 거리추가_요금 { get; set; }

        [Column("주문Id")]
        public long? 주문Id { get; set; }

        [Column("기사Id")]
        public long? 기사Id { get; set; }

        [Column("잠금여부")]
        public bool 잠금여부 { get; set; }

        [Column("잠금시각")]
        public DateTime? 잠금시각 { get; set; }

        [Column("시도횟수")]
        public int? 시도횟수 { get; set; }

        [Column("픽업거리_m")]
        public int? 픽업거리_m { get; set; }

        [Column("픽업예상시간_sec")]
        public int? 픽업예상시간_sec { get; set; }

        [Column("배차점수")]
        public decimal? 배차점수 { get; set; }

        [Column("실패사유")]
        public string 실패사유 { get; set; } = string.Empty;

        [Column("메모")]
        public string 메모 { get; set; } = string.Empty;

        [Column("배차생성시각")]
        public DateTime? 배차생성시각 { get; set; }

        [Column("배차완료시각")]
        public DateTime? 배차완료시각 { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
