using Ssalddel.Contracts.Common.Payments;

namespace Ssalddel.Contracts.Common.Community;

public sealed class 노드스티커등록Request
{
    public string 창작자UserId { get; set; } = string.Empty;
    public string 창작자표시명 { get; set; } = string.Empty;
    public string 팩Key { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 요약 { get; set; } = string.Empty;
    public IReadOnlyList<string> 원장템플릿Keys { get; set; } = [];
    public IReadOnlyList<string> 스타일Tags { get; set; } = [];
    public 노드스티커거래정책Response 거래정책 { get; set; } = 노드스티커거래정책Response.무료샘플();
    public IReadOnlyList<노드스티커이미지등록Request> 이미지목록 { get; set; } = [];
}

public sealed class 노드스티커이미지등록Request
{
    public string 이미지Key { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 이미지Url { get; set; } = string.Empty;
    public string 대체Text { get; set; } = string.Empty;
    public string MimeType { get; set; } = "image/png";
    public int 원본너비Px { get; set; }
    public int 원본높이Px { get; set; }
    public IReadOnlyList<string> 노드종류목록 { get; set; } = [];
    public IReadOnlyList<string> 노드제목목록 { get; set; } = [];
    public IReadOnlyList<string> 상태라벨목록 { get; set; } = [];
    public IReadOnlyList<string> 역할라벨목록 { get; set; } = [];
}

public sealed class 노드스티커검수Request
{
    public string 관리자UserId { get; set; } = string.Empty;
    public string 관리자표시명 { get; set; } = string.Empty;
    public string 검수상태 { get; set; } = 노드스티커검수상태.승인;
    public string 검수메모 { get; set; } = string.Empty;
}

public sealed class 노드스티커상점상품Response
{
    public string 상품Key { get; set; } = string.Empty;
    public string 팩Key { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 창작자표시명 { get; set; } = string.Empty;
    public string 요약 { get; set; } = string.Empty;
    public string 검수상태 { get; set; } = 노드스티커검수상태.초안;
    public string 판매상태 { get; set; } = 노드스티커판매상태.비공개;
    public 노드스티커거래정책Response 거래정책 { get; set; } = 노드스티커거래정책Response.무료샘플();
    public IReadOnlyList<노드스티커이미지Response> 이미지목록 { get; set; } = [];
}

public sealed class 노드스티커구매Request
{
    public string 구매자UserId { get; set; } = string.Empty;
    public string 상품Key { get; set; } = string.Empty;
    public string 팩Key { get; set; } = string.Empty;
    public decimal 결제금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
}

public sealed class 노드스티커구매Response
{
    public string 구매Id { get; set; } = string.Empty;
    public string 구매자UserId { get; set; } = string.Empty;
    public string 상품Key { get; set; } = string.Empty;
    public string 팩Key { get; set; } = string.Empty;
    public string 구매상태 { get; set; } = 노드스티커구매상태.대기;
    public decimal 결제금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
}

public sealed class 노드스티커보유권Response
{
    public string 보유권Id { get; set; } = string.Empty;
    public string 사용자UserId { get; set; } = string.Empty;
    public string 팩Key { get; set; } = string.Empty;
    public IReadOnlyList<string> 이미지Keys { get; set; } = [];
    public string 보유권출처 { get; set; } = 노드스티커보유권출처.구매;
}

public sealed class 노드스티커보유권동기화Response
{
    public string 사용자UserId { get; set; } = string.Empty;
    public DateTime 서버기준시각Utc { get; set; }
    public IReadOnlyList<노드스티커보유권Response> 보유권목록 { get; set; } = [];
}

public sealed class 노드스티커FakePg결제승인Request
{
    public string 상품Key { get; set; } = string.Empty;
    public string 팩Key { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string 결제수단 { get; set; } = "FakePG";
    public string? IdempotencyKey { get; set; }
    public string? 메모 { get; set; }
}

public sealed class 노드스티커FakePg결제승인Response
{
    public string 결제Id { get; set; } = string.Empty;
    public int 결제대상유형 { get; set; } = 계약결제대상유형.노드스티커팩;
    public int 결제제공자 { get; set; } = 계약결제제공자.FakePG;
    public string OrderId { get; set; } = string.Empty;
    public string PaymentKey { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string 통화Code { get; set; } = "KRW";
    public string 결제상태 { get; set; } = string.Empty;
    public string 결제응답 { get; set; } = string.Empty;
    public DateTime 승인일시Utc { get; set; }
    public bool 이미완료됨 { get; set; }
    public 노드스티커상점상품Response 상품 { get; set; } = new();
    public 노드스티커구매Response 구매 { get; set; } = new();
    public 노드스티커보유권Response 보유권 { get; set; } = new();
}

public sealed class 노드스티커노드적용Request
{
    public string 사용자UserId { get; set; } = string.Empty;
    public string 원장Id { get; set; } = string.Empty;
    public string 노드Id { get; set; } = string.Empty;
    public string 이미지Key { get; set; } = string.Empty;
}

public sealed class 노드스티커노드적용판정Response
{
    public bool 적용가능 { get; set; }
    public string 판정Code { get; set; } = 노드스티커노드적용판정Codes.적용가능;
    public string 안내문구 { get; set; } = string.Empty;
}

public static class 노드스티커판매상태
{
    public const string 비공개 = "Hidden";
    public const string 판매중 = "Published";
    public const string 판매중지 = "Paused";
    public const string 운영정지 = "Suspended";
}

public static class 노드스티커구매상태
{
    public const string 대기 = "Pending";
    public const string 완료 = "Completed";
    public const string 환불 = "Refunded";
    public const string 취소 = "Cancelled";
}

public static class 노드스티커보유권출처
{
    public const string 구매 = "Purchase";
    public const string 무료팩 = "FreePack";
    public const string 창작자본인 = "CreatorOwn";
    public const string 관리자지급 = "AdminGrant";
}

public static class 노드스티커노드적용판정Codes
{
    public const string 적용가능 = "Applicable";
    public const string 이미지없음 = "ImageNotFound";
    public const string 검수미승인 = "ReviewNotApproved";
    public const string 구매필요 = "PurchaseRequired";
}
