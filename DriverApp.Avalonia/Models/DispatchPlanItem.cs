namespace DriverApp.Avalonia.Models;

public sealed class DispatchPlanItem
{
    public string PlanId { get; set; } = string.Empty;
    public string StartPoint { get; set; } = string.Empty;
    public string ReturnPoint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
