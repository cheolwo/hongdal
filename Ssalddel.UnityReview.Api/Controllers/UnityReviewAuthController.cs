using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ssalddel.Contracts.Common;
using Ssalddel.UnityReview.Api.Configuration;

namespace Ssalddel.UnityReview.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class UnityReviewAuthController(
    IOptions<UnityReviewAccessOptions> accessOptions,
    TimeProvider timeProvider) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public ActionResult<토큰응답> Login([FromBody] 로그인요청 request)
    {
        var options = accessOptions.Value;
        var userName = request?.UserNameOrEmail?.Trim() ?? string.Empty;
        var password = request?.Password ?? string.Empty;
        var validUserName = string.Equals(
            userName,
            options.AdminUserName,
            StringComparison.OrdinalIgnoreCase);
        var validPassword = UnityReviewAccessOptions.VerifyPassword(
            password,
            options.AdminPasswordPbkdf2);
        if (!validUserName || !validPassword)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unity 검토 관리자 로그인 실패",
                Detail = "아이디 또는 비밀번호를 확인해 주세요."
            });
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddHours(Math.Clamp(options.TokenLifetimeHours, 1, 24));
        var userId = "unity-review-admin";
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, options.AdminUserName),
            new Claim(ClaimTypes.Role, "서버관리자")
        };
        var signingKey = new SymmetricSecurityKey(
            Convert.FromBase64String(options.JwtSigningKeyBase64));
        var token = new JwtSecurityToken(
            issuer: options.JwtIssuer,
            audience: options.JwtAudience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return Ok(new 토큰응답
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            AccessTokenExpiresAtUtc = expiresAt,
            RefreshToken = string.Empty,
            RefreshTokenExpiresAtUtc = expiresAt,
            UserId = userId,
            UserName = options.AdminUserName,
            Roles = ["서버관리자"],
            PreferredLanguageCode = "ko"
        });
    }
}
