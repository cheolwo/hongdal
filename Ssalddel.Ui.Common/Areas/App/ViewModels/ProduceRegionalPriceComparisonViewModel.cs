using System.Globalization;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum 농산물가격표시통화
{
    KRW,
    CNY,
    USD
}

public enum 농산물가격분류
{
    과일,
    채소
}

public enum 농산물가격보기방식
{
    비교표,
    카드
}

public sealed record 농산물가격품목(
    string Key,
    농산물가격분류 Category,
    string Name,
    string Emoji,
    decimal DefaultWeightGrams);

public sealed record 농산물가격필터선택지(
    string Value,
    string Label);

public sealed record 농산물지역가격관측값(
    string ProductKey,
    string CountryCode,
    string CountryName,
    string RegionCode,
    string RegionName,
    string Variety,
    decimal NativePrice,
    농산물가격표시통화 NativeCurrency,
    string NativeUnit,
    decimal NativeKilogramPrice,
    string MarketStage,
    string ReferenceDate,
    string SourceName,
    string SourceUrl,
    string Limitation);

public sealed record 농산물지역가격비교항목(
    농산물지역가격관측값 Observation,
    decimal NativeWeightPrice,
    decimal DisplayWeightPrice,
    decimal DisplayKilogramPrice,
    decimal DisplayDifferenceFromLowest,
    decimal DifferencePercentFromLowest);

public sealed class 농산물지역가격비교ViewModel : PageViewModelBase
{
    public const string AllFilter = "ALL";

    private const decimal PoundsPerKilogram = 2.2046226218m;

    private static readonly IReadOnlyDictionary<농산물가격표시통화, decimal> UnitsPerEuro =
        new Dictionary<농산물가격표시통화, decimal>
        {
            [농산물가격표시통화.KRW] = 1688.78m,
            [농산물가격표시통화.CNY] = 7.7266m,
            [농산물가격표시통화.USD] = 1.1408m
        };

    private static readonly IReadOnlyList<농산물가격품목> ProductCatalog =
    [
        new("apple", 농산물가격분류.과일, "사과", "🍎", 250m),
        new("pear", 농산물가격분류.과일, "배", "🍐", 220m),
        new("tomato", 농산물가격분류.채소, "토마토", "🍅", 200m),
        new("onion", 농산물가격분류.채소, "양파", "🧅", 200m)
    ];

    private static readonly IReadOnlyList<농산물지역가격관측값> Observations =
    [
        new(
            "apple",
            "KR",
            "한국",
            "KR-NATIONAL",
            "전국",
            "후지 · 상품",
            25_808m,
            농산물가격표시통화.KRW,
            "10개",
            25_808m / 10m * 4m,
            "소매 조사값",
            "2026-05-21",
            "aT KAMIS",
            "https://www.kamis.or.kr/customer/info/retail/period.do?action=daily&countycode=&itemcategorycode=400&itemcode=411&kindcode=&marketclscode=&productrankcode=&regday=2026.05.21",
            "10개 묶음에 실제 중량이 없어 1개를 250g으로 가정한 전국 관측값입니다."),
        new(
            "apple",
            "US",
            "미국",
            "US-SC",
            "South Central U.S.",
            "Fuji",
            1.97m,
            농산물가격표시통화.USD,
            "lb",
            1.97m * PoundsPerKilogram,
            "광고 소매가",
            "2026-07-24",
            "USDA AMS",
            "https://www.ams.usda.gov/mnreports/fvwretail.pdf",
            "주요 소매점 광고의 지역 가중평균이며 실제 결제가는 아닙니다."),
        new(
            "apple",
            "US",
            "미국",
            "US-SW",
            "Southwest U.S.",
            "Fuji",
            2.99m,
            농산물가격표시통화.USD,
            "lb",
            2.99m * PoundsPerKilogram,
            "광고 소매가",
            "2026-07-24",
            "USDA AMS",
            "https://www.ams.usda.gov/mnreports/fvwretail.pdf",
            "주요 소매점 광고의 지역 가중평균이며 광고 수가 적은 지역값일 수 있습니다."),
        new(
            "apple",
            "CN",
            "중국",
            "CN-SD-JS",
            "山东临沂蒙阴 → 江苏无锡惠山",
            "红富士苹果",
            6m,
            농산물가격표시통화.CNY,
            "kg",
            6m,
            "산지·도매 관측값",
            "2026-07-21",
            "中国农业农村部 重点农产品市场信息平台",
            "https://ncpscxx.moa.gov.cn/",
            "개별 유통 경로의 관측값이며 중국 전체 평균이나 소비자 소매가가 아닙니다."),
        UsdaRegion("pear", "US-NE", "Northeast U.S.", "Bartlett", 1.99m),
        UsdaRegion("pear", "US-SE", "Southeast U.S.", "Bartlett", 1.99m),
        UsdaRegion("pear", "US-MW", "Midwest U.S.", "Bartlett", 1.84m),
        UsdaRegion("pear", "US-SC", "South Central U.S.", "Bartlett", 1.91m),
        UsdaRegion("pear", "US-SW", "Southwest U.S.", "Bartlett", 0.99m),
        UsdaRegion("tomato", "US-SC", "South Central U.S.", "Tomatoes", 2.53m),
        UsdaRegion("tomato", "US-SW", "Southwest U.S.", "Tomatoes", 0.99m),
        UsdaRegion("tomato", "US-NW", "Northwest U.S.", "Tomatoes", 1.99m),
        UsdaRegion("onion", "US-SC", "South Central U.S.", "Dry Yellow", 1.10m),
        UsdaRegion("onion", "US-SW", "Southwest U.S.", "Dry Yellow", 1.61m)
    ];

    private 농산물가격분류 _selectedCategory = 농산물가격분류.과일;
    private string _selectedProductKey = "apple";
    private string _selectedCountryCode = AllFilter;
    private string _selectedRegionCode = AllFilter;
    private decimal _comparisonWeightGrams = 250m;
    private 농산물가격표시통화 _displayCurrency;
    private 농산물가격보기방식 _viewMode = 농산물가격보기방식.비교표;

    public 농산물지역가격비교ViewModel()
        : this(CultureInfo.CurrentUICulture)
    {
    }

    public 농산물지역가격비교ViewModel(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        _displayCurrency = ResolveDisplayCurrency(culture);
    }

    public IReadOnlyList<농산물가격품목> Products => ProductCatalog;

    public IReadOnlyList<농산물가격품목> AvailableProducts
        => ProductCatalog.Where(product => product.Category == SelectedCategory).ToArray();

    public 농산물가격품목 SelectedProduct
        => ProductCatalog.Single(product => product.Key == SelectedProductKey);

    public IReadOnlyList<농산물가격필터선택지> AvailableCountries
        => [new(AllFilter, "모든 국가"), .. Observations
            .Where(observation => observation.ProductKey == SelectedProductKey)
            .GroupBy(observation => observation.CountryCode)
            .Select(group => new 농산물가격필터선택지(group.Key, group.First().CountryName))
            .OrderBy(option => option.Label)];

    public IReadOnlyList<농산물가격필터선택지> AvailableRegions
        => [new(AllFilter, "모든 지역"), .. Observations
            .Where(observation => observation.ProductKey == SelectedProductKey)
            .Where(observation => SelectedCountryCode == AllFilter
                || observation.CountryCode == SelectedCountryCode)
            .Select(observation => new 농산물가격필터선택지(
                observation.RegionCode,
                $"{observation.CountryName} · {observation.RegionName}"))
            .DistinctBy(option => option.Value)
            .OrderBy(option => option.Label)];

    public 농산물가격분류 SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (!SetProperty(ref _selectedCategory, value))
            {
                return;
            }

            var firstProduct = ProductCatalog.First(product => product.Category == value);
            _selectedProductKey = firstProduct.Key;
            _comparisonWeightGrams = firstProduct.DefaultWeightGrams;
            ResetLocationFilters();
            NotifySelectionChanged();
        }
    }

    public string SelectedProductKey
    {
        get => _selectedProductKey;
        set
        {
            var product = ProductCatalog.FirstOrDefault(candidate =>
                candidate.Key == value && candidate.Category == SelectedCategory);
            if (product is null || !SetProperty(ref _selectedProductKey, product.Key))
            {
                return;
            }

            _comparisonWeightGrams = product.DefaultWeightGrams;
            ResetLocationFilters();
            NotifySelectionChanged();
        }
    }

    public string SelectedCountryCode
    {
        get => _selectedCountryCode;
        set
        {
            var normalized = AvailableCountries.Any(option => option.Value == value)
                ? value
                : AllFilter;
            if (!SetProperty(ref _selectedCountryCode, normalized))
            {
                return;
            }

            _selectedRegionCode = AllFilter;
            OnPropertyChanged(nameof(SelectedRegionCode));
            OnPropertyChanged(nameof(AvailableRegions));
            OnPropertyChanged(nameof(Items));
        }
    }

    public string SelectedRegionCode
    {
        get => _selectedRegionCode;
        set
        {
            var normalized = AvailableRegions.Any(option => option.Value == value)
                ? value
                : AllFilter;
            if (SetProperty(ref _selectedRegionCode, normalized))
            {
                OnPropertyChanged(nameof(Items));
            }
        }
    }

    public decimal ComparisonWeightGrams
    {
        get => _comparisonWeightGrams;
        set
        {
            var normalized = Math.Clamp(value, 100m, 1000m);
            if (SetProperty(ref _comparisonWeightGrams, normalized))
            {
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(WeightLabel));
            }
        }
    }

    public 농산물가격표시통화 DisplayCurrency
    {
        get => _displayCurrency;
        set
        {
            if (SetProperty(ref _displayCurrency, value))
            {
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(DisplayCurrencyLabel));
            }
        }
    }

    public 농산물가격보기방식 ViewMode
    {
        get => _viewMode;
        set => SetProperty(ref _viewMode, value);
    }

    public string WeightLabel => $"{ComparisonWeightGrams:0}g";

    public string DisplayCurrencyLabel => DisplayCurrency switch
    {
        농산물가격표시통화.KRW => "원화(KRW)",
        농산물가격표시통화.CNY => "위안(CNY)",
        _ => "달러(USD)"
    };

    public string ExchangeRateReference
        => "ECB 기준환율 · 2026-07-22 · EUR 1 = KRW 1,688.78 / CNY 7.7266 / USD 1.1408";

    public IReadOnlyList<농산물지역가격비교항목> Items
    {
        get
        {
            var converted = Observations
            .Where(observation => observation.ProductKey == SelectedProductKey)
            .Where(observation => SelectedCountryCode == AllFilter
                || observation.CountryCode == SelectedCountryCode)
            .Where(observation => SelectedRegionCode == AllFilter
                || observation.RegionCode == SelectedRegionCode)
            .Select(observation =>
            {
                var nativeWeightPrice = observation.NativeKilogramPrice * ComparisonWeightGrams / 1000m;
                return new 농산물지역가격비교항목(
                    observation,
                    nativeWeightPrice,
                    Convert(nativeWeightPrice, observation.NativeCurrency, DisplayCurrency),
                    Convert(observation.NativeKilogramPrice, observation.NativeCurrency, DisplayCurrency),
                    0m,
                    0m);
            })
            .ToArray();

            if (converted.Length == 0)
            {
                return converted;
            }

            var lowest = converted.Min(item => item.DisplayWeightPrice);
            return converted
                .Select(item =>
                {
                    var difference = item.DisplayWeightPrice - lowest;
                    var percentage = lowest == 0m ? 0m : difference / lowest * 100m;
                    return item with
                    {
                        DisplayDifferenceFromLowest = difference,
                        DifferencePercentFromLowest = percentage
                    };
                })
                .OrderBy(item => item.DisplayWeightPrice)
                .ToArray();
        }
    }

    public string FormatDisplayPrice(decimal value)
        => DisplayCurrency switch
        {
            농산물가격표시통화.KRW => $"{decimal.Round(value, 0, MidpointRounding.AwayFromZero):N0}원",
            농산물가격표시통화.CNY => $"¥{value:N2}",
            _ => $"${value:N2}"
        };

    public static 농산물가격표시통화 ResolveDisplayCurrency(CultureInfo culture)
        => culture.TwoLetterISOLanguageName.ToLowerInvariant() switch
        {
            "ko" => 농산물가격표시통화.KRW,
            "zh" => 농산물가격표시통화.CNY,
            _ => 농산물가격표시통화.USD
        };

    protected override Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    private static 농산물지역가격관측값 UsdaRegion(
        string productKey,
        string regionCode,
        string regionName,
        string variety,
        decimal pricePerPound)
        => new(
            productKey,
            "US",
            "미국",
            regionCode,
            regionName,
            variety,
            pricePerPound,
            농산물가격표시통화.USD,
            "lb",
            pricePerPound * PoundsPerKilogram,
            "광고 소매가",
            "2026-07-24",
            "USDA AMS",
            "https://www.ams.usda.gov/mnreports/fvwretail.pdf",
            "주요 소매점 광고의 지역 가중평균이며 품종·광고 수에 따라 지역 대표성이 다릅니다.");

    private void ResetLocationFilters()
    {
        _selectedCountryCode = AllFilter;
        _selectedRegionCode = AllFilter;
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedProductKey));
        OnPropertyChanged(nameof(ComparisonWeightGrams));
        OnPropertyChanged(nameof(WeightLabel));
        OnPropertyChanged(nameof(AvailableProducts));
        OnPropertyChanged(nameof(SelectedProduct));
        OnPropertyChanged(nameof(SelectedCountryCode));
        OnPropertyChanged(nameof(SelectedRegionCode));
        OnPropertyChanged(nameof(AvailableCountries));
        OnPropertyChanged(nameof(AvailableRegions));
        OnPropertyChanged(nameof(Items));
    }

    private static decimal Convert(
        decimal value,
        농산물가격표시통화 source,
        농산물가격표시통화 target)
        => source == target
            ? value
            : value / UnitsPerEuro[source] * UnitsPerEuro[target];
}
