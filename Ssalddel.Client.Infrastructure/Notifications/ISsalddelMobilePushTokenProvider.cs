namespace Ssalddel.Client.Infrastructure.Notifications;

public sealed record SsalddelMobilePushTokenSnapshot(
    string InstallationId,
    string AppKey,
    string Platform,
    string PushToken,
    string? AppVersion,
    string? DeviceModel);

public interface ISsalddelMobilePushTokenProvider
{
    Task<SsalddelMobilePushTokenSnapshot?> GetCurrentAsync(
        CancellationToken cancellationToken = default);
}

public sealed class NullSsalddelMobilePushTokenProvider : ISsalddelMobilePushTokenProvider
{
    public Task<SsalddelMobilePushTokenSnapshot?> GetCurrentAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<SsalddelMobilePushTokenSnapshot?>(null);
}
