namespace Ssalddel.Contracts.Admin.Management;

public sealed class 기사목록응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
    public int 배차건수 { get; set; }
}

public sealed class 기사상세응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
}

public sealed class 기사배차내역응답
{
    public long Id { get; set; }
    public string 배차명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 배차일 { get; set; }
    public string 픽업지 { get; set; } = string.Empty;
    public string 배송지 { get; set; } = string.Empty;
    public decimal? 배차점수 { get; set; }
    public string 실패사유 { get; set; } = string.Empty;
    public DateTime? 배차생성시각 { get; set; }
    public DateTime? 배차완료시각 { get; set; }
}