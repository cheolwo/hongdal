using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common;
using Ssalddel.Security;
using Ssalddel.Services.Auth;
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

    private static 인증UseCase CreateUseCase(UserManager<ApplicationUser> userManager)
        => new(
            userManager,
            null!,
            null!,
            Options.Create(new JwtOptions()),
            null!,
            null!,
            null!);

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
                .AddIdentityCore<ApplicationUser>()
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
}
