using Hongdal.Contracts.Common.Privacy;
using Hongdal.Services.Security;
using Microsoft.AspNetCore.Mvc;
using 홍달.Infrastructure.Security;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Security;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Route("api/v1/security/isms-p/transport")]
public sealed class IsmsPTransportProtectionController : ControllerBase
{
    private readonly IIsmsPClientTransportProtectionService protectionService;
    private readonly IIsmsPTransportKeyStatusStore keyStatusStore;

    public IsmsPTransportProtectionController(
        IIsmsPClientTransportProtectionService protectionService,
        IIsmsPTransportKeyStatusStore keyStatusStore)
    {
        this.protectionService = protectionService;
        this.keyStatusStore = keyStatusStore;
    }

    [HttpGet("public-key")]
    public async Task<ActionResult<IsmsPClientEncryptionPublicKeyResponse>> GetPublicKey(
        CancellationToken cancellationToken)
    {
        var publicKey = protectionService.GetPublicKey();
        await keyStatusStore.MarkActiveAsync(publicKey, cancellationToken);
        return Ok(publicKey);
    }
}
