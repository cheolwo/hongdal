namespace Ssalddel.Domain.PublicData.Korea;

public static class 건축물형태분석Engine
{
    public const string 규칙개정번호 = "kr-building-massing-v1";

    public static 건축물형태분석결과 분석(
        건축물대장표제부Record 건축물,
        string 용도CategoryCode)
    {
        ArgumentNullException.ThrowIfNull(건축물);

        var 추정층고 = 추정층고계산(용도CategoryCode);
        var 관측층수 = Positive(건축물.AboveGroundFloorCount);
        int? 추정층수 = 관측층수 is null && 건축물.HeightMeters is > 0
            ? Math.Clamp((int)Math.Round(
                건축물.HeightMeters.Value / 추정층고,
                MidpointRounding.AwayFromZero), 1, 60)
            : null;
        var 표현층수 = 관측층수 ?? 추정층수 ?? 1;
        var 단순건폐비율 = Ratio(건축물.BuildingAreaSquareMeters, 건축물.SiteAreaSquareMeters);
        var 단순연면적대지비율 = Ratio(건축물.TotalFloorAreaSquareMeters, 건축물.SiteAreaSquareMeters);
        var 밀도기준 = 건축물.OfficialFloorAreaRatioPercent ?? 단순연면적대지비율;
        var 근거 = 관측층수 is not null
            ? 건축물형태근거종류Codes.관측값우선
            : 추정층수 is not null
                ? 건축물형태근거종류Codes.일부추정
                : 건축물형태근거종류Codes.자료부족;

        return new 건축물형태분석결과(
            관측층수,
            추정층수,
            표현층수,
            건축물.OfficialBuildingCoveragePercent,
            건축물.OfficialFloorAreaRatioPercent,
            단순건폐비율,
            단순연면적대지비율,
            추정층고,
            바닥면적등급(건축물.BuildingAreaSquareMeters),
            높이등급(표현층수),
            밀도등급(밀도기준),
            근거);
    }

    public static 건축물시각구성결과 시각구성(
        건축물형태분석결과 형태,
        string 용도CategoryCode)
    {
        ArgumentNullException.ThrowIfNull(형태);
        var 건폐기준 = 형태.공식건폐율Percent ?? 형태.단순건폐비율Percent;
        var 점유등급 = 건폐기준 switch
        {
            null => "unknown",
            <= 20m => "low",
            <= 50m => "medium",
            _ => "high",
        };
        var 여백등급 = 점유등급 switch
        {
            "low" => "wide",
            "medium" => "standard",
            "high" => "compact",
            _ => "unknown",
        };

        return new 건축물시각구성결과(
            시각Family(용도CategoryCode, 형태.표현지상층수),
            형태.표현지상층수,
            Math.Max(0, 형태.표현지상층수 - 2),
            점유등급,
            여백등급,
            형태.표현지상층수 >= 6 ? "region-and-task" : "task");
    }

    private static decimal 추정층고계산(string categoryCode) => categoryCode switch
    {
        건축물용도CategoryCodes.Agriculture => 4.5m,
        건축물용도CategoryCodes.LogisticsStorage => 5m,
        건축물용도CategoryCodes.Industrial => 4.5m,
        건축물용도CategoryCodes.Commercial => 3.6m,
        건축물용도CategoryCodes.BusinessOffice => 3.6m,
        _ => 3m,
    };

    private static string 시각Family(string categoryCode, int floors) => categoryCode switch
    {
        건축물용도CategoryCodes.Agriculture => "farm-lowrise",
        건축물용도CategoryCodes.LogisticsStorage => "hub-warehouse",
        건축물용도CategoryCodes.Industrial => "industrial-building",
        _ when floors >= 6 => "city-midrise",
        건축물용도CategoryCodes.Commercial or 건축물용도CategoryCodes.BusinessOffice => "town-commercial",
        _ => "town-lowrise",
    };

    private static int? Positive(int? value) => value is > 0 ? value : null;

    private static decimal? Ratio(decimal? numerator, decimal? denominator) =>
        numerator is >= 0 && denominator is > 0
            ? decimal.Round(numerator.Value / denominator.Value * 100m, 4)
            : null;

    private static string 바닥면적등급(decimal? area) => area switch
    {
        null => "unknown",
        < 100m => "small",
        < 500m => "medium",
        < 2_000m => "large",
        _ => "very-large",
    };

    private static string 높이등급(int floors) => floors switch
    {
        <= 2 => "lowrise",
        <= 5 => "mid-lowrise",
        <= 15 => "midrise",
        _ => "highrise",
    };

    private static string 밀도등급(decimal? ratio) => ratio switch
    {
        null => "unknown",
        < 100m => "low",
        < 250m => "medium",
        _ => "high",
    };
}
