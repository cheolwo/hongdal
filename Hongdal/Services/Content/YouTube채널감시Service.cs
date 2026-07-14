using Hongdal.Contracts.Common.Content;
using Hongdal.Domain.Content;
using Hongdal.Services.External.YouTube;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.Content;

public interface IYouTube채널감시Service
{
    Task<YouTube감시채널Dto> 채널등록Async(
        YouTube감시채널등록요청Dto 요청,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<YouTube감시채널Dto>> 채널목록조회Async(CancellationToken cancellationToken);

    Task<IReadOnlyList<YouTube채널영상Dto>> 영상목록조회Async(
        string? channelId,
        bool 신규업로드만,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<YouTube채널영상Dto>> 공개영상목록조회Async(
        string? channelId,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<YouTube재생목록Dto>> 재생목록목록조회Async(
        string channelId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<YouTube재생목록영상Dto>> 재생목록영상목록조회Async(
        string playlistId,
        int take,
        CancellationToken cancellationToken);

    Task<YouTube채널영상Dto> 영상공개설정Async(
        string videoId,
        bool 공개여부,
        CancellationToken cancellationToken);

    Task<YouTube채널동기화결과Dto> 동기화Async(
        string? channelId,
        CancellationToken cancellationToken);
}

public sealed class YouTube채널감시Service : IYouTube채널감시Service
{
    private readonly IYouTubeDataApiClient _client;
    private readonly IYouTube채널감시저장소 _저장소;
    private readonly YouTubeOptions _options;

    public YouTube채널감시Service(
        IYouTubeDataApiClient client,
        IYouTube채널감시저장소 저장소,
        IOptions<YouTubeOptions> options)
    {
        _client = client;
        _저장소 = 저장소;
        _options = options.Value;
    }

    public async Task<YouTube감시채널Dto> 채널등록Async(
        YouTube감시채널등록요청Dto 요청,
        CancellationToken cancellationToken)
    {
        var channelId = NormalizeId(요청.ChannelId);
        if (await _저장소.추적조회Async(channelId, cancellationToken) is not null)
        {
            throw new InvalidOperationException("이미 감시 중인 YouTube 채널입니다.");
        }

        var 채널 = await 원격채널생성Async(channelId, 요청.표시이름, cancellationToken);

        _저장소.채널추가(채널);
        await _저장소.저장Async(cancellationToken);
        return ToChannelDto(채널);
    }

    public async Task<IReadOnlyList<YouTube감시채널Dto>> 채널목록조회Async(
        CancellationToken cancellationToken)
        => (await _저장소.채널목록조회Async(cancellationToken))
            .Select(ToChannelDto)
            .ToArray();

    public async Task<IReadOnlyList<YouTube채널영상Dto>> 영상목록조회Async(
        string? channelId,
        bool 신규업로드만,
        int take,
        CancellationToken cancellationToken)
        => (await _저장소.영상목록조회Async(channelId?.Trim(), 신규업로드만, take, cancellationToken))
            .Select(ToVideoDto)
            .ToArray();

    public async Task<IReadOnlyList<YouTube채널영상Dto>> 공개영상목록조회Async(
        string? channelId,
        int take,
        CancellationToken cancellationToken)
        => (await _저장소.공개영상목록조회Async(channelId?.Trim(), take, cancellationToken))
            .Select(ToVideoDto)
            .ToArray();

    public async Task<IReadOnlyList<YouTube재생목록Dto>> 재생목록목록조회Async(
        string channelId,
        CancellationToken cancellationToken)
        => (await _client.재생목록목록조회Async(NormalizeId(channelId), cancellationToken))
            .Select(ToPlaylistDto)
            .ToArray();

    public async Task<IReadOnlyList<YouTube재생목록영상Dto>> 재생목록영상목록조회Async(
        string playlistId,
        int take,
        CancellationToken cancellationToken)
        => (await _client.재생목록영상조회Async(
                NormalizePlaylistId(playlistId),
                Math.Clamp(take, 1, 200),
                cancellationToken))
            .Select(ToPlaylistVideoDto)
            .ToArray();

    public async Task<YouTube채널영상Dto> 영상공개설정Async(
        string videoId,
        bool 공개여부,
        CancellationToken cancellationToken)
    {
        var normalizedVideoId = NormalizeVideoId(videoId);
        var video = await _저장소.영상추적조회Async(normalizedVideoId, cancellationToken)
            ?? throw new InvalidOperationException("설정할 YouTube 영상을 찾지 못했습니다.");

        video.공유상태 = 공개여부
            ? YouTube채널영상.공개상태
            : YouTube채널영상.숨김상태;
        await _저장소.저장Async(cancellationToken);
        return ToVideoDto(video);
    }

    public async Task<YouTube채널동기화결과Dto> 동기화Async(
        string? channelId,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return new YouTube채널동기화결과Dto(
                false, 0, 0, 0, 0, null, "YouTube Data API가 비활성화되어 있습니다.");
        }

        if (string.IsNullOrWhiteSpace(channelId))
        {
            await 기본감시채널확보Async(cancellationToken);
        }

        IReadOnlyList<YouTube감시채널> channels;
        if (string.IsNullOrWhiteSpace(channelId))
        {
            channels = await _저장소.활성채널추적조회Async(cancellationToken);
        }
        else
        {
            var channel = await _저장소.추적조회Async(NormalizeId(channelId), cancellationToken)
                ?? throw new InvalidOperationException("감시 중인 YouTube 채널을 찾지 못했습니다.");
            channels = [channel];
        }

        var now = DateTime.UtcNow;
        var receivedCount = 0;
        var addedCount = 0;
        var newUploadCount = 0;

        foreach (var channel in channels)
        {
            var remoteVideos = await _client.업로드목록조회Async(
                channel.UploadsPlaylistId,
                _options.MaxResultsPerChannel,
                cancellationToken);
            receivedCount += remoteVideos.Count;

            var candidateIds = remoteVideos.Select(x => x.VideoId).ToArray();
            var existingIds = await _저장소.기존영상Id조회Async(
                channel.ChannelId,
                candidateIds,
                cancellationToken);
            var isInitialSync = !channel.초기동기화완료여부;

            foreach (var remote in remoteVideos.OrderBy(x => x.게시일시Utc))
            {
                if (!existingIds.Add(remote.VideoId))
                {
                    continue;
                }

                var isNewUpload = !isInitialSync;
                var video = new YouTube채널영상
                {
                    감시채널 = channel,
                    VideoId = remote.VideoId,
                    ChannelId = channel.ChannelId,
                    제목 = remote.제목,
                    설명 = remote.설명,
                    게시일시Utc = remote.게시일시Utc,
                    썸네일Url = remote.썸네일Url,
                    신규업로드여부 = isNewUpload,
                    공유상태 = isNewUpload
                        ? YouTube채널영상.공유대기상태
                        : YouTube채널영상.기준선공유상태,
                    최초감지일시Utc = now
                };
                _저장소.영상추가(video);
                addedCount++;
                if (isNewUpload)
                {
                    newUploadCount++;
                }
            }

            var latest = remoteVideos.MaxBy(x => x.게시일시Utc);
            if (latest is not null)
            {
                channel.마지막영상Id = latest.VideoId;
                channel.마지막영상게시일시Utc = latest.게시일시Utc;
            }

            channel.초기동기화완료여부 = true;
            channel.마지막동기화일시Utc = now;
            channel.수정일시Utc = now;
        }

        if (channels.Count > 0)
        {
            await _저장소.저장Async(cancellationToken);
        }

        return new YouTube채널동기화결과Dto(
            true,
            channels.Count,
            receivedCount,
            addedCount,
            newUploadCount,
            now,
            newUploadCount > 0
                ? $"새 YouTube 영상 {newUploadCount}건을 감지했습니다."
                : "YouTube 채널 동기화를 완료했습니다.");
    }

    private async Task 기본감시채널확보Async(CancellationToken cancellationToken)
    {
        if (_options.DefaultChannels is not { Count: > 0 })
        {
            return;
        }

        var 설정채널Ids = new HashSet<string>(StringComparer.Ordinal);
        var 추가됨 = false;

        foreach (var 기본채널 in _options.DefaultChannels)
        {
            if (string.IsNullOrWhiteSpace(기본채널.ChannelId))
            {
                continue;
            }

            var channelId = NormalizeId(기본채널.ChannelId);
            if (!설정채널Ids.Add(channelId)
                || await _저장소.추적조회Async(channelId, cancellationToken) is not null)
            {
                continue;
            }

            var 채널 = await 원격채널생성Async(
                channelId,
                기본채널.DisplayName,
                cancellationToken);
            _저장소.채널추가(채널);
            추가됨 = true;
        }

        if (추가됨)
        {
            await _저장소.저장Async(cancellationToken);
        }
    }

    private async Task<YouTube감시채널> 원격채널생성Async(
        string channelId,
        string? 표시이름,
        CancellationToken cancellationToken)
    {
        var remote = await _client.채널조회Async(channelId, cancellationToken)
            ?? throw new InvalidOperationException("YouTube 채널을 찾지 못했거나 업로드 목록을 확인할 수 없습니다.");

        var now = DateTime.UtcNow;
        return new YouTube감시채널
        {
            ChannelId = remote.ChannelId,
            채널명 = string.IsNullOrWhiteSpace(표시이름) ? remote.채널명 : 표시이름.Trim(),
            UploadsPlaylistId = remote.UploadsPlaylistId,
            썸네일Url = remote.썸네일Url,
            생성일시Utc = now,
            수정일시Utc = now
        };
    }

    private static string NormalizeId(string channelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);
        var normalized = channelId.Trim();
        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(channelId), "YouTube 채널 ID가 너무 깁니다.");
        }

        return normalized;
    }

    private static string NormalizeVideoId(string videoId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);
        var normalized = videoId.Trim();
        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(videoId), "YouTube 영상 ID가 너무 깁니다.");
        }

        return normalized;
    }

    private static string NormalizePlaylistId(string playlistId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);
        var normalized = playlistId.Trim();
        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(playlistId), "YouTube 재생목록 ID가 너무 깁니다.");
        }

        return normalized;
    }

    private static YouTube감시채널Dto ToChannelDto(YouTube감시채널 channel)
        => new(
            channel.ChannelId,
            channel.채널명,
            channel.썸네일Url,
            channel.활성화여부,
            channel.초기동기화완료여부,
            channel.마지막동기화일시Utc,
            channel.마지막영상Id,
            channel.마지막영상게시일시Utc);

    private static YouTube채널영상Dto ToVideoDto(YouTube채널영상 video)
        => new(
            video.VideoId,
            video.ChannelId,
            video.감시채널?.채널명 ?? string.Empty,
            video.제목,
            video.설명,
            video.게시일시Utc,
            video.썸네일Url,
            $"https://www.youtube.com/watch?v={Uri.EscapeDataString(video.VideoId)}",
            video.신규업로드여부,
            video.공유상태,
            video.최초감지일시Utc);

    private static YouTube재생목록Dto ToPlaylistDto(YouTube재생목록응답 playlist)
        => new(
            playlist.PlaylistId,
            playlist.ChannelId,
            playlist.제목,
            playlist.설명,
            playlist.게시일시Utc,
            playlist.영상수,
            playlist.썸네일Url,
            $"https://www.youtube.com/playlist?list={Uri.EscapeDataString(playlist.PlaylistId)}");

    private static YouTube재생목록영상Dto ToPlaylistVideoDto(YouTube영상응답 video)
        => new(
            video.VideoId,
            video.ChannelId,
            video.제목,
            video.설명,
            video.게시일시Utc,
            video.썸네일Url,
            $"https://www.youtube.com/watch?v={Uri.EscapeDataString(video.VideoId)}");
}
