using Ssalddel.Contracts.Common.PublicData;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.External.PublicData;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V2_5)]
[ApiController]
[Route("api/v1/public-data/apis")]
public sealed class PublicDataApiMetadataController : ControllerBase
{
    private readonly IPublicDataApiMetadataCatalog _catalog;

    public PublicDataApiMetadataController(IPublicDataApiMetadataCatalog catalog)
    {
        _catalog = catalog;
    }

    [HttpGet]
    public ActionResult<PublicDataApiMetadataResponse> Get(
        [FromQuery] string? domain,
        [FromQuery] string? versionScope,
        [FromQuery] bool? containsResidentialData)
    {
        var response = _catalog.GetCatalog(new PublicDataApiMetadataQuery
        {
            Domain = domain,
            VersionScope = versionScope,
            ContainsResidentialData = containsResidentialData
        });

        return Ok(response);
    }
}
