using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Options;
using 살뜰.도메인.결제;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티활동유료상세UseCaseTests
{
    [Fact]
    public async Task 작성자는_본인_활동에만_유료상세를_등록하고_내용을_열람한다()
    {
        await using var db = CreateContext();
        var post = await AddPostAsync(db, "seller-1");
        var owner = CreateUseCase(db, "seller-1");

        var result = await owner.등록Async(CreateRequest(post.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.열람가능);
        Assert.Equal(커뮤니티활동상세열람근거.작성자본인, result.Value.열람근거);
        Assert.Equal("구매자에게만 보이는 상세 자료", result.Value.상세내용);
        Assert.Single(await db.커뮤니티활동유료상세목록.ToListAsync());
    }

    [Fact]
    public async Task 다른_사용자는_작성자의_활동에_유료상세를_등록할수없다()
    {
        await using var db = CreateContext();
        var post = await AddPostAsync(db, "seller-1");

        var result = await CreateUseCase(db, "buyer-1")
            .등록Async(CreateRequest(post.Id), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(403, result.Errors.Single().Metadata["StatusCode"]);
        Assert.Empty(await db.커뮤니티활동유료상세목록.ToListAsync());
    }

    [Fact]
    public async Task 구매전에는_미리보기만_보이고_상세내용조회는_거절한다()
    {
        await using var db = CreateContext();
        var detail = await RegisterAsync(db);
        var buyer = CreateUseCase(db, "buyer-1");

        var preview = await buyer.조회Async(detail.상세Id, false, CancellationToken.None);
        var activityPreview = await buyer.게시글별조회Async(detail.게시글Id, CancellationToken.None);
        var content = await buyer.조회Async(detail.상세Id, true, CancellationToken.None);

        Assert.True(preview.IsSuccess);
        Assert.True(activityPreview.IsSuccess);
        Assert.Equal(detail.상세Id, activityPreview.Value.상세Id);
        Assert.False(preview.Value.열람가능);
        Assert.Null(preview.Value.상세내용);
        Assert.Equal(커뮤니티활동상세열람근거.구매필요, preview.Value.열람근거);
        Assert.True(content.IsFailed);
        Assert.Equal(403, content.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task FakePG_승인은_결제와_열람권을_함께_기록하고_구매자에게_상세를_연다()
    {
        await using var db = CreateContext();
        var detail = await RegisterAsync(db);
        var buyer = CreateUseCase(db, "buyer-1");

        var payment = await buyer.페이크결제승인Async(
            detail.상세Id,
            new 커뮤니티활동상세FakePg결제승인Request
            {
                Amount = 3_000,
                IdempotencyKey = "purchase-1"
            },
            CancellationToken.None);
        var content = await buyer.조회Async(detail.상세Id, true, CancellationToken.None);

        Assert.True(payment.IsSuccess);
        Assert.False(payment.Value.이미완료됨);
        Assert.Equal(결제공통정의.결제대상유형.커뮤니티활동상세열람, payment.Value.결제대상유형);
        Assert.Equal(커뮤니티활동상세구매상태.열람권발급됨, payment.Value.구매Workflow.현재상태);
        Assert.Equal(
            [
                커뮤니티활동상세구매상태.요청됨,
                커뮤니티활동상세구매상태.결제승인됨,
                커뮤니티활동상세구매상태.열람권발급됨
            ],
            payment.Value.구매Workflow.상태이력.Select(x => x.상태));
        Assert.True(content.IsSuccess);
        Assert.Equal(커뮤니티활동상세열람근거.구매, content.Value.열람근거);
        Assert.Equal("구매자에게만 보이는 상세 자료", content.Value.상세내용);
        Assert.Single(await db.결제.Where(x => x.대상Id == detail.상세Id).ToListAsync());
        Assert.Single(await db.커뮤니티활동상세열람권목록.ToListAsync());
        Assert.Single(await db.커뮤니티활동상세구매목록.ToListAsync());
        Assert.Equal(3, await db.커뮤니티활동상세구매상태이력목록.CountAsync());
    }

    [Fact]
    public async Task 같은_구매를_반복해도_결제와_열람권을_중복생성하지않는다()
    {
        await using var db = CreateContext();
        var detail = await RegisterAsync(db);
        var buyer = CreateUseCase(db, "buyer-1");
        var request = new 커뮤니티활동상세FakePg결제승인Request
        {
            Amount = 3_000,
            IdempotencyKey = "same-purchase"
        };

        var first = await buyer.페이크결제승인Async(detail.상세Id, request, CancellationToken.None);
        var second = await buyer.페이크결제승인Async(detail.상세Id, request, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True(second.Value.이미완료됨);
        Assert.Equal(first.Value.결제Id, second.Value.결제Id);
        Assert.Single(await db.결제.Where(x => x.대상Id == detail.상세Id).ToListAsync());
        Assert.Single(await db.커뮤니티활동상세열람권목록.ToListAsync());
        Assert.Single(await db.커뮤니티활동상세구매목록.ToListAsync());
        Assert.Equal(3, await db.커뮤니티활동상세구매상태이력목록.CountAsync());
    }

    [Fact]
    public async Task 다른_구매자가_이미사용된_멱등성Key를_재사용하면_거절한다()
    {
        await using var db = CreateContext();
        var detail = await RegisterAsync(db);
        var request = new 커뮤니티활동상세FakePg결제승인Request
        {
            Amount = 3_000,
            IdempotencyKey = "shared-key"
        };

        var first = await CreateUseCase(db, "buyer-1")
            .페이크결제승인Async(detail.상세Id, request, CancellationToken.None);
        var collision = await CreateUseCase(db, "buyer-2")
            .페이크결제승인Async(detail.상세Id, request, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(collision.IsFailed);
        Assert.Equal(409, collision.Errors.Single().Metadata["StatusCode"]);
        Assert.Single(await db.커뮤니티활동상세구매목록.ToListAsync());
    }

    [Fact]
    public async Task 금액변조와_작성자본인구매와_운영모드FakePG를_거절한다()
    {
        await using var db = CreateContext();
        var detail = await RegisterAsync(db);
        var wrongAmount = await CreateUseCase(db, "buyer-1").페이크결제승인Async(
            detail.상세Id,
            new 커뮤니티활동상세FakePg결제승인Request { Amount = 100 },
            CancellationToken.None);
        var ownerPurchase = await CreateUseCase(db, "seller-1").페이크결제승인Async(
            detail.상세Id,
            new 커뮤니티활동상세FakePg결제승인Request { Amount = 3_000 },
            CancellationToken.None);
        var operationalPurchase = await CreateUseCase(
                db,
                "buyer-1",
                SsalddelExecutionMode.Operational,
                Environments.Production)
            .페이크결제승인Async(
                detail.상세Id,
                new 커뮤니티활동상세FakePg결제승인Request { Amount = 3_000 },
                CancellationToken.None);

        Assert.True(wrongAmount.IsFailed);
        Assert.True(ownerPurchase.IsFailed);
        Assert.True(operationalPurchase.IsFailed);
        Assert.Empty(await db.결제.Where(x => x.대상Id == detail.상세Id).ToListAsync());
        Assert.Empty(await db.커뮤니티활동상세구매목록.ToListAsync());
    }

    [Fact]
    public async Task 구매자는_완료된_구매원장을_재조회하지만_다른사용자는_조회할수없다()
    {
        await using var db = CreateContext();
        var detail = await RegisterAsync(db);
        var buyer = CreateUseCase(db, "buyer-1");
        var paid = await buyer.페이크결제승인Async(
            detail.상세Id,
            new 커뮤니티활동상세FakePg결제승인Request { Amount = 3_000 },
            CancellationToken.None);

        var own = await buyer.구매조회Async(paid.Value.구매Workflow.구매Id, CancellationToken.None);
        var other = await CreateUseCase(db, "buyer-2")
            .구매조회Async(paid.Value.구매Workflow.구매Id, CancellationToken.None);

        Assert.True(own.IsSuccess);
        Assert.Equal(3, own.Value.상태이력.Count);
        Assert.True(other.IsFailed);
        Assert.Equal(403, other.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public void 구매Policy는_순차상태전이만_허용한다()
    {
        Assert.True(커뮤니티활동유료상세Policy.상태전이가능한가(
            커뮤니티활동상세구매상태.요청됨,
            커뮤니티활동상세구매상태.결제승인됨));
        Assert.True(커뮤니티활동유료상세Policy.상태전이가능한가(
            커뮤니티활동상세구매상태.결제승인됨,
            커뮤니티활동상세구매상태.열람권발급됨));
        Assert.False(커뮤니티활동유료상세Policy.상태전이가능한가(
            커뮤니티활동상세구매상태.요청됨,
            커뮤니티활동상세구매상태.열람권발급됨));
        Assert.False(커뮤니티활동유료상세Policy.상태전이가능한가(
            커뮤니티활동상세구매상태.열람권발급됨,
            커뮤니티활동상세구매상태.결제승인됨));
    }

    [Fact]
    public async Task 관계형DB에서_구매결제열람권과_상태이력을_함께저장하고_새Context로_재조회한다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        string purchaseId;
        string detailId;

        await using (var writeDb = CreateContext(connection))
        {
            await writeDb.Database.EnsureCreatedAsync();
            var detail = await RegisterAsync(writeDb);
            detailId = detail.상세Id;
            var paid = await CreateUseCase(writeDb, "buyer-1").페이크결제승인Async(
                detailId,
                new 커뮤니티활동상세FakePg결제승인Request
                {
                    Amount = 3_000,
                    IdempotencyKey = "sqlite-purchase"
                },
                CancellationToken.None);
            Assert.True(paid.IsSuccess);
            purchaseId = paid.Value.구매Workflow.구매Id;
        }

        await using (var readDb = CreateContext(connection))
        {
            var buyer = CreateUseCase(readDb, "buyer-1");
            var purchase = await buyer.구매조회Async(purchaseId, CancellationToken.None);
            var content = await buyer.조회Async(detailId, true, CancellationToken.None);

            Assert.True(purchase.IsSuccess);
            Assert.Equal(커뮤니티활동상세구매상태.열람권발급됨, purchase.Value.현재상태);
            Assert.Equal(3, purchase.Value.상태이력.Count);
            Assert.True(content.IsSuccess);
            Assert.Equal("구매자에게만 보이는 상세 자료", content.Value.상세내용);
        }
    }

    private static async Task<커뮤니티활동유료상세Response> RegisterAsync(SsalddelContext db)
    {
        var post = await AddPostAsync(db, "seller-1");
        var result = await CreateUseCase(db, "seller-1")
            .등록Async(CreateRequest(post.Id), CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static 커뮤니티활동유료상세등록Request CreateRequest(long postId)
        => new()
        {
            게시글Id = postId,
            공개미리보기 = "구매 전에 공개되는 요약",
            상세내용 = "구매자에게만 보이는 상세 자료",
            가격금액 = 3_000,
            통화Code = "KRW"
        };

    private static async Task<PlatformCommunityPost> AddPostAsync(SsalddelContext db, string authorUserId)
    {
        var now = DateTime.UtcNow;
        var post = new PlatformCommunityPost
        {
            AppKey = "platform",
            Category = "활동",
            WorkflowTag = "커뮤니티 신뢰",
            RoleTag = "커뮤니티회원",
            Title = "판매할 수 있는 활동 기록",
            Body = "누구나 보는 활동 요약",
            AuthorUserId = authorUserId,
            Nickname = "활동 작성자",
            PasswordHash = "registered-user",
            PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Published,
            PublishedAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        return post;
    }

    private static 커뮤니티활동유료상세UseCase CreateUseCase(
        SsalddelContext db,
        string? userId,
        SsalddelExecutionMode mode = SsalddelExecutionMode.Simulation,
        string environmentName = "Production")
    {
        var currentUser = new TestCurrentUserAccessor(userId);
        var processManager = new 커뮤니티활동상세구매ProcessManager(
            db,
            currentUser,
            new TestHostEnvironment(environmentName),
            new TestExecutionModePolicy(mode));
        return new 커뮤니티활동유료상세UseCase(db, currentUser, processManager);
    }

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"community-paid-detail-{Guid.NewGuid():N}")
                .Options,
            new DummyPersonalDataEncryptionService());

    private static SsalddelContext CreateContext(SqliteConnection connection)
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseSqlite(connection)
                .Options,
            new DummyPersonalDataEncryptionService());

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }

    private sealed record TestCurrentUserAccessor(string? UserId) : ICurrentUserAccessor
    {
        public string? Role => "커뮤니티회원";
    }

    private sealed class TestExecutionModePolicy(SsalddelExecutionMode mode) : ISsalddelExecutionModePolicy
    {
        public SsalddelExecutionMode Mode { get; } = mode;
        public bool IsSimulation => Mode == SsalddelExecutionMode.Simulation;
        public bool IsOperational => Mode == SsalddelExecutionMode.Operational;
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Ssalddel.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
