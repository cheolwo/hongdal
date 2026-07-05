using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.Services.Documents;

public static class 문서상태값
{
    public const string 생성대기 = "생성대기";
    public const string 생성완료 = "생성완료";
    public const string 실패 = "실패";
    public const string 폐기 = "폐기";

    public const string 조회 = "조회";
    public const string 다운로드 = "다운로드";
}

[Table("문서종류정책")]
public sealed class 문서종류정책
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("문서코드")]
    [MaxLength(100)]
    public string 문서코드 { get; set; } = string.Empty;

    [Column("문서명")]
    [MaxLength(200)]
    public string 문서명 { get; set; } = string.Empty;

    [Column("사용여부")]
    public bool 사용여부 { get; set; }

    [Column("암호화여부")]
    public bool 암호화여부 { get; set; }

    [Column("다운로드허용여부")]
    public bool 다운로드허용여부 { get; set; }

    [Column("서명필요여부")]
    public bool 서명필요여부 { get; set; }

    [Column("자동생성시점")]
    [MaxLength(100)]
    public string 자동생성시점 { get; set; } = string.Empty;

    [Column("조회가능역할목록_json")]
    public string 조회가능역할목록Json { get; set; } = string.Empty;

    [Column("보관일수")]
    public int 보관일수 { get; set; }

    [Column("수정가능여부")]
    public bool 수정가능여부 { get; set; }

    [Column("감사로그여부")]
    public bool 감사로그여부 { get; set; }

    [Column("생성일시")]
    public DateTime 생성일시 { get; set; } = DateTime.UtcNow;

    [Column("수정일시")]
    public DateTime? 수정일시 { get; set; }
}

[Table("운송문서")]
public sealed class 운송문서
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("의뢰_id")]
    [MaxLength(100)]
    public string 의뢰Id { get; set; } = string.Empty;

    [Column("배송_운송_id")]
    public long? 배송운송Id { get; set; }

    [Column("문서코드")]
    [MaxLength(100)]
    public string 문서코드 { get; set; } = string.Empty;

    [Column("문서명")]
    [MaxLength(200)]
    public string 문서명 { get; set; } = string.Empty;

    [Column("파일명")]
    [MaxLength(260)]
    public string 파일명 { get; set; } = string.Empty;

    [Column("파일경로")]
    [MaxLength(600)]
    public string 파일경로 { get; set; } = string.Empty;

    [Column("content_type")]
    [MaxLength(200)]
    public string ContentType { get; set; } = "application/pdf";

    [Column("암호화됨")]
    public bool 암호화됨 { get; set; }

    [Column("암호화키식별자")]
    [MaxLength(200)]
    public string 암호화키식별자 { get; set; } = string.Empty;

    [Column("생성상태")]
    [MaxLength(100)]
    public string 생성상태 { get; set; } = 문서상태값.생성대기;

    [Column("다운로드허용여부")]
    public bool 다운로드허용여부 { get; set; }

    [Column("수정가능여부")]
    public bool 수정가능여부 { get; set; }

    [Column("보관만료일시")]
    public DateTime? 보관만료일시 { get; set; }

    [Column("생성일시")]
    public DateTime 생성일시 { get; set; } = DateTime.UtcNow;

    [Column("수정일시")]
    public DateTime? 수정일시 { get; set; }

    [Column("생성자")]
    [MaxLength(200)]
    public string 생성자 { get; set; } = string.Empty;
}

[Table("문서조회로그")]
public sealed class 문서조회로그
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("문서_id")]
    public long 문서Id { get; set; }

    [Column("행위")]
    [MaxLength(50)]
    public string 행위 { get; set; } = string.Empty;

    [Column("사용자_id")]
    [MaxLength(200)]
    public string 사용자Id { get; set; } = string.Empty;

    [Column("사용자명")]
    [MaxLength(200)]
    public string 사용자명 { get; set; } = string.Empty;

    [Column("역할명")]
    [MaxLength(200)]
    public string 역할명 { get; set; } = string.Empty;

    [Column("클라이언트_ip")]
    [MaxLength(100)]
    public string ClientIp { get; set; } = string.Empty;

    [Column("user_agent")]
    [MaxLength(1000)]
    public string UserAgent { get; set; } = string.Empty;

    [Column("생성일시")]
    public DateTime 생성일시 { get; set; } = DateTime.UtcNow;
}

public sealed class 문서조회요약응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public long? 배송운송Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 파일명 { get; set; } = string.Empty;
    public string 생성상태 { get; set; } = string.Empty;
    public bool 암호화됨 { get; set; }
    public bool 다운로드허용여부 { get; set; }
    public bool 수정가능여부 { get; set; }
    public DateTime 생성일시 { get; set; }
    public DateTime? 보관만료일시 { get; set; }
}

public sealed class 문서정책요약응답
{
    public long Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public bool 사용여부 { get; set; }
    public bool 암호화여부 { get; set; }
    public bool 다운로드허용여부 { get; set; }
    public bool 서명필요여부 { get; set; }
    public string 자동생성시점 { get; set; } = string.Empty;
    public string 조회가능역할목록Json { get; set; } = string.Empty;
    public int 보관일수 { get; set; }
    public bool 수정가능여부 { get; set; }
    public bool 감사로그여부 { get; set; }
    public DateTime 생성일시 { get; set; }
    public DateTime? 수정일시 { get; set; }
}

public sealed class 문서조회로그요약응답
{
    public long Id { get; set; }
    public long 문서Id { get; set; }
    public string 행위 { get; set; } = string.Empty;
    public string 사용자Id { get; set; } = string.Empty;
    public string 사용자명 { get; set; } = string.Empty;
    public string 역할명 { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime 생성일시 { get; set; }
}

public sealed class 문서정책수정요청
{
    public bool 사용여부 { get; set; }
    public bool 암호화여부 { get; set; }
    public bool 다운로드허용여부 { get; set; }
    public bool 서명필요여부 { get; set; }
    public string 자동생성시점 { get; set; } = string.Empty;
    public string 조회가능역할목록Json { get; set; } = string.Empty;
    public int 보관일수 { get; set; }
    public bool 수정가능여부 { get; set; }
    public bool 감사로그여부 { get; set; }
}

public sealed class 문서생성요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public long? 배송운송Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 파일명 { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public bool? 암호화여부 { get; set; }
    public bool? 다운로드허용여부 { get; set; }
    public string? 생성자 { get; set; }
}

public sealed class 문서다운로드응답
{
    public long Id { get; set; }
    public string 파일명 { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] 내용 { get; set; } = [];
}
