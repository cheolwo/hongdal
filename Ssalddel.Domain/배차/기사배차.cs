using System;

namespace 살뜰.도메인.배차
{
    public class 기사배차
    {
        public long Id { get; set; }

        public long? 배차Id { get; set; }

        public string 배차명 { get; set; } = string.Empty;

        public string 상태 { get; set; } = "배차대기";

        public DateTime? 배차일 { get; set; }

        public long? 용달기사_id { get; set; }

        public string 픽업지 { get; set; } = string.Empty;

        public string 배송지 { get; set; } = string.Empty;

        public long? 기본요금 { get; set; }

        public long? 거리추가_요금 { get; set; }

        public long? 주문Id { get; set; }

        public long? 기사Id { get; set; }

        public bool 잠금여부 { get; set; }

        public DateTime? 잠금시각 { get; set; }

        public int? 시도횟수 { get; set; }

        public int? 픽업거리_m { get; set; }

        public int? 픽업예상시간_sec { get; set; }

        public decimal? 배차점수 { get; set; }

        public string 실패사유 { get; set; } = string.Empty;

        public string 메모 { get; set; } = string.Empty;

        public DateTime? 배차생성시각 { get; set; }

        public DateTime? 배차완료시각 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
