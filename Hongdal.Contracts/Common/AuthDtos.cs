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

public sealed class 기사회원가입요청
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BusinessRegistrationNumber { get; set; } = string.Empty;
}