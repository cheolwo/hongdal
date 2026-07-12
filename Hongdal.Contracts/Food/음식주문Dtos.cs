using Hongdal.Contracts.Common.Participants;

namespace Hongdal.Contracts.Food;

public static class 음식주문상태코드
{
    public const string 주문대기 = "주문대기";
    public const string 조리중 = "조리중";
    public const string 픽업대기 = "픽업대기";
    public const string 기사배정 = "기사배정";
    public const string 픽업완료 = "픽업완료";
    public const string 전달완료 = "전달완료";
    public const string 취소 = "취소";

    public static string Normalize(string? value)
        => value?.Trim() switch
        {
            "주문접수" => 주문대기,
            주문대기 => 주문대기,
            조리중 => 조리중,
            픽업대기 => 픽업대기,
            기사배정 => 기사배정,
            픽업완료 => 픽업완료,
            전달완료 => 전달완료,
            취소 => 취소,
            _ => 주문대기
        };

    public static bool CanRestaurantAccept(string? value)
        => Normalize(value) == 주문대기;
}

public static class 음식주문배차상태코드
{
    public const string 미요청 = "미요청";
    public const string 배차대기 = "배차대기";
    public const string 추천중 = "추천중";
    public const string 기사배정 = "기사배정";
    public const string 배차불가 = "배차불가";
}

public sealed class 음식주문상품Dto
{
    public string 상품명 { get; set; } = string.Empty;
    public int 수량 { get; set; }
    public decimal 단가 { get; set; }
}

public sealed class 음식주문등록요청
{
    public long 음식점Id { get; set; }
    public string 주문자UserId { get; set; } = string.Empty;
    public 음식주문수령인정보Dto 수령인정보 { get; set; } = new();
    public IReadOnlyList<음식주문상품Dto> 상품목록 { get; set; } = [];
    public string? 결제수단 { get; set; }
}

public sealed class 음식점주문수락요청
{
    public string? 처리UserId { get; set; }
    public string 음식점명 { get; set; } = string.Empty;
    public string 음식점주소 { get; set; } = string.Empty;
    public string 음식점상세주소 { get; set; } = string.Empty;
    public decimal? 음식점위도 { get; set; }
    public decimal? 음식점경도 { get; set; }
    public int? 조리예상분 { get; set; }
    public bool 즉시픽업가능여부 { get; set; }
    public string? 수락메모 { get; set; }
}

public sealed class 음식주문응답
{
    public string 주문번호 { get; set; } = string.Empty;
    public long 음식점Id { get; set; }
    public string 음식점명 { get; set; } = string.Empty;
    public string 음식점주소 { get; set; } = string.Empty;
    public string 음식점상세주소 { get; set; } = string.Empty;
    public decimal? 음식점위도 { get; set; }
    public decimal? 음식점경도 { get; set; }
    public string 주문자UserId { get; set; } = string.Empty;
    public 음식주문수령인정보Dto 수령인정보 { get; set; } = new();
    public IReadOnlyList<음식주문상품Dto> 상품목록 { get; set; } = [];
    public decimal 총주문금액 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = 음식주문배차상태코드.미요청;
    public long? 배차대기Id { get; set; }
    public string? 결제수단 { get; set; }
    public DateTime? 음식점수락시각Utc { get; set; }
    public DateTime? 조리예상완료시각Utc { get; set; }
    public DateTime? 배차요청시각Utc { get; set; }
    public string? 수락메모 { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<음식주문상태전이기록Dto> 상태이력 { get; set; } = [];
}

public sealed class 음식주문목록응답
{
    public IReadOnlyList<음식주문응답> Items { get; set; } = [];
}

public sealed class 음식주문상태전이기록Dto
{
    public string 이전상태 { get; set; } = string.Empty;
    public string 다음상태 { get; set; } = string.Empty;
    public string 사유 { get; set; } = string.Empty;
    public DateTime 전이시각Utc { get; set; }
}
