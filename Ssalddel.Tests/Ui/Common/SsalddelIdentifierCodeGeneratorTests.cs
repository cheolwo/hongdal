using Ssalddel.Contracts.Common.Identifiers;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class SsalddelIdentifierCodeGeneratorTests
{
    [Theory]
    [InlineData(SsalddelMachineReadableCodeFormatCode.QrCode)]
    [InlineData(SsalddelMachineReadableCodeFormatCode.Code128)]
    public void GenerateReturnsSvgDataUri(string format)
    {
        var generator = new ZxingSsalddelIdentifierCodeGenerator();
        var payload = SsalddelIdentifierCodePayloads.Create(SsalddelIdentifierKindCode.Order, "ORD-20260710-0001");

        var image = generator.Generate(new SsalddelIdentifierCodeImageRequest(payload, format, 220, 120, 1));

        Assert.Equal(payload.RawCode, image.RawCode);
        Assert.Contains("<svg", image.SvgMarkup, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("data:image/svg+xml;base64,", image.DataUri);
    }
}
