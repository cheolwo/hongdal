using FluentResults;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Mart;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Mart;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.창고;
using 살뜰.도메인.마트;
using 살뜰.도메인.판매;

namespace Ssalddel.Tests.Application.Mart;

public sealed class 마트공개상품구매후기UseCaseTests
{
    [Fact]
    public async Task 완료된공개상품은_기존후기게시판과원장을연결해작성한다()
    {
        await using var context = CreateContext();
        var productId = await SeedAsync(context, "완료");
        var publisher = new RecordingPublisher();
        var useCase = new 마트공개상품구매후기UseCase(context, publisher);

        var result = await useCase.작성Async(productId, new 마트공개상품구매후기작성요청
        {
            작성자표시명 = "감자 구매자",
            글비밀번호 = "safe-password",
            제목 = "함께 산 감자 후기",
            본문 = "상태가 좋았고 공동구매 진행 과정도 확인하기 쉬웠습니다."
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(901, result.Value.게시글Id);
        Assert.NotNull(publisher.LastRequest);
        Assert.Equal(CommunityLedgerCompletionPublication.Category, publisher.LastRequest.Category);
        Assert.Equal("ledger-review-1", publisher.LastRequest.커뮤니티원장Id);
        Assert.Equal("구매 참여자", publisher.LastRequest.RoleTag);
        Assert.Equal("감자 구매자", publisher.LastRequest.Nickname);
        Assert.Equal("safe-password", publisher.LastRequest.Password);
    }

    [Fact]
    public async Task 미완료원장은_후기발행을호출하지않고409를반환한다()
    {
        await using var context = CreateContext();
        var productId = await SeedAsync(context, "진행중");
        var publisher = new RecordingPublisher();
        var useCase = new 마트공개상품구매후기UseCase(context, publisher);

        var result = await useCase.작성Async(productId, new 마트공개상품구매후기작성요청
        {
            작성자표시명 = "구매자",
            글비밀번호 = "1234",
            제목 = "아직 완료 전",
            본문 = "완료 전에는 공개 후기 등록이 되면 안 됩니다."
        }, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(409, result.Errors.Single().Metadata["StatusCode"]);
        Assert.Null(publisher.LastRequest);
    }

    private static async Task<long> SeedAsync(SsalddelContext context, string ledgerState)
    {
        var now = new DateTime(2026, 7, 21, 8, 0, 0, DateTimeKind.Utc);
        var inbound = new 입고상품
        {
            상품명 = "감자",
            SKU = "POTATO",
            커뮤니티원장Id = "ledger-review-1",
            커뮤니티원장상태 = ledgerState,
            커뮤니티원장템플릿Key = "group-purchase",
            커뮤니티원장동기화시각Utc = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.입고상품.Add(inbound);
        await context.SaveChangesAsync();
        var salesProduct = new 판매상품
        {
            입고상품Id = inbound.Id,
            대표상품명 = "감자",
            판매SKU = "POTATO-SALE",
            CreatedAt = now,
            UpdatedAt = now
        };
        context.판매상품.Add(salesProduct);
        await context.SaveChangesAsync();
        var publicProduct = new 마트공개상품
        {
            판매상품Id = salesProduct.Id,
            상품명 = "감자 10kg",
            판매단위 = "상자",
            공개여부 = true,
            판매허용여부 = true,
            판매가능수량 = 5,
            재고기준시각Utc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        context.마트공개상품.Add(publicProduct);
        await context.SaveChangesAsync();
        return publicProduct.Id;
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class RecordingPublisher : I커뮤니티게시글발행UseCase
    {
        public PlatformCommunityPostCreateRequest? LastRequest { get; private set; }

        public Task<Result<PlatformCommunityPostResponse>> 생성Async(
            PlatformCommunityPostCreateRequest? request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(Result.Ok(new PlatformCommunityPostResponse
            {
                Id = 901,
                Category = request!.Category,
                WorkflowTag = request.WorkflowTag,
                RoleTag = request.RoleTag,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                커뮤니티원장Id = request.커뮤니티원장Id,
                Nickname = request.Nickname.Trim(),
                PublishedAtUtc = new DateTime(2026, 7, 21, 8, 30, 0, DateTimeKind.Utc),
                CreatedAtUtc = new DateTime(2026, 7, 21, 8, 30, 0, DateTimeKind.Utc)
            }));
        }

        public Task<Result<PlatformCommunityPostResponse>> 수정Async(
            long id,
            PlatformCommunityPostUpdateRequest? request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<Result> 삭제Async(
            long id,
            PlatformCommunityPostPasswordRequest? request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
