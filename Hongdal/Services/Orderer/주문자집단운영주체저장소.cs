using Hongdal.Contracts.Common.Hr;
using Hongdal.Contracts.Common.Orderer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 홍달.Services.Options;

namespace Hongdal.Services.Orderer;

public interface I주문자집단운영주체저장소
{
    Task<IReadOnlyList<주문자집단운영주체Dto>> 목록조회Async(
        주문자집단운영주체조회조건 query,
        CancellationToken cancellationToken = default);

    Task<주문자집단운영주체Dto?> 배송권키로조회Async(
        string ordererGroupScopeKey,
        CancellationToken cancellationToken = default);

    Task<주문자집단운영주체Dto> 저장Async(
        주문자집단운영주체저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public sealed class Mongo주문자집단운영주체저장소 : I주문자집단운영주체저장소
{
    private const string CollectionName = "orderer_group_operating_entities";
    private readonly IMongoCollection<주문자집단운영주체문서> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public Mongo주문자집단운영주체저장소(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var databaseName = options.Value.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(databaseName.Trim())
            .GetCollection<주문자집단운영주체문서>(CollectionName);
    }

    public async Task<IReadOnlyList<주문자집단운영주체Dto>> 목록조회Async(
        주문자집단운영주체조회조건 query,
        CancellationToken cancellationToken = default)
    {
        await 인덱스준비Async(cancellationToken);

        var items = await _collection
            .Find(필터구성(query))
            .SortByDescending(x => x.수정시각Utc)
            .Limit(200)
            .ToListAsync(cancellationToken);

        return items.Select(Dto로).ToArray();
    }

    public async Task<주문자집단운영주체Dto?> 배송권키로조회Async(
        string ordererGroupScopeKey,
        CancellationToken cancellationToken = default)
    {
        await 인덱스준비Async(cancellationToken);

        var normalized = 필수값정규화(ordererGroupScopeKey, "ordererGroupScopeKey");
        var item = await _collection
            .Find(x => x.주문자집단배송권키정규화 == normalized)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : Dto로(item);
    }

    public async Task<주문자집단운영주체Dto> 저장Async(
        주문자집단운영주체저장요청 request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await 인덱스준비Async(cancellationToken);
        검증(request);

        var now = DateTime.UtcNow;
        var scopeKeyNormalized = 필수값정규화(request.주문자집단배송권키, "ordererGroupScopeKey");
        var existing = await _collection
            .Find(x => x.주문자집단배송권키정규화 == scopeKeyNormalized)
            .FirstOrDefaultAsync(cancellationToken);
        var entityId = string.IsNullOrWhiteSpace(request.운영주체Id)
            ? existing?.운영주체Id ?? ObjectId.GenerateNewId().ToString()
            : request.운영주체Id.Trim();
        var entityType = 운영주체유형정규화(request.운영주체유형);
        var verificationStatus = 사업자검증상태정규화(request.사업자검증상태, entityType);
        var is검증완료Business = 사업자주체인가(entityType) && verificationStatus == 주문자집단사업자검증상태코드.검증완료;
        var canActAsImporter = request.수입자역할가능 ?? is검증완료Business;
        var canEmployWorkers = request.고용가능 ?? is검증완료Business;
        var canIssuePayroll = request.급여지급가능 ?? canEmployWorkers;
        var rolePolicies = request.고용역할정책목록.Count == 0
            ? 기본역할정책생성()
            : request.고용역할정책목록.Select(문서로).ToArray();

        var document = new 주문자집단운영주체문서
        {
            Id = existing?.Id ?? ObjectId.GenerateNewId(),
            운영주체Id = entityId,
            운영주체Id정규화 = 필수값정규화(entityId, "entityId"),
            주문자집단배송권키 = request.주문자집단배송권키.Trim(),
            주문자집단배송권키정규화 = scopeKeyNormalized,
            주문자집단배송권명 = request.주문자집단배송권명.Trim(),
            운영주체유형 = entityType,
            대표UserId = request.대표UserId.Trim(),
            대표자명 = request.대표자명.Trim(),
            법적주체명 = request.법적주체명.Trim(),
            사업자등록번호 = 사업자등록번호정규화(request.사업자등록번호),
            마스킹사업자등록번호 = 사업자등록번호마스킹(request.사업자등록번호),
            사업자검증상태 = verificationStatus,
            수입자역할가능 = canActAsImporter,
            고용가능 = canEmployWorkers,
            급여지급가능 = canIssuePayroll,
            고용준비상태 = 고용준비상태계산(entityType, verificationStatus, canEmployWorkers, canIssuePayroll),
            수입통관준비상태 = string.IsNullOrWhiteSpace(request.수입통관준비상태)
                ? 수입통관준비상태계산(entityType, verificationStatus, canActAsImporter)
                : request.수입통관준비상태.Trim(),
            급여정산방식 = 지급방식정규화(request.급여정산방식),
            고용역할정책목록 = rolePolicies,
            필요조치코드목록 = 필요조치계산(request.필요조치코드목록, entityType, verificationStatus, canActAsImporter, canEmployWorkers, canIssuePayroll),
            관리자메모 = request.관리자메모.Trim(),
            수정자 = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy.Trim(),
            생성시각Utc = existing?.생성시각Utc ?? now,
            수정시각Utc = now
        };

        await _collection.ReplaceOneAsync(
            x => x.주문자집단배송권키정규화 == scopeKeyNormalized,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return Dto로(document);
    }

    private FilterDefinition<주문자집단운영주체문서> 필터구성(주문자집단운영주체조회조건 query)
    {
        var builder = Builders<주문자집단운영주체문서>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(query.주문자집단배송권키))
        {
            filter &= builder.Eq(x => x.주문자집단배송권키정규화, 필수값정규화(query.주문자집단배송권키, "ordererGroupScopeKey"));
        }

        if (!string.IsNullOrWhiteSpace(query.운영주체유형))
        {
            filter &= builder.Eq(x => x.운영주체유형, 운영주체유형정규화(query.운영주체유형));
        }

        if (!string.IsNullOrWhiteSpace(query.사업자검증상태))
        {
            filter &= builder.Eq(x => x.사업자검증상태, 사업자검증상태정규화(query.사업자검증상태, null));
        }

        if (query.수입자역할가능.HasValue)
        {
            filter &= builder.Eq(x => x.수입자역할가능, query.수입자역할가능.Value);
        }

        if (query.고용가능.HasValue)
        {
            filter &= builder.Eq(x => x.고용가능, query.고용가능.Value);
        }

        return filter;
    }

    private async Task 인덱스준비Async(CancellationToken cancellationToken)
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
                new CreateIndexModel<주문자집단운영주체문서>(
                    Builders<주문자집단운영주체문서>.IndexKeys.Ascending(x => x.주문자집단배송권키정규화),
                    new CreateIndexOptions { Unique = true, Name = "ux_orderer_group_scope_key" }),
                new CreateIndexModel<주문자집단운영주체문서>(
                    Builders<주문자집단운영주체문서>.IndexKeys
                        .Ascending(x => x.운영주체유형)
                        .Ascending(x => x.사업자검증상태)
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_entity_type_verification_updated" }),
                new CreateIndexModel<주문자집단운영주체문서>(
                    Builders<주문자집단운영주체문서>.IndexKeys
                        .Ascending(x => x.수입자역할가능)
                        .Ascending(x => x.고용가능)
                        .Descending(x => x.수정시각Utc),
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

    private static void 검증(주문자집단운영주체저장요청 request)
    {
        if (string.IsNullOrWhiteSpace(request.주문자집단배송권키)) throw new InvalidOperationException("ordererGroupScopeKey is required.");
        if (string.IsNullOrWhiteSpace(request.주문자집단배송권명)) throw new InvalidOperationException("ordererGroupScopeName is required.");
    }

    private static string 필수값정규화(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string 운영주체유형정규화(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, 주문자집단운영주체유형코드.개인사업자, StringComparison.OrdinalIgnoreCase)) return 주문자집단운영주체유형코드.개인사업자;
        if (string.Equals(normalized, 주문자집단운영주체유형코드.법인, StringComparison.OrdinalIgnoreCase)) return 주문자집단운영주체유형코드.법인;
        if (string.Equals(normalized, 주문자집단운영주체유형코드.협동조합, StringComparison.OrdinalIgnoreCase)) return 주문자집단운영주체유형코드.협동조합;
        if (string.Equals(normalized, 주문자집단운영주체유형코드.관리사무소위임, StringComparison.OrdinalIgnoreCase)) return 주문자집단운영주체유형코드.관리사무소위임;
        return string.Equals(normalized, 주문자집단운영주체유형코드.플랫폼위임, StringComparison.OrdinalIgnoreCase)
            ? 주문자집단운영주체유형코드.플랫폼위임
            : 주문자집단운영주체유형코드.비사업자모임;
    }

    private static string 사업자검증상태정규화(string? value, string? entityType)
    {
        if (entityType == 주문자집단운영주체유형코드.비사업자모임)
        {
            return 주문자집단사업자검증상태코드.불필요;
        }

        var normalized = value?.Trim();
        if (string.Equals(normalized, 주문자집단사업자검증상태코드.불필요, StringComparison.OrdinalIgnoreCase)) return 주문자집단사업자검증상태코드.불필요;
        if (string.Equals(normalized, 주문자집단사업자검증상태코드.대기, StringComparison.OrdinalIgnoreCase)) return 주문자집단사업자검증상태코드.대기;
        if (string.Equals(normalized, 주문자집단사업자검증상태코드.검증완료, StringComparison.OrdinalIgnoreCase)) return 주문자집단사업자검증상태코드.검증완료;
        if (string.Equals(normalized, 주문자집단사업자검증상태코드.반려, StringComparison.OrdinalIgnoreCase)) return 주문자집단사업자검증상태코드.반려;
        return 주문자집단사업자검증상태코드.필요;
    }

    private static bool 사업자주체인가(string entityType)
        => entityType is 주문자집단운영주체유형코드.개인사업자
            or 주문자집단운영주체유형코드.법인
            or 주문자집단운영주체유형코드.협동조합
            or 주문자집단운영주체유형코드.관리사무소위임
            or 주문자집단운영주체유형코드.플랫폼위임;

    private static string 고용준비상태계산(string entityType, string verificationStatus, bool canEmployWorkers, bool canIssuePayroll)
    {
        if (!사업자주체인가(entityType))
        {
            return 주문자집단고용준비상태코드.사업자주체필요;
        }

        if (verificationStatus != 주문자집단사업자검증상태코드.검증완료)
        {
            return 주문자집단고용준비상태코드.미준비;
        }

        return canEmployWorkers && canIssuePayroll
            ? 주문자집단고용준비상태코드.계약초안가능
            : 주문자집단고용준비상태코드.미준비;
    }

    private static string 수입통관준비상태계산(string entityType, string verificationStatus, bool canActAsImporter)
    {
        if (!사업자주체인가(entityType))
        {
            return "NeedsBusinessEntity";
        }

        if (verificationStatus != 주문자집단사업자검증상태코드.검증완료)
        {
            return "NeedsBusinessVerification";
        }

        return canActAsImporter ? "Ready" : "UseProxyImporter";
    }

    private static IReadOnlyList<string> 필요조치계산(
        IReadOnlyList<string> requested,
        string entityType,
        string verificationStatus,
        bool canActAsImporter,
        bool canEmployWorkers,
        bool canIssuePayroll)
    {
        var items = requested.Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        if (!사업자주체인가(entityType)) items.Add("ChooseBusinessEntityOrEntrustedOperator");
        if (verificationStatus == 주문자집단사업자검증상태코드.필요) items.Add("VerifyBusinessRegistration");
        if (verificationStatus == 주문자집단사업자검증상태코드.대기) items.Add("WaitBusinessVerification");
        if (!canActAsImporter) items.Add("AssignImporterOfRecordOrProxy");
        if (!canEmployWorkers) items.Add("SelectEmploymentResponsibleEntity");
        if (!canIssuePayroll) items.Add("ConfigurePayrollSettlement");
        return items.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<주문자집단고용역할정책문서> 기본역할정책생성()
        =>
        [
            new()
            {
                역할코드 = HrDetailedRoleCodes.OrdererGroupSortingWorker,
                역할명 = "공동주문 입고 분류 알바",
                참여자분류 = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                근로자출처선호 = 주문자집단근로자출처선호코드.입주민우선,
                입주민우선 = true,
                외부근로자허용 = false,
                계약유형 = HrEmploymentContractTypes.PartTime,
                임금유형 = HrWageTypes.Hourly,
                지급주기 = HrPaymentCycles.Monthly,
                업무설명템플릿 = "공동수입 물품 입고 확인, 세대/판매 단위 분류, 수량 검수 보조",
                근로전서명계약필요 = true
            },
            new()
            {
                역할코드 = HrDetailedRoleCodes.OrdererGroupDistributionWorker,
                역할명 = "공동주문 단지 내 배분 알바",
                참여자분류 = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                근로자출처선호 = 주문자집단근로자출처선호코드.입주민우선,
                입주민우선 = true,
                외부근로자허용 = false,
                계약유형 = HrEmploymentContractTypes.PartTime,
                임금유형 = HrWageTypes.Hourly,
                지급주기 = HrPaymentCycles.Monthly,
                업무설명템플릿 = "공동주택 거점 수령 이후 세대별 배분, 미수령 물품 관리 보조",
                근로전서명계약필요 = true
            },
            new()
            {
                역할코드 = HrDetailedRoleCodes.OrdererGroupParcelAggregationWorker,
                역할명 = "택배/공동구매 물품 집합 보조",
                참여자분류 = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                근로자출처선호 = 주문자집단근로자출처선호코드.입주민우선,
                입주민우선 = true,
                외부근로자허용 = false,
                계약유형 = HrEmploymentContractTypes.PartTime,
                임금유형 = HrWageTypes.Hourly,
                지급주기 = HrPaymentCycles.Monthly,
                업무설명템플릿 = "단지로 들어오는 택배, 공동구매, 공동수입 물품을 지정 장소에 집합하고 수량/보관 상태를 확인",
                근로전서명계약필요 = true
            },
            new()
            {
                역할코드 = HrDetailedRoleCodes.OrdererGroupSecurityWorker,
                역할명 = "단지 내부 경비/순찰 보조",
                참여자분류 = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                근로자출처선호 = 주문자집단근로자출처선호코드.입주민우선,
                입주민우선 = true,
                외부근로자허용 = false,
                계약유형 = HrEmploymentContractTypes.PartTime,
                임금유형 = HrWageTypes.Hourly,
                지급주기 = HrPaymentCycles.Monthly,
                업무설명템플릿 = "단지 내부 경비, 순찰, 공동 물품 반입 시간대 안내와 거점 질서 유지 보조",
                근로전서명계약필요 = true
            },
            new()
            {
                역할코드 = HrDetailedRoleCodes.OrdererGroupCommunityFacilityWorker,
                역할명 = "공동주택 관리 보조",
                참여자분류 = HrParticipantCategoryCodes.CommunityPartTimeWorker,
                근로자출처선호 = 주문자집단근로자출처선호코드.입주민우선,
                입주민우선 = true,
                외부근로자허용 = false,
                계약유형 = HrEmploymentContractTypes.PartTime,
                임금유형 = HrWageTypes.Hourly,
                지급주기 = HrPaymentCycles.Monthly,
                업무설명템플릿 = "공동주택 공용공간 관리, 물품 보관 장소 정리, 공동 작업 일정 안내 보조",
                근로전서명계약필요 = true
            }
        ];

    private static string 지급방식정규화(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, HrPaymentMethods.BankTransfer, StringComparison.OrdinalIgnoreCase)) return HrPaymentMethods.BankTransfer;
        if (string.Equals(normalized, HrPaymentMethods.Cash, StringComparison.OrdinalIgnoreCase)) return HrPaymentMethods.Cash;
        return HrPaymentMethods.PlatformSettlement;
    }

    private static string 사업자등록번호정규화(string? value)
        => new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string 사업자등록번호마스킹(string? value)
    {
        var digits = 사업자등록번호정규화(value);
        return digits.Length == 10
            ? $"{digits[..3]}-**-{digits[^5..]}"
            : string.Empty;
    }

    private static string Normalize근로자출처선호(string? value)
    {
        var normalized = value?.Trim();
        if (string.Equals(normalized, 주문자집단근로자출처선호코드.입주민만, StringComparison.OrdinalIgnoreCase)) return 주문자집단근로자출처선호코드.입주민만;
        return string.Equals(normalized, 주문자집단근로자출처선호코드.외부허용, StringComparison.OrdinalIgnoreCase)
            ? 주문자집단근로자출처선호코드.외부허용
            : 주문자집단근로자출처선호코드.입주민우선;
    }

    private static 주문자집단고용역할정책문서 문서로(주문자집단고용역할정책Dto source)
        => new()
        {
            역할코드 = string.IsNullOrWhiteSpace(source.역할코드) ? HrDetailedRoleCodes.OrdererGroupSortingWorker : source.역할코드.Trim(),
            역할명 = source.역할명.Trim(),
            참여자분류 = HrParticipantCategoryCodes.Normalize(source.참여자분류),
            근로자출처선호 = Normalize근로자출처선호(source.근로자출처선호),
            입주민우선 = source.입주민우선 || Normalize근로자출처선호(source.근로자출처선호) is 주문자집단근로자출처선호코드.입주민우선 or 주문자집단근로자출처선호코드.입주민만,
            외부근로자허용 = source.외부근로자허용 || Normalize근로자출처선호(source.근로자출처선호) == 주문자집단근로자출처선호코드.외부허용,
            계약유형 = source.계약유형.Trim(),
            임금유형 = source.임금유형.Trim(),
            지급주기 = source.지급주기.Trim(),
            업무설명템플릿 = source.업무설명템플릿.Trim(),
            근로전서명계약필요 = source.근로전서명계약필요
        };

    private static 주문자집단운영주체Dto Dto로(주문자집단운영주체문서 source)
        => new()
        {
            운영주체Id = source.운영주체Id,
            주문자집단배송권키 = source.주문자집단배송권키,
            주문자집단배송권명 = source.주문자집단배송권명,
            고용주체범위유형 = HrScopeTypes.OrdererGroup,
            고용주체범위Id = source.주문자집단배송권키,
            운영주체유형 = source.운영주체유형,
            대표UserId = source.대표UserId,
            대표자명 = source.대표자명,
            법적주체명 = source.법적주체명,
            사업자등록번호 = source.사업자등록번호,
            마스킹사업자등록번호 = source.마스킹사업자등록번호,
            사업자검증상태 = source.사업자검증상태,
            수입자역할가능 = source.수입자역할가능,
            고용가능 = source.고용가능,
            급여지급가능 = source.급여지급가능,
            고용준비상태 = source.고용준비상태,
            수입통관준비상태 = source.수입통관준비상태,
            급여정산방식 = source.급여정산방식,
            고용역할정책목록 = source.고용역할정책목록.Select(Dto로).ToArray(),
            필요조치코드목록 = source.필요조치코드목록,
            관리자메모 = source.관리자메모,
            수정자 = source.수정자,
            생성시각Utc = source.생성시각Utc,
            수정시각Utc = source.수정시각Utc
        };

    private static 주문자집단고용역할정책Dto Dto로(주문자집단고용역할정책문서 source)
        => new()
        {
            역할코드 = source.역할코드,
            역할명 = source.역할명,
            참여자분류 = source.참여자분류,
            근로자출처선호 = source.근로자출처선호,
            입주민우선 = source.입주민우선,
            외부근로자허용 = source.외부근로자허용,
            계약유형 = source.계약유형,
            임금유형 = source.임금유형,
            지급주기 = source.지급주기,
            업무설명템플릿 = source.업무설명템플릿,
            근로전서명계약필요 = source.근로전서명계약필요
        };
}

public static class 주문자집단운영주체공개변환기
{
    public static 주문자집단운영주체공개Dto 공개Dto로(주문자집단운영주체Dto source)
        => new()
        {
            주문자집단배송권키 = source.주문자집단배송권키,
            주문자집단배송권명 = source.주문자집단배송권명,
            고용주체범위유형 = HrScopeTypes.OrdererGroup,
            고용주체범위Id = source.주문자집단배송권키,
            운영주체유형 = source.운영주체유형,
            법적주체명 = source.법적주체명,
            마스킹사업자등록번호 = source.마스킹사업자등록번호,
            사업자검증상태 = source.사업자검증상태,
            수입자역할가능 = source.수입자역할가능,
            고용가능 = source.고용가능,
            급여지급가능 = source.급여지급가능,
            고용준비상태 = source.고용준비상태,
            수입통관준비상태 = source.수입통관준비상태,
            고용역할정책목록 = source.고용역할정책목록,
            필요조치코드목록 = source.필요조치코드목록,
            수정시각Utc = source.수정시각Utc
        };
}

public sealed class 주문자집단운영주체문서
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string 운영주체Id { get; set; } = string.Empty;
    public string 운영주체Id정규화 { get; set; } = string.Empty;
    public string 주문자집단배송권키 { get; set; } = string.Empty;
    public string 주문자집단배송권키정규화 { get; set; } = string.Empty;
    public string 주문자집단배송권명 { get; set; } = string.Empty;
    public string 운영주체유형 { get; set; } = 주문자집단운영주체유형코드.비사업자모임;
    public string 대표UserId { get; set; } = string.Empty;
    public string 대표자명 { get; set; } = string.Empty;
    public string 법적주체명 { get; set; } = string.Empty;
    public string 사업자등록번호 { get; set; } = string.Empty;
    public string 마스킹사업자등록번호 { get; set; } = string.Empty;
    public string 사업자검증상태 { get; set; } = 주문자집단사업자검증상태코드.필요;
    public bool 수입자역할가능 { get; set; }
    public bool 고용가능 { get; set; }
    public bool 급여지급가능 { get; set; }
    public string 고용준비상태 { get; set; } = 주문자집단고용준비상태코드.미준비;
    public string 수입통관준비상태 { get; set; } = string.Empty;
    public string 급여정산방식 { get; set; } = HrPaymentMethods.PlatformSettlement;
    public IReadOnlyList<주문자집단고용역할정책문서> 고용역할정책목록 { get; set; } = [];
    public IReadOnlyList<string> 필요조치코드목록 { get; set; } = [];
    public string 관리자메모 { get; set; } = string.Empty;
    public string 수정자 { get; set; } = string.Empty;
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 주문자집단고용역할정책문서
{
    public string 역할코드 { get; set; } = HrDetailedRoleCodes.OrdererGroupSortingWorker;
    public string 역할명 { get; set; } = string.Empty;
    public string 참여자분류 { get; set; } = HrParticipantCategoryCodes.CommunityPartTimeWorker;
    public string 근로자출처선호 { get; set; } = 주문자집단근로자출처선호코드.입주민우선;
    public bool 입주민우선 { get; set; } = true;
    public bool 외부근로자허용 { get; set; }
    public string 계약유형 { get; set; } = HrEmploymentContractTypes.PartTime;
    public string 임금유형 { get; set; } = HrWageTypes.Hourly;
    public string 지급주기 { get; set; } = HrPaymentCycles.Monthly;
    public string 업무설명템플릿 { get; set; } = string.Empty;
    public bool 근로전서명계약필요 { get; set; } = true;
}
