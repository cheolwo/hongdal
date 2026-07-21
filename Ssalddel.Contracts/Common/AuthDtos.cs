namespace Ssalddel.Contracts.Common;

public sealed class 로그인요청
{
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public static class 소셜로그인ProviderIds
{
    public const string Kakao = "kakao";
    public const string Google = "google";
    public const string Naver = "naver";

    public static readonly string[] 지원Provider목록 = [Kakao, Google, Naver];

    public static bool 지원여부(string? providerId)
        => !string.IsNullOrWhiteSpace(providerId)
            && 지원Provider목록.Contains(providerId.Trim(), StringComparer.OrdinalIgnoreCase);
}

public sealed class 소셜로그인시작요청
{
    public string ProviderId { get; set; } = string.Empty;
    public string? AppKey { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed class 소셜로그인시작응답
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class 소셜로그인완료요청
{
    public string ProviderId { get; set; } = string.Empty;
    public string AuthorizationCode { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? AppKey { get; set; }
}

public sealed class 토큰갱신요청
{
    public string UserId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class 토큰응답
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string? PreferredLanguageCode { get; set; }
}

public sealed class 표시언어설정요청
{
    public string LanguageCode { get; set; } = string.Empty;
}

public sealed class 표시언어설정응답
{
    public string LanguageCode { get; set; } = string.Empty;
}

public static class 커뮤니티회원가입개인정보동의문
{
    public const string 현재버전 = "community-signup-privacy-2026-07-20";
    public const string 수집이용목적 = "회원 계정 생성, 로그인, 계정 보안과 필수 서비스 안내";
    public const string 수집항목 = "아이디, 이메일, 비밀번호(원문을 저장하지 않고 단방향 해시로 처리)";
    public const string 보유이용기간 = "회원 탈퇴 시까지. 단, 관계 법령에 따른 보존 의무가 있으면 해당 기간 동안 보관";
    public const string 동의거부안내 = "동의를 거부할 수 있으며, 거부하면 회원 계정은 만들 수 없습니다. 자유·생활 등 익명 쓰기를 허용한 게시판은 회원가입 없이 이용할 수 있습니다.";

    public static bool 유효한동의(bool accepted, string? version)
        => accepted
            && string.Equals(version?.Trim(), 현재버전, StringComparison.Ordinal);
}

public sealed class 커뮤니티회원가입요청
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool PrivacyConsentAccepted { get; set; }
    public string PrivacyConsentVersion { get; set; } = string.Empty;
}

public sealed class 커뮤니티회원가입응답
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrivacyConsentVersion { get; set; } = string.Empty;
    public DateTime PrivacyConsentedAtUtc { get; set; }
}

public sealed class 주문자회원가입요청
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoadAddress { get; set; } = string.Empty;
    public string? JibunAddress { get; set; }
    public string? DetailAddress { get; set; }
    public string? KakaoRegionLevel1 { get; set; }
    public string? KakaoRegionLevel2 { get; set; }
    public string? KakaoRegionLevel3 { get; set; }
    public string? ApartmentComplexCode { get; set; }
    public string? ApartmentComplexName { get; set; }
}

public sealed class 주문자집단온보딩요청
{
    public string RoadAddress { get; set; } = string.Empty;
    public string? JibunAddress { get; set; }
    public string? DetailAddress { get; set; }
    public string? KakaoRegionLevel1 { get; set; }
    public string? KakaoRegionLevel2 { get; set; }
    public string? KakaoRegionLevel3 { get; set; }
    public string? ApartmentComplexCode { get; set; }
    public string? ApartmentComplexName { get; set; }
}

public sealed class 주문자회원가입응답
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public 주문자집단자동배정응답? OrdererGroupScope { get; set; }
}

public sealed class 주문자집단자동배정응답
{
    public string ScopeKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Basis { get; set; } = string.Empty;
    public string AddressHint { get; set; } = string.Empty;
    public string? ApartmentComplexCode { get; set; }
    public string? ApartmentComplexName { get; set; }
    public bool IsApartmentScope { get; set; }
    public string PrivacyNote { get; set; } = string.Empty;
}

public sealed class 기사회원가입요청
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BusinessRegistrationNumber { get; set; } = string.Empty;
}

public sealed class 기사회원가입응답
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string BusinessRegistrationNumber { get; set; } = string.Empty;
}
