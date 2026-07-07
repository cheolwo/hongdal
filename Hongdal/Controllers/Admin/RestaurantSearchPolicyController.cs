using System.Security.Claims;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Admin;

[HongdalApiVersion(HongdalProductVersion.V3_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/restaurant-search-policy")]
public sealed class RestaurantSearchPolicyController : ControllerBase
{
    private readonly IRestaurantSearchPolicyStore _store;

    public RestaurantSearchPolicyController(IRestaurantSearchPolicyStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<RestaurantSearchPolicyDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _store.GetAsync(cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] RestaurantSearchPolicyUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _store.UpdateAsync(request, ResolveUserName(), cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return this.ToProblemActionResult(ex.Message);
        }
    }

    [HttpPost("reset")]
    public async Task<ActionResult<RestaurantSearchPolicyDto>> Reset(CancellationToken cancellationToken)
    {
        return Ok(await _store.ResetAsync(ResolveUserName(), cancellationToken));
    }

    private string? ResolveUserName()
    {
        return User.Identity?.Name
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue(ClaimTypes.Email);
    }
}
