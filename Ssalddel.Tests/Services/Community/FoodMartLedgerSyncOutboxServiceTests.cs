using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Participants;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Community;
using Ssalddel.Services.Outbox;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Services.Community;

public sealed class FoodMartLedgerSyncOutboxServiceTests
{
    [Fact]
    public async Task Mongo결과가없으면_대기로남기고_다음처리에서성공한다()
    {
        await using var db = CreateContext();
        var sync = new RecordingLedgerSync { ReturnNull = true };
        var service = CreateService(db, sync);

        await service.음식주문예약후즉시처리Async(
            CreateOrder(),
            "orderer-a",
            "food-event:event-1");

        var pending = await db.음식마트원장동기화Outbox.SingleAsync();
        Assert.Equal(OutboxProcessingStatuses.Pending, pending.처리상태);
        Assert.Equal(1, pending.시도횟수);
        Assert.NotEmpty(pending.마지막오류);

        pending.UpdatedAtUtc = DateTime.UtcNow - TimeSpan.FromMinutes(1);
        await db.SaveChangesAsync();
        sync.ReturnNull = false;

        var processed = await service.대기항목처리Async();

        Assert.Equal(1, processed);
        Assert.Equal(OutboxProcessingStatuses.Succeeded, pending.처리상태);
        Assert.Equal(2, pending.시도횟수);
        Assert.Equal(2, sync.FoodAttempts);
    }

    [Fact]
    public async Task 같은멱등키는_Outbox항목을중복생성하지않는다()
    {
        await using var db = CreateContext();
        var sync = new RecordingLedgerSync();
        var service = CreateService(db, sync);
        var order = CreateOrder();

        await service.음식주문예약후즉시처리Async(order, "orderer-a", "food-event:event-2");
        await service.음식주문예약후즉시처리Async(order, "orderer-a", "food-event:event-2");

        Assert.Equal(1, await db.음식마트원장동기화Outbox.CountAsync());
        Assert.Equal(1, sync.FoodAttempts);
    }

    [Fact]
    public async Task 출고동기화는_Outbox에저장한Id로최신Rdb항목을다시읽는다()
    {
        await using var db = CreateContext();
        var outbound = new 출고예정
        {
            주문참조번호 = "MART-100",
            상품명 = "쌀",
            SKU = "RICE-10KG",
            수량 = 1,
            상태 = 출고상태.예정,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.출고예정.Add(outbound);
        await db.SaveChangesAsync();

        var sync = new RecordingLedgerSync();
        var service = CreateService(db, sync);
        await service.출고원장예약후즉시처리Async(
            [outbound],
            [],
            "warehouse-a",
            "warehouse:MART-100:1",
            currentStageKey: "출고 예정");

        var savedOutbox = await db.음식마트원장동기화Outbox.SingleAsync();
        Assert.True(sync.OutboundAttempts == 1, savedOutbox.마지막오류);
        Assert.Equal(outbound.Id, Assert.Single(sync.LastOutboundIds));
        Assert.Equal(
            OutboxProcessingStatuses.Succeeded,
            savedOutbox.처리상태);
    }

    private static 음식마트원장동기화OutboxService CreateService(
        SsalddelContext db,
        I음식마트원장Mongo동기화Service sync)
        => new(
            db,
            sync,
            NullLogger<음식마트원장동기화OutboxService>.Instance);

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .EnableSensitiveDataLogging()
                .Options,
            new PassThroughEncryption());

    private static 음식주문응답 CreateOrder()
        => new()
        {
            주문번호 = "FOOD-100",
            음식점Id = 101,
            음식점명 = "테스트 음식점",
            주문자UserId = "orderer-a",
            상태 = "주문 접수",
            배차상태 = 음식주문배차상태코드.미요청,
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = "주문자",
                주소 = "서울시"
            },
            CreatedAt = DateTime.UtcNow
        };

    private sealed class RecordingLedgerSync : I음식마트원장Mongo동기화Service
    {
        public bool ReturnNull { get; set; }
        public int FoodAttempts { get; private set; }
        public int OutboundAttempts { get; private set; }
        public IReadOnlyList<long> LastOutboundIds { get; private set; } = [];

        public Task<커뮤니티원장Dto?> 음식주문동기화Async(
            음식주문응답 주문,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            FoodAttempts++;
            return Task.FromResult(ReturnNull ? null : CreateLedger($"food-order:{주문.주문번호}"));
        }

        public Task<커뮤니티원장Dto?> 출고원장동기화Async(
            IReadOnlyList<출고예정> 출고목록,
            IReadOnlyList<입고요청> 입고목록,
            string updatedBy,
            string? 현재단계Key = null,
            string? 원장템플릿Key = null,
            CancellationToken cancellationToken = default)
        {
            OutboundAttempts++;
            LastOutboundIds = 출고목록.Select(x => x.Id).ToArray();
            return Task.FromResult(ReturnNull ? null : CreateLedger("warehouse-outbound:MART-100"));
        }

        private static 커뮤니티원장Dto CreateLedger(string id)
            => new()
            {
                원장Id = id,
                상태 = "동기화됨"
            };
    }

    private sealed class PassThroughEncryption : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
