using System.Globalization;
using System.Text;
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

/// <summary>모바일 로컬 주문 이행 Simulation의 사용자 목표별 route입니다.</summary>
public static class OrderFulfillmentSimulationPageRoutes
{
    private const char OrderKeySeparator = '\u001f';

    public const string Root = SalesOrderPageRoutes.FulfillmentRoot;
    public const string Samples = $"{Root}/samples";
    public const string Orders = $"{Root}/orders";
    public const string OrderDetailTemplate = $"{Orders}/{{OrderKey}}";
    public const string Inventory = $"{Root}/inventory";
    public const string Picking = $"{Root}/picking";
    public const string PickingTaskTemplate = $"{Picking}/{{TaskId:long}}";
    public const string Packing = $"{Root}/packing";
    public const string PackingTaskTemplate = $"{Packing}/{{TaskId:long}}";
    public const string RestockPolicy = $"{Root}/restock-policy";

    public static string OrderDetailFor(string channelType, string channelOrderNo)
        => $"{Orders}/{EncodeOrderKey(channelType, channelOrderNo)}";

    public static string PickingTaskFor(long taskId)
        => $"{Picking}/{RequireTaskId(taskId)}";

    public static string PackingTaskFor(long taskId)
        => $"{Packing}/{RequireTaskId(taskId)}";

    public static bool TryDecodeOrderKey(
        string? orderKey,
        out string channelType,
        out string channelOrderNo)
    {
        channelType = string.Empty;
        channelOrderNo = string.Empty;
        if (string.IsNullOrWhiteSpace(orderKey))
        {
            return false;
        }

        try
        {
            var normalized = orderKey.Trim().Replace('-', '+').Replace('_', '/');
            normalized = normalized.PadRight(normalized.Length + ((4 - normalized.Length % 4) % 4), '=');
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
            var separator = decoded.IndexOf(OrderKeySeparator);
            if (separator <= 0
                || separator == decoded.Length - 1
                || decoded.IndexOf(OrderKeySeparator, separator + 1) >= 0)
            {
                return false;
            }

            var decodedChannel = decoded[..separator];
            var decodedOrderNo = decoded[(separator + 1)..];
            if (string.IsNullOrWhiteSpace(decodedChannel)
                || string.IsNullOrWhiteSpace(decodedOrderNo))
            {
                return false;
            }

            channelType = decodedChannel;
            channelOrderNo = decodedOrderNo;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string EncodeOrderKey(string channelType, string channelOrderNo)
    {
        var normalizedChannel = RequireOrderKeyPart(channelType, nameof(channelType));
        var normalizedOrderNo = RequireOrderKeyPart(channelOrderNo, nameof(channelOrderNo));
        var bytes = Encoding.UTF8.GetBytes($"{normalizedChannel}{OrderKeySeparator}{normalizedOrderNo}");
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string RequireOrderKeyPart(string value, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.Contains(OrderKeySeparator))
        {
            throw new ArgumentException("채널과 주문번호는 비어 있지 않은 단일 값이어야 합니다.", parameterName);
        }

        return normalized;
    }

    private static long RequireTaskId(long taskId)
        => taskId > 0
            ? taskId
            : throw new ArgumentOutOfRangeException(nameof(taskId), "작업 ID는 1 이상이어야 합니다.");
}

/// <summary>로컬 Simulation 주문 목록 필터와 stable 주문 상세 복귀 문맥입니다.</summary>
public sealed record FulfillmentOrderNavigationContext
{
    public string? From { get; init; }
    public string? Search { get; init; }
    public string? Scope { get; init; }
    public string? Status { get; init; }

    public string ResolveReturnPath()
        => PageNavigationContext.ResolveReturnPath(
            From,
            OrderFulfillmentSimulationPageRoutes.Orders);

    public FulfillmentOrderNavigationContext WithListState(
        string? search,
        string? scope,
        string? status)
        => this with
        {
            Search = Normalize(search),
            Scope = Normalize(scope),
            Status = Normalize(status)
        };

    public string ListPath()
    {
        var values = new List<string>();
        Add(values, "q", Search);
        Add(values, "scope", Scope);
        Add(values, "status", Status);
        return values.Count == 0
            ? OrderFulfillmentSimulationPageRoutes.Orders
            : $"{OrderFulfillmentSimulationPageRoutes.Orders}?{string.Join('&', values)}";
    }

    public string DetailPath(string channelType, string channelOrderNo)
        => PageNavigationContext.WithReturnPath(
            OrderFulfillmentSimulationPageRoutes.OrderDetailFor(channelType, channelOrderNo),
            ListPath());

    public static FulfillmentOrderNavigationContext Parse(string? uri)
    {
        var query = ParseQuery(uri);
        return new FulfillmentOrderNavigationContext
        {
            From = PageNavigationContext.NormalizeReturnPath(Get(query, "from")),
            Search = Get(query, "q") ?? Get(query, "search"),
            Scope = Get(query, "scope"),
            Status = Get(query, "status")
        };
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Add(ICollection<string> values, string key, string? value)
    {
        var normalized = Normalize(value);
        if (normalized is not null)
        {
            values.Add($"{key}={Uri.EscapeDataString(normalized)}");
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
