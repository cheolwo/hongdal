using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.AgriculturalFisheries;
using Hongdal.Services.AgriculturalFisheries.Information;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[ApiController]
[AllowAnonymous]
[HongdalApiVersion(HongdalProductVersion.V0_0)]
[Route("api/v1/agricultural-fisheries")]
public sealed class AgriculturalFisheriesInformationController : ControllerBase
{
    private readonly IAgriculturalFisheriesInformationService _informationService;
    private readonly I미국농수산가격조회Service _usPriceService;

    public AgriculturalFisheriesInformationController(
        IAgriculturalFisheriesInformationService informationService,
        I미국농수산가격조회Service usPriceService)
    {
        _informationService = informationService;
        _usPriceService = usPriceService;
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
}
