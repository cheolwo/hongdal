namespace 홍달.Services.Dispatch.Coordination;

public sealed record 배달권판정결과(
    string 배달권키,
    string 배달권명,
    string 판정방식,
    string? 법정동코드 = null,
    string? 시도명 = null,
    string? 시군구명 = null,
    string? 대표건물명 = null,
    string? 대표건물주소 = null,
    decimal? 대표위도 = null,
    decimal? 대표경도 = null);

public sealed record 기초배달권항목(
    string 법정동코드,
    string 시도명,
    string? 시군구명,
    string 대표건물명,
    string 대표건물주소,
    decimal 대표위도,
    decimal 대표경도)
{
    public string 행정계층 => string.IsNullOrWhiteSpace(시군구명) ? "시도" : "시군구";

    public string 배달권명 => string.IsNullOrWhiteSpace(시군구명) ? 시도명 : 시군구명!;

    public string 배달권키 => 행정계층 == "시도"
        ? $"bjd-sido:{법정동코드[..2]}"
        : $"bjd-sigungu:{법정동코드[..5]}";

    public string 판정방식 => 행정계층 == "시도" ? "법정동시도" : "법정동시군구";
}
