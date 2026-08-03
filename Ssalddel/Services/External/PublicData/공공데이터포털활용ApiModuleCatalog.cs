using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;

namespace 살뜰.Services.External.PublicData;

[SsalddelCodeMetadata(
    공공데이터포털활용ApiModuleFeature.Key,
    SsalddelCodeLayer.ExternalAdapter,
    "공공데이터포털 활용 중 33개 API를 10개 업무 모듈로 분류하고 기존 client metadata와 연결",
    ContractType = typeof(공공데이터포털활용ApiModuleResponse),
    FlowOrder = 3,
    Boundary = "공개 UDDI만 보존하고 활용계정 record ID와 인증키는 저장하거나 반환하지 않음")]
public sealed class 공공데이터포털활용ApiModuleCatalog : I공공데이터포털활용ApiModuleCatalog
{
    private static readonly DateOnly VerifiedOn = new(2026, 8, 3);

    private readonly IPublicDataApiMetadataCatalog _publicDataApiMetadataCatalog;

    private static readonly IReadOnlyList<ModuleDefinition> Modules =
    [
        new(
            "mfds-imported-food-evidence",
            "식약처 수입식품 근거",
            "공개 제조·신고 근거 조회에만 사용하며 수입 가능 판정이나 업체 추천을 자동 확정하지 않습니다.",
            [
                Api("uddi:7b3cd17d-f76a-4805-b90a-c6ef930071c9_202110131626", "식품의약품안전처", "수입식품 수입신고 접수 업체"),
                Api("uddi:e9147116-7f79-4008-ad6f-d85289c1b744_202406241519", "식품의약품안전처", "수입식품 해외제조업소 정보", "mfds-imported-food-overseas-manufacturer")
            ]),
        new(
            "mof-fisheries-distribution-reference",
            "해양수산부 수협 유통 기준정보",
            "위판장·조합·창고·재고·입출고·위탁판매를 별도 원천으로 보존하며 거래 가능성이나 재고 소유권을 자동 확정하지 않습니다.",
            [
                Api("uddi:7dbd14ad-61d5-456d-a763-f1b2e1828c3a_202110292122", "해양수산부", "수협 산지조합 위판장 현황", "mof-fisheries-distribution-module"),
                Api("uddi:01931245-3283-4076-8751-c01e68d5b346_202110291733", "해양수산부", "수협 산지조합 위판장 정보", "mof-fisheries-distribution-module"),
                Api("uddi:d716e486-2b43-485b-8ec9-c52a0a96db40_202110291728", "해양수산부", "수협 산지조합 정보", "mof-fisheries-distribution-module"),
                Api("uddi:2ea6729c-9f6b-4f8e-8b2f-874f22bd6a34_202110292126", "해양수산부", "수협 물류센터/공판장 품목별 입출고 현황", "mof-fisheries-distribution-module"),
                Api("uddi:c5a4cca6-1acd-4578-90bb-82106bb5144f_202110292129", "해양수산부", "수협 물류센터/공판장 품목별 재고 현황", "mof-fisheries-distribution-module"),
                Api("uddi:f672801f-e13e-4682-94b8-b004d2cfe7c3_202110292125", "해양수산부", "수협 산지조합 창고 품목별 입출고 현황", "mof-fisheries-distribution-module"),
                Api("uddi:585104ba-68e6-431a-853d-9298f40c6968_202110291945", "해양수산부", "수협 산지조합 창고 정보", "mof-fisheries-distribution-module"),
                Api("uddi:e21e565e-9a78-4f8e-a666-aa0aac42581e_202110292125", "해양수산부", "수협 산지조합 창고 품목별 재고 현황", "mof-fisheries-distribution-module"),
                Api("uddi:cf6fe65c-3693-4b38-85c6-e99a2efc67d0_202110291941", "해양수산부", "수협 산지조합 창고별 매출처 정보", "mof-fisheries-distribution-module"),
                Api("uddi:1ccdd8cd-a33c-4a2e-885f-be92bea3d022_202110291927", "해양수산부", "위판장별 위탁판매 현황", "mof-fisheries-distribution-module"),
                Api("uddi:62051143-7dbd-4fb2-82c7-90a059f2745a_202110291930", "해양수산부", "일자별 위탁판매 현황", "mof-fisheries-distribution-module")
            ]),
        new(
            "molit-logistics-warehouse-reference",
            "국토교통부 물류창고 기준정보",
            "등록 현황은 창고 후보 기준정보로만 사용하고 실제 보관 계약, 가용 용량과 운영 허가를 별도로 확인합니다.",
            [
                Api("uddi:0853b006-1eed-4288-9976-d64fd8067e9e", "국토교통부", "물류창고업등록정보")
            ]),
        new(
            "molit-apartment-reference",
            "국토교통부 공동주택 기준정보",
            "단지 단위 공개 정보만 사용하며 세대·거주자 식별, 자동 가입, 계약 또는 관리비 반영 근거로 사용하지 않습니다.",
            [
                Api("uddi:476ec6a4-ab8d-49dd-bc73-f9e12f3f37db_202503111638", "국토교통부", "공동주택 단지 목록제공 서비스", "kapt-apartment-complex-list"),
                Api("uddi:80c40307-716f-44bb-8d99-8bef35ebf870_202503211331", "국토교통부", "공동주택 유지관리 이력 정보제공 서비스", "kapt-apartment-operations-module"),
                Api("uddi:df14c732-f741-44ca-b568-e529949bb22f_202503211601", "국토교통부", "공동주택 입찰결과공지 정보제공 서비스", "kapt-apartment-operations-module"),
                Api("uddi:f8e87a24-f07a-4168-9693-a4cbf39332a1_202503181721", "국토교통부", "공동주택관리비(개별사용료)정보제공서비스", "kapt-apartment-management-fees"),
                Api("uddi:689f4b6b-e8d9-4f2b-b0e3-8efc837b801d_202503271643", "국토교통부", "공동주택 수의계약 공지 정보제공 서비스", "kapt-apartment-operations-module"),
                Api("uddi:789d7a2b-0f8e-46ca-bf97-ae7474ad594b_202503271649", "국토교통부", "공동주택관리비(공용관리비)정보제공서비스", "kapt-apartment-management-fees"),
                Api("uddi:8a4a8529-2f36-4774-9df0-fba3798e142c_202503311144", "국토교통부", "공동주택 입찰공고 정보제공 서비스", "kapt-apartment-operations-module"),
                Api("uddi:bc7b2aea-e6b9-4fc2-860c-f7ca83ad72b2_202504021733", "국토교통부", "공동주택 에너지 사용 정보", "kapt-apartment-operations-module"),
                Api("uddi:de13ff51-9432-4a10-86e9-a4bf813ac638_202504021714", "국토교통부", "공동주택관리비(장기수선충당금)정보서비스", "kapt-apartment-management-fees"),
                Api("uddi:a12c6e76-8982-4e3e-afe2-94412c7d0a79_202506271534", "국토교통부", "공동주택 기본 정보제공 서비스", "kapt-apartment-complex-basic")
            ]),
        new(
            "mois-administrative-code-reference",
            "행정안전부 행정표준코드",
            "기관코드와 법정동코드를 서로 다른 기준정보로 보존하고 사람·주소의 공개 식별자로 사용하지 않습니다.",
            [
                Api("uddi:5eb1ab7b-29f0-4a69-8afb-b70e917f00d7_202202221456", "행정안전부", "행정표준코드_기관코드"),
                Api("uddi:01d1b3d2-87c5-4666-a727-9e00fcb66c8a_202104021130", "행정안전부", "행정표준코드_법정동코드")
            ]),
        new(
            "customs-country-trade-statistics",
            "관세청 품목·국가별 수출입 통계",
            "월별 수출입 실적은 관측 통계로 사용하며 실제 계약가격, 관세·검역·국내 물류비를 포함한 가격으로 표현하지 않습니다.",
            [
                Api("uddi:de645b0e-dcde-4ff2-9f14-9ce47fa641b2_202205261542", "관세청", "품목별 국가별 수출입실적(GW)", "customs-hs-country-import-statistics")
            ]),
        new(
            "tourapi-regional-culture",
            "한국관광공사 국문 관광정보",
            "관광정보와 이미지의 원천·수정일·공공누리 유형을 보존하고 지역문화 대표성을 자동 확정하지 않습니다.",
            [
                Api("uddi:5f6e3d4a-5b6e-4e40-8a4a-e95be38ed35a_202504161413", "한국관광공사", "국문 관광정보 서비스_GW", "tourapi-korean-tourism")
            ]),
        new(
            "online-price-kosis-comparison",
            "온라인가격·KOSIS 비교자료",
            "온라인 상품가격과 집계 통계의 품목·단위·시점을 따로 보존하고 정렬 전 가격 순위나 절감액을 산출하지 않습니다.",
            [
                Api("uddi:5874b0b0-afed-4467-a89e-c5a91f0019a2_202105101042", "국가데이터처", "온라인 수집 가격 정보", "online-collected-prices"),
                Api("uddi:318354f6-2c89-4c8b-a186-d2599694c2f6_202404301439", "국가데이터처", "KOSIS 지표정보 조회 서비스", "kosis-indicator-info"),
                Api("uddi:f7fd3b31-0be3-4752-9935-d4ea013a2dc7_202404301356", "국가데이터처", "KOSIS 통계자료 조회 서비스", "kosis-statistics-data")
            ]),
        new(
            "jeju-international-school-reference",
            "제주 국제학교 기준정보",
            "학교 현황을 지역 교육 기준정보로만 사용하며 학생·가족 형태나 경제력을 커뮤니티 자격 또는 추천 기준으로 사용하지 않습니다.",
            [
                Api("uddi:ab574f63-252f-44c3-b588-c4bc6782e173", "제주특별자치도", "국제학교현황")
            ]),
        new(
            "nts-business-registration-verification",
            "국세청 사업자등록 검증",
            "권한이 있는 업무 검증에서만 조회하고 사업자등록번호와 조회 결과를 공개 지도나 공개 원장에 노출하지 않습니다.",
            [
                Api("uddi:cd05d81f-c717-48cf-a7d1-d7fb41ca8f0a", "국세청", "사업자등록정보 진위확인 및 상태조회 서비스", "nts-business-registration-status")
            ])
    ];

    public 공공데이터포털활용ApiModuleCatalog(IPublicDataApiMetadataCatalog publicDataApiMetadataCatalog)
    {
        _publicDataApiMetadataCatalog = publicDataApiMetadataCatalog;
    }

    public 공공데이터포털활용ApiModuleResponse GetCatalog(공공데이터포털활용ApiModuleQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var metadataByKey = _publicDataApiMetadataCatalog
            .GetCatalog(new PublicDataApiMetadataQuery())
            .Items
            .ToDictionary(item => item.Key, StringComparer.Ordinal);

        IEnumerable<공공데이터포털활용ApiModuleItem> items = Modules.Select(module =>
        {
            var apis = module.Apis
                .Select(api => ToItem(api, metadataByKey))
                .ToArray();
            var mappedCount = module.Apis.Count(api => !string.IsNullOrWhiteSpace(api.MetadataKey));

            return new 공공데이터포털활용ApiModuleItem
            {
                Key = module.Key,
                DisplayName = module.DisplayName,
                ProductBoundary = module.ProductBoundary,
                CoverageCode = mappedCount == module.Apis.Count
                    ? 공공데이터포털활용ApiModuleCoverageCodes.Full
                    : mappedCount > 0
                        ? 공공데이터포털활용ApiModuleCoverageCodes.Partial
                        : 공공데이터포털활용ApiModuleCoverageCodes.CatalogOnly,
                Apis = apis
            };
        });

        if (!string.IsNullOrWhiteSpace(query.ModuleKey))
        {
            items = items.Where(item => string.Equals(
                item.Key,
                query.ModuleKey,
                StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.ImplementationStatusCode))
        {
            items = items
                .Select(item => item with
                {
                    Apis = item.Apis
                        .Where(api => string.Equals(
                            api.ImplementationStatusCode,
                            query.ImplementationStatusCode,
                            StringComparison.OrdinalIgnoreCase))
                        .ToArray()
                })
                .Where(item => item.Apis.Count > 0);
        }

        return new 공공데이터포털활용ApiModuleResponse
        {
            VerifiedOn = VerifiedOn,
            Items = items.ToArray()
        };
    }

    private static 공공데이터포털활용ApiItem ToItem(
        ApiDefinition api,
        IReadOnlyDictionary<string, PublicDataApiMetadataItem> metadataByKey)
    {
        if (string.IsNullOrWhiteSpace(api.MetadataKey)
            || !metadataByKey.TryGetValue(api.MetadataKey, out var metadata))
        {
            return new 공공데이터포털활용ApiItem
            {
                DataId = api.DataId,
                Provider = api.Provider,
                DisplayName = api.DisplayName
            };
        }

        return new 공공데이터포털활용ApiItem
        {
            DataId = api.DataId,
            Provider = api.Provider,
            DisplayName = api.DisplayName,
            MetadataKey = metadata.Key,
            ImplementationStatusCode = metadata.ImplementationStatusCode,
            ClientType = metadata.ClientType,
            IsServiceKeyConfigured = metadata.IsServiceKeyConfigured
        };
    }

    private static ApiDefinition Api(
        string dataId,
        string provider,
        string displayName,
        string metadataKey = "") => new(dataId, provider, displayName, metadataKey);

    private sealed record ModuleDefinition(
        string Key,
        string DisplayName,
        string ProductBoundary,
        IReadOnlyList<ApiDefinition> Apis);

    private sealed record ApiDefinition(
        string DataId,
        string Provider,
        string DisplayName,
        string MetadataKey);
}
