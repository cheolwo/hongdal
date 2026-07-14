using Hongdal.Domain.Content;
using Hongdal.Services.Content;
using Microsoft.Extensions.Options;
using SkiaSharp;
using 홍달.Services.Options;

namespace Hongdal.Tests.Services.Content;

public sealed class HongikHakdangCardVariantRendererTests
{
    [Theory]
    [InlineData(HongikHakdangCardImageVariantKinds.Notification, 1024, 576, 900_000)]
    [InlineData(HongikHakdangCardImageVariantKinds.LockScreenPortrait, 1080, 2400, 2_500_000)]
    public void Render_CreatesExpectedJpegVariant(
        string variantKind,
        int expectedWidth,
        int expectedHeight,
        int maxBytes)
    {
        var renderer = new HongikHakdangCardVariantRenderer(
            Options.Create(new HongikHakdangCardOptions
            {
                NotificationImageMaxBytes = 900_000,
                LockScreenImageMaxBytes = 2_500_000
            }));
        var sourceBytes = CreateSourceImage();

        var rendered = renderer.Render(variantKind, sourceBytes);

        Assert.Equal(expectedWidth, rendered.Width);
        Assert.Equal(expectedHeight, rendered.Height);
        Assert.InRange(rendered.Bytes.Length, 1, maxBytes);
        using var decoded = SKBitmap.Decode(rendered.Bytes);
        Assert.NotNull(decoded);
        Assert.Equal(expectedWidth, decoded.Width);
        Assert.Equal(expectedHeight, decoded.Height);
    }

    private static byte[] CreateSourceImage()
    {
        using var bitmap = new SKBitmap(720, 1080, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(238, 229, 207));
        using var accent = new SKPaint { Color = new SKColor(124, 70, 42), IsAntialias = true };
        canvas.DrawCircle(360, 400, 230, accent);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }
}
