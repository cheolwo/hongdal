namespace Ssalddel.Contracts.Common.Notifications;

public sealed record SsalddelMobilePushInstallationUpsertRequest(
    string InstallationId,
    string AppKey,
    string Platform,
    string PushToken,
    string? AppVersion = null,
    string? DeviceModel = null);

public sealed record SsalddelMobilePushInstallationResponse(
    long Id,
    string InstallationId,
    string AppKey,
    string Platform,
    string? AppVersion,
    string? DeviceModel,
    bool IsActive,
    DateTime LastSeenAtUtc);
