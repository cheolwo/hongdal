using Hongdal.Contracts.Common.PublicData;

namespace 홍달.Services.External.PublicData;

public sealed class PublicDataApiMetadataCatalog : IPublicDataApiMetadataCatalog
{
    private static readonly IReadOnlyList<PublicDataApiMetadataItem> Catalog =
    [
        new()
        {
            Key = "juso-road-address-search",
            Provider = "행정안전부",
            DisplayName = "실시간 주소정보 조회 API",
            Purpose = "사용자가 입력한 주소를 도로명/지번/우편번호 기준으로 표준화합니다.",
            Domain = "Address",
            VersionScope = "2.5",
            ApiType = "REST",
            DataFormat = "JSON/XML",
            BaseUrl = "https://business.juso.go.kr",
            DocumentationUrl = "https://www.data.go.kr/data/15057017/openapi.do",
            RequiresServiceKey = true,
            ContainsResidentialData = true,
            ContainsPersonalData = false,
            MainParameters = ["confmKey", "currentPage", "countPerPage", "keyword", "resultType"],
            MainResponseFields = ["roadAddr", "jibunAddr", "zipNo", "admCd", "rnMgtSn", "bdMgtSn"],
            UsageNotes =
            [
                "공동주택 단지 후보 조회 전에 주소 문자열을 표준화하는 1차 관문으로 사용합니다.",
                "상세주소, 동/호 정보는 직접 공개하지 않고 내부 단지 소속 확인 흐름에서만 사용합니다."
            ]
        },
        new()
        {
            Key = "kapt-apartment-complex-list",
            Provider = "국토교통부",
            DisplayName = "공동주택 단지 목록제공 서비스",
            Purpose = "시도/시군구/읍면동/도로명주소 기반으로 공동주택 단지 후보를 조회합니다.",
            Domain = "ApartmentComplex",
            VersionScope = "2.5",
            ApiType = "REST",
            DataFormat = "JSON/XML",
            BaseUrl = "https://apis.data.go.kr",
            DocumentationUrl = "https://www.data.go.kr/data/15057332/openapi.do",
            RequiresServiceKey = true,
            ContainsResidentialData = true,
            ContainsPersonalData = false,
            MainParameters = ["serviceKey", "sidoCode", "sigunguCode", "dongCode", "roadName"],
            MainResponseFields = ["kaptCode", "kaptName", "as1", "as2", "as3", "as4"],
            UsageNotes =
            [
                "외부 단지 코드는 내부 공동주택 식별자로 바로 노출하지 않고 매핑 테이블을 둡니다.",
                "동명이거나 주소가 유사한 단지가 있을 수 있으므로 주소 표준화 결과와 함께 후보 상태로 보관합니다."
            ]
        },
        new()
        {
            Key = "kapt-apartment-complex-basic",
            Provider = "국토교통부",
            DisplayName = "공동주택 기본 정보제공 서비스",
            Purpose = "단지의 동수, 세대수, 관리 방식, 설비 등 기본 정보를 확인합니다.",
            Domain = "ApartmentComplex",
            VersionScope = "2.5",
            ApiType = "REST",
            DataFormat = "JSON/XML",
            BaseUrl = "https://apis.data.go.kr",
            DocumentationUrl = "https://www.data.go.kr/data/15058453/openapi.do",
            RequiresServiceKey = true,
            ContainsResidentialData = true,
            ContainsPersonalData = false,
            MainParameters = ["serviceKey", "kaptCode"],
            MainResponseFields = ["kaptCode", "kaptName", "hoCnt", "dongCnt", "kaptdWtimebus", "kaptdPcnt"],
            UsageNotes =
            [
                "공동 주문 목표 수량과 단지 내 분류 규모를 추정할 때 참고 정보로 사용합니다.",
                "관리사무소 승인 또는 공식 협약을 대체하는 데이터로 사용하지 않습니다."
            ]
        },
        new()
        {
            Key = "standard-apartment-complex-data",
            Provider = "국토교통부",
            DisplayName = "전국공동주택표준데이터",
            Purpose = "공동주택 표준 데이터셋을 보강 데이터로 활용합니다.",
            Domain = "ApartmentComplex",
            VersionScope = "2.5",
            ApiType = "REST/File",
            DataFormat = "JSON/CSV",
            BaseUrl = "https://www.data.go.kr",
            DocumentationUrl = "https://www.data.go.kr/data/15096285/standard.do",
            RequiresServiceKey = true,
            ContainsResidentialData = true,
            ContainsPersonalData = false,
            MainParameters = ["serviceKey", "page", "perPage", "cond"],
            MainResponseFields = ["단지명", "법정동주소", "도로명주소", "세대수", "동수"],
            UsageNotes =
            [
                "K-apt 단지 목록과 주소 검색 API 결과를 보강하는 용도로 사용합니다.",
                "갱신 주기와 제공 필드가 API별로 다를 수 있으므로 동기화 시점을 기록합니다."
            ]
        },
        new()
        {
            Key = "customs-cargo-tracking",
            Provider = "관세청/공공데이터포털",
            DisplayName = "화물 통관 진행 정보 조회",
            Purpose = "수입 화물의 통관 진행 상태를 조회해 FCL/LCL 운송 계획에 참고합니다.",
            Domain = "Customs",
            VersionScope = "2.0",
            ApiType = "REST",
            DataFormat = "XML",
            BaseUrl = "https://apis.data.go.kr",
            DocumentationUrl = "https://www.data.go.kr",
            RequiresServiceKey = true,
            ContainsResidentialData = false,
            ContainsPersonalData = false,
            MainParameters = ["serviceKey", "cargMtNo", "mblNo", "hblNo"],
            MainResponseFields = ["csclPrgsStts", "shedNm", "prcsDttm"],
            UsageNotes =
            [
                "통관 상태는 운송 가능성 판단의 참고값이며, 기본 운송 요청을 무조건 차단하는 게이트로 사용하지 않습니다.",
                "조회 결과 원문과 해석 결과를 분리해 저장합니다."
            ]
        }
    ];

    public PublicDataApiMetadataResponse GetCatalog(PublicDataApiMetadataQuery query)
    {
        IEnumerable<PublicDataApiMetadataItem> items = Catalog;

        if (!string.IsNullOrWhiteSpace(query.Domain))
        {
            items = items.Where(item => string.Equals(item.Domain, query.Domain, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.VersionScope))
        {
            items = items.Where(item => string.Equals(item.VersionScope, query.VersionScope, StringComparison.OrdinalIgnoreCase));
        }

        if (query.ContainsResidentialData.HasValue)
        {
            items = items.Where(item => item.ContainsResidentialData == query.ContainsResidentialData.Value);
        }

        return new PublicDataApiMetadataResponse
        {
            Items = items
                .OrderBy(item => item.VersionScope, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.Domain, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };
    }
}
