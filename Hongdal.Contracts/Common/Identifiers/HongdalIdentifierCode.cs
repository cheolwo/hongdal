namespace Hongdal.Contracts.Common.Identifiers;

public static class HongdalIdentifierKindCode
{
    public const string Unknown = "unknown";
    public const string Order = "order";
    public const string Ledger = "ledger";
    public const string OutboundPlan = "outbound-plan";
    public const string TransportRequest = "transport-request";
    public const string InboundRequest = "inbound-request";
    public const string Product = "product";
    public const string StorageLocation = "storage-location";
    public const string HandlingUnit = "handling-unit";
    public const string Bundle = "bundle";
}

public static class HongdalMachineReadableCodeFormatCode
{
    public const string QrCode = "qr-code";
    public const string Code128 = "code-128";
}

public sealed record HongdalIdentifierCodePayload(
    string Kind,
    string Value,
    string RawCode,
    string DisplayName,
    string HumanReadableText);

public static class HongdalIdentifierCodePayloads
{
    public static HongdalIdentifierCodePayload Create(
        string kind,
        string value,
        string? displayName = null,
        string? rawCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedKind = NormalizeKind(kind);
        var normalizedValue = NormalizeValue(value);
        var normalizedRawCode = string.IsNullOrWhiteSpace(rawCode)
            ? $"{ResolvePrefix(normalizedKind)}:{normalizedValue}"
            : rawCode.Trim();

        return new HongdalIdentifierCodePayload(
            normalizedKind,
            normalizedValue,
            normalizedRawCode,
            string.IsNullOrWhiteSpace(displayName) ? ResolveDisplayName(normalizedKind) : displayName.Trim(),
            normalizedValue);
    }

    public static HongdalIdentifierCodePayload Parse(string rawCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawCode);

        var normalized = rawCode.Trim();
        var segments = normalized.Split(':', 3, StringSplitOptions.TrimEntries);
        if (segments.Length == 3 && segments[0].Equals("HD", StringComparison.OrdinalIgnoreCase))
        {
            return Create(ResolveKindFromPrefix(segments[1]), segments[2], rawCode: normalized);
        }

        var separatorIndex = normalized.IndexOf(':');
        if (separatorIndex <= 0)
        {
            return Create(HongdalIdentifierKindCode.Unknown, normalized, rawCode: normalized);
        }

        var prefix = normalized[..separatorIndex];
        var value = normalized[(separatorIndex + 1)..];
        return Create(ResolveKindFromPrefix(prefix), value, rawCode: normalized);
    }

    public static string NormalizeKind(string kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return HongdalIdentifierKindCode.Unknown;
        }

        return kind.Trim().ToLowerInvariant() switch
        {
            "ord" or "order" or "order-number" or "order-no" => HongdalIdentifierKindCode.Order,
            "led" or "ledger" or "ledger-number" or "ledger-no" => HongdalIdentifierKindCode.Ledger,
            "out" or "outbound" or "outbound-plan" or "outbound-planned" => HongdalIdentifierKindCode.OutboundPlan,
            "trq" or "transport" or "transport-request" or "request" => HongdalIdentifierKindCode.TransportRequest,
            "inb" or "ib" or "inbound" or "inbound-request" => HongdalIdentifierKindCode.InboundRequest,
            "sku" or "prd" or "item" or "product" => HongdalIdentifierKindCode.Product,
            "loc" or "bin" or "rack" or "storage-location" => HongdalIdentifierKindCode.StorageLocation,
            "hu" or "pallet" or "box" or "handling-unit" => HongdalIdentifierKindCode.HandlingUnit,
            "bnd" or "bundle" => HongdalIdentifierKindCode.Bundle,
            _ => HongdalIdentifierKindCode.Unknown
        };
    }

    public static string ResolvePrefix(string kind)
    {
        return NormalizeKind(kind) switch
        {
            HongdalIdentifierKindCode.Order => "ORD",
            HongdalIdentifierKindCode.Ledger => "LED",
            HongdalIdentifierKindCode.OutboundPlan => "OUT",
            HongdalIdentifierKindCode.TransportRequest => "TRQ",
            HongdalIdentifierKindCode.InboundRequest => "INB",
            HongdalIdentifierKindCode.Product => "SKU",
            HongdalIdentifierKindCode.StorageLocation => "LOC",
            HongdalIdentifierKindCode.HandlingUnit => "HU",
            HongdalIdentifierKindCode.Bundle => "BND",
            _ => "HD"
        };
    }

    public static string ResolveKindFromPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return HongdalIdentifierKindCode.Unknown;
        }

        return prefix.Trim().ToUpperInvariant() switch
        {
            "ORD" or "ORDER" => HongdalIdentifierKindCode.Order,
            "LED" or "LEDGER" => HongdalIdentifierKindCode.Ledger,
            "OUT" or "OBP" => HongdalIdentifierKindCode.OutboundPlan,
            "TRQ" or "REQ" => HongdalIdentifierKindCode.TransportRequest,
            "INB" or "IB" => HongdalIdentifierKindCode.InboundRequest,
            "SKU" or "PRD" or "ITEM" => HongdalIdentifierKindCode.Product,
            "LOC" or "BIN" or "RACK" => HongdalIdentifierKindCode.StorageLocation,
            "HU" or "PALLET" or "BOX" => HongdalIdentifierKindCode.HandlingUnit,
            "BND" or "BUNDLE" => HongdalIdentifierKindCode.Bundle,
            _ => HongdalIdentifierKindCode.Unknown
        };
    }

    public static string ResolveDisplayName(string kind)
    {
        return NormalizeKind(kind) switch
        {
            HongdalIdentifierKindCode.Order => "주문 번호",
            HongdalIdentifierKindCode.Ledger => "원장 번호",
            HongdalIdentifierKindCode.OutboundPlan => "출고예정 번호",
            HongdalIdentifierKindCode.TransportRequest => "운송 의뢰 번호",
            HongdalIdentifierKindCode.InboundRequest => "입고요청 번호",
            HongdalIdentifierKindCode.Product => "상품 바코드",
            HongdalIdentifierKindCode.StorageLocation => "적재 위치",
            HongdalIdentifierKindCode.HandlingUnit => "핸들링 유닛",
            HongdalIdentifierKindCode.Bundle => "묶음 바코드",
            _ => "식별 번호"
        };
    }

    private static string NormalizeValue(string value)
    {
        return value.Trim().Replace('\r', ' ').Replace('\n', ' ');
    }
}
