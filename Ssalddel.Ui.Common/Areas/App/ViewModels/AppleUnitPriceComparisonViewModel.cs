using System.Globalization;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum 사과가격표시통화
{
    KRW,
    CNY,
    USD
}

public sealed record 사과가격관측값(
    string CountryCode,
    string CountryName,
    string Variety,
    decimal NativePrice,
    사과가격표시통화 NativeCurrency,
    string NativeUnit,
    decimal NativeKilogramPrice,
    string MarketStage,
    string Region,
    string ReferenceDate,
    string SourceName,
    string SourceUrl,
    string Limitation);

public sealed record 사과한개가격비교항목(
    사과가격관측값 Observation,
    decimal NativeApplePrice,
    decimal DisplayApplePrice,
    decimal DisplayKilogramPrice);

public sealed class 사과한개가격비교ViewModel : PageViewModelBase
{
    private const decimal PoundsPerKilogram = 2.2046226218m;

    private static readonly IReadOnlyDictionary<사과가격표시통화, decimal> UnitsPerEuro =
        new Dictionary<사과가격표시통화, decimal>
        {
            [사과가격표시통화.KRW] = 1688.78m,
            [사과가격표시통화.CNY] = 7.7266m,
            [사과가격표시통화.USD] = 1.1408m
        };

    private static readonly IReadOnlyList<사과가격관측값> Observations =
    [
        new(
            "KR",
            "한국",
            "후지 · 상품",
            25_808m,
            사과가격표시통화.KRW,
            "10개",
            25_808m / 10m * 4m,
            "소매 조사값",
            "전국",
            "2026-05-21",
            "aT KAMIS",
            "https://www.kamis.or.kr/customer/info/retail/period.do?action=daily&countycode=&itemcategorycode=400&itemcode=411&kindcode=&marketclscode=&productrankcode=&regday=2026.05.21",
            "10개 묶음에 실제 중량이 없으므로 선택한 한 개 중량과 같다고 가정합니다."),
        new(
            "US",
            "미국",
            "Fuji",
            1.99m,
            사과가격표시통화.USD,
            "lb",
            1.99m * PoundsPerKilogram,
            "광고 소매가",
            "South Central U.S.",
            "2026-07-17",
            "USDA AMS",
            "https://www.ams.usda.gov/mnreports/fvwretail.pdf",
            "주요 소매점 광고의 지역 관측값이며 미국 전체 평균이나 실제 결제가는 아닙니다."),
        new(
            "CN",
            "중국",
            "红富士苹果",
            6m,
            사과가격표시통화.CNY,
            "kg",
            6m,
            "산지·도매 관측값",
            "山东省临沂市蒙阴县 → 江苏省无锡市惠山区",
            "2026-07-21",
            "中国农业农村部 重点农产品市场信息平台",
            "https://ncpscxx.moa.gov.cn/",
            "개별 유통 경로의 관측값이며 중국 전체 평균이나 소비자 소매가가 아닙니다.")
    ];

    private decimal _appleWeightGrams = 250m;
    private 사과가격표시통화 _displayCurrency;

    public 사과한개가격비교ViewModel()
        : this(CultureInfo.CurrentUICulture)
    {
    }

    public 사과한개가격비교ViewModel(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        _displayCurrency = ResolveDisplayCurrency(culture);
    }

    public decimal AppleWeightGrams
    {
        get => _appleWeightGrams;
        set
        {
            var normalized = Math.Clamp(value, 100m, 500m);
            if (SetProperty(ref _appleWeightGrams, normalized))
            {
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(WeightLabel));
            }
        }
    }

    public 사과가격표시통화 DisplayCurrency
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

    public string WeightLabel => $"{AppleWeightGrams:0}g";

    public string DisplayCurrencyLabel => DisplayCurrency switch
    {
        사과가격표시통화.KRW => "원화(KRW)",
        사과가격표시통화.CNY => "위안(CNY)",
        _ => "달러(USD)"
    };

    public string ExchangeRateReference
        => "ECB 기준환율 · 2026-07-22 · EUR 1 = KRW 1,688.78 / CNY 7.7266 / USD 1.1408";

    public IReadOnlyList<사과한개가격비교항목> Items
        => Observations
            .Select(observation =>
            {
                var nativeApplePrice = observation.NativeKilogramPrice * AppleWeightGrams / 1000m;
                return new 사과한개가격비교항목(
                    observation,
                    nativeApplePrice,
                    Convert(nativeApplePrice, observation.NativeCurrency, DisplayCurrency),
                    Convert(observation.NativeKilogramPrice, observation.NativeCurrency, DisplayCurrency));
            })
            .ToArray();

    public string FormatDisplayPrice(decimal value)
        => DisplayCurrency switch
        {
            사과가격표시통화.KRW => $"{decimal.Round(value, 0, MidpointRounding.AwayFromZero):N0}원",
            사과가격표시통화.CNY => $"¥{value:N2}",
            _ => $"${value:N2}"
        };

    public static 사과가격표시통화 ResolveDisplayCurrency(CultureInfo culture)
        => culture.TwoLetterISOLanguageName.ToLowerInvariant() switch
        {
            "ko" => 사과가격표시통화.KRW,
            "zh" => 사과가격표시통화.CNY,
            _ => 사과가격표시통화.USD
        };

    protected override Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    private static decimal Convert(
        decimal value,
        사과가격표시통화 source,
        사과가격표시통화 target)
        => source == target
            ? value
            : value / UnitsPerEuro[source] * UnitsPerEuro[target];
}
