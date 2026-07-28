using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Admin.Restaurants;
using Ssalddel.Contracts.Admin.Restaurants;
using Ssalddel.Contracts.Food;
using Ssalddel.Controllers.Admin;
using Ssalddel.Filters;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Versioning;
using 살뜰.도메인.음식;

namespace Ssalddel.Tests.Application.Admin.Restaurants;

public sealed class 음식점운영자접근관리UseCaseTests
{
    [Fact]
    public async Task 관리자는_실제음식점에사용자역할과범위를배정하고해제할수있다()
    {
        await using var fixture = await Fixture.CreateAsync();
        var request = new 음식점운영자접근배정요청
        {
            UserId = fixture.User.Id,
            음식점Id = fixture.Restaurant.Id
        };

        var assigned = await fixture.UseCase.배정Async(request, CancellationToken.None);
        var queried = await fixture.UseCase.조회Async(fixture.User.Id, CancellationToken.None);

        Assert.True(assigned.IsSuccess);
        Assert.True(assigned.Value.접근가능);
        Assert.Equal(fixture.Restaurant.Id, assigned.Value.음식점Id);
        Assert.True(queried.Value.음식점역할보유);
        Assert.Contains(
            await fixture.UserManager.GetClaimsAsync(fixture.User),
            claim => claim.Type == 음식점접근ClaimTypes.음식점Id
                     && claim.Value == fixture.Restaurant.Id.ToString());

        var revoked = await fixture.UseCase.해제Async(request, CancellationToken.None);

        Assert.True(revoked.IsSuccess);
        Assert.False(revoked.Value.접근가능);
        Assert.False(await fixture.UserManager.IsInRoleAsync(fixture.User, 역할명.음식점));
    }

    [Fact]
    public async Task 존재하지않는음식점에는접근범위를배정하지않는다()
    {
        await using var fixture = await Fixture.CreateAsync();

        var result = await fixture.UseCase.배정Async(
            new 음식점운영자접근배정요청
            {
                UserId = fixture.User.Id,
                음식점Id = 99999
            },
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(404, result.Errors.Single().Metadata["StatusCode"]);
        Assert.False(await fixture.UserManager.IsInRoleAsync(fixture.User, 역할명.음식점));
    }

    [Fact]
    public void 관리Api는_서버관리자정책과고정경로를사용한다()
    {
        var type = typeof(음식점운영자접근Controller);

        Assert.Equal(
            "서버관리자전용",
            type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(
            "api/v1/admin/restaurants/operator-access",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.FoodDeliveryWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(type.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V3_0, version.Version);
        Assert.Equal(VersionFeatureFlagKeys.FoodDeliveryWorkflow, version.FeatureKey);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly ServiceProvider provider;

        private Fixture(
            ServiceProvider provider,
            ApplicationUser user,
            음식점공개프로필 restaurant)
        {
            this.provider = provider;
            User = user;
            Restaurant = restaurant;
            UserManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            UseCase = new 음식점운영자접근관리UseCase(
                provider.GetRequiredService<SsalddelContext>(),
                UserManager);
        }

        public ApplicationUser User { get; }
        public 음식점공개프로필 Restaurant { get; }
        public UserManager<ApplicationUser> UserManager { get; }
        public 음식점운영자접근관리UseCase UseCase { get; }

        public static async Task<Fixture> CreateAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IPersonalDataEncryptionService, PassThroughEncryption>();
            services.AddDbContext<SsalddelContext>(options =>
                options.UseInMemoryDatabase($"restaurant-access-{Guid.NewGuid():N}"));
            services
                .AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SsalddelContext>();

            var provider = services.BuildServiceProvider();
            var db = provider.GetRequiredService<SsalddelContext>();
            await db.Database.EnsureCreatedAsync();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            await roleManager.CreateAsync(new IdentityRole(역할명.음식점));
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = "restaurant-operator",
                Email = "restaurant-operator@example.com"
            };
            var created = await userManager.CreateAsync(user);
            Assert.True(created.Succeeded);
            var restaurant = new 음식점공개프로필
            {
                상호명 = "검증 식당",
                카테고리 = "한식",
                공개주소 = "서울시",
                공개여부 = true,
                주문가능여부 = true
            };
            db.음식점공개프로필.Add(restaurant);
            await db.SaveChangesAsync();
            return new Fixture(provider, user, restaurant);
        }

        public ValueTask DisposeAsync() => provider.DisposeAsync();
    }

    private sealed class PassThroughEncryption : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
