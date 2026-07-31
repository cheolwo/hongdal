using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Data;

namespace Ssalddel.Services.Content;

public interface I지역문화이미지Prompt조회UseCase
{
    Task<RegionalCultureImagePromptListResponse> 목록조회Async(
        string? countryCode,
        CancellationToken cancellationToken = default);

    Task<RegionalCultureImagePromptDto?> 상세조회Async(
        string regionKey,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.Application,
    "한국 시·도, 미국 주, 중국 성급 지역의 문화 이미지 조사 초안과 생성 프롬프트를 영속 DB에서 조회",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "프롬프트를 조회할 뿐 외부 이미지 생성 요청이나 게시·첨부 상태를 만들지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalCultureImagePrompt,
    SsalddelCodeLayer.Application,
    "지역문화 이미지 프롬프트 목록·상세 영속 조회",
    ContractType = typeof(I지역문화이미지Prompt조회UseCase),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "ResearchDraft는 생성 승인 상태가 아니며 RequiresEvidenceReview 경계를 그대로 반환합니다.")]
public sealed class 지역문화이미지Prompt조회UseCase(
    SsalddelContext db) : I지역문화이미지Prompt조회UseCase
{
    public async Task<RegionalCultureImagePromptListResponse> 목록조회Async(
        string? countryCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCountryCode = NormalizeCountryCode(countryCode);
        var query = db.지역문화이미지Prompts
            .AsNoTracking()
            .AsQueryable();

        if (normalizedCountryCode is not null)
        {
            query = query.Where(item => item.CountryCode == normalizedCountryCode);
        }

        var entities = await query
            .OrderBy(item => item.CountryCode)
            .ThenBy(item => item.RegionNameEn)
            .ToArrayAsync(cancellationToken);

        var items = entities.Select(ToDto).ToArray();
        return new RegionalCultureImagePromptListResponse(
            normalizedCountryCode,
            items.Length,
            items);
    }

    public async Task<RegionalCultureImagePromptDto?> 상세조회Async(
        string regionKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedRegionKey = NormalizeRegionKey(regionKey);
        var entity = await db.지역문화이미지Prompts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.RegionKey == normalizedRegionKey,
                cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

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

    private static RegionalCultureImagePromptDto ToDto(
        Ssalddel.Domain.Content.지역문화이미지Prompt entity)
        => new(
            entity.RegionKey,
            entity.CountryCode,
            entity.SubdivisionCode,
            entity.RegionNameKo,
            entity.RegionNameEn,
            entity.RegionNameLocal,
            entity.RegionTypeCode,
            entity.GeographySummaryKo,
            entity.CultureSummaryKo,
            DeserializeList(entity.VisualAnchorsJson),
            DeserializeList(entity.AvoidExpressionsJson),
            entity.PromptKo,
            entity.AspectRatio,
            entity.SafeCrop,
            entity.ReviewStatusCode,
            entity.RequiresEvidenceReview,
            entity.EvidenceNotesKo,
            entity.PromptVersion,
            RegionalCultureAnimationStyleCodes.CinematicStylized3D,
            RegionalCultureAnimationStyleCodes.TargetImagesPerRegion,
            entity.UpdatedAtUtc);

    private static IReadOnlyList<string> DeserializeList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
