namespace Ssalddel.Domain.PublicData.Korea;

public static class 공개사업장영업상태Codes
{
    public const string 영업 = "Open";
    public const string 휴업 = "Suspended";
    public const string 폐업 = "Closed";
    public const string 미확인 = "Unresolved";
}

public static class 공개사업장영업상태Engine
{
    public static string 분류(string? businessStatusName, string? detailedStatusName)
    {
        var status = $"{businessStatusName} {detailedStatusName}".Trim();
        if (status.Contains("폐업", StringComparison.Ordinal))
            return 공개사업장영업상태Codes.폐업;
        if (status.Contains("휴업", StringComparison.Ordinal)
            || status.Contains("정지", StringComparison.Ordinal))
            return 공개사업장영업상태Codes.휴업;
        if (status.Contains("영업", StringComparison.Ordinal)
            || status.Contains("정상", StringComparison.Ordinal))
            return 공개사업장영업상태Codes.영업;
        return 공개사업장영업상태Codes.미확인;
    }
}
