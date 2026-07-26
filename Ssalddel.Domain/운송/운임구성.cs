using System;

namespace 살뜰.도메인.운송
{
    public class 운임구성
    {
        public long Id { get; set; }

        public string 의뢰Id { get; set; } = string.Empty;

        public decimal 기본운임 { get; set; }

        public decimal 거리운임 { get; set; }

        public decimal 할증 { get; set; }

        public decimal 대기료 { get; set; }

        public decimal 수작업비 { get; set; }

        public decimal 최종운임 { get; set; }

        // 화주 청구액과 구분되는 기사 지급 약정 금액입니다.
        public decimal? 기사지급예정운임 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
