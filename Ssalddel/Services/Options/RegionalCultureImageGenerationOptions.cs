namespace 살뜰.Services.Options;

public sealed class RegionalCultureImageGenerationOptions
{
    public const string SectionName = "RegionalCultureImageGeneration";

    public bool Enabled { get; set; }

    public int TargetImagesPerRegion { get; set; } = 10;

    public int MaxNewJobsPerCycle { get; set; } = 1;

    public int MaxDailySubmissions { get; set; } = 10;

    public int IntervalMinutes { get; set; } = 5;

    public string CountryOrder { get; set; } = "KR,US,CN";

    public string AspectRatio { get; set; } = "16:9";

    public string Resolution { get; set; } = "1K";
}
