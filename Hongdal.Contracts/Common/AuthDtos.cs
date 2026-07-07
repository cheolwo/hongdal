namespace Hongdal.Contracts.Common;

public sealed class 로그인요청
{
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
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
