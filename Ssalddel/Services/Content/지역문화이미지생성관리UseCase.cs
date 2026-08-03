using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.Content;
using 살뜰.Data;
using 살뜰.Services.Images;
using 살뜰.Services.Options;
using 살뜰.도메인.공통;

namespace Ssalddel.Services.Content;

public interface I지역문화이미지생성관리UseCase
{
    Task<RegionalCultureImageGenerationProgressResponse> 진행현황조회Async(
        string? countryCode,
        CancellationToken cancellationToken = default);

    Task<RegionalCultureImageGenerationApprovalResponse> 생성승인Async(
        string regionKey,
        RegionalCultureImageGenerationApprovalRequest request,
        CancellationToken cancellationToken = default);

    Task<RegionalCultureImageGenerationNextResponse> 다음장면생성Async(
        RegionalCultureImageGenerationNextRequest request,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.Application,
    "지역문화 3D 애니메이션 이미지의 근거 검토 승인, 지역별 10장 진행 현황과 bounded 생성 요청을 관리",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "ResearchDraft를 자동 승인하지 않고, 명시적 검토와 Operational·비용 한도를 모두 통과한 경우에만 외부 생성 작업을 등록합니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalCultureImagePrompt,
    SsalddelCodeLayer.Application,
    "지역문화 애니메이션 이미지 승인·진행 현황·순차 생성 조율",
    ContractType = typeof(I지역문화이미지생성관리UseCase),
    FlowOrder = 35,
    Effects = SsalddelCodeEffect.PersistentRead
              | SsalddelCodeEffect.PersistentWrite
              | SsalddelCodeEffect.NetworkCall
              | SsalddelCodeEffect.ThirdPartyApiCall
              | SsalddelCodeEffect.ObjectStorageWrite
              | SsalddelCodeEffect.MayIncurExternalCost,
    Boundary = "외부 생성은 기본 비활성이고 공식 근거·고정관념 위험 검토, Operational 모드, 일일 한도와 단일 활성 작업 경계를 요구합니다.")]
public sealed class 지역문화이미지생성관리UseCase(
    SsalddelContext db,
    I지역문화이미지순차생성Service sequenceService,
    IOptions<RegionalCultureImageGenerationOptions> options)
    : I지역문화이미지생성관리UseCase
{
    private readonly RegionalCultureImageGenerationOptions _options = options.Value;

    public async Task<RegionalCultureImageGenerationProgressResponse> 진행현황조회Async(
        string? countryCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCountryCode = NormalizeCountryCode(countryCode);
        var promptQuery = db.지역문화이미지Prompts
            .AsNoTracking()
            .AsQueryable();
        if (normalizedCountryCode is not null)
        {
            promptQuery = promptQuery.Where(item => item.CountryCode == normalizedCountryCode);
        }

        var prompts = await promptQuery
            .OrderBy(item => item.CountryCode)
            .ThenBy(item => item.RegionNameEn)
            .ToArrayAsync(cancellationToken);
        var jobs = await db.생성이미지작업
            .AsNoTracking()
            .Where(item =>
                item.대상타입 == 지역문화이미지대상Resolver.대상타입값
                && item.이미지용도 == 생성이미지용도.지역문화애니메이션)
            .ToArrayAsync(cancellationToken);
        var jobsByTarget = jobs
            .GroupBy(item => item.대상식별자, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var targetCount = ResolveTargetCount();
        var items = prompts
            .Select(prompt => ToProgressItem(prompt, targetCount, jobsByTarget))
            .ToArray();

        return new RegionalCultureImageGenerationProgressResponse(
            normalizedCountryCode,
            RegionalCultureAnimationStyleCodes.CinematicStylized3D,
            targetCount,
            items.Length,
            items.Sum(item => item.TargetCount),
            items.Sum(item => item.CompletedCount),
            items.Sum(item => item.RunningCount),
            items.Sum(item => item.FailedCount),
            items.Sum(item => item.RemainingCount),
            items);
    }

    public async Task<RegionalCultureImageGenerationApprovalResponse> 생성승인Async(
        string regionKey,
        RegionalCultureImageGenerationApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalizedRegionKey = NormalizeRegionKey(regionKey);
        var reviewNote = request.ReviewNoteKo?.Trim() ?? string.Empty;

        if (!request.OfficialSourcesReviewed)
        {
            throw new ArgumentException(
                "해당 지역의 공식 문화·관광·박물관 또는 공동체 원천을 검토해야 합니다.",
                nameof(request));
        }

        if (!request.StereotypeRiskReviewed)
        {
            throw new ArgumentException(
                "단일 랜드마크·복식·민족·음식으로 지역 전체를 고정하지 않는지 검토해야 합니다.",
                nameof(request));
        }

        var reviewedSourceKeys = request.ReviewedSourceKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (reviewedSourceKeys.Count < 2)
        {
            throw new ArgumentException(
                "생성 승인에는 해당 국가의 공식 원천 source key가 최소 2개 필요합니다.",
                nameof(request));
        }

        if (reviewNote.Length is < 20 or > 2000)
        {
            throw new ArgumentException(
                "검토 메모는 20자 이상 2,000자 이하여야 합니다.",
                nameof(request));
        }

        var entity = await db.지역문화이미지Prompts
            .FirstOrDefaultAsync(
                item => item.RegionKey == normalizedRegionKey,
                cancellationToken)
            ?? throw new KeyNotFoundException("지역문화 이미지 프롬프트를 찾을 수 없습니다.");

        if (entity.ReviewStatusCode == 지역문화이미지Prompt검토상태Codes.Retired)
        {
            throw new InvalidOperationException("폐기된 지역문화 프롬프트는 생성 승인할 수 없습니다.");
        }

        if (entity.PromptVersion < 2)
        {
            throw new InvalidOperationException(
                "3D 애니메이션 스타일 v2 프롬프트로 갱신한 뒤 생성 승인해야 합니다.");
        }

        var verifiedSourceKeys = await db.지역문화공공기관Sources
            .AsNoTracking()
            .Where(source => source.CountryCode == entity.CountryCode
                             && reviewedSourceKeys.Contains(source.SourceKey))
            .Select(source => source.SourceKey)
            .ToArrayAsync(cancellationToken);
        if (verifiedSourceKeys.Length != reviewedSourceKeys.Count)
        {
            throw new ArgumentException(
                "검토한 source key가 지역 국가와 일치하는 공식 원천 카탈로그에 모두 등록되어 있어야 합니다.",
                nameof(request));
        }

        var now = DateTime.UtcNow;
        entity.ReviewStatusCode = 지역문화이미지Prompt검토상태Codes.ApprovedForGeneration;
        entity.RequiresEvidenceReview = false;
        entity.EvidenceNotesKo =
            $"{entity.EvidenceNotesKo.Trim()}\n\n"
            + $"[{now:yyyy-MM-dd} 생성 승인 검토 · {string.Join(", ", reviewedSourceKeys)}] {reviewNote}";
        entity.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);

        return new RegionalCultureImageGenerationApprovalResponse(
            entity.RegionKey,
            entity.ReviewStatusCode,
            entity.RequiresEvidenceReview,
            entity.UpdatedAtUtc);
    }

    public async Task<RegionalCultureImageGenerationNextResponse> 다음장면생성Async(
        RegionalCultureImageGenerationNextRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await sequenceService.다음배치생성Async(
            request.MaxCount,
            request.IncludeFailed,
            cancellationToken);

        return new RegionalCultureImageGenerationNextResponse(
            result.Accepted,
            result.ResultCode,
            result.Message,
            result.Jobs.Count,
            result.Jobs
                .Select(item => new RegionalCultureImageGenerationJobDto(
                    item.Id,
                    item.작업코드,
                    item.대상식별자,
                    item.상태,
                    item.생성시각))
                .ToArray());
    }

    private RegionalCultureImageGenerationProgressItemDto ToProgressItem(
        지역문화이미지Prompt prompt,
        int targetCount,
        IReadOnlyDictionary<string, 생성이미지작업[]> jobsByTarget)
    {
        var slots = Enumerable.Range(1, targetCount)
            .Select(sceneNumber =>
            {
                var targetIdentifier =
                    지역문화이미지대상Resolver.BuildTargetIdentifier(
                        prompt.RegionKey,
                        sceneNumber);
                jobsByTarget.TryGetValue(targetIdentifier, out var jobs);
                return ToSlot(sceneNumber, targetIdentifier, jobs ?? []);
            })
            .ToArray();
        var completed = slots.Count(item =>
            item.StatusCode == RegionalCultureImageGenerationSlotStatusCodes.Completed);
        var running = slots.Count(item =>
            item.StatusCode is RegionalCultureImageGenerationSlotStatusCodes.Queued
                or RegionalCultureImageGenerationSlotStatusCodes.Running);
        var failed = slots.Count(item =>
            item.StatusCode == RegionalCultureImageGenerationSlotStatusCodes.Failed);

        return new RegionalCultureImageGenerationProgressItemDto(
            prompt.RegionKey,
            prompt.CountryCode,
            prompt.RegionNameKo,
            prompt.ReviewStatusCode,
            prompt.ReviewStatusCode == 지역문화이미지Prompt검토상태Codes.ApprovedForGeneration
            && !prompt.RequiresEvidenceReview,
            targetCount,
            completed,
            running,
            failed,
            targetCount - completed - running,
            slots);
    }

    private static RegionalCultureImageGenerationSlotDto ToSlot(
        int sceneNumber,
        string targetIdentifier,
        IReadOnlyList<생성이미지작업> jobs)
    {
        var completed = jobs
            .Where(item => item.상태 == 생성이미지작업상태.완료)
            .OrderByDescending(item => item.완료시각 ?? item.수정시각)
            .FirstOrDefault();
        if (completed is not null)
        {
            return new RegionalCultureImageGenerationSlotDto(
                sceneNumber,
                targetIdentifier,
                RegionalCultureImageGenerationSlotStatusCodes.Completed,
                completed.저장Url,
                completed.완료시각,
                null);
        }

        var active = jobs
            .Where(item =>
                item.상태 == 생성이미지작업상태.생성대기
                || item.상태 == 생성이미지작업상태.생성요청됨
                || item.상태 == 생성이미지작업상태.생성중
                || item.상태 == 생성이미지작업상태.업로드중)
            .OrderByDescending(item => item.수정시각)
            .FirstOrDefault();
        if (active is not null)
        {
            var statusCode = active.상태 is 생성이미지작업상태.생성대기
                or 생성이미지작업상태.생성요청됨
                ? RegionalCultureImageGenerationSlotStatusCodes.Queued
                : RegionalCultureImageGenerationSlotStatusCodes.Running;
            return new RegionalCultureImageGenerationSlotDto(
                sceneNumber,
                targetIdentifier,
                statusCode,
                null,
                null,
                null);
        }

        var failed = jobs
            .Where(item => item.상태 == 생성이미지작업상태.실패)
            .OrderByDescending(item => item.최종실패시각 ?? item.수정시각)
            .FirstOrDefault();
        return failed is null
            ? new RegionalCultureImageGenerationSlotDto(
                sceneNumber,
                targetIdentifier,
                RegionalCultureImageGenerationSlotStatusCodes.Missing,
                null,
                null,
                null)
            : new RegionalCultureImageGenerationSlotDto(
                sceneNumber,
                targetIdentifier,
                RegionalCultureImageGenerationSlotStatusCodes.Failed,
                null,
                null,
                failed.실패사유);
    }

    private int ResolveTargetCount()
        => Math.Clamp(
            _options.TargetImagesPerRegion,
            1,
            RegionalCultureAnimationStyleCodes.TargetImagesPerRegion);

    private static string? NormalizeCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        var normalized = countryCode.Trim().ToUpperInvariant();
        if (!RegionalCultureImagePromptCountryCodes.All.Contains(
                normalized,
                StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"CountryCode는 {string.Join(", ", RegionalCultureImagePromptCountryCodes.All)} 중 하나여야 합니다.",
                nameof(countryCode));
        }

        return normalized;
    }

    private static string NormalizeRegionKey(string regionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionKey);
        var normalized = regionKey.Trim().ToLowerInvariant();
        if (normalized.Length > 80
            || normalized.IndexOfAny(['/', '\\', '?', '#']) >= 0)
        {
            throw new ArgumentException(
                "RegionKey는 80자 이하이며 경로 구분자를 포함하지 않아야 합니다.",
                nameof(regionKey));
        }

        return normalized;
    }
}
