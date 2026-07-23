using System.Globalization;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.Orderer;

public interface I공동구매내원함조회UseCase
{
    Task<공동구매내원함목록응답> 조회Async(
        string 주문자키,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandOperatingSystem,
    SsalddelCodeLayer.Application,
    "로그인 주문자가 소유한 활성·닫힌 개별 원함 원장과 연결 자동집단의 공개 요약을 조회합니다.",
    ContractType = typeof(I공동구매내원함조회UseCase),
    FlowOrder = 65,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "생성자가 현재 주문자인 개별 원함 원장만 반환하고, 자동집단에서는 공개 집계 필드만 투영합니다.")]
public sealed class 공동구매내원함조회UseCase : I공동구매내원함조회UseCase
{
    private const int 최대조회건수 = 200;
    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly I공동구매자동집단화저장소 _자동집단저장소;

    public 공동구매내원함조회UseCase(
        I커뮤니티원장저장소 원장저장소,
        I공동구매자동집단화저장소 자동집단저장소)
    {
        _원장저장소 = 원장저장소;
        _자동집단저장소 = 자동집단저장소;
    }

    public async Task<공동구매내원함목록응답> 조회Async(
        string 주문자키,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(주문자키);
        var normalizedOrdererId = 주문자키.Trim();
        var ledgers = await _원장저장소.원장목록조회Async(
            new 커뮤니티원장조회조건
            {
                원장템플릿Key = CommunityLedgerTemplateKeys.IndividualDemand,
                접근UserId = normalizedOrdererId,
                Limit = 최대조회건수
            },
            cancellationToken);

        var ownLedgers = ledgers
            .Where(ledger =>
                string.Equals(
                    ledger.원장템플릿Key,
                    CommunityLedgerTemplateKeys.IndividualDemand,
                    StringComparison.Ordinal)
                && string.Equals(
                    ledger.생성자UserId,
                    normalizedOrdererId,
                    StringComparison.Ordinal)
                && 지원상태(ledger.상태))
            .ToArray();

        var groupIds = ownLedgers
            .Select(자동집단Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var groupTasks = groupIds.ToDictionary(
            id => id,
            id => _자동집단저장소.집단조회Async(id, cancellationToken),
            StringComparer.Ordinal);
        await Task.WhenAll(groupTasks.Values);
        var groups = groupTasks.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Result,
            StringComparer.Ordinal);
        var groupPurchaseLedgerIds = groups.Values
            .Where(group => group is not null)
            .Select(group => group!.공동구매주문집계원장Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        IReadOnlyList<커뮤니티원장Dto> groupImportLedgers = groupPurchaseLedgerIds.Length == 0
            ? []
            : await _원장저장소.원장목록조회Async(
                new 커뮤니티원장조회조건
                {
                    원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
                    포함원장Ids = groupPurchaseLedgerIds,
                    Limit = 최대조회건수
                },
                cancellationToken);
        var groupImportLedgerIds = groupImportLedgers
            .SelectMany(ledger => ledger.포함원장목록
                .Where(reference => groupPurchaseLedgerIds.Contains(
                    reference.원장Id,
                    StringComparer.Ordinal))
                .Select(reference => new
                {
                    GroupPurchaseLedgerId = reference.원장Id,
                    GroupImportLedgerId = ledger.원장Id,
                    ledger.수정시각Utc
                }))
            .GroupBy(item => item.GroupPurchaseLedgerId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(item => item.수정시각Utc)
                    .First()
                    .GroupImportLedgerId,
                StringComparer.Ordinal);

        var items = ownLedgers
            .Select(ledger => 응답으로(
                ledger,
                groups.GetValueOrDefault(자동집단Id(ledger)),
                groupImportLedgerIds))
            .OrderBy(item => item.원함상태 == 공동구매내원함상태코드.닫힘)
            .ThenByDescending(item => item.수정시각Utc)
            .ToArray();

        return new 공동구매내원함목록응답
        {
            전체건수 = items.Length,
            활성건수 = items.Count(item =>
                item.원함상태 == 공동구매내원함상태코드.활성),
            닫힘건수 = items.Count(item =>
                item.원함상태 == 공동구매내원함상태코드.닫힘),
            원함목록 = items
        };
    }

    private static 공동구매내원함응답 응답으로(
        커뮤니티원장Dto ledger,
        공동구매자동집단응답? group,
        IReadOnlyDictionary<string, string> groupImportLedgerIds)
    {
        var transactionType = 공동구매거래유형코드.정규화(값(ledger, "TransactionType"));
        var ownDemand = group?.수요목록.FirstOrDefault(demand =>
                string.Equals(
                    demand.개별원함원장Id,
                    ledger.원장Id,
                    StringComparison.Ordinal))
            ?? group?.수요목록.FirstOrDefault(demand =>
                string.Equals(
                    demand.수요출처키,
                    값(ledger, "DemandSourceKey"),
                    StringComparison.Ordinal)
                && string.Equals(
                    demand.주문자키,
                    ledger.생성자UserId,
                    StringComparison.Ordinal));
        var groupPurchaseLedgerId = 기본값(
            ownDemand?.공동구매주문집계원장Id ?? string.Empty,
            group?.공동구매주문집계원장Id ?? string.Empty);
        var isClosed = string.Equals(
            ledger.상태,
            커뮤니티원장상태.닫힘,
            StringComparison.Ordinal);
        return new 공동구매내원함응답
        {
            개별원함원장Id = ledger.원장Id,
            Revision = ledger.Revision,
            수요출처키 = 값(ledger, "DemandSourceKey"),
            원함상태 = isClosed
                ? 공동구매내원함상태코드.닫힘
                : 공동구매내원함상태코드.활성,
            상품키 = 값(ledger, "ProductKey"),
            상품명 = 값(ledger, "ProductName"),
            HS코드 = group?.HS코드 ?? string.Empty,
            희망수량 = Decimal값(ledger, "DesiredQuantity"),
            수량단위 = 값(ledger, "QuantityUnit"),
            배송권키 = 값(ledger, "DeliveryScopeKey"),
            배송권명 = 값(ledger, "DeliveryScopeName"),
            온도코드 = 값(ledger, "TemperatureCode"),
            물류방식 = 기본값(
                값(ledger, "LogisticsMode"),
                공동구매자동수요물류방식코드.후속검토),
            거래유형 = transactionType,
            가격표시기준 = 공동구매가격표시기준코드.정규화(
                값(ledger, "PriceBasis"),
                transactionType),
            구매조직참조키 = 값(ledger, "PurchasingOrganizationReference"),
            구매조직표시명 = 값(ledger, "PurchasingOrganizationName"),
            세금계산서필요 = Bool값(ledger, "TaxInvoiceRequired"),
            목표참여자수 = ownDemand?.목표참여자수,
            목표수량 = ownDemand?.목표수량,
            자동집단Id = 자동집단Id(ledger),
            공동구매주문집계원장Id = groupPurchaseLedgerId,
            개별주문원장Id = ownDemand?.개별주문원장Id ?? string.Empty,
            공동수입원장Id = isClosed
                ? string.Empty
                : groupImportLedgerIds.GetValueOrDefault(groupPurchaseLedgerId)
                  ?? string.Empty,
            자동집단요약 = group is null ? null : 요약응답으로(group),
            생성시각Utc = ledger.생성시각Utc,
            수정시각Utc = ledger.수정시각Utc
        };
    }

    private static 공동구매자동집단요약응답 요약응답으로(
        공동구매자동집단응답 source)
        => new()
        {
            자동집단Id = source.자동집단Id,
            상품키 = source.상품키,
            상품명 = source.상품명,
            HS코드 = source.HS코드,
            온도코드 = source.온도코드,
            물류방식 = source.물류방식,
            거래유형 = source.거래유형,
            가격표시기준 = source.가격표시기준,
            배송권키 = source.배송권키,
            배송권명 = source.배송권명,
            현재상태 = source.현재상태,
            수요건수 = source.수요건수,
            예약결제건수 = source.예약결제건수,
            참여자수 = source.참여자수,
            예약결제참여자수 = source.예약결제참여자수,
            총희망수량 = source.총희망수량,
            수량단위 = source.수량단위,
            목표참여자수 = source.목표참여자수,
            목표수량 = source.목표수량,
            모집종료시각Utc = source.모집종료시각Utc,
            모집종료여부 = source.모집종료여부,
            모집조건충족여부 = source.모집조건충족여부,
            생성시각Utc = source.생성시각Utc,
            수정시각Utc = source.수정시각Utc
        };

    private static bool 지원상태(string? state)
        => string.Equals(state, 커뮤니티원장상태.진행중, StringComparison.Ordinal)
           || string.Equals(state, 커뮤니티원장상태.닫힘, StringComparison.Ordinal);

    private static string 자동집단Id(커뮤니티원장Dto ledger)
        => 값(ledger, "AutomaticGroupId");

    private static string 값(커뮤니티원장Dto ledger, string key)
        => ledger.외부참조.TryGetValue(key, out var value)
            ? value?.Trim() ?? string.Empty
            : string.Empty;

    private static decimal Decimal값(커뮤니티원장Dto ledger, string key)
        => decimal.TryParse(
            값(ledger, key),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : 0m;

    private static bool Bool값(커뮤니티원장Dto ledger, string key)
        => bool.TryParse(값(ledger, key), out var value) && value;

    private static string 기본값(string value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
