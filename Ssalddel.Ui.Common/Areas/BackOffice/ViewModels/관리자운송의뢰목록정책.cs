namespace Ssalddel.Ui.Common.Areas.BackOffice.ViewModels;

public static class 관리자운송의뢰목록정책
{
    public static IReadOnlyList<TItem> 의뢰Id검색<TItem>(
        IEnumerable<TItem> items,
        Func<TItem, string?> requestIdSelector,
        string? searchText)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(requestIdSelector);

        var normalized = searchText?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return items.ToArray();
        }

        return items
            .Where(item => requestIdSelector(item) is { } requestId
                           && requestId.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public static string 권역표시(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return "-";
        }

        var parts = address
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => !우편번호인가(part))
            .Take(2)
            .ToArray();

        if (parts.Length >= 2)
        {
            return string.Join(' ', parts);
        }

        var normalized = parts.FirstOrDefault() ?? address.Trim();
        const int maximumSingleTokenLength = 6;
        return normalized.Length <= maximumSingleTokenLength
            ? normalized
            : $"{normalized[..maximumSingleTokenLength]}…";
    }

    public static string 상세경로(string requestId)
        => $"/requests/{Uri.EscapeDataString(필수의뢰Id(requestId))}";

    public static string 관제경로(string requestId)
        => $"/transports/{Uri.EscapeDataString(필수의뢰Id(requestId))}";

    private static bool 우편번호인가(string value)
        => value.Length == 5 && value.All(char.IsDigit);

    private static string 필수의뢰Id(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("운송 의뢰 ID가 필요합니다.", nameof(requestId));
        }

        return requestId.Trim();
    }
}
