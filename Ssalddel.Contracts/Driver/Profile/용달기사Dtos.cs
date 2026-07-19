namespace Ssalddel.Contracts.Driver.Profile;

public sealed class 용달기사등록요청
{
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string? 상태 { get; set; }
    public string? 메모 { get; set; }
    public string? 기본복귀지주소 { get; set; }
    public decimal? 기본복귀지위도 { get; set; }
    public decimal? 기본복귀지경도 { get; set; }
    public bool 집주소를복귀지로사용허용 { get; set; }
}

public sealed class 용달기사등록응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string? 차량코드 { get; set; }
    public string? 차량명 { get; set; }
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
    public string 메모 { get; set; } = string.Empty;
    public string? 기본복귀지주소 { get; set; }
    public decimal? 기본복귀지위도 { get; set; }
    public decimal? 기본복귀지경도 { get; set; }
    public bool 집주소를복귀지로사용허용 { get; set; }
    public bool 푸시토큰등록됨 { get; set; }
}