namespace Hongdal.Application.Driver.Transport;

public sealed record 기사운송증빙조건(string 결제수단, string 증빙방식, string 요청사항, string 정산메모)
{
    public static readonly 기사운송증빙조건 Empty = new(string.Empty, string.Empty, string.Empty, string.Empty);
}

public static class 기사운송증빙조건정책
{
    public static bool 인수증필요(기사운송증빙조건? condition)
        => condition is not null && 인수증필요(condition.증빙방식, condition.결제수단);

    public static bool 인수증필요(string? 증빙방식, string? 결제수단)
        => string.Equals(증빙방식, "인수증", StringComparison.Ordinal)
           || (결제수단?.Contains("인수증", StringComparison.Ordinal) ?? false);

    public static bool 인수증서명필수(기사운송증빙조건? condition)
        => condition is not null && 인수증서명필수(condition.요청사항, condition.정산메모);

    public static bool 인수증서명필수(string? 요청사항, string? 정산메모)
        => ContainsSignatureRequired(요청사항) || ContainsSignatureRequired(정산메모);

    private static bool ContainsSignatureRequired(string? value)
        => value?.Contains("서명필수", StringComparison.Ordinal) == true
           || value?.Contains("서명 필수", StringComparison.Ordinal) == true;
}
