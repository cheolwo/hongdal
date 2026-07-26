using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Content;

public static class RegionalCulturePublicInstitutionCountryCodes
{
    public const string Korea = "KR";
    public const string UnitedStates = "US";

    public static IReadOnlyList<string> All { get; } = [Korea, UnitedStates];
}

public static class RegionalCulturePublicInstitutionJurisdictionLevels
{
    public const string National = "National";
    public const string StateProvince = "StateProvince";
    public const string CountyMunicipality = "CountyMunicipality";
    public const string Neighborhood = "Neighborhood";
    public const string MultiLevelDirectory = "MultiLevelDirectory";
}

public static class RegionalCulturePublicInstitutionSourceKinds
{
    public const string GovernmentOffice = "GovernmentOffice";
    public const string PublicInstitution = "PublicInstitution";
    public const string OfficialDirectory = "OfficialDirectory";
    public const string OfficialDataset = "OfficialDataset";
}

public static class RegionalCulturePublicInstitutionDataFormats
{
    public const string WebPage = "WebPage";
    public const string File = "File";
    public const string OpenApi = "OpenApi";
    public const string StandardDataset = "StandardDataset";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalCulturePublicInstitution,
    SsalddelCodeLayer.Contract,
    "한국과 미국의 지역문화 공공기관 및 공식 디렉터리 근거를 전달",
    FlowOrder = 10,
    Boundary = "기관 디렉터리는 문화 대표성이나 현재 담당 부서를 보장하지 않으며 표시 전 공식 원천을 다시 확인합니다.")]
public sealed record RegionalCulturePublicInstitutionSourceDto(
    string SourceKey,
    string CountryCode,
    string JurisdictionLevelCode,
    string SourceKindCode,
    string InstitutionNameKo,
    string InstitutionNameEn,
    string SupervisingInstitutionNameKo,
    string ResponsibilitySummaryKo,
    string RegionKeyPattern,
    string GeographicIdentifierScheme,
    string OfficialPageUrl,
    string DataUrl,
    string DataFormatCode,
    bool IsMachineReadable,
    string RefreshCycleCode,
    bool RequiresRegionalVerification,
    string LimitationsKo,
    DateTime EvidenceCheckedAtUtc,
    int SourceVersion);

public sealed record RegionalCulturePublicInstitutionSourceListResponse(
    string? CountryCode,
    string? JurisdictionLevelCode,
    int TotalCount,
    IReadOnlyList<RegionalCulturePublicInstitutionSourceDto> Items);
