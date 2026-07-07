using Hongdal.Contracts.Driver.Development;
using Hongdal.Services.Driver.Development;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Driver.Development00;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
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
