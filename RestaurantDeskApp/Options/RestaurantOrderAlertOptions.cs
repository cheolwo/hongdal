namespace RestaurantDeskApp.Options;

public sealed class RestaurantOrderAlertOptions
{
    public const string SectionName = "RestaurantDesk:OrderAlert";

    public bool Enabled { get; set; } = true;

    public int RepeatCount { get; set; } = 3;

    public int IntervalMilliseconds { get; set; } = 650;

    public bool UseBeepTone { get; set; } = true;

    public int BeepFrequency { get; set; } = 1200;

    public int BeepDurationMilliseconds { get; set; } = 180;
}
