using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Shipper.Request;
using Ssalddel.Contracts.Shipper.Request;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Options;
using 살뜰.도메인.결제;
using 살뜰.도메인.운송;
using 살뜰.도메인.화주;

namespace Ssalddel.Tests.Application.Shipper.Request;

public sealed class 관리자운송의뢰취소환불CommandHandlerTests
{
    [Fact]
    public async Task Simulation에서_결제완료_미배차_의뢰를_원장과_함께_종료한다()
    {
        await using var db = CreateContext();
        var entity = NewRequest("REQ-PAID", "결제완료", "배차대기");
        var payment = new 결제
        {
            결제Id = "PAY-001",
            의뢰Id = entity.의뢰Id,
            대상Id = entity.의뢰Id,
            결제상태 = "결제완료",
            공통결제상태 = 결제공통정의.결제상태.승인완료
        };
        var ledger = new 운송원장
        {
            의뢰Id = entity.의뢰Id,
            원본의뢰Id = entity.의뢰Id,
            상태 = "배차대기",
            현재추천대상기사Id = "driver-1"
        };
        db.AddRange(entity, payment, ledger);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db, role: 역할명.서버관리자, mode: SsalddelExecutionMode.Simulation);
        var result = await handler.Handle(
            new 관리자운송의뢰취소환불Command(entity.의뢰Id, entity.의뢰Id, "화주 요청 확인"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("취소", entity.상태);
        Assert.Equal("환불됨", entity.결제상태);
        Assert.Equal(운임정산상태.정산취소.ToString(), entity.정산상태);
        Assert.Equal("취소", entity.배차상태);
        Assert.Equal("환불됨", payment.결제상태);
        Assert.Equal(결제공통정의.결제상태.환불완료, payment.공통결제상태);
        Assert.NotNull(payment.취소일시);
        Assert.Equal("취소", ledger.상태);
        Assert.Equal(99, ledger.배차큐단계);
        Assert.Equal(990, ledger.배차노출상태);
        Assert.Null(ledger.현재추천대상기사Id);

        var audit = Assert.Single(db.운송이벤트);
        Assert.Equal(운송이벤트유형.관리자취소환불상태기록, audit.이벤트타입);
        using var auditMetadata = JsonDocument.Parse(audit.메타데이터);
        Assert.Equal("화주 요청 확인", auditMetadata.RootElement.GetProperty("사유").GetString());
    }

    [Fact]
    public async Task 결제대기_의뢰는_환불이_아닌_결제취소로_기록한다()
    {
        await using var db = CreateContext();
        var entity = NewRequest("REQ-WAITING", "결제대기", "미시작");
        var payment = new 결제
        {
            결제Id = "PAY-002",
            의뢰Id = entity.의뢰Id,
            대상Id = entity.의뢰Id,
            결제상태 = "결제대기",
            공통결제상태 = 결제공통정의.결제상태.요청생성
        };
        db.AddRange(entity, payment);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db, 역할명.서버관리자, SsalddelExecutionMode.Simulation);
        var result = await handler.Handle(
            new 관리자운송의뢰취소환불Command(entity.의뢰Id, entity.의뢰Id, "중복 의뢰"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("결제취소", entity.결제상태);
        Assert.Equal("결제취소", payment.결제상태);
        Assert.Equal(결제공통정의.결제상태.취소완료, payment.공통결제상태);
    }

    [Fact]
    public async Task 같은_관리자취소환불을_재시도하면_이벤트를중복기록하지않는다()
    {
        await using var db = CreateContext();
        var entity = NewRequest("REQ-IDEMPOTENT", "결제대기", "미시작");
        db.Add(entity);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db, 역할명.서버관리자, SsalddelExecutionMode.Simulation);
        var command = new 관리자운송의뢰취소환불Command(entity.의뢰Id, entity.의뢰Id, "사용자 취소 검토 승인");

        var first = await handler.Handle(command, CancellationToken.None);
        var retried = await handler.Handle(command, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(retried.IsSuccess);
        Assert.Single(db.운송이벤트);
    }

    [Fact]
    public async Task 운송중인_의뢰는_서버에서_거부한다()
    {
        await using var db = CreateContext();
        var entity = NewRequest("REQ-ACTIVE", "결제완료", "운송중");
        db.Add(entity);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db, 역할명.서버관리자, SsalddelExecutionMode.Simulation);
        var result = await handler.Handle(
            new 관리자운송의뢰취소환불Command(entity.의뢰Id, entity.의뢰Id, "운영 확인"),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal("생성됨", entity.상태);
        Assert.Empty(db.운송이벤트);
    }

    [Fact]
    public async Task 서버관리자가_아니면_의뢰_존재여부와_무관하게_거부한다()
    {
        await using var db = CreateContext();
        var handler = CreateHandler(db, 역할명.화주, SsalddelExecutionMode.Simulation);

        var result = await handler.Handle(
            new 관리자운송의뢰취소환불Command("REQ-NONE", "REQ-NONE", "잘못된 요청"),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("서버관리자", result.Errors[0].Message);
    }

    [Fact]
    public async Task Operational_모드에서는_외부환불_연동_전까지_거부한다()
    {
        await using var db = CreateContext();
        var handler = CreateHandler(db, 역할명.서버관리자, SsalddelExecutionMode.Operational);

        var result = await handler.Handle(
            new 관리자운송의뢰취소환불Command("REQ-NONE", "REQ-NONE", "운영 요청"),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("Operational", result.Errors[0].Message);
    }

    [Fact]
    public async Task 확인용_의뢰Id가_다르면_변경하지_않는다()
    {
        await using var db = CreateContext();
        var entity = NewRequest("REQ-CHECK", "결제대기", "미시작");
        db.Add(entity);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db, 역할명.서버관리자, SsalddelExecutionMode.Simulation);
        var result = await handler.Handle(
            new 관리자운송의뢰취소환불Command(entity.의뢰Id, "REQ-OTHER", "운영 요청"),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal("생성됨", entity.상태);
    }

    private static 관리자운송의뢰취소환불CommandHandler CreateHandler(
        SsalddelContext db,
        string role,
        SsalddelExecutionMode mode)
        => new(
            db,
            new TestCurrentUserAccessor("admin-1", role),
            new TestExecutionModePolicy(mode));

    private static 화주운송의뢰 NewRequest(string requestId, string paymentStatus, string dispatchStatus)
        => new()
        {
            의뢰Id = requestId,
            주문자UserId = "shipper-1",
            화주Id = "shipper-1",
            화물종류 = "생활용품",
            상태 = "생성됨",
            결제상태 = paymentStatus,
            정산상태 = paymentStatus,
            배차상태 = dispatchStatus,
            픽업_도로명주소 = "서울특별시 강남구",
            하차_도로명주소 = "경기도 성남시"
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"admin-request-cancel-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role) : ICurrentUserAccessor;

    private sealed record TestExecutionModePolicy(SsalddelExecutionMode Mode) : ISsalddelExecutionModePolicy
    {
        public bool IsSimulation => Mode == SsalddelExecutionMode.Simulation;
        public bool IsOperational => Mode == SsalddelExecutionMode.Operational;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
