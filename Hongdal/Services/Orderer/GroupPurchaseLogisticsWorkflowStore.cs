using System.Text.RegularExpressions;
using Hongdal.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 홍달.Services.Options;

namespace Hongdal.Services.Orderer;

public interface IGroupPurchaseLogisticsWorkflowStore
{
    Task<IReadOnlyList<GroupPurchaseLogisticsWorkflowDefinitionDto>> ListAsync(
        GroupPurchaseLogisticsWorkflowQuery query,
        CancellationToken cancellationToken = default);

    Task<GroupPurchaseLogisticsWorkflowDefinitionDto?> GetAsync(
        string workflowId,
        string? version = null,
        CancellationToken cancellationToken = default);

    Task<GroupPurchaseLogisticsWorkflowDefinitionDto?> ResolveAsync(
        GroupPurchaseLogisticsWorkflowQuery query,
        CancellationToken cancellationToken = default);

    Task<GroupPurchaseLogisticsWorkflowDefinitionDto> UpsertAsync(
        GroupPurchaseLogisticsWorkflowUpsertRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task SeedDefaultsAsync(CancellationToken cancellationToken = default);
}

public sealed class MongoGroupPurchaseLogisticsWorkflowStore : IGroupPurchaseLogisticsWorkflowStore
{
    private const string CollectionName = "orderer_group_purchase_logistics_workflows";
    private readonly IMongoCollection<GroupPurchaseLogisticsWorkflowDocument> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public MongoGroupPurchaseLogisticsWorkflowStore(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<GroupPurchaseLogisticsWorkflowDocument>(CollectionName);
    }

    public async Task<IReadOnlyList<GroupPurchaseLogisticsWorkflowDefinitionDto>> ListAsync(
        GroupPurchaseLogisticsWorkflowQuery query,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var filter = BuildFilter(query);
        var items = await _collection
            .Find(filter)
            .SortBy(x => x.ProductCategoryCode)
            .ThenBy(x => x.TemperatureCode)
            .ThenBy(x => x.LogisticsMode)
            .ThenBy(x => x.SellerOriginType)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return items.Select(ToDto).ToArray();
    }

    public async Task<GroupPurchaseLogisticsWorkflowDefinitionDto?> GetAsync(
        string workflowId,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(workflowId))
        {
            return null;
        }

        var builder = Builders<GroupPurchaseLogisticsWorkflowDocument>.Filter;
        var filter = builder.Eq(x => x.WorkflowId, workflowId.Trim());
        if (!string.IsNullOrWhiteSpace(version))
        {
            filter &= builder.Eq(x => x.Version, version.Trim());
        }

        var item = await _collection
            .Find(filter)
            .SortByDescending(x => x.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : ToDto(item);
    }

    public async Task<GroupPurchaseLogisticsWorkflowDefinitionDto?> ResolveAsync(
        GroupPurchaseLogisticsWorkflowQuery query,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        query.ActiveOnly = true;
        var item = await _collection
            .Find(BuildFilter(query))
            .SortByDescending(x => x.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : ToDto(item);
    }

    public async Task<GroupPurchaseLogisticsWorkflowDefinitionDto> UpsertAsync(
        GroupPurchaseLogisticsWorkflowUpsertRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        Validate(request);

        var now = DateTime.UtcNow;
        var workflowId = string.IsNullOrWhiteSpace(request.WorkflowId)
            ? BuildWorkflowId(request)
            : NormalizeKey(request.WorkflowId);
        var version = string.IsNullOrWhiteSpace(request.Version) ? "1.0" : request.Version.Trim();

        var existing = await _collection
            .Find(x => x.WorkflowId == workflowId && x.Version == version)
            .FirstOrDefaultAsync(cancellationToken);

        var document = new GroupPurchaseLogisticsWorkflowDocument
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            WorkflowId = workflowId,
            Version = version,
            DisplayName = request.DisplayName.Trim(),
            ProductCategoryCode = request.ProductCategoryCode.Trim(),
            TemperatureCode = request.TemperatureCode.Trim(),
            LogisticsMode = request.LogisticsMode.Trim(),
            SellerOriginType = NormalizeSellerOriginType(request.SellerOriginType),
            OrdererGroupScopeType = request.OrdererGroupScopeType.Trim(),
            IsActive = request.IsActive,
            Steps = request.Steps.Select(ToDocument).OrderBy(x => x.Sequence).ToArray(),
            ResponsibilitySegments = request.ResponsibilitySegments.Select(ToDocument).ToArray(),
            Tags = request.Tags.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            Memo = request.Memo.Trim(),
            UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            CreatedAtUtc = existing?.CreatedAtUtc ?? now,
            UpdatedAtUtc = now
        };

        await _collection.ReplaceOneAsync(
            x => x.WorkflowId == workflowId && x.Version == version,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return ToDto(document);
    }

    public async Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var defaults = new[]
        {
            CreateDefaultDomesticColdChainApartmentWorkflow(),
            CreateDefaultOverseasColdChainApartmentWorkflow()
        };

        foreach (var item in defaults)
        {
            var exists = await _collection
                .Find(x => x.WorkflowId == item.WorkflowId && x.Version == (item.Version ?? "1.0"))
                .AnyAsync(cancellationToken);
            if (!exists)
            {
                await UpsertAsync(item, "seed", cancellationToken);
            }
        }
    }

    private FilterDefinition<GroupPurchaseLogisticsWorkflowDocument> BuildFilter(GroupPurchaseLogisticsWorkflowQuery query)
    {
        var builder = Builders<GroupPurchaseLogisticsWorkflowDocument>.Filter;
        var filter = builder.Empty;

        if (query.ActiveOnly)
        {
            filter &= builder.Eq(x => x.IsActive, true);
        }

        if (!string.IsNullOrWhiteSpace(query.ProductCategoryCode))
        {
            filter &= builder.Eq(x => x.ProductCategoryCode, query.ProductCategoryCode.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.TemperatureCode))
        {
            filter &= builder.Eq(x => x.TemperatureCode, query.TemperatureCode.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.LogisticsMode))
        {
            filter &= builder.Eq(x => x.LogisticsMode, query.LogisticsMode.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.SellerOriginType))
        {
            filter &= builder.Eq(x => x.SellerOriginType, NormalizeSellerOriginType(query.SellerOriginType));
        }

        if (!string.IsNullOrWhiteSpace(query.OrdererGroupScopeType))
        {
            filter &= builder.Eq(x => x.OrdererGroupScopeType, query.OrdererGroupScopeType.Trim());
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
                new CreateIndexModel<GroupPurchaseLogisticsWorkflowDocument>(
                    Builders<GroupPurchaseLogisticsWorkflowDocument>.IndexKeys
                        .Ascending(x => x.WorkflowId)
                        .Ascending(x => x.Version),
                    new CreateIndexOptions { Unique = true, Name = "ux_workflow_version" }),
                new CreateIndexModel<GroupPurchaseLogisticsWorkflowDocument>(
                    Builders<GroupPurchaseLogisticsWorkflowDocument>.IndexKeys
                        .Ascending(x => x.ProductCategoryCode)
                        .Ascending(x => x.TemperatureCode)
                        .Ascending(x => x.LogisticsMode)
                        .Ascending(x => x.SellerOriginType)
                        .Ascending(x => x.OrdererGroupScopeType)
                        .Ascending(x => x.IsActive),
                    new CreateIndexOptions { Name = "ix_workflow_match" })
            };

            await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static void Validate(GroupPurchaseLogisticsWorkflowUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName)) throw new InvalidOperationException("displayName is required.");
        if (string.IsNullOrWhiteSpace(request.ProductCategoryCode)) throw new InvalidOperationException("productCategoryCode is required.");
        if (string.IsNullOrWhiteSpace(request.TemperatureCode)) throw new InvalidOperationException("temperatureCode is required.");
        if (string.IsNullOrWhiteSpace(request.LogisticsMode)) throw new InvalidOperationException("logisticsMode is required.");
        if (string.IsNullOrWhiteSpace(request.SellerOriginType)) throw new InvalidOperationException("sellerOriginType is required.");
        if (string.IsNullOrWhiteSpace(request.OrdererGroupScopeType)) throw new InvalidOperationException("ordererGroupScopeType is required.");
        if (request.Steps.Count == 0) throw new InvalidOperationException("steps are required.");
        if (request.ResponsibilitySegments.Count == 0) throw new InvalidOperationException("responsibilitySegments are required.");
    }

    private static string BuildWorkflowId(GroupPurchaseLogisticsWorkflowUpsertRequest request)
        => NormalizeKey($"{request.ProductCategoryCode}-{request.TemperatureCode}-{request.LogisticsMode}-{request.SellerOriginType}-{request.OrdererGroupScopeType}");

    private static string NormalizeSellerOriginType(string? value)
        => string.Equals(value?.Trim(), GroupPurchaseSellerOriginTypeCode.Overseas, StringComparison.OrdinalIgnoreCase)
            ? GroupPurchaseSellerOriginTypeCode.Overseas
            : GroupPurchaseSellerOriginTypeCode.Domestic;

    private static string NormalizeKey(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        return Regex.Replace(trimmed, "[^a-z0-9가-힣]+", "-").Trim('-');
    }

    private static GroupPurchaseLogisticsWorkflowStepDocument ToDocument(GroupPurchaseLogisticsWorkflowStepDto source)
        => new()
        {
            StepCode = source.StepCode.Trim(),
            DisplayName = source.DisplayName.Trim(),
            Sequence = source.Sequence,
            ResponsiblePartyCode = source.ResponsiblePartyCode.Trim(),
            Description = source.Description.Trim(),
            RequiredEvidenceCodes = source.RequiredEvidenceCodes.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray(),
            FailureHandlingCodes = source.FailureHandlingCodes.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray()
        };

    private static GroupPurchaseResponsibilitySegmentDocument ToDocument(GroupPurchaseResponsibilitySegmentDto source)
        => new()
        {
            SegmentCode = source.SegmentCode.Trim(),
            FromStepCode = source.FromStepCode.Trim(),
            ToStepCode = source.ToStepCode.Trim(),
            ResponsiblePartyCode = source.ResponsiblePartyCode.Trim(),
            ResponsibilityScope = source.ResponsibilityScope.Trim(),
            RequiredEvidenceCodes = source.RequiredEvidenceCodes.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray()
        };

    private static GroupPurchaseLogisticsWorkflowDefinitionDto ToDto(GroupPurchaseLogisticsWorkflowDocument source)
        => new()
        {
            WorkflowId = source.WorkflowId,
            Version = source.Version,
            DisplayName = source.DisplayName,
            ProductCategoryCode = source.ProductCategoryCode,
            TemperatureCode = source.TemperatureCode,
            LogisticsMode = source.LogisticsMode,
            SellerOriginType = source.SellerOriginType,
            OrdererGroupScopeType = source.OrdererGroupScopeType,
            IsActive = source.IsActive,
            Steps = source.Steps.Select(ToDto).ToArray(),
            ResponsibilitySegments = source.ResponsibilitySegments.Select(ToDto).ToArray(),
            Tags = source.Tags,
            Memo = source.Memo,
            UpdatedBy = source.UpdatedBy,
            CreatedAtUtc = source.CreatedAtUtc,
            UpdatedAtUtc = source.UpdatedAtUtc
        };

    private static GroupPurchaseLogisticsWorkflowStepDto ToDto(GroupPurchaseLogisticsWorkflowStepDocument source)
        => new()
        {
            StepCode = source.StepCode,
            DisplayName = source.DisplayName,
            Sequence = source.Sequence,
            ResponsiblePartyCode = source.ResponsiblePartyCode,
            Description = source.Description,
            RequiredEvidenceCodes = source.RequiredEvidenceCodes,
            FailureHandlingCodes = source.FailureHandlingCodes
        };

    private static GroupPurchaseResponsibilitySegmentDto ToDto(GroupPurchaseResponsibilitySegmentDocument source)
        => new()
        {
            SegmentCode = source.SegmentCode,
            FromStepCode = source.FromStepCode,
            ToStepCode = source.ToStepCode,
            ResponsiblePartyCode = source.ResponsiblePartyCode,
            ResponsibilityScope = source.ResponsibilityScope,
            RequiredEvidenceCodes = source.RequiredEvidenceCodes
        };

    private static GroupPurchaseLogisticsWorkflowUpsertRequest CreateDefaultDomesticColdChainApartmentWorkflow()
        => new()
        {
            WorkflowId = "food-cold-chain-domestic-apartment-v1",
            Version = "1.0",
            DisplayName = "공동주택 국내 판매자 냉장/냉동 먹거리 공동주문 기본 흐름",
            ProductCategoryCode = "FoodColdChain",
            TemperatureCode = "Frozen",
            LogisticsMode = "DomesticBulk",
            SellerOriginType = GroupPurchaseSellerOriginTypeCode.Domestic,
            OrdererGroupScopeType = "ApartmentComplex",
            IsActive = true,
            Tags = ["orderer-group", "apartment", "cold-chain", "domestic-seller", "responsibility"],
            Memo = "국내 판매자 공동주문에서 판매자 출고, 국내 기사 운송, 대표 수령, 세대별 배분 책임 구간을 분리한다.",
            Steps =
            [
                new()
                {
                    StepCode = "GroupOrderConfirmed",
                    DisplayName = "공동주문 확정",
                    Sequence = 10,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Platform,
                    Description = "참여자, 수량, 결제 상태를 확정한다.",
                    RequiredEvidenceCodes = ["PaymentSnapshot"],
                    FailureHandlingCodes = ["ExcludeUnpaidOrderer", "RecalculateQuantity"]
                },
                new()
                {
                    StepCode = "SellerPacked",
                    DisplayName = "국내 판매자 포장/출고 준비",
                    Sequence = 20,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Seller,
                    Description = "국내 판매자가 품목, 수량, 온도 조건에 맞춰 포장한다.",
                    RequiredEvidenceCodes = [GroupPurchaseLogisticsEvidenceCode.SellerPackingList],
                    FailureHandlingCodes = ["SellerShortageClaim", "PackingDefectClaim"]
                },
                new()
                {
                    StepCode = "CarrierPickup",
                    DisplayName = "국내 기사 상차 인계",
                    Sequence = 30,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Carrier,
                    Description = "국내 판매자 또는 국내 출고지에서 기사에게 화물을 인계하고 상차 증빙을 남긴다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.PickupPhoto,
                        GroupPurchaseLogisticsEvidenceCode.PickupHandoverReceipt,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ],
                    FailureHandlingCodes = ["PickupQuantityMismatch", "TemperatureOutOfRange"]
                },
                new()
                {
                    StepCode = "ApartmentDropoff",
                    DisplayName = "공동주택 거점 하차",
                    Sequence = 40,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Carrier,
                    Description = "공동주택 지정 거점에 하차하고 대표 수령자에게 인계한다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.DropoffPhoto,
                        GroupPurchaseLogisticsEvidenceCode.GroupRepresentativeReceipt
                    ],
                    FailureHandlingCodes = ["DropoffDelay", "RepresentativeAbsent", "DamageAtDropoff"]
                },
                new()
                {
                    StepCode = "UnitDistribution",
                    DisplayName = "세대별 배분",
                    Sequence = 50,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.GroupRepresentative,
                    Description = "대표 수령자가 세대별 수량을 분류하고 미수령분을 관리한다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.UnitDistributionChecklist,
                        GroupPurchaseLogisticsEvidenceCode.IndividualReceiptConfirmation
                    ],
                    FailureHandlingCodes = ["UnitMissingItem", "UnclaimedStorage", "InternalDistributionDispute"]
                }
            ],
            ResponsibilitySegments =
            [
                new()
                {
                    SegmentCode = "SellerToCarrier",
                    FromStepCode = "SellerPacked",
                    ToStepCode = "CarrierPickup",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Seller,
                    ResponsibilityScope = "포장 완료부터 기사 상차 인계 전까지 상품 수량, 포장 상태, 출고 가능 온도에 대한 책임",
                    RequiredEvidenceCodes = [GroupPurchaseLogisticsEvidenceCode.SellerPackingList]
                },
                new()
                {
                    SegmentCode = "CarrierTransit",
                    FromStepCode = "CarrierPickup",
                    ToStepCode = "ApartmentDropoff",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Carrier,
                    ResponsibilityScope = "상차 인수 이후 공동주택 거점 하차 인계 전까지 운송 지연, 파손, 분실, 온도 유지에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.PickupPhoto,
                        GroupPurchaseLogisticsEvidenceCode.DropoffPhoto,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ]
                },
                new()
                {
                    SegmentCode = "RepresentativeDistribution",
                    FromStepCode = "ApartmentDropoff",
                    ToStepCode = "UnitDistribution",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.GroupRepresentative,
                    ResponsibilityScope = "대표 수령 이후 세대별 배분, 미수령 보관, 내부 누락 확인에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.GroupRepresentativeReceipt,
                        GroupPurchaseLogisticsEvidenceCode.UnitDistributionChecklist
                    ]
                }
            ]
        };

    private static GroupPurchaseLogisticsWorkflowUpsertRequest CreateDefaultOverseasColdChainApartmentWorkflow()
        => new()
        {
            WorkflowId = "food-cold-chain-overseas-apartment-v1",
            Version = "1.0",
            DisplayName = "공동주택 해외 판매자 냉장/냉동 먹거리 공동주문 기본 흐름",
            ProductCategoryCode = "FoodColdChain",
            TemperatureCode = "Frozen",
            LogisticsMode = "InternationalToDomesticBulk",
            SellerOriginType = GroupPurchaseSellerOriginTypeCode.Overseas,
            OrdererGroupScopeType = "ApartmentComplex",
            IsActive = true,
            Tags = ["orderer-group", "apartment", "cold-chain", "overseas-seller", "customs", "logistics-proxy", "marketplace", "responsibility"],
            Memo = "해외 판매자 공동주문은 해외 포장, 국제 운송/통관, 국내 물류대행 입고, 판매채널 출품, 출고 배치 가능 구간을 별도 책임 구간으로 분리한다.",
            Steps =
            [
                new()
                {
                    StepCode = "GroupOrderConfirmed",
                    DisplayName = "공동주문 확정",
                    Sequence = 10,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Platform,
                    Description = "참여자, 수량, 결제 상태와 수입 가능 조건을 확정한다.",
                    RequiredEvidenceCodes = ["PaymentSnapshot"],
                    FailureHandlingCodes = ["ExcludeUnpaidOrderer", "RecalculateQuantity", "ImportConditionReviewFailed"]
                },
                new()
                {
                    StepCode = "OverseasSellerPacked",
                    DisplayName = "해외 판매자 포장/출고 준비",
                    Sequence = 20,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.OverseasSeller,
                    Description = "해외 판매자가 수출 포장, 수량, 냉장/냉동 조건, 송장 정보를 준비한다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.OverseasSellerPackingList,
                        GroupPurchaseLogisticsEvidenceCode.ExportInvoice,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ],
                    FailureHandlingCodes = ["OverseasSellerShortageClaim", "ExportPackingDefectClaim", "ExportDocumentMismatch"]
                },
                new()
                {
                    StepCode = "InternationalTransport",
                    DisplayName = "국제 운송",
                    Sequence = 30,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Importer,
                    Description = "해외 출고 이후 국내 반입 전까지 국제 운송 상태와 온도 이력을 관리한다.",
                    RequiredEvidenceCodes = [GroupPurchaseLogisticsEvidenceCode.TemperatureLog],
                    FailureHandlingCodes = ["InternationalDelay", "TemperatureOutOfRange", "InTransitDamage"]
                },
                new()
                {
                    StepCode = "CustomsCleared",
                    DisplayName = "통관/검역 완료",
                    Sequence = 40,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.CustomsBroker,
                    Description = "수입 신고, 식품 검역 또는 검사 결과를 확인하고 국내 반입 가능 상태를 만든다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.CustomsDeclaration,
                        GroupPurchaseLogisticsEvidenceCode.ImportInspectionResult
                    ],
                    FailureHandlingCodes = ["CustomsHold", "InspectionFailed", "AdditionalDocumentRequired"]
                },
                new()
                {
                    StepCode = "DomesticWarehouseReceived",
                    DisplayName = "국내 물류대행 입고/재포장 확인",
                    Sequence = 50,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.DomesticLogisticsProxy,
                    Description = "국내 물류대행 입고지에서 수량, 온도, 파손 여부를 확인하고 판매 및 국내 배송 가능한 단위로 정리한다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.DomesticWarehouseReceivingReport,
                        GroupPurchaseLogisticsEvidenceCode.LogisticsProxyInboundReceipt,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ],
                    FailureHandlingCodes = ["DomesticReceivingMismatch", "ColdChainBreakAfterImport", "RepackingRequired"]
                },
                new()
                {
                    StepCode = "InventoryLotConfirmed",
                    DisplayName = "공동수입 재고 로트 확정",
                    Sequence = 60,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.DomesticLogisticsProxy,
                    Description = "물류대행사가 입고상품, 판매 가능 수량, 보관 위치를 확정해 판매채널 주문을 출고 배치에 연결할 수 있게 한다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.InventoryLotSnapshot,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ],
                    FailureHandlingCodes = ["InventoryLotMismatch", "MarketableQuantityBlocked", "StorageLocationMissing"]
                },
                new()
                {
                    StepCode = "SalesChannelListed",
                    DisplayName = "스마트스토어/쿠팡 등 판매채널 등록",
                    Sequence = 70,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.SalesChannelOperator,
                    Description = "공동주문 참여자가 판매할 상품을 판매상품과 채널출품으로 연결하고 판매 가능 상태를 확인한다.",
                    RequiredEvidenceCodes = [GroupPurchaseLogisticsEvidenceCode.SalesChannelListingSnapshot],
                    FailureHandlingCodes = ["ListingRejected", "ChannelProductMappingMissing", "PriceOrComplianceReviewRequired"]
                },
                new()
                {
                    StepCode = "OutboundBatchReady",
                    DisplayName = "판매 주문 출고 배치 가능",
                    Sequence = 80,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Platform,
                    Description = "판매채널 주문 동기화 시 입고상품 재고를 기준으로 출고예정을 만들 수 있는 상태를 검증한다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.InventoryLotSnapshot,
                        GroupPurchaseLogisticsEvidenceCode.SalesChannelListingSnapshot,
                        GroupPurchaseLogisticsEvidenceCode.OutboundBatchPlanSnapshot
                    ],
                    FailureHandlingCodes = ["OutboundAllocationFailed", "InsufficientInventory", "WarehouseServiceAreaMismatch"]
                },
                new()
                {
                    StepCode = "DomesticCarrierPickup",
                    DisplayName = "국내 기사 상차 인계",
                    Sequence = 90,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Carrier,
                    Description = "국내 입고지 또는 재포장 거점에서 공동주택 직배송 물량을 국내 기사에게 인계하고 상차 증빙을 남긴다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.PickupPhoto,
                        GroupPurchaseLogisticsEvidenceCode.PickupHandoverReceipt,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ],
                    FailureHandlingCodes = ["PickupQuantityMismatch", "TemperatureOutOfRange"]
                },
                new()
                {
                    StepCode = "ApartmentDropoff",
                    DisplayName = "공동주택 거점 하차",
                    Sequence = 100,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Carrier,
                    Description = "공동주택 지정 거점에 하차하고 대표 수령자에게 인계한다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.DropoffPhoto,
                        GroupPurchaseLogisticsEvidenceCode.GroupRepresentativeReceipt
                    ],
                    FailureHandlingCodes = ["DropoffDelay", "RepresentativeAbsent", "DamageAtDropoff"]
                },
                new()
                {
                    StepCode = "UnitDistribution",
                    DisplayName = "세대별 배분",
                    Sequence = 110,
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.GroupRepresentative,
                    Description = "대표 수령자가 세대별 수량을 분류하고 미수령분을 관리한다.",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.UnitDistributionChecklist,
                        GroupPurchaseLogisticsEvidenceCode.IndividualReceiptConfirmation
                    ],
                    FailureHandlingCodes = ["UnitMissingItem", "UnclaimedStorage", "InternalDistributionDispute"]
                }
            ],
            ResponsibilitySegments =
            [
                new()
                {
                    SegmentCode = "OverseasSellerExport",
                    FromStepCode = "OverseasSellerPacked",
                    ToStepCode = "InternationalTransport",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.OverseasSeller,
                    ResponsibilityScope = "해외 판매자 포장 완료부터 국제 운송 인계 전까지 수량, 포장, 수출 서류, 출고 온도에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.OverseasSellerPackingList,
                        GroupPurchaseLogisticsEvidenceCode.ExportInvoice
                    ]
                },
                new()
                {
                    SegmentCode = "ImportAndCustoms",
                    FromStepCode = "InternationalTransport",
                    ToStepCode = "DomesticWarehouseReceived",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Importer,
                    ResponsibilityScope = "국제 운송, 통관/검역, 국내 입고 전까지 지연, 반입 불가, 온도 이탈, 서류 보완에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.CustomsDeclaration,
                        GroupPurchaseLogisticsEvidenceCode.ImportInspectionResult,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ]
                },
                new()
                {
                    SegmentCode = "LogisticsProxyInventoryCustody",
                    FromStepCode = "DomesticWarehouseReceived",
                    ToStepCode = "InventoryLotConfirmed",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.DomesticLogisticsProxy,
                    ResponsibilityScope = "국내 물류대행 입고 이후 판매 가능 재고 로트 확정 전까지 보관, 재포장, 수량 확인, 냉장/냉동 유지에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.LogisticsProxyInboundReceipt,
                        GroupPurchaseLogisticsEvidenceCode.InventoryLotSnapshot,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ]
                },
                new()
                {
                    SegmentCode = "MarketplaceListingToOutboundBatch",
                    FromStepCode = "InventoryLotConfirmed",
                    ToStepCode = "OutboundBatchReady",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Platform,
                    ResponsibilityScope = "재고 로트 확정 이후 판매상품/채널출품 연결과 판매채널 주문의 출고 배치 가능 상태 검증에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.InventoryLotSnapshot,
                        GroupPurchaseLogisticsEvidenceCode.SalesChannelListingSnapshot,
                        GroupPurchaseLogisticsEvidenceCode.OutboundBatchPlanSnapshot
                    ]
                },
                new()
                {
                    SegmentCode = "DomesticWarehouseToCarrier",
                    FromStepCode = "DomesticWarehouseReceived",
                    ToStepCode = "DomesticCarrierPickup",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.DomesticLogisticsProxy,
                    ResponsibilityScope = "공동주택 직배송 물량의 국내 입고 확인 이후 국내 기사 상차 전까지 보관, 재포장, 수량 확인, 냉장/냉동 유지에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.DomesticWarehouseReceivingReport,
                        GroupPurchaseLogisticsEvidenceCode.LogisticsProxyInboundReceipt,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ]
                },
                new()
                {
                    SegmentCode = "DomesticCarrierTransit",
                    FromStepCode = "DomesticCarrierPickup",
                    ToStepCode = "ApartmentDropoff",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.Carrier,
                    ResponsibilityScope = "국내 기사 상차 인수 이후 공동주택 거점 하차 인계 전까지 운송 지연, 파손, 분실, 온도 유지에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.PickupPhoto,
                        GroupPurchaseLogisticsEvidenceCode.DropoffPhoto,
                        GroupPurchaseLogisticsEvidenceCode.TemperatureLog
                    ]
                },
                new()
                {
                    SegmentCode = "RepresentativeDistribution",
                    FromStepCode = "ApartmentDropoff",
                    ToStepCode = "UnitDistribution",
                    ResponsiblePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.GroupRepresentative,
                    ResponsibilityScope = "대표 수령 이후 세대별 배분, 미수령 보관, 내부 누락 확인에 대한 책임",
                    RequiredEvidenceCodes =
                    [
                        GroupPurchaseLogisticsEvidenceCode.GroupRepresentativeReceipt,
                        GroupPurchaseLogisticsEvidenceCode.UnitDistributionChecklist
                    ]
                }
            ]
        };
}

public sealed class GroupPurchaseLogisticsWorkflowDocument
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string WorkflowId { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string DisplayName { get; set; } = string.Empty;
    public string ProductCategoryCode { get; set; } = string.Empty;
    public string TemperatureCode { get; set; } = string.Empty;
    public string LogisticsMode { get; set; } = string.Empty;
    public string SellerOriginType { get; set; } = GroupPurchaseSellerOriginTypeCode.Domestic;
    public string OrdererGroupScopeType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<GroupPurchaseLogisticsWorkflowStepDocument> Steps { get; set; } = [];
    public IReadOnlyList<GroupPurchaseResponsibilitySegmentDocument> ResponsibilitySegments { get; set; } = [];
    public IReadOnlyList<string> Tags { get; set; } = [];
    public string Memo { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class GroupPurchaseLogisticsWorkflowStepDocument
{
    public string StepCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string ResponsiblePartyCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string> RequiredEvidenceCodes { get; set; } = [];
    public IReadOnlyList<string> FailureHandlingCodes { get; set; } = [];
}

public sealed class GroupPurchaseResponsibilitySegmentDocument
{
    public string SegmentCode { get; set; } = string.Empty;
    public string FromStepCode { get; set; } = string.Empty;
    public string ToStepCode { get; set; } = string.Empty;
    public string ResponsiblePartyCode { get; set; } = string.Empty;
    public string ResponsibilityScope { get; set; } = string.Empty;
    public IReadOnlyList<string> RequiredEvidenceCodes { get; set; } = [];
}
