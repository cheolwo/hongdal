using System.Globalization;
using System.Text;

namespace Ssalddel.Services.FoodCulture;

public static class JapanImportedFoodManufacturerPrefectureRegionCodes
{
    public const string Prefix = "JP-";

    public const string OtherOrUnclassified = "JP-OTHER-UNCLASSIFIED";

    public static string Prefecture(string jisCode)
        => $"{Prefix}{jisCode.Trim()}";
}

public static class JapanImportedFoodManufacturerPrefectureMethodCodes
{
    public const string OfficialAreaName = "OfficialAreaName";

    public const string FacilityAddressPrefectureName = "FacilityAddressPrefectureName";

    public const string CountryOnly = "CountryOnly";
}

public sealed record JapanPrefectureDefinition(
    string JisCode,
    string EnglishName,
    string JapaneseName,
    string KoreanName)
{
    public string RegionCode =>
        JapanImportedFoodManufacturerPrefectureRegionCodes.Prefecture(JisCode);
}

public static class JapanImportedFoodManufacturerPrefectureClassifier
{
    private static readonly JapanPrefectureDefinition[] Prefectures =
    [
        Prefecture("01", "Hokkaido", "北海道", "홋카이도"),
        Prefecture("02", "Aomori", "青森県", "아오모리현"),
        Prefecture("03", "Iwate", "岩手県", "이와테현"),
        Prefecture("04", "Miyagi", "宮城県", "미야기현"),
        Prefecture("05", "Akita", "秋田県", "아키타현"),
        Prefecture("06", "Yamagata", "山形県", "야마가타현"),
        Prefecture("07", "Fukushima", "福島県", "후쿠시마현"),
        Prefecture("08", "Ibaraki", "茨城県", "이바라키현"),
        Prefecture("09", "Tochigi", "栃木県", "도치기현"),
        Prefecture("10", "Gunma", "群馬県", "군마현"),
        Prefecture("11", "Saitama", "埼玉県", "사이타마현"),
        Prefecture("12", "Chiba", "千葉県", "지바현"),
        Prefecture("13", "Tokyo", "東京都", "도쿄도"),
        Prefecture("14", "Kanagawa", "神奈川県", "가나가와현"),
        Prefecture("15", "Niigata", "新潟県", "니가타현"),
        Prefecture("16", "Toyama", "富山県", "도야마현"),
        Prefecture("17", "Ishikawa", "石川県", "이시카와현"),
        Prefecture("18", "Fukui", "福井県", "후쿠이현"),
        Prefecture("19", "Yamanashi", "山梨県", "야마나시현"),
        Prefecture("20", "Nagano", "長野県", "나가노현"),
        Prefecture("21", "Gifu", "岐阜県", "기후현"),
        Prefecture("22", "Shizuoka", "静岡県", "시즈오카현"),
        Prefecture("23", "Aichi", "愛知県", "아이치현"),
        Prefecture("24", "Mie", "三重県", "미에현"),
        Prefecture("25", "Shiga", "滋賀県", "시가현"),
        Prefecture("26", "Kyoto", "京都府", "교토부"),
        Prefecture("27", "Osaka", "大阪府", "오사카부"),
        Prefecture("28", "Hyogo", "兵庫県", "효고현"),
        Prefecture("29", "Nara", "奈良県", "나라현"),
        Prefecture("30", "Wakayama", "和歌山県", "와카야마현"),
        Prefecture("31", "Tottori", "鳥取県", "돗토리현"),
        Prefecture("32", "Shimane", "島根県", "시마네현"),
        Prefecture("33", "Okayama", "岡山県", "오카야마현"),
        Prefecture("34", "Hiroshima", "広島県", "히로시마현"),
        Prefecture("35", "Yamaguchi", "山口県", "야마구치현"),
        Prefecture("36", "Tokushima", "徳島県", "도쿠시마현"),
        Prefecture("37", "Kagawa", "香川県", "가가와현"),
        Prefecture("38", "Ehime", "愛媛県", "에히메현"),
        Prefecture("39", "Kochi", "高知県", "고치현"),
        Prefecture("40", "Fukuoka", "福岡県", "후쿠오카현"),
        Prefecture("41", "Saga", "佐賀県", "사가현"),
        Prefecture("42", "Nagasaki", "長崎県", "나가사키현"),
        Prefecture("43", "Kumamoto", "熊本県", "구마모토현"),
        Prefecture("44", "Oita", "大分県", "오이타현"),
        Prefecture("45", "Miyazaki", "宮崎県", "미야자키현"),
        Prefecture("46", "Kagoshima", "鹿児島県", "가고시마현"),
        Prefecture("47", "Okinawa", "沖縄県", "오키나와현")
    ];

    public static IReadOnlyList<JapanPrefectureDefinition> Definitions =>
        Prefectures;

    public static ImportedFoodManufacturerRegionClassification? Classify(
        string? countryName,
        string? officialAreaName,
        string? facilityAddress)
    {
        if (!IsJapan(countryName))
        {
            return null;
        }

        var normalizedArea = Normalize(officialAreaName);
        foreach (var prefecture in Prefectures)
        {
            if (IsExactName(normalizedArea, prefecture))
            {
                return Result(
                    prefecture,
                    JapanImportedFoodManufacturerPrefectureMethodCodes.OfficialAreaName,
                    $"식약처 해외제조업소 지역명에서 {prefecture.KoreanName} 확인",
                    1.0000m);
            }
        }

        var normalizedAddress = Normalize(facilityAddress);
        foreach (var prefecture in Prefectures
                     .OrderByDescending(item => item.EnglishName.Length))
        {
            if (ContainsName(normalizedAddress, prefecture))
            {
                return Result(
                    prefecture,
                    JapanImportedFoodManufacturerPrefectureMethodCodes
                        .FacilityAddressPrefectureName,
                    $"식약처 해외제조업소 주소에서 {prefecture.KoreanName} 확인",
                    0.9500m);
            }
        }

        return new ImportedFoodManufacturerRegionClassification(
            JapanImportedFoodManufacturerPrefectureRegionCodes.OtherOrUnclassified,
            "일본 기타·미분류",
            "제품 국가가 일본으로 확인되지만 47개 도도부현을 판정할 공식 지역 근거가 없는 항목입니다.",
            JapanImportedFoodManufacturerPrefectureMethodCodes.CountryOnly,
            "제품 국가는 일본이나 도도부현을 판정할 공식 지역 근거가 없음",
            0.5000m);
    }

    public static JapanPrefectureDefinition? FindByRegionCode(string regionCode)
        => Prefectures.FirstOrDefault(item => string.Equals(
            item.RegionCode,
            regionCode,
            StringComparison.Ordinal));

    private static bool IsExactName(
        string normalizedValue,
        JapanPrefectureDefinition prefecture)
        => normalizedValue == Normalize(prefecture.EnglishName)
           || normalizedValue == Normalize(prefecture.JapaneseName)
           || normalizedValue == Normalize(prefecture.KoreanName)
           || normalizedValue == Normalize($"{prefecture.EnglishName} Prefecture");

    private static bool ContainsName(
        string normalizedValue,
        JapanPrefectureDefinition prefecture)
        => normalizedValue.Contains(
               Normalize(prefecture.JapaneseName),
               StringComparison.Ordinal)
           || normalizedValue.Contains(
               Normalize(prefecture.KoreanName),
               StringComparison.Ordinal)
           || ContainsDelimitedEnglishName(
               normalizedValue,
               Normalize(prefecture.EnglishName));

    private static bool ContainsDelimitedEnglishName(string source, string name)
        => source.Length > 0
           && name.Length > 0
           && $" {source} ".Contains($" {name} ", StringComparison.Ordinal);

    private static ImportedFoodManufacturerRegionClassification Result(
        JapanPrefectureDefinition prefecture,
        string methodCode,
        string evidence,
        decimal confidence)
        => new(
            prefecture.RegionCode,
            prefecture.KoreanName,
            $"일본 47개 도도부현 중 {prefecture.KoreanName} 소재 해외제조업소를 뜻합니다. " +
            $"JIS/ISO 숫자 코드 {prefecture.JisCode}. " +
            "원재료 생산·재배·어획지, GI 보호지역이나 통관항을 확정하지 않습니다.",
            methodCode,
            evidence,
            confidence);

    private static bool IsJapan(string? countryName)
    {
        var normalized = Normalize(countryName);
        return normalized is "JP" or "JPN" or "JAPAN" or "일본" or "日本"
               || normalized.Contains("JAPAN", StringComparison.Ordinal)
               || normalized.Contains("일본", StringComparison.Ordinal)
               || normalized.Contains("日本", StringComparison.Ordinal);
    }

    private static JapanPrefectureDefinition Prefecture(
        string jisCode,
        string englishName,
        string japaneseName,
        string koreanName)
        => new(jisCode, englishName, japaneseName, koreanName);

    private static string Normalize(string? value)
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
}
