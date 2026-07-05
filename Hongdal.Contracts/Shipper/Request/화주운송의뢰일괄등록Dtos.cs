namespace Hongdal.Contracts.Shipper.Request;

public sealed class 화주운송의뢰일괄등록행입력
{
    public int 행번호 { get; set; }
    public string? 화주Id { get; set; }
    public string 화물종류 { get; set; } = string.Empty;
    public string? 화물설명 { get; set; }
    public int? 화물수량 { get; set; }
    public int? 화물길이Mm { get; set; }
    public int? 화물폭Mm { get; set; }
    public int? 화물높이Mm { get; set; }
    public decimal? 화물중량Kg { get; set; }
    public decimal? 화물부피Cbm { get; set; }
    public int? 팔레트개수 { get; set; }
    public bool 화물파손주의여부 { get; set; }
    public string? 화물온도조건 { get; set; }
    public string? 픽업도로명주소 { get; set; }
    public string? 픽업상세주소 { get; set; }
    public string? 하차도로명주소 { get; set; }
    public string? 하차상세주소 { get; set; }
    public string? 운송방식 { get; set; }
    public string? 차량종류 { get; set; }
    public string? 서비스레벨 { get; set; }
    public string? 요청사항 { get; set; }
    public string? 결제수단 { get; set; }
    public string? 정산시점 { get; set; }
    public string? 증빙방식 { get; set; }
    public string? 수납주체 { get; set; }
    public string? 클라이언트행Id { get; set; }
}

public sealed class 화주운송의뢰추천결과
{
    public string 운송방식 { get; set; } = string.Empty;
    public string 차량종류 { get; set; } = string.Empty;
    public string 결제수단 { get; set; } = string.Empty;
    public string 정산시점 { get; set; } = string.Empty;
    public string? 증빙방식 { get; set; }
    public string? 수납주체 { get; set; }
    public string? 추천사유 { get; set; }
    public decimal? 추정화물부피Cbm { get; set; }
    public IReadOnlyList<string> 추천사유목록 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> 경고목록 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<차량추천후보응답> 후보차량목록 { get; set; } = Array.Empty<차량추천후보응답>();
}

public sealed class 화주운송의뢰일괄미리보기행응답
{
    public int 행번호 { get; set; }
    public bool 유효함 { get; set; }
    public bool 등록대상여부 { get; set; } = true;
    public string? 최종선택차량종류 { get; set; }
    public 화주운송의뢰일괄등록행입력 원본행 { get; set; } = new();
    public 화주운송의뢰추천결과? 추천결과 { get; set; }
    public IReadOnlyList<string> 오류목록 { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> 경고목록 { get; set; } = Array.Empty<string>();
}

public sealed class 화주운송의뢰일괄미리보기응답
{
    public int 전체행수 { get; set; }
    public int 유효행수 { get; set; }
    public int 오류행수 { get; set; }
    public List<화주운송의뢰일괄미리보기행응답> 행목록 { get; set; } = [];
}

public sealed class 화주운송의뢰일괄확정등록행
{
    public int 행번호 { get; set; }
    public bool 등록여부 { get; set; } = true;
    public string? 최종선택차량종류 { get; set; }
    public 화주운송의뢰일괄등록행입력 원본행 { get; set; } = new();
}

public sealed class 화주운송의뢰일괄확정등록요청
{
    public IReadOnlyList<화주운송의뢰일괄확정등록행> 행목록 { get; set; } = Array.Empty<화주운송의뢰일괄확정등록행>();
}

public sealed class 화주운송의뢰일괄등록행결과
{
    public int 행번호 { get; set; }
    public bool 성공 { get; set; }
    public string? 의뢰Id { get; set; }
    public 화주운송의뢰추천결과? 추천결과 { get; set; }
    public List<string> 오류 { get; set; } = [];
}

public sealed class 화주운송의뢰일괄등록결과응답
{
    public int 전체행수 { get; set; }
    public int 성공행수 { get; set; }
    public int 실패행수 { get; set; }
    public List<화주운송의뢰일괄등록행결과> 행결과목록 { get; set; } = [];
}

public static class 화주운송의뢰일괄등록템플릿
{
    public static readonly string[] 헤더 =
    [
        "행번호",
        "화주Id",
        "화물종류",
        "화물설명",
        "화물수량",
        "화물길이Mm",
        "화물폭Mm",
        "화물높이Mm",
        "화물중량Kg",
        "화물부피Cbm",
        "팔레트개수",
        "화물파손주의여부",
        "화물온도조건",
        "픽업도로명주소",
        "픽업상세주소",
        "하차도로명주소",
        "하차상세주소",
        "운송방식",
        "차량종류",
        "서비스레벨",
        "요청사항",
        "결제수단",
        "정산시점",
        "증빙방식",
        "수납주체",
        "클라이언트행Id"
    ];
}
