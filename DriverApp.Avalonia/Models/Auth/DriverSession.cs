namespace DriverApp.Avalonia.Models.Auth;

public sealed class DriverSession
{
    public bool IsLoggedIn { get; set; }
    public string DriverId { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}
