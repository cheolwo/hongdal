using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.VehicleLoading;
using Ssalddel.Services.LogisticsProcessing.VehicleLoading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.WarehouseFulfillment)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Warehouse)]
[ApiController]
[Authorize]
[Route("api/v1/unloading-perspectives")]
public sealed class UnloadingPerspectiveReadController(
    IUnloadingPerspectiveReadService service) : ControllerBase
{
    [HttpGet("orderer")]
    public Task<IActionResult> 주문자하차목록([FromQuery] 하차관점목록조회요청 request, CancellationToken cancellationToken)
        => Query(하차업무관점코드.주문자, null, request, cancellationToken);

    [HttpGet("seller")]
    public Task<IActionResult> 판매자하차목록([FromQuery] 하차관점목록조회요청 request, CancellationToken cancellationToken)
        => Query(하차업무관점코드.판매자, null, request, cancellationToken);

    [HttpGet("warehouse")]
    public Task<IActionResult> 창고관리자하차목록([FromQuery] 하차관점목록조회요청 request, CancellationToken cancellationToken)
        => Query(하차업무관점코드.창고관리자, null, request, cancellationToken);

    [HttpGet("transport")]
    public Task<IActionResult> 운송담당자하차목록([FromQuery] 하차관점목록조회요청 request, CancellationToken cancellationToken)
        => Query(하차업무관점코드.운송담당자, null, request, cancellationToken);

    [HttpGet("community-ledgers/{communityLedgerId}")]
    public Task<IActionResult> 공동원장하차목록(
        string communityLedgerId,
        [FromQuery] 하차관점목록조회요청 request,
        CancellationToken cancellationToken)
        => Query(하차업무관점코드.공동원장, communityLedgerId, request, cancellationToken);

    private async Task<IActionResult> Query(
        string perspectiveCode,
        string? communityLedgerId,
        하차관점목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryAsync(
            perspectiveCode,
            communityLedgerId,
            request,
            cancellationToken));
}
