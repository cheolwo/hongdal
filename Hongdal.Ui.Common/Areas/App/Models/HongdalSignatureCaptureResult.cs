namespace Hongdal.Ui.Common.Areas.App.Models;

public sealed record HongdalSignatureCaptureResult(
    string SignerName,
    string SignatureDataUrl,
    DateTimeOffset CapturedAtUtc,
    int StrokeCount,
    int CanvasWidth,
    int CanvasHeight)
{
    public bool HasSignature => !string.IsNullOrWhiteSpace(SignatureDataUrl) && StrokeCount > 0;
}

public sealed class HongdalSignatureCanvasState
{
    public bool IsEmpty { get; set; } = true;

    public string DataUrl { get; set; } = string.Empty;

    public int StrokeCount { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}
