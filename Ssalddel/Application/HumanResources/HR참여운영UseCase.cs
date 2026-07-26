using FluentResults;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Services.HumanResources;
using Microsoft.AspNetCore.Http;

namespace Ssalddel.Application.HumanResources;

public interface IHR참여운영UseCase
{
    Task<Result<HrEmploymentContractListResponse>> 고용계약목록Async(string? workerUserId, string? employerScopeType, string? employerScopeId, CancellationToken cancellationToken);
    Task<Result<HrEmploymentContractResponse>> 고용계약상세Async(Guid contractId, CancellationToken cancellationToken);
    Task<Result<HrEmploymentContractResponse>> 고용계약초안생성Async(HrEmploymentContractDraftRequest request, CancellationToken cancellationToken);
    Task<Result<HrEmploymentContractResponse>> 고용계약서명Async(Guid contractId, HrEmploymentContractSignRequest request, CancellationToken cancellationToken);
    Task<Result<HrPayrollScheduleListResponse>> 급여스케줄생성Async(Guid contractId, HrPayrollScheduleCreateRequest request, CancellationToken cancellationToken);
    Task<Result<HrParticipationBenefitRecordListResponse>> 참여혜택목록Async(string? userId, string? sourceType, CancellationToken cancellationToken);
    Task<Result<HrParticipationBenefitRecordResponse>> 참여혜택전환Async(HrParticipationBenefitTransferRequest request, CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.HrParticipation)]
[SsalddelUseCase("HR 참여 운영", Summary = "같이 주문 집단이나 운영 주체가 내부 참여자를 고용하고 혜택을 급여/수당 흐름으로 전환합니다.")]
[SsalddelUseCaseActor(SsalddelActor.EmployerOrOperatingEntity)]
[SsalddelUseCaseActor(SsalddelActor.Worker)]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator, SsalddelUseCaseActorRole.Supporting)]
[SsalddelUseCaseRelation(
    SsalddelUseCaseRelationKind.Extend,
    "사회보험신고UseCase",
    Condition = "근무 형태, 보수, 기간이 건강보험·국민연금·고용보험 신고 준비 대상에 해당하는 경우",
    Summary = "고용계약과 참여 인력 운영을 사회보험 신고 준비 흐름으로 확장합니다.")]
[SsalddelUseCaseRelation(
    SsalddelUseCaseRelationKind.Extend,
    "플랫폼수익환급UseCase",
    Condition = "참여 보상이나 플랫폼 수익 환급을 급여·수당·정산 일정으로 연결하는 경우",
    Summary = "참여 인력 보상을 플랫폼 수익 환급과 지급 일정 흐름으로 확장합니다.")]
public sealed class HR참여운영UseCase : IHR참여운영UseCase
{
    private readonly IHrEmploymentContractService _contractService;
    private readonly IHrParticipationBenefitRecordService _benefitRecordService;

    public HR참여운영UseCase(
        IHrEmploymentContractService contractService,
        IHrParticipationBenefitRecordService benefitRecordService)
    {
        _contractService = contractService;
        _benefitRecordService = benefitRecordService;
    }

    public async Task<Result<HrEmploymentContractListResponse>> 고용계약목록Async(
        string? workerUserId,
        string? employerScopeType,
        string? employerScopeId,
        CancellationToken cancellationToken)
    {
        var items = await _contractService.ListAsync(workerUserId, employerScopeType, employerScopeId, cancellationToken);
        return Result.Ok(new HrEmploymentContractListResponse { Items = items });
    }

    public async Task<Result<HrEmploymentContractResponse>> 고용계약상세Async(Guid contractId, CancellationToken cancellationToken)
    {
        var contract = await _contractService.GetAsync(contractId, cancellationToken);
        return contract is null
            ? Result.Fail<HrEmploymentContractResponse>(new Error("HR 고용계약을 찾을 수 없습니다.").WithMetadata("StatusCode", StatusCodes.Status404NotFound))
            : Result.Ok(contract);
    }

    public async Task<Result<HrEmploymentContractResponse>> 고용계약초안생성Async(
        HrEmploymentContractDraftRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok(await _contractService.CreateDraftAsync(request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<HrEmploymentContractResponse>(ex.Message);
        }
    }

    public async Task<Result<HrEmploymentContractResponse>> 고용계약서명Async(
        Guid contractId,
        HrEmploymentContractSignRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok(await _contractService.SignAsync(contractId, request.SignedByUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail<HrEmploymentContractResponse>(new Error(ex.Message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<HrEmploymentContractResponse>(ex.Message);
        }
    }

    public async Task<Result<HrPayrollScheduleListResponse>> 급여스케줄생성Async(
        Guid contractId,
        HrPayrollScheduleCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedules = await _contractService.CreatePayrollSchedulesAsync(
                contractId,
                request.ScheduleStartDate,
                request.ScheduleEndDate,
                cancellationToken);

            return Result.Ok(new HrPayrollScheduleListResponse { Items = schedules });
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail<HrPayrollScheduleListResponse>(new Error(ex.Message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<HrPayrollScheduleListResponse>(ex.Message);
        }
    }

    public async Task<Result<HrParticipationBenefitRecordListResponse>> 참여혜택목록Async(
        string? userId,
        string? sourceType,
        CancellationToken cancellationToken)
    {
        var items = await _benefitRecordService.ListAsync(userId, sourceType, cancellationToken);
        return Result.Ok(new HrParticipationBenefitRecordListResponse { Items = items });
    }

    public async Task<Result<HrParticipationBenefitRecordResponse>> 참여혜택전환Async(
        HrParticipationBenefitTransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok(await _benefitRecordService.TransferAsync(request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<HrParticipationBenefitRecordResponse>(ex.Message);
        }
    }
}
