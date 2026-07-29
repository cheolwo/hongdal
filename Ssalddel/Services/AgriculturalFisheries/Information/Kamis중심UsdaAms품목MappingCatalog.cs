using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal sealed record Kamis중심UsdaAms품목MappingDefinition(
    string KamisItemCode,
    string MatchQualityCode,
    IReadOnlyList<string> ExactCommodityNames,
    IReadOnlyList<string> CommodityPrefixes,
    string ReviewNote);

internal sealed record Kamis중심UsdaAms품목MappingResolution(
    string MappingStatusCode,
    string MatchQualityCode,
    string MatchQualityLabel,
    IReadOnlyList<string> MatchedCommodities,
    string ReviewNote);

internal static class Kamis중심UsdaAms품목MappingCatalog
{
    private static readonly IReadOnlyList<string> HotPepperNames =
    [
        "Anaheim Pepper",
        "Jalapeno Peppers",
        "Poblano Pepper",
        "Serrano Peppers",
        "Peppers, Aji Dulce",
        "Peppers, Ajie",
        "Peppers, Anaheim",
        "Peppers, Cherry Hot",
        "Peppers, Chilaca",
        "Peppers, Finger Hot",
        "Peppers, Fresno",
        "Peppers, Habanero",
        "Peppers, Hungarian Wax",
        "Peppers, Jalapeno",
        "Peppers, Long Hot",
        "Peppers, Manzano",
        "Peppers, Pasilla",
        "Peppers, Poblano",
        "Peppers, Scotch Bonnet",
        "Peppers, Serrano",
        "Peppers, Thai Chili Hots",
        "Peppers, Yellow Chile"
    ];

    private static readonly IReadOnlyDictionary<string, Kamis중심UsdaAms품목MappingDefinition>
        Definitions = new[]
        {
            Direct("151", ["Sweet Potatoes"], "고구마 공통 품목 후보입니다."),
            Direct("152", ["Potatoes"], "식용 감자 공통 품목 후보이며 종서용 감자는 제외합니다."),
            Broad("211", ["Chinese Cabbage"], "배추와 Chinese Cabbage의 세부 품종을 확인해야 합니다."),
            Direct("212", ["Cabbage"], "양배추 공통 품목 후보입니다."),
            Direct("213", ["Spinach"], "시금치 공통 품목 후보입니다."),
            Broad("214", ["Lettuce"], "미국 상추 품종군과 국내 상추 품종을 구분해야 합니다.", ["Lettuce,"]),
            Broad("215", ["Chinese Cabbage"], "얼갈이배추와 Chinese Cabbage의 생육 단계·품종을 확인해야 합니다."),
            Broad("216", ["Gai Choy (Chinese Mustard)", "Greens, Mustard", "Mustard"], "갓과 미국 mustard greens 계열의 품종을 확인해야 합니다."),
            Direct("221", ["Watermelons"], "수박 공통 품목 후보입니다."),
            Direct("222", ["Melon, Korean"], "참외와 Korean melon 공통 품목 후보입니다."),
            Direct("223", ["Cucumbers"], "오이 공통 품목 후보입니다."),
            Broad("224", ["Squash"], "호박류 안에서 애호박·단호박·주키니 등 품종을 구분해야 합니다.", ["Squash,"]),
            Direct("225", ["Tomatoes"], "일반 토마토 공통 품목 후보입니다."),
            Direct("226", ["Strawberries"], "딸기 공통 품목 후보입니다."),
            Broad("231", ["Daikon", "Lo Bok", "Radishes"], "한국 무와 미국 무류의 품종·크기를 확인해야 합니다."),
            Direct("232", ["Carrots"], "당근 공통 품목 후보입니다."),
            Broad("233", ["Radishes"], "열무와 일반 radish는 수확 단계와 용도가 달라 검토가 필요합니다."),
            Broad("242", HotPepperNames, "풋고추는 여러 미국 생고추 품종군 후보와 색·매운맛·크기를 확인해야 합니다."),
            Broad("243", HotPepperNames, "붉은고추는 여러 미국 생고추 품종군 후보와 건조 여부를 확인해야 합니다."),
            Direct("244", ["Garlic"], "피마늘과 garlic 공통 품목 후보입니다."),
            Direct("245", ["Onions, Dry"], "일반 양파와 dry onion 공통 품목 후보입니다."),
            Direct("246", ["Onions, Green"], "파와 green onion 공통 품목 후보입니다."),
            Direct("247", ["Ginger Root"], "생강 공통 품목 후보입니다."),
            Direct("255", ["Peppers (Bell Type)", "Peppers, Bell Type"], "피망과 bell pepper 공통 품목 후보입니다."),
            Broad("256", ["Peppers (Bell Type)", "Peppers, Bell Type"], "파프리카와 미국 bell pepper의 색·품종·크기를 확인해야 합니다."),
            Broad(
                "257",
                ["Cantaloupes", "Honeydews"],
                "멜론류 안에서 품종을 먼저 맞춰야 합니다.",
                ["Melon,"]),
            Broad("258", ["Garlic"], "깐마늘과 원물 garlic은 가공·손질 상태가 달라 직접 비교하지 않습니다."),
            Broad("279", ["Chinese Cabbage"], "알배기배추와 Chinese Cabbage의 크기·생육 단계를 확인해야 합니다."),
            Direct("280", ["Broccoli"], "브로콜리 공통 품목 후보입니다."),
            Direct("314", ["Peanuts"], "땅콩 공통 품목 후보입니다."),
            Broad("315", ["Mushrooms"], "느타리버섯은 USDA mushroom 품종 표기를 추가로 확인해야 합니다."),
            Broad("316", ["Mushrooms"], "팽이버섯은 USDA mushroom 품종 표기를 추가로 확인해야 합니다."),
            Broad("317", ["Mushrooms"], "새송이버섯은 USDA mushroom 품종 표기를 추가로 확인해야 합니다."),
            Direct("318", ["Walnuts"], "호두 공통 품목 후보입니다."),
            Direct("319", ["Almonds"], "아몬드 공통 품목 후보입니다."),
            Direct("411", ["Apples"], "사과 공통 품목 후보입니다."),
            Broad("412", ["Apple Pears", "Pears"], "한국 배와 미국 pear 계열의 품종을 확인해야 합니다."),
            Direct("413", ["Peaches"], "복숭아 공통 품목 후보입니다."),
            Direct("414", ["Grapes"], "포도 공통 품목 후보입니다."),
            Broad(
                "415",
                ["Clementines", "Satsuma", "Satsumas", "Tangerines", "Tangerines/Mandarins"],
                "감귤과 미국 mandarin·satsuma 계열의 품종을 확인해야 합니다."),
            Direct("416", ["Persimmons"], "단감과 persimmon 공통 품목 후보입니다."),
            Direct("418", ["Bananas"], "바나나 공통 품목 후보입니다."),
            Direct("419", ["Kiwifruit"], "참다래와 kiwifruit 공통 품목 후보입니다."),
            Direct("420", ["Pineapples"], "파인애플 공통 품목 후보입니다."),
            Direct("421", ["Oranges"], "오렌지 공통 품목 후보입니다."),
            Direct("422", ["Tomatoes, Cherry"], "방울토마토와 cherry tomato 공통 품목 후보입니다."),
            Direct("424", ["Lemons"], "레몬 공통 품목 후보이며 Meyer lemon은 별도 품종으로 둡니다."),
            Direct("425", ["Cherries"], "체리 공통 품목 후보입니다."),
            Direct("428", ["Mangoes"], "망고 공통 품목 후보입니다."),
            Direct("430", ["Avocados"], "아보카도 공통 품목 후보입니다.")
        }.ToDictionary(item => item.KamisItemCode, StringComparer.Ordinal);

    public static Kamis중심UsdaAms품목MappingResolution Resolve(
        string kamisItemCode,
        IReadOnlyCollection<string> availableAmsCommodities)
    {
        if (!Definitions.TryGetValue(kamisItemCode, out var definition))
        {
            return NoCandidate(
                "현재 USDA AMS 전문청과 보고서에서 안전하게 연결할 품목 후보가 없습니다.");
        }

        var available = availableAmsCommodities.ToHashSet(
            StringComparer.OrdinalIgnoreCase);
        var resolved = definition.ExactCommodityNames
            .Where(available.Contains)
            .Concat(available.Where(commodity =>
                definition.CommodityPrefixes.Any(prefix =>
                    commodity.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(commodity => commodity, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (resolved.Length == 0)
        {
            return NoCandidate(
                "매핑 규칙은 있으나 선택 연도의 USDA AMS 관측에서 해당 품목을 찾지 못했습니다.");
        }

        return new Kamis중심UsdaAms품목MappingResolution(
            Kamis중심UsdaAms매핑상태Codes.후보있음,
            definition.MatchQualityCode,
            definition.MatchQualityCode == Kamis중심UsdaAms매핑품질Codes.동일품목후보
                ? "동일 품목 후보"
                : "넓은 품목군 후보",
            resolved,
            $"{definition.ReviewNote} 시장 단계·날짜·품종·등급·원산지·거래단위와 환율을 맞추기 전에는 차액을 계산하지 않습니다.");
    }

    private static Kamis중심UsdaAms품목MappingResolution NoCandidate(string note)
        => new(
            Kamis중심UsdaAms매핑상태Codes.후보없음,
            Kamis중심UsdaAms매핑품질Codes.후보없음,
            "후보 없음",
            [],
            note);

    private static Kamis중심UsdaAms품목MappingDefinition Direct(
        string kamisItemCode,
        IReadOnlyList<string> exactCommodityNames,
        string note,
        IReadOnlyList<string>? prefixes = null)
        => new(
            kamisItemCode,
            Kamis중심UsdaAms매핑품질Codes.동일품목후보,
            exactCommodityNames,
            prefixes ?? [],
            note);

    private static Kamis중심UsdaAms품목MappingDefinition Broad(
        string kamisItemCode,
        IReadOnlyList<string> exactCommodityNames,
        string note,
        IReadOnlyList<string>? prefixes = null)
        => new(
            kamisItemCode,
            Kamis중심UsdaAms매핑품질Codes.광의품목후보,
            exactCommodityNames,
            prefixes ?? [],
            note);
}
