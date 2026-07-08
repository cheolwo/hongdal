using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Hr;
using Hongdal.Services.HumanResources;
using Microsoft.AspNetCore.Http;

namespace Hongdal.Application.HumanResources;

public interface I사회보험신고UseCase
{
    Task<Result<SocialInsuranceFilingPlanListResponse>> 목록Async(
        string? workerUserId,
        string? employerScopeType,
        string? employerScopeId,
        string? filingStatus,
        CancellationToken cancellationToken);

    Task<Result<SocialInsuranceFilingPlanResponse>> 상세Async(Guid id, CancellationToken cancellationToken);
    Task<Result<SocialInsuranceEligibilityAssessmentResponse>> 가입요건평가Async(SocialInsuranceEligibilityAssessmentRequest request, CancellationToken cancellationToken);
    Task<Result<SocialInsuranceFilingPlanResponse>> 계획생성Async(SocialInsuranceFilingPlanCreateRequest request, CancellationToken cancellationToken);
    Task<Result<SocialInsuranceFilingPlanResponse>> 상태수정Async(Guid id, SocialInsuranceFilingStatusUpdateRequest request, CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.HrParticipation)]
[HongdalUseCase("사회보험 신고", Summary = "공동주문 참여 고용 흐름에서 건강보험, 국민연금, 고용보험 신고 계획을 평가하고 진행 상태를 관리합니다.")]
[HongdalUseCaseActor(HongdalActor.EmployerOrOperatingEntity)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
public sealed class 사회보험신고UseCase : I사회보험신고UseCase
{
    private readonly ISocialInsuranceFilingService _filingService;

    public 사회보험신고UseCase(ISocialInsuranceFilingService filingService)
    {
        _filingService = filingService;
    }

    public async Task<Result<SocialInsuranceFilingPlanListResponse>> 목록Async(
        string? workerUserId,
        string? employerScopeType,
        string? employerScopeId,
        string? filingStatus,
        CancellationToken cancellationToken)
    {
        var items = await _filingService.ListAsync(
            workerUserId,
            employerScopeType,
            employerScopeId,
            filingStatus,
            cancellationToken);

        return Result.Ok(new SocialInsuranceFilingPlanListResponse { Items = items });
    }

    public async Task<Result<SocialInsuranceFilingPlanResponse>> 상세Async(Guid id, CancellationToken cancellationToken)
    {
        var plan = await _filingService.GetAsync(id, cancellationToken);
        return plan is null
            ? Result.Fail<SocialInsuranceFilingPlanResponse>(new Error("Social insurance filing plan was not found.").WithMetadata("StatusCode", StatusCodes.Status404NotFound))
            : Result.Ok(plan);
    }

    public async Task<Result<SocialInsuranceEligibilityAssessmentResponse>> 가입요건평가Async(
        SocialInsuranceEligibilityAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok(await _filingService.AssessAsync(request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<SocialInsuranceEligibilityAssessmentResponse>(ex.Message);
        }
    }

    public async Task<Result<SocialInsuranceFilingPlanResponse>> 계획생성Async(
        SocialInsuranceFilingPlanCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok(await _filingService.CreatePlanAsync(request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<SocialInsuranceFilingPlanResponse>(ex.Message);
        }
    }

    public async Task<Result<SocialInsuranceFilingPlanResponse>> 상태수정Async(
        Guid id,
        SocialInsuranceFilingStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok(await _filingService.UpdateStatusAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail<SocialInsuranceFilingPlanResponse>(new Error(ex.Message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<SocialInsuranceFilingPlanResponse>(ex.Message);
        }
    }
}
