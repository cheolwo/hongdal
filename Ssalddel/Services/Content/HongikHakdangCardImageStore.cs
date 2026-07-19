using System.Security.Cryptography;
using Ssalddel.Services.External.HongikHakdang;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Content;

public sealed record HongikHakdangStoredImage(
    string RelativePath,
    string ContentType,
    long SizeBytes,
    string Sha256);

public interface IHongikHakdangCardImageStore
{
    bool Exists(string? relativePath);

    Task<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken);

    Task<HongikHakdangStoredImage> SaveAsync(
        string sourceKey,
        HongikHakdangCardImageContent content,
        CancellationToken cancellationToken);

    Task<HongikHakdangStoredImage> SaveVariantAsync(
        string variantKind,
        byte[] bytes,
        CancellationToken cancellationToken);
}

public sealed class HongikHakdangCardImageStore : IHongikHakdangCardImageStore
{
    private readonly string _contentRootPath;
    private readonly string _storageRootPath;
    private readonly string _storageRelativePath;

    public HongikHakdangCardImageStore(
        IHostEnvironment environment,
        IOptions<HongikHakdangCardOptions> options)
    {
        _contentRootPath = Path.GetFullPath(environment.ContentRootPath);
        _storageRelativePath = NormalizeStorageFolder(options.Value.StorageFolder);
        _storageRootPath = Path.GetFullPath(Path.Combine(_contentRootPath, _storageRelativePath));
        EnsureInsideContentRoot(_storageRootPath);
    }

    public bool Exists(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(Path.Combine(_contentRootPath, relativePath));
        EnsureInsideContentRoot(fullPath);
        return File.Exists(fullPath);
    }

    public async Task<HongikHakdangStoredImage> SaveAsync(
        string sourceKey,
        HongikHakdangCardImageContent content,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentNullException.ThrowIfNull(content);
        if (content.Bytes.Length == 0)
        {
            throw new InvalidOperationException("저장할 카드 이미지가 비어 있습니다.");
        }

        var extension = ResolveExtension(sourceKey, content.ContentType);
        return await SaveBytesAsync(
            Path.Combine(_storageRelativePath, Convert.ToHexString(SHA256.HashData(content.Bytes)).ToLowerInvariant()[..2]),
            content.Bytes,
            extension,
            NormalizeContentType(content.ContentType, extension),
            cancellationToken);
    }

    public async Task<byte[]> ReadAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        var fullPath = Path.GetFullPath(Path.Combine(_contentRootPath, relativePath));
        EnsureInsideContentRoot(fullPath);
        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public async Task<HongikHakdangStoredImage> SaveVariantAsync(
        string variantKind,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variantKind);
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length == 0)
        {
            throw new InvalidOperationException("저장할 카드 파생 이미지가 비어 있습니다.");
        }

        var safeKind = new string(variantKind
            .Where(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_')
            .ToArray());
        if (safeKind.Length == 0 || safeKind.Length > 60)
        {
            throw new InvalidOperationException("카드 파생 이미지 종류가 올바르지 않습니다.");
        }

        return await SaveBytesAsync(
            Path.Combine(_storageRelativePath, "variants", safeKind.ToLowerInvariant()),
            bytes,
            ".jpg",
            "image/jpeg",
            cancellationToken);
    }

    private async Task<HongikHakdangStoredImage> SaveBytesAsync(
        string relativeFolder,
        byte[] bytes,
        string extension,
        string contentType,
        CancellationToken cancellationToken)
    {
        var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var relativePath = Path.Combine(relativeFolder, $"{sha256}{extension}");
        var fullPath = Path.GetFullPath(Path.Combine(_contentRootPath, relativePath));
        EnsureInsideContentRoot(fullPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        if (!File.Exists(fullPath))
        {
            var tempPath = $"{fullPath}.{Guid.NewGuid():N}.tmp";
            try
            {
                await File.WriteAllBytesAsync(tempPath, bytes, cancellationToken);
                File.Move(tempPath, fullPath, overwrite: false);
            }
            catch (IOException) when (File.Exists(fullPath))
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        return new HongikHakdangStoredImage(
            relativePath.Replace(Path.DirectorySeparatorChar, '/'),
            contentType,
            bytes.LongLength,
            sha256);
    }

    private static string NormalizeStorageFolder(string value)
    {
        var folder = string.IsNullOrWhiteSpace(value)
            ? "App_Data/hongik-hakdang-cards"
            : value.Trim();
        if (Path.IsPathRooted(folder) || folder.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("카드 이미지 저장 폴더는 콘텐츠 루트 아래의 상대 경로여야 합니다.");
        }

        return folder.Replace('/', Path.DirectorySeparatorChar);
    }

    private static string ResolveExtension(string sourceKey, string? contentType)
    {
        var extension = Path.GetExtension(sourceKey).ToLowerInvariant();
        if (extension is ".jpg" or ".jpeg" or ".png" or ".webp")
        {
            return extension == ".jpeg" ? ".jpg" : extension;
        }

        return contentType?.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }

    private static string NormalizeContentType(string? contentType, string extension)
        => contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/png" or "image/webp" => contentType.ToLowerInvariant(),
            _ => extension switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            }
        };

    private void EnsureInsideContentRoot(string fullPath)
    {
        var rootWithSeparator = _contentRootPath.TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fullPath, _contentRootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("카드 이미지 저장 경로가 콘텐츠 루트 밖을 가리킵니다.");
        }
    }
}
