using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.ViewSettings;
using 홍달.Data;
using 홍달.Services.Audit;
using 홍달.Services.ViewSettings;

namespace Hongdal.Application.ViewSettings;

public interface IView설정UseCase
{
    Task<Result<View가시성목록응답>> 조회Async(string? appKey, View설정요청Context context, CancellationToken cancellationToken);
    Task<Result> 저장Async(사용자View가시성수정요청? request, View설정요청Context context, CancellationToken cancellationToken);
}

[HongdalUseCaseActor(HongdalActor.Driver)]
[HongdalUseCaseActor(HongdalActor.Shipper)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
public sealed class View설정UseCase : IView설정UseCase
{
    private readonly IView가시성Service _viewVisibilityService;
    private readonly I사용자행위로그Service _activityLogService;

    public View설정UseCase(
        IView가시성Service viewVisibilityService,
        I사용자행위로그Service activityLogService)
    {
        _viewVisibilityService = viewVisibilityService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<View가시성목록응답>> 조회Async(
        string? appKey,
        View설정요청Context context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            return Result.Fail<View가시성목록응답>("appKey is required");
        }

        var normalizedAppKey = appKey.Trim();
        var roleResult = 역할명해결(normalizedAppKey, context.RoleName);
        if (roleResult.IsFailed)
        {
            return Result.Fail<View가시성목록응답>(roleResult.Errors);
        }

        var items = await _viewVisibilityService.GetEffectiveViewsAsync(
            normalizedAppKey,
            roleResult.Value,
            context.UserId,
            cancellationToken);

        return new View가시성목록응답
        {
            Items = items
        };
    }

    public async Task<Result> 저장Async(
        사용자View가시성수정요청? request,
        View설정요청Context context,
        CancellationToken cancellationToken)
    {
        if (request is null) return Result.Fail("request body is required");
        if (string.IsNullOrWhiteSpace(request.AppKey)) return Result.Fail("appKey is required");
        if (string.IsNullOrWhiteSpace(request.ViewKey)) return Result.Fail("viewKey is required");
        if (string.IsNullOrWhiteSpace(context.UserId)) return Result.Fail("userId could not be resolved");

        var appKey = request.AppKey.Trim();
        var roleResult = 역할명해결(appKey, context.RoleName);
        if (roleResult.IsFailed)
        {
            return Result.Fail(roleResult.Errors);
        }

        try
        {
            await _viewVisibilityService.SetUserVisibilityAsync(
                appKey,
                roleResult.Value,
                context.UserId,
                request.ViewKey.Trim(),
                request.IsVisible,
                cancellationToken);

            await _activityLogService.기록Async(new 사용자행위로그기록
            {
                AppKey = appKey,
                UserId = context.UserId,
                RoleName = roleResult.Value,
                ActionType = "ViewSettings",
                ActionName = "UserVisibilityChanged",
                Route = context.Route,
                TraceId = context.TraceId,
                IsSuccess = true,
                ClientIp = context.ClientIp,
                UserAgent = context.UserAgent,
                OccurredAtUtc = DateTime.UtcNow,
                MetadataJson = $"{{\"viewKey\":\"{request.ViewKey.Trim()}\",\"isVisible\":{request.IsVisible.ToString().ToLowerInvariant()}}}"
            }, cancellationToken);

            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private static Result<string> 역할명해결(string appKey, string? requestedRoleName)
    {
        if (!string.IsNullOrWhiteSpace(requestedRoleName))
        {
            return requestedRoleName.Trim();
        }

        return appKey switch
        {
            App식별자.DriverApp => 역할명.기사,
            App식별자.ShipperApp => 역할명.화주,
            App식별자.HongdalAdmin => 역할명.서버관리자,
            _ => Result.Fail<string>("지원하지 않는 appKey 입니다.")
        };
    }
}

public sealed record View설정요청Context(
    string? UserId,
    string? RoleName,
    string Route,
    string TraceId,
    string ClientIp,
    string UserAgent);
