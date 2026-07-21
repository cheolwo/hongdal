using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Mart;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.창고;
using 살뜰.도메인.마트;
using 살뜰.도메인.판매;

namespace Ssalddel.Tests.Application.Mart;

public sealed class 마트공개상품조회UseCaseTests
{
    [Fact]
    public async Task 목록은_공개투영만중립정렬해반환하고내부원장을읽지않는다()
    {
        await using var context = CreateContext();
        var (availableId, _, _) = await SeedAsync(context);
        var useCase = new 마트공개상품조회UseCase(context);

        var result = await useCase.목록Async(new()
        {
            검색어 = "생수",
            판매가능만 = true,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(availableId, item.Id);
        Assert.Equal("생수 6입", item.상품명);
        Assert.True(item.판매가능여부);
        Assert.Equal(8, item.판매가능수량);
        Assert.Contains("직접 공개한 값이 아니라", result.Value.재고기준안내);
    }

    [Fact]
    public async Task 판매가능필터를끄면_공개품절상품도반환하지만비공개상품은숨긴다()
    {
        await using var context = CreateContext();
        var (_, unavailableId, _) = await SeedAsync(context);
        var useCase = new 마트공개상품조회UseCase(context);

        var result = await useCase.목록Async(new()
        {
            판매가능만 = false,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Contains(result.Value.Items, item => item.Id == unavailableId && !item.판매가능여부);
        Assert.DoesNotContain(result.Value.Items, item => item.상품명 == "비공개 상품");
    }

    [Fact]
    public async Task 정확한상세는_비공개ProductId를404로숨기고다른상품으로대체하지않는다()
    {
        await using var context = CreateContext();
        var (availableId, _, privateId) = await SeedAsync(context);
        var useCase = new 마트공개상품조회UseCase(context);

        var found = await useCase.상세Async(availableId, CancellationToken.None);
        var hidden = await useCase.상세Async(privateId, CancellationToken.None);

        Assert.True(found.IsSuccess);
        Assert.Equal(availableId, found.Value.Id);
        Assert.Equal("한 묶음", found.Value.판매단위);
        Assert.True(hidden.IsFailed);
        Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 상세는_완료원장의공개후기만최근3건으로투영하고원장원문을노출하지않는다()
    {
        await using var context = CreateContext();
        var now = new DateTime(2026, 7, 21, 6, 0, 0, DateTimeKind.Utc);
        const string ledgerId = "ledger-private-42";
        var inbound = new 입고상품
        {
            상품명 = "제철 감자",
            SKU = "POTATO-10KG",
            커뮤니티원장Id = ledgerId,
            커뮤니티원장상태 = "완료",
            커뮤니티원장동기화시각Utc = now.AddMinutes(-5),
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddMinutes(-5)
        };
        context.입고상품.Add(inbound);
        await context.SaveChangesAsync();

        var salesProduct = new 판매상품
        {
            입고상품Id = inbound.Id,
            대표상품명 = "제철 감자",
            판매SKU = "SALE-POTATO",
            판매가 = 19_000m,
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now
        };
        context.판매상품.Add(salesProduct);
        await context.SaveChangesAsync();

        var publicProduct = new 마트공개상품
        {
            판매상품Id = salesProduct.Id,
            상품명 = "제철 감자 10kg",
            카테고리 = "농산물",
            설명 = "완료된 공동구매를 거친 공개 상품",
            판매단위 = "상자",
            판매가 = 19_000m,
            공개여부 = true,
            판매허용여부 = true,
            판매가능수량 = 12,
            재고기준시각Utc = now,
            UpdatedAtUtc = now
        };
        context.마트공개상품.Add(publicProduct);
        context.PlatformCommunityPosts.Add(new PlatformCommunityPost
        {
            커뮤니티원장Id = ledgerId,
            Category = CommunityLedgerCompletionPublication.Category,
            AuthorUserId = CommunityLedgerCompletionPublication.SystemAuthorKey,
            Nickname = "시스템",
            Title = "공동구매 완료",
            Body = "개인정보를 제외한 완료 요약",
            PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Published,
            PublishedAtUtc = now.AddHours(-5),
            CreatedAtUtc = now.AddHours(-5)
        });
        for (var index = 1; index <= 4; index++)
        {
            context.PlatformCommunityPosts.Add(new PlatformCommunityPost
            {
                커뮤니티원장Id = ledgerId,
                Category = CommunityLedgerCompletionPublication.Category,
                AuthorUserId = $"buyer-{index}",
                Nickname = $"구매자 {index}",
                Title = $"공개 후기 {index}",
                Body = $"감자 상태가 좋았습니다.{Environment.NewLine}후기 {index}",
                RecommendationCount = index,
                CommentCount = index + 1,
                PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Published,
                PublishedAtUtc = now.AddHours(-index),
                CreatedAtUtc = now.AddHours(-index)
            });
        }
        context.PlatformCommunityPosts.Add(new PlatformCommunityPost
        {
            커뮤니티원장Id = ledgerId,
            Category = CommunityLedgerCompletionPublication.Category,
            AuthorUserId = "buyer-hidden",
            Nickname = "숨김",
            Title = "삭제 후기",
            Body = "노출되면 안 됨",
            IsDeleted = true,
            PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Published,
            PublishedAtUtc = now
        });
        await context.SaveChangesAsync();

        var result = await new 마트공개상품조회UseCase(context)
            .상세Async(publicProduct.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var evidence = result.Value.구매근거;
        Assert.True(evidence.완료원장확인여부);
        Assert.True(evidence.후기작성가능여부);
        Assert.Equal(4, evidence.공개후기수);
        Assert.Equal(3, evidence.구매후기목록.Count);
        Assert.Equal(["공개 후기 1", "공개 후기 2", "공개 후기 3"], evidence.구매후기목록.Select(item => item.제목));
        Assert.DoesNotContain(evidence.구매후기목록, item => item.작성자표시명 is "시스템" or "숨김");
        Assert.Contains("참여자", evidence.공개범위안내);
        Assert.DoesNotContain(ledgerId, JsonSerializer.Serialize(result.Value), StringComparison.Ordinal);
    }

    [Fact]
    public async Task 상세는_미완료원장후기를판매근거로노출하지않는다()
    {
        await using var context = CreateContext();
        var now = new DateTime(2026, 7, 21, 7, 0, 0, DateTimeKind.Utc);
        var inbound = new 입고상품
        {
            상품명 = "양파",
            SKU = "ONION",
            커뮤니티원장Id = "ledger-pending",
            커뮤니티원장상태 = "진행중",
            커뮤니티원장동기화시각Utc = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.입고상품.Add(inbound);
        await context.SaveChangesAsync();
        var salesProduct = new 판매상품 { 입고상품Id = inbound.Id, 대표상품명 = "양파", 판매SKU = "ONION-SALE" };
        context.판매상품.Add(salesProduct);
        await context.SaveChangesAsync();
        var publicProduct = new 마트공개상품
        {
            판매상품Id = salesProduct.Id,
            상품명 = "양파",
            판매단위 = "망",
            공개여부 = true,
            판매허용여부 = true,
            판매가능수량 = 3,
            재고기준시각Utc = now
        };
        context.마트공개상품.Add(publicProduct);
        context.PlatformCommunityPosts.Add(new PlatformCommunityPost
        {
            커뮤니티원장Id = inbound.커뮤니티원장Id,
            Category = CommunityLedgerCompletionPublication.Category,
            AuthorUserId = "buyer",
            Nickname = "구매자",
            Title = "완료 전 글",
            Body = "완료 전에는 판매 근거로 세지 않습니다.",
            PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Published,
            PublishedAtUtc = now
        });
        await context.SaveChangesAsync();

        var result = await new 마트공개상품조회UseCase(context)
            .상세Async(publicProduct.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.구매근거.완료원장확인여부);
        Assert.False(result.Value.구매근거.후기작성가능여부);
        Assert.Empty(result.Value.구매근거.구매후기목록);
        Assert.Equal(0, result.Value.구매근거.공개후기수);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task<(long AvailableId, long UnavailableId, long PrivateId)> SeedAsync(
        SsalddelContext context)
    {
        var now = new DateTime(2026, 7, 20, 8, 30, 0, DateTimeKind.Utc);
        var available = new 마트공개상품
        {
            상품명 = "생수 6입",
            카테고리 = "상온",
            짧은설명 = "묶음 포장 상품",
            설명 = "공개 상품 설명",
            판매단위 = "한 묶음",
            판매가 = 4200m,
            판매가능수량 = 8,
            공개여부 = true,
            판매허용여부 = true,
            재고기준시각Utc = now,
            UpdatedAtUtc = now
        };
        var unavailable = new 마트공개상품
        {
            상품명 = "휴지 12롤",
            카테고리 = "생활",
            짧은설명 = "현재 투영 수량 없음",
            설명 = "공개 품절 상품",
            판매단위 = "한 팩",
            판매가 = 9800m,
            판매가능수량 = 0,
            공개여부 = true,
            판매허용여부 = true,
            재고기준시각Utc = now,
            UpdatedAtUtc = now
        };
        var privateItem = new 마트공개상품
        {
            상품명 = "비공개 상품",
            카테고리 = "내부",
            판매단위 = "개",
            판매가 = 1m,
            판매가능수량 = 99,
            공개여부 = false,
            판매허용여부 = true,
            재고기준시각Utc = now,
            UpdatedAtUtc = now
        };
        context.마트공개상품.AddRange(available, unavailable, privateItem);
        await context.SaveChangesAsync();
        return (available.Id, unavailable.Id, privateItem.Id);
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
