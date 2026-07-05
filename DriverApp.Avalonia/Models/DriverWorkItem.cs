namespace DriverApp.Avalonia.Models;

public sealed class DriverWorkItem
{
    public string WorkId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Note { get; set; } = string.Empty;
}
