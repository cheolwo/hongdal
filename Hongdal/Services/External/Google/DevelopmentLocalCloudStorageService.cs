using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace 홍달.Services.External.Google;

public sealed class DevelopmentLocalCloudStorageService : IGoogleCloudStorageService
{
    public const string BucketName = "development-local";
    public const string RequestPath = "/local-storage";
    public const string StorageDirectoryName = ".local-storage";

    private readonly string _storageRoot;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DevelopmentLocalCloudStorageService(
        IHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _storageRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, StorageDirectoryName));
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GoogleCloudStorageUploadResult> UploadAsync(
        Stream stream,
        string originalFileName,
        string? contentType,
        string? folder,
        CancellationToken cancellationToken = default)
    {
        if (stream is null || !stream.CanRead)
        {
            throw new ArgumentException("Readable stream is required.", nameof(stream));
        }

        var objectName = BuildObjectName(originalFileName, folder);
        var filePath = ResolveStoragePath(objectName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using (var output = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true))
        {
            await stream.CopyToAsync(output, cancellationToken);
        }

        return new GoogleCloudStorageUploadResult(BucketName, objectName, BuildPublicUrl(objectName));
    }

    public Task<byte[]> DownloadAsync(
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(bucketName, BucketName, StringComparison.Ordinal))
        {
            throw new FileNotFoundException($"Unknown development storage bucket: {bucketName}");
        }

        return File.ReadAllBytesAsync(ResolveStoragePath(objectName), cancellationToken);
    }

    private string BuildPublicUrl(string objectName)
    {
        var encodedObjectName = string.Join(
            '/',
            objectName.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.EscapeDataString));
        var relativeUrl = $"{RequestPath}/{encodedObjectName}";
        var request = _httpContextAccessor.HttpContext?.Request;

        return request is null || !request.Host.HasValue
            ? relativeUrl
            : $"{request.Scheme}://{request.Host}{request.PathBase}{relativeUrl}";
    }

    private string ResolveStoragePath(string objectName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectName);

        var relativePath = objectName.Replace('/', Path.DirectorySeparatorChar);
        var resolvedPath = Path.GetFullPath(Path.Combine(_storageRoot, relativePath));
        var rootPrefix = _storageRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!resolvedPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage object path must remain inside the development storage root.");
        }

        return resolvedPath;
    }

    private static string BuildObjectName(string originalFileName, string? folder)
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
