namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed record PlatformCommunityPostDraft(
    string Category,
    string WorkflowTag,
    string Title,
    string Body,
    string SharedLinkUrl,
    string SourceLabel);

public sealed class PlatformCommunityPostDraftStateService
{
    private PlatformCommunityPostDraft? _pendingDraft;

    public void Prepare(PlatformCommunityPostDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        _pendingDraft = draft;
    }

    public PlatformCommunityPostDraft? Consume()
    {
        var draft = _pendingDraft;
        _pendingDraft = null;
        return draft;
    }
}

public static class PrajnaLectureCommunityShareDraftFactory
{
    public static PlatformCommunityPostDraft Create(
        string channelName,
        string playlistTitle,
        string videoTitle,
        string videoUrl,
        string quote,
        string? reflection,
        string? timestamp)
    {
        if (string.IsNullOrWhiteSpace(quote))
        {
            throw new ArgumentException("공유할 글귀를 입력해야 합니다.", nameof(quote));
        }

        if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out var sourceUri) ||
            (sourceUri.Scheme != Uri.UriSchemeHttp && sourceUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("영상 주소가 올바르지 않습니다.", nameof(videoUrl));
        }

        var normalizedTimestamp = NormalizeTimestamp(timestamp, out var seconds);
        var sharedLinkUrl = seconds is null ? sourceUri.ToString() : AppendTimestamp(sourceUri, seconds.Value);
        var bodyLines = new List<string>
        {
            $"“{quote.Trim()}”"
        };

        if (!string.IsNullOrWhiteSpace(reflection))
        {
            bodyLines.Add(string.Empty);
            bodyLines.Add(reflection.Trim());
        }

        bodyLines.Add(string.Empty);
        bodyLines.Add("---");
        bodyLines.Add($"함께 본 강의: {videoTitle.Trim()}");
        bodyLines.Add($"재생목록: {playlistTitle.Trim()}");
        bodyLines.Add($"출처: {channelName.Trim()}");
        if (normalizedTimestamp is not null)
        {
            bodyLines.Add($"영상 위치: {normalizedTimestamp}");
        }

        var titlePrefix = "[반야 나눔] ";
        var availableTitleLength = 160 - titlePrefix.Length;
        var normalizedVideoTitle = videoTitle.Trim();
        var title = titlePrefix + (normalizedVideoTitle.Length <= availableTitleLength
            ? normalizedVideoTitle
            : normalizedVideoTitle[..availableTitleLength]);

        return new PlatformCommunityPostDraft(
            "자유",
            "커뮤니티 신뢰",
            title,
            string.Join(Environment.NewLine, bodyLines),
            sharedLinkUrl,
            $"{channelName.Trim()} · {playlistTitle.Trim()}");
    }

    private static string? NormalizeTimestamp(string? value, out int? totalSeconds)
    {
        totalSeconds = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Trim().Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 3 || parts.Any(part => !int.TryParse(part, out _)))
        {
            throw new ArgumentException("영상 위치는 12:34 또는 1:02:03 형식으로 입력해 주세요.", nameof(value));
        }

        var values = parts.Select(int.Parse).ToArray();
        if (values.Any(part => part < 0) ||
            (values.Length >= 2 && values[^1] >= 60) ||
            (values.Length == 3 && values[^2] >= 60))
        {
            throw new ArgumentException("영상 위치의 분과 초는 0부터 59 사이여야 합니다.", nameof(value));
        }

        totalSeconds = values.Length switch
        {
            1 => values[0],
            2 => checked((values[0] * 60) + values[1]),
            _ => checked((values[0] * 3600) + (values[1] * 60) + values[2])
        };

        return values.Length switch
        {
            1 => $"{totalSeconds.Value / 60}:{totalSeconds.Value % 60:00}",
            2 => $"{values[0]}:{values[1]:00}",
            _ => $"{values[0]}:{values[1]:00}:{values[2]:00}"
        };
    }

    private static string AppendTimestamp(Uri sourceUri, int totalSeconds)
    {
        var builder = new UriBuilder(sourceUri);
        var query = builder.Query.TrimStart('?');
        builder.Query = string.IsNullOrWhiteSpace(query)
            ? $"t={totalSeconds}s"
            : $"{query}&t={totalSeconds}s";
        return builder.Uri.ToString();
    }
}
