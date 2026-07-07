using Hongdal.Contracts.Common.Hr;
using Hongdal.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 홍달.Services.Options;

namespace Hongdal.Services.Orderer;

public interface IOrdererGroupOperatingEntityStore
{
    Task<IReadOnlyList<OrdererGroupOperatingEntityDto>> ListAsync(
        OrdererGroupOperatingEntityQuery query,
        CancellationToken cancellationToken = default);

    Task<OrdererGroupOperatingEntityDto?> GetByScopeKeyAsync(
        string ordererGroupScopeKey,
        CancellationToken cancellationToken = default);

    Task<OrdererGroupOperatingEntityDto> UpsertAsync(
        OrdererGroupOperatingEntityUpsertRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public sealed class MongoOrdererGroupOperatingEntityStore : IOrdererGroupOperatingEntityStore
{
    private const string CollectionName = "orderer_group_operating_entities";
    private readonly IMongoCollection<OrdererGroupOperatingEntityDocument> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public MongoOrdererGroupOperatingEntityStore(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<OrdererGroupOperatingEntityDocument>(CollectionName);
    }

    public async Task<IReadOnlyList<OrdererGroupOperatingEntityDto>> ListAsync(
        OrdererGroupOperatingEntityQuery query,
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

    public async Task<OrdererGroupOperatingEntityDto?> GetByScopeKeyAsync(
        string ordererGroupScopeKey,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var normalized = NormalizeRequired(ordererGroupScopeKey, "ordererGroupScopeKey");
        var item = await _collection
            .Find(x => x.OrdererGroupScopeKeyNormalized == normalized)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : ToDto(item);
    }

    public async Task<OrdererGroupOperatingEntityDto> UpsertAsync(
        OrdererGroupOperatingEntityUpsertRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        Validate(request);

        var now = DateTime.UtcNow;
        var scopeKeyNormalized = NormalizeRequired(request.OrdererGroupScopeKey, "ordererGroupScopeKey");
        var existing = await _collection
            .Find(x => x.OrdererGroupScopeKeyNormalized == scopeKeyNormalized)
            .FirstOrDefaultAsync(cancellationToken);
        var entityId = string.IsNullOrWhiteSpace(request.EntityId)
            ? existing?.EntityId ?? ObjectId.GenerateNewId().ToString()
            : request.EntityId.Trim();
        var entityType = NormalizeEntityType(request.EntityType);
        var verificationStatus = NormalizeBusinessVerificationStatus(request.BusinessVerificationStatus, entityType);
        var isVerifiedBusiness = IsBusinessEntity(entityType) && verificationStatus == OrdererGroupBusinessVerificationStatusCode.Verified;
        var canActAsImporter = request.CanActAsImporterOfRecord ?? isVerifiedBusiness;
        var canEmployWorkers = request.CanEmployWorkers ?? isVerifiedBusiness;
        var canIssuePayroll = request.CanIssuePayroll ?? canEmployWorkers;
        var rolePolicies = request.EmploymentRolePolicies.Count == 0
            ? CreateDefaultRolePolicies()
            : request.EmploymentRolePolicies.Select(ToDocument).ToArray();

        var document = new OrdererGroupOperatingEntityDocument
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            EntityId = entityId,
            EntityIdNormalized = NormalizeRequired(entityId, "entityId"),
            OrdererGroupScopeKey = request.OrdererGroupScopeKey.Trim(),
            OrdererGroupScopeKeyNormalized = scopeKeyNormalized,
            OrdererGroupScopeName = request.OrdererGroupScopeName.Trim(),
            EntityType = entityType,
            RepresentativeUserId = request.RepresentativeUserId.Trim(),
            RepresentativeName = request.RepresentativeName.Trim(),
            LegalEntityName = request.LegalEntityName.Trim(),
            BusinessRegistrationNumber = NormalizeBusinessRegistrationNumber(request.BusinessRegistrationNumber),
            MaskedBusinessRegistrationNumber = MaskBusinessRegistrationNumber(request.BusinessRegistrationNumber),
            BusinessVerificationStatus = verificationStatus,
            CanActAsImporterOfRecord = canActAsImporter,
            CanEmployWorkers = canEmployWorkers,
            CanIssuePayroll = canIssuePayroll,
            EmploymentReadinessStatus = ResolveEmploymentReadiness(entityType, verificationStatus, canEmployWorkers, canIssuePayroll),
            ImportCustomsReadinessStatus = string.IsNullOrWhiteSpace(request.ImportCustomsReadinessStatus)
                ? ResolveImportCustomsReadiness(entityType, verificationStatus, canActAsImporter)
                : request.ImportCustomsReadinessStatus.Trim(),
            PayrollSettlementMethod = NormalizePaymentMethod(request.PayrollSettlementMethod),
            EmploymentRolePolicies = rolePolicies,
            RequiredActionCodes = ResolveRequiredActions(request.RequiredActionCodes, entityType, verificationStatus, canActAsImporter, canEmployWorkers, canIssuePayroll),
            AdminMemo = request.AdminMemo.Trim(),
            UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            CreatedAtUtc = existing?.CreatedAtUtc ?? now,
            UpdatedAtUtc = now
        };

        await _collection.ReplaceOneAsync(
            x => x.OrdererGroupScopeKeyNormalized == scopeKeyNormalized,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return ToDto(document);
    }

    private FilterDefinition<OrdererGroupOperatingEntityDocument> BuildFilter(OrdererGroupOperatingEntityQuery query)
    {
        var builder = Builders<OrdererGroupOperatingEntityDocument>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(query.OrdererGroupScopeKey))
        {
            filter &= builder.Eq(x => x.OrdererGroupScopeKeyNormalized, NormalizeRequired(query.OrdererGroupScopeKey, "ordererGroupScopeKey"));
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            filter &= builder.Eq(x => x.EntityType, NormalizeEntityType(query.EntityType));
        }

        if (!string.IsNullOrWhiteSpace(query.BusinessVerificationStatus))
        {
            filter &= builder.Eq(x => x.BusinessVerificationStatus, NormalizeBusinessVerificationStatus(query.BusinessVerificationStatus, null));
        }

        if (query.CanActAsImporterOfRecord.HasValue)
        {
            filter &= builder.Eq(x => x.CanActAsImporterOfRecord, query.CanActAsImporterOfRecord.Value);
        }

        if (query.CanEmployWorkers.HasValue)
        {
            filter &= builder.Eq(x => x.CanEmployWorkers, query.CanEmployWorkers.Value);
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
                new CreateIndexModel<OrdererGroupOperatingEntityDocument>(
                    Builders<OrdererGroupOperatingEntityDocument>.IndexKeys.Ascending(x => x.OrdererGroupScopeKeyNormalized),
                    new CreateIndexOptions { Unique = true, Name = "ux_orderer_group_scope_key" }),
                new CreateIndexModel<OrdererGroupOperatingEntityDocument>(
                    Builders<OrdererGroupOperatingEntityDocument>.IndexKeys
                        .Ascending(x => x.EntityType)
                        .Ascending(x => x.BusinessVerificationStatus)
                        .Descending(x => x.UpdatedAtUtc),
                    new CreateIndexOptions { Name = "ix_entity_type_verification_updated" }),
                new CreateIndexModel<OrdererGroupOperatingEntityDocument>(
                    Builders<OrdererGroupOperatingEntityDocument>.IndexKeys
                        .Ascending(x => x.CanActAsImporterOfRecord)
                        .Ascending(x => x.CanEmployWorkers)
                        .Descending(x => x.UpdatedAtUtc),
                    new CreateIndexOptions { Name = "ix_capability_updated" })
            };

            await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static void Validate(OrdererGroupOperatingEntityUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OrdererGroupScopeKey)) throw new InvalidOperationException("ordererGroupScopeKey is required.");
        if (string.IsNullOrWhiteSpace(request.OrdererGroupScopeName)) throw new InvalidOperationException("ordererGroupScopeName is required.");
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeEntityType(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, OrdererGroupOperatingEntityTypeCode.IndividualBusiness, StringComparison.OrdinalIgnoreCase)) return OrdererGroupOperatingEntityTypeCode.IndividualBusiness;
        if (string.Equals(normalized, OrdererGroupOperatingEntityTypeCode.Corporation, StringComparison.OrdinalIgnoreCase)) return OrdererGroupOperatingEntityTypeCode.Corporation;
        if (string.Equals(normalized, OrdererGroupOperatingEntityTypeCode.Cooperative, StringComparison.OrdinalIgnoreCase)) return OrdererGroupOperatingEntityTypeCode.Cooperative;
        if (string.Equals(normalized, OrdererGroupOperatingEntityTypeCode.ManagementOfficeEntrusted, StringComparison.OrdinalIgnoreCase)) return OrdererGroupOperatingEntityTypeCode.ManagementOfficeEntrusted;
        return string.Equals(normalized, OrdererGroupOperatingEntityTypeCode.PlatformEntrusted, StringComparison.OrdinalIgnoreCase)
            ? OrdererGroupOperatingEntityTypeCode.PlatformEntrusted
            : OrdererGroupOperatingEntityTypeCode.InformalGroup;
    }

    private static string NormalizeBusinessVerificationStatus(string? value, string? entityType)
    {
        if (entityType == OrdererGroupOperatingEntityTypeCode.InformalGroup)
        {
            return OrdererGroupBusinessVerificationStatusCode.NotRequired;
        }

        var normalized = value?.Trim();
        if (string.Equals(normalized, OrdererGroupBusinessVerificationStatusCode.NotRequired, StringComparison.OrdinalIgnoreCase)) return OrdererGroupBusinessVerificationStatusCode.NotRequired;
        if (string.Equals(normalized, OrdererGroupBusinessVerificationStatusCode.Pending, StringComparison.OrdinalIgnoreCase)) return OrdererGroupBusinessVerificationStatusCode.Pending;
        if (string.Equals(normalized, OrdererGroupBusinessVerificationStatusCode.Verified, StringComparison.OrdinalIgnoreCase)) return OrdererGroupBusinessVerificationStatusCode.Verified;
        if (string.Equals(normalized, OrdererGroupBusinessVerificationStatusCode.Rejected, StringComparison.OrdinalIgnoreCase)) return OrdererGroupBusinessVerificationStatusCode.Rejected;
        return OrdererGroupBusinessVerificationStatusCode.Required;
    }

    private static bool IsBusinessEntity(string entityType)
        => entityType is OrdererGroupOperatingEntityTypeCode.IndividualBusiness
            or OrdererGroupOperatingEntityTypeCode.Corporation
            or OrdererGroupOperatingEntityTypeCode.Cooperative
            or OrdererGroupOperatingEntityTypeCode.ManagementOfficeEntrusted
            or OrdererGroupOperatingEntityTypeCode.PlatformEntrusted;

    private static string ResolveEmploymentReadiness(string entityType, string verificationStatus, bool canEmployWorkers, bool canIssuePayroll)
    {
        if (!IsBusinessEntity(entityType))
        {
            return OrdererGroupEmploymentReadinessStatusCode.NeedsBusinessEntity;
        }

        if (verificationStatus != OrdererGroupBusinessVerificationStatusCode.Verified)
        {
            return OrdererGroupEmploymentReadinessStatusCode.NotReady;
        }

        return canEmployWorkers && canIssuePayroll
            ? OrdererGroupEmploymentReadinessStatusCode.ReadyForDraftContract
            : OrdererGroupEmploymentReadinessStatusCode.NotReady;
    }

    private static string ResolveImportCustomsReadiness(string entityType, string verificationStatus, bool canActAsImporter)
    {
        if (!IsBusinessEntity(entityType))
        {
            return "NeedsBusinessEntity";
        }

        if (verificationStatus != OrdererGroupBusinessVerificationStatusCode.Verified)
        {
            return "NeedsBusinessVerification";
        }

        return canActAsImporter ? "Ready" : "UseProxyImporter";
    }

    private static IReadOnlyList<string> ResolveRequiredActions(
        IReadOnlyList<string> requested,
        string entityType,
        string verificationStatus,
        bool canActAsImporter,
        bool canEmployWorkers,
        bool canIssuePayroll)
    {
        var items = requested.Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        if (!IsBusinessEntity(entityType)) items.Add("ChooseBusinessEntityOrEntrustedOperator");
        if (verificationStatus == OrdererGroupBusinessVerificationStatusCode.Required) items.Add("VerifyBusinessRegistration");
        if (verificationStatus == OrdererGroupBusinessVerificationStatusCode.Pending) items.Add("WaitBusinessVerification");
        if (!canActAsImporter) items.Add("AssignImporterOfRecordOrProxy");
        if (!canEmployWorkers) items.Add("SelectEmploymentResponsibleEntity");
        if (!canIssuePayroll) items.Add("ConfigurePayrollSettlement");
        return items.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<OrdererGroupEmploymentRolePolicyDocument> CreateDefaultRolePolicies()
        =>
        [
            new()
            {
                RoleCode = HrDetailedRoleCodes.OrdererGroupSortingWorker,
                RoleName = "공동주문 입고 분류 알바",
                ParticipantCategory = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                WorkerSourcePreference = OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred,
                InternalResidentPreferred = true,
                ExternalWorkerAllowed = false,
                ContractType = HrEmploymentContractTypes.PartTime,
                WageType = HrWageTypes.Hourly,
                PaymentCycle = HrPaymentCycles.Monthly,
                WorkDescriptionTemplate = "공동수입 물품 입고 확인, 세대/판매 단위 분류, 수량 검수 보조",
                RequiresSignedContractBeforeWork = true
            },
            new()
            {
                RoleCode = HrDetailedRoleCodes.OrdererGroupDistributionWorker,
                RoleName = "공동주문 단지 내 배분 알바",
                ParticipantCategory = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                WorkerSourcePreference = OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred,
                InternalResidentPreferred = true,
                ExternalWorkerAllowed = false,
                ContractType = HrEmploymentContractTypes.PartTime,
                WageType = HrWageTypes.Hourly,
                PaymentCycle = HrPaymentCycles.Monthly,
                WorkDescriptionTemplate = "공동주택 거점 수령 이후 세대별 배분, 미수령 물품 관리 보조",
                RequiresSignedContractBeforeWork = true
            },
            new()
            {
                RoleCode = HrDetailedRoleCodes.OrdererGroupParcelAggregationWorker,
                RoleName = "택배/공동구매 물품 집합 보조",
                ParticipantCategory = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                WorkerSourcePreference = OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred,
                InternalResidentPreferred = true,
                ExternalWorkerAllowed = false,
                ContractType = HrEmploymentContractTypes.PartTime,
                WageType = HrWageTypes.Hourly,
                PaymentCycle = HrPaymentCycles.Monthly,
                WorkDescriptionTemplate = "단지로 들어오는 택배, 공동구매, 공동수입 물품을 지정 장소에 집합하고 수량/보관 상태를 확인",
                RequiresSignedContractBeforeWork = true
            },
            new()
            {
                RoleCode = HrDetailedRoleCodes.OrdererGroupSecurityWorker,
                RoleName = "단지 내부 경비/순찰 보조",
                ParticipantCategory = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                WorkerSourcePreference = OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred,
                InternalResidentPreferred = true,
                ExternalWorkerAllowed = false,
                ContractType = HrEmploymentContractTypes.PartTime,
                WageType = HrWageTypes.Hourly,
                PaymentCycle = HrPaymentCycles.Monthly,
                WorkDescriptionTemplate = "단지 내부 경비, 순찰, 공동 물품 반입 시간대 안내와 거점 질서 유지 보조",
                RequiresSignedContractBeforeWork = true
            },
            new()
            {
                RoleCode = HrDetailedRoleCodes.OrdererGroupCommunityFacilityWorker,
                RoleName = "공동주택 관리 보조",
                ParticipantCategory = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                WorkerSourcePreference = OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred,
                InternalResidentPreferred = true,
                ExternalWorkerAllowed = false,
                ContractType = HrEmploymentContractTypes.PartTime,
                WageType = HrWageTypes.Hourly,
                PaymentCycle = HrPaymentCycles.Monthly,
                WorkDescriptionTemplate = "공동주택 공용공간 관리, 물품 보관 장소 정리, 공동 작업 일정 안내 보조",
                RequiresSignedContractBeforeWork = true
            }
        ];

    private static string NormalizePaymentMethod(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, HrPaymentMethods.BankTransfer, StringComparison.OrdinalIgnoreCase)) return HrPaymentMethods.BankTransfer;
        if (string.Equals(normalized, HrPaymentMethods.Cash, StringComparison.OrdinalIgnoreCase)) return HrPaymentMethods.Cash;
        return HrPaymentMethods.PlatformSettlement;
    }

    private static string NormalizeBusinessRegistrationNumber(string? value)
        => new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string MaskBusinessRegistrationNumber(string? value)
    {
        var digits = NormalizeBusinessRegistrationNumber(value);
        return digits.Length == 10
            ? $"{digits[..3]}-**-{digits[^5..]}"
            : string.Empty;
    }

    private static string NormalizeWorkerSourcePreference(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, OrdererGroupWorkerSourcePreferenceCode.InternalResidentOnly, StringComparison.OrdinalIgnoreCase)) return OrdererGroupWorkerSourcePreferenceCode.InternalResidentOnly;
        return string.Equals(normalized, OrdererGroupWorkerSourcePreferenceCode.ExternalAllowed, StringComparison.OrdinalIgnoreCase)
            ? OrdererGroupWorkerSourcePreferenceCode.ExternalAllowed
            : OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred;
    }

    private static OrdererGroupEmploymentRolePolicyDocument ToDocument(OrdererGroupEmploymentRolePolicyDto source)
        => new()
        {
            RoleCode = string.IsNullOrWhiteSpace(source.RoleCode) ? HrDetailedRoleCodes.OrdererGroupSortingWorker : source.RoleCode.Trim(),
            RoleName = source.RoleName.Trim(),
            ParticipantCategory = HrParticipantCategoryCodes.Normalize(source.ParticipantCategory),
            WorkerSourcePreference = NormalizeWorkerSourcePreference(source.WorkerSourcePreference),
            InternalResidentPreferred = source.InternalResidentPreferred || NormalizeWorkerSourcePreference(source.WorkerSourcePreference) is OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred or OrdererGroupWorkerSourcePreferenceCode.InternalResidentOnly,
            ExternalWorkerAllowed = source.ExternalWorkerAllowed || NormalizeWorkerSourcePreference(source.WorkerSourcePreference) == OrdererGroupWorkerSourcePreferenceCode.ExternalAllowed,
            ContractType = source.ContractType.Trim(),
            WageType = source.WageType.Trim(),
            PaymentCycle = source.PaymentCycle.Trim(),
            WorkDescriptionTemplate = source.WorkDescriptionTemplate.Trim(),
            RequiresSignedContractBeforeWork = source.RequiresSignedContractBeforeWork
        };

    private static OrdererGroupOperatingEntityDto ToDto(OrdererGroupOperatingEntityDocument source)
        => new()
        {
            EntityId = source.EntityId,
            OrdererGroupScopeKey = source.OrdererGroupScopeKey,
            OrdererGroupScopeName = source.OrdererGroupScopeName,
            EmploymentEmployerScopeType = HrScopeTypes.OrdererGroup,
            EmploymentEmployerScopeId = source.OrdererGroupScopeKey,
            EntityType = source.EntityType,
            RepresentativeUserId = source.RepresentativeUserId,
            RepresentativeName = source.RepresentativeName,
            LegalEntityName = source.LegalEntityName,
            BusinessRegistrationNumber = source.BusinessRegistrationNumber,
            MaskedBusinessRegistrationNumber = source.MaskedBusinessRegistrationNumber,
            BusinessVerificationStatus = source.BusinessVerificationStatus,
            CanActAsImporterOfRecord = source.CanActAsImporterOfRecord,
            CanEmployWorkers = source.CanEmployWorkers,
            CanIssuePayroll = source.CanIssuePayroll,
            EmploymentReadinessStatus = source.EmploymentReadinessStatus,
            ImportCustomsReadinessStatus = source.ImportCustomsReadinessStatus,
            PayrollSettlementMethod = source.PayrollSettlementMethod,
            EmploymentRolePolicies = source.EmploymentRolePolicies.Select(ToDto).ToArray(),
            RequiredActionCodes = source.RequiredActionCodes,
            AdminMemo = source.AdminMemo,
            UpdatedBy = source.UpdatedBy,
            CreatedAtUtc = source.CreatedAtUtc,
            UpdatedAtUtc = source.UpdatedAtUtc
        };

    private static OrdererGroupEmploymentRolePolicyDto ToDto(OrdererGroupEmploymentRolePolicyDocument source)
        => new()
        {
            RoleCode = source.RoleCode,
            RoleName = source.RoleName,
            ParticipantCategory = source.ParticipantCategory,
            WorkerSourcePreference = source.WorkerSourcePreference,
            InternalResidentPreferred = source.InternalResidentPreferred,
            ExternalWorkerAllowed = source.ExternalWorkerAllowed,
            ContractType = source.ContractType,
            WageType = source.WageType,
            PaymentCycle = source.PaymentCycle,
            WorkDescriptionTemplate = source.WorkDescriptionTemplate,
            RequiresSignedContractBeforeWork = source.RequiresSignedContractBeforeWork
        };
}

public static class OrdererGroupOperatingEntityProjection
{
    public static OrdererGroupOperatingEntityPublicDto ToPublicDto(OrdererGroupOperatingEntityDto source)
        => new()
        {
            OrdererGroupScopeKey = source.OrdererGroupScopeKey,
            OrdererGroupScopeName = source.OrdererGroupScopeName,
            EmploymentEmployerScopeType = HrScopeTypes.OrdererGroup,
            EmploymentEmployerScopeId = source.OrdererGroupScopeKey,
            EntityType = source.EntityType,
            LegalEntityName = source.LegalEntityName,
            MaskedBusinessRegistrationNumber = source.MaskedBusinessRegistrationNumber,
            BusinessVerificationStatus = source.BusinessVerificationStatus,
            CanActAsImporterOfRecord = source.CanActAsImporterOfRecord,
            CanEmployWorkers = source.CanEmployWorkers,
            CanIssuePayroll = source.CanIssuePayroll,
            EmploymentReadinessStatus = source.EmploymentReadinessStatus,
            ImportCustomsReadinessStatus = source.ImportCustomsReadinessStatus,
            EmploymentRolePolicies = source.EmploymentRolePolicies,
            RequiredActionCodes = source.RequiredActionCodes,
            UpdatedAtUtc = source.UpdatedAtUtc
        };
}

public sealed class OrdererGroupOperatingEntityDocument
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string EntityIdNormalized { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeKeyNormalized { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string EntityType { get; set; } = OrdererGroupOperatingEntityTypeCode.InformalGroup;
    public string RepresentativeUserId { get; set; } = string.Empty;
    public string RepresentativeName { get; set; } = string.Empty;
    public string LegalEntityName { get; set; } = string.Empty;
    public string BusinessRegistrationNumber { get; set; } = string.Empty;
    public string MaskedBusinessRegistrationNumber { get; set; } = string.Empty;
    public string BusinessVerificationStatus { get; set; } = OrdererGroupBusinessVerificationStatusCode.Required;
    public bool CanActAsImporterOfRecord { get; set; }
    public bool CanEmployWorkers { get; set; }
    public bool CanIssuePayroll { get; set; }
    public string EmploymentReadinessStatus { get; set; } = OrdererGroupEmploymentReadinessStatusCode.NotReady;
    public string ImportCustomsReadinessStatus { get; set; } = string.Empty;
    public string PayrollSettlementMethod { get; set; } = HrPaymentMethods.PlatformSettlement;
    public IReadOnlyList<OrdererGroupEmploymentRolePolicyDocument> EmploymentRolePolicies { get; set; } = [];
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string AdminMemo { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class OrdererGroupEmploymentRolePolicyDocument
{
    public string RoleCode { get; set; } = HrDetailedRoleCodes.OrdererGroupSortingWorker;
    public string RoleName { get; set; } = string.Empty;
    public string ParticipantCategory { get; set; } = HrParticipantCategoryCodes.CommunityPartTimeWorker;
    public string WorkerSourcePreference { get; set; } = OrdererGroupWorkerSourcePreferenceCode.InternalResidentPreferred;
    public bool InternalResidentPreferred { get; set; } = true;
    public bool ExternalWorkerAllowed { get; set; }
    public string ContractType { get; set; } = HrEmploymentContractTypes.PartTime;
    public string WageType { get; set; } = HrWageTypes.Hourly;
    public string PaymentCycle { get; set; } = HrPaymentCycles.Monthly;
    public string WorkDescriptionTemplate { get; set; } = string.Empty;
    public bool RequiresSignedContractBeforeWork { get; set; } = true;
}
