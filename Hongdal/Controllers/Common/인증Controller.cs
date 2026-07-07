using Hongdal.Security;
using Hongdal.Services.Auth;
using Hongdal.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Services.Orderer;
using 홍달.Data;
using 홍달.Services;
using 홍달.Services.Audit;
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
        private readonly I사용자행위로그Service _activityLogService;
        private readonly I가입온보딩인연후보Service _가입온보딩인연후보Service;
        private readonly IOrdererGroupAutoAssignmentService _ordererGroupAutoAssignmentService;

        public 인증Controller(
            UserManager<ApplicationUser> userManager,
            AuthTokenService authTokenService,
            INtsBusinessRegistrationService ntsBusinessRegistrationService,
            IOptions<JwtOptions> jwtOptions,
            I사용자행위로그Service activityLogService,
            I가입온보딩인연후보Service 가입온보딩인연후보Service,
            IOrdererGroupAutoAssignmentService ordererGroupAutoAssignmentService)
        {
            _userManager = userManager;
            _authTokenService = authTokenService;
            _ntsBusinessRegistrationService = ntsBusinessRegistrationService;
            _jwtOptions = jwtOptions.Value;
            _activityLogService = activityLogService;
            _가입온보딩인연후보Service = 가입온보딩인연후보Service;
            _ordererGroupAutoAssignmentService = ordererGroupAutoAssignmentService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> 로그인([FromBody] 로그인요청 request)
        {
            if (request == null) return this.ToProblemActionResult("request body is required");
            if (string.IsNullOrWhiteSpace(request.UserNameOrEmail)) return this.ToProblemActionResult("userNameOrEmail is required");
            if (string.IsNullOrWhiteSpace(request.Password)) return this.ToProblemActionResult("password is required");

            var user = await 사용자조회Async(request.UserNameOrEmail.Trim());
            if (user == null)
            {
                await 로그인로그기록Async(request.UserNameOrEmail.Trim(), null, false, "UserNotFound", "아이디 또는 비밀번호가 올바르지 않습니다.");
                return this.ToAuthenticationProblem("아이디 또는 비밀번호가 올바르지 않습니다.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                await 로그인로그기록Async(request.UserNameOrEmail.Trim(), user, false, "PasswordInvalid", "아이디 또는 비밀번호가 올바르지 않습니다.");
                return this.ToAuthenticationProblem("아이디 또는 비밀번호가 올바르지 않습니다.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _authTokenService.CreateAccessToken(user, roles, out var accessTokenExpiresAtUtc, await _userManager.GetClaimsAsync(user));
            var refreshToken = _authTokenService.GenerateRefreshToken();
            var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

            await 리프레시토큰저장Async(user, refreshToken, refreshTokenExpiresAtUtc);
            await 로그인로그기록Async(request.UserNameOrEmail.Trim(), user, true, string.Empty, string.Empty);

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
            if (request == null) return this.ToProblemActionResult("request body is required");
            if (string.IsNullOrWhiteSpace(request.UserName)) return this.ToProblemActionResult("userName is required");
            if (string.IsNullOrWhiteSpace(request.Email)) return this.ToProblemActionResult("email is required");
            if (string.IsNullOrWhiteSpace(request.Password)) return this.ToProblemActionResult("password is required");
            if (string.IsNullOrWhiteSpace(request.BusinessRegistrationNumber)) return this.ToProblemActionResult("businessRegistrationNumber is required");

            var businessCheck = await _ntsBusinessRegistrationService.CheckStatusAsync(request.BusinessRegistrationNumber.Trim());
            if (!businessCheck.IsValid)
            {
                return this.ToProblemActionResult([
                    "사업자등록번호를 확인할 수 없습니다.",
                    businessCheck.Message,
                    businessCheck.BusinessRegistrationNumber
                ]);
            }

            var existingUser = await _userManager.FindByNameAsync(request.UserName.Trim());
            if (existingUser != null)
            {
                return this.ToConflictProblem("이미 사용 중인 아이디입니다.");
            }

            var existingEmail = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (existingEmail != null)
            {
                return this.ToConflictProblem("이미 사용 중인 이메일입니다.");
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
                return this.ToProblemActionResult(["회원가입에 실패했습니다.", .. createResult.Errors.Select(x => x.Description)]);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, 역할명.기사);
            if (!roleResult.Succeeded)
            {
                return this.ToProblemActionResult(["기사 역할 부여에 실패했습니다.", .. roleResult.Errors.Select(x => x.Description)]);
            }

            return Ok(new { userId = user.Id, userName = user.UserName, businessRegistrationNumber = user.BusinessRegistrationNumber });
        }

        [HttpPost("register/orderer")]
        public async Task<IActionResult> 주문자회원가입([FromBody] 주문자회원가입요청 request)
        {
            if (request == null) return this.ToProblemActionResult("request body is required");
            if (string.IsNullOrWhiteSpace(request.UserName)) return this.ToProblemActionResult("userName is required");
            if (string.IsNullOrWhiteSpace(request.Email)) return this.ToProblemActionResult("email is required");
            if (string.IsNullOrWhiteSpace(request.Password)) return this.ToProblemActionResult("password is required");

            var duplicateProblem = await 중복사용자검증Async(request.UserName, request.Email);
            if (duplicateProblem is not null)
            {
                return duplicateProblem;
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName.Trim(),
                Email = request.Email.Trim(),
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return this.ToProblemActionResult(["회원가입에 실패했습니다.", .. createResult.Errors.Select(x => x.Description)]);
            }

            var assignedScope = _ordererGroupAutoAssignmentService.Resolve(request);
            if (assignedScope is not null)
            {
                var claimResult = await 주문자집단클레임저장Async(user, assignedScope);
                if (!claimResult.Succeeded)
                {
                    return this.ToProblemActionResult(["주문자 집단 온보딩 저장에 실패했습니다.", .. claimResult.Errors.Select(x => x.Description)]);
                }
            }

            return Ok(new 주문자회원가입응답
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                OrdererGroupScope = assignedScope
            });
        }

        [Authorize]
        [HttpPost("onboarding/connection-candidates")]
        public async Task<IActionResult> 가입온보딩인연후보조회([FromBody] 가입인연후보조회요청 request, CancellationToken cancellationToken)
        {
            if (request == null) return this.ToProblemActionResult("request body is required");
            if (string.IsNullOrWhiteSpace(request.주문참조번호)) return this.ToProblemActionResult("orderReference is required");

            var result = await _가입온보딩인연후보Service.후보조회Async(request, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("onboarding/orderer-group-scope")]
        public async Task<IActionResult> 주문자집단온보딩([FromBody] 주문자집단온보딩요청 request)
        {
            if (request == null) return this.ToProblemActionResult("request body is required");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return this.ToAuthenticationProblem("로그인 사용자 정보를 확인할 수 없습니다.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return this.ToAuthenticationProblem("로그인 사용자 정보를 확인할 수 없습니다.");
            }

            var assignedScope = _ordererGroupAutoAssignmentService.Resolve(request);
            if (assignedScope is null)
            {
                return this.ToProblemActionResult("주소 또는 아파트 단지 정보로 주문자 집단을 계산할 수 없습니다.");
            }

            var claimResult = await 주문자집단클레임저장Async(user, assignedScope);
            if (!claimResult.Succeeded)
            {
                return this.ToProblemActionResult(["주문자 집단 온보딩 저장에 실패했습니다.", .. claimResult.Errors.Select(x => x.Description)]);
            }

            return Ok(assignedScope);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> 토큰갱신([FromBody] 토큰갱신요청 request)
        {
            if (request == null) return this.ToProblemActionResult("request body is required");
            if (string.IsNullOrWhiteSpace(request.UserId)) return this.ToProblemActionResult("userId is required");
            if (string.IsNullOrWhiteSpace(request.RefreshToken)) return this.ToProblemActionResult("refreshToken is required");

            var user = await _userManager.FindByIdAsync(request.UserId.Trim());
            if (user == null)
            {
                return this.ToAuthenticationProblem("유효하지 않은 토큰입니다.");
            }

            var storedHash = await _userManager.GetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenHashName);
            var storedExpiresAt = await _userManager.GetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenExpiresAtName);

            if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedExpiresAt))
            {
                return this.ToAuthenticationProblem("유효하지 않은 토큰입니다.");
            }

            if (!DateTime.TryParse(storedExpiresAt, out var refreshTokenExpiresAtUtc) || refreshTokenExpiresAtUtc <= DateTime.UtcNow)
            {
                return this.ToAuthenticationProblem("리프레시 토큰이 만료되었습니다.");
            }

            if (!_authTokenService.VerifyRefreshToken(request.RefreshToken, storedHash))
            {
                return this.ToAuthenticationProblem("유효하지 않은 토큰입니다.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _authTokenService.CreateAccessToken(user, roles, out var accessTokenExpiresAtUtc, await _userManager.GetClaimsAsync(user));
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

        private async Task<IActionResult?> 중복사용자검증Async(string userName, string email)
        {
            var existingUser = await _userManager.FindByNameAsync(userName.Trim());
            if (existingUser != null)
            {
                return this.ToConflictProblem("이미 사용 중인 아이디입니다.");
            }

            var existingEmail = await _userManager.FindByEmailAsync(email.Trim());
            if (existingEmail != null)
            {
                return this.ToConflictProblem("이미 사용 중인 이메일입니다.");
            }

            return null;
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

        private async Task<IdentityResult> 주문자집단클레임저장Async(
            ApplicationUser user,
            주문자집단자동배정응답 scope)
        {
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var managedClaims = existingClaims
                .Where(x => x.Type is OrdererGroupScopeClaimTypes.ScopeKey
                    or OrdererGroupScopeClaimTypes.DisplayName
                    or OrdererGroupScopeClaimTypes.Basis
                    or OrdererGroupScopeClaimTypes.AddressHint
                    or OrdererGroupScopeClaimTypes.ApartmentComplexCode
                    or OrdererGroupScopeClaimTypes.ApartmentComplexName)
                .ToArray();

            if (managedClaims.Length > 0)
            {
                var removeResult = await _userManager.RemoveClaimsAsync(user, managedClaims);
                if (!removeResult.Succeeded)
                {
                    return removeResult;
                }
            }

            var claims = new List<Claim>
            {
                new(OrdererGroupScopeClaimTypes.ScopeKey, scope.ScopeKey),
                new(OrdererGroupScopeClaimTypes.DisplayName, scope.DisplayName),
                new(OrdererGroupScopeClaimTypes.Basis, scope.Basis),
                new(OrdererGroupScopeClaimTypes.AddressHint, scope.AddressHint)
            };

            if (!string.IsNullOrWhiteSpace(scope.ApartmentComplexCode))
            {
                claims.Add(new Claim(OrdererGroupScopeClaimTypes.ApartmentComplexCode, scope.ApartmentComplexCode));
            }

            if (!string.IsNullOrWhiteSpace(scope.ApartmentComplexName))
            {
                claims.Add(new Claim(OrdererGroupScopeClaimTypes.ApartmentComplexName, scope.ApartmentComplexName));
            }

            return await _userManager.AddClaimsAsync(user, claims);
        }

        private async Task 리프레시토큰저장Async(ApplicationUser user, string refreshToken, DateTime refreshTokenExpiresAtUtc)
        {
            var refreshTokenHash = _authTokenService.HashRefreshToken(refreshToken);
            await _userManager.SetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenHashName, refreshTokenHash);
            await _userManager.SetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenExpiresAtName, refreshTokenExpiresAtUtc.ToString("O"));
        }

        private async Task 로그인로그기록Async(string userNameOrEmail, ApplicationUser? user, bool isSuccess, string errorCode, string errorMessage)
        {
            var roles = user is null ? [] : await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? string.Empty;
            await _activityLogService.기록Async(new 사용자행위로그기록
            {
                AppKey = roleName switch
                {
                    역할명.기사 => App식별자.DriverApp,
                    역할명.화주 => App식별자.ShipperApp,
                    역할명.관세사 => App식별자.HongdalAdmin,
                    역할명.서버관리자 => App식별자.HongdalAdmin,
                    _ => "Hongdal.Server"
                },
                UserId = user?.Id ?? string.Empty,
                UserName = user?.UserName ?? userNameOrEmail,
                RoleName = roleName,
                Email = user?.Email ?? userNameOrEmail,
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                ActionType = "Auth",
                ActionName = "Login",
                Route = Request.Path.Value ?? "/api/v1/auth/login",
                TraceId = HttpContext.TraceIdentifier,
                IsSuccess = isSuccess,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = Request.Headers.UserAgent.ToString(),
                OccurredAtUtc = DateTime.UtcNow,
                MetadataJson = $"{{\"userNameOrEmail\":\"{userNameOrEmail}\"}}"
            });
        }
    }

}
