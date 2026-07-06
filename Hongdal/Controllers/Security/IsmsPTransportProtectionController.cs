using Hongdal.Contracts.Common.Privacy;
using Microsoft.AspNetCore.Mvc;
using 홍달.Infrastructure.Security;

namespace Hongdal.Controllers.Security;

[ApiController]
[Route("api/v1/security/isms-p/transport")]
public sealed class IsmsPTransportProtectionController : ControllerBase
{
    private readonly IIsmsPClientTransportProtectionService protectionService;

    public IsmsPTransportProtectionController(IIsmsPClientTransportProtectionService protectionService)
    {
        this.protectionService = protectionService;
    }

    [HttpGet("public-key")]
    public ActionResult<IsmsPClientEncryptionPublicKeyResponse> GetPublicKey()
        => Ok(protectionService.GetPublicKey());
}
