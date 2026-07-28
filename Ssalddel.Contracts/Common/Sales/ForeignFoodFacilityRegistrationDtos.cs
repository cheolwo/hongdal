namespace Ssalddel.Contracts.Common.Sales;

public static class 해외판매자식품시설신청자유형코드
{
    public const string 해외시설운영자 = "ForeignFacilityOperator";
    public const string 국내수입자 = "KoreanImporter";
}

public static class 해외판매자식품시설등록유형코드
{
    public const string 신규 = "Initial";
    public const string 변경 = "Update";
    public const string 갱신 = "Renewal";
}

public static class 한국수입식품절차코드
{
    public const string 해외제조업소등록 = "ForeignFoodFacility";
    public const string 동물성식품수출국정부신청 = "AnimalFoodViaExportingGovernment";
    public const string 축산물해외작업장수출국정부신청 = "ForeignEstablishmentViaExportingGovernment";
    public const string 수산물정부경로검토 = "FisheryGovernmentReview";
}

public static class 해외판매자식품시설품목코드
{
    public const string 농산물 = "AgriculturalProducts";
    public const string 가공식품 = "ProcessedFoods";
    public const string 기구용기포장 = "ApparatusContainersPackages";
    public const string 수산물 = "FisheryProducts";
    public const string 식품첨가물 = "FoodAdditives";
    public const string 건강기능식품 = "HealthFunctionalFoods";
    public const string 동물성식품 = "AnimalBasedFoods";
    public const string 축산물 = "LivestockProducts";
}

public static class 해외판매자식품시설업종코드
{
    public const string 식품첨가물제조가공 = "FoodOrAdditiveManufacturing";
    public const string 기구용기포장제조 = "ApparatusContainerPackageManufacturing";
    public const string 농산물포장 = "AgriculturalProductPacking";
    public const string 건강기능식품제조 = "HealthFunctionalFoodManufacturing";
    public const string 수산물가공 = "FisheryProcessing";
    public const string 어선 = "FishingVessel";
    public const string 양식장 = "Aquaculture";
}

public static class 해외판매자식품시설안전관리코드
{
    public const string Haccp = "HACCP";
    public const string Iso22000 = "ISO22000";
    public const string Gmp = "GMP";
    public const string Gfsi = "GFSI";
    public const string 기타 = "Other";
}

public static class 해외판매자식품시설증빙유형코드
{
    public const string 수출국허가등록증빙 = "ExportingCountryAuthorization";
    public const string 등록확인서대체서식 = "RegistrationConfirmation";
    public const string 공식영문확인 = "OfficialEnglishStatement";
    public const string 공증번역 = "NotarizedTranslation";
    public const string 식품안전인증 = "FoodSafetyCertification";
    public const string 정부경로확인 = "GovernmentChannelConfirmation";
}

public sealed class 해외판매자식품시설증빙요청
{
    public string 문서Id { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 증빙유형 { get; set; } = string.Empty;
    public string 발급기관 { get; set; } = string.Empty;
    public string 언어코드 { get; set; } = "en";
    public DateOnly? 발급일 { get; set; }
    public DateOnly? 만료일 { get; set; }
}

public sealed class 해외판매자식품시설저장요청
{
    public long? 기대Revision { get; set; }
    public string 판매자업체명 { get; set; } = string.Empty;
    public string 판매자국가코드 { get; set; } = string.Empty;
    public string 판매자현지등록번호 { get; set; } = string.Empty;
    public string 판매자담당자명 { get; set; } = string.Empty;
    public string 판매자이메일 { get; set; } = string.Empty;
    public string 판매자전화번호 { get; set; } = string.Empty;
    public bool 판매자가시설운영자인가 { get; set; }

    public string 신청자유형 { get; set; } = 해외판매자식품시설신청자유형코드.해외시설운영자;
    public string 등록유형 { get; set; } = 해외판매자식품시설등록유형코드.신규;
    public string 기존식약처등록코드 { get; set; } = string.Empty;
    public DateOnly? 식약처등록일 { get; set; }
    public DateOnly? 식약처등록만료일 { get; set; }

    public string 시설명 { get; set; } = string.Empty;
    public string 시설대표자명 { get; set; } = string.Empty;
    public string 시설주소 { get; set; } = string.Empty;
    public string 시설국가코드 { get; set; } = string.Empty;
    public string 시설전화번호 { get; set; } = string.Empty;
    public string 시설이메일 { get; set; } = string.Empty;
    public List<string> 생산품목코드목록 { get; set; } = [];
    public List<string> 업종코드목록 { get; set; } = [];
    public List<string> 안전관리인증코드목록 { get; set; } = [];
    public string 기타안전관리인증명 { get; set; } = string.Empty;

    public string 국내수입업체명 { get; set; } = string.Empty;
    public string 국내수입업체주소 { get; set; } = string.Empty;
    public string 국내수입업체전화번호 { get; set; } = string.Empty;
    public string 국내수입업체이메일 { get; set; } = string.Empty;
    public string 국내수입식품영업등록번호 { get; set; } = string.Empty;
    public bool 국내수입업체확인여부 { get; set; }

    public List<해외판매자식품시설증빙요청> 증빙목록 { get; set; } = [];
    public bool 현지실사동의여부 { get; set; }
    public bool 정보진실성확인여부 { get; set; }
    public bool 시설운영자동의여부 { get; set; }
    public bool 수산물정부경로검토필요여부 { get; set; }
}

public sealed class 해외판매자식품시설응답
{
    public string 프로필Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public 해외판매자식품시설저장요청 등록정보 { get; set; } = new();
    public string 적용절차코드 { get; set; } = 한국수입식품절차코드.해외제조업소등록;
    public string 다음조치 { get; set; } = string.Empty;
    public bool 시설등록준비완료여부 { get; set; }
    public bool 한국수입준비완료여부 { get; set; }
    public IReadOnlyList<string> 차단사유목록 { get; set; } = [];
    public IReadOnlyList<string> 주의사항목록 { get; set; } = [];
    public bool 외부신고발생여부 { get; set; }
    public string 실행모드 { get; set; } = "Simulation";
    public IReadOnlyList<string> 공식근거Url목록 { get; set; } = [];
}

public sealed class 해외판매자식품시설목록응답
{
    public IReadOnlyList<해외판매자식품시설응답> Items { get; set; } = [];
}
