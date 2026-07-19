using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Food;
using Microsoft.Extensions.Logging;

namespace Ssalddel.Services.Community;

public sealed class 음식주문원장업무투영Handler : I원장업무투영동기화Handler
{
    private readonly ISsalddelFoodOrderStore _orderStore;
    private readonly ILogger<음식주문원장업무투영Handler> _logger;

    public 음식주문원장업무투영Handler(
        ISsalddelFoodOrderStore orderStore,
        ILogger<음식주문원장업무투영Handler> logger)
    {
        _orderStore = orderStore;
        _logger = logger;
    }

    public bool 처리대상인가(커뮤니티원장Dto 원장)
        => 음식주문원장업무투영Snapshot.처리대상인가(원장);

    public Task 동기화Async(커뮤니티원장Dto 원장, CancellationToken cancellationToken = default)
    {
        var snapshot = 음식주문원장업무투영Snapshot.생성(원장);
        if (snapshot is null)
        {
            return Task.CompletedTask;
        }

        if (_orderStore is not I커뮤니티원장반영가능음식주문Store ledgerAwareStore)
        {
            _logger.LogDebug(
                "음식 주문 저장소가 커뮤니티 원장 메타데이터 반영을 지원하지 않습니다. 주문번호={주문번호}, 원장Id={원장Id}",
                snapshot.주문번호,
                snapshot.LedgerId);
            return Task.CompletedTask;
        }

        var updated = ledgerAwareStore.커뮤니티원장반영(
            snapshot.주문번호,
            snapshot.LedgerId,
            snapshot.LedgerTemplateKey,
            snapshot.LedgerState,
            DateTime.UtcNow);

        if (updated is null)
        {
            _logger.LogDebug(
                "커뮤니티 원장과 연결할 음식 주문을 찾지 못했습니다. 주문번호={주문번호}, 원장Id={원장Id}",
                snapshot.주문번호,
                snapshot.LedgerId);
        }

        return Task.CompletedTask;
    }
}

public sealed class 음식주문원장업무투영Snapshot
{
    public string LedgerId { get; init; } = string.Empty;
    public string LedgerTemplateKey { get; init; } = string.Empty;
    public string LedgerState { get; init; } = string.Empty;
    public string 주문번호 { get; init; } = string.Empty;

    public static bool 처리대상인가(커뮤니티원장Dto 원장)
    {
        if (string.Equals(원장.원장템플릿Key, CommunityLedgerTemplateKeys.FoodOrder, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (TryGet(원장.외부참조, "음식주문번호", "주문번호", "FoodOrderNo", "orderNo") is not null)
        {
            return true;
        }

        return 원장.블록목록.Any(block =>
        {
            var entityHint = TryGet(block.Data, "업무엔티티", "Entity", "Projection");
            return ContainsAny(entityHint, "음식주문", "FoodOrder")
                   || ContainsAny(block.BlockId, "food-order", "menu")
                   || ContainsAny(block.Title, "음식 주문", "메뉴");
        });
    }

    public static 음식주문원장업무투영Snapshot? 생성(커뮤니티원장Dto 원장)
    {
        if (!처리대상인가(원장))
        {
            return null;
        }

        var orderBlock = 원장.블록목록.FirstOrDefault(block =>
            ContainsAny(block.BlockId, "food-order", "menu")
            || ContainsAny(block.Title, "음식 주문", "메뉴")
            || ContainsAny(TryGet(block.Data, "업무엔티티", "Entity"), "음식주문", "FoodOrder"));

        var orderNo = FirstNonEmpty(
            TryGet(원장.외부참조, "음식주문번호", "주문번호", "FoodOrderNo", "orderNo"),
            TryGet(orderBlock?.Data, "음식주문번호", "주문번호", "FoodOrderNo", "orderNo"),
            ExtractFoodOrderNo(원장.원장Id));

        if (orderNo is null)
        {
            return null;
        }

        return new 음식주문원장업무투영Snapshot
        {
            LedgerId = Clean(원장.원장Id) ?? orderNo,
            LedgerTemplateKey = Clean(원장.원장템플릿Key) ?? CommunityLedgerTemplateKeys.FoodOrder,
            LedgerState = Clean(원장.상태) ?? 커뮤니티원장상태.초안,
            주문번호 = orderNo
        };
    }

    private static string? ExtractFoodOrderNo(string? ledgerId)
    {
        var cleaned = Clean(ledgerId);
        const string prefix = "food-order:";
        return cleaned is not null && cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? cleaned[prefix.Length..]
            : null;
    }

    private static string? TryGet(IReadOnlyDictionary<string, string>? data, params string[] keys)
    {
        if (data is null || data.Count == 0)
        {
            return null;
        }

        foreach (var key in keys)
        {
            foreach (var pair in data)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return Clean(pair.Value);
                }
            }
        }

        return null;
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.Select(Clean).FirstOrDefault(value => value is not null);

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool ContainsAny(string? source, params string[] candidates)
    {
        var text = Clean(source);
        return text is not null && candidates.Any(candidate => text.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }
}
