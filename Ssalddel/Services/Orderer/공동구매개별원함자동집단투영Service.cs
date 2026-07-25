using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.Orderer;

public sealed record 공동구매개별원함자동집단투영결과(
    공동구매자동집단응답? 자동집단,
    공동구매자동수요철회응답? 철회);

public interface I공동구매개별원함자동집단투영Service
{
    bool 투영대상(커뮤니티원장Dto ledger);

    Task<공동구매개별원함자동집단투영결과> 투영Async(
        커뮤니티원장Dto ledger,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 개별 원함 원장을 원본으로 읽어 자동집단 수요 한 관심사만 재처리 가능하게 투영합니다.
/// 커뮤니티 원장 자체의 projection lease가 Event/Outbox 재시도와 checkpoint를 담당합니다.
/// </summary>
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Application,
    "비구속 개별 원함 원장의 현재 Revision과 상태를 자동집단 수요·철회로 투영합니다.",
    ContractType = typeof(I공동구매개별원함자동집단투영Service),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "IndividualDemand 원장만 원본으로 사용하고 주문·결제·계약·수입·운송을 만들지 않습니다. 원장별 Event/Outbox가 최신 Revision 재처리를 보장합니다.")]
public sealed class 공동구매개별원함자동집단투영Service(
    I공동구매자동집단화저장소 store,
    I공동구매수요모집ProcessManager processManager)
    : I공동구매개별원함자동집단투영Service
{
    public bool 투영대상(커뮤니티원장Dto ledger)
        => string.Equals(
               ledger.원장템플릿Key,
               CommunityLedgerTemplateKeys.IndividualDemand,
               StringComparison.Ordinal)
           && string.Equals(
               ledger.확장속성.GetValueOrDefault("ProjectionMode"),
               공동구매개별원함원장Service.자동집단투영모드,
               StringComparison.Ordinal);

    public async Task<공동구매개별원함자동집단투영결과> 투영Async(
        커뮤니티원장Dto ledger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        if (!투영대상(ledger))
        {
            return new(null, null);
        }

        var command = 활성Command(ledger);
        if (string.Equals(ledger.상태, 커뮤니티원장상태.닫힘, StringComparison.Ordinal))
        {
            var existing = await store.집단조회Async(
                값(ledger, "AutomaticGroupId"),
                cancellationToken);
            var demand = existing?.수요목록.FirstOrDefault(item =>
                string.Equals(item.수요출처키, command.수요출처키, StringComparison.Ordinal)
                && string.Equals(item.주문자키, command.주문자키, StringComparison.Ordinal));
            if (demand is null)
            {
                return new(
                    existing,
                    new 공동구매자동수요철회응답
                    {
                        요청멱등키 = 투영멱등키(ledger, "withdraw"),
                        수요출처키 = command.수요출처키,
                        자동집단Id = existing?.자동집단Id ?? 값(ledger, "AutomaticGroupId"),
                        개별원함원장Id = ledger.원장Id,
                        철회완료 = true,
                        이미처리됨 = true,
                        현재상태 = existing?.현재상태 ?? 공동구매자동집단상태코드.수요수집중,
                        철회시각Utc = ledger.수정시각Utc
                    });
            }

            var withdrawal = await processManager.수요철회조율Async(
                new 공동구매자동수요철회Command
                {
                    요청멱등키 = 투영멱등키(ledger, "withdraw"),
                    수요출처키 = command.수요출처키,
                    주문자키 = command.주문자키,
                    철회사유 = "개별 원함 원장이 철회 상태로 변경됨"
                },
                cancellationToken);
            withdrawal.개별원함원장Id = ledger.원장Id;
            return new(existing, withdrawal);
        }

        var group = await processManager.수요등록조율Async(command, cancellationToken);
        var ownDemand = group.수요목록.FirstOrDefault(item =>
            string.Equals(item.수요출처키, command.수요출처키, StringComparison.Ordinal)
            && string.Equals(item.주문자키, command.주문자키, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("개별 원함 투영 수요를 자동집단에서 찾을 수 없습니다.");
        if (!string.Equals(ownDemand.개별원함원장Id, ledger.원장Id, StringComparison.Ordinal))
        {
            group = await store.개별원함원장연결Async(
                group.자동집단Id,
                ownDemand.수요Id,
                ledger.원장Id,
                cancellationToken);
        }

        return new(group, null);
    }

    private static 공동구매자동수요등록Command 활성Command(커뮤니티원장Dto ledger)
    {
        var transactionType = 공동구매거래유형코드.정규화(값(ledger, "TransactionType"));
        return new 공동구매자동수요등록Command
        {
            요청멱등키 = 투영멱등키(ledger, "save"),
            수요출처키 = 필수값(ledger, "DemandSourceKey"),
            커뮤니티게시글Id = Long값(ledger, "SourceCommunityPostId"),
            커뮤니티원장Id = 값(ledger, "SourceCommunityLedgerId"),
            상품키 = 필수값(ledger, "ProductKey"),
            상품명 = 필수값(ledger, "ProductName"),
            HS코드 = 값(ledger, "HSCode"),
            온도코드 = 값(ledger, "TemperatureCode"),
            물류방식 = 공동구매자동수요물류방식코드.후속검토,
            거래유형 = transactionType,
            가격표시기준 = 공동구매가격표시기준코드.정규화(
                값(ledger, "PriceBasis"),
                transactionType),
            구매조직참조키 = transactionType == 공동구매거래유형코드.B2B
                ? 값(ledger, "PurchasingOrganizationReference")
                : string.Empty,
            구매조직표시명 = transactionType == 공동구매거래유형코드.B2B
                ? 값(ledger, "PurchasingOrganizationName")
                : string.Empty,
            세금계산서필요 = transactionType == 공동구매거래유형코드.B2B
                             && Bool값(ledger, "TaxInvoiceRequired"),
            주문자키 = ledger.생성자UserId
                        ?? throw new InvalidOperationException("개별 원함 원장에 원함 주체가 없습니다."),
            주문자표시명 = 기본값(값(ledger, "OrdererDisplayName"), ledger.생성자표시명),
            배송권키 = 필수값(ledger, "DeliveryScopeKey"),
            배송권명 = 값(ledger, "DeliveryScopeName"),
            희망수량 = Decimal값(ledger, "DesiredQuantity"),
            수량단위 = 기본값(값(ledger, "QuantityUnit"), "kg"),
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            메모 = 값(ledger, "Memo"),
            목표참여자수 = Int값(ledger, "TargetParticipantCount"),
            목표수량 = NullableDecimal값(ledger, "TargetQuantity")
        };
    }

    private static string 투영멱등키(커뮤니티원장Dto ledger, string operation)
    {
        var source = $"{ledger.원장Id}|{ledger.Revision}|{ledger.상태}|{operation}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))
            .ToLowerInvariant();
        return $"wish-projection-{operation}:{hash[..32]}";
    }

    private static string 필수값(커뮤니티원장Dto ledger, string key)
        => !string.IsNullOrWhiteSpace(값(ledger, key))
            ? 값(ledger, key)
            : throw new InvalidOperationException($"개별 원함 원장에 {key}가 없습니다.");

    private static string 값(커뮤니티원장Dto ledger, string key)
        => ledger.외부참조.GetValueOrDefault(key)?.Trim() ?? string.Empty;

    private static string 기본값(string value, string? fallback)
        => !string.IsNullOrWhiteSpace(value) ? value : fallback?.Trim() ?? string.Empty;

    private static decimal Decimal값(커뮤니티원장Dto ledger, string key)
        => NullableDecimal값(ledger, key) ?? 0m;

    private static decimal? NullableDecimal값(커뮤니티원장Dto ledger, string key)
        => decimal.TryParse(
            값(ledger, key),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : null;

    private static int? Int값(커뮤니티원장Dto ledger, string key)
        => int.TryParse(값(ledger, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private static long? Long값(커뮤니티원장Dto ledger, string key)
        => long.TryParse(값(ledger, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private static bool Bool값(커뮤니티원장Dto ledger, string key)
        => bool.TryParse(값(ledger, key), out var value) && value;
}
