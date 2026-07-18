using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Content;
using Hongdal.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Content07;

[HongdalApiVersion(HongdalProductVersion.V0_0)]
[ApiController]
[Route("api/v1/admin/content/information")]
[Authorize(Policy = "서버관리자전용")]
public sealed class CommunityInformationCollectionController : ControllerBase
{
    private readonly ICommunityInformationCollectionService _service;

    public CommunityInformationCollectionController(
        ICommunityInformationCollectionService service)
    {
        _service = service;
    }

    [HttpGet("sources")]
    public ActionResult<IReadOnlyList<CommunityInformationSourceDto>> GetSources()
        => Ok(_service.GetSources());

    [HttpGet("candidates")]
    public async Task<ActionResult<CommunityInformationCollectionResponse>> GetCandidates(
        [FromQuery] CommunityInformationCollectionQuery query,
        CancellationToken cancellationToken)
        => Ok(await _service.ReadAsync(query, cancellationToken));
}
