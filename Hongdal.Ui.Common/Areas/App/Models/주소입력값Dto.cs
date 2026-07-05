namespace Hongdal.Ui.Common.Areas.App.Models;

public sealed class 주소입력값Dto
{
    public string 우편번호 { get; set; } = string.Empty;
    public string 기본주소 { get; set; } = string.Empty;
    public string 도로명주소 { get; set; } = string.Empty;
    public string 지번주소 { get; set; } = string.Empty;
    public string 상세주소 { get; set; } = string.Empty;

    public string 시도 { get; set; } = string.Empty;
    public string 시군구 { get; set; } = string.Empty;
    public string 시군구코드 { get; set; } = string.Empty;
    public string 법정동코드 { get; set; } = string.Empty;
    public string 법정동명 { get; set; } = string.Empty;
    public string 행정동명 { get; set; } = string.Empty;

    public string 건물관리번호 { get; set; } = string.Empty;
    public string 건물명 { get; set; } = string.Empty;

    public double? 위도 { get; set; }
    public double? 경도 { get; set; }

    public string? 주소검색원본Json { get; set; }
}
