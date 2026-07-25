using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum 미국가격비교단위
{
    온스,
    파운드,
    개수
}

public sealed record 미국국내가격비교항목(
    미국농수산가격항목 Observation,
    decimal? PriceUsdPerPound,
    decimal? DisplayPriceUsd,
    decimal? DifferenceFromLowestUsd,
    decimal? DifferencePercentFromLowest,
    string ConversionNote)
{
    public bool IsWeightConvertible => PriceUsdPerPound.HasValue;
}

public sealed class 미국농수산가격조회ViewModel(
    I농수산공공데이터Client dataClient) : 농수산가격조회ViewModelBase
{
    public const string UsdaNassSourceUrl = "https://quickstats.nass.usda.gov/";

    public const string UsdaAmsRetailSourceUrl = "https://www.ams.usda.gov/market-news/grocerystore";

    public static IReadOnlyList<string> 품목예시 => 농수산가격기본Catalog.미국품목예시;

    private string _품목명 = "APPLES";
    private string _조사Program = "SURVEY";
    private int _시작연도 = DateTime.UtcNow.Year - 3;
    private int _종료연도 = DateTime.UtcNow.Year;
    private 미국가격비교단위 _비교단위 = 미국가격비교단위.파운드;
    private decimal _대표개당온스 = 8m;
    private 미국농수산가격조회응답? _응답;

    public string 품목명
    {
        get => _품목명;
        set
        {
            if (SetProperty(ref _품목명, value))
            {
                OnPropertyChanged(nameof(정규화품목명));
            }
        }
    }

    public string 조사Program
    {
        get => _조사Program;
        set => SetProperty(ref _조사Program, value);
    }

    public int 시작연도
    {
        get => _시작연도;
        set => SetProperty(ref _시작연도, value);
    }

    public int 종료연도
    {
        get => _종료연도;
        set => SetProperty(ref _종료연도, value);
    }

    public 미국가격비교단위 비교단위
    {
        get => _비교단위;
        set
        {
            if (SetProperty(ref _비교단위, value))
            {
                가격환산상태변경();
            }
        }
    }

    public decimal 대표개당온스
    {
        get => _대표개당온스;
        set
        {
            var normalized = Math.Clamp(value, 0.1m, 320m);
            if (SetProperty(ref _대표개당온스, normalized))
            {
                가격환산상태변경();
            }
        }
    }

    public 미국농수산가격조회응답? 응답
    {
        get => _응답;
        private set
        {
            if (SetProperty(ref _응답, value))
            {
                가격환산상태변경();
            }
        }
    }

    public string 정규화품목명
        => string.IsNullOrWhiteSpace(품목명)
            ? "품목 미선택"
            : 품목명.Trim().ToUpperInvariant();

    public string 비교기준Label => 비교단위 switch
    {
        미국가격비교단위.온스 => "1 oz",
        미국가격비교단위.파운드 => "1 lb",
        _ => $"1개 · 대표 {대표개당온스:0.#} oz"
    };

    public string? 개수환산안내
        => 비교단위 == 미국가격비교단위.개수
            ? $"개수 가격은 대표 1개 중량을 {대표개당온스:0.#} oz로 지정해 환산한 추정값입니다."
            : null;

    public IReadOnlyList<미국국내가격비교항목> 비교항목
    {
        get
        {
            var converted = (응답?.Items ?? [])
                .OrderByDescending(item => ParseYear(item.Year))
                .ThenBy(item => item.StateName, StringComparer.Ordinal)
                .ThenBy(item => item.Class, StringComparer.Ordinal)
                .Take(24)
                .Select(item =>
                {
                    var perPound = ConvertToUsdPerPound(item);
                    return new
                    {
                        Observation = item,
                        PriceUsdPerPound = perPound,
                        DisplayPriceUsd = ConvertToSelectedBasis(perPound),
                        ConversionNote = ResolveConversionNote(item.Unit, perPound)
                    };
                })
                .ToArray();

            var lowest = converted
                .Where(item => item.DisplayPriceUsd.HasValue)
                .Select(item => item.DisplayPriceUsd!.Value)
                .DefaultIfEmpty()
                .Min();

            return converted
                .Select(item =>
                {
                    decimal? difference = item.DisplayPriceUsd.HasValue
                        ? item.DisplayPriceUsd.Value - lowest
                        : null;
                    decimal? differencePercent = difference.HasValue && lowest > 0m
                        ? difference.Value / lowest * 100m
                        : null;

                    return new 미국국내가격비교항목(
                        item.Observation,
                        item.PriceUsdPerPound,
                        item.DisplayPriceUsd,
                        difference,
                        differencePercent,
                        item.ConversionNote);
                })
                .ToArray();
        }
    }

    public Task 조회Async(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(품목명))
        {
            응답 = null;
            오류메시지 = "미국 공식 품목명을 입력해 주세요.";
            return Task.CompletedTask;
        }

        return 조회실행Async(
            async token =>
            {
                응답 = null;
                응답 = await dataClient.미국가격조회Async(
                    정규화품목명,
                    조사Program,
                    시작연도,
                    종료연도,
                    cancellationToken: token);
            },
            "미국 가격 API에 연결하지 못했습니다.",
            cancellationToken);
    }

    private decimal? ConvertToSelectedBasis(decimal? priceUsdPerPound)
    {
        if (!priceUsdPerPound.HasValue)
        {
            return null;
        }

        return 비교단위 switch
        {
            미국가격비교단위.온스 => priceUsdPerPound.Value / 16m,
            미국가격비교단위.파운드 => priceUsdPerPound.Value,
            _ => priceUsdPerPound.Value * 대표개당온스 / 16m
        };
    }

    private static decimal? ConvertToUsdPerPound(미국농수산가격항목 item)
    {
        if (!item.NumericValue.HasValue)
        {
            return null;
        }

        var unit = NormalizeUnit(item.Unit);

        if (unit.Contains("/ CWT", StringComparison.Ordinal)
            || unit.Contains("/ 100 LB", StringComparison.Ordinal))
        {
            return item.NumericValue.Value / 100m;
        }

        if (unit.Contains("/ LB", StringComparison.Ordinal)
            || unit.Contains("/ POUND", StringComparison.Ordinal))
        {
            return item.NumericValue.Value;
        }

        if (unit.Contains("/ TON", StringComparison.Ordinal))
        {
            return item.NumericValue.Value / 2_000m;
        }

        return null;
    }

    private static string ResolveConversionNote(string unit, decimal? priceUsdPerPound)
    {
        if (priceUsdPerPound.HasValue)
        {
            return "US customary weight 기준으로 환산";
        }

        return NormalizeUnit(unit) switch
        {
            var value when value.Contains("BUSHEL", StringComparison.Ordinal)
                || value.Contains("/ BU", StringComparison.Ordinal)
                => "bushel은 품목별 표준 중량이 달라 원문 단위를 유지",
            var value when value.Contains("DOZEN", StringComparison.Ordinal)
                => "dozen은 개수 묶음 단위이므로 중량 환산에서 제외",
            _ => "중량 환산 규칙이 없는 USDA 원문 단위"
        };
    }

    private static string NormalizeUnit(string value)
        => value.Trim().ToUpperInvariant();

    private static int ParseYear(string value)
        => int.TryParse(value, out var year) ? year : 0;

    private void 가격환산상태변경()
    {
        OnPropertyChanged(nameof(비교기준Label));
        OnPropertyChanged(nameof(개수환산안내));
        OnPropertyChanged(nameof(비교항목));
    }
}
