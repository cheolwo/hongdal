using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Services.FoodCulture;

internal sealed record ParsedOfficialFoodRecipeIngredient(
    string CanonicalName,
    string NormalizedName,
    string CategoryCode,
    string ClassificationMethod,
    decimal ClassificationConfidence,
    string ClassificationState,
    string GroupName,
    string OriginalText,
    string SourceName,
    string QuantityText,
    decimal? QuantityValue,
    decimal? QuantityMaxValue,
    string UnitCode,
    string UnitText,
    string HouseholdMeasureText,
    string PreparationNote,
    int DisplayOrder,
    decimal ParseConfidence,
    bool RequiresReview);

internal sealed partial class OfficialFoodRecipeIngredientParser
{
    public const string ParserVersion = "ingredient-rules-v1";
    public const string ClassificationMethod = "keyword-rules-v1";

    private static readonly HashSet<string> KnownGroupNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "주재료", "부재료", "재료", "양념", "양념장", "소스", "드레싱", "육수", "고명", "장식",
        "곁들임", "채소준비", "채소 준비", "반죽", "토핑", "샐러드", "조림장", "초절임장",
        "main ingredients", "ingredients", "seasoning", "sauce", "dressing", "stock", "garnish",
        "for the sauce", "for the dressing", "to serve"
    };

    private static readonly HashSet<string> TokenOnlyKoreanKeywords = new(StringComparer.Ordinal)
    {
        "물", "파", "마", "게", "조"
    };

    private static readonly IReadOnlyDictionary<char, decimal> UnicodeFractions =
        new Dictionary<char, decimal>
        {
            ['¼'] = 0.25m,
            ['½'] = 0.5m,
            ['¾'] = 0.75m,
            ['⅐'] = 1m / 7m,
            ['⅑'] = 1m / 9m,
            ['⅒'] = 0.1m,
            ['⅓'] = 1m / 3m,
            ['⅔'] = 2m / 3m,
            ['⅕'] = 0.2m,
            ['⅖'] = 0.4m,
            ['⅗'] = 0.6m,
            ['⅘'] = 0.8m,
            ['⅙'] = 1m / 6m,
            ['⅚'] = 5m / 6m,
            ['⅛'] = 0.125m,
            ['⅜'] = 0.375m,
            ['⅝'] = 0.625m,
            ['⅞'] = 0.875m
        };

    private static readonly (string CategoryCode, decimal Confidence, string[] Keywords)[] CategoryRules =
    [
        (OfficialFoodIngredientCategoryCodes.WaterAndStock, 0.98m,
            ["물", "생수", "쌀뜨물", "육수", "채수", "탄산수", "얼음", "broth", "stock", "water"]),
        (OfficialFoodIngredientCategoryCodes.SauceAndFermented, 0.95m,
            ["간장", "된장", "고추장", "쌈장", "청국장", "액젓", "새우젓", "식초", "마요네즈",
                "머스터드", "머스타드", "겨자", "드레싱", "소스", "케첩", "케찹", "발사믹", "요리당", "올리고당", "물엿", "조청", "시럽",
                "젓갈", "두반장", "홍초", "그린커리",
                "soy sauce", "vinegar", "mayonnaise", "mustard", "dressing", "sauce"]),
        (OfficialFoodIngredientCategoryCodes.OilAndFat, 0.98m,
            ["식용유", "올리브유", "올리브오일", "참기름", "들기름", "포도씨유", "카놀라유", "오일", "기름", "버터", "마가린",
                "olive oil", "sesame oil", "vegetable oil", "butter", "margarine"]),
        (OfficialFoodIngredientCategoryCodes.ProcessedFood, 0.92m,
            ["김치", "묵은지", "깍두기", "피클", "햄", "소시지", "베이컨", "스팸", "어묵", "맛살", "크래미", "통조림", "만두피", "젤라틴", "초콜릿",
                "pickle", "sausage", "bacon", "ham", "kimchi"]),
        (OfficialFoodIngredientCategoryCodes.SeasoningAndSpice, 0.94m,
            ["소금", "설탕", "후추", "후춧가루", "고춧가루", "카레가루", "생강", "꿀", "향신", "바닐라",
                "계피", "바질", "로즈마리", "로즈메리", "오레가노", "파슬리", "타임", "민트", "월계수", "와사비",
                "강황", "치자", "이스트", "베이킹파우더", "허브", "차이브", "천일염", "스테비아",
                "알룰로스", "타가토스", "넛맥", "가람마살라", "커민", "정향",
                "salt", "sugar", "pepper", "spice", "honey", "ginger"]),
        (OfficialFoodIngredientCategoryCodes.LegumeAndSoy, 0.96m,
            ["두부", "순두부", "연두부", "유부", "콩", "서리태", "두유", "완두", "렌틸", "병아리콩", "팥", "낫토", "대두", "녹두", "그린빈",
                "tofu", "soy", "bean", "beans", "lentil", "lentils", "chickpea", "chickpeas"]),
        (OfficialFoodIngredientCategoryCodes.Mushroom, 0.99m,
            ["버섯", "송이", "표고", "느타리", "팽이", "mushroom", "mushrooms"]),
        (OfficialFoodIngredientCategoryCodes.Seaweed, 0.98m,
            ["미역", "다시마", "김", "톳", "파래", "매생이", "감태", "곰피", "해초", "seaweed", "kelp", "nori"]),
        (OfficialFoodIngredientCategoryCodes.PoultryAndEgg, 0.97m,
            ["닭", "오리", "달걀", "계란", "메추리알", "노른자", "칠면조", "chicken", "duck", "egg", "eggs", "turkey"]),
        (OfficialFoodIngredientCategoryCodes.Seafood, 0.95m,
            ["새우", "멸치", "대구", "명태", "황태", "북어", "조개", "굴", "홍합", "오징어", "문어",
                "낙지", "주꾸미", "게", "연어", "고등어", "삼치", "참치", "바지락", "전복", "장어", "코다리",
                "도미", "소라", "관자", "가다랑어", "가쓰오", "가쯔오", "가츠오", "날치알", "가자미", "광어", "골뱅이",
                "꼬막", "대하", "동태", "조기", "패주", "갈치", "과메기", "꽁치", "우렁", "다슬기", "생선", "어패류", "shrimp", "prawn", "fish",
                "salmon", "tuna", "cod", "squid", "clam", "mussel", "crab"]),
        (OfficialFoodIngredientCategoryCodes.Dairy, 0.96m,
            ["우유", "생크림", "사워크림", "휘핑크림", "요거트", "요구르트", "치즈", "연유", "milk", "cream", "yogurt", "yoghurt", "cheese"]),
        (OfficialFoodIngredientCategoryCodes.NutAndSeed, 0.95m,
            ["땅콩", "아몬드", "호두", "잣", "밤", "은행", "캐슈", "피스타치오", "참깨", "통깨", "검은깨", "검정깨", "흑임자", "견과류", "깨", "해바라기씨",
                "호박씨", "들깨", "들깻", "nut", "nuts", "almond", "walnut", "peanut", "sesame", "seed", "seeds"]),
        (OfficialFoodIngredientCategoryCodes.Fruit, 0.92m,
            ["사과", "배", "딸기", "블루베리", "오렌지", "레몬", "유자", "석류", "매실", "포도", "복숭아",
                "바나나", "파인애플", "망고", "키위", "수박", "참외", "건포도", "크랜베리", "대추", "곶감",
                "체리", "멜론", "아보카도", "오미자", "홍시", "단감", "자두", "라임", "라즈베리", "자몽",
                "귤", "감귤", "코코넛", "백년초", "크렌베리", "과일",
                "apple", "pear", "strawberry", "blueberry", "orange", "lemon", "grape", "banana", "mango", "fruit"]),
        (OfficialFoodIngredientCategoryCodes.Meat, 0.95m,
            ["돼지", "소고기", "쇠고기", "우민찌", "양고기", "사골", "갈비", "삼겹", "목살", "소불고기", "차돌박이",
                "pork", "beef", "lamb", "veal"]),
        (OfficialFoodIngredientCategoryCodes.GrainAndStarch, 0.92m,
            ["쌀", "밥", "백미", "흑미", "현미", "보리", "귀리", "밀가루", "강력분", "중력분", "박력분", "부침가루", "튀김가루", "전분", "녹말", "미숫가루", "소면", "국수",
                "면", "떡", "빵", "카스텔라", "오트", "퀴노아", "곤약", "누룽지", "라이스페이퍼", "또띠아", "춘권피",
                "청포묵", "도토리묵", "한천", "펜네", "푸실리", "수수", "조", "잡곡", "르뱅", "파스타", "시리얼", "rice", "wheat", "flour", "noodle",
                "noodles", "pasta", "bread", "oat", "oats", "starch"]),
        (OfficialFoodIngredientCategoryCodes.Vegetable, 0.88m,
            ["배추", "양배추", "시금치", "부추", "치커리", "상추", "청경채", "깻잎", "미나리", "냉이",
                "돌나물", "콩나물", "숙주", "브로콜리", "브로컬리", "컬리플라워", "콜리플라워", "양파", "대파", "쪽파", "실파", "파", "마늘",
                "감자", "고구마", "당근", "무", "오이", "호박", "가지", "토마토", "파프리카", "피망",
                "고추", "옥수수", "샐러리", "셀러리", "아스파라거스", "쑥갓", "연근", "고사리", "더덕", "마", "우엉",
                "비트", "어린잎", "새싹", "적채", "래디쉬", "레디쉬", "달래", "콜라비", "함초", "인삼", "수삼",
                "도라지", "식용꽃", "식용 꽃", "케일", "두릅", "레드어니언", "로메인", "쑥", "아욱", "근대",
                "시래기", "당귀잎", "영콘", "워터크레스", "할라피뇨", "고수", "블랙올리브", "죽순", "채소", "cabbage", "spinach", "lettuce", "onion",
                "garlic", "potato", "carrot", "radish", "cucumber", "pumpkin", "courgette", "zucchini",
                "aubergine", "eggplant", "tomato", "pepper", "broccoli", "vegetable"]),
        (OfficialFoodIngredientCategoryCodes.BeverageAndAlcohol, 0.90m,
            ["와인", "청주", "정종", "맛술", "미림", "미향", "요리술", "조미술", "막걸리", "맥주", "소주",
                "사이다", "식혜", "녹차", "럼", "브랜디", "음료", "wine", "beer", "rum", "brandy"])
    ];

    private static readonly string[] UnitTerms =
    [
        "tablespoons", "tablespoon", "teaspoons", "teaspoon", "kilograms", "kilogram", "millilitres",
        "millilitre", "milliliters", "milliliter", "grams", "gram", "litres", "litre", "liters", "liter",
        "ounces", "ounce", "pounds", "pound", "tbsp", "tsp", "cups", "cup", "kg", "mg", "ml", "oz", "lb",
        "큰술", "작은술", "스푼", "봉지", "줄기", "가닥", "송이", "조각", "마리", "팩", "캔", "컵",
        "개", "장", "알", "모", "쪽", "단", "줌", "대", "술", "ts", "t", "cc", "㎏", "㎎", "㎖", "g", "l"
    ];

    private static readonly Regex QuantityPattern = new(
        $"^(?<name>.+?)\\s+(?<quantity>(?:\\d+\\s+\\d+\\s*[/⁄]\\s*\\d+|\\d+\\s*[/⁄]\\s*\\d+|\\d+(?:[.]\\d+)?[¼½¾⅐⅑⅒⅓⅔⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞]?|[¼½¾⅐⅑⅒⅓⅔⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞])(?:\\s*[~∼–-]\\s*(?:\\d+\\s*[/⁄]\\s*\\d+|\\d+(?:[.]\\d+)?[¼½¾⅐⅑⅒⅓⅔⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞]?))?|약간|조금|적당량|to taste|as needed)\\s*(?<unit>{string.Join('|', UnitTerms.Select(Regex.Escape))})?(?<tail>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex ParentheticalQuantityPattern = new(
        $"^(?<name>.+?)\\s*\\(\\s*(?<quantity>(?:\\d+\\s+\\d+\\s*[/⁄]\\s*\\d+|\\d+\\s*[/⁄]\\s*\\d+|\\d+(?:[.]\\d+)?[¼½¾⅐⅑⅒⅓⅔⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞]?|[¼½¾⅐⅑⅒⅓⅔⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞])(?:\\s*[~∼–-]\\s*(?:\\d+\\s*[/⁄]\\s*\\d+|\\d+(?:[.]\\d+)?[¼½¾⅐⅑⅒⅓⅔⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞]?))?|약간|조금|적당량|to taste|as needed)\\s*(?<unit>{string.Join('|', UnitTerms.Select(Regex.Escape))})?(?<tail>.*?)(?:\\)*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex CompactNumericQuantityPattern = new(
        $"^(?<name>.*?\\p{{L}})(?<quantity>(?:\\d+\\s*[/⁄]\\s*\\d+|\\d+(?:[.]\\d+)?[¼½¾⅐⅑⅒⅓⅔⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞]?))\\s*(?<unit>{string.Join('|', UnitTerms.Select(Regex.Escape))})(?<tail>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex CompactApproximateQuantityPattern = new(
        @"^(?<name>.+?)(?<quantity>약간|조금|적당량|to taste|as needed)(?<unit>)(?<tail>)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex LeadingGroupPattern = new(
        @"^\((?<group>[^)]{1,40}(?:재료|양념|소스|고명|육수|반죽|토핑))\)\s*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex BracketGroupPattern = new(
        @"^\[(?<group>[^]]{1,40})\]\s*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex HtmlTagPattern = new(
        @"<[^>]+>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex QuantityOnlyPattern = new(
        @"^(?:\d+(?:[.]\d+)?|\d+\s*[/⁄]\s*\d+)\s*(?:kg|mg|ml|g|l|cc|㎏|㎎|㎖|개|장|알|모|컵|큰술|작은술)\)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex ServingPrefixPattern = new(
        @"^\s*\[[^\]]*(?:인분|serving)[^\]]*\]\s*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex WhitespacePattern = new(
        @"\s+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<ParsedOfficialFoodRecipeIngredient> Parse(
        string languageCode,
        IReadOnlyList<string> ingredientTexts)
    {
        ArgumentNullException.ThrowIfNull(ingredientTexts);
        var logicalLines = BuildLogicalLines(ingredientTexts);
        var parsed = new List<ParsedOfficialFoodRecipeIngredient>();
        var groupName = string.Empty;

        for (var lineIndex = 0; lineIndex < logicalLines.Count; lineIndex++)
        {
            var line = CleanDecorations(logicalLines[lineIndex]);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            line = ServingPrefixPattern.Replace(line, string.Empty).Trim();
            if (TryExtractHeading(line, out var heading, out var remainder))
            {
                groupName = heading;
                line = remainder;
            }
            else if (IsStandaloneHeading(logicalLines, lineIndex, line))
            {
                groupName = CleanGroupName(line);
                continue;
            }

            foreach (var segment in SplitTopLevel(line))
            {
                var item = ParseSegment(
                    languageCode,
                    groupName,
                    segment,
                    parsed.Count + 1);
                if (item is not null)
                {
                    parsed.Add(item);
                }
            }
        }

        return parsed;
    }

    public static string NormalizeName(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormKC).Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        var previousWasSpace = false;
        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSpace = false;
            }
            else if (!previousWasSpace)
            {
                builder.Append(' ');
                previousWasSpace = true;
            }
        }

        return WhitespacePattern.Replace(builder.ToString(), " ").Trim();
    }

    private static ParsedOfficialFoodRecipeIngredient? ParseSegment(
        string languageCode,
        string groupName,
        string rawSegment,
        int displayOrder)
    {
        var originalText = CleanDecorations(rawSegment).Trim().TrimEnd(',', ';');
        if (string.IsNullOrWhiteSpace(originalText)
            || KnownGroupNames.Contains(CleanGroupName(originalText)))
        {
            return null;
        }

        var parsingText = HtmlTagPattern.Replace(originalText, " ").Trim();
        var bracketGroup = BracketGroupPattern.Match(parsingText);
        if (bracketGroup.Success
            && !bracketGroup.Groups["group"].Value.Contains("인분", StringComparison.Ordinal))
        {
            groupName = CleanGroupName(bracketGroup.Groups["group"].Value);
            parsingText = parsingText[bracketGroup.Length..].Trim();
        }

        var leadingGroup = LeadingGroupPattern.Match(parsingText);
        if (leadingGroup.Success)
        {
            groupName = CleanGroupName(leadingGroup.Groups["group"].Value);
            parsingText = parsingText[leadingGroup.Length..].Trim();
        }

        if (QuantityOnlyPattern.IsMatch(parsingText))
        {
            return null;
        }

        var sourceName = parsingText;
        var quantityText = string.Empty;
        decimal? quantityValue = null;
        decimal? quantityMaxValue = null;
        var unitText = string.Empty;
        var unitCode = string.Empty;
        var householdMeasureText = string.Empty;
        var preparationNote = string.Empty;
        var parseConfidence = 0.45m;
        var requiresReview = true;

        var match = QuantityPattern.Match(parsingText);
        var parentheticalPrimaryQuantity = false;
        if (!match.Success)
        {
            match = ParentheticalQuantityPattern.Match(parsingText);
            parentheticalPrimaryQuantity = match.Success;
        }

        if (!match.Success)
        {
            match = CompactNumericQuantityPattern.Match(parsingText);
        }

        if (!match.Success)
        {
            match = CompactApproximateQuantityPattern.Match(parsingText);
        }

        if (match.Success)
        {
            sourceName = CleanIngredientName(match.Groups["name"].Value);
            var quantityToken = match.Groups["quantity"].Value.Trim();
            unitText = match.Groups["unit"].Value.Trim();
            quantityText = string.Concat(quantityToken, unitText);
            unitCode = NormalizeUnit(unitText, quantityToken);
            ParseQuantityRange(quantityToken, out quantityValue, out quantityMaxValue);

            var tail = match.Groups["tail"].Value.Trim();
            if (parentheticalPrimaryQuantity)
            {
                householdMeasureText = tail.Trim(' ', '/', ',', ';', '(', ')');
            }
            else
            {
                ExtractTail(tail, out householdMeasureText, out preparationNote);
            }
            var isApproximate = quantityToken.Equals("약간", StringComparison.OrdinalIgnoreCase)
                                || quantityToken.Equals("조금", StringComparison.OrdinalIgnoreCase)
                                || quantityToken.Equals("적당량", StringComparison.OrdinalIgnoreCase)
                                || quantityToken.Equals("to taste", StringComparison.OrdinalIgnoreCase)
                                || quantityToken.Equals("as needed", StringComparison.OrdinalIgnoreCase);
            parseConfidence = isApproximate
                ? 0.82m
                : string.IsNullOrWhiteSpace(unitText)
                    ? 0.70m
                    : 0.97m;
            requiresReview = !isApproximate && string.IsNullOrWhiteSpace(unitText);
        }

        sourceName = CleanIngredientName(sourceName);
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return null;
        }

        var canonicalName = WhitespacePattern.Replace(sourceName, " ").Trim();
        var normalizedName = NormalizeName(canonicalName);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        var classification = Classify(normalizedName);
        requiresReview |= classification.State == OfficialFoodIngredientClassificationStates.PendingReview;

        return new ParsedOfficialFoodRecipeIngredient(
            canonicalName,
            normalizedName,
            classification.CategoryCode,
            ClassificationMethod,
            classification.Confidence,
            classification.State,
            CleanGroupName(groupName),
            originalText,
            sourceName,
            quantityText,
            quantityValue,
            quantityMaxValue,
            unitCode,
            unitText,
            householdMeasureText,
            preparationNote,
            displayOrder,
            parseConfidence,
            requiresReview);
    }

    private static IReadOnlyList<string> BuildLogicalLines(IReadOnlyList<string> ingredientTexts)
    {
        var result = new List<string>();
        var buffer = new StringBuilder();
        var parenthesisDepth = 0;

        foreach (var rawText in ingredientTexts.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            foreach (var rawLine in rawText.Replace("\r", string.Empty, StringComparison.Ordinal).Split('\n'))
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                if (buffer.Length > 0)
                {
                    buffer.Append(' ');
                }

                buffer.Append(line);
                parenthesisDepth += line.Count(character => character == '(')
                                    - line.Count(character => character == ')');
                if (parenthesisDepth <= 0)
                {
                    result.Add(buffer.ToString());
                    buffer.Clear();
                    parenthesisDepth = 0;
                }
            }
        }

        if (buffer.Length > 0)
        {
            result.Add(buffer.ToString());
        }

        return result;
    }

    private static bool TryExtractHeading(
        string line,
        out string heading,
        out string remainder)
    {
        var depth = 0;
        for (var index = 0; index < line.Length; index++)
        {
            depth += line[index] == '(' ? 1 : line[index] == ')' ? -1 : 0;
            if (depth != 0 || line[index] is not (':' or '：'))
            {
                continue;
            }

            var candidate = CleanGroupName(line[..index]);
            if (candidate.Length is 0 or > 60 || QuantityPattern.IsMatch(candidate))
            {
                break;
            }

            heading = candidate;
            remainder = CleanDecorations(line[(index + 1)..]);
            return true;
        }

        heading = string.Empty;
        remainder = line;
        return false;
    }

    private static bool IsStandaloneHeading(
        IReadOnlyList<string> lines,
        int lineIndex,
        string line)
    {
        var cleaned = CleanGroupName(line);
        if (KnownGroupNames.Contains(cleaned))
        {
            return true;
        }

        if (lineIndex != 0
            || lineIndex + 1 >= lines.Count
            || cleaned.Length > 40
            || line.Contains(',')
            || QuantityPattern.IsMatch(line))
        {
            return false;
        }

        var nextLine = CleanDecorations(lines[lineIndex + 1]);
        return nextLine.Contains(',') || QuantityPattern.IsMatch(nextLine);
    }

    private static IEnumerable<string> SplitTopLevel(string line)
    {
        var start = 0;
        var depth = 0;
        for (var index = 0; index < line.Length; index++)
        {
            depth += line[index] == '(' ? 1 : line[index] == ')' ? -1 : 0;
            if (depth > 0 || line[index] is not (',' or ';'))
            {
                continue;
            }

            yield return line[start..index];
            start = index + 1;
        }

        if (start < line.Length)
        {
            yield return line[start..];
        }
    }

    private static void ExtractTail(
        string tail,
        out string householdMeasureText,
        out string preparationNote)
    {
        householdMeasureText = string.Empty;
        preparationNote = string.Empty;
        if (string.IsNullOrWhiteSpace(tail))
        {
            return;
        }

        if (tail[0] == '(')
        {
            var depth = 0;
            for (var index = 0; index < tail.Length; index++)
            {
                depth += tail[index] == '(' ? 1 : tail[index] == ')' ? -1 : 0;
                if (depth != 0)
                {
                    continue;
                }

                householdMeasureText = tail[1..index].Trim();
                preparationNote = tail[(index + 1)..].Trim(' ', ',', ';', '-');
                return;
            }
        }

        preparationNote = tail.Trim(' ', ',', ';', '-');
    }

    private static void ParseQuantityRange(
        string value,
        out decimal? minimum,
        out decimal? maximum)
    {
        var parts = Regex.Split(value, @"\s*[~∼–-]\s*", RegexOptions.CultureInvariant);
        minimum = TryParseQuantity(parts[0], out var parsedMinimum) ? parsedMinimum : null;
        maximum = parts.Length > 1 && TryParseQuantity(parts[1], out var parsedMaximum)
            ? parsedMaximum
            : null;
    }

    private static bool TryParseQuantity(string value, out decimal result)
    {
        var text = value.Trim();
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        var fractionSlashIndex = text.IndexOfAny(['/', '⁄']);
        if (fractionSlashIndex >= 0)
        {
            var left = text[..fractionSlashIndex].Trim();
            var right = text[(fractionSlashIndex + 1)..].Trim();
            var leftParts = left.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (decimal.TryParse(leftParts[^1], NumberStyles.Number, CultureInfo.InvariantCulture, out var numerator)
                && decimal.TryParse(right, NumberStyles.Number, CultureInfo.InvariantCulture, out var denominator)
                && denominator != 0)
            {
                var whole = leftParts.Length > 1
                    && decimal.TryParse(leftParts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedWhole)
                    ? parsedWhole
                    : 0m;
                result = whole + numerator / denominator;
                return true;
            }
        }

        if (text.Length > 0
            && UnicodeFractions.TryGetValue(text[^1], out var fraction))
        {
            var wholeText = text[..^1].Trim();
            var whole = wholeText.Length == 0
                ? 0m
                : decimal.TryParse(wholeText, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedWhole)
                    ? parsedWhole
                    : decimal.MinValue;
            if (whole != decimal.MinValue)
            {
                result = whole + fraction;
                return true;
            }
        }

        result = default;
        return false;
    }

    private static (string CategoryCode, decimal Confidence, string State) Classify(string normalizedName)
    {
        foreach (var rule in CategoryRules)
        {
            if (rule.Keywords.Any(keyword => ContainsKeyword(normalizedName, keyword)))
            {
                return (
                    rule.CategoryCode,
                    rule.Confidence,
                    OfficialFoodIngredientClassificationStates.AutoClassified);
            }
        }

        return (
            OfficialFoodIngredientCategoryCodes.Other,
            0.20m,
            OfficialFoodIngredientClassificationStates.PendingReview);
    }

    private static bool ContainsKeyword(string normalizedName, string keyword)
    {
        var normalizedKeyword = NormalizeName(keyword);
        if (normalizedKeyword.Any(character => character >= '가' && character <= '힣'))
        {
            if (TokenOnlyKoreanKeywords.Contains(normalizedKeyword))
            {
                return normalizedName
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Contains(normalizedKeyword, StringComparer.Ordinal);
            }

            return normalizedName.Contains(normalizedKeyword, StringComparison.Ordinal);
        }

        var paddedName = $" {normalizedName} ";
        return paddedName.Contains($" {normalizedKeyword} ", StringComparison.Ordinal);
    }

    private static string NormalizeUnit(string unitText, string quantityText)
    {
        if (string.IsNullOrWhiteSpace(unitText))
        {
            return quantityText.Equals("약간", StringComparison.OrdinalIgnoreCase)
                   || quantityText.Equals("조금", StringComparison.OrdinalIgnoreCase)
                   || quantityText.Equals("적당량", StringComparison.OrdinalIgnoreCase)
                   || quantityText.Equals("to taste", StringComparison.OrdinalIgnoreCase)
                   || quantityText.Equals("as needed", StringComparison.OrdinalIgnoreCase)
                ? "approx"
                : string.Empty;
        }

        return unitText.Trim().ToLowerInvariant() switch
        {
            "g" or "gram" or "grams" => "g",
            "kg" or "㎏" or "kilogram" or "kilograms" => "kg",
            "mg" or "㎎" => "mg",
            "ml" or "㎖" or "cc" or "milliliter" or "milliliters" or "millilitre" or "millilitres" => "ml",
            "l" or "liter" or "liters" or "litre" or "litres" => "l",
            "큰술" or "tablespoon" or "tablespoons" or "tbsp" => "tbsp",
            "ts" or "t" => "tbsp",
            "작은술" or "teaspoon" or "teaspoons" or "tsp" => "tsp",
            "컵" or "cup" or "cups" => "cup",
            "팩" => "pack",
            "캔" => "can",
            "줌" => "handful",
            "단" => "bunch",
            "쪽" => "clove",
            "개" or "장" or "알" or "모" or "봉지" or "줄기" or "가닥" or "송이" or "조각" or "마리" or "대" => "count",
            "oz" or "ounce" or "ounces" => "oz",
            "lb" or "pound" or "pounds" => "lb",
            _ => "custom"
        };
    }

    private static string CleanDecorations(string value)
        => value.Trim().TrimStart('●', '·', '•', '▪', '-', '*', ' ');

    private static string CleanGroupName(string value)
        => WhitespacePattern.Replace(
                CleanDecorations(value).Trim(' ', ':', '：', ',', ';'),
                " ")
            .Trim();

    private static string CleanIngredientName(string value)
        => WhitespacePattern.Replace(
                ServingPrefixPattern.Replace(CleanDecorations(value), string.Empty)
                    .Trim(' ', ':', '：', ',', ';'),
                " ")
            .Trim();
}
