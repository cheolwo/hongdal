using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[SsalddelApiVersion(
    SsalddelProductVersion.V3_0,
    FeatureKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[RequireVersionFeature(VersionFeatureFlagKeys.FoodDeliveryWorkflow)]
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
