using System;

namespace 살뜰.도메인.기사
{
    public class 기사월정산
    {
        public long Id { get; set; }

        public string 기사Id { get; set; } = string.Empty;

        public int 년도 { get; set; }

        public int 월 { get; set; }

        public int 배차건수 { get; set; }

        public decimal 이용료 { get; set; }

        public bool 결제완료 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
