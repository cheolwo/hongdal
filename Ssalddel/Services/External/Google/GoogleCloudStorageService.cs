using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using Ssalddel.Services.Storage;

namespace 살뜰.Services.External.Google
{
    public sealed class GoogleCloudStorageService : IObjectStorageService
    {
        private readonly GoogleCloudStorageOptions _options;
        private readonly Lazy<StorageClient> _storageClient;

        public GoogleCloudStorageService(IOptions<GoogleCloudStorageOptions> options)
        {
            _options = options.Value;
            _storageClient = new Lazy<StorageClient>(
                () => CreateStorageClient(_options),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public bool IsConfigured(ObjectStorageAccess access)
            => !string.IsNullOrWhiteSpace(ResolveBucketName(access));

        public async Task<ObjectStorageUploadResult> UploadAsync(
            Stream stream,
            string originalFileName,
            string? contentType,
            string? folder,
            ObjectStorageAccess access,
            CancellationToken cancellationToken = default)
        {
            var bucketName = ResolveBucketName(access);
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new InvalidOperationException($"Google Cloud Storage bucket configuration is required for {access} objects.");
            }

            if (stream == null || !stream.CanRead)
            {
                throw new ArgumentException("Readable stream is required.", nameof(stream));
            }

            var objectName = ObjectStorageObjectName.Create(originalFileName, folder);
            var resolvedContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType;

            await _storageClient.Value.UploadObjectAsync(
                bucket: bucketName,
                objectName: objectName,
                contentType: resolvedContentType,
                source: stream,
                cancellationToken: cancellationToken);

            var url = $"{_options.PublicBaseUrl.TrimEnd('/')}/{bucketName}/{objectName}";
            return new ObjectStorageUploadResult(bucketName, objectName, url);
        }

        public async Task<byte[]> DownloadAsync(
            string bucketName,
            string objectName,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);
            ArgumentException.ThrowIfNullOrWhiteSpace(objectName);
            EnsureKnownBucket(bucketName);

            await using var stream = new MemoryStream();
            await _storageClient.Value.DownloadObjectAsync(
                bucketName,
                objectName,
                stream,
                cancellationToken: cancellationToken);
            return stream.ToArray();
        }

        private static StorageClient CreateStorageClient(GoogleCloudStorageOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.ServiceAccountJsonPath))
            {
                var credential = CredentialFactory.FromFile<GoogleCredential>(options.ServiceAccountJsonPath);
                return StorageClient.Create(credential);
            }

            return StorageClient.Create();
        }

        private string ResolveBucketName(ObjectStorageAccess access)
            => access == ObjectStorageAccess.Public
                ? FirstConfigured(_options.PublicBucketName, _options.BucketName)
                : FirstConfigured(_options.PrivateBucketName, _options.BucketName);

        private void EnsureKnownBucket(string bucketName)
        {
            var publicBucket = ResolveBucketName(ObjectStorageAccess.Public);
            var privateBucket = ResolveBucketName(ObjectStorageAccess.Private);
            if (!string.Equals(bucketName, publicBucket, StringComparison.Ordinal)
                && !string.Equals(bucketName, privateBucket, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The requested bucket is outside the configured object storage boundary.");
            }
        }

        private static string FirstConfigured(string? preferred, string? fallback)
            => string.IsNullOrWhiteSpace(preferred)
                ? fallback?.Trim() ?? string.Empty
                : preferred.Trim();
    }
}



