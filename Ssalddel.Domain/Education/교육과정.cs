namespace Ssalddel.Domain.Education;

public static class 교육과정신청상태
{
    public const string 검토대기 = "검토대기";
    public const string 보류 = "보류";
    public const string 승인 = "승인";
    public const string 거절 = "거절";
    public const string 철회 = "철회";

    public static bool 심사가능(string 상태)
        => 상태 is 보류 or 승인 or 거절;
}

public static class 교육과정등록상태
{
    public const string 진행중 = "진행중";
    public const string 수료심사대기 = "수료심사대기";
    public const string 수료 = "수료";
    public const string 중지 = "중지";
}

public static class 교육과정제출상태
{
    public const string 제출 = "제출";
    public const string 확인 = "확인";
    public const string 보완요청 = "보완요청";
}

public sealed class 교육과정
{
    public long Id { get; set; }
    public string 과정코드 { get; set; } = string.Empty;
    public string 과정명 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public string 운영방식 { get; set; } = string.Empty;
    public int 최소이수개월 { get; set; }
    public bool 활성화여부 { get; set; } = true;
    public string? 출처Url { get; set; }
    public DateTime 생성일시Utc { get; set; } = DateTime.UtcNow;
    public DateTime 수정일시Utc { get; set; } = DateTime.UtcNow;

    public ICollection<교육과정과목> 과목목록 { get; set; } = new List<교육과정과목>();
    public ICollection<교육과정양식> 양식목록 { get; set; } = new List<교육과정양식>();
}

public sealed class 교육과정과목
{
    public long Id { get; set; }
    public long 교육과정Id { get; set; }
    public 교육과정? 교육과정 { get; set; }
    public string 과목코드 { get; set; } = string.Empty;
    public string 과목명 { get; set; } = string.Empty;
    public int 표시순서 { get; set; }
    public int 최소참석횟수 { get; set; }
}

public sealed class 교육과정양식
{
    public long Id { get; set; }
    public long 교육과정Id { get; set; }
    public 교육과정? 교육과정 { get; set; }
    public string 양식코드 { get; set; } = string.Empty;
    public string 양식명 { get; set; } = string.Empty;
    public string 목적 { get; set; } = string.Empty;
    public string 버전 { get; set; } = string.Empty;
    public string 제출주기 { get; set; } = string.Empty;
    public int 최소제출횟수 { get; set; }
    public bool 필수여부 { get; set; }
    public bool 활성화여부 { get; set; } = true;
    public string 필드정의Json { get; set; } = "[]";
    public string? 출처Url { get; set; }
    public DateTime 생성일시Utc { get; set; } = DateTime.UtcNow;
    public DateTime 수정일시Utc { get; set; } = DateTime.UtcNow;

    public ICollection<교육과정과제제출> 제출목록 { get; set; } = new List<교육과정과제제출>();
}

public sealed class 교육과정신청
{
    public long Id { get; set; }
    public long 교육과정Id { get; set; }
    public 교육과정? 교육과정 { get; set; }
    public string 신청자UserId { get; set; } = string.Empty;
    public string 이름암호문 { get; set; } = string.Empty;
    public string 별명암호문 { get; set; } = string.Empty;
    public string 이메일암호문 { get; set; } = string.Empty;
    public string 전화번호암호문 { get; set; } = string.Empty;
    public string 성별암호문 { get; set; } = string.Empty;
    public string 출생연도암호문 { get; set; } = string.Empty;
    public string 거주국가암호문 { get; set; } = string.Empty;
    public bool 회원가입확인 { get; set; }
    public bool 입교서약동의 { get; set; }
    public bool 개인정보수집이용동의 { get; set; }
    public bool 개인정보제3자제공동의 { get; set; }
    public string 개인정보동의버전 { get; set; } = string.Empty;
    public string 제3자제공동의버전 { get; set; } = string.Empty;
    public DateTime 동의일시Utc { get; set; }
    public string 상태 { get; set; } = 교육과정신청상태.검토대기;
    public string? 심사자UserId { get; set; }
    public string 심사메모암호문 { get; set; } = string.Empty;
    public DateTime 신청일시Utc { get; set; } = DateTime.UtcNow;
    public DateTime? 심사일시Utc { get; set; }
    public DateTime? 개인정보삭제일시Utc { get; set; }

    public 교육과정등록? 등록 { get; set; }
}

public sealed class 교육과정등록
{
    public long Id { get; set; }
    public long 교육과정Id { get; set; }
    public 교육과정? 교육과정 { get; set; }
    public long 교육과정신청Id { get; set; }
    public 교육과정신청? 교육과정신청 { get; set; }
    public string 참여자UserId { get; set; } = string.Empty;
    public string? 담당멘토UserId { get; set; }
    public string 상태 { get; set; } = 교육과정등록상태.진행중;
    public DateTime 시작일시Utc { get; set; }
    public DateTime? 종료일시Utc { get; set; }
    public DateTime 생성일시Utc { get; set; } = DateTime.UtcNow;

    public ICollection<교육과정참석기록> 참석목록 { get; set; } = new List<교육과정참석기록>();
    public ICollection<교육과정과제제출> 제출목록 { get; set; } = new List<교육과정과제제출>();
}

public sealed class 교육과정참석기록
{
    public long Id { get; set; }
    public long 교육과정등록Id { get; set; }
    public 교육과정등록? 교육과정등록 { get; set; }
    public long 교육과정과목Id { get; set; }
    public 교육과정과목? 교육과정과목 { get; set; }
    public string 회차Key { get; set; } = string.Empty;
    public string 회차명 { get; set; } = string.Empty;
    public DateTime 수업일시Utc { get; set; }
    public bool 참석여부 { get; set; }
    public string 기록자UserId { get; set; } = string.Empty;
    public DateTime 기록일시Utc { get; set; } = DateTime.UtcNow;
}

public sealed class 교육과정과제제출
{
    public long Id { get; set; }
    public long 교육과정등록Id { get; set; }
    public 교육과정등록? 교육과정등록 { get; set; }
    public long 교육과정양식Id { get; set; }
    public 교육과정양식? 교육과정양식 { get; set; }
    public string 제출기간Key { get; set; } = string.Empty;
    public string 답변암호문 { get; set; } = string.Empty;
    public string 상태 { get; set; } = 교육과정제출상태.제출;
    public string? 확인자UserId { get; set; }
    public string 확인메모암호문 { get; set; } = string.Empty;
    public DateTime 제출일시Utc { get; set; } = DateTime.UtcNow;
    public DateTime? 확인일시Utc { get; set; }
}
