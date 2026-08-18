using System;

namespace Ssalddel.WorkflowRules.Contracts
{
    public static class 화물배차후보차단사유코드
    {
        public const string 화물운송앱아님 = "FreightAppRequired";
        public const string 차량비활성 = "VehicleInactive";
        public const string 기사운행중아님 = "DriverNotOperating";
        public const string 명시적제외 = "CandidateExplicitlyExcluded";
        public const string 이전거절 = "CandidatePreviouslyRejected";
        public const string 위치정보없음 = "CandidateLocationMissing";
        public const string 위치정보오래됨 = "CandidateLocationStale";
        public const string 거리정보없음 = "PickupDistanceMissing";
        public const string 상차접근반경초과 = "PickupRadiusExceeded";
        public const string 상차시간창종료 = "PickupWindowExpired";
        public const string 상차시간창도착불가 = "PickupWindowArrivalInfeasible";
        public const string 차량부적합 = "VehicleIncompatible";
        public const string 차량용량부족 = "VehicleCapacityExceeded";
        public const string 차량용량단위불일치 = "VehicleCapacityUnitMismatch";
    }

    public sealed class 화물배차추천점수요청
    {
        public bool? 전체일정완수가능여부 { get; set; }
        public bool? 일정삽입가능여부 { get; set; }
        public bool 경로변경이점여부 { get; set; }
        public decimal? 예상추가순이익 { get; set; }
        public decimal? 추가지연분 { get; set; }
        public decimal? 경로기준거리Km { get; set; }
        public string 추천유형 { get; set; } = "single";
        public bool 화물민감여부 { get; set; }
        public decimal? 복귀우회증가거리Km { get; set; }
        public bool 복귀지기준사용여부 { get; set; }
    }

    public sealed class 화물배차추천점수판정
    {
        public decimal 일정점수 { get; set; }
        public decimal 수익점수 { get; set; }
        public decimal 지연점수 { get; set; }
        public decimal 거리점수 { get; set; }
        public decimal 추천유형점수 { get; set; }
        public decimal 화물민감도점수 { get; set; }
        public decimal 복귀부담점수 { get; set; }
        public decimal 총점 { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 화물배차후보입력
    {
        public string 후보StableId { get; set; } = string.Empty;
        public string 차량StableId { get; set; } = string.Empty;
        public bool 화물운송앱여부 { get; set; }
        public bool 차량활성여부 { get; set; }
        public bool 기사운행중여부 { get; set; }
        public bool 이전거절여부 { get; set; }
        public decimal? 위치경과분 { get; set; }
        public decimal? 상차거리Km { get; set; }
        public decimal? 상차접근허용반경Km { get; set; }
        public decimal 차량용량 { get; set; }
        public string 차량용량단위코드 { get; set; } = string.Empty;
        public bool 차량적합여부 { get; set; }
        public string[] 차량부적합사유코드목록 { get; set; } = Array.Empty<string>();
        public decimal 기사대기분 { get; set; }
        public string 기본추천사유 { get; set; } = string.Empty;
        public 화물배차추천점수요청 추천점수요청 { get; set; } = new 화물배차추천점수요청();
    }

    public sealed class 화물배차후보선정요청
    {
        public decimal 화물수량 { get; set; }
        public string 화물단위코드 { get; set; } = string.Empty;
        public decimal 위치유효시간분 { get; set; } = 10m;
        public decimal 기본상차접근반경Km { get; set; } = 10m;
        public decimal 원거리상차접근최대반경Km { get; set; } = 50m;
        public decimal 원거리상차평균속도KmH { get; set; } = 40m;
        public decimal 원거리상차도착여유분 { get; set; } = 10m;
        public decimal? 상차시간창남은분 { get; set; }
        public string? 제외후보StableId { get; set; }
        public 화물배차후보입력[] 후보목록 { get; set; } = Array.Empty<화물배차후보입력>();
    }

    public sealed class 화물배차후보평가
    {
        public string 후보StableId { get; set; } = string.Empty;
        public string 차량StableId { get; set; } = string.Empty;
        public bool 적격여부 { get; set; }
        public int 추천순위 { get; set; }
        public decimal 기본추천점수 { get; set; }
        public decimal 기사대기보정점수 { get; set; }
        public decimal 총추천점수 { get; set; }
        public decimal? 상차거리Km { get; set; }
        public decimal 차량용량 { get; set; }
        public string 차량용량단위코드 { get; set; } = string.Empty;
        public string 추천사유 { get; set; } = string.Empty;
        public string[] 차단사유코드목록 { get; set; } = Array.Empty<string>();
        public 화물배차추천점수판정 점수내역 { get; set; } = new 화물배차추천점수판정();
    }

    public sealed class 화물배차후보선정판정
    {
        public string? 추천후보StableId { get; set; }
        public int 적격후보수 { get; set; }
        public 화물배차후보평가[] 후보평가목록 { get; set; } = Array.Empty<화물배차후보평가>();
        public string RuleRevision { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
