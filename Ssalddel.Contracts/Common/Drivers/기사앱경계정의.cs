namespace Ssalddel.Contracts.Common.Drivers;

public static class 기사앱식별자
{
    public const string CargoYongdalDriverApp = "CargoYongdalDriverApp";
    public const string FoodDeliveryDriverApp = "FoodDeliveryDriverApp";
}

public static class 기사도메인구분
{
    public const string 화물용달 = "CargoYongdal";
    public const string 음식배달 = "FoodDelivery";
}

public static class 기사업무유형코드
{
    public const string 화물운송 = "CargoTransport";
    public const string 용달운송 = "YongdalTransport";
    public const string 음식배달 = "FoodDelivery";

    public static string Normalize(string? value)
        => value?.Trim() switch
        {
            화물운송 => 화물운송,
            용달운송 => 용달운송,
            음식배달 => 음식배달,
            _ => 용달운송
        };

    public static string GetDisplayName(string? value)
        => Normalize(value) switch
        {
            화물운송 => "화물 기사",
            음식배달 => "음식 배달 기사",
            _ => "용달 기사"
        };
}

public sealed class 기사앱라우트정의Dto
{
    public string AppId { get; set; } = string.Empty;
    public string 기사역할명 { get; set; } = string.Empty;
    public string 기사도메인 { get; set; } = string.Empty;
    public string 기본업무유형 { get; set; } = string.Empty;
    public IReadOnlyList<string> 기본라우트목록 { get; set; } = [];
}
