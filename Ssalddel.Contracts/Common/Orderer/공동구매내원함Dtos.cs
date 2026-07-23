namespace Ssalddel.Contracts.Common.Orderer;

/// <summary>
/// 로그인 주문자가 소유한 개별 공동구매 원함 목록입니다.
/// 다른 주문자의 식별자, 주소, 결제 정보는 포함하지 않습니다.
/// </summary>
public sealed class 공동구매내원함목록응답
{
    public int 전체건수 { get; set; }
    public int 활성건수 { get; set; }
    public int 닫힘건수 { get; set; }
    public IReadOnlyList<공동구매내원함응답> 원함목록 { get; set; } = [];
}

/// <summary>
/// 로그인 주문자 본인의 개별 원함과 연결된 자동집단의 공개 집계 요약입니다.
/// </summary>
public sealed class 공동구매내원함응답
{
    public string 개별원함원장Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string 수요출처키 { get; set; } = string.Empty;
    public string 원함상태 { get; set; } = 공동구매내원함상태코드.활성;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string HS코드 { get; set; } = string.Empty;
    public decimal 희망수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 물류방식 { get; set; } = 공동구매자동수요물류방식코드.후속검토;
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public string 가격표시기준 { get; set; } = 공동구매가격표시기준코드.부가세포함;
    public string 구매조직참조키 { get; set; } = string.Empty;
    public string 구매조직표시명 { get; set; } = string.Empty;
    public bool 세금계산서필요 { get; set; }
    public int? 목표참여자수 { get; set; }
    public decimal? 목표수량 { get; set; }
    public string 자동집단Id { get; set; } = string.Empty;
    public string 공동구매주문집계원장Id { get; set; } = string.Empty;
    public string 개별주문원장Id { get; set; } = string.Empty;
    public string 공동수입원장Id { get; set; } = string.Empty;
    public 공동구매자동집단요약응답? 자동집단요약 { get; set; }
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public static class 공동구매내원함상태코드
{
    public const string 활성 = "Active";
    public const string 닫힘 = "Closed";
}
