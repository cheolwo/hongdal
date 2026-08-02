namespace Ssalddel.Contracts.Common.Platform;

public static class GoogleMapsBrowserRuntimeRoutes
{
    public const string LocalDevelopment = "api/v1/platform/runtime/google-maps";
}

public sealed class GoogleMapsBrowserRuntimeResponse
{
    public string BrowserApiKey { get; init; } = string.Empty;

    public IReadOnlyList<string> AllowedOrigins { get; init; } = [];
}
