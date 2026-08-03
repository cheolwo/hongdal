using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Data;

namespace Ssalddel.Services.Content;

public interface I지역문화공공기관Source조회UseCase
{
    Task<RegionalCulturePublicInstitutionSourceListResponse> 목록조회Async(
        string? countryCode,
        string? jurisdictionLevelCode,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "한국·미국·중국의 지역문화 공공기관·공식 디렉터리 근거를 영속 DB에서 조회",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "공식 원천을 안내할 뿐 기관의 현재 담당 범위나 지역문화 대표성을 확정하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalCulturePublicInstitution,
    SsalddelCodeLayer.Application,
    "지역문화 공공기관 원천의 국가·관할 단계별 영속 조회",
    ContractType = typeof(I지역문화공공기관Source조회UseCase),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "공식 원천 확인 시각과 지역별 재확인 필요 상태를 그대로 반환합니다.")]
public sealed class 지역문화공공기관Source조회UseCase(
    SsalddelContext db) : I지역문화공공기관Source조회UseCase
{
    public async Task<RegionalCulturePublicInstitutionSourceListResponse> 목록조회Async(
        string? countryCode,
        string? jurisdictionLevelCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCountryCode = NormalizeCountryCode(countryCode);
        var normalizedLevel = NormalizeJurisdictionLevel(jurisdictionLevelCode);
        var query = db.지역문화공공기관Sources
            .AsNoTracking()
            .AsQueryable();

        if (normalizedCountryCode is not null)
        {
            query = query.Where(item => item.CountryCode == normalizedCountryCode);
        }

        if (normalizedLevel is not null)
        {
            query = query.Where(item => item.JurisdictionLevelCode == normalizedLevel);
        }

        var items = await query
            .OrderBy(item => item.CountryCode)
            .ThenBy(item => item.JurisdictionLevelCode)
            .ThenBy(item => item.InstitutionNameEn)
            .Select(item => new RegionalCulturePublicInstitutionSourceDto(
                item.SourceKey,
                item.CountryCode,
                item.JurisdictionLevelCode,
                item.SourceKindCode,
                item.InstitutionNameKo,
                item.InstitutionNameEn,
                item.SupervisingInstitutionNameKo,
                item.ResponsibilitySummaryKo,
                item.RegionKeyPattern,
                item.GeographicIdentifierScheme,
                item.OfficialPageUrl,
                item.DataUrl,
                item.DataFormatCode,
                item.IsMachineReadable,
                item.RefreshCycleCode,
                item.RequiresRegionalVerification,
                item.LimitationsKo,
                item.EvidenceCheckedAtUtc,
                item.SourceVersion))
            .ToArrayAsync(cancellationToken);

        return new RegionalCulturePublicInstitutionSourceListResponse(
            normalizedCountryCode,
            normalizedLevel,
            items.Length,
            items);
    }

    private static string? NormalizeCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        var normalized = countryCode.Trim().ToUpperInvariant();
        if (!RegionalCulturePublicInstitutionCountryCodes.All.Contains(
                normalized,
                StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"CountryCode는 {string.Join(", ", RegionalCulturePublicInstitutionCountryCodes.All)} 중 하나여야 합니다.",
                nameof(countryCode));
        }

        return normalized;
    }

    private static string? NormalizeJurisdictionLevel(string? jurisdictionLevelCode)
    {
        if (string.IsNullOrWhiteSpace(jurisdictionLevelCode))
        {
            return null;
        }

        var supported = new[]
        {
            RegionalCulturePublicInstitutionJurisdictionLevels.National,
            RegionalCulturePublicInstitutionJurisdictionLevels.StateProvince,
            RegionalCulturePublicInstitutionJurisdictionLevels.CountyMunicipality,
            RegionalCulturePublicInstitutionJurisdictionLevels.Neighborhood,
            RegionalCulturePublicInstitutionJurisdictionLevels.MultiLevelDirectory
        };
        var normalized = supported.FirstOrDefault(item => item.Equals(
            jurisdictionLevelCode.Trim(),
            StringComparison.OrdinalIgnoreCase));
        return normalized ?? throw new ArgumentException(
            $"지원하지 않는 JurisdictionLevelCode입니다: {jurisdictionLevelCode}",
            nameof(jurisdictionLevelCode));
    }
}
