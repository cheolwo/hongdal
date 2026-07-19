using System.Text.Json;
using Ssalddel.Contracts.Common.Sales;

namespace Ssalddel.Tests.Contracts.Common.Sales;

public sealed class CoupangCategoryApiContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Catalog_DeclaresCoupangCategoryApiSurfacesWithoutCredentials()
    {
        var endpoints = CoupangCategoryApiContractCatalog.All;

        Assert.Contains(endpoints, endpoint =>
            endpoint.Key == CoupangCategoryApiEndpointKeys.GetDisplayCategories &&
            endpoint.Method == "GET" &&
            endpoint.PathTemplate == CoupangCategoryApiRoutes.GetDisplayCategories &&
            !endpoint.HasRequestBody &&
            endpoint.ResponseContract == nameof(CoupangDisplayCategoryTreeResponse));
        Assert.Contains(endpoints, endpoint =>
            endpoint.Key == CoupangCategoryApiEndpointKeys.GetCategoryRelatedMetas &&
            endpoint.Method == "GET" &&
            endpoint.PathTemplate.Contains("{displayCategoryCode}", StringComparison.Ordinal) &&
            endpoint.ResponseContract == nameof(CoupangCategoryMetaResponse));
        Assert.Contains(endpoints, endpoint =>
            endpoint.Key == CoupangCategoryApiEndpointKeys.PredictCategory &&
            endpoint.Method == "POST" &&
            endpoint.HasRequestBody &&
            endpoint.RequestContract == nameof(CoupangCategoryPredictionRequest) &&
            endpoint.ResponseContract == nameof(CoupangCategoryPredictionResponse));
        Assert.All(endpoints, endpoint =>
        {
            Assert.StartsWith("/v2/providers/", endpoint.PathTemplate, StringComparison.Ordinal);
            Assert.StartsWith("https://developers.coupangcorp.com/", endpoint.OfficialDocumentationUrl, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(endpoint.Purpose));
        });
    }

    [Fact]
    public void Routes_BuildDisplayCategoryCodePaths()
    {
        Assert.Equal(
            "/v2/providers/seller_api/apis/api/v1/marketplace/meta/display-categories/0",
            CoupangCategoryApiRoutes.BuildDisplayCategoryChildrenPath(0));
        Assert.Equal(
            "/v2/providers/seller_api/apis/api/v1/marketplace/meta/category-related-metas/display-category-codes/78877",
            CoupangCategoryApiRoutes.BuildCategoryRelatedMetasPath(78877));
    }

    [Fact]
    public void DisplayCategoryTreeResponse_ParsesCoupangCategoryListShape()
    {
        const string json = """
            {
              "code": "SUCCESS",
              "message": "",
              "data": {
                "displayItemCategoryCode": 0,
                "name": "ROOT",
                "status": "ACTIVE",
                "child": [
                  {
                    "displayItemCategoryCode": 63897,
                    "name": "생활용품",
                    "status": "ACTIVE",
                    "child": []
                  }
                ]
              }
            }
            """;

        var response = JsonSerializer.Deserialize<CoupangDisplayCategoryTreeResponse>(json, JsonOptions);

        Assert.NotNull(response);
        Assert.Equal("SUCCESS", response!.Code);
        Assert.Equal(0, response.Data.EffectiveDisplayCategoryCode);
        Assert.Equal("ROOT", response.Data.Name);
        var child = Assert.Single(response.Data.Child);
        Assert.Equal(63897, child.EffectiveDisplayCategoryCode);
        Assert.Equal(CoupangCategoryStatusCodes.Active, child.Status);
    }

    [Fact]
    public void CategoryMetaResponse_ParsesOptionsNoticeDocumentsCertificationsAndOfferConditions()
    {
        const string json = """
            {
              "code": "SUCCESS",
              "message": "",
              "data": {
                "isAllowSingleItem": true,
                "attributes": [
                  {
                    "attributeTypeName": "수량",
                    "dataType": "NUMBER",
                    "basicUnit": "개",
                    "inputType": "INPUT",
                    "inputValues": [],
                    "usableUnits": ["개", "박스"],
                    "required": "OPTIONAL",
                    "groupNumber": "NONE",
                    "exposed": "EXPOSED"
                  }
                ],
                "noticeCategories": [
                  {
                    "noticeCategoryName": "기타 재화",
                    "noticeCategoryDetailNames": [
                      {
                        "noticeCategoryDetailName": "품명 및 모델명",
                        "required": "MANDATORY"
                      }
                    ]
                  }
                ],
                "requiredDocumentNames": [
                  {
                    "templateName": "기타인증서류",
                    "required": "OPTIONAL"
                  }
                ],
                "certifications": [
                  {
                    "certificationType": "NOT_REQUIRED",
                    "name": "인증대상아님",
                    "dataType": "NONE",
                    "required": "OPTIONAL"
                  }
                ],
                "allowedOfferConditions": ["NEW", "USED_GOOD"]
              }
            }
            """;

        var response = JsonSerializer.Deserialize<CoupangCategoryMetaResponse>(json, JsonOptions);

        Assert.NotNull(response);
        Assert.True(response!.Data.IsAllowSingleItem);
        var attribute = Assert.Single(response.Data.Attributes);
        Assert.Equal("수량", attribute.AttributeTypeName);
        Assert.Equal(CoupangCategoryAttributeDataTypes.Number, attribute.DataType);
        Assert.Equal(CoupangCategoryAttributeInputTypes.Input, attribute.InputType);
        Assert.Contains("박스", attribute.UsableUnits);
        Assert.Equal(CoupangCategoryAttributeExposureCodes.Exposed, attribute.Exposed);
        Assert.Equal(CoupangCategoryRequiredCodes.Mandatory, Assert.Single(Assert.Single(response.Data.NoticeCategories).NoticeCategoryDetailNames).Required);
        Assert.Equal("기타인증서류", Assert.Single(response.Data.RequiredDocumentNames).TemplateName);
        Assert.Equal("NOT_REQUIRED", Assert.Single(response.Data.Certifications).CertificationType);
        Assert.Contains(CoupangOfferConditionCodes.New, response.Data.AllowedOfferConditions);
    }

    [Fact]
    public void CategoryPredictionContracts_SerializeRequestAndParseResponse()
    {
        var request = new CoupangCategoryPredictionRequest
        {
            ProductName = "라운드티셔츠 남성 긴팔 맨투맨",
            ProductDescription = "긴소매 남성 맨투맨 티셔츠",
            Brand = "Ssalddel Sample",
            SellerSkuCode = "HD-SKU-001",
            Attributes = new Dictionary<string, string>
            {
                ["색상"] = "네이비",
                ["제조국"] = "한국"
            }
        };

        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        Assert.Contains("\"productName\"", requestJson, StringComparison.Ordinal);
        Assert.Contains("\"sellerSkuCode\"", requestJson, StringComparison.Ordinal);
        Assert.Contains("HD-SKU-001", requestJson, StringComparison.Ordinal);

        const string responseJson = """
            {
              "code": 200,
              "message": "OK",
              "data": {
                "autoCategorizationPredictionResultType": "SUCCESS",
                "predictedCategoryId": "63950",
                "predictedCategoryName": "일반 섬유유연제",
                "comment": null
              }
            }
            """;

        var response = JsonSerializer.Deserialize<CoupangCategoryPredictionResponse>(responseJson, JsonOptions);

        Assert.NotNull(response);
        Assert.Equal(200, response!.Code);
        Assert.Equal(CoupangCategoryPredictionResultTypes.Success, response.Data.AutoCategorizationPredictionResultType);
        Assert.Equal("63950", response.Data.PredictedCategoryId);
        Assert.Null(response.Data.Comment);
    }
}
