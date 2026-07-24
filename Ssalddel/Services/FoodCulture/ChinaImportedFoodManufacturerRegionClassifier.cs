using System.Globalization;
using System.Text;

namespace Ssalddel.Services.FoodCulture;

public static class ChinaImportedFoodManufacturerRegionCodes
{
    public const string LiaoningLiaodong = "CN-LIAONING-LIAODONG";

    public const string Shandong = "CN-SHANDONG";

    public const string LowerYangtzeJiangnan = "CN-LOWER-YANGTZE-JIANGNAN";

    public const string OtherOrUnclassified = "CN-OTHER-UNCLASSIFIED";
}

public static class ChinaImportedFoodManufacturerRegionMethodCodes
{
    public const string OfficialAreaProvince = "OfficialAreaProvince";

    public const string OfficialAreaCity = "OfficialAreaCity";

    public const string FacilityAddressProvince = "FacilityAddressProvince";

    public const string FacilityAddressCity = "FacilityAddressCity";

    public const string CountryOnly = "CountryOnly";
}

public sealed record ImportedFoodManufacturerRegionClassification(
    string RegionCode,
    string RegionName,
    string RegionScope,
    string ClassificationMethodCode,
    string Evidence,
    decimal Confidence);

public static class ChinaImportedFoodManufacturerRegionClassifier
{
    private static readonly RegionRule[] Rules =
    [
        new(
            ChinaImportedFoodManufacturerRegionCodes.LiaoningLiaodong,
            "랴오닝성·랴오둥권",
            "랴오닝성 기준 운영 권역입니다. 랴오둥(요동)은 역사·지리 하위권역이므로 랴오닝성 전체의 상품 산지와 같은 뜻으로 확정하지 않습니다.",
            [
                Place("LIAONING", "랴오닝성"),
                Place("辽宁", "랴오닝성"),
                Place("遼寧", "랴오닝성"),
                Place("랴오닝", "랴오닝성"),
                Place("요녕", "랴오닝성")
            ],
            [
                Place("DALIAN", "다롄"),
                Place("大连", "다롄"),
                Place("大連", "다롄"),
                Place("다롄", "다롄"),
                Place("대련", "다롄"),
                Place("DANDONG", "단둥"),
                Place("丹东", "단둥"),
                Place("丹東", "단둥"),
                Place("단둥", "단둥"),
                Place("단동", "단둥"),
                Place("SHENYANG", "선양"),
                Place("沈阳", "선양"),
                Place("瀋陽", "선양"),
                Place("선양", "선양"),
                Place("심양", "선양"),
                Place("YINGKOU", "잉커우"),
                Place("营口", "잉커우"),
                Place("營口", "잉커우"),
                Place("PANJIN", "판진"),
                Place("盘锦", "판진"),
                Place("盤錦", "판진"),
                Place("ANSHAN", "안산"),
                Place("鞍山", "안산"),
                Place("LIAOYANG", "랴오양"),
                Place("辽阳", "랴오양"),
                Place("遼陽", "랴오양"),
                Place("LIAODONG", "랴오둥(요동)"),
                Place("辽东", "랴오둥(요동)"),
                Place("遼東", "랴오둥(요동)"),
                Place("랴오둥", "랴오둥(요동)"),
                Place("요동", "랴오둥(요동)")
            ]),
        new(
            ChinaImportedFoodManufacturerRegionCodes.Shandong,
            "산둥성",
            "산둥성 행정구역 기준 운영 권역입니다.",
            [
                Place("SHANDONG", "산둥성"),
                Place("山东", "산둥성"),
                Place("山東", "산둥성"),
                Place("산둥", "산둥성"),
                Place("산동", "산둥성")
            ],
            [
                Place("QINGDAO", "칭다오"),
                Place("青岛", "칭다오"),
                Place("靑島", "칭다오"),
                Place("칭다오", "칭다오"),
                Place("청도", "칭다오"),
                Place("JINAN", "지난"),
                Place("济南", "지난"),
                Place("濟南", "지난"),
                Place("YANTAI", "옌타이"),
                Place("烟台", "옌타이"),
                Place("煙臺", "옌타이"),
                Place("WEIHAI", "웨이하이"),
                Place("威海", "웨이하이"),
                Place("WEIFANG", "웨이팡"),
                Place("潍坊", "웨이팡"),
                Place("濰坊", "웨이팡"),
                Place("RIZHAO", "르자오"),
                Place("日照", "르자오"),
                Place("LINYI", "린이"),
                Place("临沂", "린이"),
                Place("臨沂", "린이"),
                Place("DONGYING", "둥잉"),
                Place("东营", "둥잉"),
                Place("東營", "둥잉"),
                Place("ZIBO", "쯔보"),
                Place("淄博", "쯔보")
            ]),
        new(
            ChinaImportedFoodManufacturerRegionCodes.LowerYangtzeJiangnan,
            "강남·장강하류권",
            "상하이시·장쑤성·저장성으로 한정한 운영 권역입니다. 역사·문화권인 강남 전체와 같은 뜻으로 사용하지 않습니다.",
            [
                Place("SHANGHAI", "상하이시"),
                Place("上海", "상하이시"),
                Place("상하이", "상하이시"),
                Place("상해", "상하이시"),
                Place("JIANGSU", "장쑤성"),
                Place("江苏", "장쑤성"),
                Place("江蘇", "장쑤성"),
                Place("장쑤", "장쑤성"),
                Place("강소", "장쑤성"),
                Place("ZHEJIANG", "저장성"),
                Place("浙江", "저장성"),
                Place("저장", "저장성"),
                Place("절강", "저장성")
            ],
            [
                Place("NANJING", "난징"),
                Place("南京", "난징"),
                Place("SUZHOU", "쑤저우"),
                Place("苏州", "쑤저우"),
                Place("蘇州", "쑤저우"),
                Place("WUXI", "우시"),
                Place("无锡", "우시"),
                Place("無錫", "우시"),
                Place("NANTONG", "난퉁"),
                Place("南通", "난퉁"),
                Place("HANGZHOU", "항저우"),
                Place("杭州", "항저우"),
                Place("NINGBO", "닝보"),
                Place("宁波", "닝보"),
                Place("寧波", "닝보"),
                Place("WENZHOU", "원저우"),
                Place("温州", "원저우"),
                Place("溫州", "원저우"),
                Place("JINHUA", "진화"),
                Place("金华", "진화"),
                Place("金華", "진화")
            ])
    ];

    public static ImportedFoodManufacturerRegionClassification? Classify(
        string? countryName,
        string? officialAreaName,
        string? facilityAddress)
    {
        if (!IsMainlandChina(countryName))
        {
            return null;
        }

        var area = Normalize(officialAreaName);
        var address = Normalize(facilityAddress);
        foreach (var rule in Rules)
        {
            var province = Match(rule.ProvincePlaces, area);
            if (province is not null)
            {
                return Result(
                    rule,
                    ChinaImportedFoodManufacturerRegionMethodCodes.OfficialAreaProvince,
                    $"식약처 해외제조업소 지역명에서 {province.DisplayName} 확인",
                    1.0000m);
            }

            var city = Match(rule.CityPlaces, area);
            if (city is not null)
            {
                return Result(
                    rule,
                    ChinaImportedFoodManufacturerRegionMethodCodes.OfficialAreaCity,
                    $"식약처 해외제조업소 지역명에서 {city.DisplayName} 확인",
                    0.9500m);
            }

            province = Match(rule.ProvincePlaces, address);
            if (province is not null)
            {
                return Result(
                    rule,
                    ChinaImportedFoodManufacturerRegionMethodCodes.FacilityAddressProvince,
                    $"식약처 해외제조업소 주소에서 {province.DisplayName} 확인",
                    0.9500m);
            }

            city = Match(rule.CityPlaces, address);
            if (city is not null)
            {
                return Result(
                    rule,
                    ChinaImportedFoodManufacturerRegionMethodCodes.FacilityAddressCity,
                    $"식약처 해외제조업소 주소에서 {city.DisplayName} 확인",
                    0.9000m);
            }
        }

        return new ImportedFoodManufacturerRegionClassification(
            ChinaImportedFoodManufacturerRegionCodes.OtherOrUnclassified,
            "중국 기타·미분류",
            "중국 제조업소로 확인되지만 세 운영 권역에 속한다는 공식 지역 근거를 확인하지 못한 항목입니다.",
            ChinaImportedFoodManufacturerRegionMethodCodes.CountryOnly,
            "제조국은 중국이나 세 권역을 판정할 공식 지역 근거가 없음",
            0.5000m);
    }

    private static ImportedFoodManufacturerRegionClassification Result(
        RegionRule rule,
        string methodCode,
        string evidence,
        decimal confidence)
        => new(
            rule.RegionCode,
            rule.RegionName,
            rule.RegionScope,
            methodCode,
            evidence,
            confidence);

    private static PlaceToken? Match(IReadOnlyList<PlaceToken> tokens, string source)
        => source.Length == 0
            ? null
            : tokens.FirstOrDefault(token => source.Contains(token.NormalizedToken, StringComparison.Ordinal));

    private static bool IsMainlandChina(string? countryName)
    {
        var normalized = Normalize(countryName);
        if (normalized.Contains("TAIWAN", StringComparison.Ordinal)
            || normalized.Contains("대만", StringComparison.Ordinal)
            || normalized.Contains("臺灣", StringComparison.Ordinal)
            || normalized.Contains("台湾", StringComparison.Ordinal))
        {
            return false;
        }

        return normalized is "CN" or "CHN"
               || normalized.Contains("중국", StringComparison.Ordinal)
               || normalized.Contains("CHINA", StringComparison.Ordinal)
               || normalized.Contains("中國", StringComparison.Ordinal)
               || normalized.Contains("中国", StringComparison.Ordinal)
               || normalized.Contains("中华人民共和国", StringComparison.Ordinal)
               || normalized.Contains("中華人民共和國", StringComparison.Ordinal);
    }

    private static PlaceToken Place(string token, string displayName)
        => new(Normalize(token), displayName);

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormKC).ToUpper(CultureInfo.InvariantCulture);
        return string.Concat(normalized.Where(char.IsLetterOrDigit));
    }

    private sealed record RegionRule(
        string RegionCode,
        string RegionName,
        string RegionScope,
        IReadOnlyList<PlaceToken> ProvincePlaces,
        IReadOnlyList<PlaceToken> CityPlaces);

    private sealed record PlaceToken(string NormalizedToken, string DisplayName);
}
