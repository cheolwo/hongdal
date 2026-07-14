using Hongdal.ApiMetadata;
using Hongdal.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Content07;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Route("api/v1/admin/content/hongik-hakdang/cards")]
[Authorize(Policy = "서버관리자전용")]
public sealed class HongikHakdangCardController : ControllerBase
{
    private readonly IHongikHakdangCardService _service;
    private readonly IHongikHakdangCardVariantService _variantService;

    public HongikHakdangCardController(
        IHongikHakdangCardService service,
        IHongikHakdangCardVariantService variantService)
    {
        _service = service;
        _variantService = variantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCollections(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
        => Ok(await _service.GetCollectionsAsync(includeInactive, cancellationToken));

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
        => Ok(await _service.SyncAsync(cancellationToken));

    [HttpPost("variants/prepare")]
    public async Task<IActionResult> PrepareVariants(CancellationToken cancellationToken)
        => Ok(await _variantService.EnsureActiveVariantsAsync(cancellationToken));
}
