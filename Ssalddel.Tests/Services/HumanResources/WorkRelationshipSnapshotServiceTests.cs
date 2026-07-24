using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Domain.HumanResources;
using Ssalddel.Services.HumanResources;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.HumanResources;

public sealed class WorkRelationshipSnapshotServiceTests
{
    [Fact]
    public async Task 업무양쪽당사자는_같은인연기록을_자기관점의익명상대로조회한다()
    {
        await using var context = CreateContext();
        context.WorkRelationshipSnapshots.AddRange(
            CreateSnapshot(WorkRelationshipPrivacyLevels.ConnectionRequestEligible, "TR-1"),
            CreateSnapshot(WorkRelationshipPrivacyLevels.ActorVisibleAnonymized, "TR-PRIVATE"));
        await context.SaveChangesAsync();
        var service = new WorkRelationshipSnapshotService(
            context,
            new TestCurrentUserAccessor("shipper-1", "Shipper"),
            new HttpContextAccessor(),
            new TestOptionsMonitor<WorkRelationshipSnapshotOptions>(
                new WorkRelationshipSnapshotOptions { Enabled = true }));

        var result = await service.GetMineAsync(10, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("user-shipper", item.ActorAnonymousLabel);
        Assert.Equal("Shipper", item.ActorRoleCode);
        Assert.Equal("user-driver", item.CounterpartyAnonymousLabel);
        Assert.Equal("Driver", item.CounterpartyRoleCode);
    }

    private static WorkRelationshipSnapshotRecord CreateSnapshot(string privacyLevel, string requestId)
        => new()
        {
            Id = Guid.NewGuid(),
            ActorUserId = "driver-1",
            ActorAnonymousLabel = "user-driver",
            ActorRoleCode = "Driver",
            ActorRoleName = "기사",
            WorkDomain = "Dispatch",
            WorkProcess = "DriverAssignment",
            ActionCode = "DispatchAccepted",
            ActionLabel = "배차 수락",
            RelatedEntityType = "TransportRequest",
            RelatedEntityId = requestId,
            RelatedDisplayLabel = $"운송 의뢰 {requestId}",
            CounterpartyUserId = "shipper-1",
            CounterpartyAnonymousLabel = "user-shipper",
            CounterpartyRoleCode = "Shipper",
            PrivacyLevel = privacyLevel,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"work-relationship-read-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role)
        : ICurrentUserAccessor;

    private sealed class TestOptionsMonitor<T>(T currentValue) : IOptionsMonitor<T>
    {
        public T CurrentValue { get; } = currentValue;

        public T Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
