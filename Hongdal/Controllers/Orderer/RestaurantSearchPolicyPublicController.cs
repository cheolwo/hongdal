using Hongdal.Contracts.Common.Orderer;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Orderer;

[ApiController]
[Route("api/v1/orderer/restaurant-search-policy")]
public sealed class RestaurantSearchPolicyPublicController : ControllerBase
{
    private readonly IRestaurantSearchPolicyStore _store;

    public RestaurantSearchPolicyPublicController(IRestaurantSearchPolicyStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<RestaurantSearchPolicyDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _store.GetAsync(cancellationToken));
    }
}
