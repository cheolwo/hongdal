namespace Hongdal.Client.Infrastructure.Notifications;

public sealed record HongdalMobilePushTokenSnapshot(
    string InstallationId,
    string AppKey,
    string Platform,
    string PushToken,
    string? AppVersion,
    string? DeviceModel);

public interface IHongdalMobilePushTokenProvider
{
    Task<HongdalMobilePushTokenSnapshot?> GetCurrentAsync(
        CancellationToken cancellationToken = default);
}

public sealed class NullHongdalMobilePushTokenProvider : IHongdalMobilePushTokenProvider
{
    public Task<HongdalMobilePushTokenSnapshot?> GetCurrentAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<HongdalMobilePushTokenSnapshot?>(null);
}
