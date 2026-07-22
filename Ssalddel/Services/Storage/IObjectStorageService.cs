namespace Ssalddel.Services.Storage;

public enum ObjectStorageAccess
{
    Public,
    Private
}

public interface IObjectStorageService
{
    bool IsConfigured(ObjectStorageAccess access);

    Task<ObjectStorageUploadResult> UploadAsync(
        Stream stream,
        string originalFileName,
        string? contentType,
        string? folder,
        ObjectStorageAccess access,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadAsync(
        string containerName,
        string objectName,
        CancellationToken cancellationToken = default);
}

public sealed record ObjectStorageUploadResult(
    string ContainerName,
    string ObjectName,
    string Url);

internal static class ObjectStorageObjectName
{
    public static string Create(string originalFileName, string? folder)
    {
        var extension = Path.GetExtension(Path.GetFileName(originalFileName));
        var generatedFileName = $"{Guid.NewGuid():N}{extension}";
        var normalizedFolder = NormalizeFolder(folder);

        return string.IsNullOrWhiteSpace(normalizedFolder)
            ? generatedFileName
            : $"{normalizedFolder}/{generatedFileName}";
    }

    private static string NormalizeFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return string.Empty;
        }

        var segments = folder
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Path.GetFileName)
            .Where(segment => !string.IsNullOrWhiteSpace(segment) && segment is not "." and not "..");

        return string.Join('/', segments);
    }
}
