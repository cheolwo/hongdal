using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum 국내가격비교단위
{
    그램,
    킬로그램,
    개수
}

public sealed record 국내유통가격비교항목(
    string StageCode,
    string StageLabel,
    string Description,
    bool IsAvailable,
    decimal? DisplayPriceKrw,
    decimal? PriceKrwPerKg,
    decimal? DifferenceFromLowestKrw,
    decimal? DifferencePercentFromLowest,
    string ReferenceDate,
    string AvailabilityNote,
    string SourceUrl);

public sealed class 국내농수산가격조회ViewModel(
    I농수산공공데이터Client dataClient) : 농수산가격조회ViewModelBase
{
    public const string KamisAuctionSourceUrl =
        "https://www.kamis.or.kr/customer/price/market/period.do";

    private IReadOnlyList<AgriculturalFisheriesItemResponse> _품목 = 농수산가격기본Catalog.국내품목;
    private string _선택HsCode = "080810";
    private AgriculturalFisheriesDomesticPriceResponse? _응답;
    private 국내가격비교단위 _비교단위 = 국내가격비교단위.그램;
    private decimal _비교그램 = 100m;
    private decimal _대표개당그램 = 250m;

    public IReadOnlyList<AgriculturalFisheriesItemResponse> 품목
    {
        get => _품목;
        private set => SetProperty(ref _품목, value);
    }

    public string 선택HsCode
    {
        get => _선택HsCode;
        set
        {
            if (SetProperty(ref _선택HsCode, value))
            {
                OnPropertyChanged(nameof(선택품목명));
                OnPropertyChanged(nameof(비교항목));
            }
        }
    }

    public AgriculturalFisheriesDomesticPriceResponse? 응답
    {
        get => _응답;
        private set
        {
            if (SetProperty(ref _응답, value))
            {
                OnPropertyChanged(nameof(비교항목));
            }
        }
    }

    public 국내가격비교단위 비교단위
    {
        get => _비교단위;
        set
        {
            if (SetProperty(ref _비교단위, value))
            {
                OnPropertyChanged(nameof(비교기준Label));
                OnPropertyChanged(nameof(비교항목));
                OnPropertyChanged(nameof(개수환산안내));
            }
        }
    }

    public decimal 비교그램
    {
        get => _비교그램;
        set
        {
            var normalized = Math.Clamp(value, 1m, 10_000m);
            if (SetProperty(ref _비교그램, normalized))
            {
                OnPropertyChanged(nameof(비교기준Label));
                OnPropertyChanged(nameof(비교항목));
            }
        }
    }

    public decimal 대표개당그램
    {
        get => _대표개당그램;
        set
        {
            var normalized = Math.Clamp(value, 1m, 10_000m);
            if (SetProperty(ref _대표개당그램, normalized))
            {
                OnPropertyChanged(nameof(비교기준Label));
                OnPropertyChanged(nameof(비교항목));
                OnPropertyChanged(nameof(개수환산안내));
            }
        }
    }

    public string 선택품목명
        => 품목.FirstOrDefault(item => item.HsPrefix == 선택HsCode)?.ProductName
           ?? 선택HsCode;

    public string 비교기준Label => 비교단위 switch
    {
        국내가격비교단위.킬로그램 => "1kg",
        국내가격비교단위.개수 => $"1개 · 대표 {대표개당그램:N0}g",
        _ => $"{비교그램:N0}g"
    };

    public string? 개수환산안내
        => 비교단위 == 국내가격비교단위.개수
            ? $"개수당 가격은 KAMIS kg 환산값에 사용자가 지정한 대표 중량 {대표개당그램:N0}g을 적용한 추정값입니다."
            : null;

    public IReadOnlyList<국내유통가격비교항목> 비교항목
    {
        get
        {
            var response = 응답;
            if (response is null || !response.Success || response.Price is null)
            {
                return [];
            }

            var candidates = new[]
            {
                BuildUnavailableAuction(),
                BuildAvailableStage(
                    "Wholesale",
                    "중도매가",
                    "도매시장 중도매인 상회가 소상인·실수요자에게 판매하는 조사 가격",
                    response.Price.Wholesale),
                BuildAvailableStage(
                    "Retail",
                    "소매가",
                    "대형마트·전통시장 등에서 소비자에게 판매하는 조사 가격",
                    response.Price.Retail)
            };
            var lowest = candidates
                .Where(item => item.DisplayPriceKrw is > 0)
                .Min(item => item.DisplayPriceKrw);

            return candidates
                .Select(item =>
                {
                    if (lowest is not > 0 || item.DisplayPriceKrw is not > 0)
                    {
                        return item;
                    }

                    var difference = item.DisplayPriceKrw.Value - lowest.Value;
                    return item with
                    {
                        DifferenceFromLowestKrw = difference,
                        DifferencePercentFromLowest = difference / lowest.Value * 100m
                    };
                })
                .ToArray();
        }
    }

    public Task<bool> 품목초기화Async(CancellationToken cancellationToken = default)
        => 농수산공공데이터호출정책.초기화시도Async(
            async token =>
            {
                var catalog = await dataClient.국내품목조회Async(cancellationToken: token);
                품목 = catalog.Items
                    .Concat(농수산가격기본Catalog.국내품목)
                    .GroupBy(item => item.HsPrefix, StringComparer.Ordinal)
                    .Select(group => group.First())
                    .OrderBy(item => item.CategoryLabel, StringComparer.Ordinal)
                    .ThenBy(item => item.ProductName, StringComparer.Ordinal)
                    .ToArray();
                OnPropertyChanged(nameof(선택품목명));
            },
            cancellationToken);

    public Task 조회Async(CancellationToken cancellationToken = default)
        => 조회실행Async(
            async token =>
            {
                응답 = null;
                응답 = await dataClient.국내가격조회Async(선택HsCode, cancellationToken: token);
            },
            "한국 가격 API에 연결하지 못했습니다.",
            cancellationToken);

    private 국내유통가격비교항목 BuildUnavailableAuction()
        => new(
            "Auction",
            "경락가",
            "가락시장 등 도매시장에서 거래가 성립된 평균 경락 가격",
            false,
            null,
            null,
            null,
            null,
            string.Empty,
            "KAMIS 화면에는 공개되지만 현재 공식 Open API 목록에는 경락가 조회 API가 없어 값을 합산하지 않습니다.",
            KamisAuctionSourceUrl);

    private 국내유통가격비교항목 BuildAvailableStage(
        string stageCode,
        string stageLabel,
        string description,
        Ssalddel.Contracts.Common.PublicData.AtDomesticFoodPriceAggregate? aggregate)
    {
        if (aggregate is null || aggregate.AverageKrwPerKg <= 0)
        {
            return new 국내유통가격비교항목(
                stageCode,
                stageLabel,
                description,
                false,
                null,
                null,
                null,
                null,
                string.Empty,
                "선택 품목의 최근 KAMIS 조사값이 없습니다.",
                "https://www.kamis.or.kr/customer/reference/openapi_list.do");
        }

        var displayPrice = aggregate.AverageKrwPerKg * ComparisonKilogramFactor();
        return new 국내유통가격비교항목(
            stageCode,
            stageLabel,
            description,
            true,
            decimal.Round(displayPrice, 0, MidpointRounding.AwayFromZero),
            aggregate.AverageKrwPerKg,
            null,
            null,
            aggregate.LatestSurveyDate,
            $"{aggregate.SampleCount:N0}개 최근 조사값 평균",
            "https://www.kamis.or.kr/customer/reference/openapi_list.do");
    }

    private decimal ComparisonKilogramFactor()
        => 비교단위 switch
        {
            국내가격비교단위.킬로그램 => 1m,
            국내가격비교단위.개수 => 대표개당그램 / 1_000m,
            _ => 비교그램 / 1_000m
        };
}
