using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

internal sealed record BlsKamis품목후보Definition(
    string KamisCategoryCode,
    string KamisCategoryName,
    string KamisItemCode,
    string KamisItemName,
    string MatchQualityCode,
    string ReviewStatusCode,
    bool AllowsDirectPriceComparison,
    string ReviewNote);

internal sealed record Bls평균소매가격SeriesDefinition(
    string SeriesId,
    string ItemCode,
    string CanonicalProductKey,
    string ProductNameKo,
    string ItemNameEn,
    string OriginalUnit,
    string MappingStatusCode,
    IReadOnlyList<BlsKamis품목후보Definition> KamisCandidates);

internal static class Bls평균소매가격SeriesCatalog
{
    public const string SourceUrl =
        "https://api.bls.gov/publicAPI/v1/timeseries/data/";

    public const string SeriesCatalogUrl =
        "https://download.bls.gov/pub/time.series/ap/ap.series";

    public const string FredGraphCsvUrl =
        "https://fred.stlouisfed.org/graph/fredgraph.csv";

    public const string DocumentationUrl =
        "https://www.bls.gov/cpi/factsheets/average-prices.htm";

    public static DateOnly CatalogObservedAt { get; } = new(2026, 7, 14);

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> RiceCandidates =
    [
        Kamis(
            "100",
            "식량작물",
            "111",
            "쌀",
            BlsKamis비교품질Codes.직접품목후보,
            "백미라는 공통 품목 후보입니다. 품종·등급·도소매 단계와 거래단위를 맞춘 뒤 비교해야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> BeanCandidates =
    [
        Kamis(
            "100",
            "식량작물",
            "141",
            "콩",
            BlsKamis비교품질Codes.광의품목후보,
            "BLS의 dried beans는 여러 콩류를 합친 값이라 KAMIS 콩과 직접 가격 비교하지 않습니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> BeefCandidates =
    [
        Kamis(
            "500",
            "축산물",
            "4301",
            "소",
            BlsKamis비교품질Codes.광의품목후보,
            "BLS는 미국 소매 부위 가격이고 KAMIS는 국내 축산물 등급·단위가 달라 광의 품목 후보입니다."),
        Kamis(
            "500",
            "축산물",
            "4401",
            "수입 소고기",
            BlsKamis비교품질Codes.광의품목후보,
            "BLS는 원산지를 구분하지 않는 미국 소매 가격이므로 KAMIS 수입 소고기와 직접 비교하지 않습니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> PorkCandidates =
    [
        Kamis(
            "500",
            "축산물",
            "4304",
            "돼지",
            BlsKamis비교품질Codes.광의품목후보,
            "BLS는 미국 소매 부위·가공상태 가격이고 KAMIS는 국내 축산물 등급·단위가 달라 광의 품목 후보입니다."),
        Kamis(
            "500",
            "축산물",
            "4402",
            "수입 돼지고기",
            BlsKamis비교품질Codes.광의품목후보,
            "BLS는 원산지를 구분하지 않는 미국 소매 가격이므로 KAMIS 수입 돼지고기와 직접 비교하지 않습니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> ChickenCandidates =
    [
        Kamis(
            "500",
            "축산물",
            "9901",
            "닭",
            BlsKamis비교품질Codes.직접품목후보,
            "닭고기 공통 품목 후보이나 통닭·다리·가슴살 부위와 마리·중량 단위를 구분해야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> EggCandidates =
    [
        Kamis(
            "500",
            "축산물",
            "9903",
            "계란",
            BlsKamis비교품질Codes.직접품목후보,
            "대란 계열 후보입니다. BLS dozen과 KAMIS 판·개 단위 및 등급을 맞춘 뒤 비교해야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> MilkCandidates =
    [
        Kamis(
            "500",
            "축산물",
            "9908",
            "우유",
            BlsKamis비교품질Codes.직접품목후보,
            "소매 우유 공통 품목 후보입니다. 지방 함량과 gallon·liter 단위를 맞춘 뒤 비교해야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> DairyProductCandidates =
    [
        Kamis(
            "500",
            "축산물",
            "9908",
            "우유",
            BlsKamis비교품질Codes.가공연관품목,
            "우유 유래 가공품 관계만 표시하며 원유·우유 가격과 직접 비교하지 않습니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> BananaCandidates =
    [
        Kamis(
            "400",
            "과일류",
            "418",
            "바나나",
            BlsKamis비교품질Codes.직접품목후보,
            "바나나 공통 품목 후보입니다. 산지·등급·유통단계와 lb·kg 단위를 맞춰야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> OrangeCandidates =
    [
        Kamis(
            "400",
            "과일류",
            "421",
            "오렌지",
            BlsKamis비교품질Codes.직접품목후보,
            "네이블오렌지와 KAMIS 오렌지의 품종·등급·유통단위를 맞춘 뒤 비교해야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> OrangeProductCandidates =
    [
        Kamis(
            "400",
            "과일류",
            "421",
            "오렌지",
            BlsKamis비교품질Codes.가공연관품목,
            "오렌지 주스와 생과의 원재료 관계만 표시하며 가격을 직접 비교하지 않습니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> LemonCandidates =
    [
        Kamis(
            "400",
            "과일류",
            "424",
            "레몬",
            BlsKamis비교품질Codes.직접품목후보,
            "레몬 공통 품목 후보입니다. 산지·등급·유통단계와 lb·kg 단위를 맞춰야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> StrawberryCandidates =
    [
        Kamis(
            "200",
            "채소류",
            "226",
            "딸기",
            BlsKamis비교품질Codes.직접품목후보,
            "딸기 공통 품목 후보이나 BLS dry pint와 KAMIS 거래단위·등급을 맞춰야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> PotatoCandidates =
    [
        Kamis(
            "100",
            "식량작물",
            "152",
            "감자",
            BlsKamis비교품질Codes.직접품목후보,
            "흰감자 공통 품목 후보입니다. 품종·등급·도소매 단계와 lb·kg 단위를 맞춰야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> PotatoProductCandidates =
    [
        Kamis(
            "100",
            "식량작물",
            "152",
            "감자",
            BlsKamis비교품질Codes.가공연관품목,
            "감자칩과 생감자의 원재료 관계만 표시하며 가격을 직접 비교하지 않습니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> LettuceCandidates =
    [
        Kamis(
            "200",
            "채소류",
            "214",
            "상추",
            BlsKamis비교품질Codes.직접품목후보,
            "lettuce 공통 품목 후보이나 iceberg·romaine과 국내 상추 품종을 구분해야 합니다.")
    ];

    private static readonly IReadOnlyList<BlsKamis품목후보Definition> TomatoCandidates =
    [
        Kamis(
            "200",
            "채소류",
            "225",
            "토마토",
            BlsKamis비교품질Codes.직접품목후보,
            "일반 토마토 공통 품목 후보입니다. 노지·시설 및 등급·유통단계를 맞춰야 합니다.")
    ];

    public static IReadOnlyList<Bls평균소매가격SeriesDefinition> All { get; } =
    [
        Series(
            "APU0000701111", "701111", "flour-white-all-purpose", "다목적 흰밀가루",
            "Flour, white, all purpose, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000701312", "701312", "rice-white-long-grain", "장립종 백미",
            "Rice, white, long grain, uncooked, per lb. (453.6 gm)", "lb",
            RiceCandidates),
        Series(
            "APU0000701322", "701322", "spaghetti-macaroni", "스파게티·마카로니",
            "Spaghetti and macaroni, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000702111", "702111", "bread-white-pan", "식빵",
            "Bread, white, pan, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000702212", "702212", "bread-whole-wheat-pan", "통밀식빵",
            "Bread, whole wheat, pan, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000702421", "702421", "cookies-chocolate-chip", "초콜릿칩 쿠키",
            "Cookies, chocolate chip, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000703111", "703111", "beef-ground-chuck", "다진 척 소고기",
            "Ground chuck, 100% beef, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000703112", "703112", "beef-ground", "다진 소고기",
            "Ground beef, 100% beef, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000703113", "703113", "beef-ground-lean", "저지방 다진 소고기",
            "Ground beef, lean and extra lean, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000703213", "703213", "beef-chuck-roast-choice-boneless", "초이스급 척 로스트",
            "Chuck roast, USDA Choice, boneless, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000703311", "703311", "beef-round-roast-choice-boneless", "초이스급 라운드 로스트",
            "Round roast, USDA Choice, boneless, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000703432", "703432", "beef-stew-boneless", "스튜용 소고기",
            "Beef for stew, boneless, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000703511", "703511", "beef-round-steak-choice-boneless", "초이스급 라운드 스테이크",
            "Steak, round, USDA Choice, boneless, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000703512", "703512", "beef-round-steak-other", "기타 등급 라운드 스테이크",
            "Steak, round, graded and ungraded, excluding USDA Prime and Choice, per lb. (453.6 gm)",
            "lb", BeefCandidates),
        Series(
            "APU0000703613", "703613", "beef-sirloin-steak-choice-boneless", "초이스급 등심 스테이크",
            "Steak, sirloin, USDA Choice, boneless, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000704111", "704111", "pork-bacon-sliced", "슬라이스 베이컨",
            "Bacon, sliced, per lb. (453.6 gm)", "lb", PorkCandidates),
        Series(
            "APU0000704211", "704211", "pork-chop-center-bone-in", "뼈 있는 돼지 등심 중앙부",
            "Chops, center cut, bone-in, per lb. (453.6 gm)", "lb", PorkCandidates),
        Series(
            "APU0000704212", "704212", "pork-chop-boneless", "뼈 없는 돼지 등심",
            "Chops, boneless, per lb. (453.6 gm)", "lb", PorkCandidates),
        Series(
            "APU0000704312", "704312", "pork-ham-boneless", "뼈 없는 햄",
            "Ham, boneless, excluding canned, per lb. (453.6 gm)", "lb", PorkCandidates),
        Series(
            "APU0000706111", "706111", "chicken-whole-fresh", "생닭",
            "Chicken, fresh, whole, per lb. (453.6 gm)", "lb", ChickenCandidates),
        Series(
            "APU0000706212", "706212", "chicken-legs-bone-in", "뼈 있는 닭다리",
            "Chicken legs, bone-in, per lb. (453.6 gm)", "lb", ChickenCandidates),
        Series(
            "APU0000708111", "708111", "egg-grade-a-large", "A등급 대란",
            "Eggs, grade A, large, per doz.", "dozen", EggCandidates),
        Series(
            "APU0000709112", "709112", "milk-whole", "전지우유",
            "Milk, fresh, whole, fortified, per gal. (3.8 lit)", "gallon", MilkCandidates),
        Series(
            "APU0000710211", "710211", "cheese-american-processed", "아메리칸 가공치즈",
            "American processed cheese, per lb. (453.6 gm)", "lb", DairyProductCandidates),
        Series(
            "APU0000710212", "710212", "cheese-cheddar-natural", "천연 체더치즈",
            "Cheddar cheese, natural, per lb. (453.6 gm)", "lb", DairyProductCandidates),
        Series(
            "APU0000710411", "710411", "ice-cream-regular", "일반 아이스크림",
            "Ice cream, prepackaged, bulk, regular, per 1/2 gal. (1.9 lit)", "half-gallon",
            DairyProductCandidates),
        Series(
            "APU0000711211", "711211", "banana", "바나나",
            "Bananas, per lb. (453.6 gm)", "lb", BananaCandidates),
        Series(
            "APU0000711311", "711311", "orange-navel", "네이블오렌지",
            "Oranges, Navel, per lb. (453.6 gm)", "lb", OrangeCandidates),
        Series(
            "APU0000711411", "711411", "grapefruit", "자몽",
            "Grapefruit, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000711412", "711412", "lemon", "레몬",
            "Lemons, per lb. (453.6 gm)", "lb", LemonCandidates),
        Series(
            "APU0000711415", "711415", "strawberry", "딸기",
            "Strawberries, dry pint, per 12 oz. (340.2 gm)", "12 oz dry pint",
            StrawberryCandidates),
        Series(
            "APU0000712112", "712112", "potato-white", "흰감자",
            "Potatoes, white, per lb. (453.6 gm)", "lb", PotatoCandidates),
        Series(
            "APU0000712211", "712211", "lettuce-iceberg", "아이스버그 상추",
            "Lettuce, iceberg, per lb. (453.6 gm)", "lb", LettuceCandidates),
        Series(
            "APU0000712311", "712311", "tomato-field-grown", "노지토마토",
            "Tomatoes, field grown, per lb. (453.6 gm)", "lb", TomatoCandidates),
        Series(
            "APU0000713111", "713111", "orange-juice-frozen-concentrate", "냉동 농축 오렌지주스",
            "Orange juice, frozen concentrate, 12 oz. can, per 16 oz. (473.2 ml)",
            "16 oz equivalent", OrangeProductCandidates),
        Series(
            "APU0000714221", "714221", "corn-canned", "통조림 옥수수",
            "Corn, canned, any style, all sizes, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000714233", "714233", "beans-dried", "건조 콩류",
            "Beans, dried, any type, all sizes, per lb. (453.6 gm)", "lb",
            BeanCandidates),
        Series(
            "APU0000715211", "715211", "sugar-white", "백설탕",
            "Sugar, white, all sizes, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000717311", "717311", "coffee-ground-roast", "원두 분쇄커피",
            "Coffee, 100%, ground roast, all sizes, per lb. (453.6 gm)", "lb"),
        Series(
            "APU0000718311", "718311", "potato-chips", "감자칩",
            "Potato chips, per 16 oz.", "16 oz", PotatoProductCandidates),
        Series(
            "APU0000720111", "720111", "malt-beverage", "맥아음료",
            "Malt beverages, all types, all sizes, any origin, per 16 oz. (473.2 ml)",
            "16 oz"),
        Series(
            "APU0000720311", "720311", "wine-table", "테이블와인",
            "Wine, red and white table, all sizes, any origin, per 1 liter (33.8 oz)",
            "liter"),
        Series(
            "APU0000FC1101", "FC1101", "beef-ground-all-uncooked", "전체 생 다진소고기",
            "All uncooked ground beef, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000FC2101", "FC2101", "beef-roasts-all-uncooked", "전체 생 소고기 로스트",
            "All Uncooked Beef Roasts, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000FC3101", "FC3101", "beef-steaks-all-uncooked", "전체 생 소고기 스테이크",
            "All Uncooked Beef Steaks, per lb. (453.6 gm)", "lb", BeefCandidates),
        Series(
            "APU0000FC4101", "FC4101", "beef-other-all-uncooked", "전체 기타 생 소고기",
            "All Uncooked Other Beef (Excluding Veal), per lb. (453.6 gm)", "lb",
            BeefCandidates),
        Series(
            "APU0000FD2101", "FD2101", "pork-ham-all", "전체 햄",
            "All Ham (Excluding Canned Ham and Luncheon Slices), per lb. (453.6 gm)",
            "lb", PorkCandidates),
        Series(
            "APU0000FD3101", "FD3101", "pork-chops-all", "전체 돼지 등심",
            "All Pork Chops, per lb. (453.6 gm)", "lb", PorkCandidates),
        Series(
            "APU0000FD4101", "FD4101", "pork-other-all", "전체 기타 돼지고기",
            "All Other Pork (Excluding Canned Ham and Luncheon Slices), per lb. (453.6 gm)",
            "lb", PorkCandidates),
        Series(
            "APU0000FF1101", "FF1101", "chicken-breast-boneless", "뼈 없는 닭가슴살",
            "Chicken breast, boneless, per lb. (453.6 gm)", "lb", ChickenCandidates),
        Series(
            "APU0000FJ1101", "FJ1101", "milk-low-fat", "저지방우유",
            "Milk, fresh, low-fat, reduced fat, skim, per gal. (3.8 lit)", "gallon",
            MilkCandidates),
        Series(
            "APU0000FJ4101", "FJ4101", "yogurt", "요거트",
            "Yogurt, per 8 oz. (226.8 gm)", "8 oz", DairyProductCandidates),
        Series(
            "APU0000FL2101", "FL2101", "lettuce-romaine", "로메인 상추",
            "Lettuce, romaine, per lb. (453.6 gm)", "lb", LettuceCandidates),
        Series(
            "APU0000FN1101", "FN1101", "soft-drink-two-liter", "탄산음료 2리터",
            "All soft drinks, per 2 liters (67.6 oz)", "2 liter"),
        Series(
            "APU0000FN1102", "FN1102", "soft-drink-twelve-pack", "탄산음료 12캔",
            "All soft drinks, 12 pk, 12 oz., cans, per 12 oz. (354.9 ml)",
            "12 oz can"),
        Series(
            "APU0000FS1101", "FS1101", "butter-stick", "스틱 버터",
            "Butter, stick, per lb. (453.6 gm)", "lb", DairyProductCandidates)
    ];

    public static Bls평균소매가격SeriesDefinition? Find(string? seriesId)
        => string.IsNullOrWhiteSpace(seriesId)
            ? null
            : All.FirstOrDefault(item => string.Equals(
                item.SeriesId,
                seriesId.Trim(),
                StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<Bls평균소매가격Series응답> ToResponse()
        => All
            .Select(item => new Bls평균소매가격Series응답(
                item.SeriesId,
                item.ItemCode,
                item.CanonicalProductKey,
                item.ProductNameKo,
                item.ItemNameEn,
                item.OriginalUnit,
                "0000",
                "U.S. city average",
                item.MappingStatusCode,
                SeriesCatalogUrl))
            .ToArray();

    public static BlsKamis비교Catalog응답 ToKamisComparisonResponse()
    {
        var items = All
            .Select(item => new BlsKamisSeries비교검토응답(
                item.SeriesId,
                item.ItemCode,
                item.CanonicalProductKey,
                item.ProductNameKo,
                item.ItemNameEn,
                item.OriginalUnit,
                item.MappingStatusCode,
                item.KamisCandidates
                    .Select(candidate => new BlsKamis품목후보응답(
                        candidate.KamisCategoryCode,
                        candidate.KamisCategoryName,
                        candidate.KamisItemCode,
                        candidate.KamisItemName,
                        candidate.MatchQualityCode,
                        candidate.ReviewStatusCode,
                        candidate.AllowsDirectPriceComparison,
                        candidate.ReviewNote))
                    .ToArray()))
            .ToArray();
        var candidateSeries = items
            .Where(item => item.KamisCandidates.Count > 0)
            .ToArray();

        return new BlsKamis비교Catalog응답(
            CatalogObservedAt,
            items.Length,
            candidateSeries.Length,
            candidateSeries.Count(item => item.KamisCandidates.Any(candidate =>
                candidate.AllowsDirectPriceComparison)),
            candidateSeries
                .SelectMany(item => item.KamisCandidates)
                .Select(candidate =>
                    $"{candidate.KamisCategoryCode}:{candidate.KamisItemCode}")
                .Distinct(StringComparer.Ordinal)
                .Count(),
            items,
            [
                "BLS는 미국 전국 도시 소비자 소매 월평균이고 KAMIS는 한국 도·소매 관측이므로 시장 단계와 지역이 다릅니다.",
                "통화, 기준 월·일, 품종, 등급과 원 거래단위를 먼저 맞추기 전에는 가격 우열을 계산하지 않습니다.",
                "BroadCommodityCandidate와 RelatedProcessedProduct는 직접 가격 비교에 사용하지 않습니다.",
                "2026년 BLS 계열은 공식 ap.series에서 2026년 관측이 확인된 전국 식품 계열을 기준으로 했으며 이후 중단·추가 여부를 재점검해야 합니다."
            ]);
    }

    private static Bls평균소매가격SeriesDefinition Series(
        string seriesId,
        string itemCode,
        string canonicalProductKey,
        string productNameKo,
        string itemNameEn,
        string originalUnit,
        IReadOnlyList<BlsKamis품목후보Definition>? kamisCandidates = null)
    {
        var candidates = kamisCandidates ?? [];
        return new Bls평균소매가격SeriesDefinition(
            seriesId,
            itemCode,
            canonicalProductKey,
            productNameKo,
            itemNameEn,
            originalUnit,
            candidates.Count == 0
                ? Bls평균소매가격Mapping상태Codes.후보없음
                : Bls평균소매가격Mapping상태Codes.후보,
            candidates);
    }

    private static BlsKamis품목후보Definition Kamis(
        string categoryCode,
        string categoryName,
        string itemCode,
        string itemName,
        string matchQualityCode,
        string reviewNote)
        => new(
            categoryCode,
            categoryName,
            itemCode,
            itemName,
            matchQualityCode,
            Bls평균소매가격Mapping상태Codes.후보,
            matchQualityCode == BlsKamis비교품질Codes.직접품목후보,
            reviewNote);
}
