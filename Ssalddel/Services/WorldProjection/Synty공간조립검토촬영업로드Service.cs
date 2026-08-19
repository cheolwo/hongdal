using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.Storage;

namespace Ssalddel.Services.WorldProjection;

public sealed record Synty공간조립검토촬영업로드Command(
    IFormFile? File,
    string? BatchStableId,
    string? ReviewItemStableId,
    string? CaptureStableId,
    string? ViewCode,
    string? CaptureBundleHash,
    string? ParentCaptureBundleHash,
    string? SourceCompositionHash,
    long ExpectedReviewItemRevision,
    string? RenderingProfileHash,
    string? ImageSha256,
    int Width,
    int Height);

public interface ISynty공간조립검토촬영업로드Store
{
    Task<Synty공간조립검토촬영업로드Record?> 조회Async(
        string captureUploadId,
        CancellationToken cancellationToken = default);

    Task<bool> 추가Async(
        Synty공간조립검토촬영업로드Record record,
        CancellationToken cancellationToken = default);
}

public interface ISynty공간조립검토촬영업로드Service
{
    Task<Synty공간조립검토촬영업로드Response> 업로드Async(
        Synty공간조립검토촬영업로드Command command,
        CancellationToken cancellationToken = default);
}

public sealed class Synty공간조립검토촬영업로드Service(
    IObjectStorageService objectStorage,
    ISynty공간조립검토촬영업로드Store store,
    IOptions<ObjectStorageOptions> storageOptions,
    TimeProvider timeProvider) : ISynty공간조립검토촬영업로드Service
{
    public const long MaximumPngBytes = 12 * 1024 * 1024;
    public const int MaximumDimension = 4096;

    public async Task<Synty공간조립검토촬영업로드Response> 업로드Async(
        Synty공간조립검토촬영업로드Command command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.File is null)
        {
            throw new ArgumentException("촬영 PNG 파일이 필요합니다.");
        }
        if (command.File.Length is <= 0 or > MaximumPngBytes)
        {
            throw new ArgumentException($"촬영 PNG는 1바이트 이상 {MaximumPngBytes}바이트 이하여야 합니다.");
        }
        if (!string.IsNullOrWhiteSpace(command.File.ContentType)
            && !string.Equals(command.File.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("공간 조립 검토 촬영은 PNG만 허용합니다.");
        }

        var batchStableId = Require(command.BatchStableId, nameof(command.BatchStableId), 160);
        var reviewItemStableId = Require(command.ReviewItemStableId, nameof(command.ReviewItemStableId), 160);
        var captureStableId = Require(command.CaptureStableId, nameof(command.CaptureStableId), 180);
        var viewCode = Require(command.ViewCode, nameof(command.ViewCode), 80);
        var captureBundleHash = RequireSha256(command.CaptureBundleHash, nameof(command.CaptureBundleHash));
        var parentCaptureBundleHash = OptionalSha256(
            command.ParentCaptureBundleHash,
            nameof(command.ParentCaptureBundleHash));
        var sourceCompositionHash = RequireSha256(
            command.SourceCompositionHash,
            nameof(command.SourceCompositionHash));
        if (command.ExpectedReviewItemRevision < 0)
        {
            throw new ArgumentException("ExpectedReviewItemRevision은 0 이상이어야 합니다.");
        }
        var renderingProfileHash = RequireSha256(command.RenderingProfileHash, nameof(command.RenderingProfileHash));
        var expectedImageHash = RequireSha256(command.ImageSha256, nameof(command.ImageSha256));
        if (command.Width is <= 0 or > MaximumDimension || command.Height is <= 0 or > MaximumDimension)
        {
            throw new ArgumentException($"촬영 해상도는 각 축 1~{MaximumDimension}px 범위여야 합니다.");
        }

        await using var buffer = new MemoryStream((int)Math.Min(command.File.Length, MaximumPngBytes));
        await command.File.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length is <= 0 or > MaximumPngBytes)
        {
            throw new ArgumentException($"촬영 PNG는 1바이트 이상 {MaximumPngBytes}바이트 이하여야 합니다.");
        }

        var sourceBytes = buffer.ToArray();
        var (actualWidth, actualHeight) = ReadPngSize(sourceBytes);
        if (actualWidth != command.Width || actualHeight != command.Height)
        {
            throw new ArgumentException(
                $"Unity가 제출한 해상도와 PNG IHDR가 다릅니다. Expected={command.Width}x{command.Height}, Actual={actualWidth}x{actualHeight}");
        }
        var uploadedSourceHash = Convert.ToHexString(SHA256.HashData(sourceBytes)).ToLowerInvariant();
        if (!string.Equals(uploadedSourceHash, expectedImageHash, StringComparison.Ordinal))
        {
            throw new ArgumentException("Unity가 제출한 ImageSha256과 실제 PNG hash가 다릅니다.");
        }

        var storedBytes = ReencodeSafePng(sourceBytes, actualWidth, actualHeight);
        var storedImageHash = Convert.ToHexString(SHA256.HashData(storedBytes)).ToLowerInvariant();

        var captureUploadId = "capture-upload:" + Sha256(string.Join('|',
            batchStableId,
            reviewItemStableId,
            captureStableId,
            viewCode,
            captureBundleHash,
            parentCaptureBundleHash,
            sourceCompositionHash,
            command.ExpectedReviewItemRevision,
            renderingProfileHash,
            uploadedSourceHash,
            storedImageHash));
        var existing = await store.조회Async(captureUploadId, cancellationToken);
        if (existing is not null)
        {
            return ToResponse(existing);
        }

        var folder = string.Join('/',
            "world-composition-reviews",
            Sha256(batchStableId)[..16],
            Sha256(reviewItemStableId)[..16],
            captureBundleHash[..16]);
        var objectName = $"{folder}/{captureUploadId["capture-upload:".Length..]}.png";
        await using var storedStream = new MemoryStream(storedBytes, writable: false);
        var uploaded = await objectStorage.UploadImmutableAsync(
            storedStream,
            objectName,
            "image/png",
            ObjectStorageAccess.Public,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["captureUploadStableId"] = captureUploadId,
                ["imageSha256"] = storedImageHash,
                ["createdAtUtc"] = timeProvider.GetUtcNow().UtcDateTime.ToString("O")
            },
            cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var record = new Synty공간조립검토촬영업로드Record
        {
            CaptureUploadId = captureUploadId,
            BatchStableId = batchStableId,
            ReviewItemStableId = reviewItemStableId,
            CaptureStableId = captureStableId,
            ViewCode = viewCode,
            CaptureBundleHash = captureBundleHash,
            ParentCaptureBundleHash = parentCaptureBundleHash,
            SourceCompositionHash = sourceCompositionHash,
            ExpectedReviewItemRevision = command.ExpectedReviewItemRevision,
            RenderingProfileHash = renderingProfileHash,
            StorageProviderCode = storageOptions.Value.Provider?.Trim() ?? string.Empty,
            ContainerName = uploaded.ContainerName,
            ObjectName = uploaded.ObjectName,
            ImageUrl = uploaded.Url,
            UploadedSourceSha256 = uploadedSourceHash,
            StoredImageSha256 = storedImageHash,
            ContentType = "image/png",
            ContentLength = storedBytes.LongLength,
            ETag = uploaded.ETag,
            Width = actualWidth,
            Height = actualHeight,
            UploadedAtUtc = now
        };
        if (!await store.추가Async(record, cancellationToken))
        {
            return ToResponse(await store.조회Async(captureUploadId, cancellationToken)
                              ?? throw new InvalidOperationException("촬영 업로드 영수증 저장 충돌을 복구하지 못했습니다."));
        }
        return ToResponse(record);
    }

    private static (int Width, int Height) ReadPngSize(ReadOnlySpan<byte> bytes)
    {
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (bytes.Length < 24
            || !bytes[..8].SequenceEqual(signature)
            || !bytes.Slice(12, 4).SequenceEqual("IHDR"u8))
        {
            throw new ArgumentException("유효한 PNG signature와 IHDR가 필요합니다.");
        }
        var width = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(20, 4));
        if (width is <= 0 or > MaximumDimension || height is <= 0 or > MaximumDimension)
        {
            throw new ArgumentException($"PNG 해상도는 각 축 1~{MaximumDimension}px 범위여야 합니다.");
        }
        return (width, height);
    }

    private static byte[] ReencodeSafePng(byte[] sourceBytes, int expectedWidth, int expectedHeight)
    {
        using var bitmap = SKBitmap.Decode(sourceBytes)
                           ?? throw new ArgumentException("PNG pixel 자료를 해석할 수 없습니다.");
        if (bitmap.Width != expectedWidth || bitmap.Height != expectedHeight)
        {
            throw new ArgumentException("PNG IHDR와 실제 pixel 해상도가 다릅니다.");
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100)
                            ?? throw new InvalidOperationException("안전한 PNG 재인코딩에 실패했습니다.");
        return encoded.ToArray();
    }

    private static string Require(string? value, string name, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0 || normalized.Length > maxLength)
        {
            throw new ArgumentException($"{name}에는 1~{maxLength}자 값이 필요합니다.");
        }
        return normalized;
    }

    private static string RequireSha256(string? value, string name)
    {
        var normalized = Require(value, name, 64).ToLowerInvariant();
        if (normalized.Length != 64 || normalized.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException($"{name}에는 64자리 SHA-256 hex가 필요합니다.");
        }
        return normalized;
    }

    private static string OptionalSha256(string? value, string name)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : RequireSha256(value, name);

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    internal static Synty공간조립검토촬영업로드Response ToResponse(
        Synty공간조립검토촬영업로드Record record)
        => new()
        {
            CaptureUploadId = record.CaptureUploadId,
            BatchStableId = record.BatchStableId,
            ReviewItemStableId = record.ReviewItemStableId,
            CaptureStableId = record.CaptureStableId,
            ViewCode = record.ViewCode,
            CaptureBundleHash = record.CaptureBundleHash,
            ParentCaptureBundleHash = record.ParentCaptureBundleHash,
            SourceCompositionHash = record.SourceCompositionHash,
            ExpectedReviewItemRevision = record.ExpectedReviewItemRevision,
            RenderingProfileHash = record.RenderingProfileHash,
            StorageProviderCode = record.StorageProviderCode,
            ContainerName = record.ContainerName,
            ObjectName = record.ObjectName,
            ImageUrl = record.ImageUrl,
            UploadedSourceSha256 = record.UploadedSourceSha256,
            StoredImageSha256 = record.StoredImageSha256,
            ContentType = record.ContentType,
            ContentLength = record.ContentLength,
            ETag = record.ETag,
            Width = record.Width,
            Height = record.Height,
            UploadedAtUtc = record.UploadedAtUtc
        };
}
