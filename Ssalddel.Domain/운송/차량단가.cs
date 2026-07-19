using System;

namespace 살뜰.도메인.운송
{
    public class 차량단가
    {
        public long Id { get; set; }

        public string 차량종류 { get; set; } = string.Empty;

        public decimal 기본운임 { get; set; }

        public decimal Km당단가 { get; set; }

        public decimal 야간할증 { get; set; }

        public decimal 우천할증 { get; set; }

        public decimal 최소운임 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
