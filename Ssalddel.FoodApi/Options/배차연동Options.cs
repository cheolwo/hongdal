namespace Ssalddel.FoodApi.Options;

public sealed class 배차연동Options
{
    public const string SectionName = "DispatchIntegration";

    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string DispatchWaitPath { get; set; } = "/api/v1/dispatch/wait";
}
