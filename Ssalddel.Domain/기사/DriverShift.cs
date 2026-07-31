using System;
using Ssalddel.Contracts.Common.Transport;

namespace 살뜰.도메인.기사
{
    public class 기사근무
    {
        public long Id { get; set; }

        // Notion/article ID와 맞추기 위해 기사 식별자는 문자열로 둔다.
        public string 기사Id { get; set; } = string.Empty;

        public string 시작모드 { get; set; } = string.Empty; // immediate|reserved

        public DateTime? 시작시각 { get; set; }

        public string 시작위치 { get; set; } = string.Empty;

        public string 운송실행유형 { get; set; } = 운송실행유형코드.화물운송;

        public string? 복귀지 { get; set; }

        public string? 오늘의복귀지주소 { get; set; }

        public decimal? 오늘의복귀지위도 { get; set; }

        public decimal? 오늘의복귀지경도 { get; set; }

        public string 복귀지출처 { get; set; } = string.Empty;

        public DateTime? 복귀지입력일시 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
