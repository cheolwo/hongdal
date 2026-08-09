using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Driver.Transport;
using Ssalddel.Application.Warehouse;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.공통;
using 살뜰.도메인.운송;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Application.Warehouse;

public sealed class 창고입고화물인계조회UseCaseTests
{
    [Fact]
    public async Task 창고배정범위의_운송과입고만_같은Canonical관계로반환한다()
    {
        await using var context = CreateContext();
        var ids = await SeedAsync(context);
        var useCase = new 창고입고화물인계조회UseCase(
            context,
            new FakeCurrentUserAccessor("worker-a", 역할명.창고관리자));

        var visible = await useCase.조회Async(ids.AccessibleWarehouseId, default);
        var hidden = await useCase.조회Async(ids.HiddenWarehouseId, default);

        var handoff = Assert.Single(visible);
        Assert.Equal($"inbound-task:{ids.AccessibleInboundId}", handoff.InboundTaskStableId);
        Assert.Equal("InTransit", handoff.HandoffStateCode);
        Assert.Equal("transport-network", Assert.Single(handoff.Movements).WorldZoneCode);
        Assert.Empty(hidden);
    }

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options,
            new DummyEncryption());

    private static async Task<(long AccessibleWarehouseId, long HiddenWarehouseId, long AccessibleInboundId)> SeedAsync(
        SsalddelContext context)
    {
        var now = new DateTime(2026, 8, 8, 2, 0, 0, DateTimeKind.Utc);
        var accessible = new 창고 { 창고명 = "접근 창고", 소유자UserId = "owner-a", CreatedAt = now, UpdatedAt = now };
        var hidden = new 창고 { 창고명 = "숨김 창고", 소유자UserId = "owner-b", CreatedAt = now, UpdatedAt = now };
        context.창고.AddRange(accessible, hidden);
        await context.SaveChangesAsync();
        context.창고사용자.Add(new 창고사용자
        {
            창고Id = accessible.Id,
            UserId = "worker-a",
            역할명 = "입고",
            CreatedAt = now,
            UpdatedAt = now,
        });
        var visibleInbound = new 입고요청
        {
            창고Id = accessible.Id,
            운송의뢰Id = "transport-request-visible",
            상태 = "입고예정",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var hiddenInbound = new 입고요청
        {
            창고Id = hidden.Id,
            운송의뢰Id = "transport-request-hidden",
            상태 = "입고예정",
            CreatedAt = now,
            UpdatedAt = now,
        };
        context.입고요청.AddRange(visibleInbound, hiddenInbound);
        context.운송원장.AddRange(
            new 운송원장 { 운송번호 = "transport-request-visible", 상태 = 기사운송상태코드.운송중, UpdatedAt = now },
            new 운송원장 { 운송번호 = "transport-request-hidden", 상태 = 기사운송상태코드.운송중, UpdatedAt = now });
        await context.SaveChangesAsync();
        return (accessible.Id, hidden.Id, visibleInbound.Id);
    }

    private sealed class FakeCurrentUserAccessor(string? userId, string? role) : ICurrentUserAccessor
    {
        public string? UserId { get; } = userId;
        public string? Role { get; } = role;
    }

    private sealed class DummyEncryption : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
