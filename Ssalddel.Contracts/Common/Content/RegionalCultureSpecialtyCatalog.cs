using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Contracts.Common.Content;

public static class RegionalCultureSpecialtyRoutes
{
    public const string Browse = CommunityPageRoutes.Regions;
    public const string DetailTemplate = $"{Browse}/{{RegionKey}}";
    public const string RegionalProducts = "/information/regional-products";
    public const string ProducePriceComparison = "/information/produce-price-comparison";
    public const string ApplePriceComparison = "/information/apple-price-comparison";

    public static string DetailFor(string regionKey)
        => $"{Browse}/{EscapeKey(regionKey)}";

    public static string ProductsFor(string regionKey)
        => $"{RegionalProducts}?regionKey={Uri.EscapeDataString(EscapeKey(regionKey))}";

    public static string PriceComparisonFor(string regionKey, string productKey)
        => $"{ProducePriceComparison}?regionKey={Uri.EscapeDataString(EscapeKey(regionKey))}"
           + $"&productKey={Uri.EscapeDataString(EscapeKey(productKey))}";

    private static string EscapeKey(string key)
    {
        var normalized = key?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.IndexOfAny(['/', '\\', '?', '#']) >= 0)
        {
            throw new ArgumentException("지역·상품 key는 비어 있지 않고 경로 구분자를 포함하지 않아야 합니다.", nameof(key));
        }

        return normalized;
    }
}

public sealed record RegionalSpecialty(
    string Key,
    string Name,
    string Category,
    string DiscoveryNote);

public sealed record RegionalCultureSpecialty(
    string Key,
    string CountryCode,
    string CountryName,
    string RegionName,
    string RegionType,
    string Geography,
    string CultureSummary,
    string HeroImagePath,
    string HeroImageAlt,
    IReadOnlyList<string> CultureQuestions,
    IReadOnlyList<RegionalSpecialty> Specialties,
    string EvidenceBoundary);

public static class RegionalCultureSpecialtyCatalog
{
    public const string UnitedStatesCountryCode = "US";
    public const string ChinaCountryCode = "CN";

    public static IReadOnlyList<RegionalCultureSpecialty> All { get; } =
    [
        new(
            "us-maine",
            UnitedStatesCountryCode,
            "미국",
            "메인",
            "현재 주",
            "미국 북동부 대서양 연안",
            "차가운 바다와 숲, 작은 항구 도시의 생활을 음식과 함께 살펴보는 지역입니다.",
            "_content/Ssalddel.Ui.Common/images/regions/us-maine.png",
            "메인의 바닷가 마을과 숲, 블루베리와 해산물을 함께 나누는 주민들을 그린 생성 일러스트",
            ["해산물은 어느 계절과 공동체 행사에서 먹을까요?", "내륙과 해안의 식생활은 어떻게 다를까요?"],
            [
                new("lobster", "랍스터", "수산물", "어획 시기·규격·가공 형태와 냉장·냉동 조건을 함께 확인합니다."),
                new("wild-blueberry", "야생 블루베리", "농산물", "재배종과 구분하고 수확 시기·가공 형태를 확인합니다.")
            ],
            "대표성은 탐색 출발점입니다. 실제 산지·어획지와 한국 반입 조건은 상품별 공식 근거로 다시 확인합니다."),
        new(
            "us-georgia",
            UnitedStatesCountryCode,
            "미국",
            "조지아",
            "현재 주",
            "미국 남동부",
            "남부의 농업과 음식 전통이 도시 문화와 만나는 지역으로 소개합니다.",
            "_content/Ssalddel.Ui.Common/images/regions/us-georgia.png",
            "조지아의 복숭아 과수원과 피칸 나무 곁 장터에서 이웃들이 음식을 나누는 모습을 그린 생성 일러스트",
            ["복숭아와 피칸은 지역 식탁에서 어떻게 쓰일까요?", "지역 축제와 계절 음식에는 어떤 이야기가 있을까요?"],
            [
                new("peach", "복숭아", "농산물", "품종·수확 시기·생과 또는 가공 형태를 구분합니다."),
                new("pecan", "피칸", "견과", "원물·탈각·가공 제품의 규격과 알레르기 표시를 확인합니다.")
            ],
            "주의 별칭이나 이미지가 모든 생산량을 뜻하지 않습니다. 생산·유통 근거는 품목과 연도별로 확인합니다."),
        new(
            "us-california",
            UnitedStatesCountryCode,
            "미국",
            "캘리포니아",
            "현재 주",
            "미국 서부 태평양 연안과 중앙계곡",
            "이주 문화와 다양한 기후대가 여러 식재료와 조리 문화를 만든 지역으로 탐색합니다.",
            "_content/Ssalddel.Ui.Common/images/regions/us-california.png",
            "캘리포니아의 해안과 농장, 감귤·포도·아몬드 장터에서 만나는 여러 주민을 그린 생성 일러스트",
            ["같은 재료가 지역 공동체마다 어떻게 다르게 쓰일까요?", "물·기후·노동 조건은 생산 이야기에 어떻게 드러날까요?"],
            [
                new("almond", "아몬드", "견과", "산지·수확연도·가공 방식과 물 사용 근거를 함께 살펴봅니다."),
                new("citrus", "감귤류", "농산물", "품종·수확지·생과 반입과 가공품 조건을 구분합니다."),
                new("grape-product", "포도 가공품", "가공식품", "품종·산지표시·알코올 포함 여부에 따라 별도 검토합니다.")
            ],
            "주 단위 설명만으로 생산지나 지속가능성을 확정하지 않습니다. 카운티·생산자·상품 근거가 필요합니다."),
        new(
            "cn-shandong",
            ChinaCountryCode,
            "중국",
            "산둥성",
            "현재 성",
            "중국 동부 황해·발해 연안",
            "해안과 평야의 농수산물, 밀을 중심으로 한 음식 문화를 함께 살펴보는 지역입니다.",
            "_content/Ssalddel.Ui.Common/images/regions/cn-shandong.png",
            "산둥성의 연안 장터와 밀 음식 만들기, 마늘·땅콩·수산물을 둘러싼 일상을 그린 생성 일러스트",
            ["해안과 내륙의 음식은 어떻게 다를까요?", "명절·시장·가정식에서 대표 재료는 어떻게 쓰일까요?"],
            [
                new("apple", "사과", "농산물", "도시·현 단위 산지와 품종, 수확 시기를 확인합니다."),
                new("peanut", "땅콩", "농산물", "원물·기름·가공품을 구분하고 알레르기 표시를 확인합니다."),
                new("garlic", "마늘", "농산물", "신선·건조·분말 형태와 검역 조건을 구분합니다."),
                new("coastal-seafood", "연안 수산물", "수산물", "어종·어획 또는 양식·가공업소 소재지를 따로 확인합니다.")
            ],
            "식약처 해외제조업소 소재지가 원재료의 재배·어획 산지를 뜻하지 않습니다."),
        new(
            "cn-liaodong",
            ChinaCountryCode,
            "중국",
            "요동 지역",
            "역사·지리권",
            "현재 랴오닝성의 요동반도와 인접 지역",
            "한반도와 가까운 해상 교류, 항구와 농어촌의 생활문화를 현재 행정구역과 함께 살펴봅니다.",
            "_content/Ssalddel.Ui.Common/images/regions/cn-liaodong.png",
            "현재 랴오닝성 요동반도의 항구와 과수원, 사과·배·해산물을 함께 살펴보는 주민을 그린 생성 일러스트",
            ["다롄·단둥 등 도시별 식문화는 어떻게 다를까요?", "한반도와의 교류 흔적을 오늘의 생활에서 어떻게 확인할까요?"],
            [
                new("apple-pear", "사과·배", "농산물", "현재 성·시·현과 품종, 실제 재배지를 확인합니다."),
                new("seafood", "해산물", "수산물", "어종·양식 여부·가공 장소와 원산지를 구분합니다."),
                new("ginseng-product", "인삼 가공품", "가공식품", "식물 종·재배지·가공 성분과 국내 반입 기준을 확인합니다.")
            ],
            "‘요동’은 현재 행정구역명이 아닙니다. 상품 표시는 랴오닝성 등 현재 주소와 실제 원산지를 사용해야 합니다."),
        new(
            "cn-south-yangtze",
            ChinaCountryCode,
            "중국",
            "장강 이남",
            "넓은 문화·지리권",
            "장강 남쪽의 여러 성·도시를 묶은 탐색 범위",
            "하나의 단일 문화로 설명하지 않고 강남·화남·서남 등 서로 다른 생활권으로 더 나누어 살펴봅니다.",
            "_content/Ssalddel.Ui.Common/images/regions/cn-south-yangtze.png",
            "장강 이남의 강변 도시·논·차밭·아열대 장터 등 서로 다른 생활권을 이어 그린 개괄 생성 일러스트",
            ["어느 성·도시의 이야기인지 더 좁힐 수 있을까요?", "차·쌀·수산물의 생산과 소비 방식은 권역마다 어떻게 다를까요?"],
            [
                new("tea", "차", "농산·가공품", "산지·품종·제다 방식·수확 시기와 지리적 표시를 확인합니다."),
                new("rice-product", "쌀 가공품", "가공식품", "지역 조리법과 원재료 산지·첨가물 표시를 구분합니다."),
                new("bamboo-product", "대나무 가공품", "생활·식품", "식용·생활용을 구분하고 재질·가공·검역 조건을 확인합니다."),
                new("freshwater-seafood", "담수 수산물", "수산물", "어종·양식지·가공업소와 식품안전 근거를 각각 확인합니다.")
            ],
            "장강 이남은 매우 넓습니다. 성·도시·생산자 수준으로 좁히기 전에는 원산지나 문화 대표성을 확정하지 않습니다.")
    ];

    public static IReadOnlyList<RegionalCultureSpecialty> ForCountry(string? countryCode)
        => string.IsNullOrWhiteSpace(countryCode)
            ? All
            : All.Where(item => item.CountryCode.Equals(
                    countryCode.Trim(),
                StringComparison.OrdinalIgnoreCase))
                .ToArray();

    public static RegionalCultureSpecialty? Find(string? regionKey)
        => All.FirstOrDefault(item => item.Key.Equals(
            regionKey?.Trim(),
            StringComparison.OrdinalIgnoreCase));
}
