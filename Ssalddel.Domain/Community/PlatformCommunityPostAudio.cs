namespace Ssalddel.Domain.Community;

public static class 커뮤니티게시글음성상태
{
    public const string 대기 = "대기";
    public const string 생성중 = "생성중";
    public const string 재시도대기 = "재시도대기";
    public const string 설정대기 = "설정대기";
    public const string 길이제외 = "길이제외";
    public const string 완료 = "완료";
    public const string 실패 = "실패";
}

public static class 커뮤니티게시글음성접근유형
{
    public const string 재생정보조회 = "재생정보조회";
    public const string 다운로드 = "다운로드";
}

public sealed class PlatformCommunityPostAudio
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public PlatformCommunityPost Post { get; set; } = null!;
    public string Status { get; set; } = 커뮤니티게시글음성상태.대기;
    public string Provider { get; set; } = "Typecast";
    public string VoiceId { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string AudioFormat { get; set; } = "wav";
    public int AttemptCount { get; set; }
    public string? ProcessingToken { get; set; }
    public string? LastError { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public ICollection<PlatformCommunityPostAudioSegment> Segments { get; set; } = new List<PlatformCommunityPostAudioSegment>();
    public ICollection<PlatformCommunityPostAudioAccessLog> AccessLogs { get; set; } = new List<PlatformCommunityPostAudioAccessLog>();
}

public sealed class PlatformCommunityPostAudioSegment
{
    public long Id { get; set; }
    public long AudioId { get; set; }
    public PlatformCommunityPostAudio Audio { get; set; } = null!;
    public int Sequence { get; set; }
    public int CharacterCount { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class PlatformCommunityPostAudioAccessLog
{
    public long Id { get; set; }
    public long AudioId { get; set; }
    public PlatformCommunityPostAudio Audio { get; set; } = null!;
    public long PostId { get; set; }
    public int? SegmentSequence { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public string? RequesterUserId { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime AccessedAtUtc { get; set; } = DateTime.UtcNow;
}
