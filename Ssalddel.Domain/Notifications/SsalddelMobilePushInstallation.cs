namespace Ssalddel.Domain.Notifications;

public static class SsalddelMobilePlatforms
{
    public const string Android = "Android";
    public const string Ios = "iOS";

    public static bool IsSupported(string? value)
        => value is Android or Ios;
}

public sealed class SsalddelMobilePushInstallation
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string InstallationId { get; set; } = string.Empty;

    public string AppKey { get; set; } = "Ssalddel";

    public string Platform { get; set; } = string.Empty;

    public string PushToken { get; set; } = string.Empty;

    public string PushTokenHash { get; set; } = string.Empty;

    public string? AppVersion { get; set; }

    public string? DeviceModel { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
