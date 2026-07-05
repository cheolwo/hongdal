namespace Hongdal.Contracts.Common.WarehouseScanning;

public static class WarehouseBarcodeKindCode
{
    public const string Unknown = "unknown";
    public const string InboundRequest = "inbound-request";
    public const string Product = "product";
    public const string StorageLocation = "storage-location";
    public const string HandlingUnit = "handling-unit";
    public const string Bundle = "bundle";
}

public static class WarehouseScanStepCode
{
    public const string ReceiveInbound = "receive-inbound";
    public const string SplitProduct = "split-product";
    public const string CreateBundle = "create-bundle";
    public const string PutAway = "put-away";
}

public sealed record WarehouseBarcodeScan(
    string RawCode,
    string Kind,
    string Value,
    DateTimeOffset ScannedAt,
    string ScannerId = "",
    string OperatorUserId = "");

public sealed record WarehouseScanRequirement(
    string Kind,
    string Label,
    bool IsRequired = true);

public sealed record WarehouseScanStepDefinition(
    string Step,
    string DisplayName,
    IReadOnlyList<WarehouseScanRequirement> Requirements);

public sealed record WarehouseScanAction(
    string Step,
    string ActionCode,
    string DisplayName,
    bool IsReady,
    string Message,
    IReadOnlyList<WarehouseBarcodeScan> Scans);

public sealed record WarehouseScanSession(
    string Step,
    IReadOnlyList<WarehouseBarcodeScan> Scans,
    WarehouseScanAction Action);

public static class WarehouseBarcodeParser
{
    public static WarehouseBarcodeScan Parse(
        string rawCode,
        DateTimeOffset? scannedAt = null,
        string scannerId = "",
        string operatorUserId = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawCode);

        var normalized = rawCode.Trim();
        var separatorIndex = normalized.IndexOf(':');
        var prefix = separatorIndex > 0 ? normalized[..separatorIndex] : string.Empty;
        var value = separatorIndex > 0 ? normalized[(separatorIndex + 1)..] : normalized;

        return new WarehouseBarcodeScan(
            normalized,
            ResolveKind(prefix),
            value.Trim(),
            scannedAt ?? DateTimeOffset.UtcNow,
            scannerId,
            operatorUserId);
    }

    private static string ResolveKind(string prefix)
    {
        return prefix.ToUpperInvariant() switch
        {
            "INB" or "IB" => WarehouseBarcodeKindCode.InboundRequest,
            "SKU" or "PRD" or "ITEM" => WarehouseBarcodeKindCode.Product,
            "LOC" or "BIN" or "RACK" => WarehouseBarcodeKindCode.StorageLocation,
            "HU" or "PALLET" or "BOX" => WarehouseBarcodeKindCode.HandlingUnit,
            "BND" or "BUNDLE" => WarehouseBarcodeKindCode.Bundle,
            _ => WarehouseBarcodeKindCode.Unknown
        };
    }
}

public static class WarehouseScanWorkflowPlanner
{
    public static readonly IReadOnlyList<WarehouseScanStepDefinition> DefaultSteps =
    [
        new(
            WarehouseScanStepCode.ReceiveInbound,
            "Inbound receive",
            [
                new(WarehouseBarcodeKindCode.InboundRequest, "Inbound request"),
                new(WarehouseBarcodeKindCode.Product, "Product")
            ]),
        new(
            WarehouseScanStepCode.SplitProduct,
            "Product split",
            [
                new(WarehouseBarcodeKindCode.Product, "Product"),
                new(WarehouseBarcodeKindCode.HandlingUnit, "Split handling unit")
            ]),
        new(
            WarehouseScanStepCode.CreateBundle,
            "Bundle create",
            [
                new(WarehouseBarcodeKindCode.Product, "Product"),
                new(WarehouseBarcodeKindCode.Bundle, "Bundle")
            ]),
        new(
            WarehouseScanStepCode.PutAway,
            "Put away",
            [
                new(WarehouseBarcodeKindCode.Product, "Product"),
                new(WarehouseBarcodeKindCode.StorageLocation, "Storage location"),
                new(WarehouseBarcodeKindCode.Bundle, "Bundle", IsRequired: false),
                new(WarehouseBarcodeKindCode.HandlingUnit, "Handling unit", IsRequired: false)
            ])
    ];

    public static WarehouseScanSession BuildSession(
        string step,
        IEnumerable<WarehouseBarcodeScan> scans,
        IEnumerable<WarehouseScanStepDefinition>? stepDefinitions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(step);

        var definitions = stepDefinitions?.ToArray() ?? DefaultSteps;
        var definition = definitions.FirstOrDefault(x => Matches(x.Step, step))
            ?? throw new InvalidOperationException($"Unknown warehouse scan step: {step}");

        var scanList = scans
            .Where(x => !string.IsNullOrWhiteSpace(x.RawCode))
            .GroupBy(x => x.RawCode, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.OrderByDescending(scan => scan.ScannedAt).First())
            .OrderBy(x => x.ScannedAt)
            .ToArray();

        var missing = definition.Requirements
            .Where(x => x.IsRequired)
            .Where(x => !scanList.Any(scan => Matches(scan.Kind, x.Kind)))
            .Select(x => x.Label)
            .ToArray();

        var isReady = missing.Length == 0;
        var action = new WarehouseScanAction(
            definition.Step,
            ResolveActionCode(definition.Step),
            definition.DisplayName,
            isReady,
            isReady ? "Ready to process." : $"Scan required: {string.Join(", ", missing)}",
            scanList);

        return new WarehouseScanSession(definition.Step, scanList, action);
    }

    private static string ResolveActionCode(string step)
    {
        return step switch
        {
            WarehouseScanStepCode.ReceiveInbound => "confirm-inbound-received",
            WarehouseScanStepCode.SplitProduct => "confirm-product-split",
            WarehouseScanStepCode.CreateBundle => "confirm-bundle-created",
            WarehouseScanStepCode.PutAway => "confirm-put-away",
            _ => "confirm-scan-step"
        };
    }

    private static bool Matches(string actual, string expected)
    {
        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }
}
