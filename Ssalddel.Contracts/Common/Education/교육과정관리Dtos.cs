using System.Text.Json;

namespace Ssalddel.Contracts.Common.Education;

public static class 교육과정양식코드
{
    public const string 입교신청 = "입교신청";
    public const string 수련체험기 = "수련체험기";
    public const string 상담과제 = "상담과제";
    public const string 과정종료정리 = "과정종료정리";
    public const string 발원서 = "발원서";
}

public static class 교육과정양식필드유형
{
    public const string 짧은글 = "text";
    public const string 긴글 = "textarea";
    public const string 숫자 = "number";
    public const string 참거짓 = "boolean";
    public const string 단일선택 = "choice";
    public const string 날짜 = "date";
    public const string 이메일 = "email";
    public const string 전화번호 = "phone";

    public static bool 지원여부(string 유형)
        => 유형 is 짧은글 or 긴글 or 숫자 or 참거짓 or 단일선택 or 날짜 or 이메일 or 전화번호;
}

public sealed class 교육과정양식필드Dto
{
    public string Key { get; set; } = string.Empty;
    public string 라벨 { get; set; } = string.Empty;
    public string 유형 { get; set; } = 교육과정양식필드유형.짧은글;
    public string? 안내 { get; set; }
    public string? 섹션 { get; set; }
    public bool 필수여부 { get; set; }
    public bool 참값필수여부 { get; set; }
    public int 최대길이 { get; set; } = 2000;
    public int 표시순서 { get; set; }
    public IReadOnlyList<string> 선택목록 { get; set; } = [];
}

public sealed class 교육과정과목관리요청
{
    public string 과목코드 { get; set; } = string.Empty;
    public string 과목명 { get; set; } = string.Empty;
    public int 표시순서 { get; set; }
    public int 최소참석횟수 { get; set; }
}

public sealed class 교육과정양식관리요청
{
    public string 양식코드 { get; set; } = string.Empty;
    public string 양식명 { get; set; } = string.Empty;
    public string 목적 { get; set; } = string.Empty;
    public string 버전 { get; set; } = "1.0";
    public string 제출주기 { get; set; } = string.Empty;
    public int 최소제출횟수 { get; set; }
    public bool 필수여부 { get; set; }
    public bool 활성화여부 { get; set; } = true;
    public string? 출처Url { get; set; }
    public IReadOnlyList<교육과정양식필드Dto> 필드목록 { get; set; } = [];
}

public sealed class 교육과정관리요청
{
    public string 과정명 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public string 운영방식 { get; set; } = string.Empty;
    public int 최소이수개월 { get; set; }
    public bool 활성화여부 { get; set; } = true;
    public string? 출처Url { get; set; }
    public IReadOnlyList<교육과정과목관리요청> 과목목록 { get; set; } = [];
    public IReadOnlyList<교육과정양식관리요청> 양식목록 { get; set; } = [];
}

public class 교육과정목록항목Dto
{
    public string 과정코드 { get; set; } = string.Empty;
    public string 과정명 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public string 운영방식 { get; set; } = string.Empty;
    public int 최소이수개월 { get; set; }
    public bool 활성화여부 { get; set; }
    public string? 출처Url { get; set; }
}

public sealed class 교육과정과목Dto
{
    public string 과목코드 { get; set; } = string.Empty;
    public string 과목명 { get; set; } = string.Empty;
    public int 표시순서 { get; set; }
    public int 최소참석횟수 { get; set; }
}

public sealed class 교육과정양식Dto
{
    public string 양식코드 { get; set; } = string.Empty;
    public string 양식명 { get; set; } = string.Empty;
    public string 목적 { get; set; } = string.Empty;
    public string 버전 { get; set; } = string.Empty;
    public string 제출주기 { get; set; } = string.Empty;
    public int 최소제출횟수 { get; set; }
    public bool 필수여부 { get; set; }
    public bool 활성화여부 { get; set; }
    public string? 출처Url { get; set; }
    public IReadOnlyList<교육과정양식필드Dto> 필드목록 { get; set; } = [];
}

public sealed class 교육과정상세Dto : 교육과정목록항목Dto
{
    public IReadOnlyList<교육과정과목Dto> 과목목록 { get; set; } = [];
    public IReadOnlyList<교육과정양식Dto> 양식목록 { get; set; } = [];
}

public sealed class 교육과정신청요청
{
    public string 과정코드 { get; set; } = string.Empty;
    public string 이름 { get; set; } = string.Empty;
    public string? 별명 { get; set; }
    public string 이메일 { get; set; } = string.Empty;
    public string 전화번호 { get; set; } = string.Empty;
    public string 성별 { get; set; } = string.Empty;
    public int 출생연도 { get; set; }
    public string? 거주국가 { get; set; }
    public bool 회원가입확인 { get; set; }
    public bool 입교서약동의 { get; set; }
    public bool 개인정보수집이용동의 { get; set; }
    public bool 개인정보제3자제공동의 { get; set; }
    public string 개인정보동의버전 { get; set; } = string.Empty;
    public string 제3자제공동의버전 { get; set; } = string.Empty;
}

public sealed class 교육과정신청심사요청
{
    public string 상태 { get; set; } = string.Empty;
    public string? 담당멘토UserId { get; set; }
    public string? 심사메모 { get; set; }
    public DateTime? 시작일시Utc { get; set; }
}

public sealed class 교육과정신청Dto
{
    public long 신청Id { get; set; }
    public string 과정코드 { get; set; } = string.Empty;
    public string 과정명 { get; set; } = string.Empty;
    public string 신청자UserId { get; set; } = string.Empty;
    public string 이름 { get; set; } = string.Empty;
    public string 별명 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 전화번호 { get; set; } = string.Empty;
    public string 성별 { get; set; } = string.Empty;
    public int 출생연도 { get; set; }
    public string 거주국가 { get; set; } = string.Empty;
    public bool 회원가입확인 { get; set; }
    public bool 입교서약동의 { get; set; }
    public bool 개인정보수집이용동의 { get; set; }
    public bool 개인정보제3자제공동의 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string? 심사자UserId { get; set; }
    public string 심사메모 { get; set; } = string.Empty;
    public DateTime 신청일시Utc { get; set; }
    public DateTime? 심사일시Utc { get; set; }
    public long? 등록Id { get; set; }
}

public sealed class 교육과정과제제출요청
{
    public string 양식코드 { get; set; } = string.Empty;
    public string 제출기간Key { get; set; } = string.Empty;
    public Dictionary<string, JsonElement> 답변 { get; set; } = new(StringComparer.Ordinal);
}

public sealed class 교육과정과제확인요청
{
    public string 상태 { get; set; } = string.Empty;
    public string? 확인메모 { get; set; }
}

public sealed class 교육과정과제제출Dto
{
    public long 제출Id { get; set; }
    public string 양식코드 { get; set; } = string.Empty;
    public string 양식명 { get; set; } = string.Empty;
    public string 제출기간Key { get; set; } = string.Empty;
    public Dictionary<string, JsonElement> 답변 { get; set; } = new(StringComparer.Ordinal);
    public string 상태 { get; set; } = string.Empty;
    public string? 확인자UserId { get; set; }
    public string 확인메모 { get; set; } = string.Empty;
    public DateTime 제출일시Utc { get; set; }
    public DateTime? 확인일시Utc { get; set; }
}

public sealed class 교육과정참석기록요청
{
    public string 과목코드 { get; set; } = string.Empty;
    public string 회차Key { get; set; } = string.Empty;
    public string 회차명 { get; set; } = string.Empty;
    public DateTime 수업일시Utc { get; set; }
    public bool 참석여부 { get; set; }
}

public sealed class 교육과정진행현황Dto
{
    public long 등록Id { get; set; }
    public string 과정코드 { get; set; } = string.Empty;
    public string 과정명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string? 담당멘토UserId { get; set; }
    public DateTime 시작일시Utc { get; set; }
    public IReadOnlyList<교육과정과목진행Dto> 과목목록 { get; set; } = [];
    public IReadOnlyList<교육과정양식진행Dto> 양식목록 { get; set; } = [];
}

public sealed class 교육과정과목진행Dto
{
    public string 과목코드 { get; set; } = string.Empty;
    public string 과목명 { get; set; } = string.Empty;
    public int 참석횟수 { get; set; }
    public int 최소참석횟수 { get; set; }
    public bool 충족여부 { get; set; }
}

public sealed class 교육과정양식진행Dto
{
    public string 양식코드 { get; set; } = string.Empty;
    public string 양식명 { get; set; } = string.Empty;
    public int 제출횟수 { get; set; }
    public int 최소제출횟수 { get; set; }
    public bool 충족여부 { get; set; }
}
