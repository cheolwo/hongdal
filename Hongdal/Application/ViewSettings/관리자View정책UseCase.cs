using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.ViewSettings;
using Microsoft.AspNetCore.Http;
using 홍달.Services.Audit;
using 홍달.Services.ViewSettings;

namespace Hongdal.Application.ViewSettings;

public interface I관리자View정책UseCase
{
    Task<Result<관리자View정책목록응답>> 조회Async(string? appKey, CancellationToken cancellationToken);
    Task<Result<관리자View정책항목응답>> 수정Async(long id, 관리자View정책수정요청? request, 관리자View정책Context context, CancellationToken cancellationToken);
}

public sealed record 관리자View정책Context(
    string UserId,
    string UserName,
    string RoleName,
    string Route,
    string TraceId,
    string ClientIp,
    string UserAgent);

[HongdalApiWorkflow(HongdalWorkflow.DomesticTransport)]
[HongdalUseCase("관리자 View 정책", Summary = "운영자가 앱 화면 노출 정책을 조회하고 변경 이력을 남깁니다.")]
[HongdalUseCaseActor(HongdalActor.PlatformOperator)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "View설정UseCase",
    Condition = "관리자 정책이 사용자별 화면 노출 결과로 반영되는 경우",
    Summary = "관리자 View 정책을 사용자별 View 설정 조회와 저장 흐름으로 확장합니다.")]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "보조기능설정UseCase",
    Condition = "화면 노출뿐 아니라 화면 안 기능 단위까지 운영자가 제어하는 경우",
    Summary = "View 정책을 보조 기능과 Command 기능 설정 흐름으로 확장합니다.")]
public sealed class 관리자View정책UseCase : I관리자View정책UseCase
{
    private readonly IView가시성Service _viewVisibilityService;
    private readonly I사용자행위로그Service _activityLogService;

    public 관리자View정책UseCase(IView가시성Service viewVisibilityService, I사용자행위로그Service activityLogService)
    {
        _viewVisibilityService = viewVisibilityService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<관리자View정책목록응답>> 조회Async(string? appKey, CancellationToken cancellationToken)
    {
        var items = await _viewVisibilityService.GetPoliciesAsync(appKey, cancellationToken);
        return Result.Ok(new 관리자View정책목록응답
        {
            Items = items
        });
    }

    public async Task<Result<관리자View정책항목응답>> 수정Async(
        long id,
        관리자View정책수정요청? request,
        관리자View정책Context context,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result.Fail<관리자View정책항목응답>("request body is required");
        }

        try
        {
            var updated = await _viewVisibilityService.UpdatePolicyAsync(id, request.PolicyEnabled, cancellationToken);
            if (updated is null)
            {
                return Result.Fail<관리자View정책항목응답>(new Error("View 정책을 찾을 수 없습니다.").WithMetadata("StatusCode", StatusCodes.Status404NotFound));
            }

            await _activityLogService.기록Async(new 사용자행위로그기록
            {
                AppKey = App식별자.HongdalAdmin,
                UserId = context.UserId,
                UserName = context.UserName,
                RoleName = context.RoleName,
                ActionType = "ViewPolicy",
                ActionName = "PolicyChanged",
                Route = context.Route,
                TraceId = context.TraceId,
                IsSuccess = true,
                ClientIp = context.ClientIp,
                UserAgent = context.UserAgent,
                OccurredAtUtc = DateTime.UtcNow,
                MetadataJson = $"{{\"policyId\":{id},\"enabled\":{request.PolicyEnabled.ToString().ToLowerInvariant()}}}"
            }, cancellationToken);

            return Result.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail<관리자View정책항목응답>(ex.Message);
        }
    }
}
