using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.Json;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Connections.Commands;
using Ssalddel.Application.Connections.Handlers;
using Ssalddel.Application.Connections.Queries;
using Ssalddel.Domain.HumanResources;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.사용자;

namespace Ssalddel.Tests.Application.Connections;

public sealed class 업무관계친구요청작성CommandHandlerTests
{
    [Fact]
    public async Task 본인의친구요청가능업무관계는_상대식별자를노출하지않고_대기요청을만든다()
    {
        await using var context = CreateContext();
        var snapshot = CreateSnapshot(
            actorUserId: "driver-1",
            privacyLevel: WorkRelationshipPrivacyLevels.ConnectionRequestEligible);
        context.WorkRelationshipSnapshots.Add(snapshot);
        await context.SaveChangesAsync();
        var handler = new 업무관계친구요청작성CommandHandler(
            context,
            new TestCurrentUserAccessor("driver-1", "Driver"));

        var result = await handler.Handle(
            new 업무관계친구요청작성Command(
                snapshot.Id,
                "다음 운송에서도 제안받기",
                "안전하게 함께 운송해서 감사했습니다."),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var request = await context.친구요청.SingleAsync();
        Assert.Equal("driver-1", request.요청자참여자Id);
        Assert.Equal("shipper-1", request.대상자참여자Id);
        Assert.Equal(살뜰역할유형.기사, request.요청자역할);
        Assert.Equal(살뜰역할유형.판매자, request.대상자역할);
        Assert.Equal(친구요청상태.대기, request.상태);
        var outbox = await context.Command알림Outbox.SingleAsync();
        Assert.Equal("업무인연연결요청생성됨", outbox.EventName);
        using var outboxPayload = JsonDocument.Parse(outbox.PayloadJson);
        Assert.True(outboxPayload.RootElement.TryGetProperty("friendRequestId", out _));
        Assert.True(outboxPayload.RootElement.TryGetProperty("workRelationshipSnapshotId", out _));
        Assert.True(outboxPayload.RootElement.TryGetProperty("인연연결요청Id", out _));
        Assert.True(outboxPayload.RootElement.TryGetProperty("업무인연스냅샷Id", out _));
        Assert.Contains(snapshot.Id.ToString(), outbox.PayloadJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task 익명확인전용기록은_자동관계나연결요청으로바꾸지않는다()
    {
        await using var context = CreateContext();
        var snapshot = CreateSnapshot(
            actorUserId: "driver-1",
            privacyLevel: WorkRelationshipPrivacyLevels.ActorVisibleAnonymized);
        context.WorkRelationshipSnapshots.Add(snapshot);
        await context.SaveChangesAsync();
        var handler = new 업무관계친구요청작성CommandHandler(
            context,
            new TestCurrentUserAccessor("driver-1", "Driver"));

        var result = await handler.Handle(
            new 업무관계친구요청작성Command(snapshot.Id, "다시 만나기", "안녕하세요."),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("친구 요청에 사용할 수 없습니다", result.Errors.Single().Message);
        Assert.Empty(context.친구요청);
    }

    [Fact]
    public async Task 업무상대도_같은스냅샷에서_기록작성자에게친구요청을보낼수있다()
    {
        await using var context = CreateContext();
        var snapshot = CreateSnapshot(
            actorUserId: "driver-1",
            privacyLevel: WorkRelationshipPrivacyLevels.ConnectionRequestEligible);
        context.WorkRelationshipSnapshots.Add(snapshot);
        await context.SaveChangesAsync();
        var handler = new 업무관계친구요청작성CommandHandler(
            context,
            new TestCurrentUserAccessor("shipper-1", "Shipper"));

        var result = await handler.Handle(
            new 업무관계친구요청작성Command(snapshot.Id, "다음 운송 제안", "다시 함께하고 싶습니다."),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var request = await context.친구요청.SingleAsync();
        Assert.Equal("shipper-1", request.요청자참여자Id);
        Assert.Equal("driver-1", request.대상자참여자Id);
        Assert.Equal(살뜰역할유형.판매자, request.요청자역할);
        Assert.Equal(살뜰역할유형.기사, request.대상자역할);
    }

    [Fact]
    public async Task 다른사용자의업무관계기록은_존재여부를숨기고_친구요청을거부한다()
    {
        await using var context = CreateContext();
        var snapshot = CreateSnapshot(
            actorUserId: "driver-1",
            privacyLevel: WorkRelationshipPrivacyLevels.ConnectionRequestEligible);
        context.WorkRelationshipSnapshots.Add(snapshot);
        await context.SaveChangesAsync();
        var handler = new 업무관계친구요청작성CommandHandler(
            context,
            new TestCurrentUserAccessor("intruder", "Driver"));

        var result = await handler.Handle(
            new 업무관계친구요청작성Command(snapshot.Id, "다시 만나기", "안녕하세요."),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("현재 사용자의 업무 관계 기록을 찾을 수 없습니다", result.Errors.Single().Message);
        Assert.Empty(context.친구요청);
    }

    [Fact]
    public void 친구요청리팩터링은_기존직렬화와물리저장계약을유지한다()
    {
        using var context = CreateContext();

        var responseJson = JsonSerializer.Serialize(new 친구요청항목응답
        {
            친구요청Id = 17
        });
        using var responseDocument = JsonDocument.Parse(responseJson);
        Assert.Equal(17, responseDocument.RootElement.GetProperty("friendRequestId").GetInt64());
        Assert.Equal(17, responseDocument.RootElement.GetProperty("인연연결요청Id").GetInt64());

        var friendRequestEntity = context.Model.FindEntityType(typeof(친구요청));
        Assert.NotNull(friendRequestEntity);
        Assert.Equal("인연연결요청", friendRequestEntity.GetTableName());

        var consentEntity = context.Model.FindEntityType(typeof(연락처공개동의));
        Assert.NotNull(consentEntity);
        var consentTable = StoreObjectIdentifier.Table("연락처공개동의", schema: null);
        var friendRequestId = consentEntity.FindProperty(nameof(연락처공개동의.친구요청Id));
        Assert.NotNull(friendRequestId);
        Assert.Equal("인연연결요청_id", friendRequestId.GetColumnName(consentTable));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"work-relationship-connection-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static WorkRelationshipSnapshotRecord CreateSnapshot(
        string actorUserId,
        string privacyLevel)
        => new()
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            ActorAnonymousLabel = "user-driver",
            ActorRoleCode = "Driver",
            ActorRoleName = "기사",
            WorkDomain = "Dispatch",
            WorkProcess = "DriverAssignment",
            ActionCode = "DispatchAccepted",
            ActionLabel = "배차 수락",
            RelatedEntityType = "TransportRequest",
            RelatedEntityId = "TR-1",
            RelatedDisplayLabel = "운송 의뢰 TR-1",
            CounterpartyUserId = "shipper-1",
            CounterpartyAnonymousLabel = "user-shipper",
            CounterpartyRoleCode = "Shipper",
            PrivacyLevel = privacyLevel,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role)
        : ICurrentUserAccessor;

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
