using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Shipper.Payment;
using Ssalddel.Application.Shipper.Payment.Events;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Options;
using 살뜰.도메인.공통;
using 살뜰.도메인.화주;

namespace Ssalddel.Tests.Application.Shipper.Payment;

public sealed class 페이크결제승인CommandHandlerTests
{
    [Fact]
    public async Task Handle_승인완료결제와_결제완료Outbox를_같이저장한다()
    {
        await using var db = CreateContext();
        db.화주운송의뢰.Add(new 화주운송의뢰
        {
            의뢰Id = "request-1",
            화주Id = "shipper-1",
            주문자UserId = "shipper-1",
            화물종류 = "테스트 화물",
            배차상태 = 상태값.배차상태.상차완료,
            결제상태 = 상태값.결제상태.결제대기,
            결제예정금액 = 42000
        });
        await db.SaveChangesAsync();

        var handler = new 페이크결제승인CommandHandler(
            db,
            new TestCurrentUserAccessor("shipper-1", "화주"),
            new TestHostEnvironment(),
            new TestExecutionModePolicy(SsalddelExecutionMode.Simulation));

        var result = await handler.Handle(
            new 페이크결제승인Command("request-1", 42000, "카드", null, "fake-key-1"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var payment = await db.결제.SingleAsync();
        var outbox = await db.결제승인완료Outbox.SingleAsync();
        Assert.Equal(payment.Id, outbox.결제레코드Id);
        Assert.Equal(payment.결제Id, outbox.결제Id);
        Assert.Equal("Pending", outbox.처리상태);

        var payload = System.Text.Json.JsonSerializer.Deserialize<결제승인완료Event>(
            outbox.PayloadJson,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        Assert.NotNull(payload);
        Assert.Equal("request-1", payload.대상Id);
    }

    [Fact]
    public async Task Handle_이미완료된FakePG결제에_Outbox가없으면_복구한다()
    {
        await using var db = CreateContext();
        var request = new 화주운송의뢰
        {
            의뢰Id = "request-2",
            화주Id = "shipper-1",
            주문자UserId = "shipper-1",
            화물종류 = "테스트 화물",
            배차상태 = 상태값.배차상태.상차완료,
            결제상태 = 상태값.결제상태.결제완료,
            결제예정금액 = 55000
        };
        db.화주운송의뢰.Add(request);
        db.결제.Add(new 살뜰.도메인.결제.결제
        {
            결제Id = "payment-2",
            의뢰Id = request.의뢰Id,
            화주Id = request.화주Id,
            결제대상유형 = 살뜰.도메인.결제.결제공통정의.결제대상유형.용달운송의뢰,
            대상Id = request.의뢰Id,
            PG사 = "FakePG",
            결제제공자 = 살뜰.도메인.결제.결제공통정의.결제제공자.FakePG,
            결제상태 = 상태값.결제상태.결제완료,
            결제금액 = 55000,
            통화 = "KRW",
            OrderId = "order-2",
            승인일시 = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new 페이크결제승인CommandHandler(
            db,
            new TestCurrentUserAccessor("shipper-1", "화주"),
            new TestHostEnvironment(),
            new TestExecutionModePolicy(SsalddelExecutionMode.Simulation));

        var result = await handler.Handle(
            new 페이크결제승인Command("request-2", 55000, "카드", null, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.이미완료됨);
        Assert.Single(await db.결제승인완료Outbox.ToListAsync());
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"fake-payment-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role) : ICurrentUserAccessor;

    private sealed record TestExecutionModePolicy(SsalddelExecutionMode Mode) : ISsalddelExecutionModePolicy
    {
        public bool IsSimulation => Mode == SsalddelExecutionMode.Simulation;
        public bool IsOperational => Mode == SsalddelExecutionMode.Operational;
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Ssalddel.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
