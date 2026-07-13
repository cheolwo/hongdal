using Hongdal.Contracts.Common.Community;
using Hongdal.Domain.Community;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.External.Google;

namespace Hongdal.Services.Community;

public sealed record 커뮤니티게시글음성파일(
    byte[] Content,
    string ContentType,
    string FileName);

public interface I커뮤니티게시글음성조회Service
{
    Task<PlatformCommunityPostAudioResponse?> 조회Async(
        long postId,
        string? requesterUserId,
        string traceId,
        CancellationToken cancellationToken);

    Task<커뮤니티게시글음성파일?> 다운로드Async(
        long postId,
        int sequence,
        string? requesterUserId,
        string traceId,
        CancellationToken cancellationToken);
}

public sealed class 커뮤니티게시글음성조회Service : I커뮤니티게시글음성조회Service
{
    private readonly HongdalContext _db;
    private readonly IGoogleCloudStorageService _storageService;

    public 커뮤니티게시글음성조회Service(
        HongdalContext db,
        IGoogleCloudStorageService storageService)
    {
        _db = db;
        _storageService = storageService;
    }

    public async Task<PlatformCommunityPostAudioResponse?> 조회Async(
        long postId,
        string? requesterUserId,
        string traceId,
        CancellationToken cancellationToken)
    {
        var audio = await _db.PlatformCommunityPostAudio
            .AsNoTracking()
            .Include(x => x.Post)
            .Include(x => x.Segments)
            .SingleOrDefaultAsync(x => x.PostId == postId && !x.Post.IsDeleted, cancellationToken);
        if (audio is null)
        {
            return null;
        }

        if (audio.Status == 커뮤니티게시글음성상태.완료)
        {
            await AddAccessLogAsync(
                audio.Id,
                postId,
                null,
                커뮤니티게시글음성접근유형.재생정보조회,
                requesterUserId,
                traceId,
                cancellationToken);
        }

        return new PlatformCommunityPostAudioResponse
        {
            PostId = postId,
            Status = audio.Status,
            IsReady = audio.Status == 커뮤니티게시글음성상태.완료,
            Provider = audio.Provider,
            VoiceId = audio.VoiceId,
            ModelVersion = audio.ModelVersion,
            Message = ToPublicMessage(audio.Status),
            UpdatedAtUtc = audio.UpdatedAtUtc,
            CompletedAtUtc = audio.CompletedAtUtc,
            Segments = audio.Status == 커뮤니티게시글음성상태.완료
                ? audio.Segments
                    .OrderBy(x => x.Sequence)
                    .Select(x => new PlatformCommunityPostAudioSegmentResponse
                    {
                        Sequence = x.Sequence,
                        CharacterCount = x.CharacterCount,
                        ContentType = x.ContentType,
                        FileSizeBytes = x.FileSizeBytes,
                        DownloadPath = $"/api/v1/community/posts/{postId}/audio/segments/{x.Sequence}/download"
                    })
                    .ToArray()
                : []
        };
    }

    public async Task<커뮤니티게시글음성파일?> 다운로드Async(
        long postId,
        int sequence,
        string? requesterUserId,
        string traceId,
        CancellationToken cancellationToken)
    {
        var segment = await _db.PlatformCommunityPostAudioSegments
            .AsNoTracking()
            .Include(x => x.Audio)
                .ThenInclude(x => x.Post)
            .SingleOrDefaultAsync(x =>
                x.Audio.PostId == postId
                && x.Sequence == sequence
                && x.Audio.Status == 커뮤니티게시글음성상태.완료
                && !x.Audio.Post.IsDeleted,
                cancellationToken);
        if (segment is null)
        {
            return null;
        }

        var content = await _storageService.DownloadAsync(
            segment.BucketName,
            segment.ObjectName,
            cancellationToken);

        await AddAccessLogAsync(
            segment.AudioId,
            postId,
            segment.Sequence,
            커뮤니티게시글음성접근유형.다운로드,
            requesterUserId,
            traceId,
            cancellationToken);

        var extension = segment.ContentType.Contains("mpeg", StringComparison.OrdinalIgnoreCase) ? "mp3" : "wav";
        return new 커뮤니티게시글음성파일(
            content,
            segment.ContentType,
            $"community-post-{postId}-audio-{sequence}.{extension}");
    }

    private async Task AddAccessLogAsync(
        long audioId,
        long postId,
        int? sequence,
        string accessType,
        string? requesterUserId,
        string traceId,
        CancellationToken cancellationToken)
    {
        _db.PlatformCommunityPostAudioAccessLogs.Add(new PlatformCommunityPostAudioAccessLog
        {
            AudioId = audioId,
            PostId = postId,
            SegmentSequence = sequence,
            AccessType = accessType,
            RequesterUserId = Clean(requesterUserId, 450),
            TraceId = Clean(traceId, 100) ?? string.Empty,
            AccessedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string ToPublicMessage(string status)
        => status switch
        {
            커뮤니티게시글음성상태.완료 => "게시글 음성을 재생할 수 있습니다.",
            커뮤니티게시글음성상태.실패 => "게시글 음성을 준비하지 못했습니다.",
            커뮤니티게시글음성상태.설정대기 => "음성 서비스를 준비하고 있습니다.",
            커뮤니티게시글음성상태.길이제외 => "100자 이상 500자 미만인 게시글만 음성으로 제공합니다.",
            _ => "게시글 음성을 생성하고 있습니다."
        };

    private static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var clean = value.Trim();
        return clean.Length <= maxLength ? clean : clean[..maxLength];
    }
}
