using System.Security.Claims;
using Hongdal.Application.CommandProcessing;
using Hongdal.Controllers;
using Hongdal.Contracts.Common.ViewSettings;
using Hongdal.Contracts.CommandSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.설정;
using 홍달.Services.Audit;

namespace Hongdal.Controllers.Admin;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/auxiliary-feature-settings")]
public sealed class AuxiliaryFeatureSettingsController : ControllerBase
{
    private readonly HongdalContext _db;
    private readonly ICommand기능설정Resolver _resolver;
    private readonly ICommand기능CatalogResolver _catalogResolver;
    private readonly I사용자행위로그Service _activityLogService;

    public AuxiliaryFeatureSettingsController(
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

    [HttpGet]
    public async Task<ActionResult<AuxiliaryFeatureSettingsResponse>> List(
        [FromQuery] string? userId,
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

        return Ok(new AuxiliaryFeatureSettingsResponse { Items = items });
    }

    [HttpPut("global/{targetType}/{targetName}/{featureName}")]
    public async Task<IActionResult> UpdateGlobal(
        string targetType,
        string targetName,
        string featureName,
        [FromBody] AuxiliaryFeatureSettingUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateTarget(targetType, targetName, featureName);
        if (validation is not null)
        {
            return validation;
        }

        var mutability = ValidateMutableFeature(featureName);
        if (mutability is not null)
        {
            return mutability;
        }

        await UpsertAsync(Command기능설정Resolver.GlobalUserId, ToStorageTargetName(targetType, targetName), featureName, request.IsEnabled, cancellationToken);
        await LogAsync("GlobalFeatureChanged", targetType, targetName, featureName, request.IsEnabled, cancellationToken);
        return NoContent();
    }

    [HttpDelete("global/{targetType}/{targetName}/{featureName}")]
    public async Task<IActionResult> ResetGlobal(
        string targetType,
        string targetName,
        string featureName,
        CancellationToken cancellationToken)
    {
        var validation = ValidateTarget(targetType, targetName, featureName);
        if (validation is not null)
        {
            return validation;
        }

        await DeleteAsync(Command기능설정Resolver.GlobalUserId, ToStorageTargetName(targetType, targetName), featureName, cancellationToken);
        await LogAsync("GlobalFeatureReset", targetType, targetName, featureName, null, cancellationToken);
        return NoContent();
    }

    [HttpPut("users/{userId}/{targetType}/{targetName}/{featureName}")]
    public async Task<IActionResult> UpdateUser(
        string userId,
        string targetType,
        string targetName,
        string featureName,
        [FromBody] AuxiliaryFeatureSettingUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = NormalizeUserId(userId);
        if (normalizedUserId is null)
        {
            return this.ToProblemActionResult("userId is required.");
        }

        var validation = ValidateTarget(targetType, targetName, featureName);
        if (validation is not null)
        {
            return validation;
        }

        var mutability = ValidateMutableFeature(featureName);
        if (mutability is not null)
        {
            return mutability;
        }

        await UpsertAsync(normalizedUserId, ToStorageTargetName(targetType, targetName), featureName, request.IsEnabled, cancellationToken);
        await LogAsync("UserFeatureChanged", targetType, targetName, featureName, request.IsEnabled, cancellationToken, normalizedUserId);
        return NoContent();
    }

    [HttpDelete("users/{userId}/{targetType}/{targetName}/{featureName}")]
    public async Task<IActionResult> ResetUser(
        string userId,
        string targetType,
        string targetName,
        string featureName,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = NormalizeUserId(userId);
        if (normalizedUserId is null)
        {
            return this.ToProblemActionResult("userId is required.");
        }

        var validation = ValidateTarget(targetType, targetName, featureName);
        if (validation is not null)
        {
            return validation;
        }

        await DeleteAsync(normalizedUserId, ToStorageTargetName(targetType, targetName), featureName, cancellationToken);
        await LogAsync("UserFeatureReset", targetType, targetName, featureName, null, cancellationToken, normalizedUserId);
        return NoContent();
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

    private IActionResult? ValidateTarget(string targetType, string targetName, string featureName)
    {
        if (string.IsNullOrWhiteSpace(targetName) || string.IsNullOrWhiteSpace(featureName))
        {
            return this.ToProblemActionResult("targetName and featureName are required.");
        }

        if (!string.Equals(targetType, AuxiliaryFeatureTargetTypes.Command, StringComparison.OrdinalIgnoreCase))
        {
            return this.ToProblemActionResult("Only Command targets are catalog-backed in the current admin screen.");
        }

        if (!_catalogResolver.IsSupportedDriverCommand(targetName))
        {
            return this.ToProblemActionResult("Unsupported command target.");
        }

        if (!_catalogResolver.IsSupportedFeature(featureName))
        {
            return this.ToProblemActionResult("Unsupported feature.");
        }

        return null;
    }

    private IActionResult? ValidateMutableFeature(string featureName)
    {
        var policy = _catalogResolver.GetFeatures().FirstOrDefault(x => string.Equals(x.FeatureName, featureName, StringComparison.Ordinal));
        if (policy is null)
        {
            return this.ToProblemActionResult("Unsupported feature.");
        }

        if (policy.IsRequired)
        {
            return this.ToConflictProblem("Required workflow features cannot be disabled or overridden.");
        }

        return null;
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

    private async Task LogAsync(
        string actionName,
        string targetType,
        string targetName,
        string featureName,
        bool? enabled,
        CancellationToken cancellationToken,
        string? targetUserId = null)
    {
        await _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = App식별자.HongdalAdmin,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            UserName = User.Identity?.Name ?? string.Empty,
            RoleName = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            ActionType = "AuxiliaryFeatureSetting",
            ActionName = actionName,
            Route = Request.Path.Value ?? string.Empty,
            TraceId = HttpContext.TraceIdentifier,
            IsSuccess = true,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString(),
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                targetType,
                targetName,
                featureName,
                enabled,
                targetUserId
            })
        }, cancellationToken);
    }
}
