using Ssalddel.Contracts.Driver.Development;
using Ssalddel.Services.Driver.Development;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Driver.Development00;

[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[ApiController]
[AllowAnonymous]
[Route("api/v1/driver/dev-snapshot")]
public sealed class 기사개발스냅샷Controller : ControllerBase
{
    private readonly I기사개발스냅샷Provider _snapshotProvider;

    public 기사개발스냅샷Controller(I기사개발스냅샷Provider snapshotProvider)
    {
        _snapshotProvider = snapshotProvider;
    }

    [HttpGet]
    public ActionResult<기사개발스냅샷응답> 조회()
    {
        return Ok(_snapshotProvider.GetSnapshot());
    }
}
