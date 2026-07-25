using Ssalddel.Contracts.Common.PublicData;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.External.PublicData;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Platform;

[SsalddelApiVersion(SsalddelProductVersion.V2_5)]
[ApiController]
[Route("api/v1/public-data/apis")]
[SsalddelApiContractName("PublicDataApiMetadataController")]
public sealed class 공공데이터ApiMetadataController : ControllerBase
{
    private readonly IPublicDataApiMetadataCatalog _공공데이터ApiMetadataCatalog;

    public 공공데이터ApiMetadataController(IPublicDataApiMetadataCatalog 공공데이터ApiMetadataCatalog)
    {
        _공공데이터ApiMetadataCatalog = 공공데이터ApiMetadataCatalog;
    }

    [HttpGet]
    public ActionResult<PublicDataApiMetadataResponse> Get(
        [FromQuery] string? domain,
        [FromQuery] string? versionScope,
        [FromQuery] bool? containsResidentialData)
    {
        var response = _공공데이터ApiMetadataCatalog.GetCatalog(new PublicDataApiMetadataQuery
        {
            Domain = domain,
            VersionScope = versionScope,
            ContainsResidentialData = containsResidentialData
        });

        return Ok(response);
    }
}
