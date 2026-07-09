using System.Globalization;
using 홍달.Services.Dispatch.Recommendation;

namespace 홍달.Services.Dispatch.Coordination;

public static class 국내화물배달권정책
{
    private const decimal 좌표격자크기 = 0.25m;

    public static 배달권판정결과 판정(배차경로좌표? 좌표, string? 주소)
    {
        var sigunguScope = 국내행정구역배달권Catalog.주소에서찾기(주소);
        if (sigunguScope is not null)
        {
            return new 배달권판정결과(
                sigunguScope.배달권키,
                sigunguScope.배달권명,
                sigunguScope.판정방식,
                sigunguScope.법정동코드,
                sigunguScope.시도명,
                sigunguScope.시군구명,
                sigunguScope.대표건물명,
                sigunguScope.대표건물주소,
                sigunguScope.대표위도,
                sigunguScope.대표경도);
        }

        if (좌표 is not null)
        {
            var latBucket = (int)Math.Floor(좌표.Latitude / 좌표격자크기);
            var lngBucket = (int)Math.Floor(좌표.Longitude / 좌표격자크기);
            var key = FormattableString.Invariant($"geo:{latBucket}:{lngBucket}");
            var name = FormattableString.Invariant($"좌표권 {latBucket},{lngBucket}");
            return new 배달권판정결과(key, name, "좌표격자");
        }

        var addressKey = ToAddressScopeKey(주소);
        return string.IsNullOrWhiteSpace(addressKey)
            ? new 배달권판정결과("unknown", "미정 배달권", "미정")
            : new 배달권판정결과($"address:{addressKey}", addressKey, "주소");
    }

    public static bool 인접배달권여부(배달권판정결과 기준, 배달권판정결과 후보)
        => 국내행정구역배달권Catalog.인접배달권여부(기준.배달권키, 후보.배달권키);

    private static string? ToAddressScopeKey(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        var parts = address
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .ToArray();
        if (parts.Length == 0)
        {
            return null;
        }

        return string.Join("-", parts).ToLower(CultureInfo.InvariantCulture);
    }
}
