using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Controllers.Common;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Api,
    "국내 공영도매시장 경락·정산가격 원천과 비식별 가격정보 공개 조회",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "가격·물량·품질 조건만 공개하고 출하자·생산자·중도매인 식별정보와 거래 실행은 제공하지 않습니다.")]
[ApiController]
[AllowAnonymous]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[Route("api/v1/agricultural-fisheries/domestic-auction-prices")]
[SsalddelApiContractName("DomesticAgriculturalAuctionPriceController")]
public sealed class 국내농산물경락가격Controller : ControllerBase
{
    private readonly I국내농산물경락가격조회Service _service;
    private readonly I국내농산물경락가격ArchiveService _archiveService;

    public 국내농산물경락가격Controller(
        I국내농산물경락가격조회Service service,
        I국내농산물경락가격ArchiveService archiveService)
    {
        _service = service;
        _archiveService = archiveService;
    }

    [HttpGet("sources")]
    [SsalddelApiContractName("GetDomesticAgriculturalAuctionPriceSources")]
    public ActionResult<IReadOnlyList<국내농산물경락가격원천응답>> 원천목록조회()
        => Ok(_service.GetSources());

    [HttpGet]
    [SsalddelApiContractName("GetDomesticAgriculturalAuctionPrices")]
    public async Task<ActionResult<국내농산물경락가격조회응답>> 가격조회(
        [FromQuery] string settlementDate,
        [FromQuery] string sourceKey =
            국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement,
        [FromQuery] string? wholesaleMarketCode = null,
        [FromQuery] string? corporationCode = null,
        [FromQuery] string? itemName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var response = await _service.조회Async(
            new 국내농산물경락가격조회요청
            {
                SourceKey = sourceKey,
                SettlementDate = settlementDate,
                WholesaleMarketCode = wholesaleMarketCode,
                CorporationCode = corporationCode,
                ItemName = itemName,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return response.StatusCode switch
        {
            국내농산물경락가격조회상태Codes.잘못된요청 => BadRequest(response),
            국내농산물경락가격조회상태Codes.지원하지않는출처 => BadRequest(response),
            국내농산물경락가격조회상태Codes.설정안됨 =>
                StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            국내농산물경락가격조회상태Codes.자료조회불가 =>
                StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Ok(response)
        };
    }

    [HttpGet("archive")]
    [SsalddelApiContractName("GetArchivedDomesticAgriculturalAuctionPrices")]
    public async Task<ActionResult<국내농산물경락가격조회응답>> 누적가격조회(
        [FromQuery] string settlementDate,
        [FromQuery] string sourceKey =
            국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement,
        [FromQuery] string? wholesaleMarketCode = null,
        [FromQuery] string? corporationCode = null,
        [FromQuery] string? itemName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var response = await _archiveService.SearchAsync(
            new 국내농산물경락가격조회요청
            {
                SourceKey = sourceKey,
                SettlementDate = settlementDate,
                WholesaleMarketCode = wholesaleMarketCode,
                CorporationCode = corporationCode,
                ItemName = itemName,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);
        return response.StatusCode switch
        {
            국내농산물경락가격조회상태Codes.잘못된요청 => BadRequest(response),
            국내농산물경락가격조회상태Codes.지원하지않는출처 => BadRequest(response),
            _ => Ok(response)
        };
    }
}
