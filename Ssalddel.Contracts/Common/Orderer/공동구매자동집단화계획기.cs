namespace Ssalddel.Contracts.Common.Orderer;

public static class 공동구매자동집단화계획기
{
    public static string 자동집단키생성(
        string 상품키,
        string 배송권키,
        string 온도코드,
        string 물류방식)
        => 자동집단키생성(
            상품키,
            배송권키,
            온도코드,
            물류방식,
            공동구매거래유형코드.B2C,
            공동구매가격표시기준코드.부가세포함,
            string.Empty);

    public static string 자동집단키생성(
        string 상품키,
        string 배송권키,
        string 온도코드,
        string 물류방식,
        string 거래유형,
        string 가격표시기준,
        string 수량단위)
    {
        var 기본키 = string.Join(
            ':',
            "auto-gp",
            정규화(상품키, "unknown-product"),
            정규화(배송권키, "unknown-scope"),
            정규화(온도코드, "normal"),
            정규화(물류방식, "review-required"));
        if (공동구매거래유형코드.정규화(거래유형) == 공동구매거래유형코드.B2C)
        {
            // 기존 B2C 원장 ID와의 호환성을 보존합니다.
            return 기본키;
        }

        return string.Join(
            ':',
            기본키,
            "b2b",
            정규화(공동구매가격표시기준코드.정규화(가격표시기준, 거래유형), "vat-included"),
            정규화(수량단위, "business-unit"));
    }

    public static string 상태제안(int 수요건수, int 예약결제건수, decimal 총희망수량)
        => 상태제안(수요건수, 예약결제건수, 총희망수량, null, null);

    public static string 상태제안(
        int 수요건수,
        int 예약결제건수,
        decimal 총희망수량,
        int? 목표참여자수,
        decimal? 목표수량)
        => 상태제안(
            수요건수,
            예약결제건수,
            총희망수량,
            목표참여자수,
            목표수량,
            공동구매거래유형코드.B2C);

    public static string 상태제안(
        int 수요건수,
        int 예약결제건수,
        decimal 총희망수량,
        int? 목표참여자수,
        decimal? 목표수량,
        string 거래유형)
    {
        var 사업구매 = 공동구매거래유형코드.정규화(거래유형) == 공동구매거래유형코드.B2B;
        if (수요건수 < (사업구매 ? 1 : 2))
        {
            return 공동구매자동집단상태코드.수요수집중;
        }

        var 참여자목표충족 = !목표참여자수.HasValue || 수요건수 >= 목표참여자수.Value;
        var 수량목표충족 = !목표수량.HasValue || 총희망수량 >= 목표수량.Value;
        var 명시목표존재 = 목표참여자수.HasValue || 목표수량.HasValue;
        var 명시목표충족 = 명시목표존재 && 참여자목표충족 && 수량목표충족;

        var 예약결제기준 = 사업구매 ? 1 : 2;
        if (예약결제건수 >= 예약결제기준
            || 명시목표충족
            || !명시목표존재 && (수요건수 >= 5 || 총희망수량 >= 30))
        {
            return 공동구매자동집단상태코드.확정대기;
        }

        return 공동구매자동집단상태코드.수요수집중;
    }

    private static string 정규화(string? 값, string 기본값)
    {
        if (string.IsNullOrWhiteSpace(값))
        {
            return 기본값;
        }

        var chars = 값.Trim().ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();
        var 정규화값 = new string(chars);
        while (정규화값.Contains("--", StringComparison.Ordinal))
        {
            정규화값 = 정규화값.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(정규화값.Trim('-')) ? 기본값 : 정규화값.Trim('-');
    }
}
