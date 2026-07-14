using System.Security.Claims;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Content;
using Hongdal.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Route("api/v1/content/hongik-hakdang/cards")]
[Authorize]
public sealed class HongikHakdangCardMobileController : ControllerBase
{
    private readonly IHongikHakdangCardDeliveryService _deliveryService;
    private readonly IHongikHakdangCardMediaTokenService _mediaTokenService;
    private readonly IHongikHakdangCardImageStore _imageStore;
    private readonly HongdalContext _db;

    public HongikHakdangCardMobileController(
        IHongikHakdangCardDeliveryService deliveryService,
        IHongikHakdangCardMediaTokenService mediaTokenService,
        IHongikHakdangCardImageStore imageStore,
        HongdalContext db)
    {
        _deliveryService = deliveryService;
        _mediaTokenService = mediaTokenService;
        _imageStore = imageStore;
        _db = db;
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog(
        [FromQuery] string? collectionKey,
        CancellationToken cancellationToken)
        => Ok(await _deliveryService.GetCatalogAsync(collectionKey, cancellationToken));

    [HttpGet("today")]
    public async Task<IActionResult> GetToday(
        [FromQuery] string? timeZoneId,
        CancellationToken cancellationToken)
        => Ok(await _deliveryService.GetTodayAsync(timeZoneId, cancellationToken));

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreference(CancellationToken cancellationToken)
        => Ok(await _deliveryService.GetPreferenceAsync(CurrentUserId(), cancellationToken));

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreference(
        [FromBody] HongikHakdangCardDeliveryPreferenceUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _deliveryService.UpdatePreferenceAsync(
                CurrentUserId(),
                request,
                cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (TimeZoneNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet("media/{token}.jpg")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetSignedMedia(
        string token,
        CancellationToken cancellationToken)
    {
        if (!_mediaTokenService.TryValidate(token, out var grant) || grant is null)
        {
            return NotFound();
        }

        var variant = await _db.HongikHakdangCardImageVariants
            .AsNoTracking()
            .Where(x => x.Id == grant.VariantId && x.Card.IsActive)
            .Select(x => new { x.LocalImagePath, x.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        if (variant is null || !_imageStore.Exists(variant.LocalImagePath))
        {
            return NotFound();
        }

        var bytes = await _imageStore.ReadAsync(variant.LocalImagePath, cancellationToken);
        Response.Headers.CacheControl = "private, max-age=3600";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(bytes, variant.ContentType);
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
