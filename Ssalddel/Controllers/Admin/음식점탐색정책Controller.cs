using System.Security.Claims;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin;

[SsalddelApiVersion(SsalddelProductVersion.V3_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/restaurant-search-policy")]
[SsalddelApiContractName("RestaurantSearchPolicyController")]
public sealed class 음식점탐색정책Controller : ControllerBase
{
    private readonly IRestaurantSearchPolicyStore _음식점탐색정책Store;

    public 음식점탐색정책Controller(IRestaurantSearchPolicyStore 음식점탐색정책Store)
    {
        _음식점탐색정책Store = 음식점탐색정책Store;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<RestaurantSearchPolicyDto>> 조회(CancellationToken cancellationToken)
    {
        return Ok(await _음식점탐색정책Store.GetAsync(cancellationToken));
    }

    [HttpPut]
    [SsalddelApiContractName("Update")]
    public async Task<IActionResult> 수정(
        [FromBody] RestaurantSearchPolicyUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _음식점탐색정책Store.UpdateAsync(request, ResolveUserName(), cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }

    [HttpPost("reset")]
    [SsalddelApiContractName("Reset")]
    public async Task<ActionResult<RestaurantSearchPolicyDto>> 초기화(CancellationToken cancellationToken)
    {
        return Ok(await _음식점탐색정책Store.ResetAsync(ResolveUserName(), cancellationToken));
    }

    private string? ResolveUserName()
    {
        return User.Identity?.Name
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue(ClaimTypes.Email);
    }
}
