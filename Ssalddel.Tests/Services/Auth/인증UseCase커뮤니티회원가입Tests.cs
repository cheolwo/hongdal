using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common;
using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Security;
using Ssalddel.Services.Auth;
using 살뜰.Services.Audit;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Auth;

public sealed class 인증UseCase커뮤니티회원가입Tests
{
    [Fact]
    public async Task 현재동의가_없으면_계정을_만들지_않는다()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        var useCase = CreateUseCase(fixture.UserManager);

        var result = await useCase.커뮤니티회원가입Async(new 커뮤니티회원가입요청
        {
            UserName = "anonymous-choice",
            Email = "anonymous-choice@example.com",
            Password = "Valid123!",
            PrivacyConsentAccepted = false,
            PrivacyConsentVersion = 커뮤니티회원가입개인정보동의문.현재버전
        });

        Assert.True(result.IsFailed);
        Assert.Null(await fixture.UserManager.FindByNameAsync("anonymous-choice"));
    }

    [Fact]
    public async Task 현재동의로_가입하면_버전과시각과_커뮤니티역할을_저장한다()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        var useCase = CreateUseCase(fixture.UserManager);

        var beforeUtc = DateTime.UtcNow;
        var result = await useCase.커뮤니티회원가입Async(new 커뮤니티회원가입요청
        {
            UserName = "neighbor-member",
            Email = "neighbor-member@example.com",
            Password = "Valid123!",
            PrivacyConsentAccepted = true,
            PrivacyConsentVersion = 커뮤니티회원가입개인정보동의문.현재버전
        });

        Assert.True(result.IsSuccess);
        var user = Assert.IsType<ApplicationUser>(await fixture.UserManager.FindByNameAsync("neighbor-member"));
        Assert.False(user.EmailConfirmed);
        Assert.Equal(커뮤니티회원가입개인정보동의문.현재버전, user.PrivacyConsentVersion);
        Assert.NotNull(user.PrivacyConsentedAtUtc);
        Assert.InRange(user.PrivacyConsentedAtUtc.Value, beforeUtc, DateTime.UtcNow);
        Assert.Contains(역할명.커뮤니티회원, await fixture.UserManager.GetRolesAsync(user));
        Assert.Equal(user.PrivacyConsentedAtUtc, result.Value.PrivacyConsentedAtUtc);
    }

    [Fact]
    public async Task 로그인사용자의_표시언어를_계정Claim으로_저장하고_교체한다()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        var user = new ApplicationUser
        {
            UserName = "language-member",
            Email = "language-member@example.com"
        };
        Assert.True((await fixture.UserManager.CreateAsync(user)).Succeeded);
        var useCase = CreateUseCase(fixture.UserManager);

        var first = await useCase.표시언어설정Async(
            new 표시언어설정요청 { LanguageCode = "en" },
            user.Id);
        var second = await useCase.표시언어설정Async(
            new 표시언어설정요청 { LanguageCode = "ko" },
            user.Id);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(DisplayLanguageCodes.Korean, second.Value.LanguageCode);
        var languageClaims = (await fixture.UserManager.GetClaimsAsync(user))
            .Where(claim => claim.Type == SsalddelDisplayLanguageClaimTypes.PreferredLanguage)
            .ToArray();
        var languageClaim = Assert.Single(languageClaims);
        Assert.Equal(DisplayLanguageCodes.Korean, languageClaim.Value);
    }

    [Fact]
    public async Task 잘못된비밀번호가_반복되면_계정을_잠근다()
    {
        await using var fixture = await IdentityFixture.CreateAsync();
        var user = new ApplicationUser
        {
            UserName = "lockout-member",
            Email = "lockout-member@example.com",
            LockoutEnabled = true
        };
        Assert.True((await fixture.UserManager.CreateAsync(user, "Valid123!")).Succeeded);
        var audit = new RecordingActivityLogService();
        var useCase = CreateLoginUseCase(fixture.UserManager, audit);
        var context = new 인증요청Context(
            "/api/v1/auth/login",
            "trace-lockout",
            "203.0.113.10",
            "test-agent");

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var result = await useCase.로그인Async(new 로그인요청
            {
                UserNameOrEmail = user.UserName!,
                Password = "Wrong123!"
            }, context);
            Assert.True(result.IsFailed);
        }

        Assert.True(await fixture.UserManager.IsLockedOutAsync(user));
        Assert.Contains(audit.Entries, entry =>
            entry.ErrorCode == "LockedOut" && !entry.IsSuccess);

        var lockedResult = await useCase.로그인Async(new 로그인요청
        {
            UserNameOrEmail = user.UserName!,
            Password = "Valid123!"
        }, context);
        Assert.True(lockedResult.IsFailed);
    }

    private static 인증UseCase CreateUseCase(UserManager<ApplicationUser> userManager)
        => new(
            userManager,
            null!,
            null!,
            Options.Create(new JwtOptions()),
            null!,
            null!,
            null!);

    private static 인증UseCase CreateLoginUseCase(
        UserManager<ApplicationUser> userManager,
        I사용자행위로그Service activityLogService)
    {
        var jwtOptions = new JwtOptions
        {
            Issuer = "Ssalddel.Tests",
            Audience = "Ssalddel.Tests.Client",
            SecretKey = "test-only-signing-key-that-is-longer-than-thirty-two-bytes",
            AccessTokenMinutes = 5,
            RefreshTokenDays = 1
        };
        return new 인증UseCase(
            userManager,
            new AuthTokenService(Options.Create(jwtOptions)),
            null!,
            Options.Create(jwtOptions),
            activityLogService,
            null!,
            null!);
    }

    private sealed class IdentityFixture : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;

        private IdentityFixture(ServiceProvider provider, UserManager<ApplicationUser> userManager)
        {
            _provider = provider;
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; }

        public static async Task<IdentityFixture> CreateAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IPersonalDataEncryptionService, DummyPersonalDataEncryptionService>();
            services.AddDbContext<SsalddelContext>(options =>
                options.UseInMemoryDatabase($"community-signup-{Guid.NewGuid():N}"));
            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SsalddelContext>();

            var provider = services.BuildServiceProvider();
            var db = provider.GetRequiredService<SsalddelContext>();
            await db.Database.EnsureCreatedAsync();

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var roleResult = await roleManager.CreateAsync(new IdentityRole(역할명.커뮤니티회원));
            Assert.True(roleResult.Succeeded);

            return new IdentityFixture(
                provider,
                provider.GetRequiredService<UserManager<ApplicationUser>>());
        }

        public ValueTask DisposeAsync() => _provider.DisposeAsync();
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }

    private sealed class RecordingActivityLogService : I사용자행위로그Service
    {
        public List<사용자행위로그기록> Entries { get; } = [];

        public Task 기록Async(
            사용자행위로그기록 entry,
            CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }
    }
}
