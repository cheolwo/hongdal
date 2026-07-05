namespace Hongdal.Contracts.Admin.Inbound;

public sealed class 배차대기요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
    public int? 배차업무유형 { get; set; }
    public string? 원본의뢰유형 { get; set; }
    public string? 원본의뢰Id { get; set; }
    public string 픽업_도로명주소 { get; set; } = string.Empty;
    public string 픽업_상세주소 { get; set; } = string.Empty;
    public decimal? 픽업_위도 { get; set; }
    public decimal? 픽업_경도 { get; set; }
    public string 하차_도로명주소 { get; set; } = string.Empty;
    public string 하차_상세주소 { get; set; } = string.Empty;
    public decimal? 하차_위도 { get; set; }
    public decimal? 하차_경도 { get; set; }
    public string 상태 { get; set; } = string.Empty;
}
