using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Services.HumanResources;

namespace Ssalddel.Tests.Services.HumanResources;

public sealed class SocialInsuranceFilingServiceTests
{
    [Fact]
    public async Task AssessAsync_EligibleWorker_PreparesEdiFiling()
    {
        var service = new InMemorySocialInsuranceFilingService();

        var result = await service.AssessAsync(new SocialInsuranceEligibilityAssessmentRequest
        {
            WorkerUserId = "worker-1",
            WorkerName = "Worker One",
            EmployerScopeType = HrScopeTypes.OrdererGroup,
            EmployerScopeId = "orderer-group:apt-1",
            EmployerName = "Apt orderer group",
            ContractType = HrEmploymentContractTypes.PartTime,
            ContractStartDate = new DateOnly(2026, 7, 1),
            ExpectedMonthlyWorkHours = 80,
            ExpectedMonthlyWorkDays = 12,
            ExpectedEmploymentMonths = 3,
            ExpectedMonthlyWage = 1_200_000,
            EmployerCanEmployWorkers = true,
            EmployerHasBusinessRegistration = true,
            PreferEdi = true
        });

        Assert.Equal(SocialInsuranceFilingStatusCodes.EdiPreparationReady, result.OverallStatus);
        Assert.All(result.Items, item =>
        {
            Assert.NotEqual(SocialInsuranceEligibilityDecisionCodes.ManualReviewRequired, item.Decision);
        });
        Assert.Contains(
            result.Items,
            x => x.InsuranceType == SocialInsuranceTypeCodes.HealthInsurance
                && x.Decision == SocialInsuranceEligibilityDecisionCodes.Required);
    }

    [Fact]
    public async Task CreatePlanAsync_EmployerEntityUnclear_KeepsManualReview()
    {
        var service = new InMemorySocialInsuranceFilingService();

        var plan = await service.CreatePlanAsync(new SocialInsuranceFilingPlanCreateRequest
        {
            Assessment = new SocialInsuranceEligibilityAssessmentRequest
            {
                WorkerUserId = "worker-2",
                WorkerName = "Worker Two",
                EmployerScopeType = HrScopeTypes.OrdererGroup,
                EmployerScopeId = "orderer-group:apt-2",
                EmployerName = "Informal orderer group",
                ContractType = HrEmploymentContractTypes.PartTime,
                ContractStartDate = new DateOnly(2026, 7, 1),
                ExpectedMonthlyWorkHours = 80,
                ExpectedEmploymentMonths = 3,
                EmployerCanEmployWorkers = false,
                EmployerHasBusinessRegistration = false,
                PreferEdi = true
            },
            PreparedByUserId = "admin-1"
        });

        Assert.Equal(SocialInsuranceFilingChannelCodes.Manual, plan.FilingChannel);
        Assert.Equal(SocialInsuranceFilingStatusCodes.ManualReviewRequired, plan.FilingStatus);
        Assert.Contains(SocialInsuranceFilingRequiredActionCodes.ConfirmEmployerEntity, plan.RequiredActionCodes);
        Assert.Contains(SocialInsuranceFilingRequiredActionCodes.ConfirmBusinessRegistration, plan.RequiredActionCodes);
    }

    [Fact]
    public async Task UpdateStatusAsync_SubmittedByEdi_RecordsSubmission()
    {
        var service = new InMemorySocialInsuranceFilingService();
        var plan = await service.CreatePlanAsync(new SocialInsuranceFilingPlanCreateRequest
        {
            Assessment = new SocialInsuranceEligibilityAssessmentRequest
            {
                WorkerUserId = "worker-3",
                ContractStartDate = new DateOnly(2026, 7, 1),
                ExpectedMonthlyWorkHours = 80,
                ExpectedEmploymentMonths = 3,
                EmployerCanEmployWorkers = true,
                EmployerHasBusinessRegistration = true,
                PreferEdi = true
            }
        });

        var updated = await service.UpdateStatusAsync(plan.Id, new SocialInsuranceFilingStatusUpdateRequest
        {
            FilingStatus = SocialInsuranceFilingStatusCodes.SubmittedByEdi,
            SubmittedByUserId = "admin-1",
            SubmissionReferenceNumber = "EDI-20260707-1"
        });

        Assert.Equal(SocialInsuranceFilingStatusCodes.SubmittedByEdi, updated.FilingStatus);
        Assert.Equal("admin-1", updated.SubmittedByUserId);
        Assert.Equal("EDI-20260707-1", updated.SubmissionReferenceNumber);
        Assert.NotNull(updated.SubmittedAtUtc);
        Assert.Contains(SocialInsuranceFilingRequiredActionCodes.UpdateSubmissionResult, updated.RequiredActionCodes);
    }
}
