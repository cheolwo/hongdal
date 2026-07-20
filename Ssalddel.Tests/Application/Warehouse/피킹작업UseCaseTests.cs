using MediatR;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Warehouse;
using Ssalddel.Application.Warehouse.Events;
using Ssalddel.Contracts.Common.ViewSettings;
using Ssalddel.Contracts.Common.Warehouse;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Audit;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Application.Warehouse;

public sealed class 피킹작업UseCaseTests
{
    [Fact]
    public async Task 목록은_접근가능한피킹만반환하고_포장과다른창고작업을숨긴다()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var useCase = CreateUseCase(context, "worker-a");

        var result = await useCase.목록Async(new 피킹작업목록조회요청
        {
            Status = 피킹작업조회상태코드.전체,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["PICK-A"], result.Value.Items.Select(item => item.TaskKey));
    }

    [Fact]
    public async Task 상세는_정확한TaskKey만조회하고_접근불가작업은404로숨긴다()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var useCase = CreateUseCase(context, "worker-a");

        var found = await useCase.상세Async("PICK-A", CancellationToken.None);
        var hidden = await useCase.상세Async("PICK-B", CancellationToken.None);

        Assert.True(found.IsSuccess);
        Assert.Equal("PICK-A", found.Value.TaskKey);
        Assert.True(found.Value.CanStart);
        Assert.True(hidden.IsFailed);
        Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 시작은_대기에서진행중으로전이하고_재호출은감사로그를중복생성하지않는다()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var logs = new RecordingActivityLogService();
        var publisher = new RecordingPublisher();
        var useCase = CreateUseCase(context, "worker-a", logs, publisher);

        var first = await useCase.시작Async("PICK-A", RequestContext(), CancellationToken.None);
        var replay = await useCase.시작Async("PICK-A", RequestContext(), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(피킹포장작업상태.진행중, first.Value.Status);
        Assert.False(first.Value.IdempotentReplay);
        Assert.True(replay.Value.IdempotentReplay);
        Assert.Single(logs.Entries);
        Assert.Empty(publisher.Notifications);
        Assert.NotNull((await context.피킹포장작업.SingleAsync(item => item.작업Key == "PICK-A")).시작일시Utc);
    }

    [Fact]
    public async Task 완료는_적재대와두확인을검증하고_완료Event와감사를한번만발행한다()
    {
        await using var context = CreateContext();
        await SeedAsync(context, ownStatus: 피킹포장작업상태.진행중);
        var logs = new RecordingActivityLogService();
        var publisher = new RecordingPublisher();
        var useCase = CreateUseCase(context, "worker-a", logs, publisher);

        var invalid = await useCase.완료Async("PICK-A", new 피킹작업완료요청
        {
            RackCode = "WRONG",
            ProductConfirmed = true,
            QuantityConfirmed = true
        }, RequestContext(), CancellationToken.None);
        var request = new 피킹작업완료요청
        {
            RackCode = "RACK-A-01",
            ProductConfirmed = true,
            QuantityConfirmed = true
        };
        var first = await useCase.완료Async("PICK-A", request, RequestContext(), CancellationToken.None);
        var replay = await useCase.완료Async("PICK-A", request, RequestContext(), CancellationToken.None);

        Assert.True(invalid.IsFailed);
        Assert.Equal(400, invalid.Errors.Single().Metadata["StatusCode"]);
        Assert.True(first.IsSuccess);
        Assert.Equal(피킹포장작업상태.완료, first.Value.Status);
        Assert.False(first.Value.IdempotentReplay);
        Assert.True(replay.Value.IdempotentReplay);
        Assert.Single(logs.Entries);
        Assert.IsType<창고피킹완료됨Event>(Assert.Single(publisher.Notifications));
    }

    private static 피킹작업UseCase CreateUseCase(
        SsalddelContext context,
        string? userId,
        RecordingActivityLogService? logs = null,
        RecordingPublisher? publisher = null)
        => new(
            context,
            new FakeCurrentUserAccessor(userId, 역할명.창고관리자),
            logs ?? new RecordingActivityLogService(),
            publisher ?? new RecordingPublisher());

    private static 창고작업요청Context RequestContext()
        => new("WarehouseManagerApp", "worker-a", "피킹 작업자", 역할명.창고관리자,
            "/work/picking-batch", "trace-pick", "127.0.0.1", "test");

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task SeedAsync(SsalddelContext context, string ownStatus = 피킹포장작업상태.대기)
    {
        var now = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc);
        var ownWarehouse = new 창고 { 소유자UserId = "owner-a", 창고명 = "공동 창고 A", CreatedAt = now, UpdatedAt = now };
        var otherWarehouse = new 창고 { 소유자UserId = "owner-b", 창고명 = "공동 창고 B", CreatedAt = now, UpdatedAt = now };
        context.창고.AddRange(ownWarehouse, otherWarehouse);
        await context.SaveChangesAsync();

        context.피킹포장작업.AddRange(
            CreateTask("PICK-A", 피킹포장작업유형.피킹, ownStatus, ownWarehouse, "worker-a", "RACK-A-01", now),
            CreateTask("PACK-A", 피킹포장작업유형.포장, 피킹포장작업상태.대기, ownWarehouse, "worker-a", "PACK-A-01", now),
            CreateTask("PICK-B", 피킹포장작업유형.피킹, 피킹포장작업상태.대기, otherWarehouse, "worker-b", "RACK-B-01", now));
        await context.SaveChangesAsync();
    }

    private static 피킹포장작업 CreateTask(
        string key,
        string type,
        string status,
        창고 warehouse,
        string workerId,
        string rackCode,
        DateTime now)
        => new()
        {
            작업Key = key,
            작업유형 = type,
            처리방식 = "피킹포장분리",
            상태 = status,
            창고Id = warehouse.Id,
            창고명 = warehouse.창고명,
            작업자UserId = workerId,
            작업자표시명 = workerId,
            주문참조번호 = $"ORDER-{key}",
            라인Key = $"LINE-{key}",
            상품명 = "공동구매 감자",
            SKU = "POTATO-01",
            수량 = 12,
            적재대코드 = rackCode,
            묶음바코드 = $"BUNDLE-{key}",
            CreatedAt = now,
            UpdatedAt = now
        };

    private sealed class FakeCurrentUserAccessor(string? userId, string? role) : ICurrentUserAccessor
    {
        public string? UserId { get; } = userId;
        public string? Role { get; } = role;
    }

    private sealed class RecordingActivityLogService : I사용자행위로그Service
    {
        public List<사용자행위로그기록> Entries { get; } = [];

        public Task 기록Async(사용자행위로그기록 entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingPublisher : IPublisher
    {
        public List<object> Notifications { get; } = [];

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
