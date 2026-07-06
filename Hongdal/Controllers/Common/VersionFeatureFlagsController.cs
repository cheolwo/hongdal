using Hongdal.Contracts.Common.Versioning;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Common;

[ApiController]
[Route("api/v1/version-feature-flags")]
public sealed class VersionFeatureFlagsController : ControllerBase
{
    private readonly IVersionFeatureFlagService _featureFlagService;

    public VersionFeatureFlagsController(IVersionFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    [HttpGet]
    public ActionResult<VersionFeatureFlagsResponse> Get()
    {
        return Ok(new VersionFeatureFlagsResponse
        {
            Flags = _featureFlagService.GetAll()
        });
    }
}
