using Hongdal.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using 홍달.Data;
using 홍달.Services;
using Hongdal.Contracts.Common;

namespace Hongdal.Controllers.Common
{
    [ApiController]
    [Route("api/v1/auth")]
    public class 인증Controller : ControllerBase
    {
        private const string TokenProvider = "HongdalAuth";
        private const string RefreshTokenHashName = "RefreshTokenHash";
        private const string RefreshTokenExpiresAtName = "RefreshTokenExpiresAt";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthTokenService _authTokenService;
        private readonly INtsBusinessRegistrationService _ntsBusinessRegistrationService;
        private readonly JwtOptions _jwtOptions;

        public 인증Controller(
            UserManager<ApplicationUser> userManager,
            AuthTokenService authTokenService,
            INtsBusinessRegistrationService ntsBusinessRegistrationService,
            IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _authTokenService = authTokenService;
            _ntsBusinessRegistrationService = ntsBusinessRegistrationService;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("login")]
        public async Task<IActionResult> 로그인([FromBody] 로그인요청 request)
        {
            if (request == null) return BadRequest("request body is required");
            if (string.IsNullOrWhiteSpace(request.UserNameOrEmail)) return BadRequest("userNameOrEmail is required");
            if (string.IsNullOrWhiteSpace(request.Password)) return BadRequest("password is required");

            var user = await 사용자조회Async(request.UserNameOrEmail.Trim());
            if (user == null)
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호가 올바르지 않습니다." });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호가 올바르지 않습니다." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _authTokenService.CreateAccessToken(user, roles, out var accessTokenExpiresAtUtc);
            var refreshToken = _authTokenService.GenerateRefreshToken();
            var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

            await 리프레시토큰저장Async(user, refreshToken, refreshTokenExpiresAtUtc);

            return Ok(new 토큰응답
            {
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Roles = roles.ToArray()
            });
        }

        [HttpPost("register/driver")]
        public async Task<IActionResult> 기사회원가입([FromBody] 기사회원가입요청 request)
        {
            if (request == null) return BadRequest("request body is required");
            if (string.IsNullOrWhiteSpace(request.UserName)) return BadRequest("userName is required");
            if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest("email is required");
            if (string.IsNullOrWhiteSpace(request.Password)) return BadRequest("password is required");
            if (string.IsNullOrWhiteSpace(request.BusinessRegistrationNumber)) return BadRequest("businessRegistrationNumber is required");

            var businessCheck = await _ntsBusinessRegistrationService.CheckStatusAsync(request.BusinessRegistrationNumber.Trim());
            if (!businessCheck.IsValid)
            {
                return BadRequest(new
                {
                    message = "사업자등록번호를 확인할 수 없습니다.",
                    businessCheck.Message,
                    businessCheck.BusinessRegistrationNumber
                });
            }

            var existingUser = await _userManager.FindByNameAsync(request.UserName.Trim());
            if (existingUser != null)
            {
                return Conflict(new { message = "이미 사용 중인 아이디입니다." });
            }

            var existingEmail = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (existingEmail != null)
            {
                return Conflict(new { message = "이미 사용 중인 이메일입니다." });
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName.Trim(),
                Email = request.Email.Trim(),
                EmailConfirmed = true,
                BusinessRegistrationNumber = businessCheck.BusinessRegistrationNumber
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "회원가입에 실패했습니다.",
                    errors = createResult.Errors.Select(x => x.Description)
                });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, 역할명.기사);
            if (!roleResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "기사 역할 부여에 실패했습니다.",
                    errors = roleResult.Errors.Select(x => x.Description)
                });
            }

            return Ok(new { userId = user.Id, userName = user.UserName, businessRegistrationNumber = user.BusinessRegistrationNumber });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> 토큰갱신([FromBody] 토큰갱신요청 request)
        {
            if (request == null) return BadRequest("request body is required");
            if (string.IsNullOrWhiteSpace(request.UserId)) return BadRequest("userId is required");
            if (string.IsNullOrWhiteSpace(request.RefreshToken)) return BadRequest("refreshToken is required");

            var user = await _userManager.FindByIdAsync(request.UserId.Trim());
            if (user == null)
            {
                return Unauthorized(new { message = "유효하지 않은 토큰입니다." });
            }

            var storedHash = await _userManager.GetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenHashName);
            var storedExpiresAt = await _userManager.GetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenExpiresAtName);

            if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedExpiresAt))
            {
                return Unauthorized(new { message = "유효하지 않은 토큰입니다." });
            }

            if (!DateTime.TryParse(storedExpiresAt, out var refreshTokenExpiresAtUtc) || refreshTokenExpiresAtUtc <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "리프레시 토큰이 만료되었습니다." });
            }

            if (!_authTokenService.VerifyRefreshToken(request.RefreshToken, storedHash))
            {
                return Unauthorized(new { message = "유효하지 않은 토큰입니다." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _authTokenService.CreateAccessToken(user, roles, out var accessTokenExpiresAtUtc);
            var newRefreshToken = _authTokenService.GenerateRefreshToken();
            var newRefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

            await 리프레시토큰저장Async(user, newRefreshToken, newRefreshTokenExpiresAtUtc);

            return Ok(new 토큰응답
            {
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAtUtc = newRefreshTokenExpiresAtUtc,
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Roles = roles.ToArray()
            });
        }

        private async Task<ApplicationUser?> 사용자조회Async(string userNameOrEmail)
        {
            var user = await _userManager.FindByNameAsync(userNameOrEmail);
            if (user != null)
            {
                return user;
            }

            return await _userManager.FindByEmailAsync(userNameOrEmail);
        }

        private async Task 리프레시토큰저장Async(ApplicationUser user, string refreshToken, DateTime refreshTokenExpiresAtUtc)
        {
            var refreshTokenHash = _authTokenService.HashRefreshToken(refreshToken);
            await _userManager.SetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenHashName, refreshTokenHash);
            await _userManager.SetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenExpiresAtName, refreshTokenExpiresAtUtc.ToString("O"));
        }
    }

}
