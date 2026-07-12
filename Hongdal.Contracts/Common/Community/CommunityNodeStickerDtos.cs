namespace Hongdal.Contracts.Common.Community;

public sealed class 노드스티커표준Response
{
    public int 원본캔버스크기Px { get; set; } = 512;
    public IReadOnlyList<int> 표시크기Px옵션 { get; set; } = [48, 64, 96];
    public IReadOnlyList<string> 허용MimeTypes { get; set; } = ["image/png", "image/webp", "image/svg+xml"];
    public string 배경정책 { get; set; } = "투명 배경을 기본으로 합니다.";
    public string 안전영역정책 { get; set; } = "중앙 80% 안에 핵심 도형을 배치하고, 바깥 10%는 잘림 방지 여백으로 둡니다.";
    public string 텍스트정책 { get; set; } = "노드 안에서 축소되어도 읽히도록 긴 문구는 이미지 안에 넣지 않습니다.";
    public string 권리정책 { get; set; } = "업로드자는 직접 제작했거나 저작권 또는 사용권을 가진 이미지만 등록할 수 있습니다.";
    public IReadOnlyList<string> 필수MetadataKeys { get; set; } =
    [
        "원장",
        "노드",
        "상태",
        "역할",
        "스타일",
        "라이선스"
    ];

    public static 노드스티커표준Response 기본()
        => new();
}

public sealed class 노드스티커팩Response
{
    public string 팩Key { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 창작자표시명 { get; set; } = string.Empty;
    public string 요약 { get; set; } = string.Empty;
    public string 검수상태 { get; set; } = 노드스티커검수상태.승인;
    public IReadOnlyList<string> 원장템플릿Keys { get; set; } = [];
    public IReadOnlyList<string> 스타일Tags { get; set; } = [];
    public 노드스티커거래정책Response 거래정책 { get; set; } = 노드스티커거래정책Response.무료샘플();
    public IReadOnlyList<노드스티커이미지Response> 이미지목록 { get; set; } = [];
}

public sealed class 노드스티커이미지Response
{
    public string 이미지Key { get; set; } = string.Empty;
    public string 팩Key { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 이미지Url { get; set; } = string.Empty;
    public string 대체Text { get; set; } = string.Empty;
    public string MimeType { get; set; } = "image/svg+xml";
    public int 원본너비Px { get; set; } = 512;
    public int 원본높이Px { get; set; } = 512;
    public IReadOnlyList<string> 원장템플릿Keys { get; set; } = [];
    public IReadOnlyList<string> 노드종류목록 { get; set; } = [];
    public IReadOnlyList<string> 노드제목목록 { get; set; } = [];
    public IReadOnlyList<string> 상태라벨목록 { get; set; } = [];
    public IReadOnlyList<string> 역할라벨목록 { get; set; } = [];
    public IReadOnlyList<string> 스타일Tags { get; set; } = [];
    public string 라이선스Code { get; set; } = 노드스티커라이선스Code.플랫폼노드사용;
    public string 검수상태 { get; set; } = 노드스티커검수상태.승인;
}

public sealed class 노드스티커거래정책Response
{
    public string 가격모드 { get; set; } = 노드스티커가격모드.무료;
    public decimal 가격금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
    public string 플랫폼역할 { get; set; } = "플랫폼은 창작자와 사용자 사이의 등록, 검수, 사용권 확인을 이어주는 중간 다리 역할을 합니다.";
    public string 창작자정산정책 { get; set; } = "유료 팩은 결제 수수료와 환불 보류 기간을 제외한 뒤 창작자에게 정산합니다.";

    public static 노드스티커거래정책Response 무료샘플()
        => new()
        {
            가격모드 = 노드스티커가격모드.무료,
            창작자정산정책 = "기본 샘플 팩은 무료로 제공하며 정산 대상이 아닙니다."
        };
}

public sealed class 노드스티커매칭Request
{
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 노드종류 { get; set; } = string.Empty;
    public string 노드제목 { get; set; } = string.Empty;
    public string 상태라벨 { get; set; } = string.Empty;
    public string 역할라벨 { get; set; } = string.Empty;
}

public static class 노드스티커가격모드
{
    public const string 무료 = "Free";
    public const string 유료 = "Paid";
    public const string 후원 = "Donation";
}

public static class 노드스티커검수상태
{
    public const string 초안 = "Draft";
    public const string 검수대기 = "PendingReview";
    public const string 승인 = "Approved";
    public const string 반려 = "Rejected";
    public const string 정지 = "Suspended";
}

public static class 노드스티커라이선스Code
{
    public const string 플랫폼노드사용 = "PlatformNodeUse";
    public const string 커뮤니티미리보기전용 = "CommunityPreviewOnly";
}
