using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public static class 대한민국법정동CodeDataset
{
    public const string SourceId = "mois-standard-codes";
    public const string DatasetId = "korea-legal-dong-codes";
    public const string MetricCode = "geography.legal-dong.name";
    public const string DownloadPath = "etc/codeFullDown.do";
}

public sealed class 대한민국법정동CodeOptions
{
    public const string SectionName = "ExternalData:Korea:LegalDongCodes";

    public string BaseUrl { get; set; } = "https://www.code.go.kr/";
    public int MaxArchiveBytes { get; set; } = 5 * 1024 * 1024;
    public int MaxExpandedBytes { get; set; } = 20 * 1024 * 1024;
    public int MaxRecordCount { get; set; } = 100_000;
}

public sealed class 대한민국법정동CodeSourceRegistration : IExternalDataSourceRegistration
{
    private static readonly ExternalDataSourceDefinition Definition = new()
    {
        SourceId = 대한민국법정동CodeDataset.SourceId,
        DatasetId = 대한민국법정동CodeDataset.DatasetId,
        Name = "행정표준코드 법정동코드 전체자료",
        Provider = "행정안전부 행정표준코드관리시스템",
        CountryCode = "KOR",
        DataDomain = "AdministrativeGeography",
        OfficialSourceUrl = "https://www.code.go.kr/stdcode/regCodeL.do",
        DocumentationUrl = "https://www.code.go.kr/stdcodesrch/CodeSearchGuide.do",
        AccessMethod = ExternalDataAccessMethod.DownloadFile,
        CredentialType = ExternalDataCredentialType.None,
        RequiresCredential = false,
        DefaultCollectionEnabled = false,
        ApiAvailable = false,
        DataFormat = "ZIP 안의 CP949 탭 구분 텍스트",
        SpatialResolution = "대한민국 법정동 코드 계층",
        TemporalResolution = "공급처 전체자료 갱신 시점",
        RefreshCadence = "공급처 안내상 매주 월요일 오전까지 전체자료 갱신",
        License = "공급처 이용조건 확인 필요",
        RedistributionAllowed = false,
        AttributionRequirement = "행정표준코드관리시스템 법정동코드 전체자료와 수집일 표시",
        UsageLimitations = "법정동 코드와 명칭·폐지 여부 기준자료이며 행정경계 도형, 대표 좌표, 실제 농장 또는 개인 위치를 포함하지 않습니다.",
        LastVerifiedDate = new DateOnly(2026, 8, 11),
    };

    public IReadOnlyCollection<ExternalDataSourceDefinition> GetDefinitions() => [Definition];
}
