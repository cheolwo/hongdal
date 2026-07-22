using System.Globalization;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Contracts.Common.Mart;

public enum MartProductScreenKind
{
    List,
    Detail,
    Review,
    Order
}

/// <summary>Web과 주문자 앱이 공유하는 마트 공개 상품 List·Detail·Action route입니다.</summary>
public static class MartProductPageRoutes
{
    public const string Root = "/food/mart";
    public const string DetailRoot = $"{Root}/products";
    public const string ReviewRoot = $"{Root}/reviews";
    public const string OrderRoot = $"{Root}/order";
    public const string DetailTemplate = $"{DetailRoot}/{{ProductId:long}}";
    public const string ReviewTemplate = $"{ReviewRoot}/{{ProductId:long}}";
    public const string OrderTemplate = $"{OrderRoot}/{{ProductId:long}}";

    // 통합 WebApp의 기존 /orderer/mart 링크를 위한 호환 alias입니다.
    public const string LegacyWebRoot = "/orderer/mart";
    public const string LegacyWebDetailRoot = $"{LegacyWebRoot}/products";
    public const string LegacyWebReviewRoot = $"{LegacyWebRoot}/reviews";
    public const string LegacyWebOrderRoot = $"{LegacyWebRoot}/order";
    public const string LegacyWebDetailTemplate = $"{LegacyWebDetailRoot}/{{ProductId:long}}";
    public const string LegacyWebReviewTemplate = $"{LegacyWebReviewRoot}/{{ProductId:long}}";
    public const string LegacyWebOrderTemplate = $"{LegacyWebOrderRoot}/{{ProductId:long}}";

    public static string DetailFor(long productId) => $"{DetailRoot}/{RequireProductId(productId)}";

    public static string ReviewFor(long productId) => $"{ReviewRoot}/{RequireProductId(productId)}";

    public static string OrderFor(long productId) => $"{OrderRoot}/{RequireProductId(productId)}";

    private static long RequireProductId(long productId)
        => productId > 0
            ? productId
            : throw new ArgumentOutOfRangeException(nameof(productId), "마트 상품 ID는 1 이상이어야 합니다.");
}

/// <summary>마트 목록 조건과 안전한 복귀 위치를 상세·후기·주문 요청 사이에 보존합니다.</summary>
public sealed record MartProductNavigationContext
{
    public string? From { get; init; }
    public string? Search { get; init; }
    public bool AvailableOnly { get; init; }
    public int Page { get; init; } = 1;

    public string ResolveReturnPath(string fallback)
        => PageNavigationContext.ResolveReturnPath(From, fallback);

    public MartProductNavigationContext WithListState(string? search, bool availableOnly, int page)
        => this with
        {
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            AvailableOnly = availableOnly,
            Page = Math.Max(1, page)
        };

    public string PathFor(MartProductScreenKind screen, long? productId = null)
    {
        var path = screen switch
        {
            MartProductScreenKind.List => MartProductPageRoutes.Root,
            MartProductScreenKind.Detail when productId is > 0 => MartProductPageRoutes.DetailFor(productId.Value),
            MartProductScreenKind.Review when productId is > 0 => MartProductPageRoutes.ReviewFor(productId.Value),
            MartProductScreenKind.Order when productId is > 0 => MartProductPageRoutes.OrderFor(productId.Value),
            _ => throw new ArgumentException("이 화면에는 유효한 마트 상품 ID가 필요합니다.", nameof(productId))
        };

        var values = new List<string>();
        Add(values, "from", PageNavigationContext.NormalizeReturnPath(From));
        Add(values, "q", Search);
        if (AvailableOnly)
        {
            Add(values, "available", "true");
        }
        if (Page > 1)
        {
            Add(values, "page", Page.ToString(CultureInfo.InvariantCulture));
        }

        return values.Count == 0 ? path : $"{path}?{string.Join('&', values)}";
    }

    public static MartProductNavigationContext Parse(string? uri)
    {
        var query = ParseQuery(uri);
        return new MartProductNavigationContext
        {
            From = PageNavigationContext.NormalizeReturnPath(Get(query, "from")),
            Search = Get(query, "q") ?? Get(query, "search"),
            AvailableOnly = ParseBoolean(Get(query, "available")),
            Page = ParsePage(query)
        };
    }

    private static bool ParseBoolean(string? value)
        => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);

    private static int ParsePage(IReadOnlyDictionary<string, string> query)
        => int.TryParse(Get(query, "page"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? Math.Max(1, value)
            : 1;

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
