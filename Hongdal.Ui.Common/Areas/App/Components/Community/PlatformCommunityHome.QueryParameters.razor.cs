namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private static IReadOnlyDictionary<string, string> ParseCommunityQueryParameters(string uri)
    {
        var queryStart = uri.IndexOf('?', StringComparison.Ordinal);
        if (queryStart < 0 || queryStart == uri.Length - 1)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var query = uri[(queryStart + 1)..];
        var fragmentStart = query.IndexOf('#', StringComparison.Ordinal);
        if (fragmentStart >= 0)
        {
            query = query[..fragmentStart];
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=', StringComparison.Ordinal);
            var key = separator >= 0 ? pair[..separator] : pair;
            var value = separator >= 0 ? pair[(separator + 1)..] : string.Empty;

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[DecodeCommunityQueryValue(key)] = DecodeCommunityQueryValue(value);
        }

        return result;
    }

    private static string? GetCommunityQueryValue(IReadOnlyDictionary<string, string> query, string key)
        => query.TryGetValue(key, out var value) ? value : null;

    private static string DecodeCommunityQueryValue(string value)
        => Uri.UnescapeDataString(value.Replace("+", " ", StringComparison.Ordinal));
}
