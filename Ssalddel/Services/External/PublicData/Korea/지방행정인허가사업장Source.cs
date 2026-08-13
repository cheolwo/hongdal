using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed class 지방행정인허가사업장SourceRegistration : IExternalDataSourceRegistration
{
    private static readonly ExternalDataSourceDefinition Definition = new()
    {
        SourceId = 지방행정인허가사업장ImportService.SourceId,
        DatasetId = 지방행정인허가사업장ImportService.DatasetId,
        Name = "지방행정 인허가 공개 사업장 전체분",
        Provider = "행정안전부 지방행정인허가데이터개방",
        CountryCode = "KOR",
        DataDomain = "LicensedBusiness",
        OfficialSourceUrl = "https://www.localdata.go.kr/devcenter/dataDown.do?menuNo=20001",
        DocumentationUrl = "https://www.localdata.go.kr/portal/portalDataGuide.do?menuNo=30002",
        AccessMethod = ExternalDataAccessMethod.DownloadFile,
        CredentialType = ExternalDataCredentialType.None,
        RequiresCredential = false,
        DefaultCollectionEnabled = false,
        ApiAvailable = false,
        DataFormat = "업종·지역 선택형 CSV 전체 기초분",
        SpatialResolution = "공개 사업장 주소와 공급처 좌표",
        TemporalResolution = "인허가·영업상태·최종수정 시점",
        RefreshCadence = "전체분 월 마감 후 변동분 별도 반영",
        License = "공급처 이용조건과 업종별 개방항목 확인 필요",
        RedistributionAllowed = false,
        AttributionRequirement = "행정안전부 지방행정인허가데이터개방, 원본 기준시점 표시",
        UsageLimitations = "인허가 대상 업종만 포함하며 모든 사업체의 전수명부가 아닙니다. 대표자명·전화번호·사업자등록번호는 기본 정규화 원장에 저장하지 않습니다. 주소 일치는 실제 입주 확정이 아니라 파생 연결입니다.",
        LastVerifiedDate = new DateOnly(2026, 8, 13),
    };

    public IReadOnlyCollection<ExternalDataSourceDefinition> GetDefinitions() => [Definition];
}
