namespace Hongdal.Contracts.Driver.Settings;

public sealed class 기사콜범위수정요청
{
    public bool NationwideEnabled { get; set; }
}

public sealed class 기사콜범위응답
{
    public string DriverId { get; set; } = string.Empty;
    public bool NationwideEnabled { get; set; }
}