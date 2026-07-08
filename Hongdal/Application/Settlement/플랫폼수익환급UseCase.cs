using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.PlatformProfit;
using 홍달.Services.Settlement;

namespace Hongdal.Application.Settlement;

public interface I플랫폼수익환급UseCase
{
    Task<Result<PlatformRevenueEntryResponse>> 수익기록Async(PlatformRevenueEntryRequest request, CancellationToken cancellationToken);
    Task<Result<PlatformProfitReturnPolicyResponse>> 정책생성Async(PlatformProfitReturnPolicyRequest request, CancellationToken cancellationToken);
    Task<Result<PlatformProfitReturnPlanResponse>> 스케줄생성Async(PlatformProfitReturnScheduleCreateRequest request, CancellationToken cancellationToken);
    Task<Result<PlatformProfitReturnScheduleListResponse>> 스케줄목록Async(string? participantUserId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.HrParticipation)]
[HongdalUseCase("플랫폼 수익 환급", Summary = "플랫폼 수익을 참여자에게 환급하거나 정산할 정책과 지급 일정을 관리합니다.")]
[HongdalUseCaseActor(HongdalActor.PlatformOperator)]
[HongdalUseCaseActor(HongdalActor.Worker, HongdalUseCaseActorRole.Supporting)]
public sealed class 플랫폼수익환급UseCase : I플랫폼수익환급UseCase
{
    private readonly IPlatformProfitReturnService _profitReturnService;

    public 플랫폼수익환급UseCase(IPlatformProfitReturnService profitReturnService)
    {
        _profitReturnService = profitReturnService;
    }

    public async Task<Result<PlatformRevenueEntryResponse>> 수익기록Async(
        PlatformRevenueEntryRequest request,
        CancellationToken cancellationToken)
    {
        return Result.Ok(await _profitReturnService.RecordRevenueAsync(request, cancellationToken));
    }

    public async Task<Result<PlatformProfitReturnPolicyResponse>> 정책생성Async(
        PlatformProfitReturnPolicyRequest request,
        CancellationToken cancellationToken)
    {
        return Result.Ok(await _profitReturnService.CreatePolicyAsync(request, cancellationToken));
    }

    public async Task<Result<PlatformProfitReturnPlanResponse>> 스케줄생성Async(
        PlatformProfitReturnScheduleCreateRequest request,
        CancellationToken cancellationToken)
    {
        return Result.Ok(await _profitReturnService.CreateReturnSchedulesAsync(request, cancellationToken));
    }

    public async Task<Result<PlatformProfitReturnScheduleListResponse>> 스케줄목록Async(
        string? participantUserId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        var items = await _profitReturnService.ListSchedulesAsync(participantUserId, from, to, cancellationToken);
        return Result.Ok(new PlatformProfitReturnScheduleListResponse { Items = items });
    }
}
