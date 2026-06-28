using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace 홍달.Services.External.Google
{
    public interface IGoogleCloudStorageService
    {
        Task<GoogleCloudStorageUploadResult> UploadAsync(
            Stream stream,
            string originalFileName,
            string? contentType,
            string? folder,
            CancellationToken cancellationToken = default);
    }

    public sealed class GoogleCloudStorageService : IGoogleCloudStorageService
    {
        private readonly GoogleCloudStorageOptions _options;
        private readonly StorageClient _storageClient;

        public GoogleCloudStorageService(IOptions<GoogleCloudStorageOptions> options)
        {
            _options = options.Value;
            _storageClient = CreateStorageClient(_options);
        }

        public async Task<GoogleCloudStorageUploadResult> UploadAsync(
            Stream stream,
            string originalFileName,
            string? contentType,
            string? folder,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.BucketName))
            {
                throw new InvalidOperationException("GoogleCloudStorage:BucketName configuration is required.");
            }

            if (stream == null || !stream.CanRead)
            {
                throw new ArgumentException("Readable stream is required.", nameof(stream));
            }

            var objectName = BuildObjectName(originalFileName, folder);
            var resolvedContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType;

            await _storageClient.UploadObjectAsync(
                bucket: _options.BucketName,
                objectName: objectName,
                contentType: resolvedContentType,
                source: stream,
                cancellationToken: cancellationToken);

            var publicUrl = $"{_options.PublicBaseUrl.TrimEnd('/')}/{_options.BucketName}/{objectName}";
            return new GoogleCloudStorageUploadResult(_options.BucketName, objectName, publicUrl);
        }

        private static StorageClient CreateStorageClient(GoogleCloudStorageOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.ServiceAccountJsonPath))
            {
                var credential = GoogleCredential.FromFile(options.ServiceAccountJsonPath);
                return StorageClient.Create(credential);
            }

            return StorageClient.Create();
        }

        private static string BuildObjectName(string originalFileName, string? folder)
        {
            var safeFileName = Path.GetFileName(originalFileName);
            var extension = Path.GetExtension(safeFileName);
            var generated = $"{Guid.NewGuid():N}{extension}";

            if (string.IsNullOrWhiteSpace(folder))
            {
                return generated;
            }

            var normalizedFolder = folder.Trim().Trim('/').Replace("\\", "/");
            return string.IsNullOrWhiteSpace(normalizedFolder)
                ? generated
                : $"{normalizedFolder}/{generated}";
        }
    }

    public sealed record GoogleCloudStorageUploadResult(string BucketName, string ObjectName, string PublicUrl);
}



