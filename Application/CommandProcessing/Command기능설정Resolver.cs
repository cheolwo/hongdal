using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using 홍달.Data;
using 홍달.Services.Options;

namespace Hongdal.Application.CommandProcessing;

public sealed class Command기능설정Resolver : ICommand기능설정Resolver
{
    private readonly HongdalContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IOptionsMonitor<CommandProcessingOptions> _options;
    private readonly IMemoryCache _cache;

    public Command기능설정Resolver(
        HongdalContext db,
        ICurrentUserAccessor currentUserAccessor,
        IOptionsMonitor<CommandProcessingOptions> options,
        IMemoryCache cache)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
        _options = options;
        _cache = cache;
    }

    public async Task<CommandProcessingRule> ResolveAsync(string commandName, CancellationToken cancellationToken)
    {
        var rule = GetDefaultRule(commandName);
        var userId = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return rule;
        }

        var settings = await GetUserSettingsAsync(userId, commandName, cancellationToken);

        foreach (var setting in settings)
        {
            ApplyFeature(rule, setting.FeatureName, setting.IsEnabled);
        }

        return rule;
    }

    public void Invalidate(string userId, string commandName)
    {
        _cache.Remove(CacheKey(userId, commandName));
    }

    private async Task<IReadOnlyList<홍달.도메인.설정.사용자Command기능설정>> GetUserSettingsAsync(string userId, string commandName, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey(userId, commandName);
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<홍달.도메인.설정.사용자Command기능설정>? cached) && cached is not null)
        {
            return cached;
        }

        var settings = await _db.사용자Command기능설정
            .AsNoTracking()
            .Where(x => x.사용자Id == userId && x.CommandName == commandName)
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, settings, TimeSpan.FromMinutes(5));
        return settings;
    }

    private static string CacheKey(string userId, string commandName)
    {
        return $"command-feature-settings:{userId}:{commandName}";
    }

    public CommandProcessingRule GetDefaultRule(string commandName)
    {
        return _options.CurrentValue.GetRule(commandName);
    }

    public static bool GetFeatureEnabled(CommandProcessingRule rule, string featureName)
    {
        return featureName switch
        {
            Command기능명.AuditLog => rule.AuditLogEnabled.GetValueOrDefault(),
            Command기능명.Sms => rule.SmsEnabled.GetValueOrDefault(),
            Command기능명.Sns => rule.SnsEnabled.GetValueOrDefault(),
            Command기능명.Push => rule.PushEnabled.GetValueOrDefault(),
            _ => false
        };
    }

    public static void ApplyFeature(CommandProcessingRule rule, string featureName, bool isEnabled)
    {
        switch (featureName)
        {
            case Command기능명.AuditLog:
                rule.AuditLogEnabled = isEnabled;
                break;
            case Command기능명.Sms:
                rule.SmsEnabled = isEnabled;
                break;
            case Command기능명.Sns:
                rule.SnsEnabled = isEnabled;
                break;
            case Command기능명.Push:
                rule.PushEnabled = isEnabled;
                break;
        }
    }
}
