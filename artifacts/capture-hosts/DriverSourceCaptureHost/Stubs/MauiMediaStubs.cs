namespace Microsoft.Maui.Media;

public sealed class MediaPickerOptions
{
    public string? Title { get; set; }
}

public sealed class FileResult
{
    public FileResult(string fileName, string contentType, byte[] bytes)
    {
        FileName = fileName;
        ContentType = contentType;
        this.bytes = bytes;
    }

    private readonly byte[] bytes;

    public string FileName { get; }

    public string? ContentType { get; }

    public Task<Stream> OpenReadAsync()
        => Task.FromResult<Stream>(new MemoryStream(bytes));
}

public sealed class MediaPicker
{
    public static MediaPicker Default { get; } = new();

    public bool IsCaptureSupported => false;

    public Task<FileResult?> CapturePhotoAsync(MediaPickerOptions options)
        => Task.FromResult<FileResult?>(null);
}
