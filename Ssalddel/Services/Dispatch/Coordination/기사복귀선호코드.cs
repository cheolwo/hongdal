namespace 살뜰.Services.Dispatch.Coordination;

public static class 기사복귀선호코드
{
    public const string 균형 = "균형";
    public const string 복귀우선 = "복귀우선";
    public const string 수익우선 = "수익우선";

    public static string Normalize(string? value)
    {
        if (string.Equals(value, 복귀우선, StringComparison.OrdinalIgnoreCase))
        {
            return 복귀우선;
        }

        if (string.Equals(value, 수익우선, StringComparison.OrdinalIgnoreCase))
        {
            return 수익우선;
        }

        return 균형;
    }
}
