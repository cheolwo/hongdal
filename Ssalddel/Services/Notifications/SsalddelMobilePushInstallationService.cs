using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Notifications;
using Ssalddel.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;

namespace Ssalddel.Services.Notifications;

public interface ISsalddelMobilePushInstallationService
{
    Task<SsalddelMobilePushInstallationResponse> UpsertAsync(
        string userId,
        SsalddelMobilePushInstallationUpsertRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        string userId,
        string installationId,
        CancellationToken cancellationToken);
}

public sealed class SsalddelMobilePushInstallationService : ISsalddelMobilePushInstallationService
{
    private readonly SsalddelContext _db;

    public SsalddelMobilePushInstallationService(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<SsalddelMobilePushInstallationResponse> UpsertAsync(
        string userId,
        SsalddelMobilePushInstallationUpsertRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        var installationId = NormalizeRequired(request.InstallationId, 120, nameof(request.InstallationId));
        var appKey = NormalizeAppKey(request.AppKey);
        var platform = NormalizePlatform(request.Platform);
        // Data Protection expands the value before it is persisted in varchar(4096).
        var pushToken = NormalizeRequired(request.PushToken, 2000, nameof(request.PushToken));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pushToken))).ToLowerInvariant();
        var now = DateTime.UtcNow;

        var installation = await _db.SsalddelMobilePushInstallations
            .SingleOrDefaultAsync(
                x => x.AppKey == appKey && x.InstallationId == installationId,
                cancellationToken);
        if (installation is null)
        {
            installation = new SsalddelMobilePushInstallation
            {
                AppKey = appKey,
                InstallationId = installationId,
                CreatedAtUtc = now
            };
            _db.SsalddelMobilePushInstallations.Add(installation);
        }

        var duplicateTokens = await _db.SsalddelMobilePushInstallations
            .Where(x => x.PushTokenHash == tokenHash && x.Id != installation.Id && x.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var duplicate in duplicateTokens)
        {
            duplicate.IsActive = false;
            duplicate.UpdatedAtUtc = now;
        }

        installation.UserId = NormalizeRequired(userId, 450, nameof(userId));
        installation.Platform = platform;
        installation.PushToken = pushToken;
        installation.PushTokenHash = tokenHash;
        installation.AppVersion = NormalizeOptional(request.AppVersion, 40);
        installation.DeviceModel = NormalizeOptional(request.DeviceModel, 200);
        installation.IsActive = true;
        installation.LastSeenAtUtc = now;
        installation.UpdatedAtUtc = now;

        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(installation);
    }

    public async Task<bool> DeactivateAsync(
        string userId,
        string installationId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var normalizedInstallationId = NormalizeRequired(installationId, 120, nameof(installationId));
        var installation = await _db.SsalddelMobilePushInstallations
            .SingleOrDefaultAsync(
                x => x.UserId == userId && x.InstallationId == normalizedInstallationId,
                cancellationToken);
        if (installation is null)
        {
            return false;
        }

        installation.IsActive = false;
        installation.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static SsalddelMobilePushInstallationResponse ToResponse(SsalddelMobilePushInstallation installation)
        => new(
            installation.Id,
            installation.InstallationId,
            installation.AppKey,
            installation.Platform,
            installation.AppVersion,
            installation.DeviceModel,
            installation.IsActive,
            installation.LastSeenAtUtc);

    private static string NormalizePlatform(string value)
        => value.Trim().ToLowerInvariant() switch
        {
            "android" => SsalddelMobilePlatforms.Android,
            "ios" => SsalddelMobilePlatforms.Ios,
            _ => throw new ArgumentException("platform은 Android 또는 iOS여야 합니다.", nameof(value))
        };

    private static string NormalizeAppKey(string value)
    {
        var appKey = NormalizeRequired(value, 80, nameof(value));
        if (appKey.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '.' and not '-' and not '_'))
        {
            throw new ArgumentException("appKey 형식이 올바르지 않습니다.", nameof(value));
        }

        return appKey;
    }

    private static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"최대 {maxLength}자까지 입력할 수 있습니다.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
