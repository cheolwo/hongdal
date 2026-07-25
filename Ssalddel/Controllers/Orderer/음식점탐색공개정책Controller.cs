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
[SsalddelApiContractName("RestaurantSearchPolicyPublicController")]
[Route("api/v1/orderer/restaurant-search-policy")]
public sealed class 음식점탐색공개정책Controller : OrdererControllerBase
{
    private readonly IRestaurantSearchPolicyStore _정책Store;

    public 음식점탐색공개정책Controller(IRestaurantSearchPolicyStore 정책Store)
    {
        _정책Store = 정책Store;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<RestaurantSearchPolicyDto>> 조회(CancellationToken cancellationToken)
    {
        return Ok(await _정책Store.GetAsync(cancellationToken));
    }
}
