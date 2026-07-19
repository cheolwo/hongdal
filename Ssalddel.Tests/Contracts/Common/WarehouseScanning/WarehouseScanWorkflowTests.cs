using Ssalddel.Contracts.Common.WarehouseScanning;

namespace Ssalddel.Tests.Contracts.Common.WarehouseScanning;

public sealed class WarehouseScanWorkflowTests
{
    [Theory]
    [InlineData("INB:1001", WarehouseBarcodeKindCode.InboundRequest, "1001")]
    [InlineData("SKU:ABC-1", WarehouseBarcodeKindCode.Product, "ABC-1")]
    [InlineData("LOC:A-01-02", WarehouseBarcodeKindCode.StorageLocation, "A-01-02")]
    [InlineData("BND:PKG-9", WarehouseBarcodeKindCode.Bundle, "PKG-9")]
    [InlineData("PALLET:P-1", WarehouseBarcodeKindCode.HandlingUnit, "P-1")]
    public void Parse_RecognizesKnownBarcodePrefixes(string rawCode, string expectedKind, string expectedValue)
    {
        var scan = WarehouseBarcodeParser.Parse(rawCode);

        Assert.Equal(expectedKind, scan.Kind);
        Assert.Equal(expectedValue, scan.Value);
    }

    [Fact]
    public void BuildSession_InboundReceiveRequiresInboundProductAndBundle()
    {
        var session = WarehouseScanWorkflowPlanner.BuildSession(
            WarehouseScanStepCode.ReceiveInbound,
            [
                WarehouseBarcodeParser.Parse("INB:1001"),
                WarehouseBarcodeParser.Parse("SKU:ABC-1")
            ]);

        Assert.False(session.Action.IsReady);
        Assert.Contains("Inbound bundle", session.Action.Message);
    }

    [Fact]
    public void BuildSession_InboundReceiveBecomesReadyWithInboundProductAndBundle()
    {
        var session = WarehouseScanWorkflowPlanner.BuildSession(
            WarehouseScanStepCode.ReceiveInbound,
            [
                WarehouseBarcodeParser.Parse("INB:1001"),
                WarehouseBarcodeParser.Parse("SKU:ABC-1"),
                WarehouseBarcodeParser.Parse("BND:PKG-9")
            ]);

        Assert.True(session.Action.IsReady);
        Assert.Equal("confirm-inbound-received", session.Action.ActionCode);
    }

    [Fact]
    public void BuildSession_PutAwayRequiresLocationAndBundle()
    {
        var session = WarehouseScanWorkflowPlanner.BuildSession(
            WarehouseScanStepCode.PutAway,
            [
                WarehouseBarcodeParser.Parse("LOC:A-01-02"),
                WarehouseBarcodeParser.Parse("BND:PKG-9")
            ]);

        Assert.True(session.Action.IsReady);
        Assert.Equal("confirm-put-away", session.Action.ActionCode);
    }

    [Fact]
    public void BuildSession_PutAwayDoesNotRequireProductBarcode()
    {
        var session = WarehouseScanWorkflowPlanner.BuildSession(
            WarehouseScanStepCode.PutAway,
            [WarehouseBarcodeParser.Parse("LOC:A-01-02")]);

        Assert.False(session.Action.IsReady);
        Assert.DoesNotContain("Product", session.Action.Message);
        Assert.Contains("Inbound bundle", session.Action.Message);
    }

    [Fact]
    public void BuildSession_CreateBundleRejectsMoreThanThreeDistinctProducts()
    {
        var session = WarehouseScanWorkflowPlanner.BuildSession(
            WarehouseScanStepCode.CreateBundle,
            [
                WarehouseBarcodeParser.Parse("SKU:ABC-1"),
                WarehouseBarcodeParser.Parse("SKU:ABC-2"),
                WarehouseBarcodeParser.Parse("SKU:ABC-3"),
                WarehouseBarcodeParser.Parse("SKU:ABC-4"),
                WarehouseBarcodeParser.Parse("BND:PKG-9")
            ]);

        Assert.False(session.Action.IsReady);
        Assert.Contains("up to 3", session.Action.Message);
    }

    [Fact]
    public void BuildSession_DeduplicatesSameRawCode()
    {
        var first = WarehouseBarcodeParser.Parse("SKU:ABC-1", new DateTimeOffset(2026, 7, 3, 1, 0, 0, TimeSpan.Zero));
        var second = WarehouseBarcodeParser.Parse("SKU:ABC-1", new DateTimeOffset(2026, 7, 3, 2, 0, 0, TimeSpan.Zero));

        var session = WarehouseScanWorkflowPlanner.BuildSession(
            WarehouseScanStepCode.SplitProduct,
            [first, second, WarehouseBarcodeParser.Parse("HU:P-1")]);

        Assert.Equal(2, session.Scans.Count);
        Assert.Contains(session.Scans, x => x.RawCode == "SKU:ABC-1" && x.ScannedAt == second.ScannedAt);
    }
}
