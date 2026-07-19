namespace Ssalddel.Contracts.Common.Identifiers;

public static class SsalddelIdentifierKindCode
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

public static class SsalddelMachineReadableCodeFormatCode
{
    public const string QrCode = "qr-code";
    public const string Code128 = "code-128";
}

public sealed record SsalddelIdentifierCodePayload(
    string Kind,
    string Value,
    string RawCode,
    string DisplayName,
    string HumanReadableText);

public static class SsalddelIdentifierCodePayloads
{
    public static SsalddelIdentifierCodePayload Create(
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

        return new SsalddelIdentifierCodePayload(
            normalizedKind,
            normalizedValue,
            normalizedRawCode,
            string.IsNullOrWhiteSpace(displayName) ? ResolveDisplayName(normalizedKind) : displayName.Trim(),
            normalizedValue);
    }

    public static SsalddelIdentifierCodePayload Parse(string rawCode)
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
            return Create(SsalddelIdentifierKindCode.Unknown, normalized, rawCode: normalized);
        }

        var prefix = normalized[..separatorIndex];
        var value = normalized[(separatorIndex + 1)..];
        return Create(ResolveKindFromPrefix(prefix), value, rawCode: normalized);
    }

    public static string NormalizeKind(string kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return SsalddelIdentifierKindCode.Unknown;
        }

        return kind.Trim().ToLowerInvariant() switch
        {
            "ord" or "order" or "order-number" or "order-no" => SsalddelIdentifierKindCode.Order,
            "led" or "ledger" or "ledger-number" or "ledger-no" => SsalddelIdentifierKindCode.Ledger,
            "out" or "outbound" or "outbound-plan" or "outbound-planned" => SsalddelIdentifierKindCode.OutboundPlan,
            "trq" or "transport" or "transport-request" or "request" => SsalddelIdentifierKindCode.TransportRequest,
            "inb" or "ib" or "inbound" or "inbound-request" => SsalddelIdentifierKindCode.InboundRequest,
            "sku" or "prd" or "item" or "product" => SsalddelIdentifierKindCode.Product,
            "loc" or "bin" or "rack" or "storage-location" => SsalddelIdentifierKindCode.StorageLocation,
            "hu" or "pallet" or "box" or "handling-unit" => SsalddelIdentifierKindCode.HandlingUnit,
            "bnd" or "bundle" => SsalddelIdentifierKindCode.Bundle,
            _ => SsalddelIdentifierKindCode.Unknown
        };
    }

    public static string ResolvePrefix(string kind)
    {
        return NormalizeKind(kind) switch
        {
            SsalddelIdentifierKindCode.Order => "ORD",
            SsalddelIdentifierKindCode.Ledger => "LED",
            SsalddelIdentifierKindCode.OutboundPlan => "OUT",
            SsalddelIdentifierKindCode.TransportRequest => "TRQ",
            SsalddelIdentifierKindCode.InboundRequest => "INB",
            SsalddelIdentifierKindCode.Product => "SKU",
            SsalddelIdentifierKindCode.StorageLocation => "LOC",
            SsalddelIdentifierKindCode.HandlingUnit => "HU",
            SsalddelIdentifierKindCode.Bundle => "BND",
            _ => "HD"
        };
    }

    public static string ResolveKindFromPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return SsalddelIdentifierKindCode.Unknown;
        }

        return prefix.Trim().ToUpperInvariant() switch
        {
            "ORD" or "ORDER" => SsalddelIdentifierKindCode.Order,
            "LED" or "LEDGER" => SsalddelIdentifierKindCode.Ledger,
            "OUT" or "OBP" => SsalddelIdentifierKindCode.OutboundPlan,
            "TRQ" or "REQ" => SsalddelIdentifierKindCode.TransportRequest,
            "INB" or "IB" => SsalddelIdentifierKindCode.InboundRequest,
            "SKU" or "PRD" or "ITEM" => SsalddelIdentifierKindCode.Product,
            "LOC" or "BIN" or "RACK" => SsalddelIdentifierKindCode.StorageLocation,
            "HU" or "PALLET" or "BOX" => SsalddelIdentifierKindCode.HandlingUnit,
            "BND" or "BUNDLE" => SsalddelIdentifierKindCode.Bundle,
            _ => SsalddelIdentifierKindCode.Unknown
        };
    }

    public static string ResolveDisplayName(string kind)
    {
        return NormalizeKind(kind) switch
        {
            SsalddelIdentifierKindCode.Order => "주문 번호",
            SsalddelIdentifierKindCode.Ledger => "원장 번호",
            SsalddelIdentifierKindCode.OutboundPlan => "출고예정 번호",
            SsalddelIdentifierKindCode.TransportRequest => "운송 의뢰 번호",
            SsalddelIdentifierKindCode.InboundRequest => "입고요청 번호",
            SsalddelIdentifierKindCode.Product => "상품 바코드",
            SsalddelIdentifierKindCode.StorageLocation => "적재 위치",
            SsalddelIdentifierKindCode.HandlingUnit => "핸들링 유닛",
            SsalddelIdentifierKindCode.Bundle => "묶음 바코드",
            _ => "식별 번호"
        };
    }

    private static string NormalizeValue(string value)
    {
        return value.Trim().Replace('\r', ' ').Replace('\n', ' ');
    }
}
