using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.VehicleLoading;
using Ssalddel.Services.LogisticsProcessing.VehicleLoading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.WarehouseFulfillment)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Warehouse)]
[SsalddelApiAudience(SsalddelActor.WarehouseManager)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[ApiController]
[Authorize]
[Route("api/v1/unloading-perspectives")]
[SsalddelApiContractName("UnloadingPerspectiveReadController")]
public sealed class 하차업무관점조회Controller(
    IUnloadingPerspectiveReadService 하차업무관점조회Service) : ControllerBase
{
    [HttpGet("orderer")]
    public Task<IActionResult> 주문자하차목록([FromQuery] 하차관점목록조회요청 request, CancellationToken cancellationToken)
        => 목록조회(하차업무관점코드.주문자, null, request, cancellationToken);

    [HttpGet("seller")]
    public Task<IActionResult> 판매자하차목록([FromQuery] 하차관점목록조회요청 request, CancellationToken cancellationToken)
        => 목록조회(하차업무관점코드.판매자, null, request, cancellationToken);

    [HttpGet("warehouse")]
    public Task<IActionResult> 창고관리자하차목록([FromQuery] 하차관점목록조회요청 request, CancellationToken cancellationToken)
        => 목록조회(하차업무관점코드.창고관리자, null, request, cancellationToken);

    [HttpGet("transport")]
    public Task<IActionResult> 운송담당자하차목록([FromQuery] 하차관점목록조회요청 request, CancellationToken cancellationToken)
        => 목록조회(하차업무관점코드.운송담당자, null, request, cancellationToken);

    [HttpGet("community-ledgers/{communityLedgerId}")]
    public Task<IActionResult> 공동원장하차목록(
        string communityLedgerId,
        [FromQuery] 하차관점목록조회요청 request,
        CancellationToken cancellationToken)
        => 목록조회(하차업무관점코드.공동원장, communityLedgerId, request, cancellationToken);

    private async Task<IActionResult> 목록조회(
        string 업무관점코드,
        string? 공동원장Id,
        하차관점목록조회요청 요청,
        CancellationToken cancellationToken)
        => this.ToActionResult(await 하차업무관점조회Service.QueryAsync(
            업무관점코드,
            공동원장Id,
            요청,
            cancellationToken));
}
