using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

internal static class 같이수입준비주문자Projection
{
    public static 같이수입준비주문자조회응답 생성(같이수입준비원장응답 source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var preparation = source.준비자료 ?? new 같이수입준비원장저장요청();
        var evaluation = source.평가 ?? new 같이수입준비원장평가응답();
        var materialNames = 재료명사전(source, preparation);
        var transactionType = 공동구매거래유형코드.정규화(source.거래문맥?.거래유형);

        return new 같이수입준비주문자조회응답
        {
            상품명 = 공개문자열(source.상품명, 200),
            상태코드 = 원장상태(source.상태코드),
            거래유형 = transactionType,
            가격표시기준 = 공동구매가격표시기준코드.정규화(
                source.거래문맥?.가격표시기준,
                transactionType),
            기준시각Utc = source.저장시각Utc,
            재료집계목록 = 재료집계(source, preparation, materialNames),
            준비현황 = new 같이수입준비주문자진행상태응답
            {
                재료집계완료 = evaluation.재료품목구조완료,
                공급자근거있음 = evaluation.공급자근거구조완료,
                견적근거있음 = evaluation.견적구조완료,
                예상비용근거있음 = evaluation.예상비용구조완료,
                품목분류근거있음 = evaluation.품목분류후보구조완료,
                국가별검토근거있음 = evaluation.국가별검토구조완료,
                전문검토준비됨 = evaluation.전문검토인계가능,
                포워더인계준비됨 = evaluation.포워더인계준비가능
            },
            공급자근거목록 = (preparation.공급자근거목록 ?? [])
                .Select(item => new 같이수입준비주문자공급자근거응답
                {
                    조직명 = 공개문자열(item.조직명, 200),
                    국가코드 = 국가코드(item.국가코드),
                    관계코드 = 공개문자열(item.관계코드, 100),
                    근거요약 = 공개문자열(item.근거요약),
                    원출처명 = 공개문자열(item.원출처명, 200),
                    원출처Url = 공식Url(item.원출처Url),
                    확인시각Utc = item.확인시각Utc,
                    최신상태재확인필요 = item.최신상태재확인필요
                })
                .ToArray(),
            견적목록 = (preparation.견적목록 ?? [])
                .Select(item => new 같이수입준비주문자견적근거응답
                {
                    재료명 = 재료명(materialNames, item.재료키, source.상품명),
                    통화코드 = 통화코드(item.통화코드),
                    수량단위 = 공개문자열(item.수량단위, 30),
                    최소주문수량 = item.최소주문수량,
                    단가 = item.단가,
                    납기일수 = item.납기일수,
                    포장조건 = 공개문자열(item.포장조건, 500),
                    Incoterms후보 = Incoterms(item.Incoterms후보),
                    유효기한Utc = item.유효기한Utc,
                    원출처명 = 공개문자열(item.원출처명, 200),
                    원출처Url = 공식Url(item.원출처Url),
                    확인시각Utc = item.확인시각Utc
                })
                .ToArray(),
            예상비용목록 = (preparation.예상비용목록 ?? [])
                .Select(item => new 같이수입준비주문자예상비용응답
                {
                    재료명 = 재료명(materialNames, item.재료키, source.상품명),
                    범주코드 = 비용범주(item.범주코드),
                    표시명 = 공개문자열(item.표시명, 200),
                    통화코드 = 통화코드(item.통화코드),
                    예상금액 = item.예상금액,
                    계산근거 = 공개문자열(item.계산근거),
                    원출처Url = 공식Url(item.원출처Url),
                    확인시각Utc = item.확인시각Utc,
                    유효기한Utc = item.유효기한Utc
                })
                .ToArray(),
            품목분류목록 = (preparation.품목분류후보목록 ?? [])
                .Select(item => new 같이수입준비주문자품목분류응답
                {
                    재료명 = 재료명(materialNames, item.재료키, source.상품명),
                    관할국가코드 = 국가코드(item.관할국가코드),
                    분류체계코드 = 공개문자열(item.분류체계코드, 30),
                    품목코드 = 공개문자열(item.품목코드, 50),
                    분류근거 = 공개문자열(item.분류근거),
                    신뢰도 = Math.Clamp(item.신뢰도, 0m, 1m),
                    검토상태코드 = 검토상태(item.검토상태코드),
                    원출처Url = 공식Url(item.원출처Url),
                    확인시각Utc = item.확인시각Utc,
                    전문가검토필요 = item.전문가검토필요
                })
                .ToArray(),
            국가별검토목록 = (preparation.국가별검토항목목록 ?? [])
                .Select(item => new 같이수입준비주문자국가별검토응답
                {
                    관할국가코드 = 국가코드(item.관할국가코드),
                    표시명 = 공개문자열(item.표시명, 200),
                    검토상태코드 = 검토상태(item.검토상태코드),
                    책임역할코드 = 책임역할(item.책임역할코드),
                    공식원출처Url = 공식Url(item.공식원출처Url),
                    확인시각Utc = item.확인시각Utc,
                    미확인사유 = 공개문자열(item.미확인사유)
                })
                .ToArray(),
            포워더인계 = 포워더인계(preparation.포워더인계),
            국제운송검토 = 국제운송(preparation.국제운송검토)
        };
    }

    private static IReadOnlyList<같이수입준비주문자재료집계응답> 재료집계(
        같이수입준비원장응답 source,
        같이수입준비원장저장요청 preparation,
        IReadOnlyDictionary<string, string> materialNames)
    {
        var sources = source.원천수요목록 ?? [];
        var items = sources.Count > 0
            ? sources.Select(item => new 같이수입준비주문자재료집계응답
            {
                재료명 = 재료명(materialNames, item.재료키, item.재료명),
                모인수요수량 = item.모인수요수량,
                수량단위 = 공개문자열(item.수량단위, 30)
            })
            : (preparation.재료품목목록 ?? []).Select(item => new 같이수입준비주문자재료집계응답
            {
                재료명 = 재료명(materialNames, item.재료키, item.재료명),
                모인수요수량 = item.모인수요수량,
                수량단위 = 공개문자열(item.수량단위, 30)
            });

        var aggregated = items
            .GroupBy(
                item => $"{item.재료명}\u001f{item.수량단위}",
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new 같이수입준비주문자재료집계응답
            {
                재료명 = group.First().재료명,
                모인수요수량 = group.Sum(item => item.모인수요수량),
                수량단위 = group.First().수량단위
            })
            .ToArray();

        if (aggregated.Length > 0)
        {
            return aggregated;
        }

        return
        [
            new 같이수입준비주문자재료집계응답
            {
                재료명 = 공개문자열(source.상품명, 200),
                모인수요수량 = source.모인수요수량,
                수량단위 = 공개문자열(source.수량단위, 30)
            }
        ];
    }

    private static Dictionary<string, string> 재료명사전(
        같이수입준비원장응답 source,
        같이수입준비원장저장요청 preparation)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in source.원천수요목록 ?? [])
        {
            재료명추가(result, item.재료키, item.재료명);
        }

        foreach (var item in preparation.재료품목목록 ?? [])
        {
            재료명추가(result, item.재료키, item.재료명);
        }

        재료명추가(result, source.상품키, source.상품명);
        return result;
    }

    private static void 재료명추가(IDictionary<string, string> result, string? key, string? name)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        result.TryAdd(key.Trim(), 공개문자열(name, 200));
    }

    private static string 재료명(
        IReadOnlyDictionary<string, string> materialNames,
        string? key,
        string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(key)
            && materialNames.TryGetValue(key.Trim(), out var name)
            && !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var publicFallback = 공개문자열(fallback, 200);
        return string.IsNullOrWhiteSpace(publicFallback) ? "재료" : publicFallback;
    }

    private static 같이수입준비주문자포워더인계응답 포워더인계(같이수입준비포워더인계? source)
    {
        source ??= new 같이수입준비포워더인계();
        var scope = string.Equals(
            source.전달정보범위코드,
            같이수입준비포워더전달정보범위코드.동의된사용자별최소정보,
            StringComparison.OrdinalIgnoreCase)
            ? 같이수입준비포워더전달정보범위코드.동의된사용자별최소정보
            : 같이수입준비포워더전달정보범위코드.집계수요전용;

        return new 같이수입준비주문자포워더인계응답
        {
            인계상태코드 = 같이수입준비포워더인계상태코드.지원목록.Contains(source.인계상태코드)
                ? source.인계상태코드.Trim()
                : 같이수입준비포워더인계상태코드.초안,
            전달대상업체명 = 공개문자열(source.전달대상업체명, 200),
            전달정보범위코드 = scope,
            전달항목코드목록 = (source.전달항목코드목록 ?? [])
                .Where(item => 같이수입준비포워더전달항목코드.기본집계목록.Contains(
                    item,
                    StringComparer.OrdinalIgnoreCase))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            전달범위요약 = scope == 같이수입준비포워더전달정보범위코드.집계수요전용
                ? "개인 식별정보를 제외한 재료별 합산 수요와 물류 조건"
                : "운영자가 별도 정보 제공 조건 확인을 기록한 최소 범위",
            개인정보포함여부 = source.개인정보포함여부,
            운영자기록정보제공조건확인여부 = source.정보제공동의확인여부,
            인계시각Utc = source.인계시각Utc
        };
    }

    private static 같이수입준비주문자국제운송응답 국제운송(같이수입준비국제운송검토? source)
    {
        source ??= new 같이수입준비국제운송검토();
        return new 같이수입준비주문자국제운송응답
        {
            검토상태코드 = 같이수입준비국제운송검토상태코드.지원목록.Contains(source.검토상태코드)
                ? source.검토상태코드.Trim()
                : 같이수입준비국제운송검토상태코드.검토필요,
            방식후보목록 = (source.방식후보목록 ?? [])
                .Where(같이수입준비국제운송방식코드.지원여부)
                .Select(item => item.Trim().ToUpperInvariant())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            포워더제안방식코드 = 같이수입준비국제운송방식코드.지원여부(source.포워더제안방식코드)
                ? source.포워더제안방식코드.Trim().ToUpperInvariant()
                : string.Empty,
            포워더회신요약 = 공개문자열(source.포워더회신요약),
            회신업체표시명 = 공개문자열(source.회신업체표시명, 200),
            회신시각Utc = source.회신시각Utc
        };
    }

    private static string 원장상태(string? value)
        => string.Equals(
            value,
            같이수입준비원장상태코드.전문검토자료준비,
            StringComparison.OrdinalIgnoreCase)
            ? 같이수입준비원장상태코드.전문검토자료준비
            : 같이수입준비원장상태코드.초안;

    private static string 검토상태(string? value)
        => 같이수입준비검토상태코드.지원목록.Contains(value ?? string.Empty)
            ? value!.Trim()
            : 같이수입준비검토상태코드.미확인;

    private static string 비용범주(string? value)
        => 같이수입준비비용범주코드.필수목록.Contains(
            value ?? string.Empty,
            StringComparer.OrdinalIgnoreCase)
            ? value!.Trim()
            : string.Empty;

    private static string 책임역할(string? value)
    {
        var supported = new[]
        {
            같이수입준비책임역할코드.판매자수출자,
            같이수입준비책임역할코드.수입자,
            같이수입준비책임역할코드.관세사,
            같이수입준비책임역할코드.플랫폼,
            같이수입준비책임역할코드.운송수행자
        };
        return supported.Contains(value ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            ? value!.Trim()
            : string.Empty;
    }

    private static string Incoterms(string? value)
        => 같이수입준비Incoterms코드.후보목록.Contains(
            value ?? string.Empty,
            StringComparer.OrdinalIgnoreCase)
            ? value!.Trim().ToUpperInvariant()
            : string.Empty;

    private static string 국가코드(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalized.Length == 2 ? normalized : string.Empty;
    }

    private static string 통화코드(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalized.Length == 3 ? normalized : string.Empty;
    }

    private static string 공식Url(string? value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return string.Empty;
        }

        return uri.AbsoluteUri.Length <= 2048 ? uri.AbsoluteUri : string.Empty;
    }

    private static string 공개문자열(string? value, int maxLength = 1000)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var cleaned = string.Concat(value.Trim().Where(character =>
            !char.IsControl(character) || character is '\r' or '\n' or '\t'));
        return cleaned.Length <= maxLength ? cleaned : cleaned[..maxLength];
    }
}
