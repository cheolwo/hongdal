using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Orderer;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[AllowAnonymous]
[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseDemand)]
[Route("api/v1/orderer/group-purchase-products")]
public sealed class 공동구매상품CatalogController(
    I공동구매상품CatalogUseCase useCase) : OrdererControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HS먹거리공동구매상품카드>), StatusCodes.Status200OK)]
    public async Task<IActionResult> 목록(CancellationToken cancellationToken)
        => Ok(await useCase.목록조회Async(cancellationToken));
}
