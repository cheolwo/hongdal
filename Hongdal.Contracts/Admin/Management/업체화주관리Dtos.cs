namespace Hongdal.Contracts.Admin.Management;

public sealed class 업체관리응답
{
    public long Id { get; set; }
    public string 업체명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 대표연락처 { get; set; } = string.Empty;
    public string 담당자 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 주소 { get; set; } = string.Empty;
    public string 정산결제조건 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
}

public sealed class 화주관리응답
{
    public string 화주Id { get; set; } = string.Empty;
    public string 사용자명 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public int 의뢰건수 { get; set; }
    public DateTime? 최근의뢰일시 { get; set; }
    public string 거래상태 { get; set; } = string.Empty;
}