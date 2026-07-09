namespace 홍달.Services.Dispatch.Coordination;

public static partial class 국내행정구역배달권Catalog
{
    public static IReadOnlyList<기초배달권항목> 전체조회() => Items;

    public static IReadOnlyList<기초배달권항목> 시도조회() => 시도Items;

    public static IReadOnlyList<기초배달권항목> 시군구조회() => 시군구Items;

    public static IReadOnlyList<string> 인접배달권키조회(string 배달권키)
        => 인접배달권Map.TryGetValue(배달권키, out var items) ? items : [];

    public static bool 인접배달권여부(string 기준배달권키, string 후보배달권키)
    {
        if (string.IsNullOrWhiteSpace(기준배달권키) || string.IsNullOrWhiteSpace(후보배달권키))
        {
            return false;
        }

        if (string.Equals(기준배달권키, 후보배달권키, StringComparison.Ordinal))
        {
            return false;
        }

        return 인접배달권Map.TryGetValue(기준배달권키, out var 기준인접목록)
               && 기준인접목록.Contains(후보배달권키, StringComparer.Ordinal);
    }

    public static 기초배달권항목? 주소에서찾기(string? 주소)
    {
        if (string.IsNullOrWhiteSpace(주소))
        {
            return null;
        }

        var normalized = 주소.Trim();
        var sigungu = 시군구Items.FirstOrDefault(item => 주소일치(item, normalized));
        if (sigungu is not null)
        {
            return sigungu;
        }

        return 시도Items.FirstOrDefault(item => 주소일치(item, normalized));
    }

    private static bool 주소일치(기초배달권항목 item, string normalized)
    {
        var sidoMatches = normalized.Contains(item.시도명, StringComparison.Ordinal)
                          || (시도축약명(item.시도명) is { } shortName
                              && normalized.Contains(shortName, StringComparison.Ordinal));
        if (!sidoMatches)
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(item.시군구명)
               || normalized.Contains(item.시군구명, StringComparison.Ordinal);
    }

    private static string? 시도축약명(string 시도명)
        => 시도명 switch
        {
            "서울특별시" => "서울",
            "부산광역시" => "부산",
            "대구광역시" => "대구",
            "인천광역시" => "인천",
            "광주광역시" => "광주",
            "대전광역시" => "대전",
            "울산광역시" => "울산",
            "세종특별자치시" => "세종",
            "경기도" => "경기",
            "강원특별자치도" => "강원",
            "충청북도" => "충북",
            "충청남도" => "충남",
            "전북특별자치도" => "전북",
            "전라남도" => "전남",
            "경상북도" => "경북",
            "경상남도" => "경남",
            "제주특별자치도" => "제주",
            _ => null
        };
}
