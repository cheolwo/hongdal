using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.Content;
using 살뜰.Data;
using 살뜰.Services.Options;
using 살뜰.도메인.공통;

namespace 살뜰.Services.Images;

public sealed record 지역문화애니메이션장면(
    int Sequence,
    string Code,
    string GuidanceKo);

public static class 지역문화애니메이션장면Catalog
{
    public static IReadOnlyList<지역문화애니메이션장면> All { get; } =
    [
        new(1, "daily-opening",
            "이른 아침에 동네·마을·거리의 하루가 시작되는 장면. 지형과 주거, 작은 일터가 자연스럽게 연결되도록 한다."),
        new(2, "market-and-food",
            "시장·장터·가게에서 주민이 재료를 고르고 손질하거나 음식을 준비하는 장면. 특산물은 보조 요소 1~2개로 제한한다."),
        new(3, "craft-and-work",
            "지역의 현재 공예·수리·생업 작업을 여러 세대가 함께 이어가는 장면. 근거 없는 의례복이나 옛 도구를 발명하지 않는다."),
        new(4, "landscape-and-mobility",
            "지역 지형과 일상 이동을 함께 보여 주는 장면. 해안·강·산·평야·도시 중 실제 프롬프트에 있는 환경만 선택한다."),
        new(5, "home-and-generations",
            "집·골목·마을 공간에서 세대가 생활 기술과 식사를 나누는 장면. 사적인 내부를 과장하지 않고 열린 생활 공간으로 표현한다."),
        new(6, "seasonal-work",
            "지역의 계절과 날씨 속에서 주민이 평범한 일을 함께하는 장면. 축제나 의례는 공식 근거가 프롬프트에 있을 때만 사용한다."),
        new(7, "architecture-care",
            "현재 사용되는 지역 건축과 현대 생활공간을 주민이 돌보고 이용하는 장면. 랜드마크 전시나 관광 엽서 구도를 피한다."),
        new(8, "learning-and-making",
            "작은 도서관·공방·학교·커뮤니티 공간에서 배우고 만드는 장면. 특정 집단을 지역 전체의 대표로 고정하지 않는다."),
        new(9, "evening-community",
            "해 질 무렵 이웃이 일과를 정리하고 대화하거나 음식을 나누는 장면. 과도한 공연·퍼레이드·상업 광고는 넣지 않는다."),
        new(10, "contemporary-continuity",
            "청년과 노년, 전통 기술과 현대 생활이 충돌 없이 이어지는 현재의 장면. 지역의 미래를 상징물보다 사람의 활동으로 보여 준다.")
    ];

    public static 지역문화애니메이션장면 Get(int sequence)
        => All.First(item => item.Sequence == sequence);
}

public sealed class 지역문화애니메이션프롬프트생성기 : I이미지프롬프트생성기
{
    public string 이미지용도 => 생성이미지용도.지역문화애니메이션;

    public string CreatePrompt(이미지생성요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.설명);

        var sceneGuidance = string.IsNullOrWhiteSpace(request.추가맥락)
            ? "지역 프롬프트의 지형·생활문화·시각 앵커 중 서로 어울리는 요소만 선택해 하나의 생활 장면으로 구성한다."
            : request.추가맥락.Trim();

        return $"""
            Use case: stylized-concept.
            Asset type: Ssalddel regional-culture community image.
            Primary regional brief:
            {request.설명.Trim()}

            Scene-specific direction:
            {sceneGuidance}

            Visual style: polished cinematic stylized 3D animation film still, soft rounded but believable forms,
            handcrafted surface textures, expressive yet non-caricatured people, layered foreground/midground/background,
            gentle volumetric light, and a balanced warm-and-cool palette. It must read as animation rather than a
            photorealistic photograph or flat 2D editorial illustration. Do not imitate a named studio, artist,
            existing film, or copyrighted character.

            Keep present-day local life, work, food preparation, architecture, landscape, and intergenerational activity
            culturally grounded in the supplied regional brief. Do not turn a landmark, costume, ethnicity, food,
            or tourism slogan into the single identity of the whole region. Generated imagery is cultural context,
            not documentary evidence or proof of product origin.

            Composition: 16:9 cinematic landscape. Keep all essential people and cultural anchors inside a centered
            4:3 safe crop. No text, captions, prices, signs, logos, flags, maps, political symbols, official seals,
            watermarks, cosmic spheres, magical energy, fantasy glyphs, or identifiable real people.
            """;
    }
}

public sealed class 지역문화이미지대상Resolver : I샘플이미지대상Resolver
{
    public const string 대상타입값 = "지역문화";

    private readonly SsalddelContext _db;
    private readonly RegionalCultureImageGenerationOptions _options;

    public 지역문화이미지대상Resolver(
        SsalddelContext db,
        IOptions<RegionalCultureImageGenerationOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public string 대상타입 => 대상타입값;

    public string 이미지용도 => 생성이미지용도.지역문화애니메이션;

    public async Task<IReadOnlyList<샘플이미지대상항목>> GetMissingImageTargetsAsync(
        int maxCount,
        bool includeFailed,
        CancellationToken cancellationToken = default)
    {
        var prompts = await _db.지역문화이미지Prompts
            .AsNoTracking()
            .Where(item =>
                item.ReviewStatusCode == 지역문화이미지Prompt검토상태Codes.ApprovedForGeneration
                && !item.RequiresEvidenceReview)
            .ToArrayAsync(cancellationToken);

        if (prompts.Length == 0)
        {
            return [];
        }

        var jobs = await _db.생성이미지작업
            .AsNoTracking()
            .Where(item =>
                item.대상타입 == 대상타입
                && item.이미지용도 == 이미지용도)
            .ToArrayAsync(cancellationToken);
        var jobsByTarget = jobs
            .GroupBy(item => item.대상식별자, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var targetCount = ResolveTargetCount();
        var take = Math.Clamp(maxCount, 1, targetCount);

        foreach (var prompt in OrderPrompts(prompts))
        {
            var candidates = new List<샘플이미지대상항목>(targetCount);
            for (var sceneNumber = 1; sceneNumber <= targetCount; sceneNumber++)
            {
                var targetIdentifier = BuildTargetIdentifier(prompt.RegionKey, sceneNumber);
                jobsByTarget.TryGetValue(targetIdentifier, out var targetJobs);
                if (!IsAvailable(targetJobs ?? [], includeFailed))
                {
                    continue;
                }

                var scene = 지역문화애니메이션장면Catalog.Get(sceneNumber);
                candidates.Add(new 샘플이미지대상항목
                {
                    대상타입 = 대상타입,
                    대상식별자 = targetIdentifier,
                    이미지용도 = 이미지용도,
                    제목 = $"{prompt.RegionNameKo} 지역문화 애니메이션 장면 {sceneNumber:00}",
                    설명 = prompt.PromptKo,
                    추가맥락 =
                        $"장면 {sceneNumber:00}/10 · {scene.Code}. {scene.GuidanceKo} "
                        + "같은 지역의 다른 장면과 주제·시간대·구도가 겹치지 않게 구성한다.",
                    종횡비 = _options.AspectRatio,
                    해상도 = _options.Resolution,
                    샘플데이터여부 = false
                });
            }

            if (candidates.Count > 0)
            {
                return candidates.Take(take).ToArray();
            }
        }

        return [];
    }

    public Task MarkRequestedAsync(
        string 대상식별자,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task MarkCompletedAsync(
        string 대상식별자,
        string imageUrl,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task MarkFailedAsync(
        string 대상식별자,
        string? reason,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public static string BuildTargetIdentifier(string regionKey, int sceneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionKey);
        if (sceneNumber is < 1 or > RegionalCultureAnimationStyleCodes.TargetImagesPerRegion)
        {
            throw new ArgumentOutOfRangeException(nameof(sceneNumber));
        }

        return $"{regionKey.Trim().ToLowerInvariant()}--scene-{sceneNumber:00}";
    }

    private int ResolveTargetCount()
        => Math.Clamp(
            _options.TargetImagesPerRegion,
            1,
            RegionalCultureAnimationStyleCodes.TargetImagesPerRegion);

    private IEnumerable<지역문화이미지Prompt> OrderPrompts(
        IEnumerable<지역문화이미지Prompt> prompts)
    {
        var countryOrder = _options.CountryOrder
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select((code, index) => new { Code = code.ToUpperInvariant(), Index = index })
            .GroupBy(item => item.Code, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.First().Index,
                StringComparer.Ordinal);

        return prompts
            .OrderBy(item =>
                countryOrder.TryGetValue(item.CountryCode, out var index)
                    ? index
                    : int.MaxValue)
            .ThenBy(item => item.CountryCode, StringComparer.Ordinal)
            .ThenBy(item => item.RegionNameEn, StringComparer.Ordinal);
    }

    private static bool IsAvailable(
        IReadOnlyList<생성이미지작업> jobs,
        bool includeFailed)
    {
        if (jobs.Any(item => item.상태 == 생성이미지작업상태.완료))
        {
            return false;
        }

        if (jobs.Any(item =>
                item.상태 == 생성이미지작업상태.생성대기
                || item.상태 == 생성이미지작업상태.생성요청됨
                || item.상태 == 생성이미지작업상태.생성중
                || item.상태 == 생성이미지작업상태.업로드중))
        {
            return false;
        }

        return includeFailed
               || jobs.All(item => item.상태 != 생성이미지작업상태.실패);
    }
}

public sealed record 지역문화이미지순차생성결과(
    bool Accepted,
    string ResultCode,
    string Message,
    IReadOnlyList<생성이미지작업> Jobs);

public interface I지역문화이미지순차생성Service
{
    Task<지역문화이미지순차생성결과> 다음배치생성Async(
        int requestedCount,
        bool includeFailed,
        CancellationToken cancellationToken = default);
}

public sealed class 지역문화이미지순차생성Service : I지역문화이미지순차생성Service
{
    private readonly SsalddelContext _db;
    private readonly I샘플이미지생성Service _imageGenerationService;
    private readonly RegionalCultureImageGenerationOptions _options;
    private readonly GeminiImageOptions _geminiImageOptions;
    private readonly ISsalddelExecutionModePolicy _executionMode;

    public 지역문화이미지순차생성Service(
        SsalddelContext db,
        I샘플이미지생성Service imageGenerationService,
        IOptions<RegionalCultureImageGenerationOptions> options,
        IOptions<GeminiImageOptions> geminiImageOptions,
        ISsalddelExecutionModePolicy executionMode)
    {
        _db = db;
        _imageGenerationService = imageGenerationService;
        _options = options.Value;
        _geminiImageOptions = geminiImageOptions.Value;
        _executionMode = executionMode;
    }

    public async Task<지역문화이미지순차생성결과> 다음배치생성Async(
        int requestedCount,
        bool includeFailed,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Rejected(
                "Disabled",
                "RegionalCultureImageGeneration:Enabled가 꺼져 있어 외부 이미지 작업을 등록하지 않았습니다.");
        }

        if (!_executionMode.IsOperational)
        {
            return Rejected(
                "OperationalModeRequired",
                "외부 비용이 발생하는 지역문화 이미지 생성은 Operational 모드에서만 실행합니다.");
        }

        if (!_geminiImageOptions.Enabled
            || string.IsNullOrWhiteSpace(_geminiImageOptions.ApiKey))
        {
            return Rejected(
                "ApiKeyMissing",
                "GeminiImage가 활성화되지 않았거나 ApiKey가 없어 외부 이미지 작업을 등록하지 않았습니다.");
        }

        var hasActiveJob = await _db.생성이미지작업
            .AsNoTracking()
            .AnyAsync(item =>
                    item.대상타입 == 지역문화이미지대상Resolver.대상타입값
                    && item.이미지용도 == 생성이미지용도.지역문화애니메이션
                    && (item.상태 == 생성이미지작업상태.생성대기
                        || item.상태 == 생성이미지작업상태.생성요청됨
                        || item.상태 == 생성이미지작업상태.생성중
                        || item.상태 == 생성이미지작업상태.업로드중),
                cancellationToken);
        if (hasActiveJob)
        {
            return Rejected(
                "ActiveJobExists",
                "지역문화 이미지 작업 한 건이 진행 중이므로 완료 또는 실패를 확인한 뒤 다음 장면을 등록합니다.");
        }

        var utcToday = DateTime.UtcNow.Date;
        var submittedToday = await _db.생성이미지작업
            .AsNoTracking()
            .CountAsync(item =>
                    item.대상타입 == 지역문화이미지대상Resolver.대상타입값
                    && item.이미지용도 == 생성이미지용도.지역문화애니메이션
                    && item.생성시각 >= utcToday,
                cancellationToken);
        var remainingDaily = Math.Max(0, Math.Max(1, _options.MaxDailySubmissions) - submittedToday);
        if (remainingDaily == 0)
        {
            return Rejected(
                "DailyLimitReached",
                "오늘의 지역문화 이미지 생성 등록 한도에 도달했습니다.");
        }

        var maxPerCycle = Math.Clamp(_options.MaxNewJobsPerCycle, 1, 10);
        var createCount = Math.Min(
            Math.Clamp(requestedCount, 1, maxPerCycle),
            remainingDaily);
        var jobs = await _imageGenerationService.누락샘플이미지생성Async(
            지역문화이미지대상Resolver.대상타입값,
            생성이미지용도.지역문화애니메이션,
            createCount,
            includeFailed,
            cancellationToken);

        return jobs.Count == 0
            ? Rejected(
                "NoApprovedTarget",
                "승인된 지역의 누락 장면이 없거나 실패 장면이 수동 재시도를 기다리고 있습니다.")
            : new 지역문화이미지순차생성결과(
                true,
                "Created",
                $"지역문화 이미지 작업 {jobs.Count}건을 순차 생성 대기열에 등록했습니다.",
                jobs);
    }

    private static 지역문화이미지순차생성결과 Rejected(
        string code,
        string message)
        => new(false, code, message, []);
}
