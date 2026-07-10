using System.Text;
using Hongdal.Contracts.Common.Identifiers;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed record HongdalIdentifierCodeImageRequest(
    HongdalIdentifierCodePayload Payload,
    string Format = HongdalMachineReadableCodeFormatCode.QrCode,
    int Width = 240,
    int Height = 240,
    int Margin = 2);

public sealed record HongdalIdentifierCodeImage(
    string Format,
    string RawCode,
    string SvgMarkup,
    string DataUri);

public interface IHongdalIdentifierCodeGenerator
{
    HongdalIdentifierCodeImage Generate(HongdalIdentifierCodeImageRequest request);
}

public sealed class ZxingHongdalIdentifierCodeGenerator : IHongdalIdentifierCodeGenerator
{
    public HongdalIdentifierCodeImage Generate(HongdalIdentifierCodeImageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Payload.RawCode);

        var format = NormalizeFormat(request.Format);
        var writer = new BarcodeWriterSvg
        {
            Format = ResolveBarcodeFormat(format),
            Options = CreateOptions(format, request.Width, request.Height, request.Margin)
        };

        var svgMarkup = writer.Write(request.Payload.RawCode).Content;
        return new HongdalIdentifierCodeImage(
            format,
            request.Payload.RawCode,
            svgMarkup,
            $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svgMarkup))}");
    }

    private static EncodingOptions CreateOptions(string format, int width, int height, int margin)
    {
        var normalizedWidth = Math.Clamp(width, 96, 1600);
        var normalizedHeight = Math.Clamp(height, 96, 1600);
        var normalizedMargin = Math.Clamp(margin, 0, 8);

        if (format == HongdalMachineReadableCodeFormatCode.QrCode)
        {
            return new QrCodeEncodingOptions
            {
                Width = normalizedWidth,
                Height = normalizedHeight,
                Margin = normalizedMargin,
                CharacterSet = "UTF-8"
            };
        }

        return new EncodingOptions
        {
            Width = normalizedWidth,
            Height = normalizedHeight,
            Margin = normalizedMargin
        };
    }

    private static BarcodeFormat ResolveBarcodeFormat(string format)
    {
        return format switch
        {
            HongdalMachineReadableCodeFormatCode.QrCode => BarcodeFormat.QR_CODE,
            HongdalMachineReadableCodeFormatCode.Code128 => BarcodeFormat.CODE_128,
            _ => throw new InvalidOperationException($"Unsupported machine-readable code format: {format}")
        };
    }

    private static string NormalizeFormat(string format)
    {
        return (format ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "qr" or "qrcode" or "qr-code" => HongdalMachineReadableCodeFormatCode.QrCode,
            "code128" or "code-128" or "barcode" or "bar-code" => HongdalMachineReadableCodeFormatCode.Code128,
            _ => HongdalMachineReadableCodeFormatCode.QrCode
        };
    }
}
