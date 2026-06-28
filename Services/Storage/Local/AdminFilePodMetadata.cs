namespace 홍달.Services.Storage.Local
{
    public sealed record AdminFilePodMetadata(
        Guid Id,
        string FileType,
        string RequestId,
        string BucketName,
        string ObjectName,
        string Url,
        string OriginalFileName,
        string UploadStatus,
        DateTime UploadedAtUtc,
        DateTime UpdatedAtUtc);
}
