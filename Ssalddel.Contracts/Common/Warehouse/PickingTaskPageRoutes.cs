using System.Globalization;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Contracts.Common.Warehouse;

public enum PickingTaskScreenKind
{
    List,
    Detail,
    Execute
}

/// <summary>Web과 창고 앱이 공유하는 피킹 작업 List·Detail·Action route입니다.</summary>
public static class PickingTaskPageRoutes
{
    public const string Root = "/work/picking-batch";
    public const string DetailTemplate = $"{Root}/{{TaskKey}}";
    public const string ExecuteTemplate = $"{Root}/{{TaskKey}}/execute";

    // 통합 WebApp의 기존 링크를 위한 호환 alias입니다.
    public const string LegacyWebRoot = "/warehouse/work/picking-batch";
    public const string LegacyWebDetailTemplate = $"{LegacyWebRoot}/{{TaskKey}}";
    public const string LegacyWebExecuteTemplate = $"{LegacyWebRoot}/{{TaskKey}}/execute";

    public static string DetailFor(string taskKey)
        => $"{Root}/{Uri.EscapeDataString(RequireTaskKey(taskKey))}";

    public static string ExecuteFor(string taskKey)
        => $"{DetailFor(taskKey)}/execute";

    public static string NormalizeTaskKey(string? taskKey)
        => RequireTaskKey(taskKey ?? string.Empty);

    private static string RequireTaskKey(string taskKey)
    {
        var normalized = taskKey.Trim();
        return normalized.Length is > 0 and <= 120
            ? normalized
            : throw new ArgumentException("피킹 작업 Key는 1~120자여야 합니다.", nameof(taskKey));
    }
}

/// <summary>피킹 목록 조건과 안전한 복귀 위치를 세 화면 사이에 보존합니다.</summary>
public sealed record PickingTaskNavigationContext
{
    public string? From { get; init; }
    public string? Search { get; init; }
    public string Status { get; init; } = 피킹작업조회상태코드.대기;
    public int Page { get; init; }

    public string ResolveReturnPath(string fallback)
        => PageNavigationContext.ResolveReturnPath(From, fallback);

    public PickingTaskNavigationContext WithListState(string? search, string? status, int page)
        => this with
        {
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            Status = 피킹작업조회상태코드.Normalize(status),
            Page = Math.Max(0, page)
        };

    public string PathFor(PickingTaskScreenKind screen, string? taskKey = null)
    {
        var path = screen switch
        {
            PickingTaskScreenKind.List => PickingTaskPageRoutes.Root,
            PickingTaskScreenKind.Detail when !string.IsNullOrWhiteSpace(taskKey)
                => PickingTaskPageRoutes.DetailFor(taskKey),
            PickingTaskScreenKind.Execute when !string.IsNullOrWhiteSpace(taskKey)
                => PickingTaskPageRoutes.ExecuteFor(taskKey),
            _ => throw new ArgumentException("이 화면에는 유효한 피킹 작업 Key가 필요합니다.", nameof(taskKey))
        };

        var values = new List<string>();
        Add(values, "from", PageNavigationContext.NormalizeReturnPath(From));
        Add(values, "search", Search);
        var status = 피킹작업조회상태코드.Normalize(Status);
        if (status != 피킹작업조회상태코드.대기)
        {
            Add(values, "status", status);
        }
        if (Page > 0)
        {
            Add(values, "page", Page.ToString(CultureInfo.InvariantCulture));
        }

        return values.Count == 0 ? path : $"{path}?{string.Join('&', values)}";
    }

    public static PickingTaskNavigationContext Parse(string? uri)
    {
        var query = ParseQuery(uri);
        return new PickingTaskNavigationContext
        {
            From = PageNavigationContext.NormalizeReturnPath(Get(query, "from")),
            Search = Get(query, "search"),
            Status = 피킹작업조회상태코드.Normalize(Get(query, "status")),
            Page = ParsePage(query)
        };
    }

    private static int ParsePage(IReadOnlyDictionary<string, string> query)
        => int.TryParse(Get(query, "page"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? Math.Max(0, value)
            : 0;

    private static void Add(ICollection<string> values, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private static IReadOnlyDictionary<string, string> ParseQuery(string? uri)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(uri))
        {
            return result;
        }

        var queryStart = uri.IndexOf('?', StringComparison.Ordinal);
        if (queryStart < 0 || queryStart == uri.Length - 1)
        {
            return result;
        }

        var query = uri[(queryStart + 1)..];
        var fragmentStart = query.IndexOf('#', StringComparison.Ordinal);
        if (fragmentStart >= 0)
        {
            query = query[..fragmentStart];
        }

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=', StringComparison.Ordinal);
            var key = separator >= 0 ? pair[..separator] : pair;
            var value = separator >= 0 ? pair[(separator + 1)..] : string.Empty;
            if (!string.IsNullOrWhiteSpace(key))
            {
                result[Decode(key)] = Decode(value);
            }
        }

        return result;
    }

    private static string Decode(string value)
    {
        try
        {
            return Uri.UnescapeDataString(value.Replace("+", " ", StringComparison.Ordinal));
        }
        catch (UriFormatException)
        {
            return value;
        }
    }

    private static string? Get(IReadOnlyDictionary<string, string> query, string key)
        => query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;
}
