namespace Ssalddel.Contracts.Common.Sales;

public enum 판매채널인증전달위치
{
    Header,
    Query,
    Body,
    Signature,
    Endpoint
}

public sealed record 판매채널인증필드정의(
    string Key,
    string 표시명,
    bool 필수,
    bool 비밀값,
    판매채널인증전달위치 전달위치,
    string 도움말,
    string? Placeholder = null);

public sealed record 판매채널인증Schema(
    string 채널종류,
    string 표시명,
    string 시장,
    IReadOnlyList<판매채널인증필드정의> Fields);

/// <summary>
/// 판매자 앱과 서버 모듈이 같은 입력 키를 사용하도록 하는 최소 자격증명 계약입니다.
/// 실제 외부 요청 body는 각 채널 adapter가 이 값을 읽어 API별 DTO로 조립합니다.
/// </summary>
public static class 판매채널인증SchemaCatalog
{
    public static IReadOnlyList<판매채널인증Schema> Items { get; } =
    [
        new(
            CommerceChannelKeys.SmartStore,
            "네이버 스마트스토어",
            "국내",
            [
                new("clientId", "애플리케이션 ID", true, false, 판매채널인증전달위치.Signature, "커머스API센터에서 발급한 애플리케이션 ID입니다."),
                new("clientSecret", "애플리케이션 시크릿", true, true, 판매채널인증전달위치.Signature, "서버에서 전자서명을 만들 때만 사용하며 앱으로 다시 보내지 않습니다.")
            ]),
        new(
            CommerceChannelKeys.Coupang,
            "쿠팡 Wing",
            "국내",
            [
                new("accessKey", "Access Key", true, false, 판매채널인증전달위치.Signature, "Wing에서 발급한 Open API Access Key입니다."),
                new("secretKey", "Secret Key", true, true, 판매채널인증전달위치.Signature, "HMAC 서명에만 사용하며 앱으로 다시 보내지 않습니다."),
                new("vendorId", "업체코드(Vendor ID)", true, false, 판매채널인증전달위치.Endpoint, "주문·상품 API 요청 경로에 필요한 판매자 업체코드입니다.")
            ]),
        new(
            CommerceChannelKeys.Shopify,
            "Shopify",
            "해외",
            [
                new("shopDomain", "상점 도메인", true, false, 판매채널인증전달위치.Endpoint, "예: my-shop.myshopify.com", "my-shop.myshopify.com"),
                new("adminAccessToken", "Admin API Access Token", true, true, 판매채널인증전달위치.Header, "서버 요청의 X-Shopify-Access-Token 헤더에만 사용합니다.")
            ]),
        new(
            CommerceChannelKeys.Amazon,
            "Amazon",
            "해외",
            [
                new("lwaClientId", "LWA Client ID", true, false, 판매채널인증전달위치.Body, "Login with Amazon 액세스 토큰 발급에 사용합니다."),
                new("lwaClientSecret", "LWA Client Secret", true, true, 판매채널인증전달위치.Body, "서버의 LWA 토큰 요청에만 사용합니다."),
                new("refreshToken", "LWA Refresh Token", true, true, 판매채널인증전달위치.Body, "판매자 승인 뒤 발급된 갱신 토큰입니다."),
                new("marketplaceId", "Marketplace ID", true, false, 판매채널인증전달위치.Query, "주문·상품을 조회할 대상 마켓플레이스입니다.")
            ])
    ];

    public static 판매채널인증Schema? 찾기(string? 채널종류)
        => Items.FirstOrDefault(item =>
            string.Equals(item.채널종류, 채널종류?.Trim(), StringComparison.OrdinalIgnoreCase));
}
