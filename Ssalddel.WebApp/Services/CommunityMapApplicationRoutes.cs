using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Mart;
using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.WebApp.Services;

public static class CommunityMapApplicationActionCodes
{
    public const string LogisticsProxy = "logistics-proxy";
    public const string TransportProxy = "transport-proxy";
    public const string IndividualOrder = "individual-order";
}

public sealed record CommunityMapApplicationOption(
    string Code,
    string Label,
    string Description,
    string Href);

/// <summary>
/// 공개 지도 마커를 기존 신청 화면의 출발 문맥으로만 전달합니다.
/// 마커 이름을 공급자·계약 상대·실제 상하차지로 확정하지 않습니다.
/// </summary>
public static class CommunityMapApplicationRoutes
{
    public const string ChooserPageRoute = "/community/map-application-chooser";
    public const string SourceCode = "community-map";
    public const string ReturnPath = "/community/home";

    public static bool UsesStandaloneApplicationLayout(string? uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var absolute)
            && !Uri.TryCreate(new Uri("https://ssalddel.local"), uri, out absolute))
        {
            return false;
        }

        if (!IsApplicationPath(absolute.AbsolutePath)
            || !TryReadQueryValue(absolute.Query, "source", out var source))
        {
            return false;
        }

        return string.Equals(source, SourceCode, StringComparison.OrdinalIgnoreCase);
    }

    public static string BuildChooserPath(
        string markerId,
        string markerName,
        string layerCode,
        string countryCode,
        string? action = null,
        string? returnTo = null)
    {
        var context = new MarkerContext(
            Normalize(markerId, 160, nameof(markerId)),
            Normalize(markerName, 160, nameof(markerName)),
            Normalize(layerCode, 80, nameof(layerCode)),
            Normalize(countryCode, 8, nameof(countryCode)).ToUpperInvariant());

        var query = new List<(string Key, string Value)>
        {
            ("source", SourceCode),
            ("sourceMarkerId", context.Id),
            ("markerTitle", context.Name),
            ("markerLayer", context.LayerCode),
            ("country", context.CountryCode)
        };

        if (!string.IsNullOrWhiteSpace(action))
        {
            query.Add(("action", action));
        }

        if (!string.IsNullOrWhiteSpace(returnTo))
        {
            query.Add(("returnTo", returnTo));
        }

        var queryString = string.Join(
            "&",
            query.Select(item => $"{item.Key}={Uri.EscapeDataString(item.Value)}"));
        return $"{ChooserPageRoute}?{queryString}";
    }

    public static IReadOnlyList<CommunityMapApplicationOption> ForMarker(
        string markerId,
        string markerName,
        string layerCode,
        string countryCode)
    {
        var context = new MarkerContext(
            Normalize(markerId, 160, nameof(markerId)),
            Normalize(markerName, 160, nameof(markerName)),
            Normalize(layerCode, 80, nameof(layerCode)),
            Normalize(countryCode, 8, nameof(countryCode)).ToUpperInvariant());

        return
        [
            new(
                CommunityMapApplicationActionCodes.LogisticsProxy,
                "물류대행 신청",
                "창고 후보와 입고 조건을 직접 선택해 입고 요청 원장을 작성합니다.",
                BuildLogisticsProxyPath(context)),
            new(
                CommunityMapApplicationActionCodes.TransportProxy,
                "운송대행 신청",
                "화물·상하차·차량 조건을 입력한 뒤 운송 의뢰를 검토합니다.",
                BuildTransportProxyPath(context)),
            new(
                CommunityMapApplicationActionCodes.IndividualOrder,
                "개별 주문 신청",
                "공개 상품을 다시 선택하고 비구속 주문 의향을 작성합니다.",
                BuildIndividualOrderPath(context))
        ];
    }

    public static string ReturnToMarker(
        string markerId,
        string layerCode,
        string countryCode,
        string? ledgerId = null)
        => CommunityPageRoutes.WorldMapFor(
            countryCode: Normalize(countryCode, 8, nameof(countryCode)).ToUpperInvariant(),
            layerCodes: Normalize(layerCode, 80, nameof(layerCode)),
            markerId: Normalize(markerId, 160, nameof(markerId)),
            ledgerId: string.IsNullOrWhiteSpace(ledgerId)
                ? null
                : Normalize(ledgerId, 200, nameof(ledgerId)));

    private static string BuildLogisticsProxyPath(MarkerContext marker)
        => new InboundRequestNavigationContext
        {
            From = ReturnToMarker(marker.Id, marker.LayerCode, marker.CountryCode),
            Source = SourceCode,
            SourceMarkerId = marker.Id,
            NodeTitle = marker.Name,
            NodeGroup = marker.LayerCode,
            NodeDescription = $"공개 지도 마커 {marker.Id}에서 시작한 신청 문맥입니다. 실제 창고·공급자·계약 상대는 신청 화면에서 별도로 확인합니다.",
            Scope = marker.CountryCode
        }.PathFor(InboundRequestScreenKind.Create);

    private static string BuildTransportProxyPath(MarkerContext marker)
        => new ShipperRequestNavigationContext
        {
            Source = SourceCode,
            SourceMarkerId = marker.Id,
            NodeTitle = marker.Name,
            NodeKind = marker.LayerCode,
            CountryCode = marker.CountryCode,
            ReturnPath = ReturnToMarker(marker.Id, marker.LayerCode, marker.CountryCode)
        }.RootPath;

    private static string BuildIndividualOrderPath(MarkerContext marker)
    {
        var values = new (string Key, string Value)[]
        {
            ("from", ReturnToMarker(marker.Id, marker.LayerCode, marker.CountryCode)),
            ("source", SourceCode),
            ("sourceMarkerId", marker.Id),
            ("markerTitle", marker.Name),
            ("markerLayer", marker.LayerCode),
            ("country", marker.CountryCode)
        };
        var query = string.Join('&', values.Select(item =>
            $"{item.Key}={Uri.EscapeDataString(item.Value)}"));
        return $"{MartProductPageRoutes.OrderRoot}?{query}";
    }

    private static string Normalize(string value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.Length > maximumLength
            || normalized.Any(char.IsControl))
        {
            throw new ArgumentException("지도 신청 문맥 값이 올바르지 않습니다.", parameterName);
        }

        return normalized;
    }

    private static bool IsApplicationPath(string path)
    {
        var normalized = path.TrimEnd('/');
        return string.Equals(normalized, ChooserPageRoute, StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalized, InboundRequestPageRoutes.Create, StringComparison.OrdinalIgnoreCase)
               || normalized.StartsWith($"{InboundRequestPageRoutes.Root}/", StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalized, ShipperRequestPageRoutes.Root, StringComparison.OrdinalIgnoreCase)
               || normalized.StartsWith($"{ShipperRequestPageRoutes.Root}/", StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalized, MartProductPageRoutes.OrderRoot, StringComparison.OrdinalIgnoreCase)
               || normalized.StartsWith($"{MartProductPageRoutes.OrderRoot}/", StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalized, MartProductPageRoutes.LegacyWebOrderRoot, StringComparison.OrdinalIgnoreCase)
               || normalized.StartsWith($"{MartProductPageRoutes.LegacyWebOrderRoot}/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryReadQueryValue(string query, string expectedKey, out string value)
    {
        value = string.Empty;
        try
        {
            foreach (var item in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split('=', 2);
                if (!string.Equals(Uri.UnescapeDataString(parts[0].Replace('+', ' ')), expectedKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                value = Uri.UnescapeDataString((parts.Length > 1 ? parts[1] : string.Empty).Replace('+', ' '));
                return true;
            }
        }
        catch (UriFormatException)
        {
            return false;
        }

        return false;
    }

    private sealed record MarkerContext(
        string Id,
        string Name,
        string LayerCode,
        string CountryCode);
}
