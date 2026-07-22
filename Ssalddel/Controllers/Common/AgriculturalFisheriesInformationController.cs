using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.AgriculturalFisheries.Information;
using Ssalddel.Services.FoodCulture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[ApiController]
[AllowAnonymous]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[Route("api/v1/agricultural-fisheries")]
public sealed class AgriculturalFisheriesInformationController : ControllerBase
{
    private readonly IAgriculturalFisheriesInformationService _informationService;
    private readonly I미국농수산가격조회Service _usPriceService;
    private readonly I호주농수산식품가격조회Service _australiaFoodPriceService;
    private readonly I미국농어업경영체정보원천Service _usOperatorSourceService;
    private readonly IOfficialFoodRecipeIngredientIndexService _ingredientIndexService;
    private readonly IOfficialFoodRecipeArchiveService _recipeArchiveService;
    private readonly IOfficialFoodIngredientCompanyArchiveService _companyArchiveService;

    public AgriculturalFisheriesInformationController(
        IAgriculturalFisheriesInformationService informationService,
        I미국농수산가격조회Service usPriceService,
        I호주농수산식품가격조회Service australiaFoodPriceService,
        I미국농어업경영체정보원천Service usOperatorSourceService,
        IOfficialFoodRecipeIngredientIndexService ingredientIndexService,
        IOfficialFoodRecipeArchiveService recipeArchiveService,
        IOfficialFoodIngredientCompanyArchiveService companyArchiveService)
    {
        _informationService = informationService;
        _usPriceService = usPriceService;
        _australiaFoodPriceService = australiaFoodPriceService;
        _usOperatorSourceService = usOperatorSourceService;
        _ingredientIndexService = ingredientIndexService;
        _recipeArchiveService = recipeArchiveService;
        _companyArchiveService = companyArchiveService;
    }

    [HttpGet]
    public ActionResult<AgriculturalFisheriesInformationOverviewResponse> GetOverview()
        => Ok(_informationService.GetOverview());

    [HttpGet("items")]
    public ActionResult<AgriculturalFisheriesItemSearchResponse> SearchItems(
        [FromQuery] string? query,
        [FromQuery] string? categoryCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
        => Ok(_informationService.SearchItems(query, categoryCode, page, pageSize));

    [HttpGet("food-ingredients")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodIngredientDto>>> SearchFoodIngredients(
        [FromQuery] OfficialFoodIngredientQuery query,
        CancellationToken cancellationToken = default)
    {
        var publicQuery = new OfficialFoodIngredientQuery
        {
            CategoryCode = query.CategoryCode,
            LanguageCode = query.LanguageCode,
            ClassificationState = query.ClassificationState,
            SearchText = query.SearchText,
            Take = Math.Clamp(query.Take, 1, 50)
        };
        return Ok(await _ingredientIndexService.SearchIngredientsAsync(
            publicQuery,
            cancellationToken));
    }

    [HttpGet("food-ingredients/hs-codes")]
    public async Task<ActionResult<OfficialFoodIngredientHsMappingResponse>>
        GetFoodIngredientHsCodes(
            [FromServices] IOfficialFoodIngredientHsMappingService mappingService,
            [FromQuery] OfficialFoodIngredientHsQuery query,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.IngredientKey)
            && string.IsNullOrWhiteSpace(query.IngredientName))
        {
            return BadRequest();
        }

        try
        {
            return Ok(await mappingService.GetOrCreateAsync(query, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }

    [HttpGet("food-ingredients/companies")]
    public async Task<ActionResult<OfficialFoodIngredientCompanyResearchResponse>>
        SearchFoodIngredientCompanies(
            [FromQuery] OfficialFoodIngredientCompanyQuery query,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.IngredientName)
            || (query.IngredientName.Trim().Length < 2
                && string.IsNullOrWhiteSpace(query.IngredientKey)))
        {
            return BadRequest();
        }

        try
        {
            return Ok(await _companyArchiveService.ResearchAndArchiveAsync(
                new OfficialFoodIngredientCompanyQuery
                {
                    IngredientKey = query.IngredientKey,
                    IngredientName = query.IngredientName,
                    Take = Math.Clamp(query.Take, 1, 20)
                },
                cancellationToken: cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("food-ingredients/companies/archive")]
    public async Task<ActionResult<OfficialFoodIngredientCompanyArchiveResponse>>
        GetFoodIngredientCompanyArchive(
            [FromQuery] string? ingredientKey,
            [FromQuery] string? ingredientName,
            [FromQuery] bool includeInactive = false,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ingredientKey)
            && string.IsNullOrWhiteSpace(ingredientName))
        {
            return BadRequest();
        }

        var archive = await _companyArchiveService.GetArchiveAsync(
            ingredientKey,
            ingredientName,
            includeInactive,
            cancellationToken);
        return archive is null ? NotFound() : Ok(archive);
    }

    [HttpGet("food-ingredients/companies/coverage")]
    public async Task<ActionResult<OfficialFoodIngredientCompanyCoverageResponse>>
        GetFoodIngredientCompanyCoverage(
            [FromQuery] int staleAfterDays = 30,
            CancellationToken cancellationToken = default)
        => Ok(await _companyArchiveService.GetCoverageAsync(
            Math.Clamp(staleAfterDays, 1, 3650),
            cancellationToken));

    [HttpGet("food-dishes")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodRecipeDishDto>>> SearchFoodDishes(
        [FromQuery] OfficialFoodDishDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        var dishes = await _recipeArchiveService.SearchDishesAsync(
            new OfficialFoodRecipeQuery
            {
                CountryCode = string.IsNullOrWhiteSpace(query.CountryCode)
                    ? null
                    : query.CountryCode.Trim().ToUpperInvariant(),
                SearchText = query.SearchText,
                Take = Math.Clamp(query.Take, 1, 50),
                OnlyWithBrowsableIngredients = true
            },
            cancellationToken);
        return Ok(dishes
            .Where(dish => dish.ReviewState != OfficialFoodRecipeReviewStates.Excluded
                           && dish.RepresentationState != OfficialFoodRecipeRepresentationStates.Excluded)
            .ToArray());
    }

    [HttpGet("food-dishes/{dishKey}")]
    public async Task<ActionResult<OfficialFoodDishDetailDto>> GetFoodDish(
        string dishKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dish = await _recipeArchiveService.GetDishAsync(dishKey, cancellationToken);
            if (dish is null
                || dish.ReviewState == OfficialFoodRecipeReviewStates.Excluded
                || dish.RepresentationState == OfficialFoodRecipeRepresentationStates.Excluded)
            {
                return NotFound();
            }

            var variants = await _recipeArchiveService.GetVariantsAsync(dish.DishKey, cancellationToken);
            var representative = variants.FirstOrDefault(variant =>
                variant.IsFreshForPublication && variant.StructuredIngredients?.Count > 0);
            if (representative is null)
            {
                return NotFound();
            }

            return Ok(new OfficialFoodDishDetailDto(
                dish,
                representative.RecordKey,
                representative.SourceKey,
                representative.Provider,
                representative.Title,
                representative.ServingText,
                representative.OriginalUrl,
                representative.AttributionText,
                representative.CollectedAtUtc,
                representative.IsFreshForPublication,
                representative.StructuredIngredients ?? []));
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }

    [HttpGet("items/{hsCode}/domestic-price")]
    public async Task<ActionResult<AgriculturalFisheriesDomesticPriceResponse>> GetDomesticPrice(
        string hsCode,
        [FromQuery] string? referenceDate = null,
        [FromQuery] int lookbackDays = 14,
        CancellationToken cancellationToken = default)
    {
        var response = await _informationService.GetDomesticPriceAsync(
            new AgriculturalFisheriesDomesticPriceRequest
            {
                HsCode = hsCode,
                ReferenceDate = referenceDate ?? string.Empty,
                LookbackDays = lookbackDays
            },
            cancellationToken);

        return response.StatusCode switch
        {
            "InvalidRequest" => BadRequest(response),
            "MappingRequired" => NotFound(response),
            "DataUnavailable" => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Ok(response)
        };
    }

    [HttpGet("us-prices")]
    public async Task<ActionResult<미국농수산가격조회응답>> GetUsPrices(
        [FromQuery] string commodity,
        [FromQuery] string sourceKey = 미국농수산가격출처Keys.UsdaNassQuickStats,
        [FromQuery] string statisticCategory = "PRICE RECEIVED",
        [FromQuery] string program = "SURVEY",
        [FromQuery] string? sector = null,
        [FromQuery] string? group = null,
        [FromQuery] string aggregationLevel = "NATIONAL",
        [FromQuery] string? stateAlpha = null,
        [FromQuery] string domain = "TOTAL",
        [FromQuery] string? frequency = null,
        [FromQuery] int yearFrom = 0,
        [FromQuery] int? yearTo = null,
        [FromQuery] int maxItems = 100,
        CancellationToken cancellationToken = default)
    {
        var response = await _usPriceService.조회Async(
            new 미국농수산가격조회요청
            {
                SourceKey = sourceKey,
                Commodity = commodity,
                StatisticCategory = statisticCategory,
                Program = program,
                Sector = sector,
                Group = group,
                AggregationLevel = aggregationLevel,
                StateAlpha = stateAlpha,
                Domain = domain,
                Frequency = frequency,
                YearFrom = yearFrom,
                YearTo = yearTo,
                MaxItems = maxItems
            },
            cancellationToken);

        return response.StatusCode switch
        {
            미국농수산가격조회상태Codes.잘못된요청 => BadRequest(response),
            미국농수산가격조회상태Codes.지원하지않는출처 => BadRequest(response),
            미국농수산가격조회상태Codes.설정안됨 =>
                StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            미국농수산가격조회상태Codes.자료조회불가 =>
                StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Ok(response)
        };
    }

    [HttpGet("au-food-price-indexes/catalog")]
    public ActionResult<호주농수산식품가격Catalog응답> GetAustraliaFoodPriceCatalog()
        => Ok(_australiaFoodPriceService.GetCatalog());

    [HttpGet("au-food-price-indexes")]
    public async Task<ActionResult<호주농수산식품가격조회응답>> GetAustraliaFoodPriceIndexes(
        [FromQuery] string sourceKey = 호주농수산식품가격출처Keys.AbsConsumerPriceIndex,
        [FromQuery] string indexCode = 호주식품가격지수Codes.FoodAndNonAlcoholicBeverages,
        [FromQuery] string measureCode = 호주식품가격지수측정Codes.IndexNumber,
        [FromQuery] string regionCode = 호주식품가격지수지역Codes.Australia,
        [FromQuery] string? startPeriod = null,
        [FromQuery] string? endPeriod = null,
        [FromQuery] int maxItems = 60,
        CancellationToken cancellationToken = default)
    {
        var response = await _australiaFoodPriceService.조회Async(
            new 호주농수산식품가격조회요청
            {
                SourceKey = sourceKey,
                IndexCode = indexCode,
                MeasureCode = measureCode,
                RegionCode = regionCode,
                StartPeriod = startPeriod ?? string.Empty,
                EndPeriod = endPeriod ?? string.Empty,
                MaxItems = maxItems
            },
            cancellationToken);

        return response.StatusCode switch
        {
            호주농수산식품가격조회상태Codes.잘못된요청 => BadRequest(response),
            호주농수산식품가격조회상태Codes.지원하지않는출처 => BadRequest(response),
            호주농수산식품가격조회상태Codes.자료조회불가 =>
                StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Ok(response)
        };
    }

    [HttpGet("us-operator-information-sources")]
    public ActionResult<미국농어업경영체정보원천조회응답> GetUsOperatorInformationSources(
        [FromQuery] string? q = null,
        [FromQuery] string? sectorCode = null,
        [FromQuery] string? recordTypeCode = null,
        [FromQuery] string? publicAccessCode = null,
        [FromQuery] string? integrationStatusCode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => Ok(_usOperatorSourceService.Search(
            new 미국농어업경영체정보원천조회요청
            {
                SearchText = q,
                SectorCode = sectorCode,
                RecordTypeCode = recordTypeCode,
                PublicAccessCode = publicAccessCode,
                IntegrationStatusCode = integrationStatusCode,
                Page = page,
                PageSize = pageSize
            }));
}
