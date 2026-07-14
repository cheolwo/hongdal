namespace Hongdal.Domain.Content;

public sealed class YouTube감시채널
{
    public long Id { get; set; }

    public string ChannelId { get; set; } = string.Empty;

    public string 채널명 { get; set; } = string.Empty;

    public string UploadsPlaylistId { get; set; } = string.Empty;

    public string? 썸네일Url { get; set; }

    public bool 활성화여부 { get; set; } = true;

    public bool 초기동기화완료여부 { get; set; }

    public DateTime? 마지막동기화일시Utc { get; set; }

    public string? 마지막영상Id { get; set; }

    public DateTime? 마지막영상게시일시Utc { get; set; }

    public DateTime 생성일시Utc { get; set; } = DateTime.UtcNow;

    public DateTime 수정일시Utc { get; set; } = DateTime.UtcNow;

    public ICollection<YouTube채널영상> 영상 { get; set; } = new List<YouTube채널영상>();
}

public sealed class YouTube채널영상
{
    public const string 기준선공유상태 = "기준선";
    public const string 공유대기상태 = "공유대기";
    public const string 공개상태 = "공개";
    public const string 숨김상태 = "숨김";

    public long Id { get; set; }

    public long YouTube감시채널Id { get; set; }

    public YouTube감시채널? 감시채널 { get; set; }

    public string VideoId { get; set; } = string.Empty;

    public string ChannelId { get; set; } = string.Empty;

    public string 제목 { get; set; } = string.Empty;

    public string 설명 { get; set; } = string.Empty;

    public DateTime 게시일시Utc { get; set; }

    public string? 썸네일Url { get; set; }

    public bool 신규업로드여부 { get; set; }

    public string 공유상태 { get; set; } = 기준선공유상태;

    public DateTime 최초감지일시Utc { get; set; } = DateTime.UtcNow;
}
