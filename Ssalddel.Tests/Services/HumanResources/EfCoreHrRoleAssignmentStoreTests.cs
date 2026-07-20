using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Domain.HumanResources;
using Ssalddel.Services.HumanResources;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.사용자;

namespace Ssalddel.Tests.Services.HumanResources;

public sealed class EfCoreHrRoleAssignmentStoreTests
{
    [Fact]
    public async Task 역할목록으로_접근을_확인할_때_활성_배정을_조회한다()
    {
        await using var context = CreateContext();
        context.HrRoleAssignments.Add(new HrRoleAssignmentRecord
        {
            Id = Guid.NewGuid(),
            UserId = "warehouse-operator",
            ScopeType = HrScopeTypes.Platform,
            ScopeId = HrScopeIds.Global,
            ParticipantCategory = HrParticipantCategoryCodes.InternalProjectOperator,
            RoleCode = HrDetailedRoleCodes.WarehouseInboundOperator,
            RoleName = "창고 입고 담당자",
            IsActive = true,
            AssignedAtUtc = DateTime.UtcNow,
            AssignedByUserId = "system",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var store = new EfCoreHrRoleAssignmentStore(context);

        var decision = await store.AuthorizeAccessAsync(
            "warehouse-operator",
            HrScopeTypes.Platform,
            HrScopeIds.Global,
            [HrDetailedRoleCodes.WarehouseManager, HrDetailedRoleCodes.WarehouseInboundOperator],
            clientIpAddress: null,
            DateTimeOffset.UtcNow,
            CancellationToken.None);

        Assert.True(decision.IsAllowed);
        Assert.Equal(HrDetailedRoleCodes.WarehouseInboundOperator, decision.MatchedAssignment?.RoleCode);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"hr-role-access-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
