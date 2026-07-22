using System.Globalization;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Contracts.Common.Sales;

public enum SalesOrderScreenKind
{
    List,
    Detail
}

/// <summary>Web과 모바일 앱이 공유하는 영속 판매 주문 List·Detail route입니다.</summary>
public static class SalesOrderPageRoutes
{
    public const string Root = "/shipper/sales/orders";
    public const string DetailTemplate = $"{Root}/{{OrderId:long}}";

    // 로컬 피킹·포장 Simulation은 영속 주문 조회 route와 분리합니다.
    public const string FulfillmentRoot = "/shipper/sales/fulfillment";

    public static string DetailFor(long orderId)
        => $"{Root}/{RequireOrderId(orderId)}";

    private static long RequireOrderId(long orderId)
        => orderId > 0
            ? orderId
            : throw new ArgumentOutOfRangeException(nameof(orderId), "판매 주문 ID는 1 이상이어야 합니다.");
}

/// <summary>판매 주문 목록 조건과 안전한 복귀 위치를 List·Detail 사이에 보존합니다.</summary>
public sealed record SalesOrderNavigationContext
{
    public string? From { get; init; }
    public string? Search { get; init; }
    public string? SyncScope { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;

    public string ResolveReturnPath(string fallback = SalesOrderPageRoutes.Root)
        => PageNavigationContext.ResolveReturnPath(From, fallback);

    public SalesOrderNavigationContext WithListState(
        string? search,
        string? syncScope,
        string? status,
        int page)
        => this with
        {
            Search = Normalize(search),
            SyncScope = Normalize(syncScope),
            Status = Normalize(status),
            Page = Math.Max(1, page)
        };

    public string PathFor(SalesOrderScreenKind screen, long? orderId = null)
    {
        var path = screen switch
        {
            SalesOrderScreenKind.List => SalesOrderPageRoutes.Root,
            SalesOrderScreenKind.Detail when orderId is > 0 => SalesOrderPageRoutes.DetailFor(orderId.Value),
            _ => throw new ArgumentException("상세 화면에는 유효한 판매 주문 ID가 필요합니다.", nameof(orderId))
        };

        var values = new List<string>();
        Add(values, "from", PageNavigationContext.NormalizeReturnPath(From));
        Add(values, "q", Normalize(Search));
        Add(values, "scope", Normalize(SyncScope));
        Add(values, "status", Normalize(Status));
        if (Page > 1)
        {
            Add(values, "page", Page.ToString(CultureInfo.InvariantCulture));
        }

        return values.Count == 0 ? path : $"{path}?{string.Join('&', values)}";
    }

    public static SalesOrderNavigationContext Parse(string? uri)
    {
        var query = ParseQuery(uri);
        return new SalesOrderNavigationContext
        {
            From = PageNavigationContext.NormalizeReturnPath(Get(query, "from")),
            Search = Get(query, "q") ?? Get(query, "search"),
            SyncScope = Get(query, "scope") ?? Get(query, "syncScope"),
            Status = Get(query, "status"),
            Page = ParsePage(query)
        };
    }

    private static int ParsePage(IReadOnlyDictionary<string, string> query)
        => int.TryParse(Get(query, "page"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? Math.Max(1, value)
            : 1;

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
