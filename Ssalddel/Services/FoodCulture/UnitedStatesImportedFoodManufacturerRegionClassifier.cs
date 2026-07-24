using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ssalddel.Services.FoodCulture;

public static class UnitedStatesImportedFoodManufacturerRegionCodes
{
    public const string Prefix = "US-";

    public const string OtherOrUnclassified = "US-OTHER-UNCLASSIFIED";

    public static string StateOrArea(string postalCode)
        => $"{Prefix}{postalCode.Trim().ToUpperInvariant()}";
}

public static class UnitedStatesImportedFoodManufacturerRegionMethodCodes
{
    public const string OfficialAreaName = "OfficialAreaName";

    public const string OfficialAreaPostalCode = "OfficialAreaPostalCode";

    public const string FacilityAddressStateName = "FacilityAddressStateName";

    public const string FacilityAddressPostalCode = "FacilityAddressPostalCode";

    public const string ExplicitForeignFacilityAddress = "ExplicitForeignFacilityAddress";

    public const string CountryOnly = "CountryOnly";
}

public sealed record UnitedStatesImportedFoodAreaDefinition(
    string PostalCode,
    string FipsCode,
    string EnglishName,
    string KoreanName,
    bool IsState,
    bool IsDistrict,
    bool IsTerritory)
{
    public string RegionCode =>
        UnitedStatesImportedFoodManufacturerRegionCodes.StateOrArea(PostalCode);
}

public static partial class UnitedStatesImportedFoodManufacturerRegionClassifier
{
    private static readonly UnitedStatesImportedFoodAreaDefinition[] Areas =
    [
        State("AL", "01", "Alabama", "앨라배마주"),
        State("AK", "02", "Alaska", "알래스카주"),
        State("AZ", "04", "Arizona", "애리조나주"),
        State("AR", "05", "Arkansas", "아칸소주"),
        State("CA", "06", "California", "캘리포니아주"),
        State("CO", "08", "Colorado", "콜로라도주"),
        State("CT", "09", "Connecticut", "코네티컷주"),
        State("DE", "10", "Delaware", "델라웨어주"),
        District("DC", "11", "District of Columbia", "워싱턴 D.C."),
        State("FL", "12", "Florida", "플로리다주"),
        State("GA", "13", "Georgia", "조지아주"),
        State("HI", "15", "Hawaii", "하와이주"),
        State("ID", "16", "Idaho", "아이다호주"),
        State("IL", "17", "Illinois", "일리노이주"),
        State("IN", "18", "Indiana", "인디애나주"),
        State("IA", "19", "Iowa", "아이오와주"),
        State("KS", "20", "Kansas", "캔자스주"),
        State("KY", "21", "Kentucky", "켄터키주"),
        State("LA", "22", "Louisiana", "루이지애나주"),
        State("ME", "23", "Maine", "메인주"),
        State("MD", "24", "Maryland", "메릴랜드주"),
        State("MA", "25", "Massachusetts", "매사추세츠주"),
        State("MI", "26", "Michigan", "미시간주"),
        State("MN", "27", "Minnesota", "미네소타주"),
        State("MS", "28", "Mississippi", "미시시피주"),
        State("MO", "29", "Missouri", "미주리주"),
        State("MT", "30", "Montana", "몬태나주"),
        State("NE", "31", "Nebraska", "네브래스카주"),
        State("NV", "32", "Nevada", "네바다주"),
        State("NH", "33", "New Hampshire", "뉴햄프셔주"),
        State("NJ", "34", "New Jersey", "뉴저지주"),
        State("NM", "35", "New Mexico", "뉴멕시코주"),
        State("NY", "36", "New York", "뉴욕주"),
        State("NC", "37", "North Carolina", "노스캐롤라이나주"),
        State("ND", "38", "North Dakota", "노스다코타주"),
        State("OH", "39", "Ohio", "오하이오주"),
        State("OK", "40", "Oklahoma", "오클라호마주"),
        State("OR", "41", "Oregon", "오리건주"),
        State("PA", "42", "Pennsylvania", "펜실베이니아주"),
        State("RI", "44", "Rhode Island", "로드아일랜드주"),
        State("SC", "45", "South Carolina", "사우스캐롤라이나주"),
        State("SD", "46", "South Dakota", "사우스다코타주"),
        State("TN", "47", "Tennessee", "테네시주"),
        State("TX", "48", "Texas", "텍사스주"),
        State("UT", "49", "Utah", "유타주"),
        State("VT", "50", "Vermont", "버몬트주"),
        State("VA", "51", "Virginia", "버지니아주"),
        State("WA", "53", "Washington", "워싱턴주"),
        State("WV", "54", "West Virginia", "웨스트버지니아주"),
        State("WI", "55", "Wisconsin", "위스콘신주"),
        State("WY", "56", "Wyoming", "와이오밍주"),
        Territory("AS", "60", "American Samoa", "미국령 사모아"),
        Territory("GU", "66", "Guam", "괌"),
        Territory("MP", "69", "Northern Mariana Islands", "북마리아나제도"),
        Territory("PR", "72", "Puerto Rico", "푸에르토리코"),
        Territory("VI", "78", "Virgin Islands", "미국령 버진아일랜드")
    ];

    private static readonly string[] ExplicitForeignCountryNames =
    [
        "CHINA",
        "PEOPLES REPUBLIC OF CHINA",
        "VIETNAM",
        "THAILAND",
        "CANADA",
        "MEXICO",
        "PERU",
        "CHILE",
        "INDIA",
        "INDONESIA",
        "ECUADOR",
        "NEW ZEALAND",
        "AUSTRALIA",
        "TURKEY",
        "SPAIN",
        "ITALY",
        "FRANCE",
        "GERMANY",
        "SOUTH KOREA",
        "REPUBLIC OF KOREA",
        "JAPAN",
        "SINGAPORE"
    ];

    public static IReadOnlyList<UnitedStatesImportedFoodAreaDefinition> Definitions =>
        Areas;

    public static ImportedFoodManufacturerRegionClassification? Classify(
        string? countryName,
        string? officialAreaName,
        string? facilityAddress)
    {
        if (!IsUnitedStates(countryName))
        {
            return null;
        }

        var areaWords = NormalizeWords(officialAreaName);
        foreach (var area in Areas)
        {
            if (areaWords == NormalizeWords(area.EnglishName)
                || areaWords == NormalizeWords(area.KoreanName))
            {
                return Result(
                    area,
                    UnitedStatesImportedFoodManufacturerRegionMethodCodes.OfficialAreaName,
                    $"식약처 해외제조업소 지역명에서 {area.KoreanName} 확인",
                    1.0000m);
            }

            if (areaWords == area.PostalCode)
            {
                return Result(
                    area,
                    UnitedStatesImportedFoodManufacturerRegionMethodCodes
                        .OfficialAreaPostalCode,
                    $"식약처 해외제조업소 지역명에서 USPS 코드 {area.PostalCode} 확인",
                    1.0000m);
            }
        }

        var addressWords = NormalizeWords(facilityAddress);
        if (HasExplicitForeignCountry(addressWords))
        {
            return new ImportedFoodManufacturerRegionClassification(
                UnitedStatesImportedFoodManufacturerRegionCodes.OtherOrUnclassified,
                "미국 기타·미분류",
                "제품 국가가 미국으로 기록됐지만 해외제조업소 주소에 미국 외 국가 표기가 있어 주를 부여하지 않은 항목입니다.",
                UnitedStatesImportedFoodManufacturerRegionMethodCodes
                    .ExplicitForeignFacilityAddress,
                "미국 외 국가가 표시된 해외제조업소 주소이므로 주 분류 보류",
                0.4000m);
        }

        foreach (var area in Areas.OrderByDescending(item => item.EnglishName.Length))
        {
            if (ContainsPhrase(addressWords, NormalizeWords(area.EnglishName))
                || ContainsPhrase(addressWords, NormalizeWords(area.KoreanName)))
            {
                return Result(
                    area,
                    UnitedStatesImportedFoodManufacturerRegionMethodCodes
                        .FacilityAddressStateName,
                    $"식약처 해외제조업소 주소에서 {area.KoreanName} 확인",
                    0.9500m);
            }
        }

        var normalizedAddress = NormalizeAddress(facilityAddress);
        foreach (var area in Areas)
        {
            if (ContainsPostalCodeAddressEvidence(normalizedAddress, area.PostalCode))
            {
                return Result(
                    area,
                    UnitedStatesImportedFoodManufacturerRegionMethodCodes
                        .FacilityAddressPostalCode,
                    $"식약처 해외제조업소 주소에서 USPS 코드 {area.PostalCode} 확인",
                    0.9000m);
            }
        }

        return new ImportedFoodManufacturerRegionClassification(
            UnitedStatesImportedFoodManufacturerRegionCodes.OtherOrUnclassified,
            "미국 기타·미분류",
            "제품 국가가 미국으로 확인되지만 50개 주·워싱턴 D.C.·미국령 지역을 판정할 공식 주소 근거가 없는 항목입니다.",
            UnitedStatesImportedFoodManufacturerRegionMethodCodes.CountryOnly,
            "제품 국가는 미국이나 주를 판정할 공식 지역 근거가 없음",
            0.5000m);
    }

    public static bool IsStateRegionCode(string regionCode)
        => Areas.Any(area => area.IsState
                             && string.Equals(
                                 area.RegionCode,
                                 regionCode,
                                 StringComparison.Ordinal));

    public static bool IsDistrictOrTerritoryRegionCode(string regionCode)
        => Areas.Any(area => (area.IsDistrict || area.IsTerritory)
                             && string.Equals(
                                 area.RegionCode,
                                 regionCode,
                                 StringComparison.Ordinal));

    public static UnitedStatesImportedFoodAreaDefinition? FindByRegionCode(string regionCode)
        => Areas.FirstOrDefault(area => string.Equals(
            area.RegionCode,
            regionCode,
            StringComparison.Ordinal));

    private static ImportedFoodManufacturerRegionClassification Result(
        UnitedStatesImportedFoodAreaDefinition area,
        string methodCode,
        string evidence,
        decimal confidence)
        => new(
            area.RegionCode,
            area.KoreanName,
            Scope(area),
            methodCode,
            evidence,
            confidence);

    private static string Scope(UnitedStatesImportedFoodAreaDefinition area)
    {
        var areaType = area.IsState
            ? "미국 50개 주"
            : area.IsDistrict
                ? "미국 연방구"
                : "미국령 지역";
        return $"{areaType}의 {area.KoreanName} 소재 해외제조업소를 뜻합니다. " +
               $"USPS 코드 {area.PostalCode}, Census FIPS {area.FipsCode}. " +
               "원재료 생산·재배·어획 주나 법정 원산지를 확정하지 않습니다.";
    }

    private static bool IsUnitedStates(string? countryName)
    {
        var normalized = NormalizeWords(countryName);
        return normalized is "US" or "USA" or "UNITED STATES" or "UNITED STATES OF AMERICA"
               || normalized.Contains("미국", StringComparison.Ordinal)
               || normalized.Contains("미합중국", StringComparison.Ordinal);
    }

    private static bool HasExplicitForeignCountry(string addressWords)
        => ExplicitForeignCountryNames.Any(country =>
            ContainsPhrase(addressWords, country));

    private static bool ContainsPhrase(string source, string phrase)
        => source.Length > 0
           && phrase.Length > 0
           && $" {source} ".Contains($" {phrase} ", StringComparison.Ordinal);

    private static bool ContainsPostalCodeAddressEvidence(
        string address,
        string postalCode)
    {
        if (address.Length == 0)
        {
            return false;
        }

        var pattern =
            $@"(?:^|[,\s]){Regex.Escape(postalCode)}" +
            @"(?:\s*[-,]?\s*\d{5}(?:-\d{4})?\b|\s*,|\s+USA\b|" +
            @"\s+UNITED\s+STATES\b|\s*$)";
        return Regex.IsMatch(
            address,
            pattern,
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    private static string NormalizeAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value
            .Normalize(NormalizationForm.FormKC)
            .ToUpper(CultureInfo.InvariantCulture)
            .Replace(".", string.Empty, StringComparison.Ordinal);
        return WhitespaceRegex().Replace(normalized, " ").Trim();
    }

    private static string NormalizeWords(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value
            .Normalize(NormalizationForm.FormKC)
            .ToUpper(CultureInfo.InvariantCulture);
        var builder = new StringBuilder(normalized.Length);
        var previousWasSeparator = true;
        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator)
            {
                builder.Append(' ');
                previousWasSeparator = true;
            }
        }

        return builder.ToString().Trim();
    }

    private static UnitedStatesImportedFoodAreaDefinition State(
        string postalCode,
        string fipsCode,
        string englishName,
        string koreanName)
        => new(postalCode, fipsCode, englishName, koreanName, true, false, false);

    private static UnitedStatesImportedFoodAreaDefinition District(
        string postalCode,
        string fipsCode,
        string englishName,
        string koreanName)
        => new(postalCode, fipsCode, englishName, koreanName, false, true, false);

    private static UnitedStatesImportedFoodAreaDefinition Territory(
        string postalCode,
        string fipsCode,
        string englishName,
        string koreanName)
        => new(postalCode, fipsCode, englishName, koreanName, false, false, true);

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
