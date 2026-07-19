namespace Ssalddel.Contracts.Admin.Management;

public sealed class 관리자연락처검색응답
{
    public string 전화번호뒤8자리 { get; set; } = string.Empty;
    public int 검색결과수 { get; set; }
    public DateTime 조회일시Utc { get; set; }
    public IReadOnlyList<관리자연락처인물응답> 인물목록 { get; set; } = [];
}

public sealed class 관리자연락처인물응답
{
    public string UserId { get; set; } = string.Empty;
    public string 사용자명 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 전화번호뒤8자리 { get; set; } = string.Empty;
    public string 사업자번호 { get; set; } = string.Empty;
    public IReadOnlyList<string> 역할목록 { get; set; } = [];
    public IReadOnlyList<string> 연락처출처목록 { get; set; } = [];
    public 관리자연락처기사정보응답? 기사정보 { get; set; }
    public 관리자연락처주문자프로필응답? 주문자프로필 { get; set; }
    public 관리자연락처화주요약응답? 화주정보 { get; set; }
    public IReadOnlyList<관리자연락처창고참여응답> 창고참여목록 { get; set; } = [];
    public IReadOnlyList<관리자연락처최근의뢰응답> 최근의뢰목록 { get; set; } = [];
}

public sealed class 관리자연락처기사정보응답
{
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public string 활동지역 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
}

public sealed class 관리자연락처주문자프로필응답
{
    public long Id { get; set; }
    public string 표시명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 기본주소 { get; set; } = string.Empty;
}

public sealed class 관리자연락처화주요약응답
{
    public int 의뢰건수 { get; set; }
    public int 진행중의뢰건수 { get; set; }
    public DateTime? 최근의뢰일시 { get; set; }
}

public sealed class 관리자연락처창고참여응답
{
    public long 창고Id { get; set; }
    public string 창고명 { get; set; } = string.Empty;
    public string 역할명 { get; set; } = string.Empty;
    public bool 주담당여부 { get; set; }
    public string 창고유형 { get; set; } = string.Empty;
    public string 주소 { get; set; } = string.Empty;
    public string 담당자명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
}

public sealed class 관리자연락처최근의뢰응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
    public string 의뢰상태 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;
    public string 픽업지 { get; set; } = string.Empty;
    public string 하차지 { get; set; } = string.Empty;
    public DateTime 생성일시 { get; set; }
}
