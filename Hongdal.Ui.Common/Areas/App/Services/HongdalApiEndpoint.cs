namespace Hongdal.Ui.Common.Areas.App.Services;

public static class HongdalApiEndpoint
{
    public const string ConfigurationKey = "HongdalApiBaseAddress";
    public const string LocalDevelopmentBaseAddress = "https://localhost:7117/";
    public const string AndroidEmulatorDebugBaseAddress = "http://10.0.2.2:5104/";

    public static Uri CreateDefaultBaseAddress()
    {
#if DEBUG
        if (OperatingSystem.IsAndroid())
        {
            return new Uri(AndroidEmulatorDebugBaseAddress);
        }
#endif

        return new Uri(LocalDevelopmentBaseAddress);
    }

    public static Uri ResolveBaseAddress(string? configuredBaseAddress, Uri? fallback = null)
    {
        if (string.IsNullOrWhiteSpace(configuredBaseAddress))
        {
            return NormalizeBaseAddress(fallback ?? CreateDefaultBaseAddress());
        }

        if (!Uri.TryCreate(configuredBaseAddress.Trim(), UriKind.Absolute, out var configuredUri))
        {
            throw new ArgumentException(
                "The Hongdal API base address must be an absolute HTTP(S) URI.",
                nameof(configuredBaseAddress));
        }

        return NormalizeBaseAddress(configuredUri);
    }

    public static Uri NormalizeBaseAddress(Uri baseAddress)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);

        var isHttp = string.Equals(baseAddress.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || string.Equals(baseAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

        if (!baseAddress.IsAbsoluteUri
            || !isHttp
            || !string.IsNullOrEmpty(baseAddress.Query)
            || !string.IsNullOrEmpty(baseAddress.Fragment))
        {
            throw new ArgumentException(
                "The Hongdal API base address must be an absolute HTTP(S) URI without a query or fragment.",
                nameof(baseAddress));
        }

        var builder = new UriBuilder(baseAddress);
        if (!builder.Path.EndsWith("/", StringComparison.Ordinal))
        {
            builder.Path += "/";
        }

        return builder.Uri;
    }
}
