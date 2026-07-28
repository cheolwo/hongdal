using Ssalddel.Contracts.Common.Documents;
using Ssalddel.Contracts.Food;

namespace RestaurantDeskApp.Models.Restaurant;

public static class 음식점주문Desk상태코드
{
    public const string 주문대기 = "주문대기";
    public const string 수락처리중 = "수락처리중";
    public const string 수락됨 = "수락됨";
    public const string 전표출력됨 = "전표출력됨";
    public const string 상세조회실패 = "상세조회실패";
}

public enum 음식점주문복구출처
{
    실시간,
    서버재조회,
    재연결재조회
}

public enum 음식점실시간연결상태
{
    연결대기,
    연결중,
    연결됨,
    재연결중,
    연결끊김,
    인증필요
}

public sealed record 음식점실시간연결상태변경(
    음식점실시간연결상태 상태,
    string 안내);

public sealed class 음식점주문수신Payload
{
    public string 주문번호 { get; init; } = string.Empty;

    public long 음식점Id { get; init; }

    public string 고객명 { get; init; } = string.Empty;

    public string 메뉴요약 { get; init; } = string.Empty;

    public IReadOnlyList<음식주문상품Dto> 상품목록 { get; init; } = [];

    public decimal 주문금액 { get; init; }

    public DateTimeOffset 수신시각 { get; init; } = DateTimeOffset.Now;

    public string 제목 { get; init; } = "신규 주문";

    public string 본문 { get; init; } = string.Empty;
}

public sealed class 음식점주문DeskItem
{
    public long Id { get; set; }

    public string 주문번호 { get; set; } = string.Empty;

    public long 음식점Id { get; set; }

    public string 고객명 { get; set; } = string.Empty;

    public string 메뉴요약 { get; set; } = string.Empty;

    public IReadOnlyList<음식주문상품Dto> 상품목록 { get; set; } = [];

    public IReadOnlyList<음식점주문상품조리기준> 상품별조리기준 { get; set; } = [];

    public decimal 주문금액 { get; set; }

    public DateTimeOffset 접수시각 { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? 수락시각 { get; set; }

    public DateTimeOffset? 전표출력시각 { get; set; }

    public int 추천조리예상분 { get; set; }

    public int? 선택조리예상분 { get; set; }

    public string 배차상태 { get; set; } = 음식주문배차상태코드.미요청;

    public DateTime? 배차요청시각Utc { get; set; }

    public string 상태 { get; set; } = 음식점주문Desk상태코드.주문대기;

    public bool 미확인 { get; set; } = true;

    public string? 최근메시지 { get; set; }

    public 음식점주문복구출처 복구출처 { get; set; } = 음식점주문복구출처.서버재조회;

    public DateTimeOffset 최근복구시각 { get; set; } = DateTimeOffset.Now;

    public 음식주문응답? 상세주문 { get; set; }

    public bool 수락가능 => 상태 is 음식점주문Desk상태코드.주문대기 or 음식점주문Desk상태코드.상세조회실패;
}

public sealed record 음식점주문상품조리기준(
    string 상품명,
    int 수량,
    int 기본조리분,
    bool 음식점기본값사용);

public sealed class 음식점주문수락결과
{
    public bool 성공 { get; init; }

    public string 메시지 { get; init; } = string.Empty;

    public 음식점주문DeskItem? 주문 { get; init; }

    public 음식주문응답? 상세주문 { get; init; }

    public SsalddelExpectedItemDocumentDraft? 전표Draft { get; init; }
}
