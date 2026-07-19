namespace SsalddelAdmin.Services;

public sealed class 관리자대시보드요약응답
{
    public int 오늘의뢰수 { get; set; }
    public int 결제대기수 { get; set; }
    public int 결제완료수 { get; set; }
    public int 배차대기수 { get; set; }
    public int 배차확정수 { get; set; }
    public int 운송중수 { get; set; }
    public int 완료수 { get; set; }
    public int 취소환불수 { get; set; }
    public int 운송예외수 { get; set; }
    public int 관리자확인필요수 { get; set; }
}

public sealed class 화주운송의뢰응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 의뢰상태 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string 정산상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;
    public string 운송방식 { get; set; } = string.Empty;
    public string 결제수단 { get; set; } = string.Empty;
    public string 정산시점 { get; set; } = string.Empty;
    public string 증빙방식 { get; set; } = string.Empty;
    public string 수납주체 { get; set; } = string.Empty;
    public bool 세금계산서필요 { get; set; }
    public bool 현금영수증필요 { get; set; }
    public string? 정산메모 { get; set; }
    public DateTime 생성일시 { get; set; }
    public string 픽업지 { get; set; } = string.Empty;
    public string 픽업상세지 { get; set; } = string.Empty;
    public decimal? 픽업위도 { get; set; }
    public decimal? 픽업경도 { get; set; }
    public string 하차지 { get; set; } = string.Empty;
    public string 하차상세지 { get; set; } = string.Empty;
    public decimal? 하차위도 { get; set; }
    public decimal? 하차경도 { get; set; }
    public decimal? 대기료 { get; set; }
    public decimal? 수작업비 { get; set; }
    public decimal? 할증 { get; set; }
    public decimal? 최종운임 { get; set; }
    public 의뢰요약? 요약 { get; set; }
}

public sealed class 의뢰요약
{
    public string 화물종류 { get; set; } = string.Empty;
    public string 픽업지 { get; set; } = string.Empty;
    public string 하차지 { get; set; } = string.Empty;
}

public sealed class 화주운송의뢰수정요청
{
    public string? 상태 { get; set; }
    public string? 결제상태 { get; set; }
    public string? 배차상태 { get; set; }
}

public sealed class 결제목록응답
{
    public string 결제Id { get; set; } = string.Empty;
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
    public int 결제금액 { get; set; }
    public string 결제수단 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string? PaymentKey { get; set; }
    public string? Toss응답Json { get; set; }
    public DateTime 생성일시Utc { get; set; }
    public DateTime? 승인일시Utc { get; set; }
}

public sealed class 토스결제환경응답
{
    public string ClientKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
}

public sealed class 배차대기응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
    public int? 배차업무유형 { get; set; }
    public string? 원본의뢰유형 { get; set; }
    public string? 원본의뢰Id { get; set; }
    public string? 공동구매도착지유형코드 { get; set; }
    public bool? 공동구매기사세대배송여부 { get; set; }
    public string? 공동구매세대배송방식코드 { get; set; }
    public int? 공동구매세대배송건수 { get; set; }
    public string? 공동구매분배책임코드 { get; set; }
    public string 픽업_도로명주소 { get; set; } = string.Empty;
    public string 픽업_상세주소 { get; set; } = string.Empty;
    public decimal? 픽업_위도 { get; set; }
    public decimal? 픽업_경도 { get; set; }
    public string 하차_도로명주소 { get; set; } = string.Empty;
    public string 하차_상세주소 { get; set; } = string.Empty;
    public decimal? 하차_위도 { get; set; }
    public decimal? 하차_경도 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 배차대기수정요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
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

public sealed class 기사목록응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
    public int 배차건수 { get; set; }
}

public sealed class 기사상세응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
}

public sealed class 기사배차내역응답
{
    public long Id { get; set; }
    public string 배차명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 배차일 { get; set; }
    public string 픽업지 { get; set; } = string.Empty;
    public string 배송지 { get; set; } = string.Empty;
    public decimal? 배차점수 { get; set; }
    public string 실패사유 { get; set; } = string.Empty;
    public DateTime? 배차생성시각 { get; set; }
    public DateTime? 배차완료시각 { get; set; }
}

public sealed class 기사월정산관리응답
{
    public string 기사Id { get; set; } = string.Empty;
    public int 년도 { get; set; }
    public int 월 { get; set; }
    public int 배차건수 { get; set; }
    public decimal 이용료 { get; set; }
    public bool 월상한적용여부 { get; set; }
    public bool 결제완료 { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 운송진행응답
{
    public long Id { get; set; }
    public string 운송번호 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 출발_픽업 { get; set; }
    public DateTime? 도착 { get; set; }
    public string 기사_운송자 { get; set; } = string.Empty;
    public string 출발지 { get; set; } = string.Empty;
    public string 도착지 { get; set; } = string.Empty;
    public decimal? 운임 { get; set; }
    public bool 예외신고됨 { get; set; }
    public string 최근예외단계 { get; set; } = string.Empty;
    public string 최근예외코드 { get; set; } = string.Empty;
    public string 최근예외메시지 { get; set; } = string.Empty;
    public bool 관리자확인필요 { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class 운송이벤트로그응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public string 이벤트타입 { get; set; } = string.Empty;
    public DateTime 이벤트시각 { get; set; }
    public string 메타데이터 { get; set; } = string.Empty;
}

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

public sealed class 파일POD응답
{
    public Guid Id { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string UploadStatus { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 파일POD상태변경요청
{
    public string UploadStatus { get; set; } = string.Empty;
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

public sealed class 문서조회요약응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public long? 운송원장Id { get; set; }
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

public sealed class 문서생성요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public long? 운송원장Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 파일명 { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public bool? 암호화여부 { get; set; }
    public bool? 다운로드허용여부 { get; set; }
    public string? 생성자 { get; set; }
}

public sealed class 공개화물요약응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
    public int? 화물수량 { get; set; }
    public decimal? 화물중량Kg { get; set; }
    public string 운송방식 { get; set; } = string.Empty;
    public string 차량종류 { get; set; } = string.Empty;
    public string 의뢰상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;
    public DateTime 생성일시 { get; set; }
}
