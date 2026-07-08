using System.Text.Json;
using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.CommandSettings;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Application.CommandProcessing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.Audit;
using 홍달.도메인.설정;

namespace Hongdal.Application.ViewSettings;

public interface I보조기능설정UseCase
{
    Task<Result<AuxiliaryFeatureSettingsResponse>> 목록Async(
        string? userId,
        CancellationToken cancellationToken);

    Task<Result> 전역설정Async(
        string targetType,
        string targetName,
        string featureName,
        AuxiliaryFeatureSettingUpdateRequest? request,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken);

    Task<Result> 전역초기화Async(
        string targetType,
        string targetName,
        string featureName,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken);

    Task<Result> 사용자설정Async(
        string userId,
        string targetType,
        string targetName,
        string featureName,
        AuxiliaryFeatureSettingUpdateRequest? request,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken);

    Task<Result> 사용자초기화Async(
        string userId,
        string targetType,
        string targetName,
        string featureName,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken);
}

public sealed record 보조기능설정감사Context(
    string AdminUserId,
    string AdminUserName,
    string AdminRoleName,
    string Route,
    string TraceId,
    string ClientIp,
    string UserAgent);

[HongdalApiWorkflow(HongdalWorkflow.DomesticTransport)]
[HongdalUseCase("보조 기능 설정", Summary = "운영자가 Command와 화면 보조 기능의 전역/사용자별 활성 상태를 관리합니다.")]
[HongdalUseCaseActor(HongdalActor.PlatformOperator)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Include,
    "관리자View정책UseCase",
    Condition = "보조 기능이 특정 앱 화면의 운영 정책과 함께 적용되는 경우",
    Summary = "보조 기능 설정은 관리자 View 정책과 함께 화면·기능 노출 판단을 구성합니다.")]
public sealed class 보조기능설정UseCase : I보조기능설정UseCase
{
    private readonly HongdalContext _db;
    private readonly ICommand기능설정Resolver _resolver;
    private readonly ICommand기능CatalogResolver _catalogResolver;
    private readonly I사용자행위로그Service _activityLogService;

    public 보조기능설정UseCase(
        HongdalContext db,
        ICommand기능설정Resolver resolver,
        ICommand기능CatalogResolver catalogResolver,
        I사용자행위로그Service activityLogService)
    {
        _db = db;
        _resolver = resolver;
        _catalogResolver = catalogResolver;
        _activityLogService = activityLogService;
    }

    public async Task<Result<AuxiliaryFeatureSettingsResponse>> 목록Async(
        string? userId,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = NormalizeUserId(userId);
        var globalOverrides = await LoadOverridesAsync(Command기능설정Resolver.GlobalUserId, cancellationToken);
        var userOverrides = normalizedUserId is null
            ? []
            : await LoadOverridesAsync(normalizedUserId, cancellationToken);

        var features = _catalogResolver.GetFeatures();
        var items = new List<AuxiliaryFeatureSettingItem>();

        foreach (var command in _catalogResolver.GetDriverCommands())
        {
            var version = Command기능버전Catalog.Get(command.Version);
            var appRule = _resolver.GetDefaultRule(command.CommandName);
            var globalRule = await _resolver.ResolveGlobalRuleAsync(command.CommandName, cancellationToken);

            foreach (var policy in features)
            {
                var featureName = policy.FeatureName;
                var appDefault = Command기능설정Resolver.GetFeatureEnabled(appRule, featureName);
                var globalEnabled = policy.IsRequired
                    ? true
                    : Command기능설정Resolver.GetFeatureEnabled(globalRule, featureName);
                var globalOverride = FindOverride(globalOverrides, command.CommandName, featureName);
                var userOverride = normalizedUserId is null
                    ? null
                    : FindOverride(userOverrides, command.CommandName, featureName);
                var effectiveEnabled = policy.IsRequired
                    ? true
                    : userOverride?.IsEnabled ?? globalEnabled;

                items.Add(new AuxiliaryFeatureSettingItem
                {
                    TargetType = AuxiliaryFeatureTargetTypes.Command,
                    TargetName = command.CommandName,
                    TargetDisplayName = command.DisplayName,
                    Category = command.Category,
                    Version = version.Version,
                    VersionDisplayName = version.DisplayName,
                    VersionSortOrder = version.SortOrder,
                    IsCurrentRelease = version.IsCurrentRelease,
                    FeatureName = featureName,
                    FeatureDisplayName = _catalogResolver.GetFeatureDisplayName(featureName),
                    AppDefaultEnabled = appDefault,
                    GlobalEnabled = globalEnabled,
                    HasGlobalOverride = globalOverride is not null,
                    UserEnabled = userOverride?.IsEnabled,
                    HasUserOverride = userOverride is not null,
                    EffectiveEnabled = effectiveEnabled,
                    IsUserConfigurable = policy.IsUserConfigurable,
                    IsRequired = policy.IsRequired
                });
            }
        }

        return Result.Ok(new AuxiliaryFeatureSettingsResponse { Items = items });
    }

    public async Task<Result> 전역설정Async(
        string targetType,
        string targetName,
        string featureName,
        AuxiliaryFeatureSettingUpdateRequest? request,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request body is required");
        }

        var validation = ValidateTarget(targetType, targetName, featureName);
        if (validation.IsFailed)
        {
            return validation;
        }

        var mutability = ValidateMutableFeature(featureName);
        if (mutability.IsFailed)
        {
            return mutability;
        }

        await UpsertAsync(Command기능설정Resolver.GlobalUserId, ToStorageTargetName(targetType, targetName), featureName, request.IsEnabled, cancellationToken);
        await LogAsync("GlobalFeatureChanged", targetType, targetName, featureName, request.IsEnabled, auditContext, cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> 전역초기화Async(
        string targetType,
        string targetName,
        string featureName,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken)
    {
        var validation = ValidateTarget(targetType, targetName, featureName);
        if (validation.IsFailed)
        {
            return validation;
        }

        await DeleteAsync(Command기능설정Resolver.GlobalUserId, ToStorageTargetName(targetType, targetName), featureName, cancellationToken);
        await LogAsync("GlobalFeatureReset", targetType, targetName, featureName, null, auditContext, cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> 사용자설정Async(
        string userId,
        string targetType,
        string targetName,
        string featureName,
        AuxiliaryFeatureSettingUpdateRequest? request,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request body is required");
        }

        var normalizedUserId = NormalizeUserId(userId);
        if (normalizedUserId is null)
        {
            return BadRequest("userId is required.");
        }

        var validation = ValidateTarget(targetType, targetName, featureName);
        if (validation.IsFailed)
        {
            return validation;
        }

        var mutability = ValidateMutableFeature(featureName);
        if (mutability.IsFailed)
        {
            return mutability;
        }

        await UpsertAsync(normalizedUserId, ToStorageTargetName(targetType, targetName), featureName, request.IsEnabled, cancellationToken);
        await LogAsync("UserFeatureChanged", targetType, targetName, featureName, request.IsEnabled, auditContext, cancellationToken, normalizedUserId);
        return Result.Ok();
    }

    public async Task<Result> 사용자초기화Async(
        string userId,
        string targetType,
        string targetName,
        string featureName,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = NormalizeUserId(userId);
        if (normalizedUserId is null)
        {
            return BadRequest("userId is required.");
        }

        var validation = ValidateTarget(targetType, targetName, featureName);
        if (validation.IsFailed)
        {
            return validation;
        }

        await DeleteAsync(normalizedUserId, ToStorageTargetName(targetType, targetName), featureName, cancellationToken);
        await LogAsync("UserFeatureReset", targetType, targetName, featureName, null, auditContext, cancellationToken, normalizedUserId);
        return Result.Ok();
    }

    private async Task<IReadOnlyList<사용자Command기능설정>> LoadOverridesAsync(string userId, CancellationToken cancellationToken)
    {
        return await _db.사용자Command기능설정
            .AsNoTracking()
            .Where(x => x.사용자Id == userId)
            .ToArrayAsync(cancellationToken);
    }

    private async Task UpsertAsync(string userId, string storageTargetName, string featureName, bool isEnabled, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entity = await _db.사용자Command기능설정.FirstOrDefaultAsync(x =>
            x.사용자Id == userId
            && x.CommandName == storageTargetName
            && x.FeatureName == featureName,
            cancellationToken);

        if (entity is null)
        {
            entity = new 사용자Command기능설정
            {
                사용자Id = userId,
                CommandName = storageTargetName,
                FeatureName = featureName,
                IsEnabled = isEnabled,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.사용자Command기능설정.Add(entity);
        }
        else
        {
            entity.IsEnabled = isEnabled;
            entity.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _resolver.Invalidate(userId, storageTargetName);
    }

    private async Task DeleteAsync(string userId, string storageTargetName, string featureName, CancellationToken cancellationToken)
    {
        var entity = await _db.사용자Command기능설정.FirstOrDefaultAsync(x =>
            x.사용자Id == userId
            && x.CommandName == storageTargetName
            && x.FeatureName == featureName,
            cancellationToken);

        if (entity is not null)
        {
            _db.사용자Command기능설정.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        _resolver.Invalidate(userId, storageTargetName);
    }

    private Result ValidateTarget(string targetType, string targetName, string featureName)
    {
        if (string.IsNullOrWhiteSpace(targetName) || string.IsNullOrWhiteSpace(featureName))
        {
            return BadRequest("targetName and featureName are required.");
        }

        if (!string.Equals(targetType, AuxiliaryFeatureTargetTypes.Command, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only Command targets are catalog-backed in the current admin screen.");
        }

        if (!_catalogResolver.IsSupportedDriverCommand(targetName))
        {
            return BadRequest("Unsupported command target.");
        }

        if (!_catalogResolver.IsSupportedFeature(featureName))
        {
            return BadRequest("Unsupported feature.");
        }

        return Result.Ok();
    }

    private Result ValidateMutableFeature(string featureName)
    {
        var policy = _catalogResolver.GetFeatures().FirstOrDefault(x => string.Equals(x.FeatureName, featureName, StringComparison.Ordinal));
        if (policy is null)
        {
            return BadRequest("Unsupported feature.");
        }

        if (policy.IsRequired)
        {
            return Conflict("Required workflow features cannot be disabled or overridden.");
        }

        return Result.Ok();
    }

    private static 사용자Command기능설정? FindOverride(IEnumerable<사용자Command기능설정> overrides, string commandName, string featureName)
    {
        return overrides.FirstOrDefault(x =>
            string.Equals(x.CommandName, commandName, StringComparison.Ordinal)
            && string.Equals(x.FeatureName, featureName, StringComparison.Ordinal));
    }

    private static string ToStorageTargetName(string targetType, string targetName)
    {
        return string.Equals(targetType, AuxiliaryFeatureTargetTypes.Command, StringComparison.OrdinalIgnoreCase)
            ? targetName.Trim()
            : $"{targetType.Trim()}:{targetName.Trim()}";
    }

    private static string? NormalizeUserId(string? userId)
    {
        return string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
    }

    private Task LogAsync(
        string actionName,
        string targetType,
        string targetName,
        string featureName,
        bool? enabled,
        보조기능설정감사Context auditContext,
        CancellationToken cancellationToken,
        string? targetUserId = null)
    {
        return _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = App식별자.HongdalAdmin,
            UserId = auditContext.AdminUserId,
            UserName = auditContext.AdminUserName,
            RoleName = auditContext.AdminRoleName,
            ActionType = "AuxiliaryFeatureSetting",
            ActionName = actionName,
            Route = auditContext.Route,
            TraceId = auditContext.TraceId,
            IsSuccess = true,
            ClientIp = auditContext.ClientIp,
            UserAgent = auditContext.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = JsonSerializer.Serialize(new
            {
                targetType,
                targetName,
                featureName,
                enabled,
                targetUserId
            })
        }, cancellationToken);
    }

    private static Result BadRequest(string message) => Result.Fail(message);

    private static Result Conflict(string message)
        => Result.Fail(new Error(message).WithMetadata("StatusCode", StatusCodes.Status409Conflict));
}
