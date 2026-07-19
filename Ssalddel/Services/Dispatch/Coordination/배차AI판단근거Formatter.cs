namespace 살뜰.Services.Dispatch.Coordination;

public static class 배차AI판단근거Formatter
{
    public static string 요약(배차AI판단근거? 판단근거)
    {
        var ids = 판단근거?.정책근거목록
            .Select(x => x.근거Id)
            .Concat(판단근거.사례목록.Select(x => x.사례Id))
            .Distinct(StringComparer.Ordinal)
            .Take(4)
            .ToArray() ?? [];

        return ids.Length == 0 ? string.Empty : $"판단근거={string.Join(",", ids)}";
    }

    public static string 사유추가(string reason, string 판단근거요약)
        => string.IsNullOrWhiteSpace(reason)
            ? 판단근거요약
            : $"{reason} {판단근거요약}";
}
