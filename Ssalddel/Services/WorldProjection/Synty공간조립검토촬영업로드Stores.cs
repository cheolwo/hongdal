using System.Text.Json;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 살뜰.Services.Options;

namespace Ssalddel.Services.WorldProjection;

public sealed class Synty공간조립검토촬영업로드Record
{
    [BsonId]
    public string CaptureUploadId { get; set; } = string.Empty;
    public string BatchStableId { get; set; } = string.Empty;
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string CaptureStableId { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string CaptureBundleHash { get; set; } = string.Empty;
    public string ParentCaptureBundleHash { get; set; } = string.Empty;
    public string SourceCompositionHash { get; set; } = string.Empty;
    public long ExpectedReviewItemRevision { get; set; }
    public string RenderingProfileHash { get; set; } = string.Empty;
    public string StorageProviderCode { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string UploadedSourceSha256 { get; set; } = string.Empty;
    public string StoredImageSha256 { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public string ETag { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}

internal sealed class MongoSynty공간조립검토촬영업로드Store(
    IMongoClient client,
    IOptions<MongoDbOptions> options) : ISynty공간조립검토촬영업로드Store
{
    private const string CollectionName = "synty_composition_review_capture_uploads";
    private readonly IMongoCollection<Synty공간조립검토촬영업로드Record> collection =
        client.GetDatabase(RequireDatabase(options.Value.Database))
            .GetCollection<Synty공간조립검토촬영업로드Record>(CollectionName);

    public async Task<Synty공간조립검토촬영업로드Record?> 조회Async(
        string captureUploadId,
        CancellationToken cancellationToken = default)
        => await collection.Find(record => record.CaptureUploadId == captureUploadId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> 추가Async(
        Synty공간조립검토촬영업로드Record record,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await collection.InsertOneAsync(record, cancellationToken: cancellationToken);
            return true;
        }
        catch (MongoWriteException exception) when (
            exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    private static string RequireDatabase(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException("MongoDb:Database configuration is required.")
            : value.Trim();
}

public sealed class InMemorySynty공간조립검토촬영업로드Store
    : ISynty공간조립검토촬영업로드Store
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, Synty공간조립검토촬영업로드Record> records = new(StringComparer.Ordinal);
    private readonly object sync = new();

    public Task<Synty공간조립검토촬영업로드Record?> 조회Async(
        string captureUploadId,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            return Task.FromResult(records.TryGetValue(captureUploadId, out var record)
                ? Clone(record)
                : null);
        }
    }

    public Task<bool> 추가Async(
        Synty공간조립검토촬영업로드Record record,
        CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            if (records.ContainsKey(record.CaptureUploadId))
            {
                return Task.FromResult(false);
            }
            records[record.CaptureUploadId] = Clone(record);
            return Task.FromResult(true);
        }
    }

    private static Synty공간조립검토촬영업로드Record Clone(
        Synty공간조립검토촬영업로드Record record)
        => JsonSerializer.Deserialize<Synty공간조립검토촬영업로드Record>(
               JsonSerializer.Serialize(record, JsonOptions),
               JsonOptions)!;
}
