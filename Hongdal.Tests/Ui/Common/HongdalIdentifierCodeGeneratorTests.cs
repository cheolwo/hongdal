using Hongdal.Contracts.Common.Identifiers;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Tests.Ui.Common;

public sealed class HongdalIdentifierCodeGeneratorTests
{
    [Theory]
    [InlineData(HongdalMachineReadableCodeFormatCode.QrCode)]
    [InlineData(HongdalMachineReadableCodeFormatCode.Code128)]
    public void GenerateReturnsSvgDataUri(string format)
    {
        var generator = new ZxingHongdalIdentifierCodeGenerator();
        var payload = HongdalIdentifierCodePayloads.Create(HongdalIdentifierKindCode.Order, "ORD-20260710-0001");

        var image = generator.Generate(new HongdalIdentifierCodeImageRequest(payload, format, 220, 120, 1));

        Assert.Equal(payload.RawCode, image.RawCode);
        Assert.Contains("<svg", image.SvgMarkup, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("data:image/svg+xml;base64,", image.DataUri);
    }
}
