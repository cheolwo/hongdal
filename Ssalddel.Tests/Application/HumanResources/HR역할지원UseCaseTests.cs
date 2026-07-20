using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.HumanResources;
using Ssalddel.Contracts.Common.Hr;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Application.HumanResources;

public sealed class HR역할지원UseCaseTests
{
    [Fact]
    public async Task 조회와Command는_로그인사용자Id가없으면401을반환한다()
    {
        await using var context = CreateContext();
        var currentUser = new TestCurrentUserAccessor(null);
        var query = new HR역할지원조회UseCase(context, currentUser);
        var command = new HR역할지원CommandUseCase(context, currentUser);

        var queryResult = await query.내지원목록Async(CancellationToken.None);
        var commandResult = await command.제출Async(CreateRequest(), CancellationToken.None);

        Assert.Equal(401, queryResult.Errors.Single().Metadata["StatusCode"]);
        Assert.Equal(401, commandResult.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 제출은_현재동의와세가지확인을모두요구한다()
    {
        await using var context = CreateContext();
        var command = new HR역할지원CommandUseCase(context, new TestCurrentUserAccessor("user-a"));
        var request = CreateRequest();
        request.ConfirmedReviewDataUse = false;

        var result = await command.제출Async(request, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(400, result.Errors.Single().Metadata["StatusCode"]);
        Assert.Empty(context.HrRoleApplications);
    }

    [Fact]
    public async Task 제출은_요청Id와활성역할기준으로멱등하고민감정보를받지않는다()
    {
        await using var context = CreateContext();
        var command = new HR역할지원CommandUseCase(context, new TestCurrentUserAccessor("user-a"));
        var request = CreateRequest();

        var first = await command.제출Async(request, CancellationToken.None);
        var sameRequest = await command.제출Async(request, CancellationToken.None);
        var sameActiveRole = await command.제출Async(CreateRequest(), CancellationToken.None);
        var conflictingRequest = CreateRequest();
        conflictingRequest.SubmissionRequestId = request.SubmissionRequestId;
        conflictingRequest.RoleCode = HrDetailedRoleCodes.OrdererGroupDistributionWorker;
        var conflict = await command.제출Async(conflictingRequest, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(first.Value.ApplicationId, sameRequest.Value.ApplicationId);
        Assert.Equal(first.Value.ApplicationId, sameActiveRole.Value.ApplicationId);
        Assert.Equal(409, conflict.Errors.Single().Metadata["StatusCode"]);
        Assert.Single(context.HrRoleApplications);
        Assert.Equal(HrRoleApplicationStatusCodes.Submitted, first.Value.StatusCode);
        Assert.True(first.Value.CanWithdraw);
        Assert.Null(typeof(HrRoleApplicationSubmitRequest).GetProperty("PhoneNumber"));
        Assert.Null(typeof(HrRoleApplicationSubmitRequest).GetProperty("Address"));
        Assert.Null(typeof(HrRoleApplicationSubmitRequest).GetProperty("FreeText"));
    }

    [Fact]
    public async Task 철회는_본인지원만멱등처리하고같은역할재지원을허용한다()
    {
        await using var context = CreateContext();
        var ownerCommand = new HR역할지원CommandUseCase(context, new TestCurrentUserAccessor("user-a"));
        var otherCommand = new HR역할지원CommandUseCase(context, new TestCurrentUserAccessor("user-b"));
        var created = await ownerCommand.제출Async(CreateRequest(), CancellationToken.None);

        var denied = await otherCommand.철회Async(created.Value.ApplicationId, CancellationToken.None);
        var withdrawn = await ownerCommand.철회Async(created.Value.ApplicationId, CancellationToken.None);
        var repeated = await ownerCommand.철회Async(created.Value.ApplicationId, CancellationToken.None);
        var reapplied = await ownerCommand.제출Async(CreateRequest(), CancellationToken.None);

        Assert.Equal(404, denied.Errors.Single().Metadata["StatusCode"]);
        Assert.Equal(HrRoleApplicationStatusCodes.Withdrawn, withdrawn.Value.StatusCode);
        Assert.False(withdrawn.Value.CanWithdraw);
        Assert.Equal(withdrawn.Value.ApplicationId, repeated.Value.ApplicationId);
        Assert.NotEqual(withdrawn.Value.ApplicationId, reapplied.Value.ApplicationId);
        Assert.Equal(2, context.HrRoleApplications.Count());
        Assert.Single(context.HrRoleApplications.Where(item => item.ActiveApplicationKey != null));
    }

    [Fact]
    public async Task 조회는_현재사용자의지원만반환하고서버역할Catalog를함께제공한다()
    {
        await using var context = CreateContext();
        var userA = new TestCurrentUserAccessor("user-a");
        var userB = new TestCurrentUserAccessor("user-b");
        await new HR역할지원CommandUseCase(context, userA).제출Async(CreateRequest(), CancellationToken.None);
        var otherRequest = CreateRequest();
        otherRequest.RoleCode = HrDetailedRoleCodes.OrdererGroupDistributionWorker;
        await new HR역할지원CommandUseCase(context, userB).제출Async(otherRequest, CancellationToken.None);

        var result = await new HR역할지원조회UseCase(context, userA)
            .내지원목록Async(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Applications);
        Assert.Equal(HrDetailedRoleCodes.WarehouseInboundOperator, result.Value.Applications[0].RoleCode);
        Assert.Equal(HrRoleApplicationCatalog.Items.Count, result.Value.Options.Count);
    }

    private static HrRoleApplicationSubmitRequest CreateRequest()
        => new()
        {
            SubmissionRequestId = Guid.NewGuid(),
            RoleCode = HrDetailedRoleCodes.WarehouseInboundOperator,
            ConfirmedVoluntaryApplication = true,
            ConfirmedNoRoleOrEmploymentGuarantee = true,
            ConfirmedReviewDataUse = true,
            ConsentVersion = HrRoleApplicationConsent.CurrentVersion
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class TestCurrentUserAccessor(string? userId) : ICurrentUserAccessor
    {
        public string? UserId { get; } = userId;
        public string? Role => null;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
