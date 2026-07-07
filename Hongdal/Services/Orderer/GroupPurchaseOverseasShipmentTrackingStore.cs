using Hongdal.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 홍달.Services.Options;

namespace Hongdal.Services.Orderer;

public interface IGroupPurchaseOverseasShipmentTrackingStore
{
    Task<IReadOnlyList<GroupPurchaseOverseasShipmentTrackingDto>> ListAsync(
        GroupPurchaseOverseasShipmentTrackingQuery query,
        CancellationToken cancellationToken = default);

    Task<GroupPurchaseOverseasShipmentTrackingDto?> GetByDocumentManagementNumberAsync(
        string documentManagementNumber,
        CancellationToken cancellationToken = default);

    Task<GroupPurchaseOverseasShipmentTrackingDto> UpsertAsync(
        GroupPurchaseOverseasShipmentTrackingUpsertRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<GroupPurchaseOverseasShipmentTrackingDto?> AppendEventAsync(
        string documentManagementNumber,
        GroupPurchaseOverseasShipmentTrackingEventAppendRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public sealed class MongoGroupPurchaseOverseasShipmentTrackingStore : IGroupPurchaseOverseasShipmentTrackingStore
{
    private const string CollectionName = "orderer_group_purchase_overseas_shipments";
    private readonly IMongoCollection<GroupPurchaseOverseasShipmentTrackingDocument> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public MongoGroupPurchaseOverseasShipmentTrackingStore(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<GroupPurchaseOverseasShipmentTrackingDocument>(CollectionName);
    }

    public async Task<IReadOnlyList<GroupPurchaseOverseasShipmentTrackingDto>> ListAsync(
        GroupPurchaseOverseasShipmentTrackingQuery query,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var items = await _collection
            .Find(BuildFilter(query))
            .SortByDescending(x => x.UpdatedAtUtc)
            .Limit(200)
            .ToListAsync(cancellationToken);

        return items.Select(ToDto).ToArray();
    }

    public async Task<GroupPurchaseOverseasShipmentTrackingDto?> GetByDocumentManagementNumberAsync(
        string documentManagementNumber,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var normalized = NormalizeRequired(documentManagementNumber, "documentManagementNumber");
        var item = await _collection
            .Find(x => x.DocumentManagementNumberNormalized == normalized)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : ToDto(item);
    }

    public async Task<GroupPurchaseOverseasShipmentTrackingDto> UpsertAsync(
        GroupPurchaseOverseasShipmentTrackingUpsertRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        Validate(request);

        var now = DateTime.UtcNow;
        var documentManagementNumber = request.DocumentManagementNumber.Trim();
        var documentManagementNumberNormalized = NormalizeRequired(documentManagementNumber, "documentManagementNumber");
        var existing = await _collection
            .Find(x => x.DocumentManagementNumberNormalized == documentManagementNumberNormalized)
            .FirstOrDefaultAsync(cancellationToken);
        var trackingId = string.IsNullOrWhiteSpace(request.TrackingId)
            ? existing?.TrackingId ?? ObjectId.GenerateNewId().ToString()
            : request.TrackingId.Trim();
        var events = request.Events.Count == 0 && existing is not null
            ? existing.Events.OrderBy(x => x.OccurredAtUtc).ToArray()
            : request.Events
                .Select(ToDocument)
                .OrderBy(x => x.OccurredAtUtc)
                .ToArray();

        var document = new GroupPurchaseOverseasShipmentTrackingDocument
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            TrackingId = trackingId,
            GroupPurchaseId = request.GroupPurchaseId.Trim(),
            OrdererGroupScopeKey = request.OrdererGroupScopeKey.Trim(),
            OrdererGroupScopeName = request.OrdererGroupScopeName.Trim(),
            ProductSummary = request.ProductSummary.Trim(),
            DocumentManagementNumber = documentManagementNumber,
            DocumentManagementNumberNormalized = documentManagementNumberNormalized,
            TransportDocumentType = NormalizeTransportDocumentType(request.TransportDocumentType),
            TransportDocumentNumber = request.TransportDocumentNumber.Trim(),
            TransportDocumentNumberNormalized = NormalizeOptional(request.TransportDocumentNumber),
            TransportMode = NormalizeTransportMode(request.TransportMode),
            CarrierName = request.CarrierName.Trim(),
            VesselName = request.VesselName.Trim(),
            VoyageNumber = request.VoyageNumber.Trim(),
            FlightNumber = request.FlightNumber.Trim(),
            OriginCountryCode = request.OriginCountryCode.Trim().ToUpperInvariant(),
            OriginPortCode = request.OriginPortCode.Trim().ToUpperInvariant(),
            DestinationPortCode = request.DestinationPortCode.Trim().ToUpperInvariant(),
            EstimatedDepartureAtUtc = request.EstimatedDepartureAtUtc,
            ActualDepartureAtUtc = request.ActualDepartureAtUtc,
            EstimatedArrivalAtUtc = request.EstimatedArrivalAtUtc,
            ActualArrivalAtUtc = request.ActualArrivalAtUtc,
            CurrentStatusCode = string.IsNullOrWhiteSpace(request.CurrentStatusCode)
                ? ResolveCurrentStatusCode(events)
                : request.CurrentStatusCode.Trim(),
            CurrentLocationSummary = request.CurrentLocationSummary.Trim(),
            LastMilestoneAtUtc = request.LastMilestoneAtUtc ?? events.LastOrDefault()?.OccurredAtUtc,
            Events = events,
            AdminMemo = request.AdminMemo.Trim(),
            UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            CreatedAtUtc = existing?.CreatedAtUtc ?? now,
            UpdatedAtUtc = now
        };

        await _collection.ReplaceOneAsync(
            x => x.DocumentManagementNumberNormalized == documentManagementNumberNormalized,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return ToDto(document);
    }

    public async Task<GroupPurchaseOverseasShipmentTrackingDto?> AppendEventAsync(
        string documentManagementNumber,
        GroupPurchaseOverseasShipmentTrackingEventAppendRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        Validate(request);

        var normalized = NormalizeRequired(documentManagementNumber, "documentManagementNumber");
        var eventDocument = ToDocument(request);
        var now = DateTime.UtcNow;

        var update = Builders<GroupPurchaseOverseasShipmentTrackingDocument>.Update
            .Push(x => x.Events, eventDocument)
            .Set(x => x.CurrentStatusCode, eventDocument.EventCode)
            .Set(x => x.CurrentLocationSummary, eventDocument.LocationSummary)
            .Set(x => x.LastMilestoneAtUtc, eventDocument.OccurredAtUtc)
            .Set(x => x.UpdatedBy, string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim())
            .Set(x => x.UpdatedAtUtc, now);

        var item = await _collection.FindOneAndUpdateAsync(
            x => x.DocumentManagementNumberNormalized == normalized,
            update,
            new FindOneAndUpdateOptions<GroupPurchaseOverseasShipmentTrackingDocument>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);

        return item is null ? null : ToDto(item);
    }

    private FilterDefinition<GroupPurchaseOverseasShipmentTrackingDocument> BuildFilter(
        GroupPurchaseOverseasShipmentTrackingQuery query)
    {
        var builder = Builders<GroupPurchaseOverseasShipmentTrackingDocument>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(query.GroupPurchaseId))
        {
            filter &= builder.Eq(x => x.GroupPurchaseId, query.GroupPurchaseId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.OrdererGroupScopeKey))
        {
            filter &= builder.Eq(x => x.OrdererGroupScopeKey, query.OrdererGroupScopeKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.DocumentManagementNumber))
        {
            filter &= builder.Eq(
                x => x.DocumentManagementNumberNormalized,
                NormalizeRequired(query.DocumentManagementNumber, "documentManagementNumber"));
        }

        if (!string.IsNullOrWhiteSpace(query.TransportDocumentNumber))
        {
            filter &= builder.Eq(
                x => x.TransportDocumentNumberNormalized,
                NormalizeOptional(query.TransportDocumentNumber));
        }

        if (!string.IsNullOrWhiteSpace(query.CurrentStatusCode))
        {
            filter &= builder.Eq(x => x.CurrentStatusCode, query.CurrentStatusCode.Trim());
        }

        return filter;
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

            var indexes = new[]
            {
                new CreateIndexModel<GroupPurchaseOverseasShipmentTrackingDocument>(
                    Builders<GroupPurchaseOverseasShipmentTrackingDocument>.IndexKeys
                        .Ascending(x => x.DocumentManagementNumberNormalized),
                    new CreateIndexOptions { Unique = true, Name = "ux_document_management_number" }),
                new CreateIndexModel<GroupPurchaseOverseasShipmentTrackingDocument>(
                    Builders<GroupPurchaseOverseasShipmentTrackingDocument>.IndexKeys
                        .Ascending(x => x.GroupPurchaseId)
                        .Ascending(x => x.OrdererGroupScopeKey),
                    new CreateIndexOptions { Name = "ix_group_purchase_scope" }),
                new CreateIndexModel<GroupPurchaseOverseasShipmentTrackingDocument>(
                    Builders<GroupPurchaseOverseasShipmentTrackingDocument>.IndexKeys
                        .Ascending(x => x.TransportDocumentNumberNormalized),
                    new CreateIndexOptions { Name = "ix_transport_document_number" }),
                new CreateIndexModel<GroupPurchaseOverseasShipmentTrackingDocument>(
                    Builders<GroupPurchaseOverseasShipmentTrackingDocument>.IndexKeys
                        .Ascending(x => x.CurrentStatusCode)
                        .Descending(x => x.UpdatedAtUtc),
                    new CreateIndexOptions { Name = "ix_status_updated" })
            };

            await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static void Validate(GroupPurchaseOverseasShipmentTrackingUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GroupPurchaseId)) throw new InvalidOperationException("groupPurchaseId is required.");
        if (string.IsNullOrWhiteSpace(request.OrdererGroupScopeKey)) throw new InvalidOperationException("ordererGroupScopeKey is required.");
        if (string.IsNullOrWhiteSpace(request.DocumentManagementNumber)) throw new InvalidOperationException("documentManagementNumber is required.");
        if (string.IsNullOrWhiteSpace(request.TransportDocumentNumber)) throw new InvalidOperationException("transportDocumentNumber is required.");
        if (NormalizeTransportDocumentType(request.TransportDocumentType) == GroupPurchaseShipmentDocumentTypeCode.AirWaybill
            && NormalizeTransportMode(request.TransportMode) != GroupPurchaseShipmentTransportModeCode.Air)
        {
            throw new InvalidOperationException("airWaybill requires air transport mode.");
        }
    }

    private static void Validate(GroupPurchaseOverseasShipmentTrackingEventAppendRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventCode)) throw new InvalidOperationException("eventCode is required.");
        if (string.IsNullOrWhiteSpace(request.DisplayName)) throw new InvalidOperationException("displayName is required.");
        if (string.IsNullOrWhiteSpace(request.SourcePartyCode)) throw new InvalidOperationException("sourcePartyCode is required.");
    }

    private static string ResolveCurrentStatusCode(IReadOnlyList<GroupPurchaseOverseasShipmentTrackingEventDocument> events)
        => events.LastOrDefault()?.EventCode ?? GroupPurchaseShipmentStatusCode.DocumentRegistered;

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return NormalizeOptional(value);
    }

    private static string NormalizeOptional(string? value)
        => (value ?? string.Empty).Trim().Replace(" ", string.Empty).ToUpperInvariant();

    private static string NormalizeTransportDocumentType(string? value)
        => string.Equals(value?.Trim(), GroupPurchaseShipmentDocumentTypeCode.AirWaybill, StringComparison.OrdinalIgnoreCase)
            ? GroupPurchaseShipmentDocumentTypeCode.AirWaybill
            : GroupPurchaseShipmentDocumentTypeCode.BillOfLading;

    private static string NormalizeTransportMode(string? value)
        => string.Equals(value?.Trim(), GroupPurchaseShipmentTransportModeCode.Air, StringComparison.OrdinalIgnoreCase)
            ? GroupPurchaseShipmentTransportModeCode.Air
            : GroupPurchaseShipmentTransportModeCode.Ocean;

    private static GroupPurchaseOverseasShipmentTrackingEventDocument ToDocument(
        GroupPurchaseOverseasShipmentTrackingEventDto source)
        => new()
        {
            EventCode = source.EventCode.Trim(),
            DisplayName = source.DisplayName.Trim(),
            LocationSummary = source.LocationSummary.Trim(),
            OccurredAtUtc = source.OccurredAtUtc,
            SourcePartyCode = source.SourcePartyCode.Trim(),
            EvidenceReference = source.EvidenceReference.Trim(),
            Memo = source.Memo.Trim(),
            IsOrdererVisible = source.IsOrdererVisible
        };

    private static GroupPurchaseOverseasShipmentTrackingEventDocument ToDocument(
        GroupPurchaseOverseasShipmentTrackingEventAppendRequest source)
        => new()
        {
            EventCode = source.EventCode.Trim(),
            DisplayName = source.DisplayName.Trim(),
            LocationSummary = source.LocationSummary.Trim(),
            OccurredAtUtc = source.OccurredAtUtc ?? DateTime.UtcNow,
            SourcePartyCode = source.SourcePartyCode.Trim(),
            EvidenceReference = source.EvidenceReference.Trim(),
            Memo = source.Memo.Trim(),
            IsOrdererVisible = source.IsOrdererVisible
        };

    private static GroupPurchaseOverseasShipmentTrackingDto ToDto(
        GroupPurchaseOverseasShipmentTrackingDocument source)
        => new()
        {
            TrackingId = source.TrackingId,
            GroupPurchaseId = source.GroupPurchaseId,
            OrdererGroupScopeKey = source.OrdererGroupScopeKey,
            OrdererGroupScopeName = source.OrdererGroupScopeName,
            ProductSummary = source.ProductSummary,
            DocumentManagementNumber = source.DocumentManagementNumber,
            TransportDocumentType = source.TransportDocumentType,
            TransportDocumentNumber = source.TransportDocumentNumber,
            TransportMode = source.TransportMode,
            CarrierName = source.CarrierName,
            VesselName = source.VesselName,
            VoyageNumber = source.VoyageNumber,
            FlightNumber = source.FlightNumber,
            OriginCountryCode = source.OriginCountryCode,
            OriginPortCode = source.OriginPortCode,
            DestinationPortCode = source.DestinationPortCode,
            EstimatedDepartureAtUtc = source.EstimatedDepartureAtUtc,
            ActualDepartureAtUtc = source.ActualDepartureAtUtc,
            EstimatedArrivalAtUtc = source.EstimatedArrivalAtUtc,
            ActualArrivalAtUtc = source.ActualArrivalAtUtc,
            CurrentStatusCode = source.CurrentStatusCode,
            CurrentLocationSummary = source.CurrentLocationSummary,
            LastMilestoneAtUtc = source.LastMilestoneAtUtc,
            Events = source.Events.OrderBy(x => x.OccurredAtUtc).Select(ToDto).ToArray(),
            AdminMemo = source.AdminMemo,
            UpdatedBy = source.UpdatedBy,
            CreatedAtUtc = source.CreatedAtUtc,
            UpdatedAtUtc = source.UpdatedAtUtc
        };

    private static GroupPurchaseOverseasShipmentTrackingEventDto ToDto(
        GroupPurchaseOverseasShipmentTrackingEventDocument source)
        => new()
        {
            EventCode = source.EventCode,
            DisplayName = source.DisplayName,
            LocationSummary = source.LocationSummary,
            OccurredAtUtc = source.OccurredAtUtc,
            SourcePartyCode = source.SourcePartyCode,
            EvidenceReference = source.EvidenceReference,
            Memo = source.Memo,
            IsOrdererVisible = source.IsOrdererVisible
        };
}

public sealed class GroupPurchaseOverseasShipmentTrackingDocument
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string ProductSummary { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public string DocumentManagementNumberNormalized { get; set; } = string.Empty;
    public string TransportDocumentType { get; set; } = GroupPurchaseShipmentDocumentTypeCode.BillOfLading;
    public string TransportDocumentNumber { get; set; } = string.Empty;
    public string TransportDocumentNumberNormalized { get; set; } = string.Empty;
    public string TransportMode { get; set; } = GroupPurchaseShipmentTransportModeCode.Ocean;
    public string CarrierName { get; set; } = string.Empty;
    public string VesselName { get; set; } = string.Empty;
    public string VoyageNumber { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginCountryCode { get; set; } = string.Empty;
    public string OriginPortCode { get; set; } = string.Empty;
    public string DestinationPortCode { get; set; } = string.Empty;
    public DateTime? EstimatedDepartureAtUtc { get; set; }
    public DateTime? ActualDepartureAtUtc { get; set; }
    public DateTime? EstimatedArrivalAtUtc { get; set; }
    public DateTime? ActualArrivalAtUtc { get; set; }
    public string CurrentStatusCode { get; set; } = GroupPurchaseShipmentStatusCode.DocumentRegistered;
    public string CurrentLocationSummary { get; set; } = string.Empty;
    public DateTime? LastMilestoneAtUtc { get; set; }
    public IReadOnlyList<GroupPurchaseOverseasShipmentTrackingEventDocument> Events { get; set; } = [];
    public string AdminMemo { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class GroupPurchaseOverseasShipmentTrackingEventDocument
{
    public string EventCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string LocationSummary { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string SourcePartyCode { get; set; } = string.Empty;
    public string EvidenceReference { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
    public bool IsOrdererVisible { get; set; } = true;
}
