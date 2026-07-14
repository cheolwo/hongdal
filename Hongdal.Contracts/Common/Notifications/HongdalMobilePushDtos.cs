namespace Hongdal.Contracts.Common.Notifications;

public sealed record HongdalMobilePushInstallationUpsertRequest(
    string InstallationId,
    string AppKey,
    string Platform,
    string PushToken,
    string? AppVersion = null,
    string? DeviceModel = null);

public sealed record HongdalMobilePushInstallationResponse(
    long Id,
    string InstallationId,
    string AppKey,
    string Platform,
    string? AppVersion,
    string? DeviceModel,
    bool IsActive,
    DateTime LastSeenAtUtc);
