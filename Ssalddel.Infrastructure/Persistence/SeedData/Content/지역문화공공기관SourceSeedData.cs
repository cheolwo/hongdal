using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.Content;
using 살뜰.Data;

namespace Ssalddel.Infrastructure.Persistence.SeedData.Content;

public static class 지역문화공공기관SourceSeeder
{
    public static async Task<int> SeedAsync(
        SsalddelContext db,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.지역문화공공기관Sources
            .ToDictionaryAsync(item => item.SourceKey, StringComparer.Ordinal, cancellationToken);
        var changed = 0;

        foreach (var seed in 지역문화공공기관SourceSeedData.All)
        {
            if (!existing.TryGetValue(seed.SourceKey, out var current))
            {
                db.지역문화공공기관Sources.Add(seed);
                changed++;
                continue;
            }

            if (current.SourceVersion >= seed.SourceVersion)
            {
                continue;
            }

            ApplySeed(current, seed);
            changed++;
        }

        if (changed > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return changed;
    }

    private static void ApplySeed(
        지역문화공공기관Source target,
        지역문화공공기관Source seed)
    {
        target.CountryCode = seed.CountryCode;
        target.JurisdictionLevelCode = seed.JurisdictionLevelCode;
        target.SourceKindCode = seed.SourceKindCode;
        target.InstitutionNameKo = seed.InstitutionNameKo;
        target.InstitutionNameEn = seed.InstitutionNameEn;
        target.SupervisingInstitutionNameKo = seed.SupervisingInstitutionNameKo;
        target.ResponsibilitySummaryKo = seed.ResponsibilitySummaryKo;
        target.RegionKeyPattern = seed.RegionKeyPattern;
        target.GeographicIdentifierScheme = seed.GeographicIdentifierScheme;
        target.OfficialPageUrl = seed.OfficialPageUrl;
        target.DataUrl = seed.DataUrl;
        target.DataFormatCode = seed.DataFormatCode;
        target.IsMachineReadable = seed.IsMachineReadable;
        target.RefreshCycleCode = seed.RefreshCycleCode;
        target.RequiresRegionalVerification = seed.RequiresRegionalVerification;
        target.LimitationsKo = seed.LimitationsKo;
        target.EvidenceCheckedAtUtc = seed.EvidenceCheckedAtUtc;
        target.SourceVersion = seed.SourceVersion;
        target.UpdatedAtUtc = seed.UpdatedAtUtc;
    }
}

internal static class 지역문화공공기관SourceSeedData
{
    private static readonly DateTime CheckedAtUtc =
        new(2026, 7, 26, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<지역문화공공기관Source> All { get; } =
    [
        Create(
            "kr-mcst-regional-culture-policy",
            "KR",
            RegionalCulturePublicInstitutionJurisdictionLevels.National,
            RegionalCulturePublicInstitutionSourceKinds.GovernmentOffice,
            "문화체육관광부 지역문화정책과",
            "Ministry of Culture, Sports and Tourism, Regional Culture Policy Division",
            "문화체육관광부",
            "지역문화 정책, 생활문화, 문화도시, 지역문화재단과 문화의 집 관련 중앙 정책을 담당하는 공식 부서입니다.",
            "kr-*",
            "KR administrative area codes",
            "https://www.mcst.go.kr/site/s_about/organ/staff/staffGuide001.jsp?pDeptCode=0721000000&pIntro=&pTeamCD=1371746",
            "https://www.mcst.go.kr/site/s_about/organ/staff/staffGuide001.jsp?pDeptCode=0721000000&pIntro=&pTeamCD=1371746",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "OnChange",
            "담당 부서와 업무 분장은 조직개편으로 바뀔 수 있으므로 표시 전에 공식 조직 안내를 다시 확인합니다."),
        Create(
            "kr-regional-culture-promotion-agency",
            "KR",
            RegionalCulturePublicInstitutionJurisdictionLevels.National,
            RegionalCulturePublicInstitutionSourceKinds.PublicInstitution,
            "지역문화진흥원",
            "Regional Culture Promotion Agency",
            "문화체육관광부",
            "지역문화·생활문화 정책 지원, 기관 협력, 조사·연구와 지역문화 정보의 수집·공유를 수행하는 관계 기관입니다.",
            "kr-*",
            "KR administrative area codes",
            "https://www.mcst.go.kr/site/s_data/corpNaru/corpView.jsp?pSeq=615",
            "https://www.mcst.go.kr/site/s_data/corpNaru/corpView.jsp?pSeq=615",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "OnChange",
            "비영리법인 현황은 기관의 개별 사업·공모·시설 현황 전체를 대신하지 않습니다."),
        Create(
            "kr-mois-administrative-agency-jurisdiction",
            "KR",
            RegionalCulturePublicInstitutionJurisdictionLevels.Neighborhood,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDataset,
            "행정안전부 주민등록업무 행정기관 및 관할구역",
            "MOIS Resident Registration Administrative Agencies and Jurisdictions",
            "행정안전부",
            "시·도, 시·군·구, 읍·면·동과 출장소 등 주민등록 행정기관 코드 및 관할 법정동 관계를 확인하는 기준 자료입니다.",
            "kr-{administrative-dong-code}",
            "MOIS resident-registration administrative agency code",
            "https://www.data.go.kr/data/15095148/fileData.do?recommendDataYn=Y",
            "https://www.data.go.kr/data/15095148/fileData.do?recommendDataYn=Y",
            RegionalCulturePublicInstitutionDataFormats.File,
            true,
            "OnChange",
            "행정동 코드와 법정동 코드는 목적이 다릅니다. 주민센터의 실제 명칭·주소·문화 프로그램은 해당 지방자치단체의 최신 자료로 별도 확인합니다."),
        Create(
            "kr-national-museum-art-museum-standard-data",
            "KR",
            RegionalCulturePublicInstitutionJurisdictionLevels.CountyMunicipality,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDataset,
            "전국박물관미술관정보표준데이터",
            "Korea Standard Data for Museums and Art Museums",
            "공공데이터포털·각 제공기관",
            "국공립·사립 박물관과 미술관의 명칭, 소재지, 위치, 운영기관과 관리기관을 지역별로 연결하는 표준 자료입니다.",
            "kr-{legal-dong-or-local-government-code}",
            "KR legal-dong or local-government code",
            "https://www.data.go.kr/tcs/dss/selectStdDataDetailView.do?publicDataPk=15017323",
            "https://www.data.go.kr/tcs/dss/selectStdDataDetailView.do?publicDataPk=15017323",
            RegionalCulturePublicInstitutionDataFormats.StandardDataset,
            true,
            "Periodic",
            "시설별 기준일과 제공기관이 다를 수 있으며 운영시간·휴관일은 방문 전에 해당 시설 공식 페이지에서 다시 확인합니다."),
        Create(
            "kr-national-cultural-festival-standard-data",
            "KR",
            RegionalCulturePublicInstitutionJurisdictionLevels.CountyMunicipality,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDataset,
            "전국문화축제표준데이터",
            "Korea Standard Data for Cultural Festivals",
            "공공데이터포털·각 지방자치단체",
            "지역 문화·예술·국가유산·전통·생태를 소재로 한 축제와 주관·후원기관, 장소 및 일정을 지역별로 연결하는 표준 자료입니다.",
            "kr-{legal-dong-or-local-government-code}",
            "KR legal-dong or local-government code",
            "https://www.data.go.kr/data/15013104/standard.do",
            "https://www.data.go.kr/data/15013104/standard.do",
            RegionalCulturePublicInstitutionDataFormats.StandardDataset,
            true,
            "Periodic",
            "축제 일정과 주최·후원 정보는 변경 또는 취소될 수 있으므로 행사 공식 공고를 다시 확인합니다."),
        Create(
            "kr-khs-national-heritage-portal",
            "KR",
            RegionalCulturePublicInstitutionJurisdictionLevels.StateProvince,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDataset,
            "국가유산청 국가유산포털",
            "Korea Heritage Service National Heritage Portal",
            "국가유산청",
            "국가·시도 지정 문화유산, 자연유산과 무형유산을 지역·종목별로 확인하고 관련 관리기관 근거를 연결하는 공식 포털입니다.",
            "kr-{province-or-local-government-code}",
            "KR province and local-government code",
            "https://www.heritage.go.kr/main/",
            "https://www.heritage.go.kr/heri/cul/culSelectViewList.do?ccbaPcd1=99&gbn=2&pageNo=1_1_2_0&region=2&s_ctcd=00&s_kdcd=00",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "OnChange",
            "시도 지정종목은 지자체의 종목 변경 처리 시점에 따라 표시가 다를 수 있습니다. 이미지·도면·3D 자료는 항목별 이용조건도 확인합니다."),
        Create(
            "us-nea-state-regional-arts-organizations",
            "US",
            RegionalCulturePublicInstitutionJurisdictionLevels.StateProvince,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDirectory,
            "미국 국립예술기금 주·지역 예술기관 디렉터리",
            "National Endowment for the Arts State and Regional Arts Organizations",
            "National Endowment for the Arts",
            "50개 주와 관할구역의 예술기관 및 여러 주를 묶는 지역 예술기관을 공식적으로 연결하는 디렉터리입니다.",
            "us-{state-alpha}",
            "ISO 3166-2:US and USPS state abbreviation",
            "https://www.arts.gov/state-and-regional-arts-organizations",
            "https://www.arts.gov/state-and-regional-arts-organizations",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "OnChange",
            "주 예술기관은 각 주의 모든 문화·유산 업무를 대표하지 않습니다. 기관 링크와 담당 범위를 해당 주정부 페이지에서 다시 확인합니다."),
        Create(
            "us-nps-state-historic-preservation-offices",
            "US",
            RegionalCulturePublicInstitutionJurisdictionLevels.StateProvince,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDirectory,
            "미국 국립공원관리청 주 역사보존실 디렉터리",
            "National Park Service State Historic Preservation Offices Directory",
            "National Park Service",
            "각 주의 역사적 건물·유적·구역 조사, 평가와 국가사적지 등록 협의를 담당하는 주 역사보존실을 연결합니다.",
            "us-{state-alpha}",
            "ISO 3166-2:US and USPS state abbreviation",
            "https://www.nps.gov/subjects/nationalregister/state-historic-preservation-offices.htm",
            "https://www.nps.gov/subjects/nationalregister/state-historic-preservation-offices.htm",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "OnChange",
            "문화 전반이 아니라 역사·고고·건축 자원 보존 책임에 초점을 둔 기관입니다. 부족 역사보존실은 별도 주권 기관으로 구분합니다."),
        Create(
            "us-census-geographic-information",
            "US",
            RegionalCulturePublicInstitutionJurisdictionLevels.MultiLevelDirectory,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDataset,
            "미국 인구조사국 지리정보·Gazetteer",
            "U.S. Census Bureau Geographic Information and Gazetteer Files",
            "U.S. Census Bureau",
            "주, 카운티, 카운티 하위구역, place 등 지역 단위의 공식 지리 코드·명칭·좌표를 기관과 문화 자료의 공통 지역 키로 사용합니다.",
            "us-{state-fips}-{local-geoid}",
            "Census GEOID, FIPS and GNIS",
            "https://www.census.gov/data/developers/geography.html",
            "https://www.census.gov/geographies/reference-files/time-series/geo/gazetteer-files.html",
            RegionalCulturePublicInstitutionDataFormats.File,
            true,
            "Annual",
            "Census place와 실제 지방정부가 항상 일대일로 대응하지 않습니다. 정부 단위 판정은 Government Units 자료와 함께 확인합니다."),
        Create(
            "us-census-government-units",
            "US",
            RegionalCulturePublicInstitutionJurisdictionLevels.MultiLevelDirectory,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDataset,
            "미국 인구조사국 주·지방정부 조직 공개 파일",
            "U.S. Census Bureau Annual Organization Public Use Files",
            "U.S. Census Bureau",
            "주정부와 카운티·municipal·township·special district 정부 및 주요 기관을 공식 정부 단위로 식별하는 공개 파일입니다.",
            "us-{government-unit-code}",
            "Census government unit code and FIPS",
            "https://www.census.gov/topics/public-sector/government-organization/about.html",
            "https://www.census.gov/programs-surveys/gus/data/publicusefiles.html",
            RegionalCulturePublicInstitutionDataFormats.File,
            true,
            "Annual",
            "정부 단위 목록은 문화 담당 부서나 프로그램 세부 내용을 직접 제공하지 않으므로 해당 정부 공식 사이트와 연결해 보완해야 합니다."),
        Create(
            "us-usa-gov-local-governments",
            "US",
            RegionalCulturePublicInstitutionJurisdictionLevels.CountyMunicipality,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDirectory,
            "USA.gov 지방정부 디렉터리",
            "USA.gov Local Governments Directory",
            "U.S. General Services Administration",
            "주별 카운티·시·타운 등 지방정부 공식 연락처와 사이트를 찾아 문화·공원·도서관 담당 부서로 이어지는 시작점입니다.",
            "us-{state-alpha}-{local-government}",
            "State and local government name",
            "https://www.usa.gov/state-local-governments",
            "https://www.usa.gov/local-governments",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "OnChange",
            "한국의 행정복지센터와 같은 단일 전국 조직 체계로 간주하지 않습니다. 주마다 county·city·township의 권한과 명칭이 다릅니다."),
        Create(
            "us-imls-public-libraries-survey",
            "US",
            RegionalCulturePublicInstitutionJurisdictionLevels.CountyMunicipality,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDataset,
            "미국 박물관·도서관서비스기구 공공도서관 조사",
            "Institute of Museum and Library Services Public Libraries Survey",
            "Institute of Museum and Library Services",
            "미국 공공도서관 시스템과 분관·이동도서관의 위치 및 서비스 정보를 지역 문화 거점으로 연결하는 연례 자료입니다.",
            "us-{state-fips}-{library-id}",
            "IMLS library identifiers and state codes",
            "https://www.imls.gov/research-evaluation/surveys/public-libraries-survey-pls",
            "https://www.imls.gov/research-evaluation/surveys/public-libraries-survey-pls",
            RegionalCulturePublicInstitutionDataFormats.File,
            true,
            "Annual",
            "도서관은 지방정부 부서, 독립 district 또는 다른 운영형태일 수 있습니다. 문화행정기관 여부와 서비스권역을 별도 확인합니다."),
        Create(
            "cn-mct-intangible-cultural-heritage-department",
            "CN",
            RegionalCulturePublicInstitutionJurisdictionLevels.National,
            RegionalCulturePublicInstitutionSourceKinds.GovernmentOffice,
            "중국 문화여유부 비물질문화유산사",
            "Ministry of Culture and Tourism, Department of Intangible Cultural Heritage",
            "중화인민공화국 문화여유부",
            "국가 비물질문화유산 보호 정책·조사·기록·대표목록과 전승 업무를 담당하는 중앙 행정부서입니다.",
            "cn-*",
            "GB/T 2260 and ISO 3166-2:CN subdivision codes",
            "https://www.mct.gov.cn/gywhb/jgsz/bjg_jgsz/202205/t20220512_932945.htm",
            "https://www.mct.gov.cn/whzx/bnsj/fwzwhycs/",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "OnChange",
            "중앙부처 업무와 정책은 개별 성급 지역의 현재 생활문화·민족 공동체 표현을 직접 검증하지 않습니다. 생성 전 성급 문화여유 부서와 현지 기관 자료를 추가 확인합니다.",
            new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc)),
        Create(
            "cn-ihchina-national-register",
            "CN",
            RegionalCulturePublicInstitutionJurisdictionLevels.StateProvince,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDirectory,
            "중국 비물질문화유산망 국가급 대표항목 명록",
            "China Intangible Cultural Heritage National Representative List",
            "중화인민공화국 문화여유부·중국예술연구원",
            "국가급 비물질문화유산 대표항목을 성·자치구·직할시와 항목 유형별로 확인하는 공식 명록입니다.",
            "cn-{province-code}",
            "GB/T 2260 and province-level application region",
            "https://www.ihchina.cn/",
            "https://www.ihchina.cn/project",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "Periodic",
            "국가급 명록에 포함된 항목만 다루며 한 항목이 해당 성 전체의 현재 생활문화를 대표하지 않습니다. 보호단위와 실제 전승 지역도 개별 항목에서 확인합니다.",
            new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc)),
        Create(
            "cn-state-council-local-government-directory",
            "CN",
            RegionalCulturePublicInstitutionJurisdictionLevels.StateProvince,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDirectory,
            "중국정부망 성급 지방정부 웹사이트 디렉터리",
            "State Council Provincial-level Local Government Website Directory",
            "중화인민공화국 국무원",
            "31개 성·자치구·직할시의 공식 인민정부 사이트로 연결해 성급 문화여유 부서와 공개자료를 재확인하는 출발점입니다.",
            "cn-{province-code}",
            "GB/T 2260 and provincial government jurisdiction",
            "https://www.gov.cn/home/2023-03/29/content_5748954.htm",
            "https://gjzwfw.www.gov.cn/",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "OnChange",
            "지방정부 사이트 디렉터리는 문화 담당 부서·박물관·전승 공동체의 구체 근거가 아닙니다. 각 지역의 최신 조직과 원문 자료를 다시 확인해야 합니다.",
            new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc)),
        Create(
            "cn-ncha-public-information-service",
            "CN",
            RegionalCulturePublicInstitutionJurisdictionLevels.MultiLevelDirectory,
            RegionalCulturePublicInstitutionSourceKinds.OfficialDataset,
            "중국 국가문물국 공공정보 서비스",
            "National Cultural Heritage Administration Public Information Service",
            "중국 국가문물국",
            "전국 중점문물보호단위, 역사문화도시·진·촌·거리와 박물관 명록을 성별로 확인하는 공식 정보 서비스입니다.",
            "cn-{province-code}-{locality}",
            "Province and locality names in national cultural heritage registers",
            "https://www.ncha.gov.cn/",
            "https://www.ncha.gov.cn/col/col2262/index.html",
            RegionalCulturePublicInstitutionDataFormats.WebPage,
            false,
            "Periodic",
            "문물·박물관 명록은 현재 주민 생활이나 모든 문화 활동을 대표하지 않습니다. 건축·유산 표현은 지정 범위·시대·현재 용도를 항목별로 확인합니다.",
            new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc))
    ];

    private static 지역문화공공기관Source Create(
        string sourceKey,
        string countryCode,
        string jurisdictionLevelCode,
        string sourceKindCode,
        string institutionNameKo,
        string institutionNameEn,
        string supervisingInstitutionNameKo,
        string responsibilitySummaryKo,
        string regionKeyPattern,
        string geographicIdentifierScheme,
        string officialPageUrl,
        string dataUrl,
        string dataFormatCode,
        bool isMachineReadable,
        string refreshCycleCode,
        string limitationsKo,
        DateTime? evidenceCheckedAtUtc = null)
        => new()
        {
            SourceKey = sourceKey,
            CountryCode = countryCode,
            JurisdictionLevelCode = jurisdictionLevelCode,
            SourceKindCode = sourceKindCode,
            InstitutionNameKo = institutionNameKo,
            InstitutionNameEn = institutionNameEn,
            SupervisingInstitutionNameKo = supervisingInstitutionNameKo,
            ResponsibilitySummaryKo = responsibilitySummaryKo,
            RegionKeyPattern = regionKeyPattern,
            GeographicIdentifierScheme = geographicIdentifierScheme,
            OfficialPageUrl = officialPageUrl,
            DataUrl = dataUrl,
            DataFormatCode = dataFormatCode,
            IsMachineReadable = isMachineReadable,
            RefreshCycleCode = refreshCycleCode,
            RequiresRegionalVerification = true,
            LimitationsKo = limitationsKo,
            EvidenceCheckedAtUtc = evidenceCheckedAtUtc ?? CheckedAtUtc,
            SourceVersion = 1,
            CreatedAtUtc = evidenceCheckedAtUtc ?? CheckedAtUtc,
            UpdatedAtUtc = evidenceCheckedAtUtc ?? CheckedAtUtc
        };
}
