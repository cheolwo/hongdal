using System.Globalization;
using System.Net;
using System.Text;
using FluentResults;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Documents;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Services.Community;

public interface I원장관행문서초안UseCase
{
    Task<Result<원장관행문서카탈로그응답>> 카탈로그조회Async(
        string 원장Id,
        string 현재UserId,
        CancellationToken cancellationToken = default);

    Task<Result<원장관행문서초안묶음응답>> 생성Async(
        string 원장Id,
        string 현재UserId,
        string? 문서종류코드 = null,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "같이 주문·같이 수입 원장의 확정된 근거를 관행적인 구매·무역 문서의 검토용 초안으로 투영합니다.",
    ContractType = typeof(I원장관행문서초안UseCase),
    FlowOrder = 38,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "원장 원본을 변경하지 않으며 문서 발행, 서명, 계약, 결제, 신고, 운송 지시 또는 외부 전송을 수행하지 않습니다.")]
public sealed class 원장관행문서초안UseCase : I원장관행문서초안UseCase
{
    public const string 초안경계안내 = "DRAFT / 전문가 검토 전 외부 제출 금지. 이 결과는 원장 근거를 재사용한 검토용 초안이며 계약·결제·신고·운송 지시 문서가 아닙니다.";

    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly TimeProvider _timeProvider;

    public 원장관행문서초안UseCase(
        I커뮤니티원장저장소 원장저장소,
        TimeProvider timeProvider)
    {
        _원장저장소 = 원장저장소;
        _timeProvider = timeProvider;
    }

    public async Task<Result<원장관행문서카탈로그응답>> 카탈로그조회Async(
        string 원장Id,
        string 현재UserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(원장Id) || string.IsNullOrWhiteSpace(현재UserId))
        {
            return Result.Fail<원장관행문서카탈로그응답>(
                new Error("원장 ID와 사용자 ID가 필요합니다.").WithMetadata("StatusCode", 400));
        }

        var 원장 = await _원장저장소.원장조회Async(원장Id.Trim(), cancellationToken);
        if (원장 is null)
        {
            return Result.Fail<원장관행문서카탈로그응답>(
                new Error("문서 카탈로그를 조회할 주문 원장을 찾을 수 없습니다.").WithMetadata("StatusCode", 404));
        }

        if (!주문원장역할별조회Service.직접접근가능(원장, 현재UserId.Trim()))
        {
            return Result.Fail<원장관행문서카탈로그응답>(
                new Error("원장 소유자 또는 직접 참여자만 문서 카탈로그를 조회할 수 있습니다.").WithMetadata("StatusCode", 403));
        }

        var 목록 = 원장관행문서카탈로그.원장종류별(원장.원장템플릿Key);
        if (목록.Count == 0)
        {
            return Result.Fail<원장관행문서카탈로그응답>(
                new Error("같이 주문 원장과 같이 수입 원장만 관행 문서 카탈로그를 제공합니다.").WithMetadata("StatusCode", 400));
        }

        return Result.Ok(new 원장관행문서카탈로그응답
        {
            원장Id = 원장.원장Id,
            원장템플릿Key = 원장.원장템플릿Key,
            문서종류목록 = 목록
        });
    }

    public async Task<Result<원장관행문서초안묶음응답>> 생성Async(
        string 원장Id,
        string 현재UserId,
        string? 문서종류코드 = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(원장Id) || string.IsNullOrWhiteSpace(현재UserId))
        {
            return 실패("원장 ID와 사용자 ID가 필요합니다.", 400);
        }

        var 원장 = await _원장저장소.원장조회Async(원장Id.Trim(), cancellationToken);
        if (원장 is null)
        {
            return 실패("문서 초안을 만들 주문 원장을 찾을 수 없습니다.", 404);
        }

        if (!주문원장역할별조회Service.직접접근가능(원장, 현재UserId.Trim()))
        {
            return 실패("원장 소유자 또는 직접 참여자만 문서 초안을 조회할 수 있습니다.", 403);
        }

        var 정규문서종류 = string.IsNullOrWhiteSpace(문서종류코드)
            ? null
            : 문서종류코드.Trim().ToUpperInvariant();
        if (정규문서종류 is not null && !원장관행문서종류코드.지원목록.Contains(정규문서종류))
        {
            return 실패($"지원하지 않는 문서 종류입니다. DocumentTypeCode={정규문서종류}", 400);
        }

        var 생성시각 = _timeProvider.GetUtcNow();
        Result<IReadOnlyList<원장관행문서초안Dto>> 문서결과;
        if (string.Equals(원장.원장템플릿Key, CommunityLedgerTemplateKeys.GroupOrder, StringComparison.OrdinalIgnoreCase))
        {
            문서결과 = Result.Ok(같이주문문서생성(원장, 생성시각));
        }
        else if (string.Equals(원장.원장템플릿Key, CommunityLedgerTemplateKeys.GroupImport, StringComparison.OrdinalIgnoreCase))
        {
            문서결과 = 같이수입문서생성(원장, 생성시각);
        }
        else
        {
            return 실패("같이 주문 원장과 같이 수입 원장만 관행 문서 초안을 만들 수 있습니다.", 400);
        }

        if (문서결과.IsFailed)
        {
            return Result.Fail<원장관행문서초안묶음응답>(문서결과.Errors);
        }

        var 문서목록 = 문서결과.Value
            .Where(document => 정규문서종류 is null
                || string.Equals(document.문서종류코드, 정규문서종류, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (정규문서종류 is not null && 문서목록.Length == 0)
        {
            return 실패("선택한 원장 종류에서는 요청한 문서 초안을 만들 수 없습니다.", 400);
        }

        foreach (var 문서 in 문서목록)
        {
            문서.Html = Html생성(문서);
            문서.PlainText = PlainText생성(문서);
        }

        return Result.Ok(new 원장관행문서초안묶음응답
        {
            원장Id = 원장.원장Id,
            원장Revision = 원장.Revision,
            원장템플릿Key = 원장.원장템플릿Key,
            생성시각Utc = 생성시각,
            운영문서여부 = false,
            외부전송가능여부 = false,
            실행경계안내 = 초안경계안내,
            문서목록 = 문서목록
        });
    }

    private static IReadOnlyList<원장관행문서초안Dto> 같이주문문서생성(
        커뮤니티원장Dto 원장,
        DateTimeOffset 생성시각)
    {
        var 집계 = 원장.블록목록.FirstOrDefault(block =>
            string.Equals(block.BlockId, "individual-order-aggregation", StringComparison.OrdinalIgnoreCase));
        var 품목키 = 값(원장.외부참조, "ProductKey");
        var 품명 = 값(원장.외부참조, "ProductName");
        var 수량 = Decimal값(집계?.Data, "TotalRequestedQuantity");
        var 수량단위 = 값(집계?.Data, "QuantityUnit");
        var 예약결제합계 = Decimal값(집계?.Data, "TotalReservedPaymentAmount");
        var 문서일자 = 생성시각.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var 품목행 = new 원장관행문서품목행Dto
        {
            순번 = 1,
            품목키 = 품목키,
            품명 = 품명,
            수량 = 수량,
            수량단위 = 수량단위,
            원천경로 = "외부참조.ProductName + 블록[individual-order-aggregation]"
        };

        var 견적요청누락 = new List<string>();
        누락추가(견적요청누락, "견적 요청 대상 공급자", string.Empty);
        누락추가(견적요청누락, "견적 회신 기한", string.Empty);
        누락추가(견적요청누락, "희망 납품일·납품 장소", string.Empty);
        누락추가(견적요청누락, "품목명", 품명);
        누락추가(견적요청누락, "요청 수량", 수량 > 0 ? 수량.ToString(CultureInfo.InvariantCulture) : string.Empty);
        누락추가(견적요청누락, "거래 단위", 수량단위);
        var 견적요청서 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.견적요청서,
            "견적요청서 초안",
            "REQUEST FOR QUOTATION",
            [
                필드("RequestDate", "요청일", 문서일자, "생성시각", true),
                필드("Requester", "요청자 표시명", 원장.생성자표시명, "원장.생성자표시명", false),
                필드("SupplierRecipient", "공급자 수신처", string.Empty, "공급자 후보 선택 필요", false),
                필드("ResponseDueDate", "견적 회신 기한", string.Empty, "원장에 근거 없음", false),
                필드("LedgerTitle", "원장 제목", 원장.제목, "원장.제목", true)
            ],
            [품목행],
            [],
            견적요청누락,
            [
                "요청 수량과 거래 단위는 같이 주문 집계값을 그대로 사용합니다.",
                "공급자별 회신 가격·MOQ·납기·포장 조건은 원장에 별도 견적 근거로 기록해야 합니다."
            ]);

        var 구매주문서누락 = new List<string>();
        누락추가(구매주문서누락, "공급자 법인명·주소", string.Empty);
        누락추가(구매주문서누락, "구매자 조직명·주소", string.Empty);
        누락추가(구매주문서누락, "품목명", 품명);
        누락추가(구매주문서누락, "주문 수량", 수량 > 0 ? 수량.ToString(CultureInfo.InvariantCulture) : string.Empty);
        누락추가(구매주문서누락, "거래 단위", 수량단위);
        누락추가(구매주문서누락, "단가·통화", string.Empty);
        누락추가(구매주문서누락, "납품 조건", string.Empty);
        누락추가(구매주문서누락, "결제 조건", string.Empty);

        var 구매주문서 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.구매주문서,
            "구매주문서 초안",
            "PURCHASE ORDER",
            [
                필드("DocumentDate", "문서일자", 문서일자, "생성시각", true),
                필드("Buyer", "구매자 표시명", 원장.생성자표시명, "원장.생성자표시명", false),
                필드("Supplier", "공급자", string.Empty, "원장에 공급자 근거 없음", false),
                필드("LedgerTitle", "원장 제목", 원장.제목, "원장.제목", true),
                필드("ReservedPaymentAggregate", "예약 결제 합계(참고)", 예약결제합계.ToString(CultureInfo.InvariantCulture), "블록[individual-order-aggregation]", false)
            ],
            [품목행],
            [],
            구매주문서누락,
            [
                "예약 결제 합계는 참여 의향 집계값이며 확정 구매대금이나 공급자 청구금액으로 사용하지 않습니다.",
                "공급자 확인, 단가, 납품·결제 조건이 확정되기 전에는 구매주문서로 발행할 수 없습니다."
            ]);

        var 집계표누락 = new List<string>();
        누락추가(집계표누락, "품목명", 품명);
        누락추가(집계표누락, "합산 주문 수량", 수량 > 0 ? 수량.ToString(CultureInfo.InvariantCulture) : string.Empty);
        누락추가(집계표누락, "거래 단위", 수량단위);
        var 집계표 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.같이주문집계표,
            "같이 주문 집계표",
            "GROUP ORDER SHEET",
            [
                필드("LedgerTitle", "원장 제목", 원장.제목, "원장.제목", true),
                필드("ConfirmedOrdererCount", "확정 주문자 수", 값(집계?.Data, "ConfirmedOrdererCount"), "블록[individual-order-aggregation]", true),
                필드("DestinationWarehouseCount", "도착 창고 수", 값(집계?.Data, "DestinationWarehouseCount"), "블록[individual-order-aggregation]", true),
                필드("ReservedPaymentAggregate", "예약 결제 합계(참고)", 예약결제합계.ToString(CultureInfo.InvariantCulture), "블록[individual-order-aggregation]", false)
            ],
            [품목행],
            [],
            집계표누락,
            ["개인별 주문·연락처가 아닌 원장 집계값만 포함합니다."]);

        var 계약검토누락 = new List<string>();
        누락추가(계약검토누락, "공급자/판매자 계약 당사자", string.Empty);
        누락추가(계약검토누락, "확정 단가·통화", string.Empty);
        누락추가(계약검토누락, "납품·검수 조건", string.Empty);
        누락추가(계약검토누락, "결제 단계와 환불·취소 조건", string.Empty);
        누락추가(계약검토누락, "계약 문서 번호·해시", 값(원장.확장속성, "ContractDocumentNumber"));
        var 계약검토자료서 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.계약검토자료서,
            "계약 검토 자료서",
            "CONTRACT REVIEW SHEET",
            [
                필드("BuyerRepresentative", "구매자 대표 표시명", 원장.생성자표시명, "원장.생성자표시명", false),
                필드("ParticipantCount", "직접 참여자 수", 원장.참여자목록.Count.ToString(CultureInfo.InvariantCulture), "원장.참여자목록", true),
                필드("LedgerState", "원장 상태", 원장.상태, "원장.상태", true),
                필드("SignatureState", "서명 상태", 값(원장.확장속성, "SignatureState"), "주문원장서명UseCase 연계값", false),
                필드("ContractDocumentNumber", "계약 문서 번호", 값(원장.확장속성, "ContractDocumentNumber"), "주문원장서명UseCase 연계값", false)
            ],
            [품목행],
            [],
            계약검토누락,
            [
                "이 자료서는 기존 수입식품 같이 주문 계약 검토 계획기와 전자서명 모듈에 넘길 입력 점검표입니다.",
                "예약 결제 합계는 계약금액이 아니며 서명 묶음과 문서 해시가 확인되기 전에는 계약 완료로 보지 않습니다."
            ]);

        return [견적요청서, 구매주문서, 집계표, 계약검토자료서];
    }

    private static Result<IReadOnlyList<원장관행문서초안Dto>> 같이수입문서생성(
        커뮤니티원장Dto 원장,
        DateTimeOffset 생성시각)
    {
        같이수입준비원장저장요청 준비자료;
        try
        {
            준비자료 = 같이수입준비ProcessManager상태저장정책.준비자료읽기(원장);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<IReadOnlyList<원장관행문서초안Dto>>(
                new Error(exception.Message).WithMetadata("StatusCode", 422));
        }

        var 판매자책임 = 준비자료.책임초안목록.FirstOrDefault(item =>
            string.Equals(item.역할코드, 같이수입준비책임역할코드.판매자수출자, StringComparison.OrdinalIgnoreCase));
        var 수입자책임 = 준비자료.책임초안목록.FirstOrDefault(item =>
            string.Equals(item.역할코드, 같이수입준비책임역할코드.수입자, StringComparison.OrdinalIgnoreCase));
        var 공급자근거 = 준비자료.공급자근거목록.FirstOrDefault();
        var 판매자명 = !string.IsNullOrWhiteSpace(판매자책임?.당사자표시명)
            ? 판매자책임.당사자표시명
            : 공급자근거?.조직명 ?? string.Empty;
        var 판매자확인 = 판매자책임?.당사자확인여부 == true;
        var 수입자명 = 수입자책임?.당사자표시명 ?? string.Empty;
        var 수입자확인 = 수입자책임?.당사자확인여부 == true;

        var 품목목록 = 준비자료.재료품목목록.Count > 0
            ? 준비자료.재료품목목록
            : string.IsNullOrWhiteSpace(준비자료.재료명)
                ? []
                : [new 같이수입준비재료품목 { 재료키 = 준비자료.재료키, 재료명 = 준비자료.재료명 }];
        var 품목행목록 = 품목목록.Select((item, index) =>
        {
            var 견적 = 준비자료.견적목록
                .Where(quote => string.Equals(quote.재료키, item.재료키, StringComparison.OrdinalIgnoreCase))
                .Where(quote => quote.유효기한Utc >= 생성시각)
                .OrderByDescending(quote => quote.확인시각Utc)
                .FirstOrDefault();
            var 분류 = 준비자료.품목분류후보목록
                .Where(candidate => string.Equals(candidate.재료키, item.재료키, StringComparison.OrdinalIgnoreCase))
                .Where(candidate => string.Equals(candidate.관할국가코드, 준비자료.도착국가코드, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(candidate => string.Equals(
                    candidate.검토상태코드,
                    같이수입준비검토상태코드.전문가검토완료,
                    StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(candidate => candidate.신뢰도)
                .FirstOrDefault();
            var hs코드 = string.IsNullOrWhiteSpace(item.원천Hs코드) ? 분류?.품목코드 ?? string.Empty : item.원천Hs코드;
            var hs확인 = 분류 is not null
                && string.Equals(분류.검토상태코드, 같이수입준비검토상태코드.전문가검토완료, StringComparison.OrdinalIgnoreCase)
                && string.Equals(분류.품목코드, hs코드, StringComparison.OrdinalIgnoreCase);
            var 단가 = 견적 is null ? (decimal?)null : 견적.단가;
            return new 원장관행문서품목행Dto
            {
                순번 = index + 1,
                품목키 = item.재료키,
                품명 = item.재료명,
                Hs코드 = hs코드,
                Hs코드전문가확인여부 = hs확인,
                원산지국가코드 = string.Empty,
                수량 = item.모인수요수량,
                수량단위 = string.IsNullOrWhiteSpace(item.수량단위) ? 견적?.수량단위 ?? string.Empty : item.수량단위,
                단가 = 단가,
                통화코드 = 견적?.통화코드 ?? 준비자료.기준통화코드,
                금액 = 단가 is null ? null : item.모인수요수량 * 단가.Value,
                포장조건 = 견적?.포장조건 ?? string.Empty,
                원천경로 = $"준비자료.재료품목목록[{index}] + 유효 견적·품목분류 후보"
            };
        }).ToArray();

        var 합계목록 = 품목행목록
            .Where(line => line.금액.HasValue && !string.IsNullOrWhiteSpace(line.통화코드))
            .GroupBy(line => line.통화코드, StringComparer.OrdinalIgnoreCase)
            .Select(group => new 원장관행문서금액합계Dto
            {
                합계코드 = "GoodsTotal",
                표시명 = "품목 금액 합계",
                금액 = group.Sum(line => line.금액!.Value),
                통화코드 = group.Key
            })
            .ToArray();
        var Incoterms = 준비자료.견적목록
            .Where(quote => quote.유효기한Utc >= 생성시각)
            .Select(quote => quote.Incoterms후보)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

        var 공통필드 = new[]
        {
            필드("SellerExporter", "판매자/수출자", 판매자명, 판매자책임 is null ? "준비자료.공급자근거목록(후보)" : "준비자료.책임초안목록", 판매자확인),
            필드("ImporterOfRecord", "수입자", 수입자명, "준비자료.책임초안목록", 수입자확인),
            필드("ShipFromCountry", "출발 국가", 준비자료.출발국가코드, "준비자료.출발국가코드", true),
            필드("ShipToCountry", "도착 국가", 준비자료.도착국가코드, "준비자료.도착국가코드", true),
            필드("IncotermsCandidate", "Incoterms 후보", Incoterms, "준비자료.견적목록", false)
        };

        var 송장누락 = new List<string>();
        누락추가(송장누락, "판매자/수출자 법인명", 판매자명);
        누락추가(송장누락, "판매자 주소", string.Empty);
        누락추가(송장누락, "수입자 법인명", 수입자명);
        누락추가(송장누락, "수입자 주소", string.Empty);
        누락추가(송장누락, "판매자가 부여한 Invoice No.", string.Empty);
        누락추가(송장누락, "결제 조건", string.Empty);
        누락추가(송장누락, "원산지", 품목행목록.All(line => !string.IsNullOrWhiteSpace(line.원산지국가코드)) ? "입력됨" : string.Empty);
        누락추가(송장누락, "단가·통화", 품목행목록.All(line => line.단가.HasValue && !string.IsNullOrWhiteSpace(line.통화코드)) ? "입력됨" : string.Empty);
        var 상업송장 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.상업송장,
            "상업송장 초안",
            "COMMERCIAL INVOICE",
            공통필드,
            품목행목록,
            합계목록,
            송장누락,
            [
                "출발 국가와 공급자 소재 국가는 원산지를 확정하지 않으므로 원산지 필드를 자동 채우지 않았습니다.",
                "HS 코드는 전문가 검토 완료 표시가 없는 한 분류 후보입니다.",
                "판매자/수출자와 수입자 당사자 확인 후 실제 발행번호·주소·결제 조건을 보완해야 합니다."
            ]);

        var 포장누락 = new List<string>();
        누락추가(포장누락, "판매자/수출자 법인명", 판매자명);
        누락추가(포장누락, "패키지 수와 포장 종류", string.Empty);
        누락추가(포장누락, "순중량", string.Empty);
        누락추가(포장누락, "총중량", string.Empty);
        누락추가(포장누락, "용적 또는 치수", string.Empty);
        누락추가(포장누락, "Shipping Mark", string.Empty);
        var 포장명세서 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.포장명세서,
            "포장명세서 초안",
            "PACKING LIST",
            공통필드,
            품목행목록,
            [],
            포장누락,
            [
                "견적의 포장조건은 참고 근거이며 실제 선적 패키지 수·중량·치수를 대신하지 않습니다.",
                "실물 포장 완료 후 판매자 또는 포장 책임자가 계측값을 확인해야 합니다."
            ]);

        var 프로포마누락 = new List<string>();
        누락추가(프로포마누락, "판매자/수출자", 판매자명);
        누락추가(프로포마누락, "수입자/구매자", 수입자명);
        누락추가(프로포마누락, "유효한 단가·통화", 품목행목록.All(line => line.단가.HasValue && !string.IsNullOrWhiteSpace(line.통화코드)) ? "입력됨" : string.Empty);
        누락추가(프로포마누락, "견적 유효기한", 준비자료.견적목록.Any(quote => quote.유효기한Utc >= 생성시각) ? "입력됨" : string.Empty);
        누락추가(프로포마누락, "결제 조건", string.Empty);
        var 프로포마자료서 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.프로포마송장자료서,
            "프로포마 송장 발급 자료서",
            "PRO FORMA INVOICE DATA SHEET",
            [
                .. 공통필드,
                필드(
                    "QuotationValidUntil",
                    "견적 유효기한",
                    준비자료.견적목록
                        .Where(quote => quote.유효기한Utc >= 생성시각)
                        .OrderBy(quote => quote.유효기한Utc)
                        .Select(quote => quote.유효기한Utc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                        .FirstOrDefault() ?? string.Empty,
                    "준비자료.견적목록",
                    false)
            ],
            품목행목록,
            합계목록,
            프로포마누락,
            [
                "판매자/수출자가 견적 조건을 확인하고 발급할 프로포마 송장의 입력 자료입니다.",
                "플랫폼 자료서는 판매자가 발행한 Pro Forma Invoice 원본을 대체하지 않습니다."
            ]);

        var 선적지시누락 = new List<string>();
        누락추가(선적지시누락, "수출자/Shipper", 판매자명);
        누락추가(선적지시누락, "수하인/Consignee", 수입자명);
        누락추가(선적지시누락, "포워더", 준비자료.포워더인계.전달대상업체명);
        누락추가(선적지시누락, "출발항·도착항", string.Empty);
        누락추가(선적지시누락, "운임 지급 조건", string.Empty);
        누락추가(선적지시누락, "화물 인수 장소·일시", string.Empty);
        var 선적인도지시서 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.선적인도지시서,
            "선적인도지시서 초안",
            "SHIPPER'S LETTER OF INSTRUCTION",
            [
                .. 공통필드,
                필드("Forwarder", "포워더", 준비자료.포워더인계.전달대상업체명, "준비자료.포워더인계", 준비자료.포워더인계.인계시각Utc.HasValue),
                필드("TransportMode", "운송 방식", 준비자료.국제운송검토.포워더제안방식코드, "준비자료.국제운송검토", false),
                필드("ForwarderHandoffScope", "포워더 전달 범위", 준비자료.포워더인계.전달범위요약, "준비자료.포워더인계", 준비자료.포워더인계.정보제공동의확인여부),
                필드("PrivacyIncluded", "개인정보 포함 여부", 준비자료.포워더인계.개인정보포함여부 ? "포함" : "미포함", "준비자료.포워더인계", true)
            ],
            품목행목록,
            [],
            선적지시누락,
            [
                "이 문서는 수출자가 포워더에게 확인·발행할 선적 지시 초안입니다.",
                "포워더 인계 동의와 전달 범위를 넘는 개인정보는 포함하지 않습니다.",
                "운송 지시 실행이나 포워더 자동 전송은 수행하지 않습니다."
            ]);

        var 원산지누락 = new List<string>();
        누락추가(원산지누락, "생산자/제조자", string.Empty);
        누락추가(원산지누락, "품목별 원산지 국가", 품목행목록.All(line => !string.IsNullOrWhiteSpace(line.원산지국가코드)) ? "입력됨" : string.Empty);
        누락추가(원산지누락, "원산지 판정 기준과 생산 근거", string.Empty);
        누락추가(원산지누락, "특혜/비특혜 및 적용 협정", string.Empty);
        var 원산지자료서 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.원산지증명준비자료서,
            "원산지증명 준비 자료서",
            "CERTIFICATE OF ORIGIN DATA SHEET",
            [
                .. 공통필드,
                필드("ProducerManufacturer", "생산자/제조자", string.Empty, "생산·제조 근거 필요", false),
                필드("OriginBasis", "원산지 판정 근거", string.Empty, "원산지 판정 자료 필요", false),
                필드("PreferenceScheme", "특혜·협정 구분", string.Empty, "협정 적용 검토 필요", false)
            ],
            품목행목록,
            [],
            원산지누락,
            [
                "출발국·공급자 소재지·해외제조업소 소재지는 원재료 원산지의 자동 근거로 사용하지 않습니다.",
                "이 자료서는 상공회의소 또는 협정상 권한 있는 발급자의 원산지증명서를 대체하지 않습니다."
            ]);

        var 선적문서번호 = 값(원장.외부참조, "TransportDocumentNumber");
        var 선적문서유형 = 값(원장.외부참조, "TransportDocumentType");
        var 문서관리번호 = 값(원장.외부참조, "DocumentManagementNumber");
        var 통관점검누락 = new List<string>();
        누락추가(통관점검누락, "판매자 발행 송품장", 송장누락.Count == 0 ? "준비됨" : string.Empty);
        누락추가(통관점검누락, "B/L 또는 AWB 사본", 선적문서번호);
        누락추가(통관점검누락, "포장명세서", 포장누락.Count == 0 ? "준비됨" : string.Empty);
        누락추가(통관점검누락, "가격신고 근거", 합계목록.Length > 0 ? "준비됨" : string.Empty);
        누락추가(통관점검누락, "해당 시 원산지·수입요건 서류", 준비자료.국가별검토항목목록.Count > 0 ? "검토목록 있음" : string.Empty);
        var 통관점검표 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.수입통관서류점검표,
            "수입통관 서류 점검표",
            "IMPORT CUSTOMS DOCUMENT CHECKLIST",
            [
                필드("CommercialInvoice", "송품장/상업송장", 송장누락.Count == 0 ? "초안 필수값 충족" : $"누락 {송장누락.Count}건", "상업송장 초안 투영", false),
                필드("PackingList", "포장명세서", 포장누락.Count == 0 ? "초안 필수값 충족" : $"누락 {포장누락.Count}건", "포장명세서 초안 투영", false),
                필드("TransportDocument", "B/L·AWB", $"{선적문서유형} {선적문서번호}".Trim(), "공동구매해외선적추적UseCase 연계값", false),
                필드("CustomsValueEvidence", "가격신고 근거", 합계목록.Length > 0 ? "견적 기반 품목 합계 있음" : string.Empty, "준비자료.견적목록·예상비용목록", false),
                필드("OriginRequirements", "원산지·수입요건", 준비자료.국가별검토항목목록.Count > 0 ? $"{준비자료.국가별검토항목목록.Count}건 검토중" : string.Empty, "준비자료.국가별검토항목목록", false)
            ],
            품목행목록,
            합계목록,
            통관점검누락,
            [
                "관세청 서류제출 대상 여부와 해당 물품의 추가 서류는 수입자·관세사가 최신 기준으로 확인해야 합니다.",
                "이 점검표는 수입신고서, 가격신고서 또는 세관 제출을 대신하지 않습니다."
            ]);

        var 식품검토필드 = 준비자료.국가별검토항목목록.Select((item, index) =>
            필드(
                $"FoodRequirement{index + 1}",
                string.IsNullOrWhiteSpace(item.표시명) ? item.항목코드 : item.표시명,
                item.검토상태코드,
                string.IsNullOrWhiteSpace(item.공식원출처Url)
                    ? "준비자료.국가별검토항목목록"
                    : item.공식원출처Url,
                string.Equals(item.검토상태코드, 같이수입준비검토상태코드.전문가검토완료, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var 식품점검누락 = new List<string>();
        누락추가(식품점검누락, "품목·국가별 수입식품 구비서류 검토", 식품검토필드.Length > 0 ? "검토목록 있음" : string.Empty);
        if (식품검토필드.Any(field => !field.확인됨))
        {
            식품점검누락.Add("미완료 수입식품 요건의 전문가 확인");
        }
        누락추가(식품점검누락, "해외제조업소·생산자 근거", 준비자료.공급자근거목록.Count > 0 ? "후보 근거 있음" : string.Empty);
        var 식품점검표 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.수입식품서류점검표,
            "수입식품 서류 점검표",
            "IMPORT FOOD DOCUMENT CHECKLIST",
            [
                필드("DestinationCountry", "수입 국가", 준비자료.도착국가코드, "준비자료.도착국가코드", true),
                필드("MaterialCount", "품목 수", 품목행목록.Length.ToString(CultureInfo.InvariantCulture), "준비자료.재료품목목록", true),
                필드("SupplierEvidenceCount", "공급자·제조업소 근거 수", 준비자료.공급자근거목록.Count.ToString(CultureInfo.InvariantCulture), "준비자료.공급자근거목록", false),
                .. 식품검토필드
            ],
            품목행목록,
            [],
            식품점검누락,
            [
                "수입식품 구비서류는 품목·원재료·국가·질병 발생 상황·신고 시점에 따라 달라질 수 있습니다.",
                "위생·검사·식물검역 증명서는 권한 있는 기관의 검사와 발급이 필요한 외부 원본입니다."
            ]);

        var 선적참조누락 = new List<string>();
        누락추가(선적참조누락, "문서관리번호", 문서관리번호);
        누락추가(선적참조누락, "B/L·AWB 유형", 선적문서유형);
        누락추가(선적참조누락, "B/L·AWB 번호", 선적문서번호);
        누락추가(선적참조누락, "운송사·선박/항공편", 값(원장.외부참조, "CarrierName"));
        누락추가(선적참조누락, "출발항·도착항", 값(원장.외부참조, "DeparturePortCode"));
        var 선적참조표 = 문서(
            원장,
            생성시각,
            원장관행문서종류코드.선적문서참조표,
            "선적 문서 등록·참조표",
            "SHIPMENT DOCUMENT REFERENCE SHEET",
            [
                필드("DocumentManagementNumber", "문서관리번호", 문서관리번호, "공동구매해외선적추적UseCase 연계값", false),
                필드("TransportDocumentType", "운송문서 유형", 선적문서유형, "공동구매해외선적추적UseCase 연계값", false),
                필드("TransportDocumentNumber", "운송문서 번호", 선적문서번호, "공동구매해외선적추적UseCase 연계값", false),
                필드("CarrierName", "운송사", 값(원장.외부참조, "CarrierName"), "공동구매해외선적추적UseCase 연계값", false),
                필드("DeparturePort", "출발항", 값(원장.외부참조, "DeparturePortCode"), "공동구매수입물류정규화Service 연계값", false),
                필드("ArrivalPort", "도착항", 값(원장.외부참조, "ArrivalPortCode"), "공동구매수입물류정규화Service 연계값", false)
            ],
            품목행목록,
            [],
            선적참조누락,
            [
                "실제 B/L 또는 AWB는 운송사·포워더가 발행한 원본의 문서관리번호와 번호를 등록해 연결합니다.",
                "이 참조표 자체는 운송계약이나 화물 인도 권리를 증명하지 않습니다."
            ]);

        return Result.Ok<IReadOnlyList<원장관행문서초안Dto>>(
        [
            프로포마자료서,
            상업송장,
            포장명세서,
            선적인도지시서,
            원산지자료서,
            통관점검표,
            식품점검표,
            선적참조표
        ]);
    }

    private static 원장관행문서초안Dto 문서(
        커뮤니티원장Dto 원장,
        DateTimeOffset 생성시각,
        string 문서종류코드,
        string 문서명,
        string 영문문서명,
        IReadOnlyList<원장관행문서필드Dto> 필드목록,
        IReadOnlyList<원장관행문서품목행Dto> 품목행목록,
        IReadOnlyList<원장관행문서금액합계Dto> 금액합계목록,
        IReadOnlyList<string> 필수입력누락목록,
        IReadOnlyList<string> 경고목록)
    {
        var 번호 = $"{문서종류코드}-{원장.원장Id}-{원장.Revision}";
        var 카탈로그항목 = 원장관행문서카탈로그.찾기(문서종류코드);
        return new 원장관행문서초안Dto
        {
            문서종류코드 = 문서종류코드,
            문서명 = 문서명,
            영문문서명 = 영문문서명,
            초안문서번호 = 번호,
            파일명 = $"{파일명안전문자(번호)}-{생성시각:yyyyMMdd}.html",
            생성모드코드 = 카탈로그항목?.생성모드코드 ?? 원장관행문서생성모드코드.원장초안,
            발급주체코드 = 카탈로그항목?.발급주체코드 ?? 원장관행문서발급주체코드.주문자집단,
            외부발급원본대체가능여부 = false,
            상태코드 = 필수입력누락목록.Count == 0
                ? 원장관행문서초안상태코드.전문가검토준비
                : 원장관행문서초안상태코드.입력필요,
            원천원장Revision = 원장.Revision,
            필드목록 = 필드목록,
            품목행목록 = 품목행목록,
            금액합계목록 = 금액합계목록,
            필수입력누락목록 = 필수입력누락목록,
            경고목록 = [초안경계안내, .. 경고목록]
        };
    }

    private static 원장관행문서필드Dto 필드(
        string 코드,
        string 표시명,
        string? 값,
        string 원천경로,
        bool 확인됨)
        => new()
        {
            필드코드 = 코드,
            표시명 = 표시명,
            값 = 값?.Trim() ?? string.Empty,
            원천경로 = 원천경로,
            확인됨 = 확인됨
        };

    private static void 누락추가(List<string> 누락목록, string 표시명, string? 값)
    {
        if (string.IsNullOrWhiteSpace(값))
        {
            누락목록.Add(표시명);
        }
    }

    private static string 값(IReadOnlyDictionary<string, string>? data, string key)
        => data is not null && data.TryGetValue(key, out var value) ? value : string.Empty;

    private static decimal Decimal값(IReadOnlyDictionary<string, string>? data, string key)
        => decimal.TryParse(값(data, key), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;

    private static string 파일명안전문자(string value)
        => string.Concat(value.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

    private static string Html생성(원장관행문서초안Dto 문서)
    {
        static string E(object? value) => WebUtility.HtmlEncode(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
        var builder = new StringBuilder();
        builder.Append("<!doctype html><html lang=\"ko\"><head><meta charset=\"utf-8\"><title>")
            .Append(E(문서.문서명))
            .Append("</title><style>@page{size:A4;margin:18mm}body{font-family:Arial,'Noto Sans KR',sans-serif;color:#172033}h1{margin:0} .draft{margin:12px 0;padding:10px;border:2px solid #b42318;color:#b42318;font-weight:700}table{width:100%;border-collapse:collapse;margin:14px 0}th,td{border:1px solid #aeb6c2;padding:7px;text-align:left;font-size:12px}th{background:#eef4ff}.missing{color:#b42318}</style></head><body>")
            .Append("<h1>").Append(E(문서.영문문서명)).Append("</h1><p>").Append(E(문서.문서명)).Append("</p>")
            .Append("<div class=\"draft\">").Append(E(초안경계안내)).Append("</div>")
            .Append("<p>Draft No. ").Append(E(문서.초안문서번호)).Append("</p><table><tbody>");
        foreach (var field in 문서.필드목록)
        {
            builder.Append("<tr><th>").Append(E(field.표시명)).Append("</th><td>")
                .Append(E(field.값)).Append("</td><td>").Append(E(field.확인됨 ? "확인" : "미확인"))
                .Append("</td></tr>");
        }
        builder.Append("</tbody></table><table><thead><tr><th>No.</th><th>품명</th><th>HS</th><th>원산지</th><th>수량</th><th>단가</th><th>금액</th><th>포장</th></tr></thead><tbody>");
        foreach (var line in 문서.품목행목록)
        {
            builder.Append("<tr><td>").Append(E(line.순번)).Append("</td><td>").Append(E(line.품명))
                .Append("</td><td>").Append(E(line.Hs코드)).Append("</td><td>").Append(E(line.원산지국가코드))
                .Append("</td><td>").Append(E($"{line.수량} {line.수량단위}")).Append("</td><td>")
                .Append(E(line.단가.HasValue ? $"{line.단가} {line.통화코드}" : string.Empty)).Append("</td><td>")
                .Append(E(line.금액.HasValue ? $"{line.금액} {line.통화코드}" : string.Empty)).Append("</td><td>")
                .Append(E(line.포장조건)).Append("</td></tr>");
        }
        builder.Append("</tbody></table>");
        if (문서.필수입력누락목록.Count > 0)
        {
            builder.Append("<h2 class=\"missing\">필수 입력 누락</h2><ul>");
            foreach (var missing in 문서.필수입력누락목록)
            {
                builder.Append("<li>").Append(E(missing)).Append("</li>");
            }
            builder.Append("</ul>");
        }
        builder.Append("<h2>검토 경고</h2><ul>");
        foreach (var warning in 문서.경고목록)
        {
            builder.Append("<li>").Append(E(warning)).Append("</li>");
        }
        return builder.Append("</ul></body></html>").ToString();
    }

    private static string PlainText생성(원장관행문서초안Dto 문서)
    {
        var builder = new StringBuilder()
            .AppendLine(문서.영문문서명)
            .AppendLine(문서.문서명)
            .AppendLine(초안경계안내)
            .AppendLine($"Draft No. {문서.초안문서번호}");
        foreach (var field in 문서.필드목록)
        {
            builder.AppendLine($"{field.표시명}: {field.값} ({(field.확인됨 ? "확인" : "미확인")})");
        }
        foreach (var line in 문서.품목행목록)
        {
            builder.AppendLine($"{line.순번}. {line.품명} / {line.수량} {line.수량단위} / {line.단가} {line.통화코드} / {line.금액}");
        }
        if (문서.필수입력누락목록.Count > 0)
        {
            builder.AppendLine($"필수 입력 누락: {string.Join(", ", 문서.필수입력누락목록)}");
        }
        return builder.ToString();
    }

    private static Result<원장관행문서초안묶음응답> 실패(string message, int statusCode)
        => Result.Fail<원장관행문서초안묶음응답>(
            new Error(message).WithMetadata("StatusCode", statusCode));
}
