using System.Text.Json;
using Ssalddel.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Orderer;

public sealed class Mongo공급자관심구독DraftStore : I공급자관심구독DraftStore
{
    private readonly IMongoCollection<SupplierInterestSubscriptionDraftDocument> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public Mongo공급자관심구독DraftStore(
        IMongoClient mongoClient,
        IOptions<MongoDbOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Database))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(options.Value.Database.Trim())
            .GetCollection<SupplierInterestSubscriptionDraftDocument>(
                "orderer_supplier_interest_subscription_drafts");
    }

    public async Task<SupplierInterestSubscriptionDraftResponse> 저장Async(
        SupplierInterestSubscriptionDraftResponse draft,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        draft.IsDurablyPersisted = true;
        var document = new SupplierInterestSubscriptionDraftDocument
        {
            Id = ObjectId.GenerateNewId(),
            DraftId = draft.DraftId.ToString("N"),
            OwnerUserId = draft.OwnerUserId,
            SupplierKey = draft.SupplierKey,
            PayloadJson = JsonSerializer.Serialize(draft),
            CreatedAtUtc = draft.CreatedAtUtc.UtcDateTime,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _collection.ReplaceOneAsync(
            item => item.DraftId == document.DraftId,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
        return draft;
    }

    public async Task<SupplierInterestSubscriptionDraftResponse?> 조회Async(
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        if (draftId == Guid.Empty)
        {
            return null;
        }

        await EnsureIndexesAsync(cancellationToken);
        var document = await _collection
            .Find(item => item.DraftId == draftId.ToString("N"))
            .FirstOrDefaultAsync(cancellationToken);
        return document is null
            ? null
            : JsonSerializer.Deserialize<SupplierInterestSubscriptionDraftResponse>(
                document.PayloadJson);
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (_indexesReady)
        {
            return;
        }

        await _indexLock.WaitAsync(cancellationToken);
        try
        {
            if (_indexesReady)
            {
                return;
            }

            await _collection.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<SupplierInterestSubscriptionDraftDocument>(
                    Builders<SupplierInterestSubscriptionDraftDocument>.IndexKeys
                        .Ascending(item => item.DraftId),
                    new CreateIndexOptions
                    {
                        Unique = true,
                        Name = "ux_supplier_interest_draft_id"
                    }),
                new CreateIndexModel<SupplierInterestSubscriptionDraftDocument>(
                    Builders<SupplierInterestSubscriptionDraftDocument>.IndexKeys
                        .Ascending(item => item.OwnerUserId)
                        .Descending(item => item.UpdatedAtUtc),
                    new CreateIndexOptions
                    {
                        Name = "ix_supplier_interest_owner_updated"
                    }),
                new CreateIndexModel<SupplierInterestSubscriptionDraftDocument>(
                    Builders<SupplierInterestSubscriptionDraftDocument>.IndexKeys
                        .Ascending(item => item.SupplierKey)
                        .Descending(item => item.UpdatedAtUtc),
                    new CreateIndexOptions
                    {
                        Name = "ix_supplier_interest_supplier_updated"
                    })
            ], cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }
}

internal sealed class SupplierInterestSubscriptionDraftDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string DraftId { get; set; } = string.Empty;

    public string OwnerUserId { get; set; } = string.Empty;

    public string SupplierKey { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
