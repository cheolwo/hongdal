using Hongdal.Contracts.Common.Documents;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Tests.Ui.Common;

public sealed class HongdalDocumentOutputServiceTests
{
    [Fact]
    public void CreateInboundExpectedItemsIncludesDocumentAndLineBarcodes()
    {
        var service = new HongdalDocumentOutputService(new ZxingHongdalIdentifierCodeGenerator());
        var draft = CreateDraft(HongdalExpectedItemDocumentKindCode.InboundExpectedItems, "INB-TEST-1", "SKU:ABC-1");

        var output = service.CreateInboundExpectedItems(draft);

        Assert.Contains("입고예정 품목표", output.Html);
        Assert.Contains("INB:INB-TEST-1", output.Html);
        Assert.Contains("SKU:ABC-1", output.Html);
        Assert.Contains("<svg", output.Html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SKU:ABC-1", output.PlainText);
    }

    [Fact]
    public void CreateOutboundExpectedItemsIncludesDocumentAndLineBarcodes()
    {
        var service = new HongdalDocumentOutputService(new ZxingHongdalIdentifierCodeGenerator());
        var draft = CreateDraft(HongdalExpectedItemDocumentKindCode.OutboundExpectedItems, "OUT-TEST-1", "SKU:XYZ-2");

        var output = service.CreateOutboundExpectedItems(draft);

        Assert.Contains("출고예정 품목표", output.Html);
        Assert.Contains("OUT:OUT-TEST-1", output.Html);
        Assert.Contains("SKU:XYZ-2", output.Html);
        Assert.Contains("<svg", output.Html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SKU:XYZ-2", output.PlainText);
    }

    [Fact]
    public void CreateExpectedItemsAppliesPaperDensityAndBarcodeVisibility()
    {
        var service = new HongdalDocumentOutputService(new ZxingHongdalIdentifierCodeGenerator());
        var draft = CreateDraft(
            HongdalExpectedItemDocumentKindCode.OutboundExpectedItems,
            "OUT-TEST-2",
            "SKU:XYZ-3",
            new HongdalDocumentPrintLayoutOptions
            {
                PaperSize = HongdalDocumentPaperSizeCode.A5,
                Density = HongdalDocumentDensityCode.Dense,
                Orientation = HongdalDocumentOrientationCode.Landscape,
                IncludeDocumentQrCode = false,
                IncludeLineBarcodes = false
            });

        var output = service.CreateOutboundExpectedItems(draft);

        Assert.Contains("@page{size:A5 landscape", output.Html);
        Assert.Contains("data-paper=\"a5\"", output.Html);
        Assert.Contains("data-density=\"dense\"", output.Html);
        Assert.Contains("doc-code no-qr", output.Html);
        Assert.DoesNotContain("<span class=\"line-code\">", output.Html);
    }

    [Fact]
    public void CreateExpectedItemsUsesReceiptSlipLayoutForThermalPaper()
    {
        var service = new HongdalDocumentOutputService(new ZxingHongdalIdentifierCodeGenerator());
        var draft = CreateDraft(
            HongdalExpectedItemDocumentKindCode.OutboundExpectedItems,
            "OUT-SLIP-1",
            "SKU:FOOD-1",
            new HongdalDocumentPrintLayoutOptions
            {
                PaperSize = HongdalDocumentPaperSizeCode.Thermal58,
                Density = HongdalDocumentDensityCode.Compact,
                PrintStyle = HongdalDocumentPrintStyleCode.Sheet,
                IncludeDocumentQrCode = false,
                IncludeLineBarcodes = true
            });

        var output = service.CreateOutboundExpectedItems(draft);

        Assert.Contains("@page{size:58mm 200mm", output.Html);
        Assert.Contains("class=\"receipt-slip\"", output.Html);
        Assert.Contains("data-print-style=\"receipt-slip\"", output.Html);
        Assert.Contains("OUT-SLIP-1", output.Html);
        Assert.Contains("SKU:FOOD-1", output.Html);
        Assert.Contains("<span class=\"line-code\">", output.Html);
        Assert.DoesNotContain("<table>", output.Html);
    }

    private static HongdalExpectedItemDocumentDraft CreateDraft(
        string kind,
        string documentNo,
        string barcode,
        HongdalDocumentPrintLayoutOptions? printLayout = null)
    {
        return new HongdalExpectedItemDocumentDraft
        {
            DocumentNo = documentNo,
            DocumentKind = kind,
            Status = "예정",
            WarehouseName = "테스트 창고",
            OwnerName = "테스트 화주",
            CounterpartyName = "테스트 상대",
            OrderNo = "ORD-TEST-1",
            LedgerNo = "LED-TEST-1",
            ExpectedDateText = "오늘",
            WorkMemo = "테스트",
            PrintLayout = printLayout ?? new HongdalDocumentPrintLayoutOptions(),
            Lines =
            [
                new HongdalExpectedItemDocumentLine
                {
                    LineNo = "L1",
                    Sku = "ABC",
                    ProductName = "테스트 품목",
                    Quantity = 3,
                    ProductBarcode = barcode,
                    BundleBarcode = "BND:TEST-1",
                    LocationCode = "A-01",
                    StorageCondition = "상온",
                    RelatedOrderNo = "ORD-TEST-1"
                }
            ]
        };
    }
}
