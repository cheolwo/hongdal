using System.Text.Json.Serialization;

namespace Ssalddel.Contracts.Common.Sales;

public static class CoupangOpenApiConstants
{
    public const string BaseUrl = "https://api-gateway.coupang.com";
    public const string SellerApiProvider = "seller_api";
    public const string OpenApiProvider = "openapi";
}

public static class CoupangCategoryApiEndpointKeys
{
    public const string GetDisplayCategories = "Coupang.GetDisplayCategories";
    public const string GetDisplayCategoryChildren = "Coupang.GetDisplayCategoryChildren";
    public const string GetCategoryRelatedMetas = "Coupang.GetCategoryRelatedMetas";
    public const string PredictCategory = "Coupang.PredictCategory";
}

public static class CoupangCategoryApiRoutes
{
    public const string GetDisplayCategories = "/v2/providers/seller_api/apis/api/v1/marketplace/meta/display-categories";
    public const string GetDisplayCategoryChildren = "/v2/providers/seller_api/apis/api/v1/marketplace/meta/display-categories/{displayCategoryCode}";
    public const string GetCategoryRelatedMetas = "/v2/providers/seller_api/apis/api/v1/marketplace/meta/category-related-metas/display-category-codes/{displayCategoryCode}";
    public const string PredictCategory = "/v2/providers/openapi/apis/api/v1/categorization/predict";

    public static string BuildDisplayCategoryChildrenPath(long displayCategoryCode)
        => GetDisplayCategoryChildren.Replace("{displayCategoryCode}", displayCategoryCode.ToString(), StringComparison.Ordinal);

    public static string BuildCategoryRelatedMetasPath(long displayCategoryCode)
        => GetCategoryRelatedMetas.Replace("{displayCategoryCode}", displayCategoryCode.ToString(), StringComparison.Ordinal);
}

public sealed record CoupangCategoryApiEndpointDescriptor(
    string Key,
    string Method,
    string PathTemplate,
    bool HasRequestBody,
    string RequestContract,
    string ResponseContract,
    string Purpose,
    string OfficialDocumentationUrl);

public static class CoupangCategoryApiContractCatalog
{
    public static readonly IReadOnlyList<CoupangCategoryApiEndpointDescriptor> All =
    [
        new(
            CoupangCategoryApiEndpointKeys.GetDisplayCategories,
            "GET",
            CoupangCategoryApiRoutes.GetDisplayCategories,
            HasRequestBody: false,
            RequestContract: "none",
            ResponseContract: nameof(CoupangDisplayCategoryTreeResponse),
            Purpose: "쿠팡 전체 노출 카테고리 트리를 조회해 살뜰 판매채널 카테고리 캐시의 원천으로 사용합니다.",
            OfficialDocumentationUrl: "https://developers.coupangcorp.com/hc/en-us/articles/360033400814-How-to-get-category-list"),
        new(
            CoupangCategoryApiEndpointKeys.GetDisplayCategoryChildren,
            "GET",
            CoupangCategoryApiRoutes.GetDisplayCategoryChildren,
            HasRequestBody: false,
            RequestContract: "displayCategoryCode path segment",
            ResponseContract: nameof(CoupangDisplayCategoryTreeResponse),
            Purpose: "특정 노출 카테고리 코드의 바로 아래 하위 카테고리를 조회합니다. 1depth 조회는 0을 사용합니다.",
            OfficialDocumentationUrl: "https://developers.coupangcorp.com/hc/en-us/articles/360034035753-How-to-get-categories"),
        new(
            CoupangCategoryApiEndpointKeys.GetCategoryRelatedMetas,
            "GET",
            CoupangCategoryApiRoutes.GetCategoryRelatedMetas,
            HasRequestBody: false,
            RequestContract: "displayCategoryCode path segment",
            ResponseContract: nameof(CoupangCategoryMetaResponse),
            Purpose: "상품 등록 전에 카테고리별 옵션, 상품고시, 구비서류, 인증정보, 허용 상품 상태를 확인합니다.",
            OfficialDocumentationUrl: "https://developers.coupangcorp.com/hc/ko/articles/360034035713-%EC%B9%B4%ED%85%8C%EA%B3%A0%EB%A6%AC-%EB%A9%94%ED%83%80%EC%A0%95%EB%B3%B4-%EC%A1%B0%ED%9A%8C"),
        new(
            CoupangCategoryApiEndpointKeys.PredictCategory,
            "POST",
            CoupangCategoryApiRoutes.PredictCategory,
            HasRequestBody: true,
            RequestContract: nameof(CoupangCategoryPredictionRequest),
            ResponseContract: nameof(CoupangCategoryPredictionResponse),
            Purpose: "상품명, 상세설명, 브랜드, 속성, 판매자 SKU로 적합한 쿠팡 노출 카테고리 코드를 추천받습니다.",
            OfficialDocumentationUrl: "https://developers.coupangcorp.com/hc/ko/articles/360033509234-%EC%B9%B4%ED%85%8C%EA%B3%A0%EB%A6%AC-%EC%B6%94%EC%B2%9C")
    ];

    public static CoupangCategoryApiEndpointDescriptor Find(string key)
        => All.First(item => string.Equals(item.Key, key, StringComparison.Ordinal));
}

public sealed class CoupangDisplayCategoryTreeResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public CoupangDisplayCategoryNodeDto Data { get; set; } = new();
}

public sealed class CoupangDisplayCategoryNodeDto
{
    [JsonPropertyName("displayCategoryCode")]
    public long? DisplayCategoryCode { get; set; }

    [JsonPropertyName("displayItemCategoryCode")]
    public long? DisplayItemCategoryCode { get; set; }

    [JsonIgnore]
    public long EffectiveDisplayCategoryCode => DisplayCategoryCode ?? DisplayItemCategoryCode ?? 0;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("child")]
    public IReadOnlyList<CoupangDisplayCategoryNodeDto> Child { get; set; } = [];
}

public sealed class CoupangCategoryMetaResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public CoupangCategoryMetaDataDto Data { get; set; } = new();
}

public sealed class CoupangCategoryMetaDataDto
{
    [JsonPropertyName("isAllowSingleItem")]
    public bool IsAllowSingleItem { get; set; }

    [JsonPropertyName("attributes")]
    public IReadOnlyList<CoupangCategoryAttributeMetaDto> Attributes { get; set; } = [];

    [JsonPropertyName("noticeCategories")]
    public IReadOnlyList<CoupangNoticeCategoryMetaDto> NoticeCategories { get; set; } = [];

    [JsonPropertyName("requiredDocumentNames")]
    public IReadOnlyList<CoupangRequiredDocumentMetaDto> RequiredDocumentNames { get; set; } = [];

    [JsonPropertyName("certifications")]
    public IReadOnlyList<CoupangCertificationMetaDto> Certifications { get; set; } = [];

    [JsonPropertyName("allowedOfferConditions")]
    public IReadOnlyList<string> AllowedOfferConditions { get; set; } = [];
}

public sealed class CoupangCategoryAttributeMetaDto
{
    [JsonPropertyName("attributeTypeName")]
    public string AttributeTypeName { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public string Required { get; set; } = string.Empty;

    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = string.Empty;

    [JsonPropertyName("basicUnit")]
    public string BasicUnit { get; set; } = string.Empty;

    [JsonPropertyName("inputType")]
    public string InputType { get; set; } = string.Empty;

    [JsonPropertyName("inputValues")]
    public IReadOnlyList<string> InputValues { get; set; } = [];

    [JsonPropertyName("usableUnits")]
    public IReadOnlyList<string> UsableUnits { get; set; } = [];

    [JsonPropertyName("groupNumber")]
    public string GroupNumber { get; set; } = string.Empty;

    [JsonPropertyName("exposed")]
    public string Exposed { get; set; } = string.Empty;
}

public sealed class CoupangNoticeCategoryMetaDto
{
    [JsonPropertyName("noticeCategoryName")]
    public string NoticeCategoryName { get; set; } = string.Empty;

    [JsonPropertyName("noticeCategoryDetailNames")]
    public IReadOnlyList<CoupangNoticeCategoryDetailMetaDto> NoticeCategoryDetailNames { get; set; } = [];
}

public sealed class CoupangNoticeCategoryDetailMetaDto
{
    [JsonPropertyName("noticeCategoryDetailName")]
    public string NoticeCategoryDetailName { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public string Required { get; set; } = string.Empty;
}

public sealed class CoupangRequiredDocumentMetaDto
{
    [JsonPropertyName("templateName")]
    public string TemplateName { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public string Required { get; set; } = string.Empty;
}

public sealed class CoupangCertificationMetaDto
{
    [JsonPropertyName("certificationType")]
    public string CertificationType { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public string Required { get; set; } = string.Empty;
}

public sealed class CoupangCategoryPredictionRequest
{
    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("productDescription")]
    public string ProductDescription { get; set; } = string.Empty;

    [JsonPropertyName("brand")]
    public string Brand { get; set; } = string.Empty;

    [JsonPropertyName("attributes")]
    public IReadOnlyDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

    [JsonPropertyName("sellerSkuCode")]
    public string SellerSkuCode { get; set; } = string.Empty;
}

public sealed class CoupangCategoryPredictionResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public CoupangCategoryPredictionDataDto Data { get; set; } = new();
}

public sealed class CoupangCategoryPredictionDataDto
{
    [JsonPropertyName("autoCategorizationPredictionResultType")]
    public string AutoCategorizationPredictionResultType { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("predictedCategoryId")]
    public string PredictedCategoryId { get; set; } = string.Empty;

    [JsonPropertyName("predictedCategoryName")]
    public string PredictedCategoryName { get; set; } = string.Empty;
}

public static class CoupangCategoryStatusCodes
{
    public const string Active = "ACTIVE";
    public const string Ready = "READY";
    public const string Disabled = "DISABLED";
}

public static class CoupangCategoryRequiredCodes
{
    public const string Mandatory = "MANDATORY";
    public const string Optional = "OPTIONAL";
    public const string Recommend = "RECOMMEND";
    public const string MandatoryParallelImported = "MANDATORY_PARALLEL_IMPORTED";
    public const string MandatoryOverseasPurchased = "MANDATORY_OVERSEAS_PURCHASED";
}

public static class CoupangCategoryAttributeDataTypes
{
    public const string String = "STRING";
    public const string Number = "NUMBER";
    public const string Date = "DATE";
}

public static class CoupangCategoryAttributeInputTypes
{
    public const string Input = "INPUT";
    public const string Select = "SELECT";
}

public static class CoupangCategoryAttributeExposureCodes
{
    public const string Exposed = "EXPOSED";
    public const string None = "NONE";
}

public static class CoupangOfferConditionCodes
{
    public const string New = "NEW";
    public const string Refurbished = "REFURBISHED";
    public const string UsedBest = "USED_BEST";
    public const string UsedGood = "USED_GOOD";
    public const string UsedNormal = "USED_NORMAL";
}

public static class CoupangCategoryPredictionResultTypes
{
    public const string Success = "SUCCESS";
    public const string Failure = "FAILURE";
    public const string InsufficientInformation = "INSUFFICIENT_INFORMATION";
}
