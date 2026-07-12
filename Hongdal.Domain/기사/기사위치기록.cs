using System;

namespace 홍달.도메인.기사
{
    public class 기사위치기록
    {
        public long Id { get; set; }

        public string 기사Id { get; set; } = string.Empty;

        public decimal 위도 { get; set; }

        public decimal 경도 { get; set; }

        public decimal? 정확도_m { get; set; }

        public DateTime 기록시각 { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
