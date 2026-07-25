using Ssalddel.Application.Versioning;
using Ssalddel.Contracts.Common.Versioning;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Platform;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[ApiController]
[Route("api/v1/version-feature-flags")]
public sealed class VersionFeatureFlagsController : ControllerBase
{
    private readonly I버전워크플로우UseCase _useCase;

    public VersionFeatureFlagsController(I버전워크플로우UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public ActionResult<VersionFeatureFlagsResponse> Get()
    {
        return Ok(_useCase.조회());
    }
}
