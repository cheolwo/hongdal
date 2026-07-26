using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.HumanResources;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Domain.HumanResources;
using 살뜰.Data;
using 살뜰.Infrastructure.Persistence;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.사용자;

namespace Ssalddel.Tests.Application.HumanResources;

public sealed class HR역할검토조회UseCaseTests
{
    [Fact]
    public async Task 목록은_영속배정해제이력을검색하고사용자Id대신표시정보를반환한다()
    {
        await using var context = CreateContext();
        var seeded = await SeedAsync(context);
        var useCase = new HR역할검토조회UseCase(context);

        var result = await useCase.목록Async(new HrRoleReviewListRequest
        {
            Search = "입고",
            StatusCode = HrRoleReviewStatusCodes.Assigned,
            ScopeType = HrScopeTypes.Warehouse,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var review = Assert.Single(result.Value.Items);
        Assert.Equal(seeded.ActiveReviewId, review.ReviewId);
        Assert.Equal("김입고", review.ParticipantDisplayName);
        Assert.Equal("worker-a", review.ParticipantUserName);
        Assert.Equal(HrRoleReviewStatusCodes.Assigned, review.StatusCode);
        Assert.Null(typeof(HrRoleReviewSummaryResponse).GetProperty("UserId"));
    }

    [Fact]
    public async Task 상세는_정확한검토Id의근무조건을반환하고Ip원문은노출하지않는다()
    {
        await using var context = CreateContext();
        var seeded = await SeedAsync(context);
        var useCase = new HR역할검토조회UseCase(context);

        var result = await useCase.상세Async(seeded.ActiveReviewId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(seeded.ActiveReviewId, result.Value.ReviewId);
        Assert.Equal([DayOfWeek.Monday, DayOfWeek.Wednesday], result.Value.AllowedDaysOfWeek);
        Assert.Equal(new TimeOnly(9, 0), result.Value.WorkStartLocalTime);
        Assert.Equal(2, result.Value.AllowedWorksiteIpRangeCount);
        Assert.Equal("admin", result.Value.RecordedByDisplayName);
        Assert.Null(typeof(HrRoleReviewDetailResponse).GetProperty("AllowedWorksiteIpRanges"));
        Assert.Null(typeof(HrRoleReviewDetailResponse).GetProperty("UserId"));
    }

    [Fact]
    public async Task 목록과상세는_사용자지원원장을배정원장과구분해반환한다()
    {
        await using var context = CreateContext();
        var seeded = await SeedAsync(context);
        var useCase = new HR역할검토조회UseCase(context);

        var listResult = await useCase.목록Async(new HrRoleReviewListRequest
        {
            SourceCode = HrRoleReviewSourceCodes.RoleApplication,
            StatusCode = HrRoleReviewStatusCodes.Submitted
        }, CancellationToken.None);
        var detailResult = await useCase.상세Async(seeded.ApplicationReviewId, CancellationToken.None);

        Assert.True(listResult.IsSuccess);
        var application = Assert.Single(listResult.Value.Items);
        Assert.Equal(seeded.ApplicationReviewId, application.ReviewId);
        Assert.Equal(HrRoleReviewSourceCodes.RoleApplication, application.SourceCode);
        Assert.Equal(HrRoleReviewStatusCodes.Submitted, application.StatusCode);
        Assert.True(detailResult.IsSuccess);
        Assert.True(detailResult.Value.ConfirmedVoluntaryApplication);
        Assert.True(detailResult.Value.ConfirmedNoRoleOrEmploymentGuarantee);
        Assert.True(detailResult.Value.ConfirmedReviewDataUse);
        Assert.Equal(HrRoleApplicationConsent.CurrentVersion, detailResult.Value.ConsentVersion);
        Assert.Equal("지원자 본인", detailResult.Value.RecordedByDisplayName);
    }

    [Fact]
    public async Task 상세는_없는검토Id를다른배정으로대체하지않고404를반환한다()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var useCase = new HR역할검토조회UseCase(context);

        var result = await useCase.상세Async(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(404, result.Errors.Single().Metadata["StatusCode"]);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task<SeededIds> SeedAsync(SsalddelContext context)
    {
        var activeReviewId = Guid.NewGuid();
        var revokedReviewId = Guid.NewGuid();
        var applicationReviewId = Guid.NewGuid();
        var assignedAt = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc);
        context.Users.AddRange(
            new ApplicationUser { Id = "user-a", UserName = "worker-a", NormalizedUserName = "WORKER-A" },
            new ApplicationUser { Id = "user-b", UserName = "worker-b", NormalizedUserName = "WORKER-B" },
            new ApplicationUser { Id = "admin-id", UserName = "admin", NormalizedUserName = "ADMIN" });
        context.살뜰참여자.AddRange(
            new 살뜰참여자 { Id = "user-a", 표시이름 = "김입고" },
            new 살뜰참여자 { Id = "user-b", 표시이름 = "박배송" });
        context.HrRoleAssignments.AddRange(
            new HrRoleAssignmentRecord
            {
                Id = activeReviewId,
                UserId = "user-a",
                ScopeType = HrScopeTypes.Warehouse,
                ScopeId = "warehouse-17",
                ParticipantCategory = HrParticipantCategoryCodes.InternalProjectOperator,
                RoleCode = HrDetailedRoleCodes.WarehouseInboundOperator,
                RoleName = "창고 입고 담당자",
                IsActive = true,
                AssignedAtUtc = assignedAt,
                AssignedByUserId = "admin-id",
                WorkScheduleEnabled = true,
                TimeZoneId = "Asia/Seoul",
                AllowedDaysOfWeekCsv = "Monday,Wednesday",
                WorkStartLocalTimeText = "09:00:00",
                WorkEndLocalTimeText = "18:00:00",
                WorksiteIpRestrictionEnabled = true,
                AllowedWorksiteIpRangesText = "10.0.0.0/24;192.168.10.10",
                CreatedAt = assignedAt,
                UpdatedAt = assignedAt.AddMinutes(5)
            },
            new HrRoleAssignmentRecord
            {
                Id = revokedReviewId,
                UserId = "user-b",
                ScopeType = HrScopeTypes.Platform,
                ScopeId = HrScopeIds.Global,
                ParticipantCategory = HrParticipantCategoryCodes.ExternalProfessional,
                RoleCode = HrDetailedRoleCodes.ShippingAgencyOperator,
                RoleName = "배송대행 담당자",
                IsActive = false,
                AssignedAtUtc = assignedAt.AddDays(-1),
                AssignedByUserId = "admin-id",
                CreatedAt = assignedAt.AddDays(-1),
                UpdatedAt = assignedAt
            });
        context.HrRoleApplications.Add(new HrRoleApplicationRecord
        {
            Id = applicationReviewId,
            ApplicantUserId = "user-b",
            ParticipantCategory = HrParticipantCategoryCodes.CommunityPartTimeWorker,
            RequestedRoleCode = HrDetailedRoleCodes.OrdererGroupDistributionWorker,
            RequestedRoleName = "같이 주문 배부 지원",
            ScopeType = HrScopeTypes.Platform,
            ScopeId = HrScopeIds.Global,
            StatusCode = HrRoleApplicationStatusCodes.Submitted,
            SubmissionRequestId = Guid.NewGuid(),
            ActiveApplicationKey = Guid.NewGuid().ToString("N"),
            ConfirmedVoluntaryApplication = true,
            ConfirmedNoRoleOrEmploymentGuarantee = true,
            ConfirmedReviewDataUse = true,
            ConsentVersion = HrRoleApplicationConsent.CurrentVersion,
            SubmittedAtUtc = assignedAt.AddHours(1),
            CreatedAt = assignedAt.AddHours(1),
            UpdatedAt = assignedAt.AddHours(1)
        });
        await context.SaveChangesAsync();
        return new SeededIds(activeReviewId, revokedReviewId, applicationReviewId);
    }

    private sealed record SeededIds(Guid ActiveReviewId, Guid RevokedReviewId, Guid ApplicationReviewId);

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
