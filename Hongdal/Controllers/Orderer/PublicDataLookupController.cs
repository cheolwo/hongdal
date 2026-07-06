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
    private readonly IOrdererGroupScopeLookupService _ordererGroupScopeLookupService;

    public PublicDataLookupController(
        IRoadAddressLookupService roadAddressLookupService,
        IApartmentComplexLookupService apartmentComplexLookupService,
        IOrdererGroupScopeLookupService ordererGroupScopeLookupService)
    {
        _roadAddressLookupService = roadAddressLookupService;
        _apartmentComplexLookupService = apartmentComplexLookupService;
        _ordererGroupScopeLookupService = ordererGroupScopeLookupService;
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

    [HttpGet("orderer-group-scopes")]
    public ActionResult<PublicDataLookupResponse<OrdererGroupScopeCandidateItem>> FindOrdererGroupScopes(
        [FromQuery] string? roadAddress,
        [FromQuery] string? jibunAddress,
        [FromQuery] string? kakaoRegionLevel1,
        [FromQuery] string? kakaoRegionLevel2,
        [FromQuery] string? kakaoRegionLevel3,
        [FromQuery] int pageSize = 5)
    {
        var result = _ordererGroupScopeLookupService.FindCandidates(new OrdererGroupScopeLookupRequest
        {
            RoadAddress = roadAddress,
            JibunAddress = jibunAddress,
            KakaoRegionLevel1 = kakaoRegionLevel1,
            KakaoRegionLevel2 = kakaoRegionLevel2,
            KakaoRegionLevel3 = kakaoRegionLevel3,
            PageSize = pageSize
        });

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
