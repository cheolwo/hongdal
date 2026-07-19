namespace Ssalddel.FoodApi.Contracts;

public sealed class 배차주소저장응답
{
    public string 메시지 { get; set; } = string.Empty;

    public double? 상차지위도 { get; set; }
    public double? 상차지경도 { get; set; }
    public double? 하차지위도 { get; set; }
    public double? 하차지경도 { get; set; }
}
