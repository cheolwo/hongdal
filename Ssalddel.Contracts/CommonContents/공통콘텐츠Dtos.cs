namespace Ssalddel.Contracts.CommonContents;

public enum 계약살뜰콘텐츠유형
{
    이미지 = 1,
    영상링크 = 2,
    외부링크 = 3
}

[Flags]
public enum 계약살뜰노출위치
{
    없음 = 0,
    홈화면위젯 = 1,
    잠금화면위젯 = 2,
    결제전혜택 = 4,
    앱공지 = 8
}

public enum 계약살뜰보상유형
{
    없음 = 0,
    포인트 = 1,
    할인율 = 2,
    할인금액 = 3
}

public sealed class 공통콘텐츠보상정책Dto
{
    public long Id { get; set; }
    public 계약살뜰보상유형 보상유형 { get; set; }
    public int 지급포인트 { get; set; }
    public decimal 할인율 { get; set; }
    public int 할인금액 { get; set; }
    public int 최소시청초 { get; set; }
    public decimal 필요시청비율 { get; set; } = 0.8m;
    public bool 사용자당1회만지급 { get; set; } = true;
    public int? 최대할인금액 { get; set; }
}

public sealed class 관리자공통콘텐츠저장요청
{
    public string 제목 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public 계약살뜰콘텐츠유형 콘텐츠유형 { get; set; }
    public string? 이미지Url { get; set; }
    public string? 영상Url { get; set; }
    public string? 외부링크Url { get; set; }
    public 계약살뜰노출위치 노출위치 { get; set; }
    public bool 기사노출 { get; set; }
    public bool 화주노출 { get; set; }
    public bool 운영자노출 { get; set; }
    public bool 활성화여부 { get; set; } = true;
    public DateTimeOffset? 노출시작시각 { get; set; }
    public DateTimeOffset? 노출종료시각 { get; set; }
    public long? 보상정책Id { get; set; }
}

public sealed class 관리자공통콘텐츠요약응답
{
    public long Id { get; set; }
    public string 제목 { get; set; } = string.Empty;
    public 계약살뜰콘텐츠유형 콘텐츠유형 { get; set; }
    public 계약살뜰노출위치 노출위치 { get; set; }
    public bool 활성화여부 { get; set; }
    public DateTimeOffset 생성시각 { get; set; }
}

public sealed class 관리자공통콘텐츠상세응답
{
    public long Id { get; set; }
    public string 제목 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public 계약살뜰콘텐츠유형 콘텐츠유형 { get; set; }
    public string? 이미지Url { get; set; }
    public string? 영상Url { get; set; }
    public string? 외부링크Url { get; set; }
    public 계약살뜰노출위치 노출위치 { get; set; }
    public bool 기사노출 { get; set; }
    public bool 화주노출 { get; set; }
    public bool 운영자노출 { get; set; }
    public bool 활성화여부 { get; set; }
    public DateTimeOffset? 노출시작시각 { get; set; }
    public DateTimeOffset? 노출종료시각 { get; set; }
    public 공통콘텐츠보상정책Dto? 보상정책 { get; set; }
    public DateTimeOffset 생성시각 { get; set; }
}

public sealed class 살뜰위젯콘텐츠Dto
{
    public long 콘텐츠Id { get; set; }
    public string 제목 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public string? 이미지Url { get; set; }
    public string? 이동Url { get; set; }
    public string 상태문구 { get; set; } = string.Empty;
}

public sealed class 콘텐츠시청시작Request
{
    public int 영상전체초 { get; set; }
}

public sealed class 콘텐츠시청시작Result
{
    public long 세션Id { get; set; }
}

public sealed class 콘텐츠시청진행Request
{
    public int 현재시청초 { get; set; }
}

public sealed class 콘텐츠시청완료Result
{
    public bool 완료여부 { get; set; }
    public bool 보상지급여부 { get; set; }
    public string 메시지 { get; set; } = string.Empty;
    public int 지급포인트 { get; set; }
    public decimal 할인율 { get; set; }
    public int 할인금액 { get; set; }
}

public sealed class 결제혜택견적응답
{
    public int 원운임 { get; set; }
    public int 포인트사용가능액 { get; set; }
    public int 콘텐츠할인금액 { get; set; }
    public int 최종결제금액 { get; set; }
}
