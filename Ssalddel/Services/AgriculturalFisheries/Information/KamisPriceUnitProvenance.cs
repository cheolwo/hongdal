using System.Text.RegularExpressions;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal sealed record KamisPriceUnitProvenance(
    string SourcePackageLabel,
    string ComparisonUnit,
    string PriceNormalizationCode,
    string PriceNormalizationBasis);

internal static partial class KamisPriceUnitProvenanceParser
{
    public const string SourceKilogramConversionCode =
        "KamisSourceKilogramConversion";

    public const string SourceKilogramConversionBasis =
        "KAMIS 요청 p_convert_kg_yn=Y로 원천이 1kg 비교가격을 반환하며 서버는 가격을 재환산하지 않습니다.";

    public static KamisPriceUnitProvenance FromKindName(string? kindName)
    {
        var match = SourcePackageRegex().Match(kindName?.Trim() ?? string.Empty);
        return new KamisPriceUnitProvenance(
            match.Success ? match.Groups["label"].Value.Trim() : string.Empty,
            "1kg",
            SourceKilogramConversionCode,
            SourceKilogramConversionBasis);
    }

    [GeneratedRegex(
        @"\((?<label>\d+(?:\.\d+)?\s*(?:kg|g|개|마리|포기|단|속|망|상자|봉|팩|묶음|l|ml))\)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SourcePackageRegex();
}
