using Hongdal.Contracts.Common.PublicData;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.External.PublicData;

namespace Hongdal.Controllers.Orderer;

[ApiController]
[Route("api/v1/orderer/public-data")]
public sealed class PublicDataLookupController : ControllerBase
{
    private readonly IRoadAddressLookupService _roadAddressLookupService;
    private readonly IApartmentComplexLookupService _apartmentComplexLookupService;

    public PublicDataLookupController(
        IRoadAddressLookupService roadAddressLookupService,
        IApartmentComplexLookupService apartmentComplexLookupService)
    {
        _roadAddressLookupService = roadAddressLookupService;
        _apartmentComplexLookupService = apartmentComplexLookupService;
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<PublicDataLookupResponse<RoadAddressItem>>> SearchAddresses(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _roadAddressLookupService.SearchAsync(new RoadAddressSearchRequest
        {
            Keyword = keyword,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    [HttpGet("apartment-complexes")]
    public async Task<ActionResult<PublicDataLookupResponse<ApartmentComplexItem>>> SearchApartmentComplexes(
        [FromQuery] string? sidoCode,
        [FromQuery] string? sigunguCode,
        [FromQuery] string? eupmyeondongCode,
        [FromQuery] string? roadName,
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _apartmentComplexLookupService.SearchAsync(new ApartmentComplexSearchRequest
        {
            SidoCode = sidoCode,
            SigunguCode = sigunguCode,
            EupmyeondongCode = eupmyeondongCode,
            RoadName = roadName,
            Keyword = keyword,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    [HttpGet("apartment-complexes/{complexCode}/basic")]
    public async Task<ActionResult<PublicDataLookupResponse<ApartmentComplexBasicItem>>> GetApartmentComplexBasicInfo(
        string complexCode,
        CancellationToken cancellationToken)
    {
        var result = await _apartmentComplexLookupService.GetBasicInfoAsync(new ApartmentComplexBasicRequest
        {
            ComplexCode = complexCode
        }, cancellationToken);

        return Ok(result);
    }
}
