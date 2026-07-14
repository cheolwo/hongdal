namespace Hongdal.Contracts.Common.Orderer;

public static class 공동구매자동집단화계획기
{
    public static string 자동집단키생성(
        string 상품키,
        string 배송권키,
        string 온도코드,
        string 물류방식)
    {
        return string.Join(
            ':',
            "auto-gp",
            정규화(상품키, "unknown-product"),
            정규화(배송권키, "unknown-scope"),
            정규화(온도코드, "normal"),
            정규화(물류방식, "lcl"));
    }

    public static string 상태제안(int 수요건수, int 예약결제건수, decimal 총희망수량)
        => 상태제안(수요건수, 예약결제건수, 총희망수량, null, null);

    public static string 상태제안(
        int 수요건수,
        int 예약결제건수,
        decimal 총희망수량,
        int? 목표참여자수,
        decimal? 목표수량)
    {
        var 참여자목표충족 = !목표참여자수.HasValue || 수요건수 >= 목표참여자수.Value;
        var 수량목표충족 = !목표수량.HasValue || 총희망수량 >= 목표수량.Value;
        var 명시목표존재 = 목표참여자수.HasValue || 목표수량.HasValue;
        var 명시목표충족 = 명시목표존재 && 참여자목표충족 && 수량목표충족;

        if (예약결제건수 >= 2 || 명시목표충족 || !명시목표존재 && (수요건수 >= 5 || 총희망수량 >= 30))
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
