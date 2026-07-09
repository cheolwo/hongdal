namespace Microsoft.Maui.ApplicationModel;

public enum PermissionStatus
{
    Unknown,
    Denied,
    Granted
}

public sealed class PermissionException : Exception
{
    public PermissionException()
    {
    }

    public PermissionException(string message)
        : base(message)
    {
    }
}

public sealed class FeatureNotSupportedException : Exception
{
    public FeatureNotSupportedException()
    {
    }

    public FeatureNotSupportedException(string message)
        : base(message)
    {
    }
}

public static class Permissions
{
    public sealed class LocationWhenInUse
    {
    }

    public sealed class PostNotifications
    {
    }

    public static Task<PermissionStatus> CheckStatusAsync<TPermission>()
        => Task.FromResult(PermissionStatus.Granted);

    public static Task<PermissionStatus> RequestAsync<TPermission>()
        => Task.FromResult(PermissionStatus.Granted);
}
