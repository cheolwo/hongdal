using Ssalddel.Domain.Content;
using SkiaSharp;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Content;

public sealed record HongikHakdangRenderedVariant(
    string VariantKind,
    int Width,
    int Height,
    byte[] Bytes);

public interface IHongikHakdangCardVariantRenderer
{
    HongikHakdangRenderedVariant Render(string variantKind, byte[] sourceBytes);
}

public sealed class HongikHakdangCardVariantRenderer : IHongikHakdangCardVariantRenderer
{
    private static readonly SKSamplingOptions ImageSampling =
        new(SKFilterMode.Linear, SKMipmapMode.Linear);

    private readonly HongikHakdangCardOptions _options;

    public HongikHakdangCardVariantRenderer(
        Microsoft.Extensions.Options.IOptions<HongikHakdangCardOptions> options)
    {
        _options = options.Value;
    }

    public HongikHakdangRenderedVariant Render(string variantKind, byte[] sourceBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variantKind);
        ArgumentNullException.ThrowIfNull(sourceBytes);

        var specification = variantKind switch
        {
            HongikHakdangCardImageVariantKinds.Notification =>
                new VariantSpecification(1024, 576, 40, 40, 40, 40, _options.NotificationImageMaxBytes),
            HongikHakdangCardImageVariantKinds.LockScreenPortrait =>
                new VariantSpecification(1080, 2400, 72, 460, 72, 220, _options.LockScreenImageMaxBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(variantKind), variantKind, "지원하지 않는 카드 이미지 종류입니다.")
        };

        using var source = SKBitmap.Decode(sourceBytes)
            ?? throw new InvalidOperationException("카드 원본 이미지를 해석할 수 없습니다.");
        using var surface = SKSurface.Create(new SKImageInfo(
            specification.Width,
            specification.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Premul))
            ?? throw new InvalidOperationException("카드 파생 이미지 캔버스를 만들 수 없습니다.");

        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(17, 24, 39));

        using (var backgroundPaint = new SKPaint { Color = new SKColor(255, 255, 255, 95), IsAntialias = true })
        {
            canvas.DrawBitmap(
                source,
                CalculateCoverRect(source.Width, source.Height, specification.Width, specification.Height),
                ImageSampling,
                backgroundPaint);
        }

        using (var shadePaint = new SKPaint { Color = new SKColor(8, 15, 30, 198) })
        {
            canvas.DrawRect(0, 0, specification.Width, specification.Height, shadePaint);
        }

        var safeArea = new SKRect(
            specification.Left,
            specification.Top,
            specification.Width - specification.Right,
            specification.Height - specification.Bottom);
        var imageRect = CalculateContainRect(source.Width, source.Height, safeArea);

        using (var shadowPaint = new SKPaint { Color = new SKColor(0, 0, 0, 115), IsAntialias = true })
        {
            var shadowRect = imageRect;
            shadowRect.Offset(0, variantKind == HongikHakdangCardImageVariantKinds.LockScreenPortrait ? 22 : 10);
            canvas.DrawRoundRect(shadowRect, 34, 34, shadowPaint);
        }

        using var clipRoundRect = new SKRoundRect(imageRect, 30, 30);
        canvas.Save();
        canvas.ClipRoundRect(clipRoundRect, SKClipOperation.Intersect, antialias: true);
        using (var imagePaint = new SKPaint { IsAntialias = true })
        {
            canvas.DrawBitmap(source, imageRect, ImageSampling, imagePaint);
        }
        canvas.Restore();

        using var borderPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 54),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };
        canvas.DrawRoundRect(imageRect, 30, 30, borderPaint);
        canvas.Flush();

        using var image = surface.Snapshot();
        var bytes = EncodeWithinLimit(image, specification.MaxBytes);
        return new HongikHakdangRenderedVariant(
            variantKind,
            specification.Width,
            specification.Height,
            bytes);
    }

    private static byte[] EncodeWithinLimit(SKImage image, int configuredMaxBytes)
    {
        var maxBytes = Math.Clamp(configuredMaxBytes, 100_000, 10_000_000);
        byte[]? smallest = null;
        for (var quality = 86; quality >= 42; quality -= 6)
        {
            using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, quality)
                ?? throw new InvalidOperationException("카드 파생 이미지를 JPEG로 변환할 수 없습니다.");
            var bytes = encoded.ToArray();
            smallest = bytes;
            if (bytes.Length <= maxBytes)
            {
                return bytes;
            }
        }

        return smallest ?? throw new InvalidOperationException("카드 파생 이미지 인코딩 결과가 없습니다.");
    }

    private static SKRect CalculateCoverRect(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        var scale = Math.Max((float)targetWidth / sourceWidth, (float)targetHeight / sourceHeight);
        var width = sourceWidth * scale;
        var height = sourceHeight * scale;
        return new SKRect(
            (targetWidth - width) / 2,
            (targetHeight - height) / 2,
            (targetWidth + width) / 2,
            (targetHeight + height) / 2);
    }

    private static SKRect CalculateContainRect(int sourceWidth, int sourceHeight, SKRect bounds)
    {
        var scale = Math.Min(bounds.Width / sourceWidth, bounds.Height / sourceHeight);
        var width = sourceWidth * scale;
        var height = sourceHeight * scale;
        return new SKRect(
            bounds.MidX - width / 2,
            bounds.MidY - height / 2,
            bounds.MidX + width / 2,
            bounds.MidY + height / 2);
    }

    private sealed record VariantSpecification(
        int Width,
        int Height,
        int Left,
        int Top,
        int Right,
        int Bottom,
        int MaxBytes);
}
