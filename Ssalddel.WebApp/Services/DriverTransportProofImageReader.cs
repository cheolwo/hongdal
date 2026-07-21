using Microsoft.AspNetCore.Components.Forms;

namespace Ssalddel.WebApp.Services;

public sealed record DriverTransportProofImage(
    string PreviewUrl,
    string FileName,
    string ContentType,
    byte[] Bytes);

public static class DriverTransportProofImageReader
{
    public const long MaxImageBytes = 5 * 1024 * 1024;

    public static async Task<DriverTransportProofImage> ReadAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        await using var stream = file.OpenReadStream(MaxImageBytes, cancellationToken);
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "image/png"
            : file.ContentType;
        var bytes = memory.ToArray();
        return new DriverTransportProofImage(
            $"data:{contentType};base64,{Convert.ToBase64String(bytes)}",
            file.Name,
            contentType,
            bytes);
    }
}
