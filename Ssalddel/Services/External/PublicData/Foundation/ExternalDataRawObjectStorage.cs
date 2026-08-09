using System.Security.Cryptography;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using Ssalddel.Services.Storage;

namespace 살뜰.Services.External.PublicData;

/// <summary>
/// 원자료 본문은 private object storage에 두고 DB에는 hash와 위치 metadata만 남깁니다.
/// </summary>
public sealed class ExternalDataRawObjectStorage : IExternalDataRawStorage
{
    private readonly IObjectStorageService objectStorage;

    public ExternalDataRawObjectStorage(IObjectStorageService objectStorage)
        => this.objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));

    public async Task<ExternalDataRawStorageResult> StoreAsync(
        ExternalDataSourceDefinition source,
        ExternalDataCollectedPayload payload,
        DateTimeOffset collectedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(payload);
        if (!objectStorage.IsConfigured(ObjectStorageAccess.Private))
            throw new InvalidOperationException("ExternalDataPrivateObjectStorageNotConfigured");

        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "ssalddel-external-data");
        Directory.CreateDirectory(temporaryDirectory);
        var temporaryPath = Path.Combine(temporaryDirectory, Guid.NewGuid().ToString("N") + ".raw");
        try
        {
            long length = 0;
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using (var output = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             81920,
                             FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var buffer = new byte[81920];
                int read;
                while ((read = await payload.Content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    hash.AppendData(buffer, 0, read);
                    length += read;
                }
            }

            await using var staged = new FileStream(
                temporaryPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var folder = string.Join('/',
                "external-data",
                "raw",
                SafeSegment(source.SourceId),
                SafeSegment(source.DatasetId),
                collectedAtUtc.UtcDateTime.ToString("yyyy/MM/dd"));
            var uploaded = await objectStorage.UploadAsync(
                staged,
                payload.OriginalFileName,
                payload.ContentType,
                folder,
                ObjectStorageAccess.Private,
                cancellationToken);
            return new ExternalDataRawStorageResult(
                Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(),
                length,
                uploaded.ContainerName,
                uploaded.ObjectName,
                uploaded.Url);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    public async Task<Stream> OpenReadAsync(
        외부데이터RawSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var content = await objectStorage.DownloadAsync(
            snapshot.StorageContainer,
            snapshot.StorageObjectName,
            cancellationToken);
        return new MemoryStream(content, writable: false);
    }

    private static string SafeSegment(string value)
    {
        var chars = value.Trim().Select(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' or '.' ? character : '-');
        var normalized = new string(chars.ToArray()).Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
    }
}
