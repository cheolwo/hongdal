namespace Ssalddel.Application.CommandProcessing;

public sealed record Command기능버전정보(string Version, string DisplayName, int SortOrder, bool IsCurrentRelease);

public static class Command기능버전Catalog
{
    public const string CurrentRelease = "1.0";

    private static readonly IReadOnlyDictionary<string, Command기능버전정보> Versions =
        new Dictionary<string, Command기능버전정보>(StringComparer.OrdinalIgnoreCase)
        {
            ["1.0"] = new("1.0", "1.0 공동구매/주문자 집단화", 100, true),
            ["1.5"] = new("1.5", "1.5 공급/가격/무역 준비", 150, false),
            ["2.0"] = new("2.0", "2.0 국내 화물/운송 이행", 200, false),
            ["2.5"] = new("2.5", "2.5 창고/판매 이행", 250, false),
            ["3.0"] = new("3.0", "3.0 음식점 일반 배달", 300, false),
            ["3.5"] = new("3.5", "3.5 알뜰살뜰 마트/도심 즉시배송", 350, false)
        };

    public static Command기능버전정보 Get(string? version)
    {
        var key = string.IsNullOrWhiteSpace(version) ? CurrentRelease : version.Trim();
        return Versions.TryGetValue(key, out var info)
            ? info
            : new Command기능버전정보(key, $"{key} 확장 기능", 900, false);
    }
}
