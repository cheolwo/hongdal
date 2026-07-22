using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Application.Security;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Security;

[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[ApiController]
[Route("api/v1/security/isms-p/transport")]
public sealed class IsmsPTransportProtectionController : ControllerBase
{
    private readonly IISMSP전송보호UseCase _useCase;

    public IsmsPTransportProtectionController(IISMSP전송보호UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("public-key")]
    public async Task<ActionResult<IsmsPClientEncryptionPublicKeyResponse>> GetPublicKey(
        CancellationToken cancellationToken)
    {
        return Ok(await _useCase.공개키발급Async(cancellationToken));
    }
}
