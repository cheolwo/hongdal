using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public static class 대한민국행정동관할CodeDataset
{
    public const string SourceId = "mois-resident-registration-codes";
    public const string DatasetId = "korea-administrative-legal-jurisdictions";
    public const string AdministrativeMetricCode = "geography.administrative-agency.name";
    public const string JurisdictionMetricCode = "geography.administrative-legal-jurisdiction";
}

public sealed class 대한민국행정동관할CodeOptions
{
    public const string SectionName = "ExternalData:Korea:AdministrativeJurisdictions";

    public string ArchiveUrl { get; set; } =
        "https://www.mois.go.kr/cmm/fms/FileDown.do?atchFileId=FILE_00142941amDLY7b&fileSn=1";
    public int MaxArchiveBytes { get; set; } = 10 * 1024 * 1024;
    public int MaxExpandedBytes { get; set; } = 30 * 1024 * 1024;
    public int MaxRecordCount { get; set; } = 100_000;
}

public sealed class 대한민국행정동관할CodeSourceRegistration : IExternalDataSourceRegistration
{
    private static readonly ExternalDataSourceDefinition Definition = new()
    {
        SourceId = 대한민국행정동관할CodeDataset.SourceId,
        DatasetId = 대한민국행정동관할CodeDataset.DatasetId,
        Name = "행정기관(행정동) 및 관할구역(법정동) 코드",
        Provider = "행정안전부 주민과",
        CountryCode = "KOR",
        DataDomain = "AdministrativeGeography",
        OfficialSourceUrl =
            "https://www.mois.go.kr/frt/bbs/type001/commonSelectBoardArticle.do?bbsId=BBSMSTR_000000000052&nttId=124059",
        DocumentationUrl =
            "https://www.mois.go.kr/frt/bbs/type001/commonSelectBoardArticle.do?bbsId=BBSMSTR_000000000052&nttId=124059",
        AccessMethod = ExternalDataAccessMethod.DownloadFile,
        CredentialType = ExternalDataCredentialType.None,
        RequiresCredential = false,
        DefaultCollectionEnabled = false,
        ApiAvailable = false,
        DataFormat = "ZIP 안의 CP949 고정폭 KIKcd_H·KIKmix 텍스트",
        SpatialResolution = "대한민국 행정기관과 관할 법정동 관계",
        TemporalResolution = "시행 기준일",
        RefreshCadence = "행정안전부 변경내역 시행본 게시 시",
        License = "공급처 이용조건 확인 필요",
        RedistributionAllowed = false,
        AttributionRequirement = "행정안전부 행정기관·관할구역 코드 시행 기준일 표시",
        UsageLimitations = "행정기관과 관할 법정동 코드 관계이며 행정동 경계 도형, 건물 위치와 개인 주소를 포함하지 않습니다.",
        LastVerifiedDate = new DateOnly(2026, 8, 13),
    };

    public IReadOnlyCollection<ExternalDataSourceDefinition> GetDefinitions() => [Definition];
}
