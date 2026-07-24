using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class IdentityDataSeederTests
{
    [Fact]
    public async Task SeedAsync_WithDefaultOptions_CreatesRolesButNoUsers()
    {
        await using var fixture = await IdentitySeedFixture.CreateAsync();

        await IdentityDataSeeder.SeedAsync(
            fixture.Services,
            includeDevelopmentAccounts: false);

        using var assertionScope = fixture.Services.CreateScope();
        var roleManager = assertionScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = assertionScope.ServiceProvider.GetRequiredService<SsalddelContext>();
        Assert.True(await roleManager.RoleExistsAsync(역할명.서버관리자));
        Assert.Empty(await db.Users.AsNoTracking().ToArrayAsync());
    }

    [Fact]
    public async Task SeedAsync_WithExplicitBootstrap_CreatesOneServerAdminIdempotently()
    {
        await using var fixture = await IdentitySeedFixture.CreateAsync(options =>
        {
            options.BootstrapAdmin.Enabled = true;
            options.BootstrapAdmin.UserName = "bootstrap-admin";
            options.BootstrapAdmin.Email = "bootstrap-admin@example.com";
            options.BootstrapAdmin.Password = "StrongBootstrap123!";
        });

        await IdentityDataSeeder.SeedAsync(fixture.Services);
        await IdentityDataSeeder.SeedAsync(fixture.Services);

        using var assertionScope = fixture.Services.CreateScope();
        var userManager = assertionScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = assertionScope.ServiceProvider.GetRequiredService<SsalddelContext>();
        var admin = Assert.IsType<ApplicationUser>(
            await userManager.FindByNameAsync("bootstrap-admin"));
        Assert.True(admin.LockoutEnabled);
        Assert.True(await userManager.IsInRoleAsync(admin, 역할명.서버관리자));
        Assert.Equal(1, await db.Users.CountAsync());
    }

    private sealed class IdentitySeedFixture : IAsyncDisposable
    {
        private readonly ServiceProvider provider;

        private IdentitySeedFixture(ServiceProvider provider)
        {
            this.provider = provider;
            Services = provider;
            Db = provider.GetRequiredService<SsalddelContext>();
            UserManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            RoleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        }

        public IServiceProvider Services { get; }

        public SsalddelContext Db { get; }

        public UserManager<ApplicationUser> UserManager { get; }

        public RoleManager<IdentityRole> RoleManager { get; }

        public static async Task<IdentitySeedFixture> CreateAsync(
            Action<IdentitySeedOptions>? configure = null)
        {
            var services = new ServiceCollection();
            var databaseRoot = new InMemoryDatabaseRoot();
            var databaseName = $"identity-seed-{Guid.NewGuid():N}";
            services.AddLogging();
            services.AddSingleton<IPersonalDataEncryptionService, DummyPersonalDataEncryptionService>();
            services.AddDbContext<SsalddelContext>(options =>
                options.UseInMemoryDatabase(
                    databaseName,
                    databaseRoot));
            services.Configure<IdentitySeedOptions>(options => configure?.Invoke(options));
            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 12;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SsalddelContext>();

            var provider = services.BuildServiceProvider();
            var fixture = new IdentitySeedFixture(provider);
            await fixture.Db.Database.EnsureCreatedAsync();
            return fixture;
        }

        public ValueTask DisposeAsync() => provider.DisposeAsync();
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
