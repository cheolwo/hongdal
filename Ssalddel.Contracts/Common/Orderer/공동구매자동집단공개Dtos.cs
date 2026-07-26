namespace Ssalddel.Contracts.Common.Orderer;

/// <summary>
/// 자동집단 목록에서 공개할 수 있는 집계 정보입니다.
/// 참여자 식별자, 주소, 결제 금액과 내부 원장 식별자는 포함하지 않습니다.
/// </summary>
public class 공동구매자동집단요약응답
{
    public string 자동집단Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string HS코드 { get; set; } = string.Empty;
    public string 온도코드 { get; set; } = string.Empty;
    public string 물류방식 { get; set; } = string.Empty;
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public string 가격표시기준 { get; set; } = 공동구매가격표시기준코드.부가세포함;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 현재상태 { get; set; } = 공동구매자동집단상태코드.수요수집중;
    public int 수요건수 { get; set; }
    public int 예약결제건수 { get; set; }
    public int 참여자수 { get; set; }
    public int 예약결제참여자수 { get; set; }
    public decimal 총희망수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public int? 목표참여자수 { get; set; }
    public decimal? 목표수량 { get; set; }
    public DateTime 모집종료시각Utc { get; set; }
    public bool 모집종료여부 { get; set; }
    public bool 모집조건충족여부 { get; set; }
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

/// <summary>
/// 배송권 안에서 진행 중인 같이 주문을 상세 확인할 때 사용하는 공개 집계 응답입니다.
/// 다른 참여자의 식별자, 상세주소, 개별 수량, 결제와 내부 원장 정보는 포함하지 않습니다.
/// </summary>
public sealed class 같이주문공개상세응답 : 공동구매자동집단요약응답
{
    public string 같이주문표시명 { get; set; } = 같이주문용어.표시명;
    public bool 참여가능여부 { get; set; }
    public int? 추가필요참여자수 { get; set; }
    public decimal? 추가필요수량 { get; set; }
    public bool 비구속수요만허용 { get; set; } = true;
    public bool 자동참여금지 { get; set; } = true;
    public bool 별도동의필수 { get; set; } = true;
    public string 공개범위안내 { get; set; } =
        "배송권과 집계 인원만 공개하며 참여자의 이름, 상세주소와 개별 주문정보는 공개하지 않습니다.";
    public string 참여안내 { get; set; } =
        "비용과 대기시간을 확인한 뒤 참여 의향을 직접 선택합니다. 상세 보기만으로 주문·결제·배송이 시작되지 않습니다.";
    public string 배송권보기경로 { get; set; } = string.Empty;
    public string 주문방식비교경로 { get; set; } = string.Empty;
}

public sealed class 공동구매자동집단배치미리보기응답
{
    public string 정책버전 { get; set; } = 공동구매주문자집단화정책코드.현재버전;
    public string 자동집단Id { get; set; } = string.Empty;
    public string 배치유형 { get; set; } = 공동구매자동집단배치유형코드.신규집단;
    public bool 기존수요갱신여부 { get; set; }
    public IReadOnlyList<공동구매자동집단배치기준응답> 적용기준목록 { get; set; } = [];
    public 공동구매자동집단진행응답 현재진행 { get; set; } = new();
    public 공동구매자동집단진행응답 예상진행 { get; set; } = new();
    public string 안내 { get; set; } = string.Empty;
    public string 비구속안내 { get; set; } = string.Empty;
}

public sealed class 공동구매자동집단배치기준응답
{
    public string 기준코드 { get; set; } = string.Empty;
    public string 기준값 { get; set; } = string.Empty;
}

public sealed class 공동구매자동집단진행응답
{
    public string 현재상태 { get; set; } = 공동구매자동집단상태코드.수요수집중;
    public int 수요건수 { get; set; }
    public int 예약결제건수 { get; set; }
    public int 참여자수 { get; set; }
    public int 예약결제참여자수 { get; set; }
    public decimal 총희망수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public int? 목표참여자수 { get; set; }
    public decimal? 목표수량 { get; set; }
    public int? 추가필요참여자수 { get; set; }
    public decimal? 추가필요수량 { get; set; }
    public DateTime 모집종료시각Utc { get; set; }
    public bool 모집종료여부 { get; set; }
    public bool 모집조건충족여부 { get; set; }
    public bool 확정검토가능 { get; set; }
    public string 다음단계코드 { get; set; } = 공동구매자동집단다음단계코드.수요추가모집;
    public string 안내 { get; set; } = string.Empty;
}

/// <summary>
/// 수요를 등록한 사용자에게 반환하는 자동집단 응답입니다.
/// 기존 클라이언트와의 JSON 호환을 위해 본인 수요는 <c>수요목록</c>에 한 건만 담습니다.
/// </summary>
public sealed class 공동구매자동집단사용자응답 : 공동구매자동집단요약응답
{
    public string 공동구매주문집계원장Id { get; set; } = string.Empty;
    public IReadOnlyList<공동구매자동본인수요응답> 수요목록 { get; set; } = [];
}

/// <summary>
/// 로그인 사용자가 방금 등록한 본인 수요입니다.
/// 표시명과 주소 참조는 본인 응답에서도 불필요하므로 반환하지 않습니다.
/// </summary>
public sealed class 공동구매자동본인수요응답
{
    public string 수요Id { get; set; } = string.Empty;
    public string 수요출처키 { get; set; } = string.Empty;
    public long? 커뮤니티게시글Id { get; set; }
    public string 개별원함원장Id { get; set; } = string.Empty;
    public string 자동집단Id { get; set; } = string.Empty;
    public string 상품키 { get; set; } = string.Empty;
    public string 상품명 { get; set; } = string.Empty;
    public string 거래유형 { get; set; } = 공동구매거래유형코드.B2C;
    public string 가격표시기준 { get; set; } = 공동구매가격표시기준코드.부가세포함;
    public string 구매조직참조키 { get; set; } = string.Empty;
    public string 구매조직표시명 { get; set; } = string.Empty;
    public string 사업자검증상태 { get; set; } = 주문자집단사업자검증상태코드.불필요;
    public bool 세금계산서필요 { get; set; }
    public string 주문자키 { get; set; } = string.Empty;
    public string 배송권키 { get; set; } = string.Empty;
    public string 배송권명 { get; set; } = string.Empty;
    public string 입고의미상태 { get; set; } = 공동구매개별주문입고상태코드.미지정;
    public string 공동구매주문집계원장Id { get; set; } = string.Empty;
    public string 개별주문원장Id { get; set; } = string.Empty;
    public string 입고예정원장Id { get; set; } = string.Empty;
    public string 수요유형 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public decimal 희망수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public decimal? 예약결제금액 { get; set; }
    public DateTime 생성시각Utc { get; set; }
}

public static class 공동구매주문자집단화정책코드
{
    public const string 현재버전 = "orderer-grouping-v2-buyer-context";
}

public static class 공동구매자동집단배치유형코드
{
    public const string 신규집단 = "NewGroup";
    public const string 기존집단 = "ExistingGroup";
}

public static class 공동구매자동집단배치기준코드
{
    public const string 상품키 = "ProductKey";
    public const string 배송권 = "DeliveryScope";
    public const string 보관온도 = "Temperature";
    public const string 물류방식 = "LogisticsMode";
    public const string 거래유형 = "TransactionType";
    public const string 가격표시기준 = "PriceBasis";
}

public static class 공동구매자동집단다음단계코드
{
    public const string 수요추가모집 = "CollectMoreDemand";
    public const string 확정검토 = "ReviewConfirmation";
    public const string 확정완료 = "Confirmed";
    public const string 모집종료 = "RecruitmentClosed";
}

public static class 공동구매자동집단모집정책
{
    public const int 기본모집일수 = 14;

    public static DateTime 기본모집종료시각Utc(DateTime 모집시작시각Utc)
    {
        if (모집시작시각Utc == default)
        {
            throw new ArgumentException("모집 시작 시각이 필요합니다.", nameof(모집시작시각Utc));
        }

        var 시작시각Utc = 모집시작시각Utc.Kind switch
        {
            DateTimeKind.Utc => 모집시작시각Utc,
            DateTimeKind.Local => 모집시작시각Utc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(모집시작시각Utc, DateTimeKind.Utc)
        };
        return 시작시각Utc.AddDays(기본모집일수);
    }
}
