namespace Hongdal.Contracts.Common.Drivers;

public static class 기사앱식별자
{
    public const string CargoDriverApp = "CargoDriverApp";
    public const string DeliveryDriverApp = "DeliveryDriverApp";
}

public static class 기사도메인구분
{
    public const string 용달 = "Cargo";
    public const string 배달 = "Delivery";
}

public sealed class 기사앱라우트정의Dto
{
    public string AppId { get; set; } = string.Empty;
    public string 기사역할명 { get; set; } = string.Empty;
    public IReadOnlyList<string> 기본라우트목록 { get; set; } = [];
}
