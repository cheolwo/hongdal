using System.Globalization;
using 살뜰.Services.Dispatch.Recommendation;

namespace 살뜰.Services.Dispatch.Coordination;

public static class 음식배달권정책
{
    public const string 공간정책버전 = "food-cell-v1";

    private const decimal 위도격자크기 = 0.025m;
    private const decimal 경도격자크기 = 0.025m;
    private const string 셀접두어 = "food-cell:v1:";
    private const string 공통배달권접두어 = "food-scope:v1:";

    public static 배달권판정결과 판정(배차경로좌표? 좌표, string? 주소)
    {
        if (좌표 is not null)
        {
            var 위도Bucket = (int)Math.Floor(좌표.Latitude / 위도격자크기);
            var 경도Bucket = (int)Math.Floor(좌표.Longitude / 경도격자크기);
            return new 배달권판정결과(
                Build셀키(위도Bucket, 경도Bucket),
                FormattableString.Invariant($"음식배달 셀 {위도Bucket},{경도Bucket}"),
                공간정책버전);
        }

        var 공통배달권 = 국내화물배달권정책.판정(null, 주소);
        return string.Equals(공통배달권.배달권키, "unknown", StringComparison.Ordinal)
            ? new 배달권판정결과("unknown", "미정 음식배달권", 공간정책버전)
            : 공통배달권 with
            {
                배달권키 = $"{공통배달권접두어}{공통배달권.배달권키}",
                배달권명 = $"음식배달 {공통배달권.배달권명}",
                판정방식 = $"{공간정책버전}:{공통배달권.판정방식}"
            };
    }

    public static IReadOnlyList<string> 인접배달권키조회(string 배달권키)
    {
        if (TryParse셀키(배달권키, out var 위도Bucket, out var 경도Bucket))
        {
            var keys = new List<string>(8);
            for (var 위도Offset = -1; 위도Offset <= 1; 위도Offset++)
            {
                for (var 경도Offset = -1; 경도Offset <= 1; 경도Offset++)
                {
                    if (위도Offset == 0 && 경도Offset == 0)
                    {
                        continue;
                    }

                    keys.Add(Build셀키(위도Bucket + 위도Offset, 경도Bucket + 경도Offset));
                }
            }

            return keys;
        }

        if (!배달권키.StartsWith(공통배달권접두어, StringComparison.Ordinal))
        {
            return [];
        }

        var 공통배달권키 = 배달권키[공통배달권접두어.Length..];
        return 국내행정구역배달권Catalog.인접배달권키조회(공통배달권키)
            .Select(key => $"{공통배달권접두어}{key}")
            .ToArray();
    }

    public static IReadOnlyList<string> 거리확장배달권키조회(
        string 배달권키,
        decimal 반경Km)
    {
        if (!TryParse셀키(배달권키, out var 위도Bucket, out var 경도Bucket))
        {
            return [];
        }

        const decimal 보수적셀폭Km = 2m;
        var 확장칸수 = Math.Max(2, (int)Math.Ceiling(Math.Max(1m, 반경Km) / 보수적셀폭Km));
        var keys = new List<string>();
        for (var 위도Offset = -확장칸수; 위도Offset <= 확장칸수; 위도Offset++)
        {
            for (var 경도Offset = -확장칸수; 경도Offset <= 확장칸수; 경도Offset++)
            {
                if (Math.Abs(위도Offset) <= 1 && Math.Abs(경도Offset) <= 1)
                {
                    continue;
                }

                keys.Add(Build셀키(위도Bucket + 위도Offset, 경도Bucket + 경도Offset));
            }
        }

        return keys;
    }

    private static string Build셀키(int 위도Bucket, int 경도Bucket)
        => FormattableString.Invariant($"{셀접두어}{위도Bucket}:{경도Bucket}");

    private static bool TryParse셀키(string value, out int 위도Bucket, out int 경도Bucket)
    {
        위도Bucket = 0;
        경도Bucket = 0;
        if (string.IsNullOrWhiteSpace(value)
            || !value.StartsWith(셀접두어, StringComparison.Ordinal))
        {
            return false;
        }

        var parts = value[셀접두어.Length..].Split(':', StringSplitOptions.TrimEntries);
        return parts.Length == 2
               && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out 위도Bucket)
               && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out 경도Bucket);
    }
}
