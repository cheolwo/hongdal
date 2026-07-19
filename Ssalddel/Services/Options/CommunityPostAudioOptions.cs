namespace 살뜰.Services.Options;

public sealed class CommunityPostAudioOptions
{
    public const string SectionName = "CommunityPostAudio";

    public bool Enabled { get; set; }
    public string DefaultVoiceId { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = "ssfm-v30";
    public string LanguageCode { get; set; } = "kor";
    public string AudioFormat { get; set; } = "wav";
    public string StorageFolder { get; set; } = "community/posts/audio";
    public int MinCharacters { get; set; } = 100;
    public int MaxCharactersExclusive { get; set; } = 500;
    public int MaxCharactersPerSegment { get; set; } = 1900;
    public int PollingIntervalSeconds { get; set; } = 10;
    public int BatchSize { get; set; } = 10;
    public int MaxAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
    public int LeaseTimeoutMinutes { get; set; } = 5;
}
