using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Services.LogisticsProcessing.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.WarehouseFulfillment)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Warehouse)]
[ApiController]
[Authorize]
[Route("api/v1/warehouse-perspectives")]
public sealed class WarehousePerspectiveReadController(
    IWarehousePerspectiveReadService service) : ControllerBase
{
    [HttpGet("inbounds/expected/orderer")]
    public async Task<IActionResult> 주문자입고예정(
        [FromQuery] 입고요청목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedInboundsAsync(
            창고업무관점코드.주문자,
            null,
            request,
            cancellationToken));

    [HttpGet("inbounds/expected/seller")]
    public async Task<IActionResult> 판매자입고예정(
        [FromQuery] 입고요청목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedInboundsAsync(
            창고업무관점코드.판매자,
            null,
            request,
            cancellationToken));

    [HttpGet("inbounds/expected/transport")]
    public async Task<IActionResult> 운송담당자입고예정(
        [FromQuery] 입고요청목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedInboundsAsync(
            창고업무관점코드.운송담당자,
            null,
            request,
            cancellationToken));

    [HttpGet("inbounds/expected/community-ledgers/{ledgerId}")]
    public async Task<IActionResult> 공동원장입고예정(
        string ledgerId,
        [FromQuery] 입고요청목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedInboundsAsync(
            창고업무관점코드.공동원장,
            ledgerId,
            request,
            cancellationToken));

    [HttpGet("outbounds/expected/orderer")]
    public async Task<IActionResult> 주문자출고예정(
        [FromQuery] 출고예정목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedOutboundsAsync(
            창고업무관점코드.주문자,
            null,
            request,
            cancellationToken));

    [HttpGet("outbounds/expected/seller")]
    public async Task<IActionResult> 판매자출고예정(
        [FromQuery] 출고예정목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedOutboundsAsync(
            창고업무관점코드.판매자,
            null,
            request,
            cancellationToken));

    [HttpGet("outbounds/expected/warehouse")]
    public async Task<IActionResult> 창고관리자출고예정(
        [FromQuery] 출고예정목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedOutboundsAsync(
            창고업무관점코드.창고관리자,
            null,
            request,
            cancellationToken));

    [HttpGet("outbounds/expected/transport")]
    public async Task<IActionResult> 운송담당자출고예정(
        [FromQuery] 출고예정목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedOutboundsAsync(
            창고업무관점코드.운송담당자,
            null,
            request,
            cancellationToken));

    [HttpGet("outbounds/expected/community-ledgers/{ledgerId}")]
    public async Task<IActionResult> 공동원장출고예정(
        string ledgerId,
        [FromQuery] 출고예정목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await service.QueryExpectedOutboundsAsync(
            창고업무관점코드.공동원장,
            ledgerId,
            request,
            cancellationToken));
}
