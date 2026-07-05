namespace Hongdal.Contracts.Admin.Progress;

public sealed class 배차계획관리목록응답
{
    public long Id { get; set; }
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 출발지 { get; set; } = string.Empty;
    public string 복귀지 { get; set; } = string.Empty;
    public DateTime? 희망복귀시각 { get; set; }
    public DateTime? 배차가능시각 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public DateTime 신청일시 { get; set; }
    public DateTime 최근수정시각 { get; set; }
}

public sealed class 배차계획관리상세응답
{
    public long Id { get; set; }
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 출발지 { get; set; } = string.Empty;
    public string 복귀지 { get; set; } = string.Empty;
    public DateTime? 희망복귀시각 { get; set; }
    public DateTime? 배차가능시각 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public DateTime 신청일시 { get; set; }
    public DateTime 최근수정시각 { get; set; }
}