using Ssalddel.Domain.Community;
using Ssalddel.Services.External.Typecast;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 살뜰.Data;
using 살뜰.Services.External.Google;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public interface I커뮤니티게시글음성작업Processor
{
    Task<bool> 다음작업처리Async(CancellationToken cancellationToken);
}

public sealed class 커뮤니티게시글음성작업Processor : I커뮤니티게시글음성작업Processor
{
    private readonly SsalddelContext _db;
    private readonly ITypecastClient _typecastClient;
    private readonly IGoogleCloudStorageService _storageService;
    private readonly I커뮤니티게시글음성본문분할기 _본문분할기;
    private readonly CommunityPostAudioOptions _options;
    private readonly TypecastOptions _typecastOptions;
    private readonly GoogleCloudStorageOptions _storageOptions;
    private readonly ILogger<커뮤니티게시글음성작업Processor> _logger;

    public 커뮤니티게시글음성작업Processor(
        SsalddelContext db,
        ITypecastClient typecastClient,
        IGoogleCloudStorageService storageService,
        I커뮤니티게시글음성본문분할기 본문분할기,
        IOptions<CommunityPostAudioOptions> options,
        IOptions<TypecastOptions> typecastOptions,
        IOptions<GoogleCloudStorageOptions> storageOptions,
        ILogger<커뮤니티게시글음성작업Processor> logger)
    {
        _db = db;
        _typecastClient = typecastClient;
        _storageService = storageService;
        _본문분할기 = 본문분할기;
        _options = options.Value;
        _typecastOptions = typecastOptions.Value;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    public async Task<bool> 다음작업처리Async(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var leaseExpiredAt = now.AddMinutes(-Math.Max(1, _options.LeaseTimeoutMinutes));
        var candidateId = await _db.PlatformCommunityPostAudio
            .AsNoTracking()
            .Where(x =>
                ((x.Status == 커뮤니티게시글음성상태.대기
                  || x.Status == 커뮤니티게시글음성상태.재시도대기
                  || x.Status == 커뮤니티게시글음성상태.설정대기)
                 && (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now))
                || (x.Status == 커뮤니티게시글음성상태.생성중 && x.UpdatedAtUtc <= leaseExpiredAt))
            .OrderBy(x => x.NextAttemptAtUtc)
            .ThenBy(x => x.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (candidateId == 0)
        {
            return false;
        }

        var token = Guid.NewGuid().ToString("N");
        var claimed = await _db.PlatformCommunityPostAudio
            .Where(x => x.Id == candidateId &&
                (((x.Status == 커뮤니티게시글음성상태.대기
                   || x.Status == 커뮤니티게시글음성상태.재시도대기
                   || x.Status == 커뮤니티게시글음성상태.설정대기)
                  && (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now))
                 || (x.Status == 커뮤니티게시글음성상태.생성중 && x.UpdatedAtUtc <= leaseExpiredAt)))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, 커뮤니티게시글음성상태.생성중)
                .SetProperty(x => x.ProcessingToken, token)
                .SetProperty(x => x.UpdatedAtUtc, now),
                cancellationToken);

        if (claimed == 0)
        {
            return true;
        }

        var job = await _db.PlatformCommunityPostAudio
            .Include(x => x.Post)
            .Include(x => x.Segments)
            .SingleAsync(x => x.Id == candidateId && x.ProcessingToken == token, cancellationToken);

        if (job.Post is null || job.Post.IsDeleted)
        {
            SetTerminalFailure(job, "음성으로 변환할 게시글을 찾을 수 없습니다.", now);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        var narrationBody = string.IsNullOrWhiteSpace(job.Post.Body)
            ? "공유 링크가 포함된 게시글입니다."
            : job.Post.Body;
        var textSegments = _본문분할기.분할(
            job.Post.Title,
            narrationBody,
            _options.MaxCharactersPerSegment);
        var characterCount = textSegments.Sum(x => x.Length);
        var lengthDecision = 커뮤니티게시글음성길이정책.판정(
            characterCount,
            _options.MinCharacters,
            _options.MaxCharactersExclusive);
        if (!lengthDecision.음성화대상)
        {
            SetLengthExcluded(job, lengthDecision, now);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        var configurationError = GetConfigurationError();
        if (configurationError is not null)
        {
            SetConfigurationWait(job, configurationError, now);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        var configuredVoiceId = _options.DefaultVoiceId.Trim();
        var configuredModel = _options.ModelVersion.Trim();
        var voiceAvailable = await _db.Typecast음성
            .AsNoTracking()
            .AnyAsync(x =>
                x.VoiceId == configuredVoiceId
                && x.활성화여부
                && x.지원모델.Any(model => model.버전 == configuredModel),
                cancellationToken);
        if (!voiceAvailable)
        {
            SetConfigurationWait(
                job,
                "설정한 Typecast 음성 또는 모델을 동기화된 카탈로그에서 찾을 수 없습니다.",
                now);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        job.AttemptCount++;
        job.VoiceId = configuredVoiceId;
        job.ModelVersion = configuredModel;
        job.LanguageCode = _options.LanguageCode.Trim();
        job.AudioFormat = NormalizeAudioFormat(_options.AudioFormat);
        job.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var generated = new List<(string Text, Typecast음성합성결과 Audio)>(textSegments.Count);
            foreach (var text in textSegments)
            {
                var audio = await _typecastClient.음성합성Async(new Typecast음성합성요청
                {
                    VoiceId = job.VoiceId,
                    텍스트 = text,
                    모델 = job.ModelVersion,
                    언어코드 = job.LanguageCode,
                    오디오형식 = job.AudioFormat
                }, cancellationToken);
                generated.Add((text, audio));
            }

            var folder = $"{_options.StorageFolder.Trim().Trim('/')}/{job.PostId}/{token}";
            var uploaded = new List<(string Text, GoogleCloudStorageUploadResult Storage, Typecast음성합성결과 Audio)>();
            for (var index = 0; index < generated.Count; index++)
            {
                var item = generated[index];
                await using var stream = new MemoryStream(item.Audio.오디오, writable: false);
                var upload = await _storageService.UploadAsync(
                    stream,
                    $"post-{job.PostId}-audio-{index + 1}.{job.AudioFormat}",
                    item.Audio.ContentType,
                    folder,
                    cancellationToken);
                uploaded.Add((item.Text, upload, item.Audio));
            }

            _db.PlatformCommunityPostAudioSegments.RemoveRange(job.Segments);
            job.Segments.Clear();
            for (var index = 0; index < uploaded.Count; index++)
            {
                var item = uploaded[index];
                job.Segments.Add(new PlatformCommunityPostAudioSegment
                {
                    Sequence = index + 1,
                    CharacterCount = item.Text.Length,
                    BucketName = item.Storage.BucketName,
                    ObjectName = item.Storage.ObjectName,
                    ContentType = item.Audio.ContentType,
                    FileSizeBytes = item.Audio.오디오.LongLength,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            var completedAt = DateTime.UtcNow;
            job.Status = 커뮤니티게시글음성상태.완료;
            job.CompletedAtUtc = completedAt;
            job.NextAttemptAtUtc = null;
            job.LastError = null;
            job.ProcessingToken = null;
            job.UpdatedAtUtc = completedAt;
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogInformation(
                ex,
                "다른 Worker가 게시글 음성 작업 임대를 갱신해 현재 결과 저장을 중단합니다. PostId={PostId}",
                job.PostId);
        }
        catch (Exception ex)
        {
            var failedAt = DateTime.UtcNow;
            job.LastError = Truncate(ex.Message, 2000);
            job.ProcessingToken = null;
            job.UpdatedAtUtc = failedAt;
            if (job.AttemptCount >= Math.Max(1, _options.MaxAttempts))
            {
                job.Status = 커뮤니티게시글음성상태.실패;
                job.NextAttemptAtUtc = null;
            }
            else
            {
                job.Status = 커뮤니티게시글음성상태.재시도대기;
                var delay = Math.Max(10, _options.RetryDelaySeconds) * Math.Pow(2, job.AttemptCount - 1);
                job.NextAttemptAtUtc = failedAt.AddSeconds(Math.Min(3600, delay));
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogWarning(
                ex,
                "커뮤니티 게시글 음성 생성 실패. PostId={PostId}, Attempt={Attempt}, Status={Status}",
                job.PostId,
                job.AttemptCount,
                job.Status);
        }

        return true;
    }

    private string? GetConfigurationError()
    {
        if (!_typecastOptions.Enabled || string.IsNullOrWhiteSpace(_typecastOptions.ApiKey))
        {
            return "Typecast API 설정이 필요합니다.";
        }

        if (string.IsNullOrWhiteSpace(_options.DefaultVoiceId))
        {
            return "CommunityPostAudio:DefaultVoiceId 설정이 필요합니다.";
        }

        if (string.IsNullOrWhiteSpace(_storageOptions.BucketName))
        {
            return "GoogleCloudStorage:BucketName 설정이 필요합니다.";
        }

        return null;
    }

    private static void SetConfigurationWait(PlatformCommunityPostAudio job, string error, DateTime now)
    {
        job.Status = 커뮤니티게시글음성상태.설정대기;
        job.LastError = error;
        job.NextAttemptAtUtc = now.AddMinutes(5);
        job.ProcessingToken = null;
        job.UpdatedAtUtc = now;
    }

    private static void SetTerminalFailure(PlatformCommunityPostAudio job, string error, DateTime now)
    {
        job.Status = 커뮤니티게시글음성상태.실패;
        job.LastError = error;
        job.NextAttemptAtUtc = null;
        job.ProcessingToken = null;
        job.UpdatedAtUtc = now;
    }

    private static void SetLengthExcluded(
        PlatformCommunityPostAudio job,
        커뮤니티게시글음성길이판정 decision,
        DateTime now)
    {
        job.Status = 커뮤니티게시글음성상태.길이제외;
        job.LastError = $"음성화 대상은 {decision.최소글자수}자 이상 {decision.최대글자수미만}자 미만입니다. 현재 {decision.글자수}자입니다.";
        job.NextAttemptAtUtc = null;
        job.ProcessingToken = null;
        job.UpdatedAtUtc = now;
    }

    private static string NormalizeAudioFormat(string? value)
        => string.Equals(value?.Trim(), "mp3", StringComparison.OrdinalIgnoreCase) ? "mp3" : "wav";

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
