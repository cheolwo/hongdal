using System.Text;
using Hongdal.Contracts.Common.Community;

namespace Hongdal.Ui.Common.Areas.App.Services;

public enum CommunityDecorationTarget
{
    Bagua,
    DiagramNode
}

public sealed record CommunityDecorationAsset(
    string Key,
    string PackKey,
    string Title,
    string CreatorName,
    string Summary,
    CommunityDecorationTarget Target,
    string PreviewSymbol,
    string AccentColor,
    노드스티커이미지Response? NodeSticker = null,
    bool IsCustom = false);

public sealed record CommunityDecorationProduct(
    string Key,
    string PackKey,
    string Title,
    string CreatorName,
    string Summary,
    CommunityDecorationTarget Target,
    decimal PriceAmount,
    string CurrencyCode,
    IReadOnlyList<CommunityDecorationAsset> Assets)
{
    public bool IsFree => PriceAmount <= 0;
}

public sealed class PlatformCommunityDecorationStateService
{
    private const string BasicBaguaAssetKey = "bagua-basic-blue";
    private readonly HashSet<string> ownedPackKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<CommunityDecorationAsset> customAssets = [];
    private readonly List<CommunityDecorationProduct> products;

    public PlatformCommunityDecorationStateService()
    {
        products = BuildProducts();
        foreach (var product in products.Where(item => item.IsFree))
        {
            ownedPackKeys.Add(product.PackKey);
        }
    }

    public event Action? Changed;

    public IReadOnlyList<CommunityDecorationProduct> Products => products;

    public IReadOnlyList<CommunityDecorationAsset> CustomAssets => customAssets;

    public bool IsBaguaDecorationEnabled { get; private set; } = true;

    public bool IsNodeDecorationEnabled { get; private set; } = true;

    public string? ActiveBaguaAssetKey { get; private set; } = BasicBaguaAssetKey;

    public string? ActiveNodeAssetKey { get; private set; }

    public CommunityDecorationAsset? ActiveBaguaAsset
        => FindAsset(ActiveBaguaAssetKey);

    public CommunityDecorationAsset? ActiveNodeAsset
        => FindAsset(ActiveNodeAssetKey);

    public 노드스티커이미지Response? ActiveNodeSticker
        => ActiveNodeAsset?.NodeSticker;

    public string BaguaSymbol
        => IsBaguaDecorationEnabled ? ActiveBaguaAsset?.PreviewSymbol ?? "☵" : "◎";

    public string BaguaAccentColor
        => IsBaguaDecorationEnabled ? ActiveBaguaAsset?.AccentColor ?? "#2563eb" : "#64748b";

    public bool IsProductOwned(CommunityDecorationProduct product)
        => product.IsFree || ownedPackKeys.Contains(product.PackKey);

    public bool IsAssetActive(CommunityDecorationAsset asset)
        => asset.Target switch
        {
            CommunityDecorationTarget.Bagua => IsBaguaDecorationEnabled &&
                string.Equals(ActiveBaguaAssetKey, asset.Key, StringComparison.OrdinalIgnoreCase),
            _ => IsNodeDecorationEnabled &&
                string.Equals(ActiveNodeAssetKey, asset.Key, StringComparison.OrdinalIgnoreCase)
        };

    public void Purchase(CommunityDecorationProduct product)
    {
        ownedPackKeys.Add(product.PackKey);
        Changed?.Invoke();
    }

    public bool Apply(CommunityDecorationAsset asset)
    {
        var product = products.FirstOrDefault(item =>
            item.Assets.Any(candidate => string.Equals(candidate.Key, asset.Key, StringComparison.OrdinalIgnoreCase)));
        if (product is not null && !IsProductOwned(product))
        {
            return false;
        }

        if (asset.Target == CommunityDecorationTarget.Bagua)
        {
            ActiveBaguaAssetKey = asset.Key;
            IsBaguaDecorationEnabled = true;
        }
        else
        {
            ActiveNodeAssetKey = asset.Key;
            IsNodeDecorationEnabled = true;
        }

        Changed?.Invoke();
        return true;
    }

    public void UseAutomaticNodeDecoration()
    {
        ActiveNodeAssetKey = null;
        IsNodeDecorationEnabled = true;
        Changed?.Invoke();
    }

    public void SetTargetEnabled(CommunityDecorationTarget target, bool enabled)
    {
        if (target == CommunityDecorationTarget.Bagua)
        {
            IsBaguaDecorationEnabled = enabled;
        }
        else
        {
            IsNodeDecorationEnabled = enabled;
        }

        Changed?.Invoke();
    }

    public bool TryCreateCustomAsset(
        CommunityDecorationTarget target,
        string? title,
        string? symbol,
        string? accentColor,
        string? imageUrl,
        out CommunityDecorationAsset? asset,
        out string message)
    {
        var normalizedTitle = title?.Trim();
        var normalizedSymbol = symbol?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            asset = null;
            message = "꾸미기 이름을 입력해 주세요.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(normalizedSymbol) && string.IsNullOrWhiteSpace(imageUrl))
        {
            asset = null;
            message = "표시 기호나 사용 권한이 있는 이미지 주소 중 하나를 입력해 주세요.";
            return false;
        }

        normalizedSymbol = string.IsNullOrWhiteSpace(normalizedSymbol) ? "나" : normalizedSymbol[..Math.Min(2, normalizedSymbol.Length)];
        var normalizedColor = NormalizeColor(accentColor);
        var key = $"custom-{target.ToString().ToLowerInvariant()}-{Guid.NewGuid():N}";
        노드스티커이미지Response? nodeSticker = null;
        if (target == CommunityDecorationTarget.DiagramNode)
        {
            nodeSticker = new()
            {
                이미지Key = key,
                팩Key = "user-custom",
                표시명 = normalizedTitle,
                이미지Url = string.IsNullOrWhiteSpace(imageUrl)
                    ? BuildNodeStickerDataUrl(normalizedColor, normalizedSymbol, normalizedTitle)
                    : imageUrl.Trim(),
                대체Text = $"{normalizedTitle} 사용자 제작 노드 이미지",
                MimeType = string.IsNullOrWhiteSpace(imageUrl) ? "image/svg+xml" : "image/*",
                원본너비Px = 512,
                원본높이Px = 512,
                노드종류목록 = ["product", "sales-channel", "place", "warehouse", "work", "delivery", "confirm"],
                스타일Tags = ["user-custom"],
                라이선스Code = 노드스티커라이선스Code.플랫폼노드사용,
                검수상태 = 노드스티커검수상태.초안
            };
        }

        asset = new(
            key,
            "user-custom",
            normalizedTitle,
            "내 제작함",
            target == CommunityDecorationTarget.Bagua
                ? "내가 만든 사방 이동판 표시입니다."
                : "내가 만든 다이어그램 노드 이미지입니다.",
            target,
            normalizedSymbol,
            normalizedColor,
            nodeSticker,
            IsCustom: true);
        customAssets.Insert(0, asset);
        products.Insert(0, new(
            $"store-{key}",
            "user-custom",
            normalizedTitle,
            "내 제작함",
            target == CommunityDecorationTarget.Bagua
                ? "내가 직접 만든 사방 이동판 꾸미기입니다."
                : "내가 직접 만든 다이어그램 노드 이미지입니다.",
            target,
            0,
            "KRW",
            [asset]));
        ownedPackKeys.Add("user-custom");
        Apply(asset);
        message = $"{normalizedTitle}을(를) 내 제작함에 저장하고 적용했습니다.";
        return true;
    }

    private CommunityDecorationAsset? FindAsset(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return products.SelectMany(item => item.Assets)
                   .Concat(customAssets)
                   .FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));
    }

    private static List<CommunityDecorationProduct> BuildProducts()
    {
        var products = new List<CommunityDecorationProduct>
        {
            new(
                "store-bagua-basic",
                "bagua-basic",
                "후천 사방 기본",
                "Hongdal",
                "기능을 방해하지 않는 단정한 기본 괘상입니다.",
                CommunityDecorationTarget.Bagua,
                0,
                "KRW",
                [new(BasicBaguaAssetKey, "bagua-basic", "푸른 감괘", "Hongdal", "기본 후천 사방 표시", CommunityDecorationTarget.Bagua, "☵", "#2563eb")]),
            new(
                "store-bagua-wonbanggak",
                "bagua-wonbanggak",
                "원방각 사람과 업무",
                "Hongdal Community",
                "원·방·각 철학을 중심점에 담은 커뮤니티용 표시입니다.",
                CommunityDecorationTarget.Bagua,
                0,
                "KRW",
                [new("bagua-wonbanggak", "bagua-wonbanggak", "원방각", "Hongdal Community", "원방각 커뮤니티 표시", CommunityDecorationTarget.Bagua, "○□△", "#ec4899")]),
            new(
                "store-bagua-night",
                "bagua-night",
                "별빛 사방판",
                "모래별 공방",
                "어두운 남보라색과 별 표식으로 만든 창작자 테마입니다.",
                CommunityDecorationTarget.Bagua,
                900,
                "KRW",
                [new("bagua-night-star", "bagua-night", "별빛 중심", "모래별 공방", "별빛 사방판 중심 표시", CommunityDecorationTarget.Bagua, "✦", "#7c3aed")])
        };

        foreach (var pack in 노드스티커Catalog.기본팩목록)
        {
            var assets = pack.이미지목록
                .Select(image => new CommunityDecorationAsset(
                    image.이미지Key,
                    pack.팩Key,
                    image.표시명,
                    pack.창작자표시명,
                    image.대체Text,
                    CommunityDecorationTarget.DiagramNode,
                    image.표시명[..1],
                    ResolveNodeAccent(image.이미지Key),
                    image))
                .ToArray();
            products.Add(new(
                $"store-{pack.팩Key}",
                pack.팩Key,
                pack.제목,
                pack.창작자표시명,
                pack.요약,
                CommunityDecorationTarget.DiagramNode,
                pack.거래정책.가격금액,
                pack.거래정책.통화Code,
                assets));
        }

        return products;
    }

    private static string NormalizeColor(string? color)
    {
        var candidate = color?.Trim();
        return candidate is { Length: 7 } && candidate[0] == '#' && candidate[1..].All(Uri.IsHexDigit)
            ? candidate
            : "#2563eb";
    }

    private static string ResolveNodeAccent(string key)
        => key switch
        {
            var value when value.Contains("warehouse", StringComparison.OrdinalIgnoreCase) => "#16a34a",
            var value when value.Contains("delivery", StringComparison.OrdinalIgnoreCase) => "#7c3aed",
            var value when value.Contains("confirm", StringComparison.OrdinalIgnoreCase) => "#0891b2",
            var value when value.Contains("work", StringComparison.OrdinalIgnoreCase) => "#d97706",
            _ => "#2563eb"
        };

    private static string BuildNodeStickerDataUrl(string accentColor, string symbol, string title)
    {
        var safeSymbol = System.Net.WebUtility.HtmlEncode(symbol);
        var safeTitle = System.Net.WebUtility.HtmlEncode(title.Length > 8 ? title[..8] : title);
        var svg = $$"""
            <svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
              <rect width="512" height="512" rx="128" fill="#ffffff"/>
              <circle cx="256" cy="224" r="132" fill="{{accentColor}}"/>
              <text x="256" y="270" text-anchor="middle" font-size="112" font-family="sans-serif" font-weight="700" fill="#ffffff">{{safeSymbol}}</text>
              <rect x="96" y="360" width="320" height="64" rx="32" fill="#111827"/>
              <text x="256" y="402" text-anchor="middle" font-size="30" font-family="sans-serif" font-weight="700" fill="#ffffff">{{safeTitle}}</text>
            </svg>
            """;
        return $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svg))}";
    }
}
