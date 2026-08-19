using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

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

    Task<ObjectStorageUploadResult> UploadImmutableAsync(
        Stream stream,
        string objectName,
        string? contentType,
        ObjectStorageAccess access,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadAsync(
        string containerName,
        string objectName,
        CancellationToken cancellationToken = default);
}

public sealed record ObjectStorageUploadResult(
    string ContainerName,
    string ObjectName,
    string Url,
    string ETag = "");

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

    public static string NormalizeProvided(string objectName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectName);
        var segments = objectName
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Path.GetFileName)
            .Where(segment => !string.IsNullOrWhiteSpace(segment) && segment is not "." and not "..")
            .ToArray();
        var normalized = string.Join('/', segments);
        if (normalized.Length == 0 || normalized.Length > 1024)
        {
            throw new ArgumentException("Object storage object name is invalid.", nameof(objectName));
        }
        return normalized;
    }

    private static string NormalizeFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return string.Empty;
        }
        return string.Join('/', folder
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Path.GetFileName)
            .Where(segment => !string.IsNullOrWhiteSpace(segment) && segment is not "." and not ".."));
    }
}

public sealed class DevelopmentLocalStorageService : IObjectStorageService
{
    public const string PublicContainerName = "development-local";
    public const string PrivateContainerName = "development-private";
    public const string PublicRequestPath = "/local-storage";
    public const string PublicStorageDirectoryName = ".local-storage";
    public const string PrivateStorageDirectoryName = ".local-private-storage";

    private readonly string publicStorageRoot;
    private readonly string privateStorageRoot;
    private readonly IHttpContextAccessor httpContextAccessor;

    public DevelopmentLocalStorageService(
        IHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        publicStorageRoot = Path.GetFullPath(
            Path.Combine(environment.ContentRootPath, PublicStorageDirectoryName));
        privateStorageRoot = Path.GetFullPath(
            Path.Combine(environment.ContentRootPath, PrivateStorageDirectoryName));
        this.httpContextAccessor = httpContextAccessor;
    }

    public bool IsConfigured(ObjectStorageAccess access) => true;

    public async Task<ObjectStorageUploadResult> UploadAsync(
        Stream stream,
        string originalFileName,
        string? contentType,
        string? folder,
        ObjectStorageAccess access,
        CancellationToken cancellationToken = default)
    {
        var objectName = ObjectStorageObjectName.Create(originalFileName, folder);
        return await UploadImmutableAsync(
            stream,
            objectName,
            contentType,
            access,
            cancellationToken: cancellationToken);
    }

    public async Task<ObjectStorageUploadResult> UploadImmutableAsync(
        Stream stream,
        string objectName,
        string? contentType,
        ObjectStorageAccess access,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (stream is null || !stream.CanRead)
        {
            throw new ArgumentException("Readable stream is required.", nameof(stream));
        }
        var containerName = ResolveContainerName(access);
        var normalizedObjectName = ObjectStorageObjectName.NormalizeProvided(objectName);
        var filePath = ResolveStoragePath(containerName, normalizedObjectName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        try
        {
            await using var output = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);
            await stream.CopyToAsync(output, cancellationToken);
        }
        catch (IOException) when (File.Exists(filePath))
        {
            // 같은 불변 object 재시도는 기존 파일을 유지한다.
        }

        var url = access == ObjectStorageAccess.Public
            ? BuildPublicUrl(normalizedObjectName)
            : $"local-storage-private://{containerName}/{EncodeObjectName(normalizedObjectName)}";
        return new ObjectStorageUploadResult(containerName, normalizedObjectName, url);
    }

    public Task<byte[]> DownloadAsync(
        string containerName,
        string objectName,
        CancellationToken cancellationToken = default)
        => File.ReadAllBytesAsync(ResolveStoragePath(containerName, objectName), cancellationToken);

    private string BuildPublicUrl(string objectName)
    {
        var relativeUrl = $"{PublicRequestPath}/{EncodeObjectName(objectName)}";
        var request = httpContextAccessor.HttpContext?.Request;
        return request is null || !request.Host.HasValue
            ? relativeUrl
            : $"{request.Scheme}://{request.Host}{request.PathBase}{relativeUrl}";
    }

    private static string EncodeObjectName(string objectName)
        => string.Join('/', objectName.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));

    private string ResolveStoragePath(string containerName, string objectName)
    {
        var storageRoot = containerName switch
        {
            PublicContainerName => publicStorageRoot,
            PrivateContainerName => privateStorageRoot,
            _ => throw new FileNotFoundException($"Unknown local storage container: {containerName}")
        };
        var relativePath = objectName.Replace('/', Path.DirectorySeparatorChar);
        var resolvedPath = Path.GetFullPath(Path.Combine(storageRoot, relativePath));
        var rootPrefix = storageRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!resolvedPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage object path must remain inside the local storage root.");
        }
        return resolvedPath;
    }

    private static string ResolveContainerName(ObjectStorageAccess access)
        => access == ObjectStorageAccess.Public ? PublicContainerName : PrivateContainerName;
}
