using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Sales;
using Ssalddel.Contracts.Common.Sales;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Sales;
using 살뜰.도메인.판매;

namespace Ssalddel.Tests.Services.Sales;

public sealed class SalesChannelAccountReadTests
{
    [Fact]
    public async Task 판매자는_자기계정목록과정확한Id만조회하고_다른사용자Id는404로숨긴다()
    {
        await using var db = CreateContext();
        var createdAt = new DateTime(2026, 7, 19, 1, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 7, 20, 2, 0, 0, DateTimeKind.Utc);
        db.판매채널계정.AddRange(
            new 판매채널계정
            {
                Id = 11,
                UserId = "seller-1",
                채널종류 = "SmartStore",
                상점명 = "내 상점",
                연결상태 = "준비",
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            },
            new 판매채널계정
            {
                Id = 22,
                UserId = "seller-2",
                채널종류 = "Coupang",
                상점명 = "다른 상점",
                연결상태 = "준비",
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            });
        await db.SaveChangesAsync();

        var service = new SalesChannelService(
            db,
            new TestCurrentUserAccessor("seller-1", "판매자"),
            null!,
            new TestCredentialEncryptionService());
        var useCase = new 판매채널UseCase(service, null!);

        var list = await service.GetAccountsAsync(default);
        var own = await service.GetAccountAsync(11, default);
        var hidden = await service.GetAccountAsync(22, default);
        var hiddenResult = await useCase.계정상세Async(22, default);

        var item = Assert.Single(list.Items);
        Assert.Equal(11, item.Id);
        Assert.Equal(createdAt, item.등록일시);
        Assert.Equal(updatedAt, item.수정일시);
        Assert.NotNull(own);
        Assert.Equal("내 상점", own.상점명);
        Assert.Null(hidden);
        Assert.True(hiddenResult.IsFailed);
        Assert.Equal(StatusCodes.Status404NotFound, hiddenResult.Errors[0].Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 서버관리자는_운영검토를위해사용자경계밖계정도조회한다()
    {
        await using var db = CreateContext();
        db.판매채널계정.Add(new 판매채널계정
        {
            Id = 31,
            UserId = "seller-2",
            채널종류 = "Shopify",
            상점명 = "운영 검토 상점"
        });
        await db.SaveChangesAsync();
        var service = new SalesChannelService(
            db,
            new TestCurrentUserAccessor("admin-1", "서버관리자"),
            null!,
            new TestCredentialEncryptionService());

        var result = await service.GetAccountAsync(31, default);

        Assert.NotNull(result);
        Assert.Equal("운영 검토 상점", result.상점명);
    }

    [Fact]
    public async Task 판매채널자격증명은_암호화저장하고_응답에는마스킹상태만반환한다()
    {
        await using var db = CreateContext();
        var encryption = new DataProtectionSalesChannelCredentialEncryptionService(
            new EphemeralDataProtectionProvider());
        var service = new SalesChannelService(
            db,
            new TestCurrentUserAccessor("seller-1", "판매자"),
            null!,
            encryption);

        var created = await service.CreateAccountAsync(
            new 판매채널계정저장요청
            {
                채널종류 = CommerceChannelKeys.Shopify,
                상점명 = "해외 상점",
                인증정보 = new Dictionary<string, string>
                {
                    ["shopDomain"] = "my-shop.myshopify.com",
                    ["adminAccessToken"] = "shpat_test-secret-token"
                }
            },
            default);

        var stored = await db.판매채널계정.SingleAsync();
        var adapterCredentials = await service.GetAsync(stored.Id, default);
        Assert.True(encryption.IsProtected(stored.토큰암호화저장값));
        Assert.DoesNotContain("shpat_test-secret-token", stored.토큰암호화저장값, StringComparison.Ordinal);
        Assert.NotNull(adapterCredentials);
        Assert.Equal(
            "shpat_test-secret-token",
            adapterCredentials.Values["adminAccessToken"]);
        Assert.True(created.인증정보설정됨);
        Assert.DoesNotContain(
            created.인증필드상태,
            field => field.마스킹값.Contains("shpat_test-secret-token", StringComparison.Ordinal));
        Assert.Contains(
            created.인증필드상태,
            field => field.Key == "adminAccessToken"
                     && field.설정됨
                     && field.마스킹값.EndsWith("oken", StringComparison.Ordinal));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"sales-channel-account-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role) : ICurrentUserAccessor;

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }

    private sealed class TestCredentialEncryptionService : ISalesChannelCredentialEncryptionService
    {
        public string Protect(string value) => $"test:{value}";
        public string Unprotect(string protectedValue) => protectedValue["test:".Length..];
        public bool IsProtected(string value) => value.StartsWith("test:", StringComparison.Ordinal);
    }
}
